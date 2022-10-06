using System;
using System.Diagnostics;
using PostgresRestore.Constants;
using PostgresRestore.Vo;

namespace PostgresRestore.Helper;

public static class CommandExecutor
{
    public static void Execute(string commandType, string user, string database)
    {
        ExecuteCommand("psql", $@"-U {user} -c ""{commandType} database """"{database}""""");
    }

    public static void ExecuteRestore(UserConnectionVo connection)
    {
        Environment.SetEnvironmentVariable(PostgresConstants.PasswordKey, connection.Password);
        switch (connection.ActionTypeValue)
        {
            case ActionTypeConstants.DropAndRestore:
                Execute("drop", connection.UserName, connection.DatabaseName);
                Execute("create", connection.UserName, connection.DatabaseName);
                break;
            case ActionTypeConstants.CreateAndRestore:
                Execute("create", connection.UserName, connection.DatabaseName);
                break;
        }

        string arguments;
        string fileName;
        if (connection.DatabaseBackupType == CommandTypeConstants.PgDump)
        {
            fileName = "psql";
            arguments =
                $@"-U {connection.UserName} ""{connection.DatabaseName}"" < ""{connection.RestoreFileLocation}""";
        }
        else
        {
            fileName = connection.DatabaseBackupType;
            arguments =
                $@"-U {connection.UserName} -d ""{connection.DatabaseName}"" ""{connection.RestoreFileLocation}""";
        }

        ExecuteCommand(fileName, arguments);
    }


    private static void ExecuteCommand(string fileName, string arguments)
    {
        var proc = new Process();
        proc.StartInfo.FileName = fileName;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0)
        {
            proc.Close();
            throw new Exception("Error restoring database: " + error);
        }

        proc.Close();
    }
}