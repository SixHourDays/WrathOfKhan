using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

[Serializable]
public class ConnectTransmission
{
    // this is the assigned playerID. Host is always 0.
    public int playerID;

    // Total number of players including the host.
    // if this is 2, choose the simple case being next == prev.
    public int numPlayers;

    // this holds the IPAddress to connect to the "next"
    // if it's blank, it's already correct.
    // if it has something, reconnect your next (because next is currently incorrect)
    public string nextIPAddress;

    //public NetworkController.PlayerInfo[] player_information;
}

//TODO: Need to send PlayerInfo TO host on first connect. Host then needs to replicate all this to everyone connected.

[Serializable]
public class ConnectTransmissionEvent : UnityEvent<ConnectTransmission> { }