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

    private bool m_init = false;

    public void Start()
    {
        Transform hudObj = this.transform.FindChild("HUD");
        m_phase1 = hudObj.FindChild("Phase_1").GetComponent<UISection>();
        Debug.Assert(m_phase1 != null);
        m_phase2 = hudObj.FindChild("Phase_2").GetComponent<UISection>();
        Debug.Assert(m_phase2 != null);
    }

    public void Update()
    {
        if(!m_init)
        {
            SetPhaseOneActive();
            m_init = true;
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

    public void CommitPower()
    {
        SetPhaseTwoActive();
    }
}
