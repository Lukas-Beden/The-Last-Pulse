using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "DungeonGeneration/RoomTemplate")]
public class RoomTemplate : ScriptableObject
{
    [SerializeField] private GameObject _roomPrefabs;
    [SerializeField] private Vector2 _roomSize;

    public GameObject RoomPrefab => _roomPrefabs;
    public Vector2 RoomSize => _roomSize;
}
