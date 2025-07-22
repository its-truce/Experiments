using System;
using System.Linq.Expressions;

namespace Experiments.Core.Tweens;

/// <summary>
///     Provides convenient extension methods for creating tweens directly on object instances.
/// </summary>
public static class TweenExtensions
{
    /// <summary>
    ///     Creates and registers a tween to animate a property or field of this object to a static target value.
    ///     This is the most common and concise way to start a tween.
    /// </summary>
    public static Tween<T> TweenTo<T, TInstance>(this TInstance instance, Expression<Func<TInstance, T>> propertySelector, T to, int durationInTicks) where T : struct => TweenSystem.To(instance, propertySelector, to, durationInTicks);

    /// <summary>
    ///     Creates and registers a tween to animate a property of this object to a dynamic target value that is re-evaluated
    ///     each frame.
    /// </summary>
    public static Tween<T> TweenTo<T, TInstance>(this TInstance instance, Expression<Func<TInstance, T>> propertySelector, Func<T> dynamicTo, int durationInTicks) where T : struct =>
        TweenSystem.To(instance, propertySelector, dynamicTo, durationInTicks);

    /// <summary>
    ///     Creates a tween for this object with a static target without starting it, for use with Sequences.
    /// </summary>
    public static Tween<T> CreateTween<T, TInstance>(this TInstance instance, Expression<Func<TInstance, T>> propertySelector, T to, int durationInTicks) where T : struct => TweenSystem.Create(instance, propertySelector, to, durationInTicks);

    /// <summary>
    ///     Creates a tween for this object with a dynamic target without starting it, for use with Sequences.
    /// </summary>
    public static Tween<T> CreateTween<T, TInstance>(this TInstance instance, Expression<Func<TInstance, T>> propertySelector, Func<T> dynamicTo, int durationInTicks) where T : struct =>
        TweenSystem.Create(instance, propertySelector, dynamicTo, durationInTicks);
}