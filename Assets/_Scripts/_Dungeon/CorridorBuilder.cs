using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CorridorBuilder : MonoBehaviour
{
    private DungeonGraph _graph;
    Dictionary<Vector3Int, CaseType> _usedPos = new();
    Dictionary<Vector3Int, GameObject> _goPerPos = new();
    private int minLevel = -2;
    private int maxLevel = 2;
    [SerializeField] private GameObject _corridorPrefab;
    [SerializeField] private GameObject _ladderPrefab;
    [SerializeField] private GameObject _intersectionPrefab;
    [SerializeField] private GameObject _firstInterPrefab;





    private void Awake()
    {
        _graph = gameObject.GetComponent<DungeonGraph>();
    }

    

    public void AddNode(DungeonGraphNode node)
    {
        Vector3Int startPos = new Vector3Int(node.Coordinate.x * _graph.RatioSize - _graph.RealSize / 2, 0, node.Coordinate.z * _graph.RatioSize - _graph.RealSize / 2);
        for (int i  = startPos.x; i <= startPos.x + _graph.RealSize; i++)
        {
            for (int j = startPos.z; j <= startPos.z + _graph.RealSize; j++)
            {
                _usedPos[new Vector3Int(i, 0, j)] = CaseType.Room;
            }
        }
    }

    internal bool CorridorSetup(DungeonGraphNode node, DungeonGraphNode neighbour)
    {
        DoorDirection mainNodeDir = node.NeighbourPerDirection[neighbour];
        Vector3Int doorA = node.Coordinate * _graph.RatioSize + ReturnRelativeDoorPos(mainNodeDir);
        Vector3Int doorB = neighbour.Coordinate * _graph.RatioSize + ReturnRelativeDoorPos(OppositeDirection(mainNodeDir));
        Vector3Int startPos = doorA + DirectionToStep(mainNodeDir);
        Vector3Int endPos = doorB + DirectionToStep(OppositeDirection(mainNodeDir));

        bool verif = false;
        int yLevel = node.Coordinate.y;
        int multLevel = 0;
        do
        {
            verif = CorridorVerification(startPos, endPos, yLevel);
            if (!verif)
            {
                multLevel += 1;
                if (multLevel % 2 == 1)
                {
                    yLevel += multLevel;
                }
                else
                {
                    yLevel -= multLevel;
                }
            }
        } while (!verif && yLevel >= minLevel && yLevel <= maxLevel);

        return verif;
    }

    private bool CorridorVerification(Vector3Int startPos, Vector3Int endPos, int yLevel) // verifie puis ajoute le corridor s'il est valide
    {
        bool notOnFloor = yLevel != 0;
        int signY = 1;
        if (yLevel < 0) { signY = -1;}

        int signX = 1;
        int signZ = 1;
        if (startPos.x > endPos.x) { signX = -1; }
        if (startPos.z > endPos.z) { signZ = -1; }

        bool CheckPath(bool verticalFirst)
        {
            int i = startPos.x;
            int j = startPos.z;

            if (verticalFirst)
            {
                for (; j != endPos.z; j += signZ)
                {
                    if (_usedPos.ContainsKey(new Vector3Int(i, yLevel * 5, j)))
                    {
                        return false;
                    }
                }

                for (; i != endPos.x; i += signX)
                {
                    if (_usedPos.ContainsKey(new Vector3Int(i, yLevel * 5, j)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (; i != endPos.x; i += signX)
                {
                    if (_usedPos.ContainsKey(new Vector3Int(i, yLevel * 5, j)))
                    {
                        return false;
                    }
                }

                for (; j != endPos.z; j += signZ)
                {
                    if (_usedPos.ContainsKey(new Vector3Int(i, yLevel * 5, j)))
                    {
                        return false;
                    }
                }
            }

            return !_usedPos.ContainsKey(new Vector3Int(endPos.x, yLevel * 5, endPos.z));
        }
       
        void AddPath(bool verticalFirst)
        {
            int i = startPos.x;
            int j = startPos.z;

            if (verticalFirst)
            {
                for (; j != endPos.z; j += signZ)
                {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), Quaternion.identity, transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }

                for (; i != endPos.x; i += signX)
                {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), Quaternion.identity, transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }
            }
            else
            {
                for (; i != endPos.x; i += signX)
                {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), Quaternion.identity, transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }

                for (; j != endPos.z; j += signZ)
                {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), Quaternion.identity, transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }
            }
            _usedPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = CaseType.Corridor;
            GameObject lastCorridorGO = Instantiate(_corridorPrefab, new Vector3(endPos.x, yLevel * 5, endPos.z), Quaternion.identity, transform);
            _goPerPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = lastCorridorGO;

            for (int k = 0; k != yLevel * 5; k += signY)
            {
                if (_usedPos.ContainsKey(new Vector3Int(startPos.x, k, startPos.z)) && k == 0)
                {
                    _usedPos[new Vector3Int(startPos.x, k, startPos.z)] = CaseType.Intersection;
                    Destroy(_goPerPos[new Vector3Int(startPos.x, k, startPos.z)]);
                    _goPerPos.Remove(new Vector3Int(startPos.x, k, startPos.z));
                    Instantiate(_intersectionPrefab, new Vector3(startPos.x, k, startPos.z), Quaternion.identity, transform);
                } else if (_usedPos.ContainsKey(new Vector3Int(startPos.x, k, startPos.z)))
                {
                    _usedPos[new Vector3Int(startPos.x, k, startPos.z)] = CaseType.FirstInter;
                    Destroy(_goPerPos[new Vector3Int(startPos.x, k, startPos.z)]);
                    _goPerPos.Remove(new Vector3Int(startPos.x, k, startPos.z));
                    Instantiate(_firstInterPrefab, new Vector3(startPos.x, k, startPos.z), Quaternion.identity, transform);
                } else
                {
                    _usedPos[new Vector3Int(startPos.x, k, startPos.z)] = CaseType.Ladder;
                    Instantiate(_ladderPrefab, new Vector3(startPos.x, k, startPos.z), Quaternion.identity, transform);
                }
                if (_usedPos.ContainsKey(new Vector3Int(endPos.x, k, endPos.z)) && k == 0)
                {
                    _usedPos[new Vector3Int(endPos.x, k, endPos.z)] = CaseType.Intersection;
                    Destroy(_goPerPos[new Vector3Int(endPos.x, k, endPos.z)]);
                    _goPerPos.Remove(new Vector3Int(endPos.x, k, endPos.z));
                    Instantiate(_intersectionPrefab, new Vector3(endPos.x, k, endPos.z), Quaternion.identity, transform);
                } else if (_usedPos.ContainsKey(new Vector3Int(endPos.x, k, endPos.z)))
                {
                    _usedPos[new Vector3Int(endPos.x, k, endPos.z)] = CaseType.FirstInter;
                    Destroy(_goPerPos[new Vector3Int(endPos.x, k, endPos.z)]);
                    _goPerPos.Remove(new Vector3Int(endPos.x, k, endPos.z));
                    Instantiate(_firstInterPrefab, new Vector3(endPos.x, k, endPos.z), Quaternion.identity, transform);
                } else
                {
                    _usedPos[new Vector3Int(endPos.x, k, endPos.z)] = CaseType.Ladder;
                    Instantiate(_ladderPrefab, new Vector3(endPos.x, k, endPos.z), Quaternion.identity, transform);
                }
                
            }
        }

        bool firstChoice = UnityEngine.Random.Range(0, 2) == 0;

        if (CheckPath(firstChoice))
        {
            AddPath(firstChoice);
            return true;
        }
        if (CheckPath(!firstChoice))
        {
            AddPath(!firstChoice);
            return true;
        }
        return false;
    }
    
    private Vector3Int ReturnRelativeDoorPos(DoorDirection dir)
    {
        int value = _graph.RealSize / 2;
        return dir switch
        {
            DoorDirection.North => new Vector3Int(0, 0, value),
            DoorDirection.South => new Vector3Int(0, 0, -value),
            DoorDirection.East => new Vector3Int(value, 0, 0),
            DoorDirection.West => new Vector3Int(-value, 0, 0),
            _ => Vector3Int.zero
        };
    } 

    private Vector3Int DirectionToStep(DoorDirection dir)
    {
        return dir switch
        {
            DoorDirection.North => new Vector3Int(0, 0, 1),
            DoorDirection.South => new Vector3Int(0, 0, -1),
            DoorDirection.East => new Vector3Int(1, 0, 0),
            DoorDirection.West => new Vector3Int(-1, 0, 0),
            _ => Vector3Int.zero
        };
    }

    private DoorDirection OppositeDirection(DoorDirection direction)
    {
        return direction switch
        {
            DoorDirection.North => DoorDirection.South,
            DoorDirection.East => DoorDirection.West,
            DoorDirection.South => DoorDirection.North,
            DoorDirection.West => DoorDirection.East,
            _ => DoorDirection.None
        };
    }
    

    public void DebugUsedPos()
    {
        if (_usedPos.Count == 0)
        {
            Debug.Log("DebugUsedPos: aucune position enregistrée.");
            return;
        }

        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (var pos in _usedPos.Keys)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.z < minZ) minZ = pos.z;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine($"DebugUsedPos [{_usedPos.Count} cases] X:[{minX},{maxX}] Z:[{minZ},{maxZ}]");

        for (int z = maxZ; z >= minZ; z--)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var key = new Vector3Int(x, 0, z);
                if (_usedPos.TryGetValue(key, out CaseType type))
                {
                    sb.Append(type switch
                    {
                        CaseType.Room => "R",
                        CaseType.Corridor => "C",
                        _ => "?"
                    });
                }
                else
                {
                    sb.Append("_");
                }
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
