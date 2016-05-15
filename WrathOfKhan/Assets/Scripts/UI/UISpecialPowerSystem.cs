using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UISpecialPowerSystem : UIPowerSystem
{
    public Text text;
    public void SetText(bool isFederation)
    {
        text.text = isFederation ? "Scanners" : "Cloaking";
    }


    //treating it like one system that costs 2 power to run

    //any damage is 2 damage
    public override void SetDamage(int damage)
    {
        m_destroyedPower = damage;
        UpdatePowerDisplay();
    }

    //hover behaviour accepts either and groups into 2 or none
    protected override void UpdatePowerMouseOver()
    {
        bool powerOn = m_power == 2;
        bool powerNodeDestroyed = IsDestroyedPowerNode(0); //the destroy of special takes 2 hits

        int powerAdjustment = m_power - 2;
        int availablePower = m_pwrControl.availablePower + powerAdjustment;

        bool displayMouseOver = (IsMouseOverPowerNode(0) || IsMouseOverPowerNode(1)) && (availablePower >= 0) && !powerNodeDestroyed;

        if (displayMouseOver)
        {
            if (powerOn)
            {
                m_imgNodes[0].sprite = UIManager.Get().GetFullPowerMouseOverSprite();
            }
            else
            {
                m_imgNodes[0].sprite = UIManager.Get().GetEmptyPowerMouseOverSprite();
            }
        }
        else
        {
            if (powerOn)
            {
                m_imgNodes[0].sprite = UIManager.Get().GetFullPowerNodeSprite();
            }
            else if (powerNodeDestroyed)
            {
                m_imgNodes[0].sprite = UIManager.Get().GetDestroyedPowerNodeSprite();
            }
            else
            {
                m_imgNodes[0].sprite = UIManager.Get().GetEmptyPowerNodeSprite();
            }
        }
        //dupe
        m_imgNodes[1].sprite = m_imgNodes[0].sprite;
    }

    //click behaviour treats clicking either as going straight to power 2
    protected override void AssignPower()
    {
        int powerAdjustment = m_power - 2;
        int availablePower = m_pwrControl.availablePower + powerAdjustment;

        if ((IsMouseOverPowerNode(0) || IsMouseOverPowerNode(1)) && (availablePower >= 0) && !IsDestroyedPowerNode(0))
        {
            m_power = 2;
            m_pwrControl.AdjustPower(powerAdjustment);
        }
        else
        {
            m_pwrControl.AdjustPower(m_power); //give it back
            m_power = 0;
        }
    }
}