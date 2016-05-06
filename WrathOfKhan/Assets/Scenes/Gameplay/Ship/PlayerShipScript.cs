using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}

    public GameObject camera;
    public GameObject torpedoGO;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;

    //true - movement. false - pause + aimer
    bool flying = true;

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.G))
        {
            flying = !flying;
            for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(!flying); }
        }
        if ( !flying )
        {
            //keeps dot count matched to public int
            int toSpawn = aimerDotCount - transform.childCount;
            if (toSpawn > 0)
            {
                for (int i = 0; i < toSpawn; ++i)
                {
                    GameObject dotChild = Instantiate(aimerDotGO);
                    dotChild.transform.parent = transform;
                    dotChild.SetActive(true);
                }
            }
            else if (toSpawn < 0)
            {
                for (int i = transform.childCount - 1; i >= aimerDotCount; --i) { GameObject.Destroy(transform.GetChild(i)); }
            }

            //dot aimer
            Vector3 aimerPos = camera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            aimerPos.z = 0;
            Vector3 aimerDir = aimerPos.normalized;
            Vector3 aimerStart = transform.position + aimerDir * (GetComponent<CircleCollider2D>().radius + 0.1f); //step outside

            //iterate n fixed steps
            Vector3 stepPos = aimerStart;
            Vector3 stepVelo = aimerDir * torpedoVelo;
            for (int i = 0; i < aimerDotCount; ++i)
            {
                Vector3 gravForce = TorpedoScript.GetNBodyForceAtPos(stepPos);
                Vector3 accel = gravForce / mass;
                stepVelo += accel * Time.fixedDeltaTime;
                stepPos += stepVelo * Time.fixedDeltaTime;
                transform.GetChild(i).transform.position = stepPos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                GameObject torp = (GameObject)Instantiate(torpedoGO, aimerStart, new Quaternion());
                torp.GetComponent<TorpedoScript>().velocity = aimerDir * torpedoVelo;
                flying = true;
                for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(false); }
            }
        }
	}

    void FixedUpdate()
    {
        if (!flying) { return; }
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
