using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;

// purpose is to find the NetworkController and call the appropriate function to connect.

public class NetworkSelector : MonoBehaviour
{
    public InputField field;

    private NetworkController m_controller;
    private LoaderScript m_loader;

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
	
    public void ConnectToHost()
    {
        // find the Network Controller and call corresponding function.
        // need to dig out the IP Address from the text box.

        if (field != null)
        {
            IPAddress ipaddress = null;
            bool success = IPAddress.TryParse(field.text, out ipaddress);

            if (success)
            {
                if (m_controller != null && m_loader != null)
                {
                    success = m_controller.ConnectToHost(ipaddress);

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
        if (m_controller != null && m_loader != null)
        {
            bool success = m_controller.ListenForConnections(1);

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
