namespace Frends.Oracle.ExecuteProcedure.Tests;

using System;
using System.Data;
using System.Threading;
using global::Oracle.ManagedDataAccess.Client;

/// <summary>
/// Provides database setup helpers for the integration tests.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Waits for the Oracle container to accept connections.
    /// </summary>
    /// <param name="connectionString">Connection string used to check database availability.</param>
    internal static void TestConnectionBeforeRunningTests(string connectionString)
    {
        const int maxRetries = 120;
        const int sleepMs = 5000; // 5 seconds
        Exception lastException = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var con = new OracleConnection(connectionString);
                con.Open();
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(sleepMs); // wait before retrying
                continue;
            }

            return;
        }

        throw new Exception("Oracle did not accept connections before the test setup timed out.", lastException);
    }

    /// <summary>
    /// Creates the test table.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    internal static void CreateTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;

        cmd.CommandText =
            @"CREATE TABLE test_user.workers(id NUMBER, name VARCHAR2(100) NULL, address VARCHAR2(100) NULL, PRIMARY KEY(id))";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Inserts sample rows into the test table.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    internal static void InsertTestData(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;

        cmd.CommandText = @"INSERT INTO test_user.workers(id, name, address) VALUES (1, 'risto', 'haapatie 9')";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Drops the test table.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    internal static void DropTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = "drop table test_user.workers";

        cmd.CommandType = CommandType.Text;

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Drops a test procedure if it exists.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    /// <param name="name">Procedure name.</param>
    internal static void DropProcedure(OracleConnection con, string name)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = $"DROP PROCEDURE test_user.{name}";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }
        catch (OracleException ex)
        {
            // ORA-04043: object does not exist - safe to ignore in teardown
            if (ex.Number != 4043)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Creates a test procedure.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    /// <param name="procedureName">Procedure name.</param>
    internal static void CreateTestProcedure(OracleConnection con, string procedureName = "unitestproc")
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @$"
create or replace procedure test_user.{procedureName} (p_name in varchar2, p_address out varchar2) as
begin
  select address into p_address from test_user.workers where name = p_name;
end {procedureName};";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates the test database user and grants its permissions.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    internal static void CreateTestUser(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
create or replace procedure p_create_user (par_username in varchar2) is
begin
  execute immediate ('create user '    || par_username ||
                     ' identified by ' || par_username ||
                     ' temporary tablespace temp '     ||
                     ' profile default ');

  execute immediate ('grant create session to ' || par_username);
  execute immediate ('grant create table to ' || par_username);
  execute immediate ('grant create procedure to ' || par_username);
  execute immediate ('grant unlimited tablespace to ' || par_username);
end;";

        cmd.CommandType = CommandType.Text;

        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
begin
  p_create_user('test_user');
end;";

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch
        {
            DropTestUser(con);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Drops the test database user.
    /// </summary>
    /// <param name="con">Open Oracle connection.</param>
    internal static void DropTestUser(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "DROP USER test_user CASCADE";
        cmd.ExecuteNonQuery();
    }
}
