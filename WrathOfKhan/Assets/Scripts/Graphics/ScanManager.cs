using UnityEngine;
using System.Collections;

public class ScanManager : MonoBehaviour 
{
    public float m_worldToTextureScale = 0.5f;
    public int m_numberRaycasts = 1024;

    public Texture2D m_scanTexture;
    private bool m_initialized = false;
    private Vector2 m_worldSize;
    private float m_maxRayDist;

	// Use this for initialization
	void Start () 
    {
        m_initialized = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public static ScanManager Get()
    {
        ScanManager scanMan = FindObjectOfType<ScanManager>();
        Debug.Assert(scanMan != null);
        return scanMan;
    }

    public void Initialize( Vector2 levelSize )
    {
        if( m_initialized )
        {
            return;
        }

        m_worldSize = levelSize;
        Vector2 textureSize = m_worldToTextureScale * levelSize;

        m_maxRayDist = Mathf.Sqrt(m_worldSize.x * m_worldSize.x + m_worldSize.y * m_worldSize.y);

        m_scanTexture = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.ARGB32, false);
        m_scanTexture.filterMode = FilterMode.Point;

        m_initialized = true;
    }

    public bool isInitialized
    {
        get { return m_initialized; }
    }

    public void RunScan( Vector2 worldPos, float startRadius )
    {
       // Vector3 pos = worldPos + (m_worldSize / 2.0f);

        float rayStepDeg = 360.0f / (float)m_numberRaycasts;

        Color[] colors = new Color[m_scanTexture.width * m_scanTexture.height];

        for( int i = 0; i < colors.Length; ++i)
        {
            colors[i] = 0.25f*Color.green;
        }

        int numHits = 0;
        for( int i = 0; i < m_numberRaycasts; ++i )
        {
            Vector2 dirVec = Quaternion.AngleAxis(i * rayStepDeg, Vector3.forward) * Vector2.up;
            dirVec.Normalize();

            Vector2 pos = worldPos + (dirVec * startRadius);
            RaycastHit2D hit = Physics2D.Raycast(pos, dirVec, m_maxRayDist);

            if( hit.collider != null )
            {
                Vector2 hitPos = pos + (dirVec * hit.distance);
                Vector2 texPos = m_worldToTextureScale * (hitPos + (m_worldSize / 2.0f));

                int idx = (int)texPos.x + m_scanTexture.width * (int)texPos.y;
                colors[idx] = Color.white;

                ++numHits;
            }
        }

        m_scanTexture.SetPixels(colors);
        m_scanTexture.Apply();
    }

    public Texture2D ScanTexture
    {
        get { return m_scanTexture; }
    }
}
