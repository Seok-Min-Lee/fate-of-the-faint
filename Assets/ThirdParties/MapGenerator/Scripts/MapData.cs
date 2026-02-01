using System;
using UnityEngine;

[Serializable]
public sealed class MapData
{
    public int version = 1;

    public int floors;
    public int width;

    public NodeData[] nodes;
    public EdgeData[] edges;

    // 진행 상태(필요 없으면 빼도 됨)
    public int currentNodeId; // 플레이어가 현재 위치한 노드
}

[Serializable]
public struct NodeData
{
    public int id;
    public int floor;
    public int col;

    public float extraX;
    public float extraY;

    public int type;   // MapNodeType
    public int state;  // MapNodeState
}

[Serializable]
public struct EdgeData
{
    public int fromId;
    public int toId;
}