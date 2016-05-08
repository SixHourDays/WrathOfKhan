using System;
using UnityEngine.Events;

[Serializable]
public class LoginToHostTransmission
{
    // add here what you want to send to "everyone" as your selected ship, name, whatever.
}

[Serializable]
public class LoginToHostTransmissionEvent : UnityEvent<LoginToHostTransmission> { }