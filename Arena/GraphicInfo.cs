namespace Arena
{
    /// <summary>
    /// Stores basic information about a bitmap
    /// </summary>
    public class GraphicInfo
    {
        /// <summary>
        /// The filename that contains the bitmap
        /// </summary>
        public string Filename { get; }
        /// <summary>
        /// The size of the bitmap in the x direction
        /// </summary>
        public double XSize { get; }
        /// <summary>
        /// The size of the bitmap in the y direction
        /// </summary>
        public double YSize { get; }

        public GraphicInfo(string pictureFile, double xsize, double ysize)
        {
            XSize = xsize;
            YSize = ysize;
            Filename = pictureFile;
        }

        public override string ToString()
        {
            return $"{Filename}_{XSize}_{YSize}";
        }

        static public bool operator==(GraphicInfo lhs, GraphicInfo rhs)
        {
            if (lhs is null && rhs is null)
            {
                return true;
            }
            else if (lhs is null || rhs is null)
            {
                return false;
            }
            return lhs.Filename == rhs.Filename && lhs.XSize == rhs.XSize && lhs.YSize == rhs.YSize;
        }

        static public bool operator!=(GraphicInfo lhs, GraphicInfo rhs)
        {
            return !(lhs == rhs);
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Filename);
            bw.Write(XSize);
            bw.Write(YSize);
        }

        public GraphicInfo(BinaryReader br)
        {
            Filename = br.ReadString();
            XSize = br.ReadDouble();
            YSize = br.ReadDouble();
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return obj is GraphicInfo info && this == info;
        }

        public override int GetHashCode()
        {
            return Filename.GetHashCode() ^ XSize.GetHashCode() ^ YSize.GetHashCode();
        }
    }
}
