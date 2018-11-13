using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AStarAlgorithm
{
    private Graph m_graph;
    private Heuristic m_heuristic;

    public AStarAlgorithm()
    {
        m_graph = new Graph();
        m_heuristic = new Heuristic();
    }

    PriorityQueue<NodeRecord> open = new PriorityQueue<NodeRecord>();
    List<Connection> path = new List<Connection>();

    public List<Connection> GetPath(Tile start, Tile target)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        NodeRecord startRecord = new NodeRecord(start);
        startRecord.EstimatedTotalCost = m_heuristic.Estimate(start, target);

        open.Clear();
        path.Clear();

        open.Enqueue(startRecord);
        NodeRecord current = null;

        //While there are still nodes to process
        while (open.Count > 0)
        {
            current = open.Dequeue();

            if (current.Node == target)
            {
                break;
            }

            foreach (Connection connection in m_graph.GetConnections(current))
            {
                NodeRecord neighbour = connection.GetToNode;
                float newCost = current.CostSoFar + connection.GetCost;

                if (!neighbour.visited || newCost < neighbour.CostSoFar)
                {
                    neighbour.CostSoFar = newCost;
                    neighbour.Connection = connection;
                    neighbour.EstimatedTotalCost = newCost + m_heuristic.Estimate(neighbour.Node, target);
                    open.Enqueue(neighbour);
                    neighbour.visited = true;
                }
            }

            current.visited = true;
        }

        //Builds the best path from the processed nodes.
        while (current.Node != start)
        {
            path.Add(current.Connection);
            current = current.Connection.GetFromNode;
        }

        stopwatch.Stop();
        Debug.Log("AStarPathfinding duration: " + stopwatch.ElapsedMilliseconds + " ms.");
        return path;
    }

    private class Graph
    {
        private Dictionary<Tile, NodeRecord> nodes = new Dictionary<Tile, NodeRecord>();
        private HashSet<Connection> connections = new HashSet<Connection>();

        public HashSet<Connection> GetConnections(NodeRecord node)
        {
            connections.Clear();    

            foreach (var tile in node.Node.Neighbours)
            {
                if (tile.Type != Tile.Types.Wall)
                {
                    if (!nodes.ContainsKey(tile))
                    {
                        nodes[tile] = new NodeRecord(tile);
                    }

                    connections.Add(new Connection(node, nodes[tile]));
                }
            }

            return connections;
        }
    }

    public class Connection
    {
        public readonly NodeRecord GetFromNode;
        public readonly NodeRecord GetToNode;
        public readonly float GetCost;

        public Connection(NodeRecord fromNode, NodeRecord toNode)
        {
            GetFromNode = fromNode;
            GetToNode = toNode;
            GetCost = fromNode.Cost + toNode.Cost;
        }
    }

    public class NodeRecord : IComparable<NodeRecord>
    {
        public Tile Node;
        public Connection Connection = null;
        public float CostSoFar = 0;
        public float EstimatedTotalCost = 0;
        public float Cost = 1;

        public bool visited = false;

        public NodeRecord(Tile node)
        {
            Node = node;
        }

        public int CompareTo(NodeRecord other)
        {
            if (EstimatedTotalCost < other.EstimatedTotalCost)
                return -1;
            else if (EstimatedTotalCost > other.EstimatedTotalCost)
                return 1;
            else
                return 0;
        }
    }

    private class Heuristic
    {
        //euclidean distance between a node and the end node is used as the cost value estimate.
        public float Estimate(Tile current, Tile target)
        {
            return Vector3.Distance(target.TransformPosition, current.TransformPosition);
        }
    }
}