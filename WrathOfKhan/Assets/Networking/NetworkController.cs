using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

// Going to go super simple, only 2 connections allowed. (see if I have time to upgrade to multiple players later).
// Concept is both ends act the same, Host simply listens for 1 connection, then the host has the "first" turn.
// The Client listens for the host to complete what he's doing, then they switch. The host then listens for the client
// to finish his turn. We bounce back and forth like this for the whole game.

public class NetworkController : MonoBehaviour
{
    Socket m_client = null;
    Socket m_server_socket = null;

    bool m_shouldHandleEvents = true;

    const int m_recv_buffer_size = 1024 * 1024;
    static byte[] s_recv_buffer = new byte[m_recv_buffer_size];

    List<NetworkEventHandler> m_eventHandlers = new List<NetworkEventHandler>();

    // Use this for initialization
    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        HandleEvents();
	}

    private void HandleEvents()
    {
        if (!m_shouldHandleEvents)
        {
            return;
        }

        if (m_client == null)
        {
            return;
        }

        if (m_client.Available > 0)
        {
            // we have data, go grab it.

            TransmissionInfo info = RetrieveTransmission();
            DispatchEvent(info);
        }
    }


    private void DispatchEvent(TransmissionInfo info)
    {
        if (info.transmission_name == typeof(FireBullet).Name)
        {
            FireBullet temp = JsonUtility.FromJson<FireBullet>(info.transmission_payload);
            
            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }
        }
        else
        {
            Debug.LogError("Unhandled transmission type");
        }
    }

    public void PauseEventHandling()
    {
        m_shouldHandleEvents = false;
    }

    public void ResumeEventHandling()
    {
        m_shouldHandleEvents = true;
    }

    public void AddEventHandler(NetworkEventHandler handler)
    {
        m_eventHandlers.Add(handler);
    }

    public void RemoveEventHandler(NetworkEventHandler handler)
    {
        if (!m_eventHandlers.Remove(handler))
        {
            Debug.LogError("Could not find event handler to remove.");
        }
    }

    public bool ConnectToHost(IPAddress address)
    {
        // we are not the host. Connect to the address and await instructions from the host (turn order etc...)

        if (m_client != null)
        {
            if (m_client.Connected)
            {
                Debug.Log("Client already connected");
            }
            else
            {
                m_client.Shutdown(SocketShutdown.Both);
                m_client.Close();
                //m_client.Dispose();
                m_client = null;
            }
        }

        try 
        {
            m_client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_client.Connect(new IPEndPoint(address, 55555));
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception trying to connect. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");

            return false;
        }

        return true;
    }

    // returns if it was successful in starting to listen. Can deal with the UI in a fancy way if we want (instead of crashing the app on random exceptions)
    public bool ListenForConnections()
    {
        // this means we're the host. Make a Server socket and accept a bunch of connections.

        m_server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            m_server_socket.Bind(new IPEndPoint(IPAddress.Any, 55555)); // 55555 because... Always 55555
            m_server_socket.Listen(10);

            m_client = m_server_socket.Accept();
            
            m_server_socket.Close();
            m_server_socket = null;
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");

            m_server_socket.Shutdown(SocketShutdown.Both);
            m_server_socket.Close();
            //server_socket.Dispose();

            return false;
        }

        return true;
    }

    public bool SendTransmission(object transmissionObject)
    {
        TransmissionInfo infoObject = new TransmissionInfo();

        infoObject.transmission_name = transmissionObject.GetType().Name;
        infoObject.transmission_payload = JsonUtility.ToJson(transmissionObject);

        string final_payload = JsonUtility.ToJson(infoObject);

        return SendFullMessage(m_client, System.Text.Encoding.ASCII.GetBytes(final_payload));
    }

    
    public TransmissionInfo RetrieveTransmission()
    {
        byte[] data = null;
        ReceiveFullMessage(m_client, out data);

        TransmissionInfo info = JsonUtility.FromJson<TransmissionInfo>(System.Text.Encoding.ASCII.GetString(data));

        return info;
    }
    

    // for now this will be blocking. Could change it to a system where it will accumulate a buffer and we can chew through each "full message" received.
    public static bool ReceiveFullMessage(Socket socket, out byte[] out_message)
    {
        // first receive the size header.

        int header_size = sizeof(UInt32); // same as C++?
        int bytes_received = 0;

        List<byte> temp = new List<byte>();

        do
        {
                //TODO: deal with the 0 return, meaning graceful shutdown

            bytes_received += socket.Receive(s_recv_buffer, header_size - bytes_received, SocketFlags.None); // only accept up to header_size bytes.

            for (int i = 0; i < bytes_received; ++i)
            {
                temp.Add(s_recv_buffer[i]);
            }

        } while (bytes_received < header_size);

        // temp now holds the entire header. Time to convert it and accept the rest of the data.

        int data_length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(temp.ToArray(), 0));

        bytes_received = 0;
        temp.Clear();

        do
        {
            int amount_to_receive = Math.Min(m_recv_buffer_size, data_length - bytes_received);

            //TODO: deal with the 0 return, meaning graceful shutdown
            bytes_received += socket.Receive(s_recv_buffer, amount_to_receive, SocketFlags.None);

            for (int i = 0; i < bytes_received; ++i)
            {
                temp.Add(s_recv_buffer[i]);
            }

        } while (bytes_received < data_length);

        out_message = temp.ToArray();

        return true;
    }

    public static bool SendFullMessage(Socket socket, byte[] data)
    {
        // start by sending exactly 4 bytes for the message size.
        // Then send the data.

        if (socket == null)
        {
            return false;
        }

        // IPAddress HostToNetworkOrder will swap the endianness depending if we are a Little Endian system and make it Big Endian (for network transmissions)
        // will do nothing if we are a big endian system.
        byte[] header = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int32)data.Length));

        int bytes_sent = 0;

        try
        {
            do
            {
                bytes_sent += socket.Send(header);

                // offset
            } while (bytes_sent < header.Length);
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception in SendFullMessage. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");
            
            // possibly a disconnection. Not sure how C# deals with this.

            return false;
        }

        // now send the rest of the message.

        bytes_sent = 0;

        try
        {
            do
            {
                bytes_sent += socket.Send(data);

                // offset
            } while (bytes_sent < data.Length);
        }
        catch (System.Exception ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception in SendFullMessage. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");

            // possibly a disconnection. Not sure how C# deals with this.

            return false;
        }

        return true;
    }
}
