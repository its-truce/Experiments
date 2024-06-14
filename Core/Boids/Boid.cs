using System;
using System.Collections.Generic;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Boids;

public enum BehaviorType
{
    Separation,
    Alignment,
    Cohesion
}

//TODO: document
public class Boid(Vector2 position, Vector2 velocity, Vector2 acceleration, int perceptionRadius = 100, float maxForce = 1, float maxSpeed = 4, Texture2D texture = null)
{
    private Vector2 _position = position;
    private Vector2 _velocity = velocity;

    public Vector2 Acceleration = acceleration;

    public void Update()
    {
        _position += _velocity;
        _velocity += Acceleration;
        _velocity.Limit(maxSpeed);
    }

    public Vector2 CalculateForce(IEnumerable<Boid> boids, BehaviorType behaviorType)
    {
        Vector2 force = Vector2.Zero;
        int count = 0;

        foreach (Boid boid in boids)
        {
            if (boid == this)
                continue;

            float distanceSquared = Vector2.DistanceSquared(_position, boid._position);

            if (distanceSquared < perceptionRadius * perceptionRadius)
            {
                switch (behaviorType)
                {
                    case BehaviorType.Separation:
                        Vector2 difference = _position - boid._position;
                        difference /= distanceSquared;
                        force += difference;
                        break;

                    case BehaviorType.Alignment:
                        force += boid._velocity;
                        break;

                    case BehaviorType.Cohesion:
                        force += boid._position - _position;
                        break;
                }

                count++;
            }
        }

        if (count > 0)
        {
            force /= count;

            force.SetMagnitude(maxSpeed);
            force -= _velocity;
            force.Limit(maxForce);
        }

        return force;
    }

    public void Draw(Color? color = null, bool spriteFacingUpwards = false)
    {
        texture ??= ModContent.Request<Texture2D>($"{nameof(Experiments)}/Assets/Textures/Boid").Value;
        Color drawColor = color ?? Color.White;
        float offset = spriteFacingUpwards ? MathF.PI / 2 : 0;

        Main.EntitySpriteDraw(texture, _position - Main.screenPosition, null, drawColor, _velocity.ToRotation() + offset, texture.Size() / 2,
            1f, SpriteEffects.None);
    }
}