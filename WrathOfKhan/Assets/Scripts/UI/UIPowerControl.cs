using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIPowerControl : MonoBehaviour 
{
    private UIPowerSystem[] m_systems;
    public Text m_txtPowerRemaining;

    private int m_availablePower = 5;

	// Use this for initialization
	void Start () 
    {
        m_systems = new UIPowerSystem[3];
        m_systems[0] = this.transform.FindChild("Weapons").GetComponent<UIPowerSystem>();
        m_systems[1] = this.transform.FindChild("Shields").GetComponent<UIPowerSystem>();
        m_systems[2] = this.transform.FindChild("Engines").GetComponent<UIPowerSystem>();

        UpdateAvailablePower();
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public void AdjustPower( int powerAdjustment )
    {
        m_availablePower += powerAdjustment;
        Debug.Assert(m_availablePower >= 0);

        UpdateAvailablePower();

        UIManager.Get().UpdateSystemPower(m_systems[0].m_power, m_systems[1].m_power, m_systems[2].m_power);
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
