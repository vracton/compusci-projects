using System.IO;

namespace Logging
{
    class Logger
    {
        private readonly string filePath;

        public Logger(string fileName)
        {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var dataDir = Path.Combine(projectDir, "data");
            Directory.CreateDirectory(dataDir);
            var outputPath = Path.Combine(dataDir, fileName);

            filePath = outputPath;

            File.Create(filePath).Dispose();
        }

        public void WriteLine(params object[] vals)
        {
            string logToConsole = "";
            string logToFile = "";

            foreach (object o in vals)
            {
                if (o is double)
                {
                    logToConsole += ((double)o).ToString("F3") + "\t";
                    logToFile += ((double)o).ToString("F3") + " ";
                }
                else
                {
                    logToConsole += o.ToString() + "\t";
                    logToFile += o.ToString() + " ";
                }
            }

            logToConsole = logToConsole.Trim();
            logToFile = logToFile.Trim() + "\n";

            File.AppendAllText(filePath, logToFile);
            Console.WriteLine(logToConsole);
        }
    }
}
