using UnityEngine;
using System.Collections;

public class RandomPlanet : MonoBehaviour 
{
    public Texture2D[] m_sprites;
	
    // Use this for initialization
	void Start () 
    {
        int idx = Random.Range(0, m_sprites.Length);

        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        Texture2D tex = m_sprites[idx];
        spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        CircleCollider2D circleCollider = this.GetComponent<CircleCollider2D>();
        circleCollider.radius = Mathf.Max(spriteRenderer.bounds.extents.x, spriteRenderer.bounds.extents.y);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
