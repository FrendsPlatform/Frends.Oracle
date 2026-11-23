// <copyright file="UnitTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.Oracle.ExecuteProcedure.Definitions;
using global::Oracle.ManagedDataAccess.Client;
using NUnit.Framework;
using NUnit.Framework.Legacy;

/// <summary>
/// Integration tests for executing Oracle stored procedures.
/// </summary>
[TestFixture]
internal class UnitTests
{
    private const string Schema = "test_user";

    private const string ConnectionString =
        $"Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = {Schema}; Password={Schema};";

    private const string ConnectionStringSys =
        "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = sys; Password=mysecurepassword; DBA PRIVILEGE=SYSDBA";

    private const string Proc = "unitestproc";

    private static Input input;
    private static Options options;
    private static IContainer oracleContainer;

    /// <summary>
    /// Starts Oracle and prepares the test user.
    /// </summary>
    /// <returns>A task representing the setup operation.</returns>
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        input = new Input
        {
            ConnectionString = ConnectionString,
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            TimeoutSeconds = 30,
            BindParameterByName = true,
        };

        oracleContainer = new ContainerBuilder()
            .WithImage("container-registry.oracle.com/database/express:18.4.0-xe")
            .WithName("Frends.Oracle.ExecuteProcedure.Tests")
            .WithPortBinding(1521, 1521)
            .WithEnvironment("ORACLE_PWD", "mysecurepassword")
            .WithEnvironment("ORACLE_CHARACTERSET", "AL32UTF8")

            // We need to wait for the container to be ready healthy.
            // Health checks are running every 5 seconds, and it takes ~ 8 minutes to get the container ready.
            // It gives us 120 retries + 30 as a margin. We will stop waiting after 10 minutes if it's still not ready.
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilContainerIsHealthy(150, s => s.WithTimeout(TimeSpan.FromMinutes(10))))
            .WithReuse(true)
            .Build();

        await oracleContainer.StartAsync();

        Helpers.TestConnectionBeforeRunningTests(ConnectionStringSys);

        await using var con = new OracleConnection(ConnectionStringSys);
        con.Open();
        Helpers.CreateTestUser(con);
        con.Close();
    }

    /// <summary>
    /// Removes the test user and stops the container.
    /// </summary>
    /// <returns>A task representing the teardown operation.</returns>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await using var con = new OracleConnection(ConnectionStringSys);
        con.Open();
        Helpers.DropTestUser(con);
        con.Close();

        if (oracleContainer != null)
        {
            await oracleContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates and populates a fresh test table.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        input = new Input
        {
            ConnectionString = ConnectionString,
        };

        using var con = new OracleConnection(ConnectionStringSys);
        con.Open();

        try
        {
            Helpers.CreateTestTable(con);
            Helpers.InsertTestData(con);
        }
        finally
        {
            con.Close();
        }
    }

    /// <summary>
    /// Removes the test table and procedure.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        using var con = new OracleConnection(ConnectionStringSys);
        con.Open();

        try
        {
            Helpers.DropTestTable(con);
            Helpers.DropProcedure(con, Proc);
        }
        finally
        {
            con.Close();
        }
    }

    /// <summary>
    /// Verifies JSON output for an Oracle procedure.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureJSONString()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.JSONString;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies XML string output for an Oracle procedure.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureXmlString()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.XmlString;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies XDocument output for an Oracle procedure.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureXDocument()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.XDocument;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies output parameter results for an Oracle procedure.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureParameters()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.Parameters;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies affected-row results for an Oracle procedure.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureAffectedRows()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.AffectedRows;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies parameter binding by position when binding by name is disabled.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ProcedureWithoutBindByName()
    {
        var options = new Options
        {
            BindParameterByName = false,
            ThrowErrorOnFailure = true,
        };

        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.JSONString;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "n",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        input.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "a",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
    }

    /// <summary>
    /// Verifies that returned output parameters contain their expected values.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ReturnsCorrectOutputParameterValue()
    {
        var input = new Input
        {
            Command = @"
            BEGIN
                :v_doc_rev := 'OK_TEST';
            END;",
            CommandType = OracleCommandType.Command,
            Parameters = Array.Empty<InputParameter>(),
            DataReturnType = OracleCommandReturnType.Parameters,
            OutputParameters = new[]
            {
                new OutputParameter
                {
                    Name = "v_doc_rev",
                    DataType = ProcedureParameterType.NVarchar2,
                    Size = 50,
                },
            },
            ConnectionString = ConnectionString,
        };

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Success);

        var returnedParams = (Dictionary<string, object>)result.Output;
        ClassicAssert.IsTrue(returnedParams.ContainsKey("v_doc_rev"));
        ClassicAssert.AreEqual("OK_TEST", returnedParams["v_doc_rev"]?.ToString());
    }

    /// <summary>
    /// Verifies Oracle values of multiple data types, including nulls.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ViewDocument_AllTypesAndNulls()
    {
        var input = new Input
        {
            Command = @"
            BEGIN
                -- Simulate View_Document outputs
                :v_file_data := UTL_RAW.CAST_TO_RAW('PDF_BINARY_DATA_HERE');
                :v_file_type := 'PDF';
                :v_doc_title := 'Sales_Report.pdf';
                :v_err_msg := NULL;

                -- Additional test: some NULLs
                :v_optional_field := NULL;
            END;",
            CommandType = OracleCommandType.Command,
            Parameters = Array.Empty<InputParameter>(),
            DataReturnType = OracleCommandReturnType.Parameters,
            OutputParameters = new[]
            {
                new OutputParameter
                {
                    Name = "v_file_data",
                    DataType = ProcedureParameterType.Blob,
                    Size = 90000000,
                },
                new OutputParameter
                {
                    Name = "v_file_type",
                    DataType = ProcedureParameterType.Varchar2,
                    Size = 100,
                },
                new OutputParameter
                {
                    Name = "v_doc_title",
                    DataType = ProcedureParameterType.Varchar2,
                    Size = 200,
                },
                new OutputParameter
                {
                    Name = "v_err_msg",
                    DataType = ProcedureParameterType.Varchar2,
                    Size = 1000,
                },
                new OutputParameter
                {
                    Name = "v_optional_field",
                    DataType = ProcedureParameterType.Varchar2,
                    Size = 100,
                },
            },
            ConnectionString = ConnectionString,
        };

        var options = new Options
        {
            BindParameterByName = true,
            TimeoutSeconds = 30,
            ThrowErrorOnFailure = false,
        };

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Success, "Should execute successfully");
        var returnedParams = (Dictionary<string, object>)result.Output;

        ClassicAssert.AreEqual("PDF", returnedParams["v_file_type"]?.ToString());
        ClassicAssert.AreEqual("Sales_Report.pdf", returnedParams["v_doc_title"]?.ToString());

        ClassicAssert.IsNull(returnedParams["v_err_msg"], "err_msg should be NULL");
        ClassicAssert.IsNull(returnedParams["v_optional_field"], "optional field should be NULL");

        var blobValue = returnedParams["v_file_data"];
        ClassicAssert.IsNotNull(blobValue, "BLOB should have value");

        ClassicAssert.IsFalse(
            blobValue.GetType().FullName.Contains("OracleBlob"),
            "BLOB should be converted to byte[] or string, not OracleBlob object!");
    }

    /// <summary>
    /// Verifies that large BLOB values are read completely.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_LargeBlob_IsReadCompletely()
    {
        const int expectedMinSize = 250000; // > 80 KB

        input.Command = @"
            DECLARE
                l_blob BLOB;
                l_raw  RAW(32767);
            BEGIN
                DBMS_LOB.CREATETEMPORARY(l_blob, TRUE);

                FOR i IN 1..8 LOOP
                    l_raw := UTL_RAW.CAST_TO_RAW(RPAD('A', 32000, 'A'));
                    DBMS_LOB.WRITEAPPEND(l_blob, UTL_RAW.LENGTH(l_raw), l_raw);
                END LOOP;

                :out_blob := l_blob;
            END;";

        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.Parameters;
        input.OutputParameters = new[]
            {
                new OutputParameter
                {
                    Name = "out_blob",
                    DataType = ProcedureParameterType.Blob,
                    Size = 1000000,
                },
            };

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        var dict = (Dictionary<string, object>)result.Output;
        var base64 = dict["out_blob"] as string;

        ClassicAssert.IsNotNull(base64);

        byte[] decoded = Convert.FromBase64String(base64);

        ClassicAssert.Greater(decoded.Length, expectedMinSize);
        ClassicAssert.IsTrue(decoded.All(b => b == (byte)'A'));
    }

    /// <summary>
    /// Compares execution times with connection pool clearing enabled and disabled.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_ClearConnectionPools_SpeedUp_MultipleCalls()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.AffectedRows;
        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        Assert.That(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new()
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };
        input.OutputParameters = new OutputParameter[]
        {
            new()
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };
        const int numberOfCalls = 100;

        options.ClearConnectionPools = false;
        long totalTimeForFalse = 0;

        for (int i = 0; i < numberOfCalls; i++)
        {
            var sw = Stopwatch.StartNew();
            await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
            sw.Stop();
            totalTimeForFalse += sw.ElapsedMilliseconds;
        }

        options.ClearConnectionPools = true;
        long totalTimeForTrue = 0;

        for (int i = 0; i < numberOfCalls; i++)
        {
            var sw = Stopwatch.StartNew();
            await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
            sw.Stop();
            totalTimeForTrue += sw.ElapsedMilliseconds;
        }

        var averageTimeForFalse = totalTimeForFalse / numberOfCalls;
        var averageTimeForTrue = totalTimeForTrue / numberOfCalls;
        TestContext.WriteLine($"Average execution time for ClearConnectionPools set to True = {averageTimeForTrue} ms");
        TestContext.WriteLine(
            $"Average execution time for ClearConnectionPools set to False = {averageTimeForFalse} ms");
        TestContext.WriteLine(
            $"Total difference for {numberOfCalls} calls = {totalTimeForTrue - totalTimeForFalse} ms");
        Assert.That(totalTimeForFalse, Is.LessThan(totalTimeForTrue));

        // last call to clear connection pools for TearDown to work
        options.ClearConnectionPools = true;
        await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
    }

    /// <summary>
    /// Verifies that the connection remains open when configured not to close.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_CloseConnection_SetToFalse_ShouldLeaveConnectionOpen()
    {
        input.Command = @$"
create or replace procedure {Proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {Proc};";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.AffectedRows;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        Assert.That(result.Success);

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new InputParameter[]
        {
            new()
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };
        input.OutputParameters = new OutputParameter[]
        {
            new()
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        options.CloseConnection = false;
        await Oracle.ExecuteProcedure(input, options, CancellationToken.None);

        await using var con = new OracleConnection(ConnectionStringSys);
        await con.OpenAsync();
        var ex = Assert.Throws<OracleException>(() => Helpers.DropTestUser(con));
        Assert.That(ex, Is.Not.Null);
        Assert.That(
            ex.Message,
            Does.Contain("ORA-01940"),
            "Expected ORA-01940 error indicating user is still in use.");

        // last call to close connection for TearDown to work
        options.CloseConnection = true;
        await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
    }

    /// <summary>
    /// Verifies that a cached connection recovers after an Oracle restart.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_WithCachedConnection_RecoversAfterOracleRestart()
    {
        using var setupCon = new OracleConnection(ConnectionStringSys);
        setupCon.Open();
        Helpers.CreateTestProcedure(setupCon, Proc);
        setupCon.Close();

        input.Command = Proc;
        input.CommandType = OracleCommandType.StoredProcedure;
        input.Parameters = new[]
        {
        new InputParameter
        {
            Name = "p_name",
            Value = "risto",
            DataType = ProcedureParameterType.Varchar2,
            Size = 255,
        },
        };

        input.DataReturnType = OracleCommandReturnType.Parameters;
        input.OutputParameters = new[]
        {
            new OutputParameter
            {
                Name = "p_address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255,
            },
        };

        options.CloseConnection = false;
        options.ClearConnectionPools = false;
        options.ThrowErrorOnFailure = true;

        var result1 = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result1.Success);
        var dict1 = (Dictionary<string, object>)result1.Output;
        ClassicAssert.AreEqual("haapatie 9", dict1["p_address"]);

        // Restart Oracle to invalidate cached connection
        await oracleContainer.StopAsync();
        await Task.Delay(3000);
        await oracleContainer.StartAsync();
        Helpers.TestConnectionBeforeRunningTests(ConnectionStringSys);

        using (var recreateCon = new OracleConnection(ConnectionStringSys))
        {
            recreateCon.Open();
            Helpers.CreateTestUser(recreateCon);
            Helpers.CreateTestTable(recreateCon);
            Helpers.InsertTestData(recreateCon);
            Helpers.CreateTestProcedure(recreateCon, Proc);
            recreateCon.Close();
        }

        var result2 = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result2.Success);
        var dict2 = (Dictionary<string, object>)result2.Output;
        ClassicAssert.AreEqual("haapatie 9", dict2["p_address"]);

        var cleanupOptions = new Options
        {
            ThrowErrorOnFailure = false,
            CloseConnection = true,
            ClearConnectionPools = true,
        };

        await Oracle.ExecuteProcedure(input, cleanupOptions, CancellationToken.None);
    }

    /// <summary>
    /// Verifies that invalid SQL returns an error when throwing is disabled.
    /// </summary>
    /// <returns>A task representing the test operation.</returns>
    [Test]
    public async Task ExecuteProcedure_WithInvalidSql_ReturnsErrorWhenThrowErrorOnFailureFalse()
    {
        input.Command = "INVALID SQL SYNTAX HERE";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.AffectedRows;

        options.ThrowErrorOnFailure = false;

        var result = await Oracle.ExecuteProcedure(input, options, CancellationToken.None);

        ClassicAssert.IsFalse(result.Success);
        ClassicAssert.IsNotNull(result.Output);
        ClassicAssert.IsTrue(result.Output.Contains("ORA-"));
    }

    /// <summary>
    /// Verifies that invalid SQL throws when configured to throw on failure.
    /// </summary>
    [Test]
    public void ExecuteProcedure_WithInvalidSql_ThrowsExceptionWhenThrowErrorOnFailureTrue()
    {
        input.Command = "INVALID SQL SYNTAX HERE";
        input.CommandType = OracleCommandType.Command;

        input.DataReturnType = OracleCommandReturnType.AffectedRows;

        options.ThrowErrorOnFailure = true;
        options.ErrorMessageOnFailure = "Procedure execution failed";

        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await Oracle.ExecuteProcedure(input, options, CancellationToken.None));

        ClassicAssert.IsNotNull(ex);
        ClassicAssert.AreEqual("Procedure execution failed", ex.Message);
        ClassicAssert.IsNotNull(ex.InnerException);
    }
}
