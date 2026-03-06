using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapDataConverter
{
    public static MapData ToSaveData(MapGraph graph)
    {
        if (graph == null) 
        {
            throw new ArgumentNullException(nameof(graph));
        }

        MapData data = new MapData
        {
            floors = graph.Floors,
            width = graph.Width,
            currentNodeId = graph.LatestNode?.Id ?? -1
        };

        data.nodes = graph.Nodes.Select(n => new NodeData
        {
            id = n.Id,
            floor = n.Floor,
            col = n.Col,
            extraX = n.Extra.x,
            extraY = n.Extra.y,
            type = (int)n.Type,
            state = (int)n.State
        }).ToArray();

        data.edges = graph.Edges.Select(e => new EdgeData
        {
            fromId = e.FromId,
            toId = e.ToId
        }).ToArray();

        return data;
    }

    public static MapGraph FromSaveData(MapData data)
    {
        if (data == null) 
        {
            throw new ArgumentNullException(nameof(data));
        }

        MapGraph graph = new MapGraph(data.floors, data.width);

        // 1) Nodes 복구
        graph.Nodes = new List<MapNode>(data.nodes.Length);
        foreach (NodeData nd in data.nodes)
        {
            MapNode node = new MapNode(
                id: nd.id,
                floor: nd.floor,
                col: nd.col,
                extra: new Vector2(nd.extraX, nd.extraY),
                type: (MapNodeType)nd.type
            );
            node.State = (MapNodeState)nd.state; // 진행 상태 반영
            graph.Nodes.Add(node);
        }

        // 2) 캐시 초기화(노드 캐시 + 엣지 딕셔너리 준비)
        graph.RebuildCaches();

        // 3) Edges 복구 (AddEdge로 adjacency/reverse까지 자동 구축)
        graph.Edges = new List<MapEdge>(data.edges.Length);
        foreach (EdgeData ed in data.edges)
        {
            graph.AddEdge(new MapEdge(ed.fromId, ed.toId));
        }

        //
        MapNode currentNode = graph.GetNode(data.currentNodeId);
        graph.SetLatestNode(currentNode);

        return graph;
    }
}
