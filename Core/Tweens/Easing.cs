using System;

namespace Experiments.Core.Tweens;

/// <summary>
///     Defines the different easing functions available to control the rate of change of a tween.
/// </summary>
public enum Easing
{
    Linear,
    QuadraticIn, QuadraticOut, QuadraticInOut,
    CubicIn, CubicOut, CubicInOut,
    QuarticIn, QuarticOut, QuarticInOut,
    QuinticIn, QuinticOut, QuinticInOut,
    SineIn, SineOut, SineInOut,
    ExpoIn, ExpoOut, ExpoInOut,
    CircIn, CircOut, CircInOut,
    BackIn, BackOut, BackInOut
}

/// <summary>
///     A static class containing the mathematical formulas for all easing functions.
///     Based on the equations from https://easings.net/
/// </summary>
internal static class EasingFunctions
{
    /// <summary>
    ///     Calculates the eased value for a given progress and easing type.
    /// </summary>
    /// <param name="easing">The type of easing to apply.</param>
    /// <param name="t">The raw progress of the tween, from 0.0 to 1.0.</param>
    /// <returns>The transformed, "eased" progress, typically from 0.0 to 1.0.</returns>
    public static float Ease(Easing easing, float t)
    {
        switch (easing)
        {
            case Easing.Linear: return t;
            case Easing.QuadraticIn: return t * t;
            case Easing.QuadraticOut: return 1 - (1 - t) * (1 - t);
            case Easing.QuadraticInOut: return t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2;
            case Easing.CubicIn: return t * t * t;
            case Easing.CubicOut: return 1 - (float)Math.Pow(1 - t, 3);
            case Easing.CubicInOut: return t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
            case Easing.QuarticIn: return t * t * t * t;
            case Easing.QuarticOut: return 1 - (float)Math.Pow(1 - t, 4);
            case Easing.QuarticInOut: return t < 0.5f ? 8 * t * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 4) / 2;
            case Easing.QuinticIn: return t * t * t * t * t;
            case Easing.QuinticOut: return 1 - (float)Math.Pow(1 - t, 5);
            case Easing.QuinticInOut: return t < 0.5f ? 16 * t * t * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 5) / 2;
            case Easing.SineIn: return 1 - (float)Math.Cos(t * Math.PI / 2);
            case Easing.SineOut: return (float)Math.Sin(t * Math.PI / 2);
            case Easing.SineInOut: return -((float)Math.Cos(Math.PI * t) - 1) / 2;
            case Easing.ExpoIn: return t == 0 ? 0 : (float)Math.Pow(2, 10 * t - 10);
            case Easing.ExpoOut: return Math.Abs(t - 1) < 0.01 ? 1 : 1 - (float)Math.Pow(2, -10 * t);
            case Easing.ExpoInOut: return t == 0 ? 0 : Math.Abs(t - 1) < 0.01 ? 1 : t < 0.5f ? (float)Math.Pow(2, 20 * t - 10) / 2 : (2 - (float)Math.Pow(2, -20 * t + 10)) / 2;
            case Easing.CircIn: return 1 - (float)Math.Sqrt(1 - Math.Pow(t, 2));
            case Easing.CircOut: return (float)Math.Sqrt(1 - Math.Pow(t - 1, 2));
            case Easing.CircInOut: return t < 0.5f ? (1 - (float)Math.Sqrt(1 - Math.Pow(2 * t, 2))) / 2 : ((float)Math.Sqrt(1 - Math.Pow(-2 * t + 2, 2)) + 1) / 2;
            case Easing.BackIn:
                const float c1 = 1.70158f;
                const float c3 = c1 + 1;
                return c3 * t * t * t - c1 * t * t;
            case Easing.BackOut:
                const float c1Out = 1.70158f;
                const float c3Out = c1Out + 1;
                return 1 + c3Out * (float)Math.Pow(t - 1, 3) + c1Out * (float)Math.Pow(t - 1, 2);
            case Easing.BackInOut:
                const float c1InOut = 1.70158f;
                const float c2InOut = c1InOut * 1.525f;
                return t < 0.5
                    ? (float)Math.Pow(2 * t, 2) * ((c2InOut + 1) * 2 * t - c2InOut) / 2
                    : ((float)Math.Pow(2 * t - 2, 2) * ((c2InOut + 1) * (t * 2 - 2) + c2InOut) + 2) / 2;
            default:
                return t;
        }
    }
}