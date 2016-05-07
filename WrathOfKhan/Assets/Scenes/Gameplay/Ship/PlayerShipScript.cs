using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

    //there are n PlayerShipScripts in the scene to represent each player
    static int shipCount; //easy way to assign indices fixed on load order
    public int shipIndex; //of the n indices, 1 will be the local player, and n-1 will be remote players

    //this returns the one that is bound to the human playing on this computer
    public bool isLocalPlayer() { return GameplayScript.Get().localPlayerIndex == shipIndex; }
    public bool isRemotePlayer() { return GameplayScript.Get().localPlayerIndex != shipIndex; }

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

    //ui state
    public struct UIState
    {
        public int weaponPower; //start em off
        public int shieldPower;
        public int enginePower;
        public int sensorPower;
        public int cloakPower;
        public UIState(int wep, int shld, int eng, int sens, int clk)
        { weaponPower = wep; shieldPower = shld; enginePower = eng; sensorPower = sens; cloakPower = clk; }
    };
    public UIState m_uiState = new UIState(1, 1, 3, 0, 0); //sum of 5

    //return events forwarding the turn
    //any 'do once' code lives here
    public void CommitTurnStep(PlayerTurnSteps step)
    {
        switch (step)
        {
            case PlayerTurnSteps.WaitForTurn:
                {
                    Debug.Log("Commit end of WaitForTurn");

                    turnStep = PlayerTurnSteps.SetPowerLevels;
                    Debug.Log("Start SetPowerLevels");

                    UIManager.Get().SetPhaseOneActive();
                    break;
                }
            case PlayerTurnSteps.SetPowerLevels:
                {
                    //get back power levels from it
                    m_uiState.weaponPower = UIManager.Get().GetPowerLevel(0);
                    m_uiState.shieldPower = UIManager.Get().GetPowerLevel(1);
                    m_uiState.enginePower = UIManager.Get().GetPowerLevel(2);

                    Debug.Log("Commit end of SetPowerLevels" + m_uiState.weaponPower + " " + m_uiState.shieldPower + " " + m_uiState.enginePower);

                    turnStep = PlayerTurnSteps.ChooseAction;
                    Debug.Log("Start ChooseAction");

                    UIManager.Get().SetPhaseTwoActive();
                    break;
                }
            case PlayerTurnSteps.ChooseAction:
                {
                    Debug.Log("senseless!");
                    //  if ( weaponPower)
                    //todo
                    //lock powerbar UI
                    //unlock power order UI
                    //get back which power,

                    //turnStep = PlayerTurnSteps.FireWeapons;
                    //turnStep = PlayerTurnSteps.ShieldsUp;
                    //turnStep = PlayerTurnSteps.EngageEngines;
                    //turnStep = PlayerTurnSteps.LongRangeSensors;
                    //turnStep = PlayerTurnSteps.EngageCloak;
                    break;
                }
            case PlayerTurnSteps.FireWeapons:
                {
                    Debug.Log("fireWeapons");

                    //finally - check for any remaining actions, and loop, or out.
                    break;
                }
            case PlayerTurnSteps.ShieldsUp:
                {

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
                {
                    Debug.Log("engageEngines");

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

    }

    //player state (relevant across all turns)
    struct ShipState
    {
        public bool heavyTorpedos; //start em off
        public int torpedosRemaining;
        public float shieldsRemaining; //normalized so we can tune
        public float enginesRemaining; //noralized so we can tune
        public int sensorsTurnAge; //some number of turns to fade them off
        public bool cloaked;
        public ShipState(bool hvyTorp, int torps, float shld, float eng, int sens, bool clk)
        { heavyTorpedos = hvyTorp; torpedosRemaining = torps; shieldsRemaining = shld; enginesRemaining = eng; sensorsTurnAge = sens; cloaked = clk; }
    };
    ShipState m_shipState = new ShipState(false, 0, 0.0f, 0.0f, 0, false);

    

    // Use this for initialization
    void Start () {

        shipIndex = shipCount++;

        camera = FindObjectOfType<Camera>();
        Debug.Assert(camera != null);
    }

    public GameObject torpedoGO;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;

    Camera camera; //finds out scene cam

    // Update is called once per frame
    void Update ()
    {

        //this is a step by step sequence: init / live things are done here, and 'do once' / 'go to next' actions are done in CommitTurnStep above.
        switch (turnStep)
        {
            case PlayerTurnSteps.WaitForTurn:
                {
                    //keep processing:
                    //can view heatmap during all this!
                    //can see incoming fire at your ship, their movements

                    //HACKJEFFGIFFEN
                    UIManager.Get().SetPhasesInactive(); //sets the actions HUD to ghosted while we wait

                    //spins until GameplayScript notifies it's our turn
                    break;
                }
            case PlayerTurnSteps.SetPowerLevels:
                {
                    //todo
                    //draw guides (shield strength, move ranges)
                    //spins until UIManager callsback a CommitTurnStep
                    break;
                }
            case PlayerTurnSteps.ChooseAction:
                {
                  //  if ( weaponPower)
                    //todo
                    //lock powerbar UI
                    //unlock power order UI
                    //get back which power,

                    
                    break;
                }
            case PlayerTurnSteps.FireWeapons:
                {
                    //todo
                    //lock power order UI
                    //do move range picker, or torpedo aimer, or scan sweep overlay, 
                    //then actally move, shoot, or scan

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

                    bool fired = false;
                    //shoot!
                    if (Input.GetMouseButtonDown(0))
                    {
                        fired = true;
                        Vector3 velocity = aimerDir * torpedoVelo;
                        GameObject torp = (GameObject)Instantiate(torpedoGO, aimerStart, new Quaternion());
                        torp.transform.parent = transform.parent; //make it sibling to the ship
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
                    }
                    
                    //toggle between flight mode and aimNShoot mode
                    if (fired || Input.GetKeyDown(KeyCode.Space))
                    {
                        turnStep = PlayerTurnSteps.EngageEngines;
                        for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(false); }
                    }

                  //finally - check for any remaining actions, and loop, or out.
                  break;
              }
          case PlayerTurnSteps.ShieldsUp:
              {

                  break;
              }
          case PlayerTurnSteps.EngageEngines:
              {

                    //toggle between flight mode and aimNShoot mode
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        turnStep = PlayerTurnSteps.FireWeapons;
                        for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(true); }
                    }


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
        
    }

    void FixedUpdate()
    {
        if (turnStep != PlayerTurnSteps.EngageEngines) { return; }

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
        torp.transform.parent = transform.parent; //make it sibling to the ship
        torp.GetComponent<TorpedoScript>().velocity = vel;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //note we cant just start deleting players mid match - indexes rely on same count of them thruout
        Debug.Log("ship destroyed");
        gameObject.SetActive(false);
    }
}
