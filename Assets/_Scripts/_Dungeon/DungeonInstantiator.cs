using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonInstantiator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private DungeonGraph _graph;
    private SODungeon _actualDungeonTemplate;
    private List<DungeonGraphNode> _graphNodes = new();
    private DungeonGraphNode[,] _nodeGrid;
    private int _nodeCount = 0;


    
    void Start()
    {
        _graph = gameObject.GetComponent<DungeonGraph>();
        _actualDungeonTemplate = _graph.ActualDungeonTemplate;
        _graphNodes = _graph.Nodes;
        _nodeCount = _graphNodes.Count;
        bool isFinished = false;
        int iteration = 0;
        do
        {
            ClearAllNodes();
            _nodeGrid = new DungeonGraphNode[_nodeCount, _nodeCount];
            isFinished = CreateGrid(_graphNodes[0]);
            iteration += 1;
        } while (!isFinished && iteration < 50);
        
        //InstantiateRoom();
    }

    private void InstantiateRoom()
    {
        throw new NotImplementedException();
    }

    private bool CreateGrid(DungeonGraphNode startNode)
    {
        Queue<DungeonGraphNode> queue = new();
        HashSet<DungeonGraphNode> visitedNodes = new();
        List<Vector2Int> usedPos = new();
        Dictionary<DungeonGraphNode, Vector2Int> posByNode = new();

        Vector2Int startPos = new Vector2Int(_nodeCount / 2, _nodeCount / 2);

        queue.Enqueue(startNode);
        visitedNodes.Add(startNode);

        _nodeGrid[startPos.x, startPos.y] = startNode;
        posByNode[startNode] = startPos;
        usedPos.Add(startPos);

        while (queue.Count > 0)
        {
            DungeonGraphNode actualNode = queue.Dequeue();
            Vector2Int parentPos = posByNode[actualNode];

            foreach (DungeonGraphNode neighbour in actualNode.Neighbour)
            {
                if (visitedNodes.Contains(neighbour)) continue;

                visitedNodes.Add(neighbour);
                queue.Enqueue(neighbour);

                (Vector2Int, DoorDirection) result = FindEmptyCell(parentPos, usedPos);
                Vector2Int newPos = result.Item1;

                if (newPos.x >= 0 && newPos.x < _nodeCount &&
                    newPos.y >= 0 && newPos.y < _nodeCount)
                {
                    _nodeGrid[newPos.x, newPos.y] = neighbour;
                    posByNode[neighbour] = newPos;
                    usedPos.Add(newPos);

                    actualNode.AddNeighbourPerDirection(neighbour, result.Item2);
                    neighbour.AddNeighbourPerDirection(actualNode, OppositeDirection(result.Item2));
                } else
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void ClearAllNodes()
    {
        foreach (DungeonGraphNode node in _graphNodes)
        {
            node.ClearNeighbourPerDirection();
        }
    }

    private DoorDirection OppositeDirection(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North:
                return DoorDirection.South;
            case DoorDirection.East:
                return DoorDirection.West;
            case DoorDirection.South:
                return DoorDirection.North;
            case DoorDirection.West:
                return DoorDirection.East;
        }
        return DoorDirection.North;
    }

    private (Vector2Int, DoorDirection) FindEmptyCell(Vector2Int origin, List<Vector2Int> usedPos)
    {
        Dictionary<DoorDirection, Vector2Int> directions = new()
        {
            { DoorDirection.North, Vector2Int.up },
            { DoorDirection.East, Vector2Int.right },
            { DoorDirection.South, Vector2Int.down },
            { DoorDirection.West, Vector2Int.left }
        };

        List<DoorDirection> possibleDirection = new()
        {
            DoorDirection.North,
            DoorDirection.East,
            DoorDirection.South,
            DoorDirection.West
        };

        bool isEmpty = false;
        Vector2Int vectorDir = Vector2Int.zero;
        DoorDirection newDir = DoorDirection.North;

        while (!isEmpty && possibleDirection.Count > 0)
        {
            newDir = possibleDirection[UnityEngine.Random.Range(0, possibleDirection.Count)];
            vectorDir = directions[newDir] + origin;
            if (!usedPos.Contains(vectorDir))
            {
                isEmpty = true;
            } else
            {
                possibleDirection.Remove(newDir);
                vectorDir = new(-1, -1);
            }
        }
        return (vectorDir, newDir);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
