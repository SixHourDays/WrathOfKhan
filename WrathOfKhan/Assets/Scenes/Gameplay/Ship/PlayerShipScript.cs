﻿using UnityEngine;
using System.Collections;

public class PlayerShipScript : MonoBehaviour {

    public enum PlayerTurnSteps
    {
        WaitForTurn,
        SetPowerLevels,
        ChooseAction,
        FireWeapons,
        ShieldsUp,
        EngageEngines,
        LongRangeSensors,
        EngageCloak,
    };

    public PlayerTurnSteps turnStep = PlayerTurnSteps.WaitForTurn;
    Camera camera; //finds out scene cam

    //ui state
    int uiWeaponPower = 1; //start em off
    int uiShieldPower = 1;
    int uiEnginePower = 3;
    int uiSensorPower = 0;
    int uiCloakPower = 0;

    //player state (relevant across all turns)
    bool heavyTorpedos = false; //start em off
    int torpedosRemaining = 0;
    float shieldsRemaining = 0.0f; //normalized so we can tune
    float enginesRemaining = 0.0f; //noralized so we can tune
    int sensorsTurnAge = 0; //some number of turns to fade them off
    bool cloaked = false;

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
