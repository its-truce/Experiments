using Microsoft.Xna.Framework;
using Terraria;

namespace Experiments.Utils;

public static class Maths
{
    public static void SetMagnitude(this ref Vector2 vector2, float magnitude)
    {
        if (vector2 != Vector2.Zero)
            vector2.Normalize();

        vector2 *= magnitude;
    }

    public static void Limit(this ref Vector2 vector2, float magnitude)
    {
        if (vector2.Length() > magnitude)
            vector2.SetMagnitude(magnitude);
    }

    public static Vector2 GetRandomVector(this Vector2 range)
    {
        return new Vector2(Main.rand.NextFloat(-range.X, range.X), Main.rand.NextFloat(-range.Y, range.Y));
    }

    public static Vector3 GetRandomVector(this Vector3 range)
    {
        return new Vector3(Main.rand.NextFloat(-range.X, range.X), Main.rand.NextFloat(-range.Y, range.Y), Main.rand.NextFloat(-range.Z, range.Z));
    }
}