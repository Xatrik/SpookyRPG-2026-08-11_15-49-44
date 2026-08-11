using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField]
    Vector2 bounds;

    [SerializeField]
    Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += new Vector3((Mathf.Round(player.position.x/bounds.x) * bounds.x - transform.position.x) / 5, (Mathf.Round(player.position.y/bounds.y) * bounds.y - transform.position.y)/ 5, 0);
    }
}
