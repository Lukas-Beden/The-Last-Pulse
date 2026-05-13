using UnityEngine;

[CreateAssetMenu(fileName = "NewSORoom", menuName = "DungeonGeneration/SORoom")]
public class SORoom : ScriptableObject
{
    [SerializeField] private GameObject _roomPrefabs;
    [SerializeField] private Vector2 _roomSize;

    public GameObject RoomPrefab => _roomPrefabs;
    public Vector2 RoomSize => _roomSize;
}
