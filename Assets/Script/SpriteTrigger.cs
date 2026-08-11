using UnityEngine;

public class SpriteTrigger : MonoBehaviour
{
    
    [SerializeField]
    SpriteRenderer sprite;
    [SerializeField]
    Sprite newsprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void display()
    {
        sprite.sprite = newsprite;
        Destroy(this.gameObject);
    }
}
