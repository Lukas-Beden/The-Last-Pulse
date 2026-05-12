using UnityEngine;

public class DoorData : MonoBehaviour
{
    [SerializeField] private DoorDirection _direction;

    public DoorDirection Direction => _direction;
}