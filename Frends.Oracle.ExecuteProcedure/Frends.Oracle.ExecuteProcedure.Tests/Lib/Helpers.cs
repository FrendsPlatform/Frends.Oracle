﻿using System;
using System.Data;
using System.Threading;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace Frends.Oracle.ExecuteProcedure.Tests;

internal static class Helpers
{
    /// <summary>
    /// This methods waits for the docker container to be ready. 
    /// Method tests connection 20 times and if connection can't be made, it waits for a minute after every attempt
    /// and tries again. This is needed for the CI for it to wait for the container to be ready.
    /// </summary>
    internal static void TestConnectionBeforeRunningTests(string connectionString)
    {
        using var con = new OracleConnection(connectionString);
        foreach (var i in Enumerable.Range(1, 15))
        {
            try { con.Open(); }
            catch
            {
                if (con.State == ConnectionState.Open)
                    break;

                Thread.Sleep(60000);
            }
        }
        if (con.State != ConnectionState.Open)
            throw new Exception("Check that the docker container is up and running.");
        con.Close();

    }
    internal static void CreateTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;

        cmd.CommandText = @"CREATE TABLE test_user.workers(id NUMBER, name VARCHAR2(100) NULL, address VARCHAR2(100) NULL, PRIMARY KEY(id))";
        cmd.ExecuteNonQuery();
    }

    internal static void InsertTestData(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;

        cmd.CommandText = @"INSERT INTO test_user.workers(id, name, address) VALUES (1, 'risto', 'haapatie 9')";
        cmd.ExecuteNonQuery();
    }

    internal static void DropTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = "drop table test_user.workers";

        cmd.CommandType = CommandType.Text;

        cmd.ExecuteNonQuery();
    }

    internal static void DropProcedure(OracleConnection con, string name)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = $"DROP PROCEDURE test_user.{name}";

        cmd.CommandType = CommandType.Text;

        cmd.ExecuteNonQuery();
    }

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

    internal static void DropTestUser(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "DROP USER test_user CASCADE";
        cmd.ExecuteNonQuery();
    }
}

