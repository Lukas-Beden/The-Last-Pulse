using System.Collections.Generic;
using UnityEngine;

public class DungeonGraphNode
{
    private RoomType _roomType;
    private List<DungeonGraphNode> _neighbour;
    private GameObject _room;
    private Dictionary<DungeonGraphNode, DoorDirection> _neighbourPerDirection;
    private Vector3Int _coordinate;

    public RoomType RoomType => _roomType;
    public List<DungeonGraphNode> Neighbour => _neighbour;
    public GameObject Room => _room;
    public Dictionary<DungeonGraphNode, DoorDirection> NeighbourPerDirection => _neighbourPerDirection;
    public Vector3Int Coordinate => _coordinate;

    public DungeonGraphNode(RoomType roomType)
    {
        _roomType = roomType;
        _neighbour = new();
        _neighbourPerDirection = new();
    }

    public void AddNeighbour(DungeonGraphNode newNeighbour) { _neighbour.Add(newNeighbour); }
    public void SetRoom(GameObject room) { _room = room; }
    public void AddNeighbourPerDirection(DungeonGraphNode neighbour, DoorDirection doorDirection) { _neighbourPerDirection[neighbour] = doorDirection; }
    public void ClearNeighbourPerDirection() { _neighbourPerDirection.Clear(); }
    public void ChangeCoordinate(Vector3Int newCoords) { _coordinate = newCoords; }
}
