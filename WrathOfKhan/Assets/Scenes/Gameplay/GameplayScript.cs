using UnityEngine;
using System.Collections;

public class GameplayScript : MonoBehaviour
{
    public Vector2 LevelSize;

    private NetworkController m_networkController = null;

    private bool m_firstFrameInit = false;

    static GameplayScript sm_this;
    public static GameplayScript Get() { return sm_this; }

    //there are n ships for n players
    //this id binds this computer to one of those ships
    public int localPlayerIndex;
    public PlayerShipScript GetLocalPlayer()
    {
        PlayerShipScript[] players = GetComponentsInChildren<PlayerShipScript>();
        return players[localPlayerIndex];
    }

    // Use this for initialization
    void Start ()
    {
        sm_this = this;

        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene != null)
        {
            m_networkController = loaderScene.GetComponent<NetworkController>();
        }
        else
        {
            Debug.LogError("Failed to find LoaderScene. Should always be present");
        }

        if (m_networkController)
        {
            localPlayerIndex = m_networkController.GetLocalPlayerInfo().playerID;
        }
    }


	void Update ()
    {
        if (!m_firstFrameInit)
        {
            if (localPlayerIndex == 0)
            {
                // we're the host. Host always goes first (easiest).
                GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.WaitForTurn);
            }

            HeatMap.Get().Initialize( LevelSize );
            ScanManager.Get().Initialize( LevelSize );

            m_firstFrameInit = true;
        }
    }

    public void EndLocalPlayerTurn()
    {
        if (m_networkController)
        {
            m_networkController.SendTransmission(new EndTurnTransmission());
        }
    }

    public void OnEndTurnNetworkEvent(EndTurnTransmission transmission)
    {
        // previous person ended their turn, so we should start our turn.

        GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.WaitForTurn);
    }
}
