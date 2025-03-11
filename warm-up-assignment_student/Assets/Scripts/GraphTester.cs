using UnityEngine;

public class GraphTester : MonoBehaviour
{
void Start() 
{
    Graph<string> graph = new Graph<string>();
        graph.AddNode("A");
        graph.AddNode("B");
        graph.AddNode("C");
        graph.AddNode("D");
        graph.AddNode("E");
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "D");
        graph.AddEdge("D", "E");
    Debug.Log("BFS Traversal");
    graph.DFS("A");
}
}
