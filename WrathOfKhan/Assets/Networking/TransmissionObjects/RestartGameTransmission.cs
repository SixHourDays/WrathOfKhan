using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RestartGameTransmission
{

}

[Serializable]
public class RestartGameTransmissionEvent : UnityEvent<RestartGameTransmission> { }