
namespace Arena
{
    /// <summary>
    /// A registry that stores correspondences between graphic codes and actual files
    /// </summary>
    public class Registry
    {
        /// <summary>
        /// All GraphicInfo objects, with their codes stored as the indices of the list
        /// </summary>
        private readonly List<GraphicInfo?> typeMap = [];
        /// <summary>
        /// All GraphicInfo objects in a map so they can easily be decoded
        /// </summary>
        private readonly Dictionary<GraphicInfo, int> codeMap = [];

        private static readonly Lock locker = new();

        /// <summary>
        /// The directory to find all images
        /// </summary>
        public string ImageDirectory { get; private set; } = Directory.GetCurrentDirectory() + "\\";

        /// <summary>
        /// Initializes the registry with a directory to find images
        /// </summary>
        /// <param name="directory">The directory, as a local directory</param>
        /// <param name="suffix">A string that goes at the end of the directory</param>
        public void Initialize(string directory, string suffix = "")
        {
            if (directory == "")
            {
                ImageDirectory += suffix;
            }
            else
            {
                string dir = Directory.GetCurrentDirectory();
                string comparator = directory;
                var index = dir.LastIndexOf(comparator);
                var length = comparator.Length;
                ImageDirectory = string.Concat(dir.AsSpan(0, index + length), suffix);
            }
        }

        /// <summary>
        /// Gets the GraphicInfo object associated with a graphic code
        /// </summary>
        public GraphicInfo? GetInfo(int graphicCode)
        {
            return typeMap[graphicCode];
        }

        /// <summary>
        /// Gets the graphic code associated with a GraphicInfo object
        /// </summary>
        public int GetGraphicCode(GraphicInfo obj)
        {
            return codeMap[obj];
        }

        /// <summary>
        /// Gets all GraphicInfo objects stored
        /// </summary>
        public List<GraphicInfo?> GetAllGraphicInfo()
        {
            return typeMap;
        }

        /// <summary>
        /// Adds a GraphicInfo object to the registry.
        /// </summary>
        /// <returns>The new graphic code of the object</returns>
        public int AddEntry(GraphicInfo obj)
        {
            lock (locker)
            {
                if (codeMap.ContainsKey(obj))
                {
                    return GetGraphicCode(obj);
                }
                typeMap.Add(obj);
                codeMap.Add(obj, typeMap.Count - 1);
                return typeMap.Count - 1;
            }
        }

        /// <summary>
        /// Adds a GraphicInfo object to the registry with a specific index.
        /// Used when loading from a file
        /// </summary>
        internal void AddEntryWithIndex(GraphicInfo obj, int index)
        {
            lock (locker)
            {
                while (typeMap.Count <= index)
                {
                    typeMap.Add(null);
                }
                if (typeMap[index] is not null && typeMap[index]?.Filename.Length != 0)
                {
                    // Maybe this is okay
                    throw new ArgumentException("Something went wrong in reloading registries from file");
                }
                typeMap[index] = obj;
                codeMap.Add(obj, index);
            }
        }

        private const string mainHeader = "MAIN";

        /// <summary>
        /// Writes the registry to a file.
        /// </summary>
        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(mainHeader);
            bw.Write(typeMap.Count);

            for (int i = 0; i < typeMap.Count; ++i)
            {
                var gi = typeMap[i];
                if (gi is null)
                {
                    continue;
                }
                bw.Write(gi.Filename);
                bw.Write(gi.XSize);
                bw.Write(gi.YSize);
            }
        }

        /// <summary>
        /// Clears the registry of all GraphicInfo objects.
        /// </summary>
        public void Clear()
        {
            typeMap.Clear();
            codeMap.Clear();
        }

        /// <summary>
        /// Reads the registry from a file.
        /// </summary>
        public void Read(BinaryReader br)
        {
            string header = br.ReadString();
            if (header != mainHeader)
            {
                throw new FileNotFoundException("Invalid file passed to MainRegistry.FillFromFile()!");
            }

            int length = br.ReadInt32();

            Clear();

            for (int i = 0; i < length; ++i)
            {
                string filename = br.ReadString();
                double xSize = br.ReadDouble();
                double ySize = br.ReadDouble();

                var gi = new GraphicInfo(filename, xSize, ySize);
                AddEntry(gi);
            }
        }
    }
}
