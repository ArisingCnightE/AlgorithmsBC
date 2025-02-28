using UnityEngine;


public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds = new RectInt(0, 0, 100, 50);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AlgorithmsUtils.DebugRectInt(dungeonBounds, Color.yellow);
    }
}
