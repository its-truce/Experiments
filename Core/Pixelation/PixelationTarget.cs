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
    OverPlayers,
    OverWiresUI
}

public class PixelationTarget(RenderLayer renderType)
{
    public readonly List<Action<SpriteBatch>> DrawPasses = [];
    
    public RenderTarget2D ScalingTarget;
    public RenderTarget2D FinalTarget;

    public readonly RenderLayer RenderType = renderType;
    
    public void DrawToScalingTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();
        RenderTargetBinding[] oldTargets = ScalingTarget.SwapTo();

        foreach (Action<SpriteBatch> pass in DrawPasses)
        {
            Main.spriteBatch.Begin();
            pass(Main.spriteBatch);
            Main.spriteBatch.End();
        }
        
        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }

    public void DrawToFinalTarget(On_Main.orig_CheckMonoliths orig)
    {
        orig();
        
        Main.spriteBatch.Begin();
        RenderTargetBinding[] oldTargets = FinalTarget.SwapTo();
        
        Main.spriteBatch.Draw(ScalingTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None,
            0);
        
        Main.spriteBatch.End();
        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
    }
}