using UnityEngine;
using System.Collections;

public class HeatInjector : MonoBehaviour 
{
    public float heatStrength = 0.0f;
    public float influenceRadius = 10.0f;

    public bool useSprite = true;

    private SpriteRenderer m_sprite;

    private bool m_registered = false;

    void Start()
    {
        if (useSprite)
        {
            m_sprite = this.GetComponent<SpriteRenderer>();
            Debug.Assert(m_sprite != null);
        }

        m_registered = false;
    }

    // Update is called once per frame
	void Update () 
    {
	    if( !m_registered && HeatMap.Get().isInitialized )
        {
            HeatMap.Get().RegisterListener(this);
            m_registered = true;

            HeatUpdate();
        }
	}

    void OnDestroy()
    {
        HeatMap.Get().Unregister(this);
    }

    public void HeatUpdate()
    {
        if (useSprite)
        {
            Vector2 pos = new Vector2(m_sprite.bounds.center.x, m_sprite.bounds.center.y);
            Vector2 size = new Vector2(m_sprite.bounds.size.x, m_sprite.bounds.size.y);

            float centerRadius = Mathf.Max(size.x / 2.0f, size.y / 2.0f);

            HeatMap.Get().InjectHeatObject(pos, influenceRadius * size, 0.0f, heatStrength);
        }
        else
        {
            Vector2 pos = new Vector2(this.transform.position.x, this.transform.position.y);
            Vector2 size = new Vector2(influenceRadius, influenceRadius);
            HeatMap.Get().InjectHeatObject(pos, size, 0.0f, heatStrength);
        }
    }
}
