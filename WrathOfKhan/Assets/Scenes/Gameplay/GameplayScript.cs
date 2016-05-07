using UnityEngine;
using System.Collections;

public class GameplayScript : MonoBehaviour {


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
    void Start () {
        sm_this = this;

        localPlayerIndex = 0; //DAN!!!
	}

    public void EndLocalPlayerTurn()
    {
        Debug.Log("GameplayScript end local turn");
        //DAN!
    }

    // Update is called once per frame
    int dumbCount = 0;
	void Update () {
	
        //FAKE SYNC
        //wait 500 frames then END waiting for turn;
        if ( ++dumbCount == 250 )
        {
            GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.WaitForTurn);
        }
	}
}
