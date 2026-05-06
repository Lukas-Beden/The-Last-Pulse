using System.Collections.Generic;
using UnityEngine;

public class DungeonGraph : MonoBehaviour
{

    private List<DungeonGraphNode> _nodes = new();
    private int _minOfEachRoomType = 3;
    private int _maxOfEachRoomType = 4;
    [SerializeField] private List<EnumRoomType> _possibleRoomType = new();
    [SerializeField] private Dictionary<EnumRoomType, List<EnumRoomType>> _roomConstraints = new(); // a switch en SerializableDictionnary
    [SerializeField] private Dictionary<EnumRoomType, int> _maxLinkByRoomType = new(); // a switch en SerializableDictionnary
    private Dictionary<EnumRoomType, List<DungeonGraphNode>> _roomByType = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DungeonGraphNode newNode = new DungeonGraphNode(EnumRoomType.Start);
        List<DungeonGraphNode> newList = new List<DungeonGraphNode>();

        _nodes.Add(newNode);
        newList.Add(newNode);
        _roomByType[EnumRoomType.Start] = newList;

        newList.Clear();

        newNode = new DungeonGraphNode(EnumRoomType.End); // a la fin de la liste pour la lisibilité ?
        _nodes.Add(newNode);
        newList.Add(newNode);
        _roomByType[EnumRoomType.End] = newList;

        CreateBaseNode();

        foreach (DungeonGraphNode node in _nodes)
        {
            AddNeighbour(node);
        }
    }

    private void CreateBaseNode()
    {
        foreach (EnumRoomType roomType in _possibleRoomType)
        {
            List<DungeonGraphNode> newList = new List<DungeonGraphNode>();
            for (int i = 0; i < Random.Range(_minOfEachRoomType, _maxOfEachRoomType + 1); i++)
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
        EnumRoomType roomType = node.RoomType;

        EnumRoomType newTypeLink = _roomConstraints[roomType][Random.Range(0, _roomConstraints[roomType].Count)];

        DungeonGraphNode newNodeLink = _roomByType[newTypeLink][Random.Range(0, _roomByType[newTypeLink].Count)];

        node.AddNeighbour(newNodeLink);
        newNodeLink.AddNeighbour(node);

        if (node.Neighbour.Count >= _maxLinkByRoomType[roomType])
        {
            _roomByType[roomType].Remove(node);
        }

        if (newNodeLink.Neighbour.Count >= _maxLinkByRoomType[newTypeLink])
        {
            _roomByType[newTypeLink].Remove(newNodeLink);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
