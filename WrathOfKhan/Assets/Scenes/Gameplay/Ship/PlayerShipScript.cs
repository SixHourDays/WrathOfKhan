using UnityEngine;
using System.Collections.Generic;

public class PlayerShipScript : MonoBehaviour
{
    private NetworkController m_networkController = null;

    // damage dealt to things we ram
    public float rammingDamageGiven = 0.4f;

    // damage we take from ramming things
    public float rammingDamageTaken = 0.2f;

    public int playerID; // the playerID that this ship represents. Needs to be set by the GameplayScript when this class is created.

    //this returns the one that is bound to the human playing on this computer
    public bool isLocalPlayer() { return GameplayScript.Get().localPlayerIndex == playerID; }
    public bool isRemotePlayer() { return GameplayScript.Get().localPlayerIndex != playerID; }

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

                    Debug.Log("torpedo flight");
                    turnStep = PlayerTurnSteps.FireWeapons;

                    m_shipState.torpedosRemaining -= 1;
                    firedTorpedo = FireTorpedo(aimerPos, aimerVelo);
                    
                    GameObject loaderScene = GameObject.Find("LoaderScene");

                    if (loaderScene)
                    {
                        NetworkController controller = loaderScene.GetComponent<NetworkController>();
                        if (controller)
                        {
                            FireBullet bullet = new FireBullet();

                            bullet.player_id = playerID;
                            bullet.position = aimerPos;
                            bullet.velocity = aimerVelo;

                            controller.SendTransmission(bullet);
                        }
                    }

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
                    Debug.Log("end aim engines");
                    //aimer dots off
                    for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(false); }

                    turnStep = PlayerTurnSteps.EngageEngines;

                    //ENGAGE!!
                    //dont decrement enginesRemaining here, used by Update's AimEngines
                    Debug.Log("ship flight");

                    GameObject loaderScene = GameObject.Find("LoaderScene");
                    if (loaderScene)
                    {
                        NetworkController controller = loaderScene.GetComponent<NetworkController>();
                        if (controller)
                        {
                            ShipMovedTransmission mt = new ShipMovedTransmission();

                            mt.player_id = playerID;
                            mt.end_position = aimerPos;
                            mt.translation = aimerVelo;

                            controller.SendTransmission(mt);
                        }
                    }

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
                {
                    m_shipState.enginesRemaining = 0;
                    Debug.Log("end ship flight");

                    turnStep = ChooseOrDone();
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
        // Shields should regen every turn.
        // then we start applying floating damage to everything
        
        // shields will not take damage until shieldsRemaining gets to 0. Damage then splashes to whatever system.
        // all normalized for tuning. To apply damage, multiply by the number of items in the powerbar.
        public float []systemHealth;


        public bool heavyTorpedos; //start em off
        public int torpedosRemaining;
        public float shieldsRemaining; //normalized so we can tune
        public float enginesRemaining; //noralized so we can tune
        public int sensorsTurnAge; //some number of turns to fade them off
        public bool cloaked;
        public ShipState(bool hvyTorp, int torps, float shld, float eng, int sens, bool clk)
        {
            systemHealth = new float[UIPowerControl.Get().GetNumberOfDamagableSystems()];
            heavyTorpedos = hvyTorp; torpedosRemaining = torps; shieldsRemaining = shld; enginesRemaining = eng; sensorsTurnAge = sens; cloaked = clk;
        }
    };
    ShipState m_shipState;

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
            if (isLocalPlayer())
            {
                Debug.Log("End turn");
                UIManager.Get().SetPhasesInactive();
                GameplayScript.Get().EndLocalPlayerTurn();
            }

            return PlayerTurnSteps.WaitForTurn;
        }
    }

    //shoot!
    GameObject FireTorpedo(Vector3 torpPos, Vector3 torpVelo)
    {
        //will use aimerPos and aimerVelo from AimWeapons phase
        GameObject firedTorpedo = (GameObject)Instantiate(torpedoGO, torpPos, new Quaternion());
        firedTorpedo.transform.parent = transform.parent; //make it sibling to the ship
        firedTorpedo.GetComponent<TorpedoScript>().velocity = torpVelo;

        return firedTorpedo;
    }


    // Use this for initialization
    void Start ()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene)
        {
            m_networkController = loaderScene.GetComponent<NetworkController>();
        }

        camera = FindObjectOfType<Camera>();
        Debug.Assert(camera != null);

        for (int i = 0; i < aimerDotCount; ++i)
        {
            GameObject dotChild = Instantiate(aimerDotGO);
            dotChild.transform.parent = transform;
            dotChild.SetActive(false);
            dotChild.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, (float)(aimerDotCount - i) / aimerDotCount);
        }

        m_shipState = new ShipState(false, 0, 0.0f, 0.0f, 0, false);
    }

    public GameObject torpedoGO;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;
    public float engineDistance;
    public float engineSpeed;

    //dual meaning state:
    //firing/fired - pos of where torp would start, velo of torp on spawn
    //pickDest/engine - pos is final destination, velo is vector of oldpos to new pos
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

                    //convert mouse to world screen pos, and get direction of mouse vs ship
                    Vector3 worldMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
                    worldMousePos.z = 0; //need 0 to get normalized in 2d
                    Vector3 worldMouseDir = (worldMousePos - transform.position).normalized;
                    aimerPos = transform.position + worldMouseDir * (GetComponent<CircleCollider2D>().radius + 0.2f); //step outside
                    aimerVelo = worldMouseDir * torpedoVelo;
                    
                    //gravity dot aimer via predict n fixed steps
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
                    //aimer dots on
                    if (!transform.GetChild(0).gameObject.activeSelf)
                    {
                        Debug.Log("AimEngines start");
                        for (int i = 0; i < transform.childCount; ++i) { transform.GetChild(i).gameObject.SetActive(true); }
                    }

                    //convert mouse to world screen pos, and get direction of mouse vs ship
                    Vector3 worldMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
                    worldMousePos.z = 0; //need 0 to get normalized in 2d
                    Vector3 worldMouseOffset = worldMousePos - transform.position;

                    //clamp to engine power committed
                    float maxMag = m_shipState.enginesRemaining * engineDistance;
                    if ( worldMouseOffset.magnitude > maxMag)
                    {
                        worldMouseOffset = worldMouseOffset.normalized * maxMag;
                    }

                    aimerPos = transform.position + worldMouseOffset;
                    aimerVelo = worldMouseOffset;

                    //radius dot aimer
                    for (int i = 0; i < aimerDotCount; ++i)
                    {
                        Vector3 dotPos = transform.position + worldMouseOffset * ((float)i / aimerDotCount);
                        transform.GetChild(i).transform.position = dotPos;
                    }

                    //shoot!
                    if (Input.GetMouseButtonDown(0))
                    {
                        CommitTurnStep(PlayerTurnSteps.AimEngines);
                    }

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
              {
                    //move by steps of engineSpeed, until last step, then do less
                    Vector3 offsetToDest = transform.position - aimerPos;
                    bool lastStep = false;
                    float stepDist = engineSpeed;
                    if (offsetToDest.magnitude < engineSpeed)
                    {
                        lastStep = true;
                        stepDist = offsetToDest.magnitude;
                    }
                     
                    transform.position += aimerVelo.normalized * stepDist;

                    if ( lastStep )
                    {
                        CommitTurnStep(PlayerTurnSteps.EngageEngines);
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

    public float mass {  get { return GetComponent<Rigidbody2D>().mass; } }

    public void OnBulletFiredNetworkEvent(FireBullet transmission)
    {
        if (transmission.player_id == playerID)
        {
            Debug.Assert(turnStep == PlayerTurnSteps.WaitForTurn);
            // this bullet fire is for us, simulate the bullet firing.
            FireTorpedo(transmission.position, transmission.velocity);
        }
    }

    public void OnShipMovedNetworkEvent(ShipMovedTransmission transmission)
    {
        if (transmission.player_id == playerID)
        {
            Debug.Assert(turnStep == PlayerTurnSteps.WaitForTurn);
            // this is the ship that is supposed to move. Make it move.

            aimerPos = transmission.end_position;
            aimerVelo = transmission.translation;

            turnStep = PlayerTurnSteps.EngageEngines;
        }
    }

    public void OnShipDamagedNetworkEvent(DamageShipTransmission transmission)
    {
        if (transmission.player_id == playerID)
        {
            // this is the ship that is supposed to be damaged. Damage us.
           
            DistributeDamage(transmission.damage_to_apply);

            // only update the UI if it's the local player that got damaged
            if (isLocalPlayer())
            {
                for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
                {
                    UIPowerControl.Get().SetDamageValues(i, Mathf.FloorToInt((1.0f - m_shipState.systemHealth[i]) * UIPowerControl.Get().GetNumberOfItemsInSystemBar(i)));
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isLocalPlayer())
        {
            GameObject collidedObject = col.gameObject;

            TorpedoScript torpedo = collidedObject.GetComponent<TorpedoScript>();
            PlayerShipScript otherShip = collidedObject.GetComponent<PlayerShipScript>();
            // otherwise it's a ... planet or debris.

            if (torpedo)
            {
                Debug.Log("Hit a torpedo");

                DistributeDamage(torpedo.damagePower);
            }
            else if (otherShip)
            {
                // damage the other ship A LOT, while damaging us a little. idk.

                DistributeDamage(rammingDamageTaken);

                DamageShipTransmission damageTransmission = new DamageShipTransmission();
                damageTransmission.player_id = otherShip.playerID;
                damageTransmission.damage_to_apply = rammingDamageGiven;

                m_networkController.SendTransmission(damageTransmission);

                // now stop us from moving

                // with this?
                //  CommitTurnStep(PlayerTurnSteps.EngageEngines);

                // also bounce us back a bit from the ship we just hit.

                Debug.Log("Hit a ship");
            }
            else
            {
                // ouch... planets hurt.

                Debug.Log("Hit a planet.");
            }

            if (m_shipState.systemHealth != null)
            {
                for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
                {
                    UIPowerControl.Get().SetDamageValues(i, Mathf.FloorToInt((1.0f - m_shipState.systemHealth[i]) * UIPowerControl.Get().GetNumberOfItemsInSystemBar(i)));
                }
            }
        }
    }

    //public void SetDamageToSystem

    public void DistributeDamage(float damageToApply)
    {
        if (m_shipState.shieldsRemaining > 0.0f)
        {
            if (m_shipState.shieldsRemaining > damageToApply)
            {
                m_shipState.shieldsRemaining -= damageToApply;
                damageToApply = 0.0f;
            }
            else
            {
                // this will destroy the shieldsLeft, and splash to systems underneath.
                damageToApply -= m_shipState.shieldsRemaining;
                m_shipState.shieldsRemaining = 0.0f;
            }
        }

        // now apply to a random system that can sustain damage.

        do
        {
            if (damageToApply > 0.0f)
            {
                List<int> systemsToApplyDamage = new List<int>();

                for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
                {
                    if (m_shipState.systemHealth[i] > 0.0f)
                    {
                        systemsToApplyDamage.Add(i);
                    }
                }

                int randIndex = Random.Range(0, systemsToApplyDamage.Count);

                damageToApply = ApplyDamageToSystem(systemsToApplyDamage[randIndex], damageToApply);
            }
        } while (!IsDead() && damageToApply > 0.0f);
    }

    // will return the leftover damage
    public float ApplyDamageToSystem(int index, float damage)
    {
        Debug.Assert(index >= 0 && index < m_shipState.systemHealth.Length);
        
        if (m_shipState.systemHealth[index] > damage)
        {
            m_shipState.systemHealth[index] -= damage;
            return 0.0f;
        }
        else
        {
            damage -= m_shipState.systemHealth[index];
            m_shipState.systemHealth[index] = 0;
            return damage;
        }
    }

    public bool IsDead()
    {
        for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
        {
            if (m_shipState.systemHealth[i] > 0.0f)
            {
                return false;
            }
        }

        return true;
    }
}
