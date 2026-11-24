using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.Oracle.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;

namespace Frends.Oracle.ExecuteQuery.Tests;

/// <summary>
/// Oracle test database is started in Docker with Testcontainers; Docker must be running.
/// </summary>
[TestFixture]
public class TestClass
{
    /// <summary>
    /// Connection string for an Oracle database.
    /// </summary>
    private static readonly string Schema = "test_user";

    private static OracleContainer container;
    private static string connectionString;
    private static string connectionStringSys;

    /// <summary>
    /// Global variables.
    /// </summary>
    private static Input input;

    private static Connection connection;

    private static Options options;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        container = new OracleBuilder("gvenzl/oracle-xe:18-slim-faststart")
            .Build();
        await container.StartAsync();

        var dataSource = $"(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = {container.Hostname})(PORT = {container.GetMappedPublicPort(1521)}))(CONNECT_DATA = (SERVICE_NAME = XEPDB1)))";
        connectionString = $"Data Source = {dataSource}; User Id = {Schema}; Password = {Schema};";
        connectionStringSys = $"Data Source = {dataSource}; User Id = sys; Password = oracle; DBA PRIVILEGE=SYSDBA";

        using var con = new OracleConnection(connectionStringSys);
        con.Open();
        Helpers.CreateTestUser(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (container != null)
            await container.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        input = new Input
        {
            ExecuteType = ExecuteTypes.Auto,
        };

        connection = new Connection
        {
            ConnectionString = connectionString,
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            BindParameterByName = true,
            OracleIsolationLevel = TransactionIsolationLevel.Default,
            TimeoutSeconds = 30,
        };

        using var con = new OracleConnection(connectionStringSys);
        con.Open();
        Helpers.CreateTestTable(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }

    [TearDown]
    public void TearDown()
    {
        using var con = new OracleConnection(connectionStringSys);
        con.Open();
        Helpers.DropTestTable(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }

    [Test]
    public async Task ExecuteQuery_ParseNumberWithTooHighPrecision()
    {
        input.Query = "insert into workers (id, first_name) values (1, 'John')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        input.Query = "SELECT CAST(1.12345678901234567890123456789012 AS NUMBER) as big_num from workers";
        options.EnableSafeNumericMapping = true;
        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task ExecuteQuery_ThrowErrorWhenInvalidDate()
    {
        input.Query = "insert into workers (id, first_name) values (1, 'John')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Oracle query execution failed";
        input.Query = "SELECT TO_DATE('01-JAN-0001 BC', 'DD-MON-YYYY BC') as invalid_date from workers";
        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Oracle query execution failed"));
        Assert.That(result.Error.Message, Does.Contain("couldn't be properly formatted"));
        Assert.That(result.Error.AdditionalInfo, Is.Not.Null);
    }

    [Test]
    public async Task ExecuteQuery_ThrowErrorWhenInvalidNumber()
    {
        input.Query = "insert into workers (id, first_name) values (1, 'John')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        options.ThrowErrorOnFailure = false;
        input.Query = "SELECT CAST(12345678901234567890123456789012345678 AS NUMBER) as big_num from workers";
        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message.Contains("couldn't be properly formatted"), Is.True);
    }

    [Test]
    public async Task ExecuteQuery_InsertWithParameters()
    {
        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values (:id, :name, 'Meikï¿½lï¿½inen')";
        input.Parameters = new[]
        {
            new QueryParameter
            {
                Name = "name",
                Value = "Matti",
                DataType = QueryParameterType.Varchar2,
            },
            new QueryParameter
            {
                Name = "id",
                Value = 3,
                DataType = QueryParameterType.Int32,
            },
        };

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        input.Query = "select first_name from workers where id = 3";

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual(typeof(JArray), result.Output.GetType());
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_WithAllValues()
    {
        input.Query = "insert " +
                      "into workers values (1, 'Matti', 'Meikï¿½lï¿½inen', DATE '2022-04-12')";
        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_InsertMultipleRowsIntoTable()
    {
        input.Query = "insert all " +
                      "into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen') " +
                      "into workers (id, first_name, last_name) values (2, 'Teppo', 'Teikï¿½lï¿½inen') " +
                      "select * from dual";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(2, (int)result.Output["AffectedRows"]);

        input.Query = "select * from workers " +
                      "where id = :id";
        input.Parameters = new[]
        {
            new QueryParameter
            {
                Name = "id",
                Value = 1,
                DataType = QueryParameterType.Int32,
            },
        };

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
        ClassicAssert.AreEqual("Meikï¿½lï¿½inen", (string)result.Output[0]["LAST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_InsertMultipleRowsWithMultipleParameters()
    {
        input.Query = "insert all " +
                      "into workers (id, first_name, last_name) values (:id1, :fname1, :lname1) " +
                      "into workers (id, first_name, last_name) values (:id2, :fname2, :lname2) " +
                      "select * from dual";
        input.Parameters = new[]
        {
            new QueryParameter
            {
                Name = "id1",
                Value = 1,
                DataType = QueryParameterType.Int32,
            },
            new QueryParameter
            {
                Name = "fname1",
                Value = "Matti",
                DataType = QueryParameterType.Varchar2,
            },
            new QueryParameter
            {
                Name = "lname1",
                Value = "Meikï¿½lï¿½inen",
                DataType = QueryParameterType.Varchar2,
            },
            new QueryParameter
            {
                Name = "id2",
                Value = 2,
                DataType = QueryParameterType.Int32,
            },
            new QueryParameter
            {
                Name = "fname2",
                Value = "Teppo",
                DataType = QueryParameterType.Varchar2,
            },
            new QueryParameter
            {
                Name = "lname2",
                Value = "Teikï¿½lï¿½inen",
                DataType = QueryParameterType.Varchar2,
            },
        };

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(2, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_Update()
    {
        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        input.Query = "update workers " +
                      "set first_name = 'Saija', " +
                      "last_name = 'Saijalainen' " +
                      "where id = 1";

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        input.Query = "select first_name from workers where id = 1";

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual("Saija", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_SelectWithNonExistingRow()
    {
        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        input.Query = "select first_name from workers where id = 2";

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual("[]", result.Output.ToString());
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task ExecuteQuery_WithoutThrowErrorOnFailure()
    {
        options.ThrowErrorOnFailure = false;

        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values ('Matti', 1, 'Meikï¿½lï¿½inen')";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(false, result.Success);
        ClassicAssert.True(result.Message.Contains("ORA-01722"));
    }

    [Test]
    public async Task ExecuteQuery_ThatThrowsException()
    {
        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values ('Matti', 1, 'Meikï¿½lï¿½inen')";

        var error = await Assert.CatchAsync<Exception>(async () =>
            await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None));
        ClassicAssert.True(error!.Message.Contains("ORA-01722"));
    }

    [Test]
    public async Task ExecuteQuery_ErrorTesting()
    {
        input.Query = "SELECT NOW();";
        var error = await Assert.CatchAsync<Exception>(async () =>
            await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None));
        ClassicAssert.True(error!.Message.Contains("ORA-00923"));
    }

    [Test]
    public async Task ExecuteQuery_InsertWithBindParameterByNameFalse()
    {
        options.BindParameterByName = false;
        input.Query = "insert " +
                      "into workers (id, first_name, last_name) values (:p, :p, :p)";
        input.Parameters = new[]
        {
            new QueryParameter
            {
                Name = "p",
                Value = 1,
                DataType = QueryParameterType.Int32,
            },
            new QueryParameter
            {
                Name = "p",
                Value = "Matti",
                DataType = QueryParameterType.Varchar2,
            },
            new QueryParameter
            {
                Name = "p",
                Value = "Meikï¿½lï¿½inen",
                DataType = QueryParameterType.Varchar2,
            },
        };

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        input.Query = "select first_name, last_name from workers where id = 1";

        result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
        ClassicAssert.AreEqual("Meikï¿½lï¿½inen", (string)result.Output[0]["LAST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_NonQueryExecutionType_Explicit()
    {
        input.ExecuteType = ExecuteTypes.NonQuery;
        input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_ExecuteReaderExecutionType_Explicit()
    {
        input.ExecuteType = ExecuteTypes.NonQuery;
        input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        input.ExecuteType = ExecuteTypes.ExecuteReader;
        input.Query = "select first_name from workers where id = 1";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(typeof(JArray), result.Output.GetType());
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_ScalarExecutionType_Count()
    {
        input.ExecuteType = ExecuteTypes.NonQuery;
        input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        input.ExecuteType = ExecuteTypes.Scalar;
        input.Query = "select count(*) from workers";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)(decimal)result.Output);
    }

    [Test]
    public async Task ExecuteQuery_ScalarExecutionType_SingleValue()
    {
        input.ExecuteType = ExecuteTypes.NonQuery;
        input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meikï¿½lï¿½inen')";
        await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        input.ExecuteType = ExecuteTypes.Scalar;
        input.Query = "select first_name from workers where id = 1";

        var result = await Oracle.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual("Matti", (string)result.Output);
    }
}
