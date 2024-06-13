using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Experiments.Core;

/// <summary>
///     A parametric curve which spans the given control points.
/// </summary>
/// <param name="controlPoints">Points which define the curve</param>
/// <remarks>
///     For more information, see: <a href="https://wikipedia.org/wiki/Bezier_curve">this page</a>.
/// </remarks>
public class BezierCurve(params Vector2[] controlPoints)
{
    /// <summary>
    ///     Gets or sets the control point at the specified index.
    /// </summary>
    /// <param name="x">The index of the control point.</param>
    /// <returns>The control point at the specified index.</returns>
    public Vector2 this[int x]
    {
        get => controlPoints[x];
        set => controlPoints[x] = value;
    }

    /// <summary>
    ///     Generates a specified number of points along the Bézier curve.
    /// </summary>
    /// <param name="numberOfPoints">The number of points to generate along the Bézier curve. Must be at least 2.</param>
    /// <returns>An enumerable collection of <see cref="Vector2" /> points along the Bézier curve.</returns>
    /// <seealso cref="GetPoint" />
    public IEnumerable<Vector2> GetPoints(int numberOfPoints)
    {
        numberOfPoints = Math.Max(numberOfPoints, 2);
        var points = new Vector2[numberOfPoints];

        float step = 1f / (numberOfPoints - 1);

        for (int i = 0; i < numberOfPoints; i++) points[i] = GetPoint(i * step);

        return points;
    }

    /// <summary>
    ///     Calculates the approximate length of the Bézier curve using a specified number of points.
    /// </summary>
    /// <param name="numberOfPoints">
    ///     The number of points to generate along the Bézier curve for length approximation. Must be
    ///     at least 2.
    /// </param>
    /// <returns>The approximate length of the Bézier curve.</returns>
    public float GetLength(int numberOfPoints)
    {
        numberOfPoints = Math.Max(numberOfPoints, 2);
        Vector2[] points = GetPoints(numberOfPoints).ToArray();
        float length = 0;

        for (int i = 0; i < numberOfPoints - 1; i++) length += Vector2.Distance(points[i], points[i + 1]);

        return length;
    }

    /// <summary>
    ///     Draws the Bézier curve using a specified number of points.
    /// </summary>
    /// <param name="numberOfPoints">The number of points to generate along the Bézier curve for drawing. Must be at least 2.</param>
    /// <param name="texture"><see cref="TextureAssets.MagicPixel" /> by default</param>
    /// <param name="color"><see cref="Color.White" /> by default</param>
    /// <param name="spriteFacingUpwards">
    ///     Whether the sprite is facing upwards or not. Used to offset the rotation by
    ///     <see cref="MathF.PI" />/2 radians.
    /// </param>
    /// <param name="thickness">Thickness of the line</param>
    /// <remarks>
    ///     This method uses the <see cref="Graphics.DrawLine" /> method to draw the curve line segment by segment.
    /// </remarks>
    public void Draw(int numberOfPoints, Texture2D texture = null, Color? color = null, bool spriteFacingUpwards = true, float thickness = 1)
    {
        numberOfPoints = Math.Max(numberOfPoints, 2);
        Vector2[] points = GetPoints(numberOfPoints).ToArray();

        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];

            Graphics.DrawLine(start, end, texture, color, spriteFacingUpwards, thickness);
        }
    }

    /// <summary>
    ///     Returns a point along the Bézier curve at a specified parameter using De Casteljau's algorithm.
    /// </summary>
    /// <param name="t">The parameter along the curve, typically between 0 and 1.</param>
    /// <returns>The calculated point on the Bézier curve corresponding to the parameter <paramref name="t" />.</returns>
    /// <remarks>
    ///     De Casteljau's algorithm iteratively interpolates between the control points to converge on a single point on the
    ///     curve.
    ///     For more information, see: <a href="https://wikipedia.org/wiki/De_Casteljau%27s_algorithm">this page</a>.
    /// </remarks>
    private Vector2 GetPoint(float t)
    {
        t = Math.Clamp(t, 0, 1);

        int n = controlPoints.Length;
        var tempPoints = new Vector2[n];
        Array.Copy(controlPoints, tempPoints, n);

        for (int k = 1; k < n; k++)
        for (int i = 0; i < n - k; i++)
            tempPoints[i] = Vector2.Lerp(tempPoints[i], tempPoints[i + 1], t);

        return tempPoints[0];
    }
}