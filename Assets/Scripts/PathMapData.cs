using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PathMapData", menuName = "Path/Map Data", order = 1)]
public class PathMapData : ScriptableObject
{
    public PathNodeData[] nodes;
}