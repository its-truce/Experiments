using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Pixelation;

//TODO: add dust render-type
public class PixelationSystem : ModSystem
{
    private readonly List<PixelationTarget> _pixelationTargets = [];

    public override void Load()
    {
        Main.QueueMainThreadAction(() =>
        {
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderTiles));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderNPCs));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderProjectiles));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.Dust));

            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.InitialTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                pixelationTarget.HalfScaleTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
        });

        On_Main.CheckMonoliths += DrawToInitialTargets;
        On_Main.CheckMonoliths += DrawToHalfScaleTargets;
        On_Main.DrawCachedProjs += DrawPixelationTargets;
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.InitialTarget.Dispose();
                pixelationTarget.HalfScaleTarget.Dispose();
            }
        });

        On_Main.CheckMonoliths -= DrawToInitialTargets;
        On_Main.CheckMonoliths -= DrawToHalfScaleTargets;
        On_Main.DrawCachedProjs -= DrawPixelationTargets;
    }

    private void DrawToInitialTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.DrawToInitialTarget(orig);
    }

    private void DrawToHalfScaleTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.DrawToHalfScaleTarget(orig);
    }

    public void AddPixelationPass(Action<SpriteBatch> drawAction, RenderLayer renderType = RenderLayer.UnderProjectiles)
    {
        PixelationTarget target = _pixelationTargets.Find(t => t.RenderType == renderType);
        target.DrawPasses.Add(drawAction);
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

        return;

        void DrawTarget(PixelationTarget target, bool endSpriteBatch)
        {
            target.DrawPasses.Clear();

            Main.spriteBatch.Restart(samplerState: SamplerState.PointClamp, end: endSpriteBatch);
            Main.spriteBatch.Draw(target.HalfScaleTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None,
                0);

            Main.spriteBatch.End();
            if (endSpriteBatch)
                Main.spriteBatch.Restart(end: false);
        }
    }
}