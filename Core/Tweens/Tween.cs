using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace Experiments.Core.Tweens;

/// <summary>
///     Represents an animation of a value of type <typeparamref name="T" /> from a start point to an end point.
///     This object is pooled; create it via `TweenSystem` static methods.
/// </summary>
/// <typeparam name="T">The type of value to animate (must be a struct).</typeparam>
public class Tween<T> : ITween where T : struct
{
    private enum TweenState { Delayed, Running }

    private TweenState _state;
    private int _durationInTicks;
    private int _delayInTicks;
    private int _loopDelayInTicks;
    private int _currentLoopDelay;
    private int _elapsedTimeInTicks;

    private Action<T> _setter;
    private T _from;
    private T _to;
    private T _initialFrom;
    private T _initialTo;
    private Func<T> _dynamicTo;

    private Easing _easing;
    private Func<float, float> _customEasingFunc;
    private Action<bool> _onComplete;

    private Entity _targetEntity;
    private bool _isPaused;
    private int _loopCount;
    private LoopType _loopType;

    public bool IsAlive { get; private set; }
    public object Tag { get; set; }

    internal Tween() { }

    internal void Initialize(Action<T> setter, T from, T to, int durationInTicks)
    {
        _setter = setter;
        _from = from;
        _to = to;
        _initialFrom = from;
        _initialTo = to;
        _durationInTicks = durationInTicks > 0 ? durationInTicks : 1;
        IsAlive = true;
    }

    internal void Initialize(Action<T> setter, T from, Func<T> dynamicTo, int durationInTicks)
    {
        Initialize(setter, from, default(T), durationInTicks);
        _dynamicTo = dynamicTo;
    }

    /// <summary>
    ///     Assigns a predefined easing function from the `Easing` enum.
    /// </summary>
    public Tween<T> SetEase(Easing easing)
    {
        _easing = easing;
        _customEasingFunc = null;
        return this;
    }

    /// <summary>
    ///     Assigns a custom easing function via a delegate or lambda expression.
    /// </summary>
    /// <param name="customEase">A function that takes progress (0-1) and returns an adjusted progress value.</param>
    public Tween<T> SetEase(Func<float, float> customEase)
    {
        _customEasingFunc = customEase;
        return this;
    }

    /// <summary>
    ///     Assigns a tag to this tween for group management.
    /// </summary>
    public Tween<T> SetTag(object tag)
    {
        Tag = tag;
        return this;
    }

    /// <summary>
    ///     Delays the start of the tween by a specified number of ticks. This delay only occurs once.
    /// </summary>
    public Tween<T> Delay(int delayInTicks)
    {
        _delayInTicks = delayInTicks;
        _state = TweenState.Delayed;
        return this;
    }

    /// <summary>
    ///     Sets a delay that occurs after each loop iteration completes, before the next one begins.
    /// </summary>
    /// <param name="delayInTicks">The duration of the delay between loops, in ticks.</param>
    public Tween<T> SetLoopDelay(int delayInTicks)
    {
        _loopDelayInTicks = delayInTicks;
        return this;
    }

    /// <summary>
    ///     Configures the tween to loop a specific number of times.
    ///     Note: `LoopType.Yoyo` is not supported for tweens with a dynamic target.
    /// </summary>
    /// <param name="count">The number of times to loop. Use -1 for infinite looping.</param>
    /// <param name="loopType">The behavior of the loop (`Restart` or `Yoyo`).</param>
    public Tween<T> SetLoops(int count, LoopType loopType = LoopType.Restart)
    {
        if (_dynamicTo != null && loopType == LoopType.Yoyo)
            throw new NotSupportedException("LoopType.Yoyo is not supported for tweens with a dynamic target. Use LoopType.Restart instead.");

        _loopCount = count;
        _loopType = loopType;
        return this;
    }

    /// <summary>
    ///     Assigns a callback to execute upon the tween's completion.
    ///     The `bool` parameter of the action is `true` if completed successfully, `false` if cancelled.
    /// </summary>
    public Tween<T> OnComplete(Action<bool> onComplete)
    {
        _onComplete = onComplete;
        return this;
    }

    /// <summary>
    ///     Links the tween's lifecycle to a `Terraria.Entity`, causing it to auto-cancel if the entity becomes inactive.
    ///     This is typically handled automatically by the `TweenSystem`.
    /// </summary>
    public Tween<T> SetTarget(Entity target)
    {
        _targetEntity = target;
        return this;
    }

    public void Update()
    {
        if (!IsAlive || _isPaused) return;

        if (_targetEntity != null && !_targetEntity.active)
        {
            Finish(false);
            return;
        }

        if (_currentLoopDelay > 0)
        {
            _currentLoopDelay--;
            return;
        }

        if (_state == TweenState.Delayed)
        {
            if (_delayInTicks > 0)
            {
                _delayInTicks--;
                return;
            }
            _state = TweenState.Running;
        }

        if (_dynamicTo != null)
        {
            _to = _dynamicTo();
        }

        _elapsedTimeInTicks++;
        float progress = MathHelper.Clamp((float)_elapsedTimeInTicks / _durationInTicks, 0f, 1f);

        float easedProgress = _customEasingFunc != null
            ? _customEasingFunc(progress)
            : EasingFunctions.Ease(_easing, progress);

        _setter?.Invoke(Interpolate(easedProgress));

        if (_elapsedTimeInTicks >= _durationInTicks)
        {
            if (_loopCount == -1 || _loopCount > 0)
            {
                if (_loopCount > 0) _loopCount--;
                _elapsedTimeInTicks = 0;
                _currentLoopDelay = _loopDelayInTicks;

                if (_loopType == LoopType.Yoyo)
                {
                    (_from, _to) = (_to, _from);
                }
                else // Restart
                {
                    _from = _initialFrom;
                    _to = _initialTo;
                }
            }
            else
            {
                Finish(true);
            }
        }
    }

    private T Interpolate(float progress)
    {
        if (typeof(T) == typeof(float)) return (T)(object)MathHelper.Lerp((float)(object)_from, (float)(object)_to, progress);
        if (typeof(T) == typeof(double)) return (T)(object)MathHelper.Lerp((float)(double)(object)_from, (float)(double)(object)_to, progress);
        if (typeof(T) == typeof(int)) return (T)(object)(int)Math.Round(MathHelper.Lerp((int)(object)_from, (int)(object)_to, progress));
        if (typeof(T) == typeof(Vector2)) return (T)(object)Vector2.Lerp((Vector2)(object)_from, (Vector2)(object)_to, progress);
        if (typeof(T) == typeof(Vector3)) return (T)(object)Vector3.Lerp((Vector3)(object)_from, (Vector3)(object)_to, progress);
        if (typeof(T) == typeof(Color)) return (T)(object)Color.Lerp((Color)(object)_from, (Color)(object)_to, progress);
        if (typeof(T) == typeof(Rectangle))
        {
            var ra = (Rectangle)(object)_from;
            var rb = (Rectangle)(object)_to;
            return (T)(object)new Rectangle((int)MathHelper.Lerp(ra.X, rb.X, progress), (int)MathHelper.Lerp(ra.Y, rb.Y, progress), (int)MathHelper.Lerp(ra.Width, rb.Width, progress), (int)MathHelper.Lerp(ra.Height, rb.Height, progress));
        }
        throw new NotSupportedException($"Tweening not supported for type {typeof(T).Name}");
    }

    public void Cancel() => Finish(false);
    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;

    private void Finish(bool success)
    {
        if (!IsAlive) return;
        IsAlive = false;
        _onComplete?.Invoke(success);
    }


    public void Recycle()
    {
        _setter = null;
        IsAlive = false;
        _durationInTicks = 0;
        _delayInTicks = 0;
        _elapsedTimeInTicks = 0;
        _loopDelayInTicks = 0;
        _currentLoopDelay = 0;
        _state = TweenState.Delayed;
        _easing = default;
        _customEasingFunc = null;
        _dynamicTo = null;
        _onComplete = null;
        _targetEntity = null;
        Tag = null;
        _isPaused = false;
        _loopCount = 0;
        _loopType = default;
    }
}