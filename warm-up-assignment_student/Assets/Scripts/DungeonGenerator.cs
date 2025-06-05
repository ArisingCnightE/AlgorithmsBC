using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Text;
using System.Collections;


public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds = new RectInt(0, 0, 100, 50);
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int wallHeight;
    private bool splitHorizontally;
    private List<RectInt> toDoRooms = new List<RectInt>();
    [SerializeField] private List<RectInt> doneRooms = new List<RectInt>();
    private List<RectInt> doors = new List<RectInt>();
    private List<RectInt> removedRooms = new List<RectInt>();
    private Graph<RectInt> graph = new Graph<RectInt>();
    System.Diagnostics.Stopwatch stopwatch;
    System.Random random = new();
    public GenerationMethod generationMethod;
    public enum GenerationMethod
    {
        INSTANT, COROUTINE, PRESS
    }
    public int seed;


    [Button("Generate")]
    IEnumerator Start()
    {
        //get everything ready!
        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        doneRooms.Clear();
        doors.Clear();
        toDoRooms.Add(dungeonBounds);
        random = new(seed);

        switch (generationMethod)
        {
            case GenerationMethod.INSTANT:
                for (int i = 0; i < toDoRooms.Count; i++)
                    Splitrooms(toDoRooms[i]);
                break;
            case GenerationMethod.COROUTINE:
                yield return StartCoroutine(RoomSplittingCoroutine());
                break;
            case GenerationMethod.PRESS:
                break;
        }
        DoorMaker();
        CreateGraph();

        //stopwatch stop and start generating the rooms.
        stopwatch.Stop();
        Debug.Log(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3));
       

        //make a outerwall so the border doesnt look like shit.
        RectInt outerwall = new RectInt(dungeonBounds.x - 1, dungeonBounds.y - 1, dungeonBounds.width + 2, dungeonBounds.height + 2);
        doneRooms.Add(outerwall);
        Debug.Log("amount of rooms: " + doneRooms.Count);
    }

    void Update()
    {
        foreach (RectInt room in toDoRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow, 0, false, wallHeight);
        }
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, 0, false, wallHeight);
        }
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.blue, 0, false, wallHeight);
        }
        foreach (RectInt room in removedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red, 0, false, wallHeight);
        }
        foreach (RectInt room in graph.GetNodes())
        {
            Vector3 roomCenter = new(room.center.x, 0, room.center.y);
            DebugExtension.DebugCircle(roomCenter, 1, 0, false);
            foreach (RectInt edge in graph.GetNeighbors(room))
            {
                Vector3 roomEdges = new(edge.center.x, 0, edge.center.y);
                Vector3 roomCenter1 = new(room.center.x, 0, room.center.y);
                Debug.DrawLine(roomCenter1, roomEdges, Color.white);
            }
        }
        foreach (RectInt door in graph.GetNodes())
        {
            Vector3 doorCenter = new(door.center.x, 0, door.center.y);
            DebugExtension.DebugCircle(doorCenter, 1, 0, false);
        }

    }

    IEnumerator RoomSplittingCoroutine()
    {
        while (toDoRooms.Count > 0) {
            RectInt rectIntroom = toDoRooms[0];
            toDoRooms.RemoveAt(0);
            Splitrooms(rectIntroom);
            yield return new WaitForSeconds(0.001f);
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
            RectInt newRoom1 = new RectInt(room.x, room.y, room.width, split + 1);
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
            RectInt newRoom1 = new RectInt(room.x, room.y, split + 1, room.height);
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
    #region Doors
    [Button("Doors")]
    void DoorMaker()
    {
        for (int i = 0; i < doneRooms.Count; i++)
        {
            for (int j = i + 1; j < doneRooms.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(doneRooms[i], doneRooms[j]))
                {
                    RectInt intersection = AlgorithmsUtils.Intersect(doneRooms[i], doneRooms[j]);
                    if (intersection.width > intersection.height && intersection.size.x >= 3)
                    {
                        int doorPos = random.Next(intersection.xMin + 1, intersection.xMax - 1);
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

    #region Graph
    void CreateGraph()
    {
        foreach (RectInt room in doneRooms)
        {
            graph.AddNode(room);
        }
        foreach (RectInt door in doors)
        {
            graph.AddNode(door);
        }
        foreach (RectInt room in graph.GetNodes())
        {
            // foreach (RectInt roomlink in graph.GetNodes())
            // {
            //     if (AlgorithmsUtils.Intersects(room, roomlink))
            //     {
            //         graph.AddEdge(room, roomlink);
            //     }
            // }
            foreach (RectInt door in doors)
            {
                if (AlgorithmsUtils.Intersects(door, room))
                {
                    graph.AddEdge(room, door);
                }
            }
        }
    }
    #endregion
}
