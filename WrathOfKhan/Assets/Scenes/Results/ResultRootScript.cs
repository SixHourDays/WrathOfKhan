using UnityEngine;
using System.Collections;

public class ResultRootScript : MonoBehaviour
{
    NetworkController m_networkController = null;
    LoaderScript m_loader = null;

	// Use this for initialization
	void Start ()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            m_networkController = loaderScene.GetComponent<NetworkController>();
            m_loader = loaderScene.GetComponent<LoaderScript>();
        }
        else
        {
            Debug.LogError("Failed to find LoaderScene. Should always be present");
        }
    }
	

    public void OnClickRestart()
    {
        m_networkController.SendTransmission(new RestartGameTransmission());

        m_loader.SwitchToSceneNamed("GameplayScene");
    }

    public void OnRestartGameNetworkEvent(RestartGameTransmission transmission)
    {
        m_loader.SwitchToSceneNamed("GameplayScene");
    }
}
