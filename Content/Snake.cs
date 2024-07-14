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

    private int _frameCounter;
    private Limb _limb;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(14);
        NPC.damage = 10;
        NPC.lifeMax = 100;
        NPC.timeLeft = int.MaxValue;
        NPC.noGravity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.TargetClosest();
        _finder = new Pathfinder(NPC.Center.ToTileCoordinates().ToPoint16(), NPC.Target().Center.ToTileCoordinates().ToPoint16());
        _limb = new Limb(12, NPC.Center, false, 10, -4, 2, -1.5f);
    }

    public override void AI()
    {
        NPC.TargetClosest();

        _finder = _finder.SetTarget(NPC.Target().MountedCenter.ToTileCoordinates().ToPoint16());
        _finder.Update();

        _limb.Follow(NPC.Center);
        _limb.Update();

        int frames = _finder.Path.Count;
        if (_finder.Done)
        {
            Main.NewText("hi");
            if (_frameCounter < frames)
                _frameCounter++;

            NPC.velocity = NPC.DirectionTo(_finder.Path[_frameCounter - 1].Position.ToWorldCoordinates()) * 8;
        }
        else
            NPC.velocity = Vector2.Zero;

        PixelationSystem.Instance.AddRenderAction(() =>
        {
            _finder.Draw();
            _limb.Draw();
        });
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        _finder = _finder.SetStart(NPC.Center.ToTileCoordinates().ToPoint16());
        _frameCounter = 1;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
}