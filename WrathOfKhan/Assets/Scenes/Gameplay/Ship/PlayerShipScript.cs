﻿using UnityEngine;
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

    //pass the cloak postfx our position and cloak strength (inverse of our transparency)
    public Vector3 GetCloakInfo() { return new Vector3(transform.position.x, transform.position.y, 1.0f - m_shipState.cloakAlpha); }

    public enum PlayerTurnSteps
    {
        WaitForTurn,
        SetPowerLevels,
        ChooseAction,
        AimWeapons, //start of actions //fire seequence 1
        FireWeapons,    //fire sequence 2
        AimEngines, //engines 1
        EngageEngines, //engines 2
    };

    public PlayerTurnSteps turnStep = PlayerTurnSteps.WaitForTurn;

    public void SetShieldsRemaining(float remaining)
    {
        m_shipState.shieldsRemaining = remaining;
        shieldSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, m_shipState.shieldsRemaining);

    }

    public void SetCloak(bool enabled)
    {
        if (m_shipState.cloaked != enabled) { cloakSound.Play(); }
        m_shipState.cloaked = enabled;
        GetComponent<TrailRenderer>().enabled = !m_shipState.cloaked; //no trails when cloaked
    }

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
                    m_shipState.enginesRemaining = UIManager.Get().GetPowerLevel(2) / 3.0f; //normalized
                    //special
                    if ( m_shipState.isFederation )
                    {
                        SetShieldsRemaining(UIManager.Get().GetPowerLevel(1) / 3.0f); //normalized
                        m_shipState.sensors = UIManager.Get().GetPowerLevel(3) == 2;
                        UIManager.Get().EnableScanOverlay(m_shipState.sensors);
                        if (m_shipState.sensors) { scanSound.Play(); }
                    }
                    else
                    {
                        bool enableCloak = UIManager.Get().GetPowerLevel(3) == 2;
                        SetCloak( enableCloak );
                        if (m_networkController)
                        {
                            RaiseCloakTransmission rt = new RaiseCloakTransmission();
                            rt.player_id = playerID;
                            rt.cloak = enableCloak;
                            m_networkController.SendTransmission(rt);
                        }

                        //cloak means no shields
                        SetShieldsRemaining( enableCloak ? 0.0f : UIManager.Get().GetPowerLevel(1) / 3.0f); //normalized
                    }


                    if (m_shipState.shieldsRemaining > 0) { shieldUpSound.Play(); } //shields powered!

                    if (m_networkController)
                    {
                        RaiseShieldsTransmission shieldEvnt = new RaiseShieldsTransmission();

                        shieldEvnt.player_id = playerID;
                        shieldEvnt.shield_value = m_shipState.shieldsRemaining;

                        m_networkController.SendTransmission(shieldEvnt);
                    }

                    Debug.Log( "Commit end of SetPowerLevels" + m_shipState.torpedosRemaining + " " + m_shipState.shieldsRemaining + " " + m_shipState.enginesRemaining );

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
                    for (int i = 0; i < dotPile.transform.childCount; ++i) { dotPile.transform.GetChild(i).gameObject.SetActive(false); }

                    //firing when cloaked reveals you!
                    if (m_shipState.cloaked)
                    {
                        SetCloak(false);
                        if (m_networkController)
                        {
                            RaiseCloakTransmission rt = new RaiseCloakTransmission();
                            rt.player_id = playerID;
                            rt.cloak = false;
                            m_networkController.SendTransmission(rt);
                        }
                    }

                    Debug.Log("torpedo flight");
                    turnStep = PlayerTurnSteps.FireWeapons;

                    m_shipState.torpedosRemaining -= 1;
                    firedTorpedo = FireTorpedo(aimerPos, aimerVelo);
                    
                   if (m_networkController)
                    {
                        FireBullet bullet = new FireBullet();

                        bullet.player_id = playerID;
                        bullet.position = aimerPos;
                        bullet.velocity = aimerVelo;

                        m_networkController.SendTransmission(bullet);
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
            case PlayerTurnSteps.AimEngines:
                {
                    Debug.Log("end aim engines");
                    //aimer dots off
                    for (int i = 0; i < dotPile.transform.childCount; ++i) { dotPile.transform.GetChild(i).gameObject.SetActive(false); }
                    //heat trails on if max power
                    if (m_shipState.enginesRemaining == 1.0f) { m_heatTrail.enabled = true; }
                    turnStep = PlayerTurnSteps.EngageEngines;

                    //ENGAGE!!
                    //dont decrement enginesRemaining here, used by Update's AimEngines
                    Debug.Log("ship flight");

                    if (m_networkController)
                    {
                        ShipMovedTransmission mt = new ShipMovedTransmission();

                        mt.player_id = playerID;
                        mt.end_position = aimerPos;
                        mt.translation = aimerVelo;

                        m_networkController.SendTransmission(mt);
                    }

                    break;
                }
            case PlayerTurnSteps.EngageEngines:
                {
                    engineSound.Stop();
                    m_shipState.enginesRemaining = 0;
                    m_heatTrail.enabled = false;
                    Debug.Log("end ship flight");

                    turnStep = ChooseOrDone();
                    break;
                }

        }

    }

    public void SetupPlayer(int id, bool fed)
    {
        playerID = id;
        m_shipState.isFederation = fed;
        if (isLocalPlayer())
        {
            UISpecialPowerSystem special = (UISpecialPowerSystem)UIPowerControl.Get().m_systems[3];
            special.SetText(m_shipState.isFederation);
        }
    }
    public void SetupPlayer(int id, Sprite shipSprite, bool fed)
    {
        GetComponent<SpriteRenderer>().sprite = shipSprite;
        SetupPlayer(id, fed);
    }

    //player state (relevant across all turns)
    struct ShipState
    {
        // Shields should regen every turn.
        // then we start applying floating damage to everything
        
        // shields will not take damage until shieldsRemaining gets to 0. Damage then splashes to whatever system.
        // all normalized for tuning. To apply damage, multiply by the number of items in the powerbar.
        public float []systemHealth;

        public bool isFederation; //false would be Empire.
        public int torpedosRemaining;
        public float shieldsRemaining; //normalized so we can tune
        public float enginesRemaining; //noralized so we can tune
        public bool sensors; //some number of turns to fade them off
        public bool cloaked;
        public float cloakAlpha; //1.0 normal, 0.0 = transparent & cloaked.
        public ShipState(bool isFed)
        {
            isFederation = isFed; //overwrite w SetupPlayer later

            systemHealth = new float[UIPowerControl.Get().GetNumberOfDamagableSystems()];
            for (int i = 0; i < systemHealth.Length; ++i)
            {
                systemHealth[i] = 1.0f;
            }

            torpedosRemaining = 0;
            sensors = cloaked = false;
            cloakAlpha = 1.0f;
            shieldsRemaining = enginesRemaining = 0.0f;
        }
    };
    ShipState m_shipState;

    PlayerTurnSteps ChooseOrDone()
    {
        if ( !IsDead() && (m_shipState.torpedosRemaining > 0 || m_shipState.enginesRemaining > 0))
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


    //done immediately on instantiate
    void Awake()
    {
        m_shipState = new ShipState(true);
    }
    //done much later, right before first update.  Can assume all things loaded and Awaked at this point.
    void Start ()
    {
        GameObject loaderScene = GameObject.Find("LoaderScene");
        if (loaderScene)
        {
            m_networkController = loaderScene.GetComponent<NetworkController>();
        }

        m_camera = FindObjectOfType<Camera>();
        Debug.Assert(m_camera != null);

        m_postFX = m_camera.GetComponent<PostFX>();

        m_heatTrail = gameObject.GetComponent<HeatInjector>();
        m_heatTrail.enabled = false; //players, unlike planets, default to off

        for (int i = 0; i < aimerDotCount; ++i)
        {
            GameObject dotChild = Instantiate(aimerDotGO);
            dotChild.transform.parent = dotPile.transform;
            dotChild.SetActive(false);
        }
    }

    public GameObject torpedoGO;
    public GameObject dotPile;
    public GameObject shieldSprite;
    public GameObject aimerDotGO;
    public int aimerDotCount;
    public float torpedoVelo;
    public float engineDistance;
    public float engineSpeed;
    public float cloakingSpeed;

    public AudioSource engineSound;
    public AudioSource torpHitSound;
    public AudioSource shieldUpSound;
    public AudioSource scanSound;
    public AudioSource cloakSound;

    //dual meaning state:
    //firing/fired - pos of where torp would start, velo of torp on spawn
    //pickDest/engine - pos is final destination, velo is vector of oldpos to new pos
    Vector3 aimerPos;
    Vector3 aimerVelo;
    GameObject firedTorpedo;

    Camera m_camera; //finds out scene cam
    PostFX m_postFX; // for camera distortion when dying.
    HeatInjector m_heatTrail; //for max speed trails
    // Update is called once per frame

    private void UpdateDamageDisplay()
    {
        // get our total damage normalized between 0 and 1.

        if (IsDead())
        {
            m_postFX.m_noiseScale = 1.0f;
            return;
        }

        float value = 0.0f;

        for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
        {
            value += m_shipState.systemHealth[i];
        }

        // normalize
        value = value / (float)m_shipState.systemHealth.Length;

        // invert
        value = 1.0f - value;

        m_postFX.m_noiseScale = value;
    }
    
    void Update ()
    {
        if (isLocalPlayer())
        {
            UpdateDamageDisplay();
        }

        //cloak transitions
        if ( m_shipState.cloaked && m_shipState.cloakAlpha > 0.0f)
        {   
            //cloaking
            m_shipState.cloakAlpha -= cloakingSpeed * Time.deltaTime;
            m_shipState.cloakAlpha = Mathf.Clamp01(m_shipState.cloakAlpha);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, m_shipState.cloakAlpha);
        }
        else if ( !m_shipState.cloaked && m_shipState.cloakAlpha < 1.0f)
        {
            //decloaking
            m_shipState.cloakAlpha += cloakingSpeed * Time.deltaTime;
            m_shipState.cloakAlpha = Mathf.Clamp01(m_shipState.cloakAlpha);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, m_shipState.cloakAlpha);
        }
        //else - ship is static as fully cloaked or fully visible


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
                    if (!dotPile.transform.GetChild(0).gameObject.activeSelf)
                    {
                        Debug.Log("Aimweapons start");
                        for (int i = 0; i < dotPile.transform.childCount; ++i)
                        {
                            GameObject go = dotPile.transform.GetChild(i).gameObject;
                            go.SetActive(true);
                            go.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, (float)(aimerDotCount - i) / aimerDotCount);
                        }
                    }

                    //convert mouse to world screen pos, and get direction of mouse vs ship
                    Vector3 worldMousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                    worldMousePos.z = 0; //need 0 to get normalized in 2d
                    Vector3 worldMouseDir = (worldMousePos - transform.position).normalized;
                    aimerPos = transform.position + worldMouseDir * (GetComponent<CircleCollider2D>().radius * transform.localScale.x + 10.0f); //step outside
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
                        dotPile.transform.GetChild(i).transform.position = stepPos;
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
            case PlayerTurnSteps.AimEngines:
                {
                    //aimer dots on
                    if (!dotPile.transform.GetChild(0).gameObject.activeSelf)
                    {
                        Debug.Log("AimEngines start");
                        for (int i = 0; i < dotPile.transform.childCount; ++i)
                        {
                            GameObject go = dotPile.transform.GetChild(i).gameObject;
                            go.SetActive(true);
                            go.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, (float)(aimerDotCount - i) / aimerDotCount);
                        }
                    }

                    //convert mouse to world screen pos, and get direction of mouse vs ship
                    Vector3 worldMousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
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
                        dotPile.transform.GetChild(i).transform.position = dotPos;
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
                    if (!engineSound.isPlaying) { engineSound.Play(); }

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

                    HeatMap.Get().DiffusionStep();

                    if ( lastStep )
                    {
                        CommitTurnStep(PlayerTurnSteps.EngageEngines);
                    }

                    // turn the ship.
                    gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, aimerVelo);

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

            //DistributeDamage(100.0f); // tons of damage    
            //CommitTurnStep(PlayerTurnSteps.EngageEngines);

            //DistributeDamage(transmission.damage_to_apply);
        }
    }

    public void OnShipRaiseShieldsNetworkEvent(RaiseShieldsTransmission transmission)
    {
        if (transmission.player_id == playerID)
        {
            // this is the ship that is supposed to raise shields
            SetShieldsRemaining(transmission.shield_value);
        }
    }

    public void OnShipRaiseCloakNetworkEvent(RaiseCloakTransmission transmission)
    {
        if (transmission.player_id == playerID)
        {
            // this is the ship that is supposed to raise cloak
            SetCloak(transmission.cloak);
        }
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        GameObject collidedObject = col.gameObject;

        TorpedoScript torpedo = collidedObject.GetComponent<TorpedoScript>();
        PlayerShipScript otherShip = collidedObject.GetComponent<PlayerShipScript>();
        // otherwise it's a ... planet or debris.

        if (torpedo)
        {
            Debug.Log("Hit a torpedo");
            torpHitSound.Play();
            DistributeDamage(torpedo.damagePower);
        }
        else if (otherShip)
        {
            // damage the other ship A LOT, while damaging us a little. idk.
            

            //DistributeDamage(rammingDamageTaken);

            

            // now stop us from moving

            DistributeDamage(100.0f); // tons of damage

            //DamageShipTransmission damageTransmission = new DamageShipTransmission();
            //damageTransmission.player_id = playerID;
            //damageTransmission.damage_to_apply = 100.0f;

            //m_networkController.SendTransmission(damageTransmission);
            
            //damageTransmission.player_id = otherShip.playerID;
            //damageTransmission.damage_to_apply = 100.0f;

            //m_networkController.SendTransmission(damageTransmission);

            CommitTurnStep(PlayerTurnSteps.EngageEngines);

            // with this?
            //  

            // also bounce us back a bit from the ship we just hit.

            Debug.Log("Hit a ship");
        }
        else
        {
            // ouch... planets hurt.

            DistributeDamage(100.0f); // tons of damage

            //DamageShipTransmission damageTransmission = new DamageShipTransmission();   // send a message to everyone that I damaged myself to make sure I died there
            //damageTransmission.player_id = playerID;
            //damageTransmission.damage_to_apply = 100.0f;

            //m_networkController.SendTransmission(damageTransmission);

            CommitTurnStep(PlayerTurnSteps.EngageEngines);

            Debug.Log("Hit a planet.");
        }

        if (isLocalPlayer())
        {
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
        //hits decloak!
        if (m_shipState.cloaked)
        {
            SetCloak(false);
            if (m_networkController)
            {
                RaiseCloakTransmission rt = new RaiseCloakTransmission();
                rt.player_id = playerID;
                rt.cloak = false;
                m_networkController.SendTransmission(rt);
            }
        }

        if (m_shipState.shieldsRemaining > 0.0f)
        {
            if (m_shipState.shieldsRemaining > damageToApply)
            {
                SetShieldsRemaining(m_shipState.shieldsRemaining - damageToApply);
                damageToApply = 0.0f;
            }
            else
            {
                // this will destroy the shieldsLeft, and splash to systems underneath.
                damageToApply -= m_shipState.shieldsRemaining;
                SetShieldsRemaining(0.0f);
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

                if (systemsToApplyDamage.Count > 0)
                {
                    int randIndex = Random.Range(0, systemsToApplyDamage.Count);

                    damageToApply = ApplyDamageToSystem(systemsToApplyDamage[randIndex], damageToApply);
                }
            }
        } while (!IsDead() && damageToApply > 0.0f);

        // only update the UI if it's the local player that got damaged
        if (isLocalPlayer())
        {
            for (int i = 0; i < m_shipState.systemHealth.Length; ++i)
            {
                UIPowerControl.Get().SetDamageValues(i, Mathf.FloorToInt((1.0f - m_shipState.systemHealth[i]) * UIPowerControl.Get().GetNumberOfItemsInSystemBar(i)));
            }
        }
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
