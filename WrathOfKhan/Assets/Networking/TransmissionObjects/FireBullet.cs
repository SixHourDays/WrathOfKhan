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

    public void SetPosition(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public void SetVelocity(Vector3 vel)
    {
        vx = vel.x;
        vy = vel.y;
        vz = vel.z;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x, y, z);
    }

    public Vector3 GetVelocity()
    {
        return new Vector3(vx, vy, vz);
    }
}

[Serializable]
public class BulletNetworkEvent : UnityEvent<FireBullet> { }