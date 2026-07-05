using System;
using System.Collections.Generic;
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
    [SerializeField] private GameObject _intersectionLvl0Prefab;
    [SerializeField] private GameObject _cornerPrefab;

    // Offsets réglables dans l'inspecteur si tes meshes ne sont pas modélisés
    // avec la même convention que les enums (North = +Z, East = +X, etc).
    // Ajuste par pas de 90 en Play Mode jusqu'à ce que ça matche visuellement.
    [SerializeField] private float _corridorRotationOffset = 0f;
    [SerializeField] private float _cornerRotationOffset = 0f;
    [SerializeField] private float _ladderRotationOffset = 0f;
    [SerializeField] private float _intersectionRotationOffset = 0f;

    private void Awake()
    {
        _graph = gameObject.GetComponent<DungeonGraph>();
    }

    public void ClearCorridor()
    {
        _usedPos.Clear();
        _goPerPos.Clear();
    }

    public void AddNode(DungeonGraphNode node)
    {
        Vector3Int startPos = new Vector3Int(node.Coordinate.x * _graph.RatioSize - _graph.RealSize / 2, 0, node.Coordinate.z * _graph.RatioSize - _graph.RealSize / 2);
        for (int i = startPos.x; i <= startPos.x + _graph.RealSize; i++)
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
        DoorDirection oppositeNodeDir = OppositeDirection(mainNodeDir);

        Vector3Int doorA = node.Coordinate * _graph.RatioSize + ReturnRelativeDoorPos(mainNodeDir);
        Vector3Int doorB = neighbour.Coordinate * _graph.RatioSize + ReturnRelativeDoorPos(oppositeNodeDir);
        Vector3Int startPos = doorA + DirectionToStep(mainNodeDir);
        Vector3Int endPos = doorB + DirectionToStep(oppositeNodeDir);

        bool verif = false;
        int yLevel = node.Coordinate.y;
        int multLevel = 0;
        do
        {
            verif = CorridorVerification(startPos, endPos, yLevel, mainNodeDir, oppositeNodeDir);
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

    // startDoorDir : direction dans laquelle on quitte la salle "node" pour entrer dans le couloir.
    // endDoorDir : direction dans laquelle on quitte la salle "neighbour" pour entrer dans le couloir (= opposé de startDoorDir).
    private bool CorridorVerification(Vector3Int startPos, Vector3Int endPos, int yLevel, DoorDirection startDoorDir, DoorDirection endDoorDir)
    {
        int signY = 1;
        if (yLevel < 0) { signY = -1; }

        int signX = 1;
        int signZ = 1;
        if (startPos.x > endPos.x) { signX = -1; }
        if (startPos.z > endPos.z) { signZ = -1; }

        int relativePosX = Mathf.Abs(endPos.x - startPos.x);
        int relativePosZ = Mathf.Abs(endPos.z - startPos.z);

        DoorDirection verticalTravelDir = signZ > 0 ? DoorDirection.North : DoorDirection.South;
        DoorDirection horizontalTravelDir = signX > 0 ? DoorDirection.East : DoorDirection.West;

        bool CheckPath(bool verticalFirst)
        {
            int i = startPos.x;
            int j = startPos.z;

            if (verticalFirst)
            {
                if (relativePosZ < 10 && startPos.x == endPos.x)
                {
                    return true;
                }
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
                if (relativePosX < 10 && startPos.z == endPos.z)
                {
                    return true;
                }
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
                    GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), DirectionToRotation(verticalTravelDir, _corridorRotationOffset), transform);
                    _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }

                for (; i != endPos.x; i += signX)
                {
                    if (i == startPos.x && j == endPos.z && startPos.x != endPos.x && startPos.z != endPos.z) // corner
                    {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corner;
                        GameObject corridorGO = Instantiate(_cornerPrefab, new Vector3(i, yLevel * 5, j), CornerRotation(verticalTravelDir, horizontalTravelDir), transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                    }
                    else
                    {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), DirectionToRotation(horizontalTravelDir, _corridorRotationOffset), transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                    }
                }

                _usedPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = CaseType.Corridor;
                GameObject lastCorridorGO = Instantiate(_corridorPrefab, new Vector3(endPos.x, yLevel * 5, endPos.z), DirectionToRotation(horizontalTravelDir, _corridorRotationOffset), transform);
                _goPerPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = lastCorridorGO;
            }
            else
            {
                for (; i != endPos.x; i += signX)
                {
                    _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                    GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), DirectionToRotation(horizontalTravelDir, _corridorRotationOffset), transform);
                    _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                }

                for (; j != endPos.z; j += signZ)
                {
                    if (j == startPos.z && i == endPos.x && startPos.z != endPos.z && startPos.x != endPos.x) // corner
                    {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corner;
                        GameObject corridorGO = Instantiate(_cornerPrefab, new Vector3(i, yLevel * 5, j), CornerRotation(horizontalTravelDir, verticalTravelDir), transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                    }
                    else
                    {
                        _usedPos[new Vector3Int(i, yLevel * 5, j)] = CaseType.Corridor;
                        GameObject corridorGO = Instantiate(_corridorPrefab, new Vector3(i, yLevel * 5, j), DirectionToRotation(verticalTravelDir, _corridorRotationOffset), transform);
                        _goPerPos[new Vector3Int(i, yLevel * 5, j)] = corridorGO;
                    }
                }

                _usedPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = CaseType.Corridor;
                GameObject lastCorridorGO = Instantiate(_corridorPrefab, new Vector3(endPos.x, yLevel * 5, endPos.z), DirectionToRotation(verticalTravelDir, _corridorRotationOffset), transform);
                _goPerPos[new Vector3Int(endPos.x, yLevel * 5, endPos.z)] = lastCorridorGO;
            }

            // Colonnes verticales (échelles / intersections) reliant le niveau 0 au niveau du couloir.
            // La colonne "start" doit faire face à la direction par laquelle on quitte la salle de départ.
            // La colonne "end" doit faire face à la direction par laquelle on quitte la salle d'arrivée.
            BuildVerticalColumn(startPos.x, startPos.z, yLevel, signY, startDoorDir);
            BuildVerticalColumn(endPos.x, endPos.z, yLevel, signY, endDoorDir);
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

    private void BuildVerticalColumn(int x, int z, int yLevel, int signY, DoorDirection facing)
    {
        for (int k = 0; k != yLevel * 5; k += signY)
        {
            Vector3Int pos = new Vector3Int(x, k, z);

            if (_usedPos.ContainsKey(pos) && k == 0)
            {
                // Le point de départ touche déjà une salle (rez-de-chaussée) : première intersection.
                _usedPos[pos] = CaseType.FirstInter;
                if (_goPerPos.TryGetValue(pos, out GameObject toDestroy))
                {
                    Destroy(toDestroy);
                    _goPerPos.Remove(pos);
                }
                GameObject firstInterGO = Instantiate(_firstInterPrefab, new Vector3(x, k, z), DirectionToRotation(facing, _intersectionRotationOffset), transform);
                _goPerPos[pos] = firstInterGO;
            }
            else if (_usedPos.ContainsKey(pos))
            {
                // Croisement avec un couloir déjà posé à un autre niveau : intersection standard.
                _usedPos[pos] = CaseType.Intersection;
                if (_goPerPos.TryGetValue(pos, out GameObject toDestroy))
                {
                    Destroy(toDestroy);
                    _goPerPos.Remove(pos);
                }
                GameObject interGO = Instantiate(_intersectionPrefab, new Vector3(x, k, z), DirectionToRotation(facing, _intersectionRotationOffset), transform);
                _goPerPos[pos] = interGO;
            }
            else
            {
                _usedPos[pos] = CaseType.Ladder;
                GameObject ladderGO = Instantiate(_ladderPrefab, new Vector3(x, k, z), DirectionToRotation(facing, _ladderRotationOffset), transform);
                _goPerPos[pos] = ladderGO;
            }
        }
    }

    private Quaternion DirectionToRotation(DoorDirection dir, float offset)
    {
        float baseAngle = dir switch
        {
            DoorDirection.North => 0f,
            DoorDirection.East => 90f,
            DoorDirection.South => 180f,
            DoorDirection.West => 270f,
            _ => 0f
        };
        return Quaternion.Euler(0f, baseAngle + offset, 0f);
    }

    // Rotation pour un corner qui relie deux directions perpendiculaires.
    // ATTENTION : cette table suppose une convention de mesh donnée (voir commentaire).
    // Si le corner est monté à l'envers en jeu, décale _cornerRotationOffset de 90 par 90
    // jusqu'à alignement plutôt que de retoucher cette table.
    private Quaternion CornerRotation(DoorDirection from, DoorDirection to)
    {
        float baseAngle = (from, to) switch
        {
            (DoorDirection.South, DoorDirection.West) or (DoorDirection.West, DoorDirection.South) => 0f,
            (DoorDirection.South, DoorDirection.East) or (DoorDirection.East, DoorDirection.South) => 90f,
            (DoorDirection.North, DoorDirection.East) or (DoorDirection.East, DoorDirection.North) => 180f,
            (DoorDirection.North, DoorDirection.West) or (DoorDirection.West, DoorDirection.North) => 270f,
            _ => 0f
        };
        return Quaternion.Euler(0f, baseAngle + _cornerRotationOffset, 0f);
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