namespace PostgresRestore.Constants;

internal class ActionTypeConstants
{
    public const string DropAndRestore = "Drop_and_Restore";
    public const string CreateAndRestore = "Create_and_Restore";
}

internal class CommandTypeConstants
{
    public const string PgRestore = "pg_restore";
    public const string PgDump = "pg_dump";

}

internal class PostgresConstants
{
    public const string PasswordKey = "PGPASSWORD";
    public const string UserKey = "PGUSER";
}