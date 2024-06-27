using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Experiments.Core.Pathfinding;

public enum HeuristicType
{
    Manhattan,
    Euclidean,
    Chebyshev,
    Octile
}

/// <summary>
///     Implements the A* pathfinding algorithm.
///     For more information, see: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">this page</a>.
/// </summary>
public class Pathfinder
{
    private readonly HashSet<Node> _closedSet;
    private readonly bool _ignorePlatforms;
    private readonly Point16 _start;
    private Node _current;

    private bool _done;
    private Dictionary<Point16, Node> _nodeDictionary;
    private bool _noSolution;

    private List<Node> _openSet;
    private List<Node> _path;
    private Point16 _target;

    public Pathfinder(Point16 start, Point16 target, bool ignorePlatforms = false)
    {
        _start = start;
        _target = target;
        _ignorePlatforms = ignorePlatforms;

        _closedSet = [];
        _path = [];
        _openSet = [new Node(start, 0, GetHeuristic(start, target), ignorePlatforms)];

        _nodeDictionary = new Dictionary<Point16, Node> { [start] = _openSet[0] };
        _current = _openSet[0];
    }

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
                const float sqrt2 = 1.4142135623730951f;

                heuristic = Math.Max(dx, dy) + (sqrt2 - 1) * Math.Min(dx, dy);
                break;
        }

        return heuristic;
    }

    public void Update(HeuristicType heuristicType = HeuristicType.Octile)
    {
        if (_done) return;

        if (_openSet.Count > 0)
        {
            _current = _openSet.MinBy(node => node.FCost);

            _openSet.Remove(_current);
            _closedSet.Add(_current);

            if (_current.Position == _target)
            {
                if (_nodeDictionary.TryGetValue(_target, out Node node))
                    _current = node;

                _done = true;
            }

            _current.AddNeighbors(_nodeDictionary);

            foreach (Node neighbor in _current.Neighbors)
            {
                if (_closedSet.Contains(neighbor) || neighbor.IsWall) continue;

                float gScore = _current.GCost + GetHeuristic(neighbor.Position, _current.Position, heuristicType);

                if (gScore < neighbor.GCost)
                {
                    neighbor.GCost = gScore;
                    neighbor.HCost = GetHeuristic(neighbor.Position, _target, heuristicType);
                    neighbor.FCost = neighbor.GCost + neighbor.HCost;
                    neighbor.Previous = _current;

                    if (!_openSet.Contains(neighbor))
                        _openSet.Add(neighbor);
                }
            }
        }
        else
        {
            _noSolution = true;
            return;
        }

        _path = [_current];
        while (_current.Previous != null)
        {
            _path.Add(_current.Previous);
            _current = _current.Previous;
        }

        _path.Reverse();
    }

    public void SetTarget(Point16 target)
    {
        if (_target.Distance(target) > 1)
        {
            _target = target;

            if (_done)
            {
                _closedSet.Clear();
                _path.Clear();
                _openSet = [new Node(_start, 0, GetHeuristic(_start, target), _ignorePlatforms)];

                _nodeDictionary = new Dictionary<Point16, Node> { [_start] = _openSet[0] };
                _current = _openSet[0];
            }

            _done = false;
        }
    }

    /// <summary>
    ///     Returns the current path of the pathfinder.
    /// </summary>
    /// <returns>The current path of the pathfinder. Returns null if no solution was found.</returns>
    public List<Node> GetPath() => _noSolution ? null : _path;

    /// <summary>
    ///     Draws the current path of the pathfinder.
    ///     Blue means that the pathfinder is currently searching, green means that the path has been found, and red means that
    ///     there is no solution
    /// </summary>
    /// <param name="scale"></param>
    public void Draw(float scale = 8f)
    {
        for (int i = 0; i < _path.Count - 1; i++)
        {
            Color defaultColor = _noSolution ? Color.Red : Color.Blue;
            Graphics.DrawLine(_path[i].Position.ToWorldCoordinates(), _path[i + 1].Position.ToWorldCoordinates(), color: _done ? Color.Green : defaultColor,
                thickness: scale);
        }
    }
}