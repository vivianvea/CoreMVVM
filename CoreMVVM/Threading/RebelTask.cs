﻿using CoreMVVM.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoreMVVM.Threading
{
    /// <summary>
    /// A task that does not continue on the captured context as default.
    /// </summary>
    [AsyncMethodBuilder(typeof(RebelTaskMethodBuilder))]
    public readonly struct RebelTask : IEquatable<RebelTask>
    {
        private readonly Task _task;

        #region Constructors

        public static RebelTask CompletedTask { get; } = new RebelTask(Task.CompletedTask);

        public RebelTask(Task task)
        {
            _task = task;
        }

        public static RebelTask<TResult> FromResult<TResult>(TResult result)
        {
            return new RebelTask<TResult>(result);
        }

        #endregion Constructors

        /// <summary>
        /// Returns a task configured to continue on the captured context.
        /// </summary>
        public ConfiguredTaskAwaitable ContinueOnCapturedContext()
        {
            Task task = GetTaskOrComplete();
            return task.ConfigureAwait(true);
        }

        /// <summary>
        /// Returns the awaiter of this task, configured to not continue on the captured context.
        /// </summary>
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            Task task = GetTaskOrComplete();
            return task.ConfigureAwait(false).GetAwaiter();
        }

        private Task GetTaskOrComplete()
        {
            return _task ?? Task.CompletedTask;
        }

        #region Static utilities

        [DebuggerStepThrough]
        public static RebelTask Delay(int millisecondsDelay)
        {
            Task result = Task.Delay(millisecondsDelay);

            return new RebelTask(result);
        }

        [DebuggerStepThrough]
        public static RebelTask Delay(TimeSpan delay)
        {
            Task result = Task.Delay(delay);

            return new RebelTask(result);
        }

        [DebuggerStepThrough]
        public static RebelTask Run(Action action)
        {
            Task result = Task.Run(action);

            return new RebelTask(result);
        }

        [DebuggerStepThrough]
        public static RebelTask<TResult> Run<TResult>(Func<TResult> action)
        {
            Task<TResult> result = Task.Run(action);

            return new RebelTask<TResult>(result);
        }

        [DebuggerStepThrough]
        public static RebelTask Run(Func<Task> action)
        {
            Task result = action();
            return new RebelTask(result);
        }

        [DebuggerStepThrough]
        public static RebelTask<TResult> Run<TResult>(Func<Task<TResult>> action)
        {
            Task<TResult> result = action();
            return new RebelTask<TResult>(result);
        }

        [DebuggerStepThrough]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Throw single aggregate exception")]
        public static RebelTask WhenAll(IEnumerable<RebelTask> tasks)
        {
            IEnumerable<Task> wrappedTasks = tasks.Select(t => t._task);
            Task result = Task.WhenAll(wrappedTasks);

            return new RebelTask(result);
        }

        #endregion Static utilities

        #region Operators

        public override bool Equals(object obj)
        {
            if (!(obj is RebelTask other))
                return false;

            return Equals(other);
        }

        public bool Equals(RebelTask other)
        {
            return _task == other._task;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 7) + GetTaskOrComplete().GetHashCode();

            return hash;
        }

        public static bool operator ==(RebelTask left, RebelTask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RebelTask left, RebelTask right)
        {
            return !left.Equals(right);
        }

        public static implicit operator RebelTask(Task task)
        {
            return new RebelTask(task);
        }

        #endregion Operators
    }
}