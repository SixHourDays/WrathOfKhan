using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIPowerSystem : MonoBehaviour 
{
    public Image[] m_imgNodes;
    public int m_power;
    public int m_destroyedPower;

    private bool m_updateMouseOver = false;
    protected UIPowerControl m_pwrControl;

    private bool m_active;

    // Use this for initialization
	protected void Start () 
    {
        GameObject nodeObj = this.transform.FindChild("Nodes").gameObject;
        m_imgNodes = nodeObj.GetComponentsInChildren<Image>();

        m_pwrControl = this.transform.parent.GetComponent<UIPowerControl>();
        Debug.Assert(m_pwrControl != null);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (m_active && m_updateMouseOver)
        {
            UpdatePowerMouseOver();
        }
	}

    public virtual void SetDamage( int damage )
    {
        m_destroyedPower = damage;
        UpdatePowerDisplay();
    }

    public void SetPower( int power )
    {
        Debug.Assert(power >= 0 && power <= m_imgNodes.Length);
        m_power = power;
        UpdatePowerDisplay();
    }

    protected void UpdatePowerDisplay()
    {
        for( int i = 0; i < m_imgNodes.Length; ++i )
        {
            if( m_power >= (i+1) )
            {
                m_imgNodes[i].sprite = UIManager.Get().GetFullPowerNodeSprite();
            }
            else if ( IsDestroyedPowerNode(i) )
            {
                m_imgNodes[i].sprite = UIManager.Get().GetDestroyedPowerNodeSprite();
            }
            else
            {
                m_imgNodes[i].sprite = UIManager.Get().GetEmptyPowerNodeSprite();
            }
        }
    }

    private Vector2 MousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 10.0f;
        return Camera.main.ScreenToWorldPoint(pos);
    }

    protected bool IsMouseOverPowerNode( int nodeIdx )
    {
        float mouseX = MousePos().x;
        Vector3 left = new Vector3(m_imgNodes[ nodeIdx ].rectTransform.rect.xMin, 0.0f, 0.0f);
        float xPos = m_imgNodes[ nodeIdx ].transform.TransformPoint(left).x;
        
        return (mouseX > xPos);
    }

    protected bool IsDestroyedPowerNode( int nodeIdx )
    {
        return (m_imgNodes.Length - m_destroyedPower < nodeIdx + 1);
    }

    protected virtual void UpdatePowerMouseOver()
    {
        for (int i = 0; i < m_imgNodes.Length; ++i)
        {
            bool powerOn = m_power >= (i + 1);
            bool powerNodeDestroyed = IsDestroyedPowerNode(i);

            int powerAdjustment = m_power - (i + 1);
            int availablePower = m_pwrControl.availablePower + powerAdjustment;

            bool displayMouseOver = IsMouseOverPowerNode(i) && (availablePower >= 0) && !powerNodeDestroyed;

            if (displayMouseOver)
            {
                if (powerOn)
                {
                    m_imgNodes[i].sprite = UIManager.Get().GetFullPowerMouseOverSprite();
                }
                else
                {
                    m_imgNodes[i].sprite = UIManager.Get().GetEmptyPowerMouseOverSprite();
                }
            }
            else
            {
                if (powerOn)
                {
                    m_imgNodes[i].sprite = UIManager.Get().GetFullPowerNodeSprite();
                }
                else if (powerNodeDestroyed)
                {
                    m_imgNodes[i].sprite = UIManager.Get().GetDestroyedPowerNodeSprite();
                }
                else
                {
                    m_imgNodes[i].sprite = UIManager.Get().GetEmptyPowerNodeSprite();
                }
            }
        }
    }

    public void ClearPower()
    {
        int powerAdjust = m_power;
        m_pwrControl.AdjustPower(powerAdjust);
        SetPower(0);
    }

    protected virtual void AssignPower()
    {     
        int numNodes = 0;

        for (int i = 0; i < m_imgNodes.Length; ++i)
        {
            int powerAdjustment = m_power - (i + 1);
            int availablePower = m_pwrControl.availablePower + powerAdjustment;

            if ( IsMouseOverPowerNode(i) && (availablePower >= 0) && !IsDestroyedPowerNode(i))
            {
                ++numNodes;
            }
        }

        int powerAdjust = m_power - numNodes;
        m_power = numNodes;
        m_pwrControl.AdjustPower(powerAdjust);
    }

    public void OnPointerEnter()
    {
        if (!m_active)
        {
            return;
        }

        m_updateMouseOver = true;
    }

    public void OnPointerExit()
    {
        if (!m_active)
        {
            return;
        }

        m_updateMouseOver = false;
        UpdatePowerDisplay();
    }

    public void OnPointerClick()
    {
        if (!m_active)
        {
            return;
        }

        AssignPower();
    }

    public void SetActive( bool active )
    {
        m_active = active;
        UpdatePowerDisplay();
    }
}
