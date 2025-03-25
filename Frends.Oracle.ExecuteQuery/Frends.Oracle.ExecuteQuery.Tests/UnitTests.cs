using NUnit.Framework;
using System;
using System.Threading;
using Frends.Oracle.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;

namespace Frends.Oracle.ExecuteQuery.Tests;

/// <summary>
/// Oracle test database is needed to run these test. 
/// To run tests run docker-compose up -d
/// You will need free oracle account to download image
/// </summary>
[TestFixture]
class TestClass
{
    /// <summary>
    /// Connection string for Oracle database.
    /// </summary>
    private readonly static string _schema = "test_user";
    private readonly static string _connectionString = $"Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 51521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = {_schema}; Password={_schema};";
    private readonly static string _connectionStringSys = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 51521))(CONNECT_DATA = (SERVICE_NAME = XEPDB1))); User Id = sys; Password=mysecurepassword; DBA PRIVILEGE=SYSDBA";

    /// <summary>
    /// Global variables.
    /// </summary>
    private static Input _input;
    private static Options _options;

    #region OneTimeSetup&TearDown
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Helpers.TestConnectionBeforeRunningTests(_connectionStringSys);

        using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.CreateTestUser(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.DropTestUser(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }
    #endregion OneTimeSetup&TearDown
    #region Setup&TearDown
    [SetUp]
    public void Setup()
    {
        _input = new Input
        {
            ConnectionString = _connectionString,
            ExecuteType = ExecuteTypes.Auto
        };

        _options = new Options
        {
            ThrowErrorOnFailure = true,
            BindParameterByName = true,
            OracleIsolationLevel = TransactionIsolationLevel.Default,
            TimeoutSeconds = 30
        };

        using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.CreateTestTable(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }

    [TearDown]
    public void TearDown()
    {
        using var con = new OracleConnection(_connectionStringSys);
        con.Open();
        Helpers.DropTestTable(con);
        con.Close();
        if (con.State == System.Data.ConnectionState.Open)
            con.Close();
    }
    #endregion Setup&TearDown

    [Test]
    public async Task ExecuteQuery_InsertWithParameters()
    {
        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values (:id, :name, 'Meik�l�inen')";
        _input.Parameters = new QueryParameter[]
        {
            new QueryParameter { Name = "name", Value = "Matti", DataType = QueryParameterType.Varchar2 },
            new QueryParameter { Name = "id", Value = 3, DataType = QueryParameterType.Int32 },
        };

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        _input.Query = "select first_name from workers where id = 3";

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual(typeof(JArray), result.Output.GetType());
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_WithAllValues()
    {
        _input.Query = "insert " +
            "into workers values (1, 'Matti', 'Meik�l�inen', DATE '2022-04-12')";
        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_InsertMultipleRowsIntoTable()
    {
        _input.Query = "insert all " +
            "into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen') " +
            "into workers (id, first_name, last_name) values (2, 'Teppo', 'Teik�l�inen') " +
            "select * from dual";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(2, (int)result.Output["AffectedRows"]);

        _input.Query = "select * from workers " +
            "where id = :id";
        _input.Parameters = new QueryParameter[]
        {
            new QueryParameter { Name = "id", Value = 1, DataType = QueryParameterType.Int32 }
        };

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
        ClassicAssert.AreEqual("Meik�l�inen", (string)result.Output[0]["LAST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_InsertMultipleRowsWithMultipleParameters()
    {
        _input.Query = "insert all " +
            "into workers (id, first_name, last_name) values (:id1, :fname1, :lname1) " +
            "into workers (id, first_name, last_name) values (:id2, :fname2, :lname2) " +
            "select * from dual";
        _input.Parameters = new QueryParameter[]
        {
            new QueryParameter { Name = "id1", Value = 1, DataType = QueryParameterType.Int32 },
            new QueryParameter { Name = "fname1", Value = "Matti", DataType = QueryParameterType.Varchar2 },
            new QueryParameter { Name = "lname1", Value ="Meik�l�inen", DataType = QueryParameterType.Varchar2 },
            new QueryParameter { Name = "id2", Value = 2, DataType = QueryParameterType.Int32 },
            new QueryParameter { Name = "fname2", Value = "Teppo", DataType = QueryParameterType.Varchar2 },
            new QueryParameter { Name = "lname2", Value = "Teik�l�inen", DataType = QueryParameterType.Varchar2 }
        };

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(2, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_Update()
    {
        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        _input.Query = "update workers " +
            "set first_name = 'Saija', " +
            "last_name = 'Saijalainen' " +
            "where id = 1";

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        _input.Query = "select first_name from workers where id = 1";

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual("Saija", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_SelectWithNonExistingRow()
    {
        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        _input.Query = "select first_name from workers where id = 2";

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual("[]", result.Output.ToString());
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task ExecuteQuery_WithoutThrowErrorOnFailure()
    {
        _options.ThrowErrorOnFailure = false;

        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values ('Matti', 1, 'Meik�l�inen')";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(false, result.Success);
        ClassicAssert.True(result.Message.Contains("ORA-01722"));
    }

    [Test]
    public void ExecuteQuery_ThatThrowsException()
    {
        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values ('Matti', 1, 'Meik�l�inen')";

        var error = Assert.ThrowsAsync<Exception>(async () => await Oracle.ExecuteQuery(_input, _options, new CancellationToken()));
        ClassicAssert.True(error.Message.Contains("ORA-01722"));
    }

    [Test]
    public void ExecuteQuery_ErrorTesting()
    {
        _input.Query = "SELECT NOW();";
        var error = Assert.ThrowsAsync<Exception>(async () => await Oracle.ExecuteQuery(_input, _options, new CancellationToken()));
        ClassicAssert.True(error.Message.Contains("ORA-00923"));
    }

    [Test]
    public async Task ExecuteQuery_InsertWithBindParameterByNameFalse()
    {
        _options.BindParameterByName = false;
        _input.Query = "insert " +
            "into workers (id, first_name, last_name) values (:p, :p, :p)";
        _input.Parameters = new QueryParameter[]
        {
            new QueryParameter { Name = "p", Value = 1, DataType = QueryParameterType.Int32 },
            new QueryParameter { Name = "p", Value = "Matti", DataType = QueryParameterType.Varchar2 },
            new QueryParameter { Name = "p", Value ="Meik�l�inen", DataType = QueryParameterType.Varchar2 },
        };

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);

        _input.Query = "select first_name, last_name from workers where id = 1";

        result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
        ClassicAssert.AreEqual("Meik�l�inen", (string)result.Output[0]["LAST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_NonQueryExecutionType_Explicit()
    {
        _input.ExecuteType = ExecuteTypes.NonQuery;
        _input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)result.Output["AffectedRows"]);
    }

    [Test]
    public async Task ExecuteQuery_ExecuteReaderExecutionType_Explicit()
    {
        _input.ExecuteType = ExecuteTypes.NonQuery;
        _input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";
        await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        _input.ExecuteType = ExecuteTypes.ExecuteReader;
        _input.Query = "select first_name from workers where id = 1";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(typeof(JArray), result.Output.GetType());
        ClassicAssert.AreEqual("Matti", (string)result.Output[0]["FIRST_NAME"]);
    }

    [Test]
    public async Task ExecuteQuery_ScalarExecutionType_Count()
    {
        _input.ExecuteType = ExecuteTypes.NonQuery;
        _input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";
        await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        _input.ExecuteType = ExecuteTypes.Scalar;
        _input.Query = "select count(*) from workers";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual(1, (int)(decimal)result.Output);
    }

    [Test]
    public async Task ExecuteQuery_ScalarExecutionType_SingleValue()
    {
        _input.ExecuteType = ExecuteTypes.NonQuery;
        _input.Query = "insert into workers (id, first_name, last_name) values (1, 'Matti', 'Meik�l�inen')";
        await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        _input.ExecuteType = ExecuteTypes.Scalar;
        _input.Query = "select first_name from workers where id = 1";

        var result = await Oracle.ExecuteQuery(_input, _options, new CancellationToken());

        Assert.That(result, Is.Not.Null);
        ClassicAssert.AreEqual(true, result.Success);
        ClassicAssert.AreEqual("Matti", (string)result.Output);
    }
}

