using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class RaiseShieldsTransmission
{
    public int player_id;

    public float shield_value;
}

[Serializable]
public class RaiseShieldsTransmissionEvent : UnityEvent<RaiseShieldsTransmission> { }