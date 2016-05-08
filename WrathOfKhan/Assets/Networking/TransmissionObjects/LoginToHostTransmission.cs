using System;
using UnityEngine.Events;

[Serializable]
public class LoginToHostTransmission
{
    public int spriteSelection;
}

[Serializable]
public class LoginToHostTransmissionEvent : UnityEvent<LoginToHostTransmission> { }