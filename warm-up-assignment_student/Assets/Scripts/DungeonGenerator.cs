using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine;
using Unity.VisualScripting;


public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds = new RectInt(0, 0, 100, 50);
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int wallHeight;
    private bool splitHorizontally;
    private List<RectInt> toDoRooms = new List<RectInt>();
    private List<RectInt> doneRooms = new List<RectInt>();
    private List<RectInt> doors = new List<RectInt>();
    System.Diagnostics.Stopwatch stopwatch;
    System.Random random= new();
    public int seed;


    [Button("Generate")]
    void Start()
    {
        //get everything ready!
        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        doneRooms.Clear();
        doors.Clear();
        toDoRooms.Add(dungeonBounds);
        random = new(seed);

        //generate dungeon!
        for (int i = 0; i < toDoRooms.Count; i++)
            Splitrooms(toDoRooms[i]);

        toDoRooms.Clear();

        stopwatch.Stop();
        Debug.Log(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3));
        DoorMaker();


        RectInt outerwall = new RectInt(dungeonBounds.x-1, dungeonBounds.y-1, dungeonBounds.width+2,dungeonBounds.height+2);
        doneRooms.Add(outerwall);
    }

    void Update()
    {
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow,0, false, wallHeight);
        }
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.blue,0, false, wallHeight);
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
            int split = random.Next(minRoomSize, room.height - minRoomSize);
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
            int split = random.Next(minRoomSize, room.width - minRoomSize);
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
    #region doors
    void DoorMaker()
    {
        for (int i = 0; i < doneRooms.Count; i++)
        {
            for(int j = i + 1; j < doneRooms.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(doneRooms[i], doneRooms[j]))
                {
                    RectInt intersection = AlgorithmsUtils.Intersect(doneRooms[i], doneRooms [j]);
                    if (intersection.width > intersection.height && intersection.size.x >= 3)
                    {
                        int doorPos = random.Next(intersection.xMin + 1, intersection.xMax -1);
                        RectInt door = new RectInt(doorPos, intersection.y, 1, 1);
                        doors.Add(door);
                    }
                    if (intersection.height > intersection.width && intersection.size.y >= 3)
                    {
                        int doorPos = random.Next(intersection.yMin + 1, intersection.yMax - 1);
                        RectInt door = new RectInt(intersection.x, doorPos, 1, 1);
                        doors.Add(door);
                    }
                }
            }
        }
    }

    #endregion
}
