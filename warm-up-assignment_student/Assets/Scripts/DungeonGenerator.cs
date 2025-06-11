using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.AI.Navigation;


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

    public GameObject wall;
    public GameObject floor;
    public GameObject wallParent;
    public GameObject floorParent;
    public GameObject bounds;  
    [SerializeField] private HashSet<Vector3> wallPos = new();
    [SerializeField] private NavMeshSurface navMeshSurface;
    public GenerationMethod generationMethod;
    public enum GenerationMethod
    {
        INSTANT, COROUTINE
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

        SpawnAssets();
        BakeNavMesh();
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
        while (toDoRooms.Count > 0)
        {
            RectInt rectIntroom = toDoRooms[0];
            toDoRooms.RemoveAt(0);
            Splitrooms(rectIntroom);
            yield return new WaitForSeconds(0.001f);
        }
    }


    #region Spliting
    /*
    Sequence roomsplitting(r)
        make minimum room size
        make list todorooms
        make list donerooms
        add dungeon max size to todorooms
            split rooms in todorooms horizontally or vertically
            if horrizontally 
                pick a random place along the height of the room
                    create new room with the random height as start pos
                    if the new room is larger than the minimum room(width and height) add room to todorooms
                    else add room to donerooms
            if vertically
                pick a random place along the width of the room
                create new room with the random width as start pos
                if the new room is larger than the minimum room(height and width) add room to todorooms
                else add to donerooms
    */
    
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

            if (newRoom1.height >= minRoomSize * 2 || newRoom1.width >= minRoomSize * 2)
                toDoRooms.Add(newRoom1);
            else
                doneRooms.Add(newRoom1);

            if (newRoom2.width >= minRoomSize * 2 || newRoom2.height >= minRoomSize * 2)
                toDoRooms.Add(newRoom2);
            else
                doneRooms.Add(newRoom2);
        }

        else
        {
            int split = random.Next(minRoomSize, room.width - minRoomSize);
            RectInt newRoom1 = new RectInt(room.x, room.y, split + 1, room.height);
            RectInt newRoom2 = new RectInt(room.x + split, room.y, room.width - split, room.height);

            if (newRoom1.height >= minRoomSize * 2 || newRoom1.width >= minRoomSize * 2)
                toDoRooms.Add(newRoom1);
            else
                doneRooms.Add(newRoom1);

            if (newRoom2.width >= minRoomSize * 2 || newRoom2.height >= minRoomSize * 2)
                toDoRooms.Add(newRoom2);
            else
                doneRooms.Add(newRoom2);
        }
    }
    #endregion
    #region Doors
    /*
    Sequence doormaker
    make door list
    loop through all the rooms
        loop through all the next rooms
            if two rooms intersect
                if horizontally intersect and overlap is longer than 3 units
                    pick a random pos on this intersection
                    make a rect int there
                    add this rect int to doorslist
                if vertically intersects and overlap is longer than 3 units
                    pick a random pos on this intersection
                    make a rect int there
                    add this rect int to doorslist
    */
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
    /*
    Sequence CreateGraph
        make a node in the middle of all rooms
        make a node on every door pos
        loop through each room node in the graph
            loop through each door node in the graph
                if the door node is in a room 
                    make an edge from the middle of the room to the door node
    */
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
            foreach (RectInt door in doors)
            {
                if (AlgorithmsUtils.Intersects(door, room))
                {
                    graph.AddEdge(room, door);
                }
            }
        }
    }

    [Button("connected?")]
    public void BFS()
    {
        RectInt startNode = doneRooms[0];
        Queue<RectInt> queue = new Queue<RectInt>();
        queue.Enqueue(startNode);
        List<RectInt> discovered = new List<RectInt>();
        discovered.Add(startNode);


        while (queue.Count > 0)
        {
            RectInt newNode = queue.Dequeue();
            foreach (RectInt node in graph.GetNeighbors(newNode))
            {
                if (!discovered.Contains(node))
                {
                    queue.Enqueue(node);
                    discovered.Add(node);
                }
            }
        }
        if (discovered.Count == graph.GetNodeCount())
        {
            Debug.Log("CONNECTED");
        }
        else
        {
            Debug.Log("NOT CONNECTED");
        }

    }
    #endregion

    #region SpawnAssets 
    [Button("spawnDungeon")]
    void SpawnAssets()
    {
        int roomNumber = 0;
        foreach (RectInt room in doneRooms)
        {
            GameObject roomParents = new GameObject("room" + roomNumber);
            roomNumber++;
            roomParents.transform.SetParent(wallParent.transform);

            for (int x = 0; x < room.width; x++)
            {
                if (!OverlapsDoor(room.x + x, room.y))
                {
                    CreateWall(new Vector3(room.x + x + 0.5f, 0.5f, room.y + 0.5f), roomParents.transform);
                }
            }
            for (int y = 0; y < room.height; y++)
            {
                if (!OverlapsDoor(room.x, room.y + y))
                {
                    CreateWall(new Vector3(room.x + 0.5f, 0.5f, room.y + y + 0.5f), roomParents.transform);
                }
            }
        }

        for (int x = 0; x < dungeonBounds.width; x++)
        {
            CreateWall(new Vector3(x + 0.5f, 0.5f, dungeonBounds.height + 0.5f), bounds.transform);
            CreateWall(new Vector3(x + 0.5f, 0.5f, dungeonBounds.height - 0.5f), bounds.transform);
        }
        for (int y = 0; y <= dungeonBounds.height; y++)
        {
            CreateWall(new Vector3(dungeonBounds.width + 0.5f, 0.5f, y + 0.5f), bounds.transform);
            CreateWall(new Vector3(dungeonBounds.width - 0.5f, 0.5f, y + 0.5f), bounds.transform);
        }
        
        foreach (RectInt room in doneRooms)
        {
            foreach (Vector2Int position in room.allPositionsWithin)
            {
                if (!wallPos.Contains(new Vector3(position.x + 0.5f, 0.5f, position.y + 0.5f)))
                {
                    Instantiate(floor, new Vector3(position.x + 0.5f, 0, position.y + 0.5f), floor.transform.rotation, floorParent.transform);
                }
            }
        }
    }

    bool OverlapsDoor(int x, int y)
    {
        foreach (RectInt door in doors)
        {
            if (door.y == y && door.x == x) return true;
        }
        return false;
    }

    void CreateWall(Vector3 newposition, Transform wallParent)
    {
        if (!wallPos.Contains(newposition))
        {
            Instantiate(wall, newposition, quaternion.identity, wallParent);
            wallPos.Add(newposition);
        }
    }
    #endregion
    
    #region Navmesh

    [Button]
    public void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    #endregion
}
