using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour 
{
    public Texture2D m_emptyPowerNodeTexture;
    public Texture2D m_fullPowerNodeTexture;
    public Texture2D m_emptyPowerMouseOverTexture;
    public Texture2D m_fullPowerMouseOverTexture;

    private UISection m_phase1;
    private UISection m_phase2;

    public void Start()
    {
        Transform hudObj = this.transform.FindChild("HUD");
        m_phase1 = hudObj.FindChild("Phase_1").GetComponent<UISection>();
        Debug.Assert(m_phase1 != null);
        m_phase2 = hudObj.FindChild("Phase_2").GetComponent<UISection>();
        Debug.Assert(m_phase2 != null);

        //wait for turn to enable
        SetPhasesInactive();
    }

    public void Update()
    {

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
}
