namespace Experiments.Core.Tweens;

/// <summary>
///     Defines the behavior of a tween when it loops.
/// </summary>
public enum LoopType
{
    /// <summary>
    ///     The tween restarts from the beginning value upon completing a loop. (A -> B, A -> B, ...)
    /// </summary>
    Restart,

    /// <summary>
    ///     The tween reverses direction upon completing a loop, animating back and forth. (A -> B, B -> A, ...)
    /// </summary>
    Yoyo
}