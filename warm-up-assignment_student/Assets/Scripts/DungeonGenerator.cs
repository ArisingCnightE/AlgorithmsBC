using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine;


public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds = new RectInt(0, 0, 100, 50);
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int wallHeight;
    private bool splitHorizontally;
    private List<RectInt> toDoRooms = new List<RectInt>();
    private List<RectInt> doneRooms = new List<RectInt>();

    System.Diagnostics.Stopwatch stopwatch;

    
    [Button("Generate")]
    void Start()
    {
        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        doneRooms.Clear();

        RectInt outerwall = new RectInt(dungeonBounds.x-1, dungeonBounds.y-1, dungeonBounds.width+2,dungeonBounds.height+2);
        doneRooms.Add(outerwall);
        toDoRooms.Add(dungeonBounds);

        for (int i = 0; i < toDoRooms.Count; i++)
            Splitrooms(toDoRooms[i]);

        toDoRooms.Clear();
        
        stopwatch.Stop();
        Debug.Log(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3));
    }

    void Update()
    {
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow,0, false, wallHeight);
        }
    }

    #region Spliting
    void Splitrooms(RectInt room)
    {
        if (room.width > room.height)
            splitHorizontally = false;
        else
            splitHorizontally = true;

        if (splitHorizontally)
        {
            int split = UnityEngine.Random.Range(minRoomSize, room.height - minRoomSize);
            RectInt newRoom1 = new RectInt(room.x, room.y, room.width, split +1);
            RectInt newRoom2 = new RectInt(room.x, room.y + split, room.width, room.height - split);

            if (newRoom1.height >= minRoomSize * 2 && newRoom1.width >= minRoomSize * 2)
                toDoRooms.Add(newRoom1);
            else
                doneRooms.Add(newRoom1);

            if (newRoom2.width >= minRoomSize * 2 && newRoom2.height >= minRoomSize * 2)
                toDoRooms.Add(newRoom2);  
            else
                doneRooms.Add(newRoom2);
        }

        else
        {
            int split = UnityEngine.Random.Range(minRoomSize, room.width - minRoomSize);
            RectInt newRoom1 = new RectInt(room.x, room.y, split +1, room.height);
            RectInt newRoom2 = new RectInt(room.x + split, room.y, room.width - split, room.height);

            if (newRoom1.height >= minRoomSize * 2 && newRoom1.width >= minRoomSize * 2)
                toDoRooms.Add(newRoom1);
            else
                doneRooms.Add(newRoom1);

            if (newRoom2.width >= minRoomSize * 2 && newRoom2.height >= minRoomSize * 2)
                toDoRooms.Add(newRoom2);  
            else
                doneRooms.Add(newRoom2); 
        }
    }
    #endregion 
}
