using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeatMap : MonoBehaviour 
{
    public float WorldToTextureScale = 0.5f;

    private int m_heatGridWidth = 128;
    private int m_heatGridHeight = 128;

    private int m_texWidth = 2048;
    private int m_texHeight = 2048;

    public float m_maxHeat = 40.0f;
    public float m_noiseScale = 10.0f;
    public float m_noiseSpeed = 100.0f;

    public float m_heatLossSpeed = 0.1f;

    public ComputeShader m_computeHeatMap;
    private RenderTexture m_heatMapTex;
    public Texture2D m_heatMapGradient;
    private RenderTexture[] m_heatGridTex;

    private bool m_flipTex = false;

    private bool m_initialized = false;

    private Vector2 m_worldSize;

    private List<HeatInjector> m_listeners;

    private static HeatMap m_instance = null;

	// Use this for initialization
	void Start () 
    {
        m_initialized = false;
        m_listeners = new List<HeatInjector>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	}

    public static HeatMap Get()
    {
        if (!m_instance)
        {
            m_instance = FindObjectOfType<HeatMap>();
        }
        Debug.Assert(m_instance != null);
        return m_instance;
    }

    public void Initialize( Vector2 levelSize )
    {
        if( m_initialized )
        {
            return;
        }

        m_worldSize = levelSize;
        Vector2 textureSize = WorldToTextureScale * levelSize;

        m_heatGridWidth = (int)textureSize.x;
        m_heatGridHeight = (int)textureSize.y;

        m_texWidth = (int)textureSize.x;
        m_texHeight = (int)textureSize.y;

        m_heatGridTex = new RenderTexture[2];
        for (int i = 0; i < 2; ++i)
        {
            m_heatGridTex[i] = new RenderTexture(m_heatGridWidth, m_heatGridHeight, 0, RenderTextureFormat.RFloat);
            m_heatGridTex[i].enableRandomWrite = true;
            m_heatGridTex[i].Create();
        }

        m_heatMapTex = new RenderTexture(m_texWidth, m_texHeight, 0, RenderTextureFormat.ARGB32);
        m_heatMapTex.enableRandomWrite = true;
        m_heatMapTex.Create();

        m_initialized = true;
    }

    public void RegisterListener( HeatInjector listener )
    {
        m_listeners.Add(listener);
    }

    public void Unregister( HeatInjector listener )
    {
        m_listeners.Remove(listener);
    }

    void InsertRandomObjects(int numObj)
    {
        for (int i = 0; i < numObj; ++i )
        {
            Vector2 pos = new Vector2(Random.Range(0, m_heatGridWidth - 1), Random.Range(0, m_heatGridHeight - 1));
            Vector2 size = new Vector2(Random.Range(0.1f * m_heatGridWidth, 0.25f * m_heatGridWidth), Random.Range(0.1f * m_heatGridHeight, 0.25f * m_heatGridHeight));
            float heatVal = Random.Range(m_maxHeat * 0.25f, m_maxHeat * 0.8f);

           // InjectHeatObject(pos, size, heatVal);
        }
    }

    public void InjectHeatObject( Vector2 worldPos, Vector2 worldSize, float worldCenterRadius, float heatStrength )
    {
        if( !m_initialized )
        {
            return;
        }

        Vector2 pos = WorldToTextureScale * (worldPos + (m_worldSize / 2.0f));
        Vector2 size = WorldToTextureScale * worldSize;
        float centerRadius = WorldToTextureScale * worldCenterRadius;

        Vector2 halfSize = size / 2.0f;

        int kernelId = m_computeHeatMap.FindKernel("InjectHeat");
        m_computeHeatMap.SetTexture(kernelId, "HeatTexDst", CurrentSrcTexture);
        m_computeHeatMap.SetInt("GridWidth", m_heatGridWidth);
        m_computeHeatMap.SetInt("GridHeight", m_heatGridHeight);
        
        m_computeHeatMap.SetInt("injectStartX", (int)(pos.x-halfSize.x));
        m_computeHeatMap.SetInt("injectStartY", (int)(pos.y-halfSize.y));
        m_computeHeatMap.SetInt("injectEndX", (int)(pos.x+halfSize.x));
        m_computeHeatMap.SetInt("injectEndY", (int)(pos.y+halfSize.y));
        m_computeHeatMap.SetFloat("injectStrength", heatStrength);
        m_computeHeatMap.SetFloat("injectCenterRadius", centerRadius);

        m_computeHeatMap.Dispatch(kernelId, (int)size.x / 8, (int)size.y / 8, 1);
    }

    public void DiffusionStep()
    {
        if (!m_initialized)
        {
            return;
        }

        int kernelId = m_computeHeatMap.FindKernel("DiffuseHeat");
        m_computeHeatMap.SetTexture(kernelId, "HeatTexSrc", CurrentSrcTexture);
        m_computeHeatMap.SetTexture(kernelId, "HeatTexDst", CurrentDstTexture);
        m_computeHeatMap.SetInt("GridWidth", m_heatGridWidth);
        m_computeHeatMap.SetInt("GridHeight", m_heatGridHeight);
        m_computeHeatMap.SetFloat("heatLossSpeed", m_heatLossSpeed);
        m_computeHeatMap.SetFloat("deltaTime", Time.deltaTime);
        m_computeHeatMap.Dispatch(kernelId, m_heatGridWidth / 8, m_heatGridHeight / 8, 1);

        FlipTex();

        BroadcastHeatUpdate();
    }

    private void BroadcastHeatUpdate()
    {
        for( int i = 0; i < m_listeners.Count; ++i )
        {
            m_listeners[i].HeatUpdate();
        }
    }

    private int GetHeatGridIdx( int x, int y )
    {
        return ((y * m_heatGridWidth) + x);
    }

    public void RenderHeatMap()
    {
        if (!m_initialized)
        {
            return;
        }

        float time = Mathf.Sin(m_noiseSpeed * Time.fixedTime) + 1.0f;

        int kernelId = m_computeHeatMap.FindKernel("RenderHeatMap");
        m_computeHeatMap.SetTexture(kernelId, "Result", m_heatMapTex);
        m_computeHeatMap.SetTexture(kernelId, "HeatTexSrc", CurrentSrcTexture);
        m_computeHeatMap.SetTexture(kernelId, "HeatGradient", m_heatMapGradient);
        m_computeHeatMap.SetFloat("MaxHeat", m_maxHeat);
        m_computeHeatMap.SetInt("GridWidth", m_heatGridWidth);
        m_computeHeatMap.SetInt("GridHeight", m_heatGridHeight);
        m_computeHeatMap.SetVector("TexToGrid", new Vector4((float)m_heatGridWidth / (float)m_texWidth, (float)m_heatGridHeight / (float)m_texHeight, 0.0f, 0.0f));
        m_computeHeatMap.SetFloat("noiseScale", m_noiseScale);
        m_computeHeatMap.SetFloat("timeVal", time);
        m_computeHeatMap.Dispatch(kernelId, m_texWidth / 8, m_texHeight / 8, 1);
    }

    public RenderTexture HeatMapTexture
    {
        get { return m_heatMapTex; }
    }

    public bool isInitialized
    {
        get { return m_initialized;  }
    }

    private RenderTexture CurrentSrcTexture
    {
        get
        {
            if( m_flipTex )
            {
                return m_heatGridTex[0];
            }
            else
            {
                return m_heatGridTex[1];
            }

        }
    }

    private RenderTexture CurrentDstTexture
    {
        get
        {
            if (m_flipTex)
            {
                return m_heatGridTex[1];
            }
            else
            {
                return m_heatGridTex[0];
            }

        }
    }

    private void FlipTex()
    {
        m_flipTex = !m_flipTex;
    }
}
