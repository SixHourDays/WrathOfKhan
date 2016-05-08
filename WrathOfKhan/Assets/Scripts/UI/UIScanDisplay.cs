using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIScanDisplay : MonoBehaviour 
{
    private RawImage m_scanImg;
    private ScanManager m_scanManager;

    private bool m_enabled;

	// Use this for initialization
	void Start () 
    {
        m_scanImg = this.GetComponentInChildren<RawImage>();
        Debug.Assert(m_scanImg != null);

        m_scanManager = ScanManager.Get();
        Debug.Assert(m_scanManager != null);
	}


    void Update()
    {
        if (!m_scanManager.isInitialized)
        {
            return;
        }

        Texture2D scanTex = m_scanManager.ScanTexture;
        m_scanImg.texture = scanTex;

   //     float height = m_scanImg.canvas.pixelRect.height;
    //    float width = scanTex.width * (height / scanTex.height);

    //    m_scanImg.rectTransform.sizeDelta = new Vector2(width, height);
    }
}
