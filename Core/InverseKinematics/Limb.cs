﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Experiments.Core.InverseKinematics;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
public class Limb
{
    protected Bone[] Segments;

    public bool FixedBase;
    public Vector2 BasePosition;
    public Vector2 HeadPosition;

    protected Texture2D Texture;

    protected bool SpriteFacingUpwards;
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
        Texture = texture;
        SpriteFacingUpwards = spriteFacingUpwards;

        Segments = new Bone[size];
        Segments[0] = new Bone(basePosition, segmentLength, strokeWeight: strokeWeight);

        for (int i = 1; i < Segments.Length; i++)
            Segments[i] = new Bone(Segments[i - 1].End, segmentLength - lengthStep * i, strokeWeight: strokeWeight - strokeWeightStep * i);

        HeadPosition = Segments[^1].End;
    }

    protected Limb()
    {
    }

    /// <summary>
    ///     Set the target for the head segment to follow.
    /// </summary>
    /// <param name="target">The target to follow</param>
    /// <seealso cref="Bone.Follow" />
    public void Follow(Vector2 target)
    {
        Bone end = Segments[^1];
        end.Follow(target);
    }

    /// <summary>
    ///     Makes all the segments present in the chain follow their consecutive segments.
    /// </summary>
    public void Update()
    {
        for (int i = Segments.Length - 2; i >= 0; i--)
            Segments[i].Follow(Segments[i + 1].Start);

        if (FixedBase)
            Segments[0].Start = BasePosition;

        for (int i = 1; i < Segments.Length; i++)
            Segments[i].Start = Segments[i - 1].End;

        BasePosition = Segments[0].Start;
        HeadPosition = Segments[^1].End;
    }

    /// <summary>
    ///     Draws the segments present in the chain.
    /// </summary>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <seealso cref="Bone.Draw" />
    public void Draw(Color? color = null)
    {
        foreach (Bone segment in Segments) segment.Draw(Texture, color, SpriteFacingUpwards);
    }
}