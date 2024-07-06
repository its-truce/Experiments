using Experiments.Core;
using Experiments.Core.Pathfinding;
using Experiments.Core.Pixelation;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Snake : ModNPC
{
    private Pathfinder _finder;
    private Limb _limb;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(30);
        NPC.damage = 10;
        NPC.lifeMax = 100;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _finder = new Pathfinder(NPC.Center.ToTileCoordinates().ToPoint16(), NPC.Target().Center.ToTileCoordinates().ToPoint16());

        _limb = new Limb(12, NPC.Center, false, 10, -4, 2, -2);
    }

    public override void AI()
    {
        _finder.Update();
        _finder.SetTarget(NPC.Target().MountedCenter.ToTileCoordinates().ToPoint16());

        NPC.Center = _finder.Path[^1].Position.ToWorldCoordinates();

        _limb.Follow(NPC.Center);
        _limb.Update();

        PixelationSystem.Instance.AddRenderAction(() => { _limb.Draw(); });
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return false;
    }
}