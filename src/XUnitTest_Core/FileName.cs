using System.IO;

namespace XUnitTest_Core
{
    static class FileName
    {
        public static string Create(string name)
        {
            var fullName = Path.Combine(System.AppContext.BaseDirectory, "documents", name);
            return fullName;
        }
    }
}