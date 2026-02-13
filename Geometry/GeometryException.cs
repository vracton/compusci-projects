namespace Geometry
{
    /// <summary>
    /// An all-purpose exception for geometry errors.
    /// Common for divison by zero, no-slope errors, etc.
    /// </summary>

    public class GeometryException(string what) : Exception(what)
    {
    }
}
