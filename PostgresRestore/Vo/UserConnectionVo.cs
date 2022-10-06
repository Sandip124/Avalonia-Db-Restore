namespace PostgresRestore.Vo;

public class UserConnectionVo
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string DatabaseName { get; set; }
    public string RestoreFileLocation { get; set; }
    public string DatabaseBackupType { get; set; }
    public string ActionTypeValue { get; set; }
        
}