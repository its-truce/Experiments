using System;
using System.Collections.Generic;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Experiments.Core.Pixelation;

public enum RenderLayer
{
    UnderTiles,
    UnderNPCs,
    UnderProjectiles
}

public class PixelationTarget(RenderLayer renderType)
{
    public readonly List<Action> RenderActions = [];
    public readonly RenderLayer RenderType = renderType;

    /// <summary>
    ///     Initial render target for drawing the given <see cref="Action" />s.
    /// </summary>
    public RenderTarget2D PrimaryTarget;

    /// <summary>
    ///     Draws the <see cref="PrimaryTarget" /> at half scale. It is then drawn at double scale, creating a pixelation
    ///     effect.
    /// </summary>
    public RenderTarget2D ScaledTarget;

    public void RenderToPrimaryTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();
        RenderTargetBinding[] oldTargets = PrimaryTarget.SwapTo();

        foreach (Action action in RenderActions)
        {
            Main.spriteBatch.Begin();
            action();
            Main.spriteBatch.End();
        }

        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }

    public void RenderToScaledTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        Main.spriteBatch.Begin();
        RenderTargetBinding[] oldTargets = ScaledTarget.SwapTo();

        Main.spriteBatch.Draw(PrimaryTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None,
            0);

        Main.spriteBatch.End();
        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }
}