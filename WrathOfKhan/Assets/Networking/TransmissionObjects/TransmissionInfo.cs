﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TransmissionInfo
{
    // name that corresponds to the static Name field of each Transmission
    public string transmission_name;

    // playerID that the transmission originated from. Used to tell if we should propegate or not.
    public int transmission_from_id;

    // payload, or json, to deserialize into the Transmission (once we know the specific type)
    public string transmission_payload;
}
