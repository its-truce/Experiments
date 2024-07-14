using System.Collections.Generic;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Verlet;

public class VerletSolver : ModSystem
{
    public static readonly List<VerletBody> Objects = [];
    private readonly Vector2 _gravity = new Vector2(0, 100f);

    private static Vector2 Center => Main.LocalPlayer.Center;
    private readonly float _radius = 400f;

    public override void PostUpdateEverything()
    {
        return;

        const int subSteps = 8;
        const float subDeltaTime = 1f / subSteps;

        for (int i = subSteps; i > 0; i--)
        {
            foreach (VerletBody obj in Objects)
            {
                // Gravity
                obj.Acceleration += _gravity * subSteps;

                // Collisions
                foreach (VerletBody obj2 in Objects)
                {
                    if (obj == obj2) continue;

                    Vector2 collisionAxis = obj.Position - obj2.Position;
                    float collisionDist = collisionAxis.Length();
                    float minDist = obj.Radius + obj2.Radius;

                    if (collisionDist < minDist)
                    {
                        Vector2 n = collisionAxis / collisionDist;
                        float delta = minDist - collisionDist;

                        obj.Position += n * delta * 0.5f;
                        obj2.Position -= n * delta * 0.5f;
                    }
                }

                // Constraint
                Vector2 toBody = obj.Position - Center;
                float dist = toBody.Length();

                if (dist > _radius - obj.Radius)
                {
                    Vector2 n = toBody / dist;
                    obj.Position = Center + n * (_radius - obj.Radius);
                }

                // Update positions
                obj.UpdatePosition(subDeltaTime); // delta time is 1 since this method runs every frame
            }
        }

        if (Maths.ContainsPoint(Center, _radius, Main.MouseWorld) && Main.mouseLeft && !Main.mouseLeftRelease && Main.timeForVisualEffects % 5 == 0)
        {
            VerletBody verletBody = new VerletBody(Main.MouseWorld, Main.rand.Next(10, 20));
            verletBody.Register();
        }
    }

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin();

        Graphics.DrawCircle(Center, _radius, Color.Black * 0.75f);
        foreach (VerletBody obj in Objects)
            obj.Draw();

        Main.spriteBatch.End();
    }
}