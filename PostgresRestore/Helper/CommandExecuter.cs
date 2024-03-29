﻿using System;
using System.Diagnostics;
using PostgresRestore.Constants;
using PostgresRestore.Vo;

namespace PostgresRestore.Helper;

public static class CommandExecutor
{
    public static void Execute(string commandType, string user, string database)
    {
        var fileName = "psql";
        var arguments = $"-U {user} -c \"{commandType} database \"\"{database}\"\"\"";

        var process = ExecuteCommand(fileName, arguments);
        process.Start();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            process.Close();
            throw new Exception(error);
        }
        process.Close();
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

        string fileName;
        string arguments;
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

        var process = ExecuteCommand(fileName, arguments);
        process.Start();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            process.Close();
            throw new Exception(error);
        }
        process.Close();
    }


    private static Process ExecuteCommand(string fileName, string arguments)
    {
        var proc = new Process();
        proc.StartInfo.FileName = fileName;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        return proc;
    }
}