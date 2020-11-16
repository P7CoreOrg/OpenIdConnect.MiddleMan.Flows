namespace Common
{
    internal class NullDataProtection : IDataProtection
    {
        public string ProtectString(string unprotectedText)
        {
            return unprotectedText;
        }

        public string UnprotectString(string protectedText)
        {
            return protectedText;
        }
    }
}
