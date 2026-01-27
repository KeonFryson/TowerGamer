using System;
using UnityEngine;

[Serializable]
public class PathNodeData
{
    public string nodeName;
    public Vector3 position;
    public int[] nextNodeIndices; // Indices in the PathMapData.nodes array
}