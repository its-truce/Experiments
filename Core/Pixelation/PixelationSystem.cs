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
        Main.QueueMainThreadAction(() => {  
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderTiles));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderNPCs));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.UnderProjectiles));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.OverPlayers));
            _pixelationTargets.Add(new PixelationTarget(RenderLayer.OverWiresUI));

            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.ScalingTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                pixelationTarget.FinalTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
        });

        On_Main.CheckMonoliths += DrawToScalingTargets;
        On_Main.CheckMonoliths += DrawToFinalTargets;
        On_Main.DrawCachedProjs += DrawPixelationTargets;
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() => {  
            foreach (PixelationTarget pixelationTarget in _pixelationTargets)
            {
                pixelationTarget.ScalingTarget.Dispose();
                pixelationTarget.FinalTarget.Dispose();
            }
        });

        On_Main.CheckMonoliths -= DrawToScalingTargets;
        On_Main.CheckMonoliths -= DrawToFinalTargets;
        On_Main.DrawCachedProjs -= DrawPixelationTargets;
    }
    
    private void DrawToScalingTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets)
        {
            pixelationTarget.DrawToScalingTarget(orig);
        }
    }
    
    private void DrawToFinalTargets(On_Main.orig_CheckMonoliths orig)
    {
        foreach (PixelationTarget pixelationTarget in _pixelationTargets)
        {
            pixelationTarget.DrawToFinalTarget(orig);
        }
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
                {
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                }
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.UnderNPCs))
                {
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                }
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.UnderProjectiles))
                {
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                }
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsOverPlayers):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.OverPlayers))
                {
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                }
                break;

            case var _ when projCache.Equals(Main.instance.DrawCacheProjsOverWiresUI):
                foreach (PixelationTarget pixelationTarget in _pixelationTargets.Where(t => t.RenderType == RenderLayer.OverWiresUI))
                {
                    DrawTarget(pixelationTarget, !startSpriteBatch);
                }
                break;
        }
        return;

        void DrawTarget(PixelationTarget target, bool endSpriteBatch)
        {
            target.DrawPasses.Clear();
            
            Main.spriteBatch.Restart(samplerState: SamplerState.PointClamp, end: endSpriteBatch);
            Main.spriteBatch.Draw(target.FinalTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 
                0);
        
            Main.spriteBatch.End();
            if (endSpriteBatch)
                Main.spriteBatch.Restart(end: false);
        }
    }
}