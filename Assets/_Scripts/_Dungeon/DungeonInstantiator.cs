using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonInstantiator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private DungeonGraph _graph;
    private SODungeon _actualDungeonTemplate;
    private List<DungeonGraphNode> _graphNodes = new();
    private DungeonGraphNode[,] _nodeGrid;
    private int _nodeCount = 0;
    private int _ratioSize;
    //private Dictionary<int, List<DungeonGraphNode>> _finishedNodes = new();



    private CorridorBuilder _corridorBuilder;

    void Awake()
    {
        _corridorBuilder = GetComponent<CorridorBuilder>();
        _graph = gameObject.GetComponent<DungeonGraph>();
        _ratioSize = _graph.RatioSize;
    }

    private void SetupDoors(DoorsManager doorsManager, DoorDirection direction)
    {
        doorsManager.SetOpen(direction);
    }
    
    public void SetupInstantation()
    {
        _actualDungeonTemplate = _graph.ActualDungeonTemplate;
        _graphNodes = _graph.Nodes;
        _nodeCount = _graphNodes.Count;
        bool isFinished = false;
        int iteration = 0;
        do
        {
            ClearAllNodes();
            ClearTransform();
            _corridorBuilder.ClearCorridor();
            _nodeGrid = new DungeonGraphNode[_nodeCount, _nodeCount];
            isFinished = CreateGrid(_graphNodes[0]);
            iteration += 1;
        } while (!isFinished && iteration < 50);
        InstantiateRoom();
        foreach (DungeonGraphNode node in _graphNodes)
        {
            _corridorBuilder.AddNode(node);
        }
        LESCOULOIRS();
        //_corridorBuilder.DebugUsedPos();
    }

    private void ClearTransform()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void LESCOULOIRS() //verif ne fonctionne pas V1
    {
        foreach (DungeonGraphNode node in _graphNodes)
        {
            foreach (var kvp in node.NeighbourPerDirection)
            {
                if (!node.IsConnectionFinished(kvp.Key))
                {
                    node.AddFinishedCorridors(kvp.Key);
                    kvp.Key.AddFinishedCorridors(node);
                    if (!_corridorBuilder.CorridorSetup(node, kvp.Key))
                    {
                        _graph.GraphCreationLoop();
                        Debug.Log("resteartree");
                    }
                }
            }
        }
    }

    private void InstantiateRoom()
    {
        foreach (DungeonGraphNode node in _graphNodes)
        {
            SORoom newRoom = GetRandomRoom(node);
            GameObject roomGO = Instantiate(newRoom.RoomPrefab, new Vector3(node.Coordinate.x * _ratioSize, 0, node.Coordinate.z * _ratioSize), Quaternion.identity, transform);
            DoorsManager doorsManager = roomGO.GetComponent<DoorsManager>();
            foreach (var kvp in doorsManager.DoorsPerDirection)
            {
                doorsManager.SetClose(kvp.Key);
            }
            node.SetRoom(roomGO);
            foreach (var kvp in node.NeighbourPerDirection)
            {
                SetupDoors(doorsManager, kvp.Value);
            }
        }
    }

    private SORoom GetRandomRoom(DungeonGraphNode node)
    {
        List<SORoom> roomOfType = _actualDungeonTemplate.GetRoomsOfType(node.RoomType);
        SORoom rooom = roomOfType[UnityEngine.Random.Range(0, roomOfType.Count)];
        return rooom;
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
        startNode.ChangeCoordinate(new Vector3Int(startPos.x, 0, startPos.y));
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

                if (newPos.x < 0 || newPos.x >= _nodeCount ||
                    newPos.y < 0 || newPos.y >= _nodeCount)
                    return false;

                _nodeGrid[newPos.x, newPos.y] = neighbour;
                neighbour.ChangeCoordinate(new Vector3Int(newPos.x, 0, newPos.y));
                posByNode[neighbour] = newPos;
                usedPos.Add(newPos);

                actualNode.AddNeighbourPerDirection(neighbour, result.Item2);
                neighbour.AddNeighbourPerDirection(actualNode, OppositeDirection(result.Item2));
            }
        }

        foreach (DungeonGraphNode node in _graphNodes)
        {
            foreach (DungeonGraphNode neighbour in node.Neighbour)
            {
                if (node.NeighbourPerDirection.ContainsKey(neighbour)) continue;

                Vector3Int delta = neighbour.Coordinate - node.Coordinate;

                DoorDirection dir = GetDominantDirection(delta);
                if (dir == DoorDirection.None) return false;

                DoorDirection opposite = OppositeDirection(dir);

                if (!node.NeighbourPerDirection.ContainsValue(dir) &&
                    !neighbour.NeighbourPerDirection.ContainsValue(opposite))
                {
                    node.AddNeighbourPerDirection(neighbour, dir);
                    neighbour.AddNeighbourPerDirection(node, opposite);
                }
                else
                {
                    DoorDirection altDir = FindFreeDirectionPair(node, neighbour);
                    if (altDir == DoorDirection.None) return false;

                    node.AddNeighbourPerDirection(neighbour, altDir);
                    neighbour.AddNeighbourPerDirection(node, OppositeDirection(altDir));
                }
            }
        }

        return true;
    }

    private DoorDirection GetDominantDirection(Vector3Int delta)
    {
        if (delta == Vector3Int.zero) return DoorDirection.None;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.z))
            return delta.x > 0 ? DoorDirection.East : DoorDirection.West;
        else
            return delta.z > 0 ? DoorDirection.North : DoorDirection.South;
    }

    private DoorDirection FindFreeDirectionPair(DungeonGraphNode a, DungeonGraphNode b)
    {
        DoorDirection[] dirs = { DoorDirection.North, DoorDirection.East, DoorDirection.South, DoorDirection.West };

        foreach (DoorDirection dir in dirs)
        {
            DoorDirection opp = OppositeDirection(dir);
            if (!a.NeighbourPerDirection.ContainsValue(dir) &&
                !b.NeighbourPerDirection.ContainsValue(opp))
                return dir;
        }
        return DoorDirection.None;
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
        return direction switch
        {
            DoorDirection.North => DoorDirection.South,
            DoorDirection.East => DoorDirection.West,
            DoorDirection.South => DoorDirection.North,
            DoorDirection.West => DoorDirection.East,
            _ => DoorDirection.None
        };
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
