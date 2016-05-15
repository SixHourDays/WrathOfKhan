using UnityEngine;
using System.Collections;

public class PostShipCloakScript : MonoBehaviour
{
    public Texture2D m_distortTex;
    public Shader m_cloakShader;
    
    private Material m_cloakMat;

    public static PostShipCloakScript Get()
    {
        PostShipCloakScript me = FindObjectOfType<PostShipCloakScript>();
        Debug.Assert(me != null);
        return me;
    }

    // Use this for initialization
    void Start()
    {
        m_cloakMat = new Material(m_cloakShader);
        m_distortTex.wrapMode = TextureWrapMode.Repeat;
        m_cloakMat.SetTexture("_DistortTex", m_distortTex);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerShipScript[] players = FindObjectsOfType<PlayerShipScript>();
        Debug.Assert(players.Length <= 6); //hardcode shader for this many fields

        for( int i = 0; i < players.Length; ++i)
        {
            string uniform = "_CloakFields" + i;
            m_cloakMat.SetVector( uniform, players[i].GetCloakInfo() );
        }
        for( int i = players.Length; i < 6; ++i)
        {
            string uniform = "_CloakFields" + i;
            m_cloakMat.SetVector(uniform, new Vector3(0,0,0)); //zero out nonplayers
        }
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        src.wrapMode = TextureWrapMode.Clamp;
        Graphics.Blit(src, dst, m_cloakMat);
    }
}
