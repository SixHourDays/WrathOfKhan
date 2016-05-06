using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;

// purpose is to find the NetworkController and call the appropriate function to connect.

public class NetworkSelector : MonoBehaviour
{
    public InputField field;

	// Use this for initialization
	void Start ()
    {
	
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
                GameObject loaderScene = GameObject.Find("LoaderScene");
                if (loaderScene != null)
                {
                    NetworkController controller = loaderScene.GetComponent<NetworkController>();
                    LoaderScript loaderScript = loaderScene.GetComponent<LoaderScript>();

                    if (controller != null && loaderScript != null)
                    {
                        success = controller.ConnectToHost(ipaddress);

                        if (success)
                        {
                            loaderScript.SwitchToSceneNamed("GameplayScene");
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
                    Debug.LogError("Failed to find LoaderScene. Should always be present");
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
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            NetworkController controller = loaderScene.GetComponent<NetworkController>();
            LoaderScript loaderScript = loaderScene.GetComponent<LoaderScript>();

            if (controller != null && loaderScript != null)
            {
                bool success = controller.ListenForConnections();

                if (success)
                {
                    loaderScript.SwitchToSceneNamed("GameplayScene");
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
            Debug.LogError("Failed to find LoaderScene. Should always be present");
        }
    }
}
