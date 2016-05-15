using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class RaiseCloakTransmission
{
    public int player_id;
    public bool cloak;
    //implies them going up
}

[Serializable]
public class RaiseCloakTransmissionEvent : UnityEvent<RaiseCloakTransmission> { }