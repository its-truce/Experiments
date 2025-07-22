namespace Experiments.Core.Tweens;

/// <summary>
///     Defines a contract for tweenable objects, allowing them to be managed by the `TweenSystem`.
/// </summary>
public interface ITween
{
    /// <summary>
    ///     Gets a value indicating if the tween is active. When this is false, the `TweenSystem` will recycle the object.
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    ///     Gets or sets a user-defined object for grouping and identification.
    /// </summary>
    object Tag { get; set; }

    /// <summary>
    ///     Advances the tween's state by one tick.
    /// </summary>
    void Update();

    /// <summary>
    ///     Immediately stops the tween and marks it for recycling.
    /// </summary>
    void Cancel();

    /// <summary>
    ///     Pauses the tween's progress.
    /// </summary>
    void Pause();

    /// <summary>
    ///     Resumes a paused tween.
    /// </summary>
    void Resume();

    /// <summary>
    ///     Resets the object's state to its default values for object pooling.
    /// </summary>
    void Recycle();
}