using UnityEngine;

public class DoorsManager : MonoBehaviour
{
    [SerializeField] private DoorData[] _doors;

    public DoorData[] Doors => _doors;
}