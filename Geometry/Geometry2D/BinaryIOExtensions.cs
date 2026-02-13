namespace Geometry.Geometry2D
{
    static public class BinaryIOExtensions
    {
        static public void Write(this BinaryWriter bw, Point vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
        }

        static public Point ReadPoint2D(this BinaryReader br)
        {
            double x = br.ReadDouble();
            double y = br.ReadDouble();
            return new Point(x, y);
        }
    }
}
