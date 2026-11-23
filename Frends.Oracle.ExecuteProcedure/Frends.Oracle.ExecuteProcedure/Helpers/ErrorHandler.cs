// <copyright file="ErrorHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Helpers;

using System.Runtime.ExceptionServices;
using Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Handles task execution failures according to the task options.
/// </summary>
internal static class ErrorHandler
{
    /// <summary>
    /// Converts an exception into a failed result or rethrows it based on the task options.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="options">Task options that control failure handling.</param>
    /// <param name="throwCanceled">Whether to rethrow cancellation exceptions.</param>
    /// <returns>A failed result when the exception is handled instead of rethrown.</returns>
    internal static Result Handle(this Exception exception, Options options, bool throwCanceled = true)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(options);

        ThrowIfCanceled(exception, throwCanceled);
        if (options.ThrowErrorOnFailure)
        {
            ThrowBaseException(exception, options.ErrorMessageOnFailure);
        }

        return ReturnResult(exception, options.ErrorMessageOnFailure);
    }

    private static void ThrowIfCanceled(Exception exception, bool throwCanceled = true)
    {
        if (throwCanceled && exception is OperationCanceledException)
        {
            ThrowBaseException(exception);
        }
    }

    private static void ThrowBaseException(Exception exception, string customMessage = null)
    {
        if (string.IsNullOrEmpty(customMessage))
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        throw new Exception(customMessage, exception);
    }

    private static Result ReturnResult(Exception exception, string customMessage = null)
    {
        var errorMessage = string.IsNullOrEmpty(customMessage)
            ? exception.Message
            : $"{customMessage}: {exception.Message}";

        return new Result(false, null, errorMessage);
    }
}
