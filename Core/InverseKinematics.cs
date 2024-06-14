using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Experiments.Core;

public class Bone(Vector2 start, float length, float angle = 0, float strokeWeight = 1f)
{
    public Vector2 Start = start;
    public Vector2 End => Start + new Vector2(float.Cos(angle), float.Sin(angle)) * length;

    /// <summary>
    ///     Draws a line from the <see cref="Start" /> to <see cref="End" /> of the segment.
    /// </summary>
    /// <param name="texture"><see cref="TextureAssets.MagicPixel" /> by default</param>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <param name="spriteFacingUpwards">
    ///     Whether the sprite is facing upwards or not. Used to offset the rotation by
    ///     <see cref="MathF.PI" />/2 radians.
    /// </param>
    /// <remarks>
    ///     This method uses the <see cref="Graphics.DrawLine" /> method to draw the curve line segment by segment.
    /// </remarks>
    public void Draw(Texture2D texture = null, Color? color = null, bool spriteFacingUpwards = true)
    {
        Graphics.DrawLine(Start, End, texture, color, spriteFacingUpwards, strokeWeight);
    }

    public void Follow(Vector2 target)
    {
        Vector2 dir = Start.DirectionTo(target);
        angle = dir.ToRotation();

        dir *= -length;
        Start = target + dir;
    }
}

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
public class Limb
{
    private readonly Bone[] _segments;

    public bool FixedBase;
    public Vector2 BasePosition;

    private readonly Texture2D _texture;

    private readonly bool _spriteFacingUpwards;
    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global

    /// <summary>
    ///     Initializes a chain of <see cref="Bone" />s.
    ///     The segments follow each other using
    ///     <a href="https://en.wikipedia.org/wiki/Inverse_kinematics">inverse kinematics</a>.
    /// </summary>
    /// <param name="size">Number of segments</param>
    /// <param name="basePosition">Position of the base segment</param>
    /// <param name="fixedBase">Whether the base of the chain can move around or not</param>
    /// <param name="segmentLength">Length of each segment</param>
    /// <param name="lengthStep">Length to decrease each consecutive segment by</param>
    /// <param name="strokeWeight">Stroke weight of each segment</param>
    /// <param name="strokeWeightStep">Stroke weight to decrease each consecutive segment by</param>
    /// <param name="texture">Texture to use for the segments, <see cref="TextureAssets.MagicPixel" /> by default</param>
    public Limb(int size, Vector2 basePosition, bool fixedBase, float segmentLength, float lengthStep = 0, float strokeWeight = 1f,
        float strokeWeightStep = 0, Texture2D texture = null, bool spriteFacingUpwards = true)
    {
        BasePosition = basePosition;
        FixedBase = fixedBase;
        _texture = texture;
        _spriteFacingUpwards = spriteFacingUpwards;

        _segments = new Bone[size];
        _segments[0] = new Bone(basePosition, segmentLength, strokeWeight: strokeWeight);

        for (int i = 1; i < _segments.Length; i++)
            _segments[i] = new Bone(_segments[i - 1].End, segmentLength - lengthStep * i, strokeWeight: strokeWeight - strokeWeightStep * i);
    }

    /// <summary>
    ///     Set the target for the head segment to follow.
    /// </summary>
    /// <param name="target">The target to follow</param>
    /// <seealso cref="Bone.Follow" />
    public void Follow(Vector2 target)
    {
        Bone end = _segments[^1];
        end.Follow(target);
    }

    /// <summary>
    ///     Makes all the segments present in the chain follow their consecutive segments.
    /// </summary>
    public void Update()
    {
        for (int i = _segments.Length - 2; i >= 0; i--)
            _segments[i].Follow(_segments[i + 1].Start);

        if (FixedBase)
            _segments[0].Start = BasePosition;

        for (int i = 1; i < _segments.Length; i++)
            _segments[i].Start = _segments[i - 1].End;

        BasePosition = _segments[0].Start;
    }

    /// <summary>
    ///     Draws the segments present in the chain.
    /// </summary>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <seealso cref="Bone.Draw" />
    public void Draw(Color? color = null)
    {
        foreach (Bone segment in _segments) segment.Draw(_texture, color, _spriteFacingUpwards);
    }
}