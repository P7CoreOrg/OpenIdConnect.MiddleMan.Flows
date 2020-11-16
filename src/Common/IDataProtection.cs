namespace Common
{
    public interface IDataProtection
    {
        string ProtectString(string unprotectedText);
        string UnprotectString(string protectedText);
    }
}
