using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIPowerControl : MonoBehaviour 
{
    public UIPowerSystem[] m_systems;
    public Text m_txtPowerRemaining;

    private int m_availablePower = 10;

	// Use this for initialization
	void Start () 
    {
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
