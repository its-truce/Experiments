using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace Experiments.Core.Grains;

public class Simulation
{
    private readonly Vector2 _start;
    private readonly float _width;
    private readonly float _height;
    private readonly float _size;

    private readonly Dictionary<Vector2, Grain> _currentDictionary;
    private readonly Dictionary<Vector2, Grain> _nextDictionary;

    public Simulation(Vector2 start, float width, float height, float size)
    {
        _start = start;
        _width = width;
        _height = height;
        _size = size;

        _currentDictionary = new Dictionary<Vector2, Grain>();
        _nextDictionary = new Dictionary<Vector2, Grain>();

        for (float i = _start.X; i < _width; i += _size)
        {
            for (float j = _start.Y; j < _height; j += _size)
            {
                Vector2 position = new Vector2(i, j);

                _currentDictionary[position] = new Grain(position, 0, _size);
                _nextDictionary[position] = new Grain(position, 0, _size);
            }
        }
    }

    public void Update()
    {
        for (float i = _start.X; i < _width; i += _size)
        {
            for (float j = _start.Y; j < _height; j += _size)
            {
                Vector2 position = new Vector2(i, j);
                Grain grain = _currentDictionary[position];

                if (grain.HasSand)
                {
                    int direction = Main.rand.NextFromCollection([1, -1]);

                    Grain grainBelow = _currentDictionary[new Vector2(i, j + 1)];
                    if (grainBelow.Value == 0 && !Framing.GetTileSafely(grainBelow.Position).HasTile)
                    {
                        Grain nextGrain = _nextDictionary[grainBelow.Position];
                        nextGrain.Value = grain.Value;
                        continue;
                    }

                    // Ensure i + direction and i - direction are within bounds
                    bool withinBoundsPlus = i + direction >= _start.X && i + direction < _start.Y;
                    bool withinBoundsMinus = i - direction >= _start.X && i - direction < _start.Y;

                    Grain grainPlus = _currentDictionary[new Vector2(i + direction, j + 1)];
                    Grain grainMinus = _currentDictionary[new Vector2(i - direction, j + 1)];

                    if (withinBoundsPlus && grainPlus.Value == 0 && !Framing.GetTileSafely(grainPlus.Position).HasTile)
                    {
                        Grain nextGrain = _nextDictionary[grainPlus.Position];
                        nextGrain.Value = grain.Value;
                    }
                    else if (withinBoundsMinus && grainMinus.Value == 0 && !Framing.GetTileSafely(grainMinus.Position).HasTile)
                    {
                        Grain nextGrain = _nextDictionary[grainMinus.Position];
                        nextGrain.Value = grain.Value;
                    }
                    else
                    {
                        Grain currentGrain = _nextDictionary[grain.Position];
                        currentGrain.Value = grain.Value;
                    }
                }
            }
        }
    }
}