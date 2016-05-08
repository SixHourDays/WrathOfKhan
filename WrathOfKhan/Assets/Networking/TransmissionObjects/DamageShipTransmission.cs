using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DamageShipTransmission
{
    public int player_id; // player to damage

    public float damage_to_apply;
}

[Serializable]
public class DamageShipTransmissionEvent : UnityEvent<DamageShipTransmission> { }