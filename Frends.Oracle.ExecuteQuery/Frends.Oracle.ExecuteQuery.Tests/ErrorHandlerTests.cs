using System;
using Frends.Oracle.ExecuteQuery.Definitions;
using Frends.Oracle.ExecuteQuery.Helpers;
using NUnit.Framework;

namespace Frends.Oracle.ExecuteQuery.Tests;

[TestFixture]
public class ErrorHandlerTests
{
    [Test]
    public void HandleReturnsErrorDetailsAndConfiguredMessage()
    {
        var exception = new InvalidOperationException("The query failed.");
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Oracle query execution failed",
        };

        var result = exception.Handle(options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Oracle query execution failed: The query failed."));
        Assert.That(result.Error.Message, Is.EqualTo(result.Message));
        Assert.That(result.Error.AdditionalInfo, Is.SameAs(exception));
    }
}
