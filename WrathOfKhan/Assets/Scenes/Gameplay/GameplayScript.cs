using UnityEngine;
using System.Collections.Generic;

public class GameplayScript : MonoBehaviour
{
    private NetworkController m_networkController = null;
    private LoaderScript m_loader = null;

    private bool m_firstFrameInit = false;
    private bool m_showingDead = false;

    static GameplayScript sm_this;
    public static GameplayScript Get() { return sm_this; }

    //there are n ships for n players
    //this id binds this computer to one of those ships
    public int localPlayerIndex;

    public GameObject playerShipPrefab;

    public Vector2 LevelSize;

    public PlayerShipScript GetLocalPlayer()
    {
        PlayerShipScript[] players = GetComponentsInChildren<PlayerShipScript>();
        
        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].playerID == localPlayerIndex)
            {
                return players[i];
            }
        }

        return null;
    }

    public void InstantiatePlayerObject(Vector3 position, int playerID)
    {
        GameObject newShip = (GameObject)GameObject.Instantiate(playerShipPrefab, position, new Quaternion());
        newShip.transform.parent = transform; //make it our child

        if (m_networkController)
        {
            bool isFederation = m_networkController.GetPlayerInfo(playerID).spriteSelectionID <= 2;
            Sprite sprite = m_networkController.GetSpriteForPlayer(playerID);
            
            newShip.GetComponent<PlayerShipScript>().SetupPlayer(playerID, sprite, isFederation);
        }
        else
        {
            //debug offline state for playing right from gamescene
            newShip.GetComponent<PlayerShipScript>().SetupPlayer(playerID, true);
        }
    }

    // Use this for initialization
    void Start ()
    {
        sm_this = this;

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

        if (m_networkController)
        {
            localPlayerIndex = m_networkController.GetLocalPlayerInfo().playerID;

            List<NetworkController.PlayerInfo> players = m_networkController.GetRemotePlayerInfos();
            
            for (int i = 0; i < players.Count; ++i)
            {
                GameObject spawnAnchor = GameObject.Find("Anchor" + players[i].playerID);

                InstantiatePlayerObject(spawnAnchor.transform.position, players[i].playerID);
            }
        }
        else
        {
            InstantiatePlayerObject(GameObject.Find("Anchor1").transform.position, 0);
        }
    }
    
	void Update ()
    {
        if (!m_firstFrameInit)
        {
            UIManager.Get().SetPhasesInactive(); //sets the actions HUD to ghosted while we wait
            
            if (localPlayerIndex == 0)
            {
                // we're the host. Host always goes first (easiest).
                GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.WaitForTurn);
            }

            HeatMap.Get().Initialize(LevelSize);
            ScanManager.Get().Initialize(LevelSize);

            m_firstFrameInit = true;
        }

        if (!m_showingDead)
        {
            if (GetLocalPlayer().IsDead())
            {
                m_showingDead = true;
                GameObject gameOverText = this.transform.FindChild("Canvas").FindChild("GameOver").gameObject; // I feel so dumb... but GameObject.Find wouldn't find it >_>
                gameOverText.SetActive(true);
                
            }
        }

        PlayerShipScript[] players = GetComponentsInChildren<PlayerShipScript>();

        int dedPlayers = 0;
        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].IsDead())
            {
                dedPlayers++;
            }
        }

        if (m_networkController)
        {
            if (dedPlayers >= m_networkController.GetNumberPlayers() - 1)
            {
                m_loader.SwitchToSceneNamed("Results");
            }
        }
    }

    public void EndLocalPlayerTurn()
    {
        if (m_networkController)
        {
            m_networkController.SendTransmission(new EndTurnTransmission());
        }

        UIPowerControl.Get().ClearPower();
    }

    public void OnEndTurnNetworkEvent(EndTurnTransmission transmission)
    {
        // previous person ended their turn, so we should start our turn.

        if (GetLocalPlayer().IsDead())
        {
            // transfer the end turn onwards because we ded
            EndLocalPlayerTurn();
        }
        else
        {
            GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.WaitForTurn);
        }
    }
}
