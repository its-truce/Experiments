using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

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

    public static bool ContainsPoint(Vector2 center, float radius, Vector2 point) => Vector2.DistanceSquared(center, point) <= radius * radius;

    public static Vector2 GetRandomVector(this Vector2 range) => new(Main.rand.NextFloat(-range.X, range.X), Main.rand.NextFloat(-range.Y, range.Y));

    public static Vector3 GetRandomVector(this Vector3 range) =>
        new(Main.rand.NextFloat(-range.X, range.X), Main.rand.NextFloat(-range.Y, range.Y), Main.rand.NextFloat(-range.Z, range.Z));

    public static Point16 ToPoint16(this Point point) => new(point);

    public static float Distance(this Point16 point1, Point16 point2)
    {
        int differenceX = point1.X - point2.X;
        int differenceY = point1.Y - point2.Y;

        return (float)Math.Sqrt(differenceX * differenceX + differenceY * differenceY);
    }
}