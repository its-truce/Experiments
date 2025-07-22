using System.Collections.Generic;
using System.Linq;

namespace Experiments.Core.Tweens;

/// <summary>
///     Represents a collection of `ITween` objects that are executed in a defined order.
///     Supports playing tweens sequentially with `.Append()` or in parallel with `.Join()`.
///     This object is pooled; create one using `TweenSystem.CreateSequence()`.
/// </summary>
public class Sequence : ITween
{
    private readonly List<List<ITween>> _steps = new();
    private int _currentStepIndex;
    private bool _isPaused;

    public bool IsAlive { get; private set; }
    public object Tag { get; set; }

    internal Sequence() { }

    /// <summary>
    ///     Appends a new step to the sequence containing a single tween.
    ///     This tween will start after all previous steps have completed.
    ///     The provided tween should be created with `TweenSystem.Create()`.
    /// </summary>
    public Sequence Append(ITween tween)
    {
        _steps.Add([tween]);
        return this;
    }

    /// <summary>
    ///     Adds a tween to the most recently added step, allowing for parallel execution.
    ///     If no steps exist, this behaves like `Append`.
    ///     The provided tween should be created with `TweenSystem.Create()`.
    /// </summary>
    public Sequence Join(ITween tween)
    {
        if (_steps.Count == 0)
            return Append(tween);

        _steps.Last().Add(tween);
        return this;
    }

    /// <summary>
    ///     Appends a delay to the sequence as a new step.
    /// </summary>
    /// <param name="delayInTicks">The duration of the delay in ticks.</param>
    public Sequence AppendInterval(int delayInTicks)
    {
        var delayTween = TweenSystem.Instance.GetTween<float>();
        delayTween.Initialize(null, 0f, 1f, delayInTicks);
        return Append(delayTween);
    }

    /// <summary>
    ///     Registers the sequence with the `TweenSystem` to begin execution.
    /// </summary>
    public void Play()
    {
        if (IsAlive || _steps.Count == 0) return;
        IsAlive = true;
        _currentStepIndex = 0;
        TweenSystem.Instance.Register(this);
    }

    public void Update()
    {
        if (!IsAlive || _isPaused) return;

        var currentStep = _steps[_currentStepIndex];

        for (int i = currentStep.Count - 1; i >= 0; i--)
        {
            currentStep[i].Update();
        }

        if (currentStep.All(t => !t.IsAlive))
        {
            AdvanceToNextStep();
        }
    }

    private void AdvanceToNextStep()
    {
        _currentStepIndex++;
        if (_currentStepIndex >= _steps.Count)
        {
            IsAlive = false;
        }
    }

    public void Recycle()
    {
        foreach (var step in _steps)
        {
            foreach (ITween tween in step)
            {
                TweenSystem.Instance.Recycle(tween);
            }
        }
        _steps.Clear();
        _currentStepIndex = -1;
        IsAlive = false;
        Tag = null;
        _isPaused = false;
    }

    public void Cancel()
    {
        if (!IsAlive) return;
        IsAlive = false;
    }

    public void Pause()
    {
        if (!IsAlive || _isPaused) return;
        _isPaused = true;
        if (_currentStepIndex < _steps.Count)
        {
            foreach (ITween tween in _steps[_currentStepIndex])
                tween.Pause();
        }
    }

    public void Resume()
    {
        if (!IsAlive || !_isPaused) return;
        _isPaused = false;
        if (_currentStepIndex < _steps.Count)
        {
            foreach (ITween tween in _steps[_currentStepIndex])
                tween.Resume();
        }
    }
}