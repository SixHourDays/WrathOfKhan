using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ShipMovedTransmission
{
    public int player_id;
    public Vector3 start_position;
    public Vector3 end_position;
}

[Serializable]
public class ShipMovedTransmissionEvent : UnityEvent<ShipMovedTransmission> { }