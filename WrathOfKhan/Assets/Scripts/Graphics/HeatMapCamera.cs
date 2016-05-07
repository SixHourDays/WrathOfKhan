using UnityEngine;
using System.Collections;

public class HeatMapCamera : MonoBehaviour 
{
    private HeatMap m_heatMapControl;

	// Use this for initialization
	void Start () 
    {
        m_heatMapControl = FindObjectOfType<HeatMap>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(m_heatMapControl.HeatMapTexture, dst);
    }
}
