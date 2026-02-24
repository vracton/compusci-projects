namespace DongUtility
{
    /// <summary>
    /// Generic file utilities
    /// </summary>
    public static class FileUtilities
    {
        static public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Gets the root directory of the project
        /// </summary>
        static public string GetMainProjectDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory + @"../../../";
        }

        static private string documentsFolder = @"C:\Users\pdong\OneDrive\IMSA\Classes\CompuSci\Spring 2026\";

        /// <summary>
        /// Set the name of the documents folder
        /// </summary>
        /// <param name="folder"></param>
        static public void SetDocumentsFolder(string folder)
        {
            documentsFolder = folder;
        }

        /// <summary>
        /// Gets the place to store all documents
        /// </summary>
        static public string GetDocumentsFolder()
        {
            if (Directory.Exists(documentsFolder))
            {
                return documentsFolder;
            }
            else
            {
                return @"C:\Users\pdong\OneDrive - school\OneDrive - imsa.edu\IMSA\Classes\CompuSci\Spring 2026\";
            }
        }

        /// <summary>
        /// Checks whether a BinaryReader is currently at end of file
        /// </summary>
        static public bool IsEndOfFile(BinaryReader br)
        {
            // From https://stackoverflow.com/questions/10942848/c-sharp-checking-for-binary-reader-end-of-file
            return br.BaseStream.Position == br.BaseStream.Length;
        }

        static public string FindFileIfNameContains(string directory, string target)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory {directory} does not exist!");
            string file = "";
            foreach (var myfile in Directory.GetFiles(directory))
            {
                if (myfile.Contains(target, StringComparison.OrdinalIgnoreCase))
                {
                    if (file.Length > 0)
                    {
                        throw new Exception($"Found multiple files containing {target} in {directory}: {file}, {myfile}");
                    }
                    file = myfile;
                }
            }
            if (file.Length == 0)
            {
                throw new FileNotFoundException($"Could not find file containing {target} in {directory}");
            }
            else
            {
                return file;
            }
        }
    }
}
