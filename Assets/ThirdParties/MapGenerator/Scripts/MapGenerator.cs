using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapNodeType
{
    Combat,
    Elite,
    Event,
    Shop,
    Rest,
    Treasure,
    Boss,
    Start
}

public enum MapNodeState
{
    None,
    Visited,
    Impossible,
}

[Serializable]
public sealed class MapNode
{
    public MapNode(int id, int floor, int col, Vector2 extra, MapNodeType type = MapNodeType.Combat)
    {
        Id = id;
        Floor = floor;
        Col = col;
        Type = type;

        Extra = extra;
        State = MapNodeState.None;
    }

    public int Id { get; private set; }
    public int Floor { get; private set; }   // 0..Floors-1
    public int Col { get; private set; }     // 0..Width-1
    public MapNodeType Type;

    public Vector2 Extra { get; private set; }
    public MapNodeState State;
}
public struct MapEdge
{
    public MapEdge(int fromId, int toId)
    {
        FromId = fromId;
        ToId = toId;
    }
    public int FromId { get; private set; }
    public int ToId { get; private set; }
}

[Serializable]
public sealed class MapGraph
{
    public MapNode LatestNode { get; private set; }
    public int Floors { get; private set; }
    public int Width { get; private set; }
    public List<MapNode> Nodes = new ();
    
    public IReadOnlyDictionary<int, List<int>> Adjacency => _edgeByFromId;
    public IReadOnlyDictionary<int, List<int>> Reverse => _edgeByToId;

    // Convenience caches
    private Dictionary<int, MapNode> _nodeById;
    private Dictionary<int, List<MapNode>> _nodeByFloor;

    public List<MapEdge> Edges = new ();
    private Dictionary<int, List<int>> _edgeByFromId;
    private Dictionary<int, List<int>> _edgeByToId;
    public MapGraph(int floors, int width)
    {
        Floors = floors;
        Width = width;
    }

    public void RebuildCaches()
    {
        _nodeById = Nodes.ToDictionary(n => n.Id, n => n);
        _nodeByFloor = Nodes.GroupBy(n => n.Floor).ToDictionary(g => g.Key, g => g.ToList());

        _edgeByFromId = new Dictionary<int, List<int>>();
        _edgeByToId = new Dictionary<int, List<int>>();
    }
    public void AddEdge(MapEdge edge)
    {
        Edges.Add(edge);

        if (!_edgeByFromId.ContainsKey(edge.FromId))
        {
            _edgeByFromId[edge.FromId] = new List<int>();
        }
        _edgeByFromId[edge.FromId].Add(edge.ToId);

        if (!_edgeByToId.ContainsKey(edge.ToId))
        {
            _edgeByToId[edge.ToId] = new List<int>();
        }
        _edgeByToId[edge.ToId].Add(edge.FromId);
    }
    public MapNode GetNode(int id) 
    {
        if (_nodeById.TryGetValue(id, out MapNode node))
        {

        }

        return node;
    }

    public IReadOnlyList<MapNode> GetFloor(int floor) 
    {
        if (_nodeByFloor.TryGetValue(floor, out List<MapNode> list))
        {
            return list;
        }

        return Array.Empty<MapNode>();
    }

    public IEnumerable<int> GetNext(int fromId)
    {
        if (_edgeByFromId.TryGetValue(fromId, out List<int> list))
        {
            return list;
        }

        return Enumerable.Empty<int>();
    }

    public IEnumerable<int> GetPrev(int toId)
    {
        if (_edgeByToId.TryGetValue(toId, out List<int> list))
        {
            return list;
        }

        return Enumerable.Empty<int>();
    }
    public void SetLatestNode(MapNode node)
    {
        LatestNode = node;
    }
    public HashSet<int> BFS(int startNodeId)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> q = new Queue<int>();

        visited.Add(startNodeId);
        q.Enqueue(startNodeId);

        while (q.Count > 0)
        {
            int cur = q.Dequeue();

            if (_edgeByFromId.TryGetValue(cur, out List<int> nextList))
            {
                for (int i = 0; i < nextList.Count; i++)
                {
                    int nxt = nextList[i];
                    if (visited.Add(nxt))
                    {
                        q.Enqueue(nxt);
                    }
                }
            }
        }

        return visited;
    }
}

[Serializable]
public sealed class MapGenConfig
{
    [Header("Shape")]
    public int Floors = 15;
    public int Width = 7;
    public int MinNodesPerFloor = 2;
    public int MaxNodesPerFloor = 5;

    [Header("Connections")]
    public int MinOutDegree = 1;
    public int MaxOutDegree = 2;
    public int MaxDeltaCol = 1; // 1 => {-1,0,+1}

    [Header("Retries")]
    public int MaxAttempts = 50;

    [Header("Type targets (rough)")]
    public int TargetShops = 2;
    public int TargetRests = 3;
    public int TargetElites = 3;
    public int TargetTreasures = 1;

    [Header("Type constraints")]
    public int NoShopBeforeFloor = 3;     // floors < 3 => no shop
    public int NoRestBeforeFloor = 2;     // floors < 2 => no rest
    public int NoEliteBeforeFloor = 3;    // floors < 3 => no elite
    public int MinEliteSpacing = 2;       // floors distance between elites along floor index (soft constraint)

    [Header("Seed")]
    public int Seed = 0; // 0 => random
}

public static class MapGenerator
{
    public static MapGraph Generate(MapGenConfig cfg)
    {
        System.Random rng = cfg.Seed == 0 ? 
                            new System.Random(Environment.TickCount) : 
                            new System.Random(cfg.Seed);

        for (int attempt = 0; attempt < cfg.MaxAttempts; attempt++)
        {
            MapGraph graph = new MapGraph(floors: cfg.Floors, width: cfg.Width);

            // 1) Create nodes floor-by-floor (positions only)
            int nextId = 1;

            // Start node (floor 0) is special: we still place it in floor 0, but you can also keep start as separate.
            // Here: create floor 0 nodes normally, then mark the "best center" as Start, or create a dedicated Start.
            // We'll create a dedicated Start at floor 0 with center-ish col.
            int startCol = cfg.Width / 2;
            MapNode startNode = new MapNode(
                id: nextId++, 
                floor: 0, 
                col: startCol, 
                extra: new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()),
                type: MapNodeType.Start
            );
            startNode.State = MapNodeState.Visited; //
            graph.Nodes.Add(startNode);

            // Floors 1..Floors-2 are regular; last floor is Boss
            for (int f = 1; f < cfg.Floors - 1; f++)
            {
                int count = rng.Next(cfg.MinNodesPerFloor, cfg.MaxNodesPerFloor + 1);
                List<int> cols = SampleUniqueCols(rng, cfg.Width, count);
                foreach (int c in cols)
                {
                    graph.Nodes.Add(new MapNode(
                        id: nextId++, 
                        floor: f, 
                        col: c,
                        extra: new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()),
                        type: MapNodeType.Combat
                    )); // temp type
                }
            }

            // Boss floor
            MapNode boss = new MapNode(
                id:  nextId++, 
                floor: cfg.Floors - 1, 
                col: startCol,
                extra: new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()),
                type: MapNodeType.Boss
            );
            graph.Nodes.Add(boss);

            graph.RebuildCaches();

            // 2) Connect floor 0(Start) -> floor 1
            if (!ConnectStart(graph, cfg, rng, startNode.Id))
            {
                continue;
            }

            // 3) Connect intermediate floors i -> i+1
            bool ok = true;
            for (int f = 1; f < cfg.Floors - 1; f++)
            {
                if (!ConnectFloors(graph, cfg, rng, f, f + 1))
                {
                    ok = false;
                    break;
                }
            }
            if (!ok)
            {
                continue;
            }

            // 4) Validate connectivity: start reaches boss; no isolated nodes
            if (!ValidateGraph(graph, startNode.Id, boss.Id))
            {
                continue;
            }

            // 5) Assign node types with constraints + targets
            AssignTypes(graph, cfg, rng, startNode.Id, boss.Id);

            //
            graph.SetLatestNode(startNode);

            Debug.Log(attempt);

            return graph;
        }

        throw new Exception("Map generation failed: exceeded MaxAttempts. Try loosening constraints or increasing attempts.");
    }

    private static bool ConnectStart(MapGraph g, MapGenConfig cfg, System.Random rng, int startId)
    {
        IReadOnlyList<MapNode> nextFloor = g.GetFloor(1);

        if (nextFloor.Count == 0)
        {
            return false;
        }

        // Start connects to 1-2 nodes near its col
        int outDeg = rng.Next(cfg.MinOutDegree, cfg.MaxOutDegree + 1);
        MapNode start = g.GetNode(startId);

        List<MapNode> candidates = nextFloor
            .Where(n => Math.Abs(n.Col - start.Col) <= cfg.MaxDeltaCol + 1) // allow a bit wider for start
            .OrderBy(n => Math.Abs(n.Col - start.Col))
            .ToList();

        if (candidates.Count == 0) 
        {
            candidates = nextFloor.ToList();
        } 

        List<MapNode> picked = PickDistinct(rng, candidates, Math.Min(outDeg, candidates.Count));

        for (int i = 0; i < picked.Count; i++)
        {
            g.AddEdge(new MapEdge(fromId: start.Id, toId: picked[i].Id));
        }

        return picked.Count > 0;
    }

    private static bool ConnectFloors(MapGraph g, MapGenConfig cfg, System.Random rng, int fromFloor, int toFloor)
    {
        IReadOnlyList<MapNode> fromNodes = g.GetFloor(fromFloor);
        IReadOnlyList<MapNode> toNodes = g.GetFloor(toFloor);

        // Boss floor has single node: ensure every from node can reach it (at least one path)
        if (toNodes.Count == 0) 
        {
            return false;
        }

        // Build candidate list for each from node
        foreach (MapNode from in fromNodes)
        {
            int outDeg = rng.Next(cfg.MinOutDegree, cfg.MaxOutDegree + 1);

            List<MapNode> candidates = toNodes
                                        .Where(n => Math.Abs(n.Col - from.Col) <= cfg.MaxDeltaCol)
                                        .OrderBy(n => Math.Abs(n.Col - from.Col))
                                        .ToList();

            if (candidates.Count == 0)
            {
                // If none available within delta, fall back to nearest
                candidates = toNodes.OrderBy(n => Math.Abs(n.Col - from.Col))
                                    .Take(2)
                                    .ToList();
            }

            List<MapNode> picked = PickDistinct(rng, candidates, Math.Min(outDeg, candidates.Count));

            for (int i = 0; i < picked.Count; i++)
            {
                g.AddEdge(new MapEdge(fromId: from.Id, toId: picked[i].Id));
            }
        }

        // Ensure every node in toFloor has at least one incoming edge (avoid isolated)
        foreach (MapNode to in toNodes)
        {
            if (!g.GetPrev(to.Id).Any())
            {
                // attach from nearest node
                MapNode nearestFrom = fromNodes.OrderBy(n => Math.Abs(n.Col - to.Col)).FirstOrDefault();

                if (nearestFrom == null)
                {
                    return false;
                } 

                g.AddEdge(new MapEdge(fromId: nearestFrom.Id, toId: to.Id));
            }
        }

        // Optional: reduce excessive crossings (basic heuristic)
        ReduceCrossings(g, fromNodes);

        return true;
    }

    private static void ReduceCrossings(MapGraph g, IReadOnlyList<MapNode> fromNodes)
    {
        // Simple heuristic:
        // For each from node, sort its outgoing by to.Col. This doesn't fully eliminate crossings
        // but reduces visual chaos.
        foreach (MapNode from in fromNodes)
        {
            if (!g.Adjacency.TryGetValue(from.Id, out List<int> list) || list.Count <= 1)
            {
                continue;
            }

            list.Sort((a, b) => g.GetNode(a).Col.CompareTo(g.GetNode(b).Col));
        }
    }

    private static bool ValidateGraph(MapGraph g, int startId, int bossId)
    {
        // 1) Reachability: start -> boss
        HashSet<int> reachable = BFS(g, startId);

        if (!reachable.Contains(bossId))
        { 
            return false;
        }

        // 2) No isolated nodes (except Start/Boss already covered)
        foreach (MapNode node in g.Nodes)
        {
            if (node.Id == startId) 
            {
                continue;
            }

            bool hasIn = g.GetPrev(node.Id).Any();
            bool hasOut = g.GetNext(node.Id).Any();

            if (node.Id == bossId)
            {
                if (!hasIn) 
                {
                    return false;
                }
                continue;
            }

            if (!hasIn || !hasOut)
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<int> BFS(MapGraph g, int startId)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> q = new Queue<int>();
        visited.Add(startId);
        q.Enqueue(startId);

        while (q.Count > 0)
        {
            int cur = q.Dequeue();

            foreach (int nxt in g.GetNext(cur))
            {
                if (visited.Add(nxt))
                {
                    q.Enqueue(nxt);
                }
            }
        }

        return visited;
    }

    private static void AssignTypes(MapGraph g, MapGenConfig cfg, System.Random rng, int startId, int bossId)
    {
        // Fixed
        g.GetNode(startId).Type = MapNodeType.Start;
        g.GetNode(bossId).Type = MapNodeType.Boss;

        // Collect assignable nodes (exclude start/boss)
        List<MapNode> nodes = g.Nodes.Where(n => n.Id != startId && n.Id != bossId).ToList();

        // We'll do a two-pass:
        // (A) Place scarce targets (Shop/Rest/Elite/Treasure) with constraints
        // (B) Fill remaining with Combat/Event using floor-based weights

        // Track placed floors for spacing constraints
        List<int> eliteFloors = new List<int>();

        PlaceTargetType(MapNodeType.Shop, cfg.TargetShops, n => AllowShop(cfg, n), score: n => MidFloorScore(cfg, n), nodes, rng);
        PlaceTargetType(MapNodeType.Treasure, cfg.TargetTreasures, n => AllowTreasure(cfg, n), score: n => MidFloorScore(cfg, n), nodes, rng);

        // Rest: prefer late floors, especially boss-1
        PlaceTargetType(MapNodeType.Rest, cfg.TargetRests, n => AllowRest(cfg, n),
            score: n => LateFloorScore(cfg, n), nodes, rng);

        // Elite: prefer mid-late, with spacing
        int eliteToPlace = cfg.TargetElites;
        for (int i = 0; i < eliteToPlace; i++)
        {
            List<MapNode> candidates = nodes
                .Where(n => n.Type == MapNodeType.Combat) // only overwrite Combat placeholder
                .Where(n => AllowElite(cfg, n))
                .Where(n => eliteFloors.All(ef => Math.Abs(ef - n.Floor) >= cfg.MinEliteSpacing))
                .OrderByDescending(n => LateMidScore(cfg, n))
                .ToList();

            if (candidates.Count == 0) 
            {
                break; // soft constraint: if impossible, skip remaining
            }

            MapNode pick = WeightedPick(rng, candidates, n => 1 + LateMidScore(cfg, n));
            pick.Type = MapNodeType.Elite;
            eliteFloors.Add(pick.Floor);
        }

        // 미구현
        //// (B) Fill remaining nodes with Combat/Event (and a little Rest if none, etc.)
        //foreach (MapNode n in nodes)
        //{
        //    if (n.Type != MapNodeType.Combat) 
        //    {
        //        continue; // already placed scarce types
        //    }

        //    // Floor-based weights
        //    // Early: Combat heavy
        //    // Mid: Event appears
        //    // Late: Combat slightly rises, rest handled earlier
        //    int combatW = 70;
        //    int eventW = 30;

        //    if (n.Floor < 3) 
        //    {
        //        combatW = 85; 
        //        eventW = 15; 
        //    }
        //    else if (n.Floor >= cfg.Floors - 3)
        //    { 
        //        combatW = 80; 
        //        eventW = 20; 
        //    }

        //    // If floor is immediately before boss, slightly reduce event
        //    if (n.Floor == cfg.Floors - 2) 
        //    { 
        //        combatW = 90; 
        //        eventW = 10; 
        //    }

        //    n.Type = Roll2(rng, combatW, eventW) ? MapNodeType.Combat : MapNodeType.Event;
        //}
    }

    private static void PlaceTargetType(
        MapNodeType type,
        int targetCount,
        Func<MapNode, bool> allow,
        Func<MapNode, int> score,
        List<MapNode> nodes,
        System.Random rng
    )
    {
        int placed = 0;

        while (placed < targetCount)
        {
            List<MapNode> candidates = nodes
                .Where(n => n.Type == MapNodeType.Combat) // only replace default
                .Where(allow)
                .ToList();

            if (candidates.Count == 0) 
            {
                break;
            }

            MapNode pick = WeightedPick(rng, candidates, n => 1 + score(n));
            pick.Type = type;
            placed++;
        }
    }

    private static bool AllowShop(MapGenConfig cfg, MapNode n)
    { 
        return n.Floor >= cfg.NoShopBeforeFloor && n.Floor <= cfg.Floors - 3;
    }

    private static bool AllowRest(MapGenConfig cfg, MapNode n)
    { 
        return n.Floor >= cfg.NoRestBeforeFloor && n.Floor <= cfg.Floors - 2; // boss-1 allowed
    }

    private static bool AllowElite(MapGenConfig cfg, MapNode n)
    { 
        return n.Floor >= cfg.NoEliteBeforeFloor && n.Floor <= cfg.Floors - 3;
    }

    private static bool AllowTreasure(MapGenConfig cfg, MapNode n)
    {
        return n.Floor >= 3 && n.Floor <= cfg.Floors - 4;
    }

    // Scoring helpers (higher => more likely)
    private static int MidFloorScore(MapGenConfig cfg, MapNode n)
    {
        // Peak near middle
        float mid = (cfg.Floors - 1) * 0.5f;
        float dist = Mathf.Abs(n.Floor - mid);
        return Mathf.Max(0, 10 - Mathf.RoundToInt(dist));
    }

    private static int LateFloorScore(MapGenConfig cfg, MapNode n)
    {
        // Later floors higher score
        return Mathf.Clamp(n.Floor, 0, cfg.Floors);
    }

    private static int LateMidScore(MapGenConfig cfg, MapNode n)
    {
        // Prefer mid-to-late, avoid too early
        int baseScore = n.Floor;

        if (n.Floor < 4) 
        {
            baseScore -= 3;
        }

        return Mathf.Max(0, baseScore);
    }

    private static bool Roll2(System.Random rng, int wA, int wB)
    {
        int total = wA + wB;
        int r = rng.Next(0, total);
        return r < wA;
    }

    private static T WeightedPick<T>(System.Random rng, List<T> items, Func<T, int> weight)
    {
        int sum = 0;
        for (int i = 0; i < items.Count; i++)
        {
            int w = Math.Max(0, weight(items[i]));
            sum += w;
        }

        if (sum <= 0) 
        {
            return items[rng.Next(items.Count)];
        } 

        int roll = rng.Next(0, sum);
        int acc = 0;

        for (int i = 0; i < items.Count; i++)
        {
            acc += Math.Max(0, weight(items[i]));
            if (roll < acc) 
            {
                return items[i];
            }
        }

        return items[^1];
    }

    private static List<int> SampleUniqueCols(System.Random rng, int width, int count)
    {
        List<int> cols = Enumerable.Range(0, width).ToList();
        Shuffle(rng, cols);

        return cols.Take(count).OrderBy(c => c).ToList();
    }

    private static List<T> PickDistinct<T>(System.Random rng, List<T> source, int count)
    {
        if (count <= 0) 
        {
            return new List<T>();
        }

        List<T> copy = new List<T>(source);
        Shuffle(rng, copy);

        return copy.Take(count).ToList();
    }

    private static void Shuffle<T>(System.Random rng, IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
