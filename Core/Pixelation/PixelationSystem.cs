using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Pixelation;

public class PixelationSystem : ModSystem
{
    private readonly List<PixelationTarget> _pixelationTargets = [];
    public static PixelationSystem Instance => ModContent.GetInstance<PixelationSystem>();

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Main.QueueMainThreadAction(() =>
        {
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderTiles));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderNPCs));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderProjectiles));

            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.PrimaryTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                pixelationTarget.ScaledTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
        });

        On_Main.CheckMonoliths += RenderToPrimaryTargets;
        On_Main.CheckMonoliths += RenderToScaledTargets;

        On_Main.DrawCachedProjs += DrawPixelationTargets;

        Main.OnResolutionChanged += ResizeTargets;
    }

    public override void Unload()
    {
        if (Main.dedServ)
            return;

        Main.QueueMainThreadAction(() =>
        {
            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.PrimaryTarget.Dispose();
                pixelationTarget.ScaledTarget.Dispose();
            }
        });

        On_Main.CheckMonoliths -= RenderToPrimaryTargets;
        On_Main.CheckMonoliths -= RenderToScaledTargets;

        On_Main.DrawCachedProjs -= DrawPixelationTargets;

        Main.OnResolutionChanged -= ResizeTargets;
    }

    private void RenderToPrimaryTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.RenderToPrimaryTarget(orig);
    }

    private void RenderToScaledTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.RenderToScaledTarget(orig);
    }

    private void ResizeTargets(Vector2 newSize)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets)
        {
            pixelationTarget.PrimaryTarget.Dispose();
            pixelationTarget.ScaledTarget.Dispose();

            pixelationTarget.PrimaryTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)newSize.X, (int)newSize.Y);
            pixelationTarget.ScaledTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)newSize.X, (int)newSize.Y);
        }
    }

    private static void DrawTarget(PixelationTarget target, bool endSpriteBatch)
    {
        if (target.RenderActions.Count == 0)
            return;

        target.RenderActions.Clear();

        Main.spriteBatch.Restart(samplerState: SamplerState.PointClamp, end: endSpriteBatch);
        Main.spriteBatch.Draw(target.ScaledTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None,
            0);

        Main.spriteBatch.End();
        if (endSpriteBatch)
            Main.spriteBatch.Restart(end: false);
    }

    private void DrawPixelationTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
    {
        orig(self, projCache, startSpriteBatch);

        switch (projCache)
        {
            case var _ when projCache.Equals(Main.instance.DrawCacheProjsBehindNPCsAndTiles):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.UnderTiles))
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.UnderNPCs))
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.UnderProjectiles))
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                break;
        }
    }

    /// <summary>
    ///     Adds an action for pixelation
    /// </summary>
    /// <param name="drawAction">The action to invoke</param>
    /// <param name="renderType">Render layer to draw to</param>
    public void AddRenderAction(Action<SpriteBatch> drawAction, RenderLayer renderType = RenderLayer.UnderProjectiles)
    {
        PixelationTarget target = _pixelationTargets.Find(t => t.RenderType == renderType);
        target.RenderActions.Add(drawAction);
    }
}