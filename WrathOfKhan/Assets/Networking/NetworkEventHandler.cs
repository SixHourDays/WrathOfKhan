using UnityEngine;

public class NetworkEventHandler : MonoBehaviour
{
    public BulletNetworkEvent bulletNetworkEvent = new BulletNetworkEvent();
    public ConnectTransmissionEvent connectTransmissionEvent = new ConnectTransmissionEvent();
    public EndTurnTransmissionEvent endTurnNetworkEvent = new EndTurnTransmissionEvent();
    public ShipMovedTransmissionEvent shipMovedNetworkEvent = new ShipMovedTransmissionEvent();
    public DamageShipTransmissionEvent damageShipNetworkEvent = new DamageShipTransmissionEvent();
    public RaiseShieldsTransmissionEvent raiseShieldsNetworkEvent = new RaiseShieldsTransmissionEvent();
    public RaiseCloakTransmissionEvent raiseCloakNetworkEvent = new RaiseCloakTransmissionEvent();
    public RestartGameTransmissionEvent restartGameNetworkEvent = new RestartGameTransmissionEvent();

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

    void OnDestroy()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            NetworkController controller = loaderScene.GetComponent<NetworkController>();

            if (controller != null)
            {
                controller.RemoveEventHandler(this);
            }
            else
            {
                Debug.LogError("Failed to find NetworkController on LoaderScene object.");
            }
        }
    }

    public void OnNetworkEvent(FireBullet bullet)
    {
        if (bulletNetworkEvent != null)
        {
            bulletNetworkEvent.Invoke(bullet);
        }
    }

    public void OnNetworkEvent(ConnectTransmission transmission)
    {
        if (connectTransmissionEvent != null)
        {
            connectTransmissionEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(EndTurnTransmission transmission)
    {
        if (endTurnNetworkEvent != null)
        {
            endTurnNetworkEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(ShipMovedTransmission transmission)
    {
        if (shipMovedNetworkEvent != null)
        {
            shipMovedNetworkEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(DamageShipTransmission transmission)
    {
        if (damageShipNetworkEvent != null)
        {
            damageShipNetworkEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(RaiseShieldsTransmission transmission)
    {
        if (raiseShieldsNetworkEvent != null)
        {
            raiseShieldsNetworkEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(RaiseCloakTransmission transmission)
    {
        if (raiseCloakNetworkEvent != null)
        {
            raiseCloakNetworkEvent.Invoke(transmission);
        }
    }

    public void OnNetworkEvent(RestartGameTransmission transmission)
    {
        if (restartGameNetworkEvent != null)
        {
            restartGameNetworkEvent.Invoke(transmission);
        }
    }
}
