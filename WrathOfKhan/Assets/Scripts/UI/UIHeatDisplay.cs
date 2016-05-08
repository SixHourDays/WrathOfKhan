using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIHeatDisplay : MonoBehaviour 
{
    private RawImage m_heatImg;
    private HeatMap m_heatManager;

    private bool m_enabled;

	// Use this for initialization
	void Start () 
    {
        m_heatImg = this.GetComponentInChildren<RawImage>();
        Debug.Assert(m_heatImg != null);

        m_heatManager = HeatMap.Get();
        Debug.Assert(m_heatManager != null);
	}

    void Update()
    {
        if( !m_heatManager.isInitialized )
        {
            return;
        }
        
        m_heatManager.RenderHeatMap();

        RenderTexture heatTex = m_heatManager.HeatMapTexture;
        m_heatImg.texture = heatTex;

        float height = Mathf.Min(heatTex.height, Screen.height);
    }
}
