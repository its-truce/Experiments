using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Experiments.Core;

public enum HeuristicType
{
    Manhattan,
    Euclidean,
    Chebyshev,
    Octile
}

public class Node(Point16 position, float gCost = float.MaxValue)
{
    public readonly List<Node> Neighbors = [];
    public readonly Point16 Position = position;

    public float GCost = gCost;
    public float HCost;

    public Node Previous;
    public float FCost => GCost + HCost;

    public bool IsWall => Framing.GetTileSafely(Position).HasTile && Main.tileSolid[Framing.GetTileSafely(Position).TileType];
    public bool IsStranded => Neighbors.Count <= 0 || !Neighbors.Any(t => t.IsWall);

    public void AddNeighbors()
    {
        if (Neighbors.Count > 0) return;

        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                // Skip the center point
                if (di == 0 && dj == 0) continue;

                Point16 point = new(Position.X + di, Position.Y + dj);

                if (WorldGen.InWorld(point.X, point.Y))
                    Neighbors.Add(new Node(point));
            }
        }
    }
}

public class Pathfinding(Point16 start, Point16 end)
{
    private readonly HashSet<Node> _closedSet = [];
    private readonly List<Node> _openSet = [new Node(start, 0)];
    private Node _current;
    private bool _done;
    private List<Node> _path = [];

    private static float GetHeuristic(Point16 start, Point16 end, HeuristicType heuristicType = HeuristicType.Octile)
    {
        float heuristic = 0;

        switch (heuristicType)
        {
            case HeuristicType.Euclidean:
                heuristic = start.Distance(end);
                break;

            case HeuristicType.Manhattan:
                heuristic = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
                break;

            case HeuristicType.Chebyshev:
                heuristic = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                break;

            case HeuristicType.Octile:
                int dx = Math.Abs(start.X - end.X);
                int dy = Math.Abs(start.Y - end.Y);
                const float sqrt2 = 1.4142135623730951f; // Square root of 2

                heuristic = Math.Max(dx, dy) + (sqrt2 - 1) * Math.Min(dx, dy);
                break;
        }

        return heuristic;
    }

    public void Update(HeuristicType heuristicType = HeuristicType.Octile)
    {
        if (_done)
        {
            Main.NewText("done");
            return;
        }

        if (_openSet.Count > 0)
        {
            _current = _openSet.MinBy(node => node.FCost);

            if (_current.Position == end)
                _done = true;

            _openSet.Remove(_current);
            _closedSet.Add(_current);

            _current.AddNeighbors();
            foreach (Node neighbour in _current.Neighbors)
            {
                if (_closedSet.Contains(neighbour) || neighbour.IsWall) continue;

                neighbour.AddNeighbors();

                float tempGCost = _current.GCost + GetHeuristic(neighbour.Position, _current.Position, heuristicType);

                if (tempGCost < neighbour.GCost)
                {
                    neighbour.GCost = tempGCost;
                    neighbour.HCost = GetHeuristic(neighbour.Position, end, heuristicType);
                    neighbour.Previous = _current;

                    if (!_openSet.Contains(neighbour))
                        _openSet.Add(neighbour);
                }
            }
        }
        else
            Main.NewText("no solution :(");

        _path = [_current];
        while (_current.Previous != null)
        {
            _path.Add(_current.Previous);
            _current = _current.Previous;
        }

        _path.Reverse();
    }

    public void Draw(Color? color = null, float scale = 8f)
    {
        for (int i = 0; i < _path.Count - 1; i++)
            Graphics.DrawLine(_path[i].Position.ToWorldCoordinates(), _path[i + 1].Position.ToWorldCoordinates(), color: color, thickness: scale);
    }
}