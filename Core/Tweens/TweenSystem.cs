using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Tweens;

/// <summary>
///     A `ModSystem` that manages the lifecycle and updates of all active `ITween` objects.
///     It utilizes object pooling to minimize garbage collection.
/// </summary>
public class TweenSystem : ModSystem
{
    /// <summary>
    ///     Provides static access to the singleton instance of the `TweenSystem`.
    /// </summary>
    public static TweenSystem Instance { get; private set; }

    private readonly List<ITween> _activeTweens = new();
    private readonly ConcurrentDictionary<MemberInfo, Delegate> _setterCache = new();
    private readonly Dictionary<Type, Stack<ITween>> _pool = new();

    /// <summary>
    ///     Creates and registers a tween to animate a property or field to a static target value.
    ///     This is the recommended method for most use cases, often used via the `.TweenTo()` extension method.
    /// </summary>
    public static Tween<T> To<T, TInstance>(TInstance instance, Expression<Func<TInstance, T>> propertySelector, T to, int durationInTicks) where T : struct
    {
        var tween = Instance.GetTween<T>();
        Instance.InitializeTweenFromSelector(tween, instance, propertySelector, to, durationInTicks);
        Instance.Register(tween);
        return tween;
    }

    /// <summary>
    ///     Creates and registers a tween to animate a property or field to a dynamic target value that is re-evaluated each
    ///     frame.
    /// </summary>
    public static Tween<T> To<T, TInstance>(TInstance instance, Expression<Func<TInstance, T>> propertySelector, Func<T> dynamicTo, int durationInTicks) where T : struct
    {
        var tween = Instance.GetTween<T>();
        Instance.InitializeTweenFromSelector(tween, instance, propertySelector, dynamicTo, durationInTicks);
        Instance.Register(tween);
        return tween;
    }

    /// <summary>
    ///     Creates a tween without registering it for updates. This is intended for use with `Sequence` objects.
    /// </summary>
    public static Tween<T> Create<T, TInstance>(TInstance instance, Expression<Func<TInstance, T>> propertySelector, T to, int durationInTicks) where T : struct
    {
        var tween = Instance.GetTween<T>();
        Instance.InitializeTweenFromSelector(tween, instance, propertySelector, to, durationInTicks);
        return tween;
    }

    /// <summary>
    ///     Creates a tween with a dynamic target without registering it for updates. For use with `Sequence` objects.
    /// </summary>
    public static Tween<T> Create<T, TInstance>(TInstance instance, Expression<Func<TInstance, T>> propertySelector, Func<T> dynamicTo, int durationInTicks) where T : struct
    {
        var tween = Instance.GetTween<T>();
        Instance.InitializeTweenFromSelector(tween, instance, propertySelector, dynamicTo, durationInTicks);
        return tween;
    }

    /// <summary>
    ///     Retrieves a new, empty `Sequence` from the object pool.
    /// </summary>
    public static Sequence CreateSequence() => Instance.GetSequence();

    /// <summary>
    ///     Cancels all active tweens and sequences that have the specified tag.
    /// </summary>
    public static void CancelAllByTag(object tag) => Instance.CancelTweensByTag(tag);

    /// <summary>
    ///     Pauses all active tweens and sequences that have the specified tag.
    /// </summary>
    public static void PauseAllByTag(object tag) => Instance.PauseTweensByTag(tag);

    /// <summary>
    ///     Resumes all paused tweens and sequences that have the specified tag.
    /// </summary>
    public static void ResumeAllByTag(object tag) => Instance.ResumeTweensByTag(tag);

    public override void Load() => Instance = this;

    public override void Unload()
    {
        _activeTweens.Clear();
        _setterCache.Clear();
        _pool.Clear();
        Instance = null;
    }

    public override void PostUpdateEverything()
    {
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            ITween tween = _activeTweens[i];
            tween.Update();

            if (!tween.IsAlive)
            {
                _activeTweens.RemoveAt(i);
                Recycle(tween);
            }
        }
    }

    internal void Register(ITween tween)
    {
        if (tween != null && tween.IsAlive)
        {
            _activeTweens.Add(tween);
        }
    }

    internal void Recycle(ITween tween)
    {
        Type type = tween.GetType();
        if (!_pool.TryGetValue(type, out var stack))
        {
            stack = new Stack<ITween>();
            _pool[type] = stack;
        }
        tween.Recycle();
        stack.Push(tween);
    }

    internal Tween<T> GetTween<T>() where T : struct
    {
        Type type = typeof(Tween<T>);
        if (_pool.TryGetValue(type, out var stack) && stack.Count > 0)
            return (Tween<T>)stack.Pop();
        return new Tween<T>();
    }

    internal Sequence GetSequence()
    {
        Type type = typeof(Sequence);
        if (_pool.TryGetValue(type, out var stack) && stack.Count > 0)
            return (Sequence)stack.Pop();
        return new Sequence();
    }

    private void InitializeTweenFromSelector<T, TInstance>(Tween<T> tween, TInstance instance, Expression<Func<TInstance, T>> propertySelector, T to, int durationInTicks) where T : struct
    {
        var getterAndSetter = GetGetterAndSetter(instance, propertySelector);
        tween.Initialize(getterAndSetter.Setter, getterAndSetter.Getter(), to, durationInTicks);
        if (instance is Entity entity)
            tween.SetTarget(entity);
    }

    private void InitializeTweenFromSelector<T, TInstance>(Tween<T> tween, TInstance instance, Expression<Func<TInstance, T>> propertySelector, Func<T> dynamicTo, int durationInTicks) where T : struct
    {
        var getterAndSetter = GetGetterAndSetter(instance, propertySelector);
        tween.Initialize(getterAndSetter.Setter, getterAndSetter.Getter(), dynamicTo, durationInTicks);
        if (instance is Entity entity)
            tween.SetTarget(entity);
    }

    private (Func<T> Getter, Action<T> Setter) GetGetterAndSetter<T, TInstance>(TInstance instance, Expression<Func<TInstance, T>> propertySelector) where T : struct
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (propertySelector.Body is not MemberExpression memberSelector)
            throw new ArgumentException("Expression must be a simple member access, e.g. p => p.Center", nameof(propertySelector));

        var compiledSelector = propertySelector.Compile();
        var getter = (Func<T>)(() => compiledSelector(instance));

        if (!_setterCache.TryGetValue(memberSelector.Member, out Delegate genericSetterDelegate))
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");
            if (memberSelector.Member.DeclaringType is not null)
            {
                UnaryExpression castedInstance = Expression.Convert(instanceParam, memberSelector.Member.DeclaringType);
                MemberExpression memberAccess = Expression.MakeMemberAccess(castedInstance, memberSelector.Member);
                BinaryExpression assign = Expression.Assign(memberAccess, valueParam);
                var setterLambda = Expression.Lambda<Action<object, T>>(assign, instanceParam, valueParam);
                genericSetterDelegate = setterLambda.Compile();
            }
            _setterCache[memberSelector.Member] = genericSetterDelegate;
        }

        var genericSetter = (Action<object, T>)genericSetterDelegate;
        var setter = (Action<T>)(value => genericSetter(instance, value));

        return (getter, setter);
    }

    private void CancelTweensByTag(object tag)
    {
        if (tag == null) return;
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            if (tag.Equals(_activeTweens[i].Tag))
                _activeTweens[i].Cancel();
        }
    }

    private void PauseTweensByTag(object tag)
    {
        if (tag == null) return;
        foreach (ITween tween in _activeTweens)
            if (tag.Equals(tween.Tag))
                tween.Pause();
    }

    private void ResumeTweensByTag(object tag)
    {
        if (tag == null) return;
        foreach (ITween tween in _activeTweens)
            if (tag.Equals(tween.Tag))
                tween.Resume();
    }
}