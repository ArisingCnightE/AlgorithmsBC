using System.Collections.Generic;
using UnityEngine;


public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds = new RectInt(0, 0, 100, 50);
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int maxRoomSize = 10;
    [SerializeField] private bool splitHorizontally;
    [SerializeField] private List<RectInt> rooms = new List<RectInt>();
    

    void Start()
    {
        rooms.Add(dungeonBounds);
        Splitrooms(dungeonBounds);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }
    }
    void Splitrooms(RectInt room)
    {
        if (splitHorizontally)
        {
            int split = Random.Range(minRoomSize, room.height - minRoomSize);
            RectInt newRoom1 = new RectInt(room.x, room.y, room.width, split);
            rooms.Add(newRoom1);
            RectInt newRoom2 = new RectInt(room.x, room.y + split, room.width, room.height - split);
            rooms.Remove(room);
            rooms.Add(newRoom2);
        }
        else
        {
            int split = Random.Range(minRoomSize, room.width - minRoomSize);
            RectInt newRoom1 = new RectInt(room.x, room.y, split, room.height);
            rooms.Add(newRoom1);
            RectInt newRoom2 = new RectInt(room.x + split, room.y, room.width - split, room.height);
            rooms.Remove(room);
            rooms.Add(newRoom2);
        }
    }
}
