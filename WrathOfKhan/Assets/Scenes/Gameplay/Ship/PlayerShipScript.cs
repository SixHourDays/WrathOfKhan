using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

    public enum PlayerTurnSteps
    {
        WaitForTurn,
        SetPowerLevels,
        ChooseAction,
        FireWeapons,    //start of actions
        ShieldsUp,
        EngageEngines,
        LongRangeSensors,
        EngageCloak,
    };

    public PlayerTurnSteps turnStep = PlayerTurnSteps.WaitForTurn;
    Camera camera; //finds out scene cam

    //ui state
    struct UIState
    {
        int weaponPower; //start em off
        int shieldPower;
        int enginePower;
        int sensorPower;
        int cloakPower;
        public UIState(int wep, int shld, int eng, int sens, int clk)
        {   weaponPower = wep; shieldPower = shld; enginePower = eng; sensorPower = sens; cloakPower = clk; }
    };
    UIState m_uiState = new UIState(1,1,3,0,0); //sum of 5

    //player state (relevant across all turns)
    struct ShipState
    {
        bool heavyTorpedos; //start em off
        int torpedosRemaining;
        float shieldsRemaining; //normalized so we can tune
        float enginesRemaining; //noralized so we can tune
        int sensorsTurnAge; //some number of turns to fade them off
        bool cloaked;
        public ShipState(bool hvyTorp, int torps, float shld, float eng, int sens, bool clk)
        { heavyTorpedos = hvyTorp; torpedosRemaining = torps; shieldsRemaining = shld; enginesRemaining = eng; sensorsTurnAge = sens; cloaked = clk; }
    };
    ShipState m_shipState = new ShipState(false, 0, 0.0f, 0.0f, 0, false);

    // Use this for initialization
    void Start () {
        camera = FindObjectOfType<Camera>();
        Debug.Assert(camera != null);
    }

    public GameObject torpedoGO;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;

    //true - movement. false - pause + aimer
    bool flying = true;

	// Update is called once per frame
	void Update () {

        switch (turnStep)
        {
            case PlayerTurnSteps.WaitForTurn:
                {
                    //keep processing:
                    //can view heatmap during all this!
                    //can see incoming fire at your ship, their movements

                    //HACKJEFFGIFFEN i am THE player
                    turnStep = PlayerTurnSteps.SetPowerLevels;
                    break;
                }
            case PlayerTurnSteps.SetPowerLevels:
                {
                    //todo
                    //unlock powerbar UI
                    //draw guides (shield strength, move ranges)
                    //get back power levels from it

                    weaponPower = 1;
                    shieldPower = 2;
                    enginePower = 3;

                    turnStep = PlayerTurnSteps.ChooseAction;
                    break;
                }
            case PlayerTurnSteps.ChooseAction:
                {
                    if ( weaponPower)
                    //todo
                    //lock powerbar UI
                    //unlock power order UI
                    //get back which power,

                    turnStep = PlayerTurnSteps.FireWeapons;
                    //turnStep = PlayerTurnSteps.ShieldsUp;
                    //turnStep = PlayerTurnSteps.EngageEngines;
                    //turnStep = PlayerTurnSteps.LongRangeSensors;
                    //turnStep = PlayerTurnSteps.EngageCloak;
                    break;
                }
            case PlayerTurnSteps.FireWeapons:
                {
                    //todo
                    //lock power order UI
                    //do move range picker, or torpedo aimer, or scan sweep overlay, 
                    //then actally move, shoot, or scan
                    
                    //finally - check for any remaining actions, and loop, or out.
                    break;
                }
            case PlayerTurnSteps.ShieldsUp:
                {

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
                {

                    break;
                }
            case PlayerTurnSteps.LongRangeSensors:
                {

                    break;
                }
            case PlayerTurnSteps.EngageCloak:
                {
                    break;
                }
        }

        //toggle between flight mode and aimNShoot mode
        if (Input.GetKeyDown(KeyCode.Space))
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
            Vector3 aimerPos = camera.ScreenToWorldPoint(Input.mousePosition);
            aimerPos.z = 0; //need 0 to get normalized in 2d
            Vector3 aimerDir = (aimerPos - transform.position).normalized;
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

            //shoot!
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 velocity = aimerDir * torpedoVelo;
                GameObject torp = (GameObject)Instantiate(torpedoGO, aimerStart, new Quaternion());
                torp.GetComponent<TorpedoScript>().velocity = velocity;

                GameObject loaderScene = GameObject.Find("LoaderScene");
                if (loaderScene)
                {
                    NetworkController controller = loaderScene.GetComponent<NetworkController>();
                    if (controller)
                    {
                        FireBullet bullet = new FireBullet();

                        bullet.x = aimerStart.x;
                        bullet.y = aimerStart.y;
                        bullet.z = aimerStart.z;

                        bullet.vx = velocity.x;
                        bullet.vy = velocity.y;
                        bullet.vz = velocity.z;

                        controller.SendTransmission(bullet);
                    }
                }

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

    public void OnBulletFired(FireBullet bullet)
    {
        Debug.Log("Fired a bullet at (" + bullet.x + ", " + bullet.y + ", " + bullet.z + ")");

        Vector3 vel = new Vector3(bullet.vx, bullet.vy, bullet.vz);
        Vector3 pos = new Vector3(bullet.x, bullet.y, bullet.z);

        GameObject torp = (GameObject)Instantiate(torpedoGO, pos, new Quaternion());
        torp.GetComponent<TorpedoScript>().velocity = vel;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("ship destroyed");
        GameObject.Destroy(gameObject); //this context is the script comopnent. destroy your master too, not just you!!
    }
}
