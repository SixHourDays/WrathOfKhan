using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

// Going to go super simple, only 2 connections allowed. (see if I have time to upgrade to multiple players later).
// Concept is both ends act the same, Host simply listens for 1 connection, then the host has the "first" turn.
// The Client listens for the host to complete what he's doing, then they switch. The host then listens for the client
// to finish his turn. We bounce back and forth like this for the whole game.

// scratch that, we're going N players allowed. Going to do a ring buffer of people. First everyone connects to the host,
// then the host tells the clients how to fix themselves up to be a ring buffer (and telling them their playerID)

public class NetworkController : MonoBehaviour
{
    [Serializable]
    public class PlayerInfo
    {
        public int playerID;
    }
    
    Socket m_next = null;
    Socket m_prev = null;


    // Shared information between host and clients.
    int m_localPlayerID;
    List<PlayerInfo> m_players = new List<PlayerInfo>();
    

    // event handling and network processing
    bool m_shouldHandleEvents = true;

    const int m_recv_buffer_size = 1024 * 1024;
    static byte[] s_recv_buffer = new byte[m_recv_buffer_size];

    List<NetworkEventHandler> m_eventHandlers = new List<NetworkEventHandler>();


    public PlayerInfo GetLocalPlayerInfo()
    {
        return GetPlayerInfo(m_localPlayerID);
    }

    public PlayerInfo GetPlayerInfo(int playerID)
    {
        for (int i = 0; i < m_players.Count; ++i)
        {
            if (m_players[i].playerID == playerID)
            {
                return m_players[i];
            }
        }

        return null;
    }

    public List<PlayerInfo> GetRemotePlayerInfos()
    {
        return new List<PlayerInfo>(m_players);
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

        // Always "hear" about things from the prev.
        if (m_prev == null)
        {
            return;
        }

        if (m_prev.Available > 0)
        {
            // we have data, go grab it.

            // if this was sent FROM our playerID, consume it.
            // otherwise read it, send it to next, and dispatch.

            TransmissionInfo info = RetrieveTransmission();

            if (info.transmission_from_id != m_localPlayerID)
            {
                bool shouldPropegate = DispatchEvent(info);

                if (shouldPropegate)
                {
                    SendTransmissionInfo(info);
                }
            }
        }
    }


    private bool DispatchEvent(TransmissionInfo info)
    {
        bool shouldPropegate = true;

        // I WOULD template this... but C# said fuck you to template functions.

        if (info.transmission_name == typeof(FireBullet).Name)
        {
            FireBullet temp = JsonUtility.FromJson<FireBullet>(info.transmission_payload);
            
            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }
        }
        else if (info.transmission_name == typeof(ConnectTransmission).Name)
        {
            ConnectTransmission temp = JsonUtility.FromJson<ConnectTransmission>(info.transmission_payload);

            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }

            shouldPropegate = false;
        }
        else if (info.transmission_name == typeof(EndTurnTransmission).Name)
        {
            EndTurnTransmission temp = JsonUtility.FromJson<EndTurnTransmission>(info.transmission_payload);

            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }

            shouldPropegate = false;
        }
        else if (info.transmission_name == typeof(ShipMovedTransmission).Name)
        {
            ShipMovedTransmission temp = JsonUtility.FromJson<ShipMovedTransmission>(info.transmission_payload);

            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }
        }
        else if (info.transmission_name == typeof(DamageShipTransmission).Name)
        {
            DamageShipTransmission temp = JsonUtility.FromJson<DamageShipTransmission>(info.transmission_payload);

            for (int i = 0; i < m_eventHandlers.Count; ++i)
            {
                m_eventHandlers[i].OnNetworkEvent(temp);
            }
        }
        else
        {
            Debug.LogError("Unhandled transmission type");
        }

        return shouldPropegate;
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

        // 1 because we are not the host. Host will tell us later what ID we are.
        m_localPlayerID = 1;

        if (m_next != null)
        {
            if (m_next.Connected)
            {
                Debug.Log("Client already connected");
            }
            else
            {
                m_next.Shutdown(SocketShutdown.Both);
                m_next.Close();
                //m_client.Dispose();
                m_next = null;
            }
        }

        try 
        {
            m_next = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_next.Connect(new IPEndPoint(address, 55555));
            m_prev = m_next; // prev and next are host until it tells us otherwise.

            // now send some data about what we selected

            LoginToHostTransmission loginTransmission = new LoginToHostTransmission();

            SendTransmission(loginTransmission);
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception trying to connect. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");

            return false;
        }

        // now we need to listen for transmissions from the host to tell us our playerID and how to fix up the ring buffer.

        return true;
    }

    // returns if it was successful in starting to listen. Can deal with the UI in a fancy way if we want (instead of crashing the app on random exceptions)
    public bool ListenForConnections(int numberOfConnections)
    {
        List<Socket> sockets_connected = new List<Socket>();

        // this means we're the host. Make a Server socket and accept a bunch of connections.
        Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            // This is where we would put OUR information.
            PlayerInfo temp_info = new PlayerInfo();
            temp_info.playerID = 0;
            m_players.Add(temp_info);

            server_socket.Bind(new IPEndPoint(IPAddress.Any, 55555)); // 55555 because... Always 55555
            server_socket.Listen(10);

            for (int i = 0; i < numberOfConnections; ++i)
            {
                sockets_connected.Add(server_socket.Accept());
            }
            
            server_socket.Close();
            server_socket = null;

            // now that we have all of our connections, form the ring buffer.
        }
        catch (SocketException ex)
        {
            // failed to bind and listen... 
            Debug.Log("Socket Exception. Message = (" + ex.Message + ")");
            Debug.Log("Stack trace = (" + ex.StackTrace + ")");

            //server_socket.Shutdown(SocketShutdown.Both);
            server_socket.Close();
            //server_socket.Dispose();

            return false;
        }
        
        // accept all login transmissions

        for (int i = 0; i < sockets_connected.Count; ++i)
        {
            TransmissionInfo temp_trans = RetrieveTransmission(sockets_connected[i]);

            LoginToHostTransmission login_transmission = JsonUtility.FromJson<LoginToHostTransmission>(temp_trans.transmission_payload);

            // we now have the login information from this person.
            // setup their things inside our PlayerInfo list.

            PlayerInfo info = new PlayerInfo();
            info.playerID = i + 1;
            m_players.Add(info);
        }

        int numPlayers = m_players.Count;

        if (numPlayers == 2)
        {
            // just me and you.
            // hook up our prev and next to be the same guy.

            m_prev = m_next = sockets_connected[0];

            ConnectTransmission trans = new ConnectTransmission();
            trans.numPlayers = numPlayers;
            trans.playerID = 1;
            trans.nextIPAddress = "";
            trans.player_information = m_players.ToArray();

            SendTransmission(trans);
        }
        else
        {
            for (int i = 0; i < sockets_connected.Count; ++i)
            {
                ConnectTransmission trans = new ConnectTransmission();
                trans.numPlayers = numPlayers;
                trans.playerID = i + 1;
                trans.player_information = m_players.ToArray();

                if (i == 0)
                {
                    trans.nextIPAddress = "";
                }
                else
                {
                    IPEndPoint remoteIpEndPoint = sockets_connected[i - 1].RemoteEndPoint as IPEndPoint;
                    trans.nextIPAddress = remoteIpEndPoint.Address.ToString();
                    Debug.Log("Sending IP Address of " + trans.nextIPAddress);
                }

                SendTransmission(trans, sockets_connected[i]);
            }

            m_prev = sockets_connected[0];
            m_next = sockets_connected[sockets_connected.Count - 1];
        }

        return true;
    }

    public bool SendTransmission(object transmissionObject, Socket socket = null)
    {
        TransmissionInfo infoObject = new TransmissionInfo();

        infoObject.transmission_name = transmissionObject.GetType().Name;
        infoObject.transmission_from_id = m_localPlayerID;
        infoObject.transmission_payload = JsonUtility.ToJson(transmissionObject);

        return SendTransmissionInfo(infoObject, socket);
    }

    private bool SendTransmissionInfo(TransmissionInfo info, Socket socket = null)
    {
        string final_payload = JsonUtility.ToJson(info);

        Socket sock = socket;

        if (sock == null)
        {
            sock = m_next;
        }

        return SendFullMessage(sock, System.Text.Encoding.ASCII.GetBytes(final_payload));
    }

    
    public TransmissionInfo RetrieveTransmission(Socket socket = null)
    {
        Socket sock = m_prev;

        if (socket != null)
        {
            sock = socket;
        }

        byte[] data = null;
        ReceiveFullMessage(sock, out data);

        TransmissionInfo info = JsonUtility.FromJson<TransmissionInfo>(System.Text.Encoding.ASCII.GetString(data));

        return info;
    }
    

    public void OnConnectTransmission(ConnectTransmission connectTransmission)
    {
        m_localPlayerID = connectTransmission.playerID;

        if (connectTransmission.numPlayers > 2)
        {
            // need to patch up by listening and connecting.

            if (connectTransmission.nextIPAddress != String.Empty)
            {
                // connect to that IP, and set next == to it.

                Debug.Log("Received next IP Address of " + connectTransmission.nextIPAddress);

                if (m_next != null)
                {
                    // if we're the last guy... don't close next as they're the same.
                    if (connectTransmission.numPlayers != m_localPlayerID + 1)
                    {
                        m_next.Shutdown(SocketShutdown.Both);
                        m_next.Close();
                        m_next = null;
                    }
                }

                IPAddress address = IPAddress.Parse(connectTransmission.nextIPAddress);

                m_next = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_next.Connect(new IPEndPoint(address, 55555));

                Debug.Log("Connected to next IP of " + connectTransmission.nextIPAddress);
            }

            // if we're the last one to connect, our prev is fine.
            // just need to set next and don't bother listening
            if (connectTransmission.numPlayers != m_localPlayerID + 1)
            {
                // listen for prev.
                Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                server_socket.Bind(new IPEndPoint(IPAddress.Any, 55555)); // 55555 because... Always 55555
                server_socket.Listen(10);

                m_prev = server_socket.Accept();

                server_socket.Close();
                server_socket = null;
            }
        }
        
            // this should include us as well.
        for (int i = 0; i < connectTransmission.player_information.Length; ++i)
        {
            m_players.Add(connectTransmission.player_information[i]);
        }
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
            Debug.Log("Socket is null");
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
