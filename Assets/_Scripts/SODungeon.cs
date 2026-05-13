using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSODungeon", menuName = "DungeonGeneration/SODungeon")]
public class SODungeon : ScriptableObject
{
    [SerializeField] private int _seed = 0;
    [SerializeField] private bool _isRandomSeed = false;
    [SerializeField] private RoomTypePool[] _roomPools;



    public int Seed => _seed;
    public bool IsRandomSeed => _isRandomSeed;
    public RoomTypePool[] RoomPools => _roomPools;



    public List<SORoom> GetRoomsOfType(RoomType type)
    {
        foreach (var pool in _roomPools)
        {
            if (pool.Type == type)
            {
                return pool.Rooms;
            } 
        } 
        return new List<SORoom>();
    }

    public List<RoomType> GetAllType()
    {
        List<RoomType> roomTypes = new List<RoomType>();
        foreach (var pool in _roomPools)
        {
            roomTypes.Add(pool.Type);
        }
        return roomTypes;
    }
}

[Serializable]
public class RoomTypePool
{
    [SerializeField] private RoomType _type;
    [SerializeField] private List<SORoom> _rooms = new List<SORoom>();

    public RoomType Type => _type;
    public List<SORoom> Rooms => _rooms;
}
