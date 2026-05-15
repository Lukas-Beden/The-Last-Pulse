using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DoorsManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _openDoors;
    [SerializeField] private GameObject[] _closedDoors;
    private Dictionary<DoorDirection, (GameObject, GameObject)> _doorsPerDirection = new();

    public GameObject[] OpenDoors => _openDoors;
    public GameObject[] ClosedDoors => _closedDoors;
    public Dictionary<DoorDirection, (GameObject, GameObject)> DoorsPerDirection => _doorsPerDirection;

    private void Awake()
    {
        _doorsPerDirection[DoorDirection.North] = (_openDoors[0], _closedDoors[0]);
        _doorsPerDirection[DoorDirection.East] = (_openDoors[1], _closedDoors[1]);
        _doorsPerDirection[DoorDirection.South] = (_openDoors[2], _closedDoors[2]);
        _doorsPerDirection[DoorDirection.West] = (_openDoors[3], _closedDoors[3]);
    }

    public void SetOpen(DoorDirection direction)
    {
        _doorsPerDirection[direction].Item2.SetActive(false);
        _doorsPerDirection[direction].Item1.SetActive(true);
    }
    public void SetClose(DoorDirection direction)
    {
        _doorsPerDirection[direction].Item1.SetActive(false);
        _doorsPerDirection[direction].Item2.SetActive(true);
    }
}