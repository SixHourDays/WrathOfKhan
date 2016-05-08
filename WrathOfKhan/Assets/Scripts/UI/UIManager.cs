using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour 
{
    public Texture2D m_emptyPowerNodeTexture;
    public Texture2D m_fullPowerNodeTexture;
    public Texture2D m_emptyPowerMouseOverTexture;
    public Texture2D m_fullPowerMouseOverTexture;
    public Texture2D m_destroyedPowerNodeTexture;

    private GameObject m_hud;
    private GameObject m_heatMap;
    private GameObject m_scanMap;
    private GameObject m_mapSelect;

    private UISection m_phase1;
    private UISection m_phase2;

    private bool m_heatMapActive = false;
    private bool m_scanMapActive = false;

    public void Start()
    {
        Transform hudObj = this.transform.FindChild("HUD");
        m_hud = hudObj.gameObject;

        m_phase1 = hudObj.FindChild("Phase_1").GetComponent<UISection>();
        Debug.Assert(m_phase1 != null);
        m_phase2 = hudObj.FindChild("Phase_2").GetComponent<UISection>();
        Debug.Assert(m_phase2 != null);

        m_mapSelect = this.transform.FindChild("MapSelect").gameObject;

        m_heatMap = this.transform.FindChild("HeatMap").gameObject;

        m_scanMap = this.transform.FindChild("ScanMap").gameObject;

        //wait for turn to enable
        SetPhasesInactive();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
        {
            EnableMainDisplay();
        }

        if( Input.GetKeyUp(KeyCode.H))
        {
            EnableHeatMap();
        }

        if( Input.GetKeyUp(KeyCode.S))
        {
            EnableScanMap();
        }
    }

    public static UIManager Get()
    {
        UIManager uiMan = FindObjectOfType<UIManager>();
        Debug.Assert(uiMan != null);
        return uiMan;
    }

    private Sprite GetSprite( Texture2D texture )
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public Sprite GetEmptyPowerNodeSprite()
    {
        return GetSprite(m_emptyPowerNodeTexture);
    }

    public Sprite GetFullPowerNodeSprite()
    {
        return GetSprite(m_fullPowerNodeTexture);
    }

    public Sprite GetEmptyPowerMouseOverSprite()
    {
        return GetSprite(m_emptyPowerMouseOverTexture);
    }

    public Sprite GetFullPowerMouseOverSprite()
    {
        return GetSprite(m_fullPowerMouseOverTexture);
    }

    public Sprite GetDestroyedPowerNodeSprite()
    {
        return GetSprite(m_destroyedPowerNodeTexture);
    }

    public void SetPhasesInactive()
    {
        m_phase1.SetActive(false);
        m_phase2.SetActive(false);
    }

    public void SetPhaseOneActive()
    {
        m_phase1.SetActive(true);
        m_phase2.SetActive(false);
    }

    public void SetPhaseTwoActive()
    {
        m_phase1.SetActive(false);
        m_phase2.SetActive(true);
    }

    int[] powerLevel = new int[3]; //weapon, shield, engine, special
    public int GetPowerLevel(int i) { return powerLevel[i]; }

    public UISection GetPhase(int i) { return i == 0 ? m_phase1 : m_phase2; }

    public void CommitPower()
    {
        GameplayScript.Get().GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.SetPowerLevels);
    }
    public PlayerShipScript.PlayerTurnSteps actionChoice { get; set; }

    public void ShootButtonDown()
    {
        actionChoice = PlayerShipScript.PlayerTurnSteps.AimWeapons; //set choice here, player script gets later
        GameplayScript.Get().GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.ChooseAction); //we are ENDING the choose phase via this
    }

    public void MoveButtonDown()
    {
        actionChoice = PlayerShipScript.PlayerTurnSteps.AimEngines;
        GameplayScript.Get().GetLocalPlayer().CommitTurnStep(PlayerShipScript.PlayerTurnSteps.ChooseAction);
    } 

    public void UpdateSystemPower( int weaponPower, int shieldPower, int enginePower )
    {
        powerLevel[0] = weaponPower;
        powerLevel[1] = shieldPower;
        powerLevel[2] = enginePower;
        //todo - 4th tier
    }

    public void EnableMainDisplay()
    {
        m_heatMapActive = false;
        m_scanMapActive = false;

        m_hud.SetActive(true);
        m_heatMap.SetActive(false);
        m_scanMap.SetActive(false);
    }

    public void EnableHeatMap()
    {
        m_heatMapActive = true;
        m_scanMapActive = false;
        
        m_hud.SetActive(false);
        m_heatMap.SetActive(true);
        m_scanMap.SetActive(false);
    }

    public void EnableScanMap()
    {
        if( !m_scanMapActive)
        {
            PlayerShipScript player = GameplayScript.Get().GetLocalPlayer();

            Vector2 pos = new Vector2(player.transform.position.x, player.transform.position.y);
            float startRadius = player.GetComponent<CircleCollider2D>().radius + 0.2f;

            ScanManager.Get().RunScan(pos, startRadius);
        }
   
        m_heatMapActive = false;
        m_scanMapActive = true;

        m_hud.SetActive(false);
        m_heatMap.SetActive(false);
        m_scanMap.SetActive(true);
    }
}
