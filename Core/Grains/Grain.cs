using Microsoft.Xna.Framework;

namespace Experiments.Core.Grains;

public struct Grain(Vector2 position, float value, float size)
{
    public Vector2 Position = position;
    public Vector2 Center = position + new Vector2(size / 2);
    public float Value = value;

    public bool HasSand => Value > 0;
}