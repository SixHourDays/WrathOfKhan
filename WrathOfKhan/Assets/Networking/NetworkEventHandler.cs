using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class NetworkEventHandler : MonoBehaviour
{
    public BulletNetworkEvent bulletNetworkEvent = new BulletNetworkEvent();

    public void Start()
    {
        // add ourselves to the NetworkController to be called when we receive network events.

        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            NetworkController controller = loaderScene.GetComponent<NetworkController>();

            if (controller != null)
            {
                controller.AddEventHandler(this);
            }
            else
            {
                Debug.LogError("Failed to find NetworkController on LoaderScene object.");
            }
        }
        else
        {
            Debug.LogError("Failed to find LoaderScene. Should always be present");
        }
    }

    public void OnNetworkEvent(FireBullet bullet)
    {
        if (bulletNetworkEvent != null)
        {
            bulletNetworkEvent.Invoke(bullet);
        }
    }
}
