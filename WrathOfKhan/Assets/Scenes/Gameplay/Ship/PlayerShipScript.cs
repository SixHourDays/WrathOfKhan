using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour
{
    private NetworkController m_networkController = null;

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
        AimWeapons, //start of actions //fire seequence 1
        FireWeapons,    //fire sequence 2
        ShieldsUp,
        AimEngines, //engines 1
        EngageEngines, //engines 2
        LongRangeSensors,
        EngageCloak,
    };

    public PlayerTurnSteps turnStep = PlayerTurnSteps.WaitForTurn;

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
                    m_shipState.torpedosRemaining = UIManager.Get().GetPowerLevel(0); //a literal count
                    m_shipState.shieldsRemaining = UIManager.Get().GetPowerLevel(1) / 3.0f; //normalized
                    m_shipState.enginesRemaining = UIManager.Get().GetPowerLevel(2) / 3.0f; //normalized
                    //...
                    Debug.Log( "Commit end of SetPowerLevels" + m_shipState.torpedosRemaining + " " + m_shipState.shieldsRemaining + " " + m_shipState.enginesRemaining );
                    //TODO toggle UI with avaiable buttons for powers picked

                    //internal log of chosen state
                    turnStep = ChooseOrDone();

                    break;
                }
            case PlayerTurnSteps.ChooseAction:
                {
                    //UI choose which action to do
                    UIManager.Get().SetPhasesInactive();
                    Debug.Log("commit action" + UIManager.Get().actionChoice.ToString());

                    Debug.Log("start action");
                    turnStep = UIManager.Get().actionChoice;
                    break;
                }
            case PlayerTurnSteps.AimWeapons:
                {
                    Debug.Log("end aimWeapons");
                    //aimer dots off
                    for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(false); }

                    turnStep = PlayerTurnSteps.FireWeapons;

                    //shoot!
                    Debug.Log("torpedo flight");

                    m_shipState.torpedosRemaining -= 1;
                    //will use aimerPos and aimerVelo from AimWeapons phase
                    firedTorpedo = (GameObject)Instantiate(torpedoGO, aimerPos, new Quaternion());
                    firedTorpedo.transform.parent = transform.parent; //make it sibling to the ship
                    firedTorpedo.GetComponent<TorpedoScript>().velocity = aimerVelo;

                    /*GameObject loaderScene = GameObject.Find("LoaderScene");

                    if (loaderScene)
                    {
                        NetworkController controller = loaderScene.GetComponent<NetworkController>();
                        if (controller)
                        {
                            FireBullet bullet = new FireBullet();

                            bullet.SetPosition(aimerPos);
                            bullet.SetVelocity(aimerVelo);

                            controller.SendTransmission(bullet);
                        }
                    }
                    */
                    break;
                }
            case PlayerTurnSteps.FireWeapons:
                {
                    Debug.Log("end flight");
                    
                    //return to power choice if any left
                    //elsewise end turn
                    turnStep = ChooseOrDone();

                    break;
                }
            case PlayerTurnSteps.ShieldsUp:
                {

                    break;
                }
            case PlayerTurnSteps.AimEngines:
                {

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
                {
                    UIManager.Get().SetPhasesInactive();

                    Debug.Log("chosen engageEngines");
                    turnStep = PlayerTurnSteps.FireWeapons;

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

    PlayerTurnSteps ChooseOrDone()
    {
        if (m_shipState.torpedosRemaining > 0 || m_shipState.shieldsRemaining > 0 || m_shipState.enginesRemaining > 0)
        {
            Debug.Log("Start ChooseAction");
            UIManager.Get().SetPhaseTwoActive(); //sets all buttons on

            UIManager.Get().GetPhase(1).SetButtonActive("Shoot", m_shipState.torpedosRemaining > 0);
            UIManager.Get().GetPhase(1).SetButtonActive("Move", m_shipState.enginesRemaining > 0);
            //... shields
            //... special


            return PlayerTurnSteps.ChooseAction;
        }
        else
        {
            Debug.Log("End turn");
            UIManager.Get().SetPhasesInactive();
            GameplayScript.Get().EndLocalPlayerTurn();
            return PlayerTurnSteps.WaitForTurn;
        }
    }


    // Use this for initialization
    void Start ()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene)
        {
            m_networkController = loaderScene.GetComponent<NetworkController>();
        }


        shipIndex = shipCount++;

        camera = FindObjectOfType<Camera>();
        Debug.Assert(camera != null);

        for (int i = 0; i < aimerDotCount; ++i)
        {
            GameObject dotChild = Instantiate(aimerDotGO);
            dotChild.transform.parent = transform;
            dotChild.SetActive(false);
            dotChild.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, (float)(aimerDotCount - i) / aimerDotCount);
        }
    }

    public GameObject torpedoGO;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;

    //state across Firing/Fired
    Vector3 aimerPos;
    Vector3 aimerVelo;
    GameObject firedTorpedo;

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
                    //todo
                    //spins until UIManager callsback a CommitTurnStep
                    break;
                }
            case PlayerTurnSteps.AimWeapons:
                {
                    //aimer dots on
                    if (!transform.GetChild(0).gameObject.activeSelf)
                    {
                        Debug.Log("Aimweapons start");
                        for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(true); }
                    }

                    //gravity dot aimer
                    Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0; //need 0 to get normalized in 2d
                    Vector3 mouseDir = (mousePos - transform.position).normalized;
                    aimerPos = transform.position + mouseDir * (GetComponent<CircleCollider2D>().radius + 0.2f); //step outside
                    aimerVelo = mouseDir * torpedoVelo;

                    //iterate n fixed steps
                    Vector3 stepPos = aimerPos;
                    Vector3 stepVelo = aimerVelo;
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
                        CommitTurnStep(PlayerTurnSteps.AimWeapons);
                    }
                    
                    break;
              }
            case PlayerTurnSteps.FireWeapons:
                {
                    //listen for our torpedo to die
                    if (firedTorpedo == null)
                    {
                        CommitTurnStep(PlayerTurnSteps.FireWeapons);
                    }
                    break;
                }
          case PlayerTurnSteps.ShieldsUp:
              {

                  break;
              }
            case PlayerTurnSteps.AimEngines:
                {
                    /*
                    //aimer dots on
                    if (!transform.GetChild(0).gameObject.activeSelf)
                    {
                        Debug.Log("AimEngines start");
                        for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(true); }
                    }

                    //radius dot aimer
                    Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0; //need 0 to get normalized in 2d
                    Vector3 mouseDir = (mousePos - transform.position).normalized;
                    aimerPos = transform.position + mouseDir * (GetComponent<CircleCollider2D>().radius + 0.2f); //step outside
                    aimerVelo = mouseDir * torpedoVelo;

                    //iterate n fixed steps
                    Vector3 stepPos = aimerPos;
                    Vector3 stepVelo = aimerVelo;
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
                        CommitTurnStep(PlayerTurnSteps.AimWeapons);
                    }
                    */

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
