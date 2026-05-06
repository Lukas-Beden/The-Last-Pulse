using System.Collections.Generic;

public class DungeonGraphNode
{
    private EnumRoomType _roomType;
    private List<DungeonGraphNode> _neighbour;

    public EnumRoomType RoomType => _roomType;
    public List<DungeonGraphNode> Neighbour => _neighbour;

    public DungeonGraphNode(EnumRoomType roomType)
    {
        _roomType = roomType;
        _neighbour = new List<DungeonGraphNode>();
    }

    public void AddNeighbour(DungeonGraphNode newNeighbour) { _neighbour.Add(newNeighbour); }
}
