using Experiments.Core.InverseKinematics;
using Experiments.Core.Pathfinding;
using Experiments.Core.Pixelation;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class RandomLengthLimb : Limb
{
    public RandomLengthLimb(int size, Vector2 basePosition, bool fixedBase, float segmentLength, float lengthStep = 0, float strokeWeight = 1f,
        uint strokeWeightRange = 0, Texture2D texture = null, bool spriteFacingUpwards = true)
    {
        BasePosition = basePosition;
        FixedBase = fixedBase;
        Texture = texture;
        SpriteFacingUpwards = spriteFacingUpwards;

        Segments = new Bone[size];
        Segments[0] = new Bone(basePosition, segmentLength, strokeWeight: strokeWeight);

        for (int i = 1; i < Segments.Length; i++)
        {
            Segments[i] = new Bone(Segments[i - 1].End, segmentLength - lengthStep * i,
                strokeWeight: strokeWeight - Main.rand.Next((int)-strokeWeightRange, (int)strokeWeightRange + 1));
        }
    }
}

public class Snake : ModNPC
{
    private int _currentWaypoint;
    private Pathfinder _finder;
    private RandomLengthLimb _limb;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(8);
        NPC.damage = 10;
        NPC.lifeMax = 100;
        NPC.timeLeft = int.MaxValue;
        NPC.noGravity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.TargetClosest();
        _finder = new Pathfinder(NPC.Center.ToTileCoordinates().ToPoint16(), NPC.Target().Center.ToTileCoordinates().ToPoint16(), true);
        _limb = new RandomLengthLimb(4, NPC.Center, false, 25, -4, 10, 4);
    }

    public override void AI()
    {
        if (_finder?.Path is null) return;

        PixelationSystem.Instance.AddRenderAction(() => { _limb?.Draw(); });

        _finder = _finder.SetTarget(NPC.Target().MountedCenter.ToTileCoordinates().ToPoint16());

        if (_finder.Done && _currentWaypoint < _finder.Path.Count)
        {
            Vector2 position = _finder.Path[_currentWaypoint].Position.ToWorldCoordinates();
            NPC.velocity = NPC.DirectionTo(position) * 12;

            if (NPC.DistanceSQ(position) < 10 * 10)
                _currentWaypoint++;
        }
        else
        {
            _finder = _finder.Done ? _finder : _finder.SetStart(NPC.Center.ToTileCoordinates().ToPoint16());
            _finder.Update(30);
            NPC.velocity = Vector2.Zero;
        }

        _limb.Follow(NPC.Center);
        _limb.Update();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        _finder = _finder.SetStart(NPC.Center.ToTileCoordinates16());
        _currentWaypoint = 0;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        _finder?.Draw();

        return false;
    }

    public override bool? CanFallThroughPlatforms() => true;
}