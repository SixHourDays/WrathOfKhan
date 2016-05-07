using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TransmissionInfo
{
    // name that corresponds to the static Name field of each Transmission
    public string transmission_name;

    // payload, or json, to deserialize into the Transmission (once we know the specific type)
    public string transmission_payload;
}
