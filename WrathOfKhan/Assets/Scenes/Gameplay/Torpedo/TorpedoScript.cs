using UnityEngine;
using System.Collections;

public class TorpedoScript : MonoBehaviour {

    public float torpedoLifetime;
    float torpedoStartTime;

    // Use this for initialization
    void Start () {
        torpedoStartTime = Time.time;
	}

    public static Vector3 GetNBodyForceAtPos(Vector3 pos)
    {
        GameObject[] gravBodies = GameObject.FindGameObjectsWithTag("GravBody");

        Vector3 gravForce = new Vector3(0, 0, 0);
        foreach (GameObject go in gravBodies)
        {
            // f = GM / r2 when orbiting mass is insignificant
            Vector3 offset = go.transform.position - pos;
            float forceMag = go.GetComponent<GravBodyScript>().GM / offset.sqrMagnitude;
            Vector3 force = offset.normalized * forceMag;
            gravForce += force;
        }
        return gravForce;
    }

    public float mass { get { return GetComponent<Rigidbody2D>().mass; } }
    public Vector3 velocity;

    void FixedUpdate()
    {
        Vector3 gravForce = GetNBodyForceAtPos(transform.position);
        Vector3 accel = gravForce / mass;
        velocity += accel * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }


    // Update is called once per frame
    void Update () {
	
        if ( Time.time - torpedoStartTime > torpedoLifetime )
        {
            GameObject.Destroy(gameObject);
        }
	}
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("torpedo destroyed");
        GameObject.Destroy(gameObject); //this context is the script comopnent. destroy your master too, not just you!!
    }
}
