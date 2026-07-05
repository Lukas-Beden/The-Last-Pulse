using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGraph : MonoBehaviour
{
    [SerializeField] private List<SODungeon> _dungeonTemplate;
    private SODungeon _actualDungeonTemplate;
    private int _floor = 0; // dans le gameManager ???
    private List<DungeonGraphNode> _nodes = new();
    [SerializeField] private int _minOfEachRoomType = 3;
    [SerializeField] private int _maxOfEachRoomType = 4;
    private List<RoomType> _possibleRoomType = new();
    [SerializeField] private SerializableDictionary<RoomType, List<RoomType>> _roomConstraints = new();
    [SerializeField] private SerializableDictionary<RoomType, Vector2> _maxLinkByRoomType = new();
    private Dictionary<RoomType, List<DungeonGraphNode>> _roomByType = new();
    private DungeonGraphNode _startNode = null;
    private DungeonInstantiator _dungeonInstantiator;
    private int _ratioSize = 20;
    private int _realSize = 12;

    public List<DungeonGraphNode> Nodes => _nodes;
    public SODungeon ActualDungeonTemplate => _actualDungeonTemplate;
    public int RatioSize => _ratioSize;
    public int RealSize => _realSize;

    private void Awake()
    {
        _dungeonInstantiator = GetComponent<DungeonInstantiator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("DungeonGraph Start called"); // doit apparaître
        Debug.Log("Template count: " + _dungeonTemplate.Count);
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
        _dungeonInstantiator.SetupInstantation();
    }

    public void GraphCreationLoop()
    {
        if (!_actualDungeonTemplate.IsRandomSeed)
        {
            
        }


        List<DungeonGraphNode> _dfsVisitedNode = new();
        _possibleRoomType = _actualDungeonTemplate.GetAllType();
        _possibleRoomType.Remove(RoomType.Start);
        _possibleRoomType.Remove(RoomType.End);
        do
        {
            _nodes.Clear();
            _dfsVisitedNode.Clear();
            _roomByType.Clear();
            CreateDungeon();

            Dictionary<int, List<DungeonGraphNode>> nodeByDistance = new();
            nodeByDistance = GetFarthestNode(_startNode);

            AddEndNode(nodeByDistance);

            
            //DebugDistance();

        } while (!DFS(_startNode, _dfsVisitedNode));
        _dungeonInstantiator.SetupInstantation();
        DebugGraph();
    }

    private void AddEndNode(Dictionary<int, List<DungeonGraphNode>> nodeByDistance)
    {
        int distance = nodeByDistance.Keys.Max();
        List<DungeonGraphNode> farestNodes = nodeByDistance[distance];
        List<DungeonGraphNode> usableNodes = new();
        
        do
        {
            if (distance < 0)
            {
                Debug.LogError("AddEndNode: aucun nœud valide trouvé pour End, on recrée le graphe.");
                return; // la boucle do...while dans GraphCreationLoop va réessayer
            }
            usableNodes.Clear();
            foreach (DungeonGraphNode node in farestNodes)
            {
                if (_roomConstraints[RoomType.End].Contains(node.RoomType) && node.Neighbour.Count < _maxLinkByRoomType[node.RoomType].y)
                {
                    usableNodes.Add(node);
                }
            }
            distance -= 1;
            if (distance < 0 || !nodeByDistance.ContainsKey(distance))
            {
                Debug.LogError("AddEndNode: impossible de placer le End node.");
                return;
            }
            farestNodes = nodeByDistance[distance];
        } while (usableNodes.Count < 2);

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
        Debug.Log("CreateDungeon start");
        DungeonGraphNode newNode = new DungeonGraphNode(RoomType.Start);
        List<DungeonGraphNode> newList = new List<DungeonGraphNode>();

        _startNode = newNode;
        _nodes.Add(newNode);
        newList.Add(newNode);
        _roomByType[RoomType.Start] = newList;

        newList.Clear();

        CreateBaseNode();
        Debug.Log("CreateBaseNode done, node count: " + _nodes.Count);
        foreach (DungeonGraphNode node in _nodes)
        {
            Debug.Log("AddNeighbour for: " + node.RoomType);
            AddNeighbour(node);
        }
        Debug.Log("CreateDungeon end");
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
        if (!_roomConstraints.ContainsKey(node.RoomType))
        {
            Debug.LogError("MISSING KEY in _roomConstraints: " + node.RoomType);
            return;
        }
        if (!_maxLinkByRoomType.ContainsKey(node.RoomType))
        {
            Debug.LogError("MISSING KEY in _maxLinkByRoomType: " + node.RoomType);
            return;
        }

        RoomType roomType = node.RoomType;

        // Candidats valides : bon type, pas déjà voisin, pas soi-même, pas au max
        List<DungeonGraphNode> candidates = new();
        foreach (RoomType nextRoomType in _roomConstraints[roomType])
        {
            if (!_roomByType.ContainsKey(nextRoomType)) continue;
            foreach (DungeonGraphNode candidate in _roomByType[nextRoomType])
            {
                if (candidate == node) continue;
                if (!IsNodeNotInNeighbour(node, candidate)) continue;
                if (candidate.Neighbour.Count >= _maxLinkByRoomType[nextRoomType].y) continue;
                candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0) return;
        if (node.Neighbour.Count >= _maxLinkByRoomType[roomType].y) return;

        DungeonGraphNode newNodeLink = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        RoomType newTypeLink = newNodeLink.RoomType;

        node.AddNeighbour(newNodeLink);
        newNodeLink.AddNeighbour(node);

        // Retirer des disponibles si max atteint
        if (node.Neighbour.Count >= _maxLinkByRoomType[roomType].y)
            _roomByType[roomType].Remove(node);

        if (newNodeLink.Neighbour.Count >= _maxLinkByRoomType[newTypeLink].y)
            _roomByType[newTypeLink].Remove(newNodeLink);

        // Rappel récursif si min non atteint
        if (node.Neighbour.Count < _maxLinkByRoomType[roomType].x)
            AddNeighbour(node);
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
        // Assigner un ID unique à chaque node pour ce debug
        Dictionary<DungeonGraphNode, int> nodeIds = new();
        for (int i = 0; i < _nodes.Count; i++)
        {
            nodeIds[_nodes[i]] = i;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine("========== GRAPH DEBUG ==========");
        foreach (DungeonGraphNode node in _nodes)
        {
            int id = nodeIds[node];
            sb.AppendLine($"[{id}] {node.RoomType}");
            if (node.Neighbour.Count == 0)
            {
                sb.AppendLine("    └─ (aucun voisin)");
            }
            else
            {
                foreach (DungeonGraphNode neighbour in node.Neighbour)
                {
                    sb.AppendLine($"    └─ [{nodeIds[neighbour]}] {neighbour.RoomType}");
                }
            }
        }
        sb.AppendLine("=================================");
        Debug.Log(sb.ToString());
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
