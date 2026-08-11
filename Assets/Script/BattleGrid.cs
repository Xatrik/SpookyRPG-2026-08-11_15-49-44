using UnityEngine;

public class BattleGrid : MonoBehaviour
{
    [SerializeField]
    int gridSize;
    [SerializeField]
    float spacing;
    [SerializeField]
    GameObject square;
    [SerializeField]
    Vector2 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Instantiate(square, new Vector3((i - gridSize/2) * spacing + offset.x, (j - gridSize/2) * spacing + offset.y, 0), Quaternion.identity);
            }
        }
        transform.position = transform.up * 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
