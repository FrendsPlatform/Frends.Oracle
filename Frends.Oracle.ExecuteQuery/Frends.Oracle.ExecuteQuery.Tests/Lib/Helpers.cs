using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Frends.Oracle.ExecuteQuery.Tests;

internal static class Helpers
{
    internal static void CreateTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;

        cmd.CommandText = @"create table test_user.workers(" +
                "id NUMBER, " +
                "first_name varchar2(50), " +
                "last_name varchar2(50), " +
                "start_date date, " +
                "primary key(id))";
        cmd.ExecuteNonQuery();
    }

    internal static void DropTestTable(OracleConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = "drop table test_user.workers";

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
        catch (Exception)
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
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
