﻿#nullable enable
using System;
using System.Threading;
using ReLogic.Threading;

// CREDIT: https://github.com/terraria-catalyst/nitrate-mod/blob/master/src/Nitrate/API/Threading/FasterParallel.cs
namespace Experiments.Utils;

/// <summary>
///     A faster reimplementation of <see cref="FastParallel"/>.
/// </summary>
/// <seealso cref="FastParallel"/>
public static class FasterParallel
{
    /// <summary>
    ///     A faster reimplementation of <see cref="FastParallel.For"/>.
    /// </summary>
    public static void For(int fromInclusive, int toExclusive, ParallelForAction callback, object? context = null)
    {
        var rangeLength = toExclusive - fromInclusive;

        if (rangeLength == 0)
        {
            return;
        }

        var initialCount = Math.Min(Math.Max(1, Environment.ProcessorCount - 1), rangeLength);
        var rangeLengthPerTask = rangeLength / initialCount;
        var remainder = rangeLength % initialCount;
        CountdownEvent countdownEvent = new(initialCount);
        var currentRangeStart = toExclusive;

        for (var i = initialCount - 1; i >= 0; --i)
        {
            var rangeLengthForTask = rangeLengthPerTask;

            if (i < remainder)
            {
                rangeLengthForTask++;
            }

            currentRangeStart -= rangeLengthForTask;
            var rangeStart = currentRangeStart;
            var rangeEnd = rangeStart + rangeLengthForTask;
            RangeTask rangeTask = new(callback, rangeStart, rangeEnd, context, countdownEvent);

            if (i < 1)
            {
                InvokeTask(rangeTask);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(InvokeTask, rangeTask);
            }
        }

        countdownEvent.Wait();
    }

    private static void InvokeTask(object? context)
    {
        (context as RangeTask)?.Invoke();
    }

    private class RangeTask(ParallelForAction action, int fromInclusive, int toExclusive, object? context, CountdownEvent countdownEvent)
    {
        public void Invoke()
        {
            try
            {
                if (fromInclusive == toExclusive)
                {
                    return;
                }

                action(fromInclusive, toExclusive, context);
            }
            finally
            {
                countdownEvent.Signal();
            }
        }
    }
}