using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

// Going to go super simple, only 2 connections allowed. (see if I have time to upgrade to multiple players later).
// Concept is both ends act the same, Host simply listens for 1 connection, then the host has the "first" turn.
// The Client listens for the host to complete what he's doing, then they switch. The host then listens for the client
// to finish his turn. We bounce back and forth like this for the whole game.

public class NetworkController : MonoBehaviour
{
    Socket m_client = null;

    const int m_recv_buffer_size = 1024 * 1024;
    static byte[] s_recv_buffer = new byte[m_recv_buffer_size];

    static bool balls = false;

    // Use this for initialization
    void Start ()
    {
        ListenForConnections();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!balls)
        {
            balls = true;
            ConnectToHost(IPAddress.Loopback);
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
            m_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_client.Connect(new IPEndPoint(address, 55555));
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            System.Console.WriteLine("Socket Exception trying to connect. Message = (" + ex.Message + ")");
            System.Console.WriteLine("Stack trace = (" + ex.StackTrace + ")");

            return false;
        }

        // connection successful.

        byte[] message = null;
        ReceiveFullMessage(m_client, ref message);

        Debug.Log("Received from client: " + System.Text.Encoding.ASCII.GetString(message));

        SendFullMessage(m_client, System.Text.Encoding.ASCII.GetBytes("Hello world"));

        return true;
    }

    // returns if it was successful in starting to listen. Can deal with the UI in a fancy way if we want (instead of crashing the app on random exceptions)
    public bool ListenForConnections()
    {
        // this means we're the host. Make a Server socket and accept a bunch of connections.

        Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server_socket.Bind(new IPEndPoint(IPAddress.Any, 55555)); // 55555 because... Always 55555
            server_socket.Listen(100);

                // BeginAccept because we want it to be async. Don't wanna lock up the main thread.
            server_socket.BeginAccept(OnAcceptClient, server_socket);
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            System.Console.WriteLine("Socket Exception. Message = (" + ex.Message + ")");
            System.Console.WriteLine("Stack trace = (" + ex.StackTrace + ")");

            server_socket.Shutdown(SocketShutdown.Both);
            server_socket.Close();
            //server_socket.Dispose();

            return false;
        }

        return true;
    }

    public void OnAcceptClient(System.IAsyncResult result)
    {
        // Client has connected. Time to start the game I guess.
        // Not sure if this is sent on a separate thread or not. Assuming not until proven otherwise (I'll debug later)

        Socket server_socket = result.AsyncState as Socket;

        Socket client = server_socket.EndAccept(result); // strange syntax >_>

        // only allowing one connection so shutdown the "server" and lets go.
        server_socket.Shutdown(SocketShutdown.Both);
        server_socket.Close();
        //server_socket.Dispose();


        // debug testing
        Debug.Log("Client connected.");
        
        SendFullMessage(client, System.Text.Encoding.ASCII.GetBytes("Hello world"));

        byte[] message = null;
        ReceiveFullMessage(client, ref message);

        Debug.Log("Received from client: " + System.Text.Encoding.ASCII.GetString(message));
    }

    // for now this will be blocking. Could change it to a system where it will accumulate a buffer and we can chew through each "full message" received.
    public static bool ReceiveFullMessage(Socket socket, ref byte[] out_message)
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
            //TODO: deal with the 0 return, meaning graceful shutdown
            bytes_received += socket.Receive(s_recv_buffer, data_length = bytes_received, SocketFlags.None);

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
            System.Console.WriteLine("Socket Exception in SendFullMessage. Message = (" + ex.Message + ")");
            System.Console.WriteLine("Stack trace = (" + ex.StackTrace + ")");
            
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
            System.Console.WriteLine("Socket Exception in SendFullMessage. Message = (" + ex.Message + ")");
            System.Console.WriteLine("Stack trace = (" + ex.StackTrace + ")");

            // possibly a disconnection. Not sure how C# deals with this.

            return false;
        }

        return true;
    }
}
