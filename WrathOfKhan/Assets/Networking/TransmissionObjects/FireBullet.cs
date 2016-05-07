using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

[Serializable]
public class FireBullet 
{
    public float x;
    public float y;
    public float z;

    public float vx;
    public float vy;
    public float vz;
}

[Serializable]
public class BulletNetworkEvent : UnityEvent<FireBullet> { }
// perhaps... I should have the structure:

// { "action" : "name_of_action", "nested_json" : "{......}" }
// that nested_json will need to be re-fed into JsonUtility and switched on a map for the proper type.
// should test this to make sure it works properly