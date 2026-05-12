using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGraph : MonoBehaviour
{
    [SerializeField] private List<SODungeon> _dungeonTemplate;
    private SODungeon _actualDungeonTemplate;
    private int _floor = 0; // dans le gameManager ???
    private List<DungeonGraphNode> _nodes = new();
    private int _minOfEachRoomType = 3;
    private int _maxOfEachRoomType = 4;
    private List<RoomType> _possibleRoomType = new();
    [SerializeField] private SerializableDictionary<RoomType, List<RoomType>> _roomConstraints = new();
    [SerializeField] private SerializableDictionary<RoomType, Vector2> _maxLinkByRoomType = new();
    private Dictionary<RoomType, List<DungeonGraphNode>> _roomByType = new();
    private DungeonGraphNode _startNode = null;

    public List<DungeonGraphNode> Nodes => _nodes;
    public SODungeon ActualDungeonTemplate => _actualDungeonTemplate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _actualDungeonTemplate = _dungeonTemplate[_floor / 10];
        GraphCreationLoop();
    }

    private void ChangeFloor()
    {
        _floor += 1;

        //transi vers prochain lvl

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        GraphCreationLoop();
    }

    private void GraphCreationLoop()
    {
        List<DungeonGraphNode> _dfsVisitedNode = new();
        _possibleRoomType = _actualDungeonTemplate.GetAllType();
        do
        {
            _nodes.Clear();
            _dfsVisitedNode.Clear();
            _roomByType.Clear();
            CreateDungeon();

            Dictionary<int, List<DungeonGraphNode>> nodeByDistance = new();
            nodeByDistance = GetFarthestNode(_startNode);

            AddEndNode(nodeByDistance);

            //DebugGraph();
            DebugDistance();

        } while (!DFS(_startNode, _dfsVisitedNode));
    }

    private void AddEndNode(Dictionary<int, List<DungeonGraphNode>> nodeByDistance)
    {
        List<DungeonGraphNode> farestNodes = nodeByDistance[nodeByDistance.Keys.Max()];

        List<DungeonGraphNode> usableNodes = new();

        foreach (DungeonGraphNode node in farestNodes)
        {
            if (_roomConstraints[RoomType.End].Contains(node.RoomType))
            {
                usableNodes.Add(node);
            }
        }

        if (usableNodes.Count < 2)
        {
            return;
        }

        DungeonGraphNode newNode = new DungeonGraphNode(RoomType.End);
        _nodes.Add(newNode);

        for (int i = 0; i < 2; i++)
        {
            DungeonGraphNode node = usableNodes[UnityEngine.Random.Range(0, usableNodes.Count)];
            node.AddNeighbour(newNode);
            newNode.AddNeighbour(node);
            usableNodes.Remove(node);
        }
    }

    private void CreateDungeon()
    {
        DungeonGraphNode newNode = new DungeonGraphNode(RoomType.Start);
        List<DungeonGraphNode> newList = new List<DungeonGraphNode>();

        _startNode = newNode;
        _nodes.Add(newNode);
        newList.Add(newNode);
        _roomByType[RoomType.Start] = newList;

        newList.Clear();

        CreateBaseNode();

        foreach (DungeonGraphNode node in _nodes)
        {
            AddNeighbour(node);
        }
    }

    private void CreateBaseNode()
    {
        foreach (RoomType roomType in _possibleRoomType)
        {
            List<DungeonGraphNode> newList = new List<DungeonGraphNode>();
            for (int i = 0; i < UnityEngine.Random.Range(_minOfEachRoomType, _maxOfEachRoomType + 1); i++)
            {
                DungeonGraphNode newNode = new DungeonGraphNode(roomType);
                _nodes.Add(newNode);
                newList.Add(newNode);
            }
            _roomByType[roomType] = newList;
        }
    }

    private void AddNeighbour(DungeonGraphNode node)
    {
        RoomType roomType = node.RoomType;

        List<RoomType> usableRoomType = new();

        foreach (RoomType nextRoomType in _roomConstraints[roomType])
        {
            if (_roomByType[nextRoomType].Count > 0)
            {
                usableRoomType.Add(nextRoomType);
            }
        }

        if (usableRoomType.Count <= 0)
        {
            return;
        }

        RoomType newTypeLink = usableRoomType[UnityEngine.Random.Range(0, usableRoomType.Count)];

        DungeonGraphNode newNodeLink = _roomByType[newTypeLink][UnityEngine.Random.Range(0, _roomByType[newTypeLink].Count)];

        if (IsNodeNotInNeighbour(node, newNodeLink))
        {
            node.AddNeighbour(newNodeLink);
            newNodeLink.AddNeighbour(node);
        }

        if (node.Neighbour.Count >= _maxLinkByRoomType[roomType].y)
        {
            _roomByType[roomType].Remove(node);
        }

        if (newNodeLink.Neighbour.Count >= _maxLinkByRoomType[newTypeLink].y)
        {
            _roomByType[newTypeLink].Remove(newNodeLink);
        }

        if (node.Neighbour.Count < _maxLinkByRoomType[roomType].x)
        {
            AddNeighbour(node);
        }
    }

    private bool IsNodeNotInNeighbour(DungeonGraphNode mainNode, DungeonGraphNode potentialNeighbour)
    {
        List<DungeonGraphNode> neighbour = mainNode.Neighbour;

        foreach (DungeonGraphNode neighbourNode in neighbour)
        {
            if (neighbourNode == potentialNeighbour)
            {
                return false;
            }
        }
        return true;
    }

    private bool DFS(DungeonGraphNode node, List<DungeonGraphNode> visitedNode)
    {
        visitedNode.Add(node);

        if (node.RoomType == RoomType.End)
        {
            return true;
        }

        if (node.Neighbour.Count <= 0)
        {
            return false;
        }

        foreach (DungeonGraphNode neighbour in node.Neighbour)
        {
            if (!visitedNode.Contains(neighbour))
            {
                if (DFS(neighbour, visitedNode))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Dictionary<int, List<DungeonGraphNode>> GetFarthestNode(DungeonGraphNode startNode)
    {
        Queue<DungeonGraphNode> queue = new();
        Dictionary<int, List<DungeonGraphNode>> nodeByDistance = new();
        Dictionary<DungeonGraphNode, int> distance = new();
        DungeonGraphNode actualNode;

        queue.Enqueue(startNode);
        distance[startNode] = 0;
        nodeByDistance[0] = new();
        nodeByDistance[0].Add(startNode);

        while (queue.Count > 0)
        {
            actualNode = queue.Dequeue();

            foreach (DungeonGraphNode neighbour in actualNode.Neighbour)
            {
                if (!distance.ContainsKey(neighbour))
                {
                    distance[neighbour] = distance[actualNode] + 1;
                    queue.Enqueue(neighbour);

                    if (!nodeByDistance.ContainsKey(distance[actualNode] + 1))
                    {
                        nodeByDistance[distance[actualNode] + 1] = new();
                    }
                    nodeByDistance[distance[actualNode] + 1].Add(neighbour);
                }
            }
        }
        return nodeByDistance;
    }









    private void DebugGraph()
    {
        foreach (DungeonGraphNode node in _nodes)
        {
            Debug.Log(node.RoomType.ToString());
            Debug.Log("****");
            foreach (DungeonGraphNode neighbour in node.Neighbour)
            {
                Debug.Log(neighbour.RoomType.ToString());
            }
            Debug.Log("\n------------------------------------\n");
        }
    }

    private void DebugDistance()
    {
        Dictionary<int, List<DungeonGraphNode>> nodeByDistance = GetFarthestNode(_startNode);
        foreach (int key in nodeByDistance.Keys)
        {
            foreach (DungeonGraphNode item in nodeByDistance[key])
            {
                Debug.Log(key + ", " + item.RoomType);
            }
            Debug.Log("\n");
        }
    }
}
