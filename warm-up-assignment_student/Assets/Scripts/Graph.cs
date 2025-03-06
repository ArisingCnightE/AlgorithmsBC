using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Graph<node>
{
    private Dictionary<node, List<node>> adjacencyList;
    public Graph() {adjacencyList = new Dictionary<node, List<node>>();}
    public void AddNode(node node) 
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<node>();
        }
    }
    public void AddEdge(node fromNode,node toNode) 
    {
        if (!adjacencyList.ContainsKey(fromNode) || !adjacencyList.ContainsKey(toNode))
        {
            // Debug.Log("one or both nodes do not exist in the graph.");
            return;
        }
        
        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    // public void PrintGraph()
    // {
    //     foreach (var node in adjacencyList)
    //     {
    //         Debug.Log("key: " + node.Key);
    //         foreach (var edge in node.Value)
    //         {
    //             Debug.Log("value: " + edge);
    //         }
    //     }
    // }
}
