using System.IO;

namespace Common.Classes
{
    public static class ValidationData
    {
        public static bool AboveZero(int number)
        {
            if (number <= 0)
                return false;

            return true;
        }

        public static bool FileExist(string path)
        {
            if (!File.Exists(path))
                return false;

            return true;
        }
    }
}