using System;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Experiments.Core.InverseKinematics;

public class Bone(Vector2 start, float length, float angle = 0, float strokeWeight = 1f)
{
    public Vector2 Start = start;
    public Vector2 End => Start + new Vector2(float.Cos(angle), float.Sin(angle)) * length;

    /// <summary>
    ///     Draws a line from the <see cref="Start" /> to <see cref="End" /> of the segment.
    /// </summary>
    /// <param name="texture"><see cref="TextureAssets.MagicPixel" /> by default</param>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <param name="spriteFacingUpwards">
    ///     Whether the sprite is facing upwards or not. Used to offset the rotation by
    ///     <see cref="MathF.PI" />/2 radians.
    /// </param>
    /// <remarks>
    ///     This method uses the <see cref="Graphics.DrawLine" /> method to draw the curve line segment by segment.
    /// </remarks>
    public void Draw(Texture2D texture = null, Color? color = null, bool spriteFacingUpwards = true)
    {
        Graphics.DrawLine(Start, End, texture, color, spriteFacingUpwards, strokeWeight);
    }

    public void Follow(Vector2 target)
    {
        Vector2 dir = Start.DirectionTo(target);
        angle = dir.ToRotation();

        dir *= -length;
        Start = target + dir;
    }
}