using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class FireBullet 
{
    public int player_id;
    public Vector3 position;
    public Vector3 velocity;
}

[Serializable]
public class BulletNetworkEvent : UnityEvent<FireBullet> { }