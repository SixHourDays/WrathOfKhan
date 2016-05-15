using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIPowerControl : MonoBehaviour 
{
    public UIPowerSystem[] m_systems;
    public Text m_txtPowerRemaining;

    private int m_availablePower = 5;

    private static UIPowerControl m_instance = null;

	// Use this for initialization
	void Start () 
    {
        UpdateAvailablePower();
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public static UIPowerControl Get()
    {
        if (!m_instance)
        {
            m_instance = FindObjectOfType<UIPowerControl>();
        }
        Debug.Assert(m_instance != null);
        return m_instance;
    }

    public int GetNumberOfDamagableSystems()
    {
        return m_systems.Length;
    }

    public int GetNumberOfItemsInSystemBar(int index)
    {
        return m_systems[index].m_imgNodes.Length;
    }

    public void ClearPower()
    {
        for (int i = 0; i < m_systems.Length; ++i)
        {
            m_systems[i].ClearPower();
        }
    }

    public void AdjustPower( int powerAdjustment )
    {
        m_availablePower += powerAdjustment;
        Debug.Assert(m_availablePower >= 0);

        UpdateAvailablePower();

        UIManager.Get().UpdateSystemPower(m_systems[0].m_power, m_systems[1].m_power, m_systems[2].m_power, m_systems[3].m_power);
    }

    public void SetDamageValues(int index, int damage)
    {
        m_systems[index].ClearPower();
        m_systems[index].SetDamage(damage);
    }

    public int availablePower
    {
        get { return m_availablePower;  }
    }

    private void UpdateAvailablePower()
    {
        m_txtPowerRemaining.text = "" + m_availablePower;
    }
}
