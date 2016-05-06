using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public GameObject torpedoGO;

	// Update is called once per frame
	void Update () {

        if ( Input.anyKeyDown )
        {
            Debug.Log(transform.forward * 10.0f);
            GameObject torp = (GameObject)Instantiate(torpedoGO, transform.position + velocity * 0.2f, new Quaternion());
            torp.GetComponent<TorpedoScript>().velocity = velocity;
        }
	}

    void FixedUpdate()
    {
        Vector3 gravForce = TorpedoScript.GetNBodyForceAtPos(transform.position);
        Vector3 accel = gravForce / mass;
        velocity += accel * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    public float mass {  get { return GetComponent<Rigidbody2D>().mass; } }
    public Vector3 velocity;

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("ship destroyed");
        GameObject.Destroy(gameObject); //this context is the script comopnent. destroy your master too, not just you!!
    }
}
