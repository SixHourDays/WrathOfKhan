using UnityEngine;
using System.Collections;

public class HeatMap : MonoBehaviour 
{
    public int m_heatGridWidth = 128;
    public int m_heatGridHeight = 128;

    public int m_texWidth = 2048;
    public int m_texHeight = 2048;

    public ComputeShader m_computeHeatMap;
    private ComputeBuffer m_heatBuffer;
    private float[] m_heatGrid;

    private RenderTexture m_heatMapTex;

	// Use this for initialization
	void Start () 
    {
        m_heatBuffer = new ComputeBuffer(m_heatGridWidth * m_heatGridHeight, sizeof(float));
        m_heatGrid = new float[m_heatGridWidth * m_heatGridHeight];

        m_heatMapTex = new RenderTexture(m_texWidth, m_texHeight, 0, RenderTextureFormat.ARGB32);
        m_heatMapTex.enableRandomWrite = true;
        m_heatMapTex.Create();
	}
	
	// Update is called once per frame
	void Update () 
    {
        RenderHeatMap();
	}

    public void AddHeat( int x, int y, float heatValue )
    {
        int idx = (y * m_heatGridWidth) + x;
        m_heatGrid[idx] += heatValue;
    }

    public void RenderHeatMap()
    {
        int kernelId = m_computeHeatMap.FindKernel("RenderHeatMap");
        m_computeHeatMap.SetTexture(kernelId, "Result", m_heatMapTex);
        m_computeHeatMap.Dispatch(kernelId, m_texWidth / 8, m_texHeight / 8, 1);
    }

    public RenderTexture HeatMapTexture
    {
        get { return m_heatMapTex; }
    }
}
