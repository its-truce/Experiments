#nullable enable
using System;
using System.Threading;
using ReLogic.Threading;

// CREDIT: https://github.com/terraria-catalyst/nitrate-mod/blob/master/src/Nitrate/API/Threading/FasterParallel.cs
namespace Experiments.Utils;

/// <summary>
///     A faster reimplementation of <see cref="FastParallel" />.
/// </summary>
/// <seealso cref="FastParallel" />
public static class FasterParallel
{
    /// <summary>
    ///     A faster reimplementation of <see cref="FastParallel.For" />.
    /// </summary>
    public static void For(int fromInclusive, int toExclusive, ParallelForAction callback, object? context = null)
    {
        int rangeLength = toExclusive - fromInclusive;

        if (rangeLength == 0) return;

        int initialCount = Math.Min(Math.Max(1, Environment.ProcessorCount - 1), rangeLength);
        int rangeLengthPerTask = rangeLength / initialCount;
        int remainder = rangeLength % initialCount;
        CountdownEvent countdownEvent = new(initialCount);
        int currentRangeStart = toExclusive;

        for (int i = initialCount - 1; i >= 0; --i)
        {
            int rangeLengthForTask = rangeLengthPerTask;

            if (i < remainder) rangeLengthForTask++;

            currentRangeStart -= rangeLengthForTask;
            int rangeStart = currentRangeStart;
            int rangeEnd = rangeStart + rangeLengthForTask;
            RangeTask rangeTask = new(callback, rangeStart, rangeEnd, context, countdownEvent);

            if (i < 1)
                InvokeTask(rangeTask);
            else
                ThreadPool.QueueUserWorkItem(InvokeTask, rangeTask);
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
                if (fromInclusive == toExclusive) return;

                action(fromInclusive, toExclusive, context);
            }
            finally
            {
                countdownEvent.Signal();
            }
        }
    }
}