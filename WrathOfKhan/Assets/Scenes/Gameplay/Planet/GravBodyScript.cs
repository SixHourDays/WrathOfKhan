using UnityEngine;
using System.Collections;

public class GravBodyScript : MonoBehaviour {

    public float GM;
    public float radius { get { return GetComponent<CircleCollider2D>().radius; } }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
