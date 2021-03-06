﻿using System;
using System.Runtime.CompilerServices;

namespace CoreMVVM
{
    /// <summary>
    /// Thrown when attempting to read a property that has not yet been initialized.
    /// </summary>
    public sealed class NotInitializedException : Exception
    {
        public NotInitializedException()
        {
            Message = "Class, instance, or property not initialized.";
            MemberName = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NotInitializedException"/> class,
        /// with the specified message.
        /// </summary>
        /// <param name="memberName">The name of the uninitialized member.</param>
        public NotInitializedException([CallerMemberName] string? memberName = null)
        {
            Message = $"Attempted to read uninitialized member '{memberName}'.";
            MemberName = memberName ?? string.Empty;
        }

        public NotInitializedException(string message, [CallerMemberName] string? memberName = null)
        {
            Message = message;
            MemberName = memberName ?? string.Empty;
        }

        public NotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
            Message = message;
            MemberName = string.Empty;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// Gets the name of the unitialized member.
        /// </summary>
        public string MemberName { get; }
    }
}