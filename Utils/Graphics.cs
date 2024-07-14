using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Experiments.Utils;

public static class Graphics
{
    public const string TextureDirectory = $"{nameof(Experiments)}/Assets/Textures/";

    public static Texture2D GetTexture(string name, string path = TextureDirectory) => ModContent.Request<Texture2D>(path + name).Value;

    public static Texture2D[] GetTextures(string path = TextureDirectory, params string[] names)
    {
        path ??= TextureDirectory;
        List<Texture2D> textures = [];
        textures.AddRange(names.Select(name => GetTexture(name, path)));

        return textures.ToArray();
    }

    /// <summary>
    ///     Draws a straight line from one point to another.
    /// </summary>
    /// <param name="start">Start of the line</param>
    /// <param name="end">End of the line</param>
    /// <param name="texture"><see cref="TextureAssets.MagicPixel" /> by default</param>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <param name="spriteFacingUpwards">
    ///     Whether the sprite is facing upwards or not. Used to offset the rotation by
    ///     <see cref="MathF.PI" />/2 radians.
    /// </param>
    /// <param name="thickness">Thickness of the line</param>
    public static void DrawLine(Vector2 start, Vector2 end, Texture2D texture = null, Color? color = null, bool spriteFacingUpwards = true, float thickness = 1)
    {
        texture ??= TextureAssets.MagicPixel.Value;
        Color drawColor = color ?? Color.White;

        Vector2 scale = new(thickness, Vector2.Distance(start, end) / texture.Height);
        Vector2 origin = new(texture.Width / 2f, 0f);

        float rotation = start.DirectionTo(end).ToRotation() - (spriteFacingUpwards ? MathF.PI / 2 : 0);

        Main.EntitySpriteDraw(texture, start - Main.screenPosition, null, drawColor, rotation, origin, scale, SpriteEffects.None);
    }

    /// <summary>
    ///     Draws a circle at the given position.
    /// </summary>
    /// <param name="center">Center or position of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    public static void DrawCircle(Vector2 center, float radius, Color? color = null)
    {
        Texture2D texture = Graphics.GetTexture("Circle");
        Color drawColor = color ?? Color.White;

        float scale = radius / (texture.Height / 2);

        Main.EntitySpriteDraw(texture, center - Main.screenPosition, null, drawColor, 0, texture.Size() / 2, scale, SpriteEffects.None);
    }

    /// <summary>
    ///     Sets the current render target to the provided one.
    /// </summary>
    /// <param name="target">The render target to swap to</param>
    /// <param name="flushColor">The color to clear the screen with. Transparent by default</param>
    /// <returns>The old render target bindings.</returns>
    public static RenderTargetBinding[] SwapTo(this RenderTarget2D target, Color? flushColor = null)
    {
        if (Main.gameMenu || Main.dedServ || target is null || Main.instance.GraphicsDevice is null || Main.spriteBatch is null)
            return null;

        RenderTargetBinding[] oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

        Main.graphics.GraphicsDevice.SetRenderTarget(target);
        Main.graphics.GraphicsDevice.Clear(flushColor ?? Color.Transparent);

        return oldTargets;
    }

    /// <summary>
    ///     Restarts the spriteBatch with the given parameters. Use sparingly.
    /// </summary>
    public static void Restart(this SpriteBatch spriteBatch, SpriteSortMode spriteSortMode = SpriteSortMode.Deferred, BlendState blendState = null,
        SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null,
        Matrix? transformMatrix = null, bool end = true)
    {
        if (end) spriteBatch.End();

        spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? Main.DefaultSamplerState, depthStencilState ?? DepthStencilState.None,
            rasterizerState ?? Main.Rasterizer, effect, transformMatrix ?? Main.Transform);
    }

    public static Color ToColor(this Vector3 vector3) => new(vector3.X * 255, vector3.Y * 255, vector3.Z * 255);

    public static Color Add(this Color color, Color color2)
    {
        Color sumColor = new(
            Math.Clamp(color.R + color2.R, 0, 255),
            Math.Clamp(color.G + color2.G, 0, 255),
            Math.Clamp(color.B + color2.B, 0, 255),
            Math.Clamp(color.A + color2.A, 0, 255)
        );

        return sumColor;
    }
}