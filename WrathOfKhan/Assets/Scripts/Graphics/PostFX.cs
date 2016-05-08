using UnityEngine;
using System.Collections;

public class PostFX : MonoBehaviour 
{
    public Texture2D m_crackedGlassTex;
    public Shader m_damageShader;

    public float m_noiseScale;

    private Material m_damageMat;

	// Use this for initialization
	void Start () 
    {
        m_damageMat = new Material(m_damageShader);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        src.wrapMode = TextureWrapMode.Repeat;
        m_damageMat.SetTexture("_CrackedGlass", m_crackedGlassTex);
        m_damageMat.SetFloat("_NoiseScale", m_noiseScale);
        Graphics.Blit(src, dst, m_damageMat);
    }
}
