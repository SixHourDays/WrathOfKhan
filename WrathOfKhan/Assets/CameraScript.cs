using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    
	void Update () {

        Vector3 playerPos = GameplayScript.Get().GetLocalPlayer().transform.position;
        transform.position = new Vector3(playerPos.x, playerPos.y, -1);
	}
}
