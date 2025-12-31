using NUnit.Framework;
using System.Threading;
using Frends.Oracle.ExecuteProcedure.Definitions;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using NUnit.Framework.Legacy;
using System.Collections.Generic;
using System;

namespace Frends.Oracle.ExecuteProcedure.Tests;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

[TestFixture]
class UnitTests
{
    private static Input _input;
    private static Options _options;
    private static IContainer oracleContainer;

    private readonly static string schema = "test_user";
    private readonly static string _connectionString = $"Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = {schema}; Password={schema};";
    private readonly static string _connectionStringSys = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = sys; Password=mysecurepassword; DBA PRIVILEGE=SYSDBA";
    private readonly static string _proc = "unitestproc";

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _input = new Input
        {
            ConnectionString = _connectionString,
        };

        _options = new Options
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
            .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy(150, s => s.WithTimeout(TimeSpan.FromMinutes(10))))
            .WithReuse(true)
            .Build();

        await oracleContainer.StartAsync();

        Helpers.TestConnectionBeforeRunningTests(_connectionStringSys);

        await using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.CreateTestUser(con);
        con.Close();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.DropTestUser(con);
        con.Close();

        if (oracleContainer != null)
        {
            await oracleContainer.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        using var con = new OracleConnection(_connectionStringSys);
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

    [TearDown]
    public void TearDown()
    {
        using var con = new OracleConnection(_connectionStringSys);
        con.Open();

        try
        {
            Helpers.DropTestTable(con);
            Helpers.DropProcedure(con, _proc);
        }
        finally
        {
            con.Close();
        }
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureJSONString()
    {
        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.JSONString
        };

        var result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureXmlString()
    {
        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.XmlString
        };

        var result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureXDocument()
    {
        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.XDocument
        };

        var result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureParameters()
    {
        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.Parameters
        };

        var result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureAffectedRows()
    {
        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.AffectedRows
        };

        var result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "name",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "address",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, _options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

    [Test]
    public async Task ExecuteProcedure_ProcedureWithoutBindByName()
    {
        var options = new Options
        {
            BindParameterByName = false,
            ThrowErrorOnFailure = true
        };

        _input.Command = @$"
create or replace procedure {_proc} (name in varchar2, address out varchar2) as
begin
  select address into address from workers where name = name;
end {_proc};";
        _input.CommandType = OracleCommandType.Command;

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.JSONString
        };

        var result = await Oracle.ExecuteProcedure(_input, output, options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);

        _input.Command = _proc;
        _input.CommandType = OracleCommandType.StoredProcedure;
        _input.Parameters = new InputParameter[]
        {
            new InputParameter
            {
                Name = "n",
                Value = "risto",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        output.OutputParameters = new OutputParameter[]
        {
            new OutputParameter
            {
                Name = "a",
                DataType = ProcedureParameterType.Varchar2,
                Size = 255
            }
        };

        result = await Oracle.ExecuteProcedure(_input, output, options, new CancellationToken());
        ClassicAssert.IsTrue(result.Success);
    }

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
            ConnectionString = _connectionString
        };

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.Parameters,
            OutputParameters = new[]
            {
                new OutputParameter
                {
                    Name = "v_doc_rev",
                    DataType = ProcedureParameterType.NVarchar2,
                    Size = 50
                }
            }
        };

        var result = await Oracle.ExecuteProcedure(input, output, _options, new CancellationToken());

        ClassicAssert.IsTrue(result.Success);

        var returnedParams = (Dictionary<string, object>)result.Output;
        ClassicAssert.IsTrue(returnedParams.ContainsKey("v_doc_rev"));
        ClassicAssert.AreEqual("OK_TEST", returnedParams["v_doc_rev"]?.ToString());
    }

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
            ConnectionString = _connectionString
        };

        var output = new Output
        {
            DataReturnType = OracleCommandReturnType.Parameters,
            OutputParameters = new[]
            {
                new OutputParameter { Name = "v_file_data", DataType = ProcedureParameterType.Blob, Size = 90000000 },
                new OutputParameter { Name = "v_file_type", DataType = ProcedureParameterType.Varchar2, Size = 100 },
                new OutputParameter { Name = "v_doc_title", DataType = ProcedureParameterType.Varchar2, Size = 200 },
                new OutputParameter { Name = "v_err_msg", DataType = ProcedureParameterType.Varchar2, Size = 1000 },
                new OutputParameter { Name = "v_optional_field", DataType = ProcedureParameterType.Varchar2, Size = 100 }
            }
        };

        var options = new Options
        {
            BindParameterByName = true,
            TimeoutSeconds = 30,
            ThrowErrorOnFailure = false
        };

        var result = await Oracle.ExecuteProcedure(input, output, options, new CancellationToken());

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
            "BLOB should be converted to byte[] or string, not OracleBlob object!"
        );
    }
}