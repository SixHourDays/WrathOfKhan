using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;

// purpose is to find the NetworkController and call the appropriate function to connect.

public class NetworkSelector : MonoBehaviour
{
    public InputField ipAddressField;
    public InputField numberOfPlayersField;

    public Sprite m_federationShip1;
    public Sprite m_federationShip2;
    public Sprite m_federationShip3;
    public Sprite m_empireShip1;
    public Sprite m_empireShip2;
    public Sprite m_empireShip3;

    public SpriteRenderer m_selectedShip;

    private NetworkController m_controller;
    private LoaderScript m_loader;

    public int m_selectedShipIndex = 0;

    // Use this for initialization
    void Start()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            m_controller = loaderScene.GetComponent<NetworkController>();
            m_loader = loaderScene.GetComponent<LoaderScript>();
        }
        else
        {
            Debug.LogError("Failed to find LoaderScene. Should always be present");
        }
    }

    public void SelectFederationShip1()
    {
        m_selectedShipIndex = 0;
    }

    public void SelectFederationShip2()
    {
        m_selectedShipIndex = 1;
    }

    public void SelectFederationShip3()
    {
        m_selectedShipIndex = 2;
    }

    public void SelectEmpireShip1()
    {
        m_selectedShipIndex = 3;
    }

    public void SelectEmpireShip2()
    {
        m_selectedShipIndex = 4;
    }

    public void SelectEmpireShip3()
    {
        m_selectedShipIndex = 5;
    }


    public void UpdateSelectedShip()
    {
        if (m_selectedShipIndex == 0)
        {
            m_selectedShip.sprite = m_federationShip1;
        }

        if (m_selectedShipIndex == 1)
        {
            m_selectedShip.sprite = m_federationShip2;
        }

        if (m_selectedShipIndex == 2)
        {
            m_selectedShip.sprite = m_federationShip3;
        }

        if (m_selectedShipIndex == 3)
        {
            m_selectedShip.sprite = m_empireShip1;
        }

        if (m_selectedShipIndex == 4)
        {
            m_selectedShip.sprite = m_empireShip2;
        }

        if (m_selectedShipIndex == 5)
        {
            m_selectedShip.sprite = m_empireShip3;
        }
    }

    public void ToggleMusic(bool enabled)
    {
        m_loader.ToggleMusic(enabled);
    }

    void Update()
    {
        UpdateSelectedShip();
    }

    public void ConnectToHost()
    {
        // find the Network Controller and call corresponding function.
        // need to dig out the IP Address from the text box.

        if (ipAddressField != null)
        {
            IPAddress ipaddress = null;
            bool success = IPAddress.TryParse(ipAddressField.text, out ipaddress);

            if (success)
            {
                if (m_controller != null && m_loader != null)
                {
                    success = m_controller.ConnectToHost(ipaddress, m_selectedShipIndex);

                    if (success)
                    {
                        // await messages from the host to "fix up" our connections.
                        //m_loader.SwitchToSceneNamed("GameplayScene");
                    }
                    else
                    {
                        Debug.Log("Failed to connect to host.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to find NetworkController or LoaderScript on LoaderScene object.");
                }
            }
            else
            {
                Debug.LogError("Failed to parse ip address");
            }
        }
    }

    public void HostGame()
    {
        if (numberOfPlayersField == null)
        {
            Debug.LogError("NumberOfPlayersField is not set.");
            return;
        }

        int numPlayers = 0;

        if (int.TryParse(numberOfPlayersField.text, out numPlayers) && numPlayers > 1)
        {
            if (m_controller != null && m_loader != null)
            {
                bool success = m_controller.ListenForConnections(numPlayers - 1, m_selectedShipIndex);

                if (success)
                {
                    m_loader.SwitchToSceneNamed("GameplayScene");
                }
                else
                {
                    Debug.Log("Failed to Retrieve all connections");
                }
            }
            else
            {
                Debug.LogError("Failed to find NetworkController or LoaderScript on LoaderScene object.");
            }
        }
        else
        {
            numberOfPlayersField.text = "";
        }
    }

    public void OnConnectTransmission(ConnectTransmission connectEvent)
    {
        // deal with it properly, then start the gameplay screen.

        if (m_controller)
        {
            m_controller.OnConnectTransmission(connectEvent);
        }

        m_loader.SwitchToSceneNamed("GameplayScene");
    }
}
