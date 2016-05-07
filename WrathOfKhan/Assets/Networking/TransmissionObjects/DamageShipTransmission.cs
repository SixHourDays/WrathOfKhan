using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DamageShipTransmission
{
    public int player_id;

}

[Serializable]
public class DamageShipTransmissionEvent : UnityEvent<DamageShipTransmission> { }