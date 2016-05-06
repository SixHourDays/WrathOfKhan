using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour 
{
    public Texture2D m_emptyPowerNodeTexture;
    public Texture2D m_fullPowerNodeTexture;
    public Texture2D m_emptyPowerMouseOverTexture;
    public Texture2D m_fullPowerMouseOverTexture;

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
}
