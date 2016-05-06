using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.position = transform.position + new Vector3(0.01f,0.0f,0.0f);

	}

    void OnCollisionEnter2D(Collision2D col)
    {
        GameObject.Destroy(gameObject); //this context is the script comopnent. destroy your master too, not just you!!
    }
}
