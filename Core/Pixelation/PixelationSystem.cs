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
                pixelationTarget.InitialTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                pixelationTarget.HalfScaleTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
        });

        On_Main.CheckMonoliths += DrawToInitialTargets;
        On_Main.CheckMonoliths += DrawToHalfScaleTargets;
        
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
                pixelationTarget.InitialTarget.Dispose();
                pixelationTarget.HalfScaleTarget.Dispose();
            }
        });

        On_Main.CheckMonoliths -= DrawToInitialTargets;
        On_Main.CheckMonoliths -= DrawToHalfScaleTargets;
        
        On_Main.DrawCachedProjs -= DrawPixelationTargets;
        
        Main.OnResolutionChanged -= ResizeTargets;
    }

    private void DrawToInitialTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.DrawToInitialTarget(orig);
    }

    private void DrawToHalfScaleTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets) pixelationTarget.DrawToHalfScaleTarget(orig);
    }
    
    private void ResizeTargets(Vector2 newSize)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets)
        {
            pixelationTarget.InitialTarget.Dispose();
            pixelationTarget.HalfScaleTarget.Dispose();
            
            pixelationTarget.InitialTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)newSize.X, (int)newSize.Y);
            pixelationTarget.HalfScaleTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)newSize.X, (int)newSize.Y);
        }
    }
    
    private static void DrawTarget(PixelationTarget target, bool endSpriteBatch)
    {
        if (target.DrawActions.Count == 0)
            return;
        
        target.DrawActions.Clear();

        Main.spriteBatch.Restart(samplerState: SamplerState.PointClamp, end: endSpriteBatch);
        Main.spriteBatch.Draw(target.HalfScaleTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None,
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
    
    public void AddPixelationAction(Action<SpriteBatch> drawAction, RenderLayer renderType = RenderLayer.UnderProjectiles)
    {
        PixelationTarget target = _pixelationTargets.Find(t => t.RenderType == renderType);
        target.DrawActions.Add(drawAction);
    }
}