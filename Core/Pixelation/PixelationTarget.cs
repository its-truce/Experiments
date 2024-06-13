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
    UnderProjectiles,
}

public class PixelationTarget(RenderLayer renderType)
{
    public readonly List<Action<SpriteBatch>> DrawActions = [];
    public readonly RenderLayer RenderType = renderType;

    /// <summary>
    ///     Draws the <see cref="InitialTarget" /> at half scale. It is then drawn at double scale, creating a pixelation
    ///     effect.
    /// </summary>
    public RenderTarget2D HalfScaleTarget;

    /// <summary>
    ///     Initial render target for drawing the given <see cref="Action" />s.
    /// </summary>
    public RenderTarget2D InitialTarget;

    public void DrawToInitialTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();
        RenderTargetBinding[] oldTargets = InitialTarget.SwapTo();

        foreach (Action<SpriteBatch> action in DrawActions)
        {
            Main.spriteBatch.Begin();
            action(Main.spriteBatch);
            Main.spriteBatch.End();
        }

        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }

    public void DrawToHalfScaleTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        Main.spriteBatch.Begin();
        RenderTargetBinding[] oldTargets = HalfScaleTarget.SwapTo();

        Main.spriteBatch.Draw(InitialTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None,
            0);

        Main.spriteBatch.End();
        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }
}