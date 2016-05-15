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

        Color transparent = new Color(0, 0, 0, 0);
        for ( int y = 0; y < m_scanTexture.height; ++y)
        {
            for (int x = 0; x < m_scanTexture.width; ++x)
            {
                m_scanTexture.SetPixel(x, y, transparent);
            }
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

                m_scanTexture.SetPixel((int)texPos.x, (int)texPos.y, Color.yellow);
                ++numHits;
            }
        }

        m_scanTexture.Apply(); //upload to gpu
    }

    public Texture2D ScanTexture
    {
        get { return m_scanTexture; }
    }
}
