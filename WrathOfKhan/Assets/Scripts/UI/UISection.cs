using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UISection : MonoBehaviour 
{
    private GameObject m_darken;
    private RectTransform[] m_controls;

    // Use this for initialization
    private bool initialized = false;
    void Start()
    {
        if (initialized) { return; }

        m_darken = this.transform.FindChild("Darken").gameObject;
        Debug.Assert(m_darken != null);

        GameObject ctrlObj = this.transform.FindChild("Controls").gameObject;
        Debug.Assert(ctrlObj != null);
        m_controls = ctrlObj.transform.GetComponentsInChildren<RectTransform>();

        initialized = true;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void SetButtonActive(string name, bool active)
    {
        foreach (RectTransform rect in m_controls)
        {
            if (rect.gameObject.name == name)
            {
                rect.gameObject.GetComponent<Button>().interactable = active;
                break;
            }
        }
    }

    public void SetActive(bool active)
    {
        if (!initialized) { Start(); }

        if ( active )
        {
            m_darken.SetActive(false);

            for( int i = 0; i < m_controls.Length; ++i)
            {
                Button b = m_controls[i].gameObject.GetComponent<Button>();
                if( b != null )
                {
                    b.interactable = true;
                }

                UIPowerSystem pwrSys = m_controls[i].gameObject.GetComponent<UIPowerSystem>();
                if( pwrSys != null )
                {
                    pwrSys.SetActive( true );
                }
            }
        }
        else
        {
            m_darken.SetActive(true);

            for (int i = 0; i < m_controls.Length; ++i)
            {
                Button b = m_controls[i].gameObject.GetComponent<Button>();
                if (b != null)
                {
                    b.interactable = false;
                }

                UIPowerSystem pwrSys = m_controls[i].gameObject.GetComponent<UIPowerSystem>();
                if (pwrSys != null)
                {
                    pwrSys.SetActive( false );
                }
            }
        }
    }
}
