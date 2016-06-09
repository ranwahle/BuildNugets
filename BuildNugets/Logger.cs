using System;
using System.IO;

namespace BuildNugets
{
    internal class Logger
    {
        internal static void WriteLine(string format, params object[] args)
        {
            using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "log.txt"), FileMode.Append))
                using (StreamWriter wr = new StreamWriter(fs))
            {
                wr.WriteLine(format, args);
            }
        }
    }
}