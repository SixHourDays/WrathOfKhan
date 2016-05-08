using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ShipMovedTransmission
{
    public int player_id;
    public Vector3 end_position;
    public Vector3 translation;
}

[Serializable]
public class ShipMovedTransmissionEvent : UnityEvent<ShipMovedTransmission> { }