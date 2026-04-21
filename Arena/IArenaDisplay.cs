using Geometry.Geometry2D;

namespace Arena
{
    /// <summary>
    /// Interface for the arena display.
    /// </summary>
    public interface IArenaDisplay
    {
        /// <summary>
        /// Add an object to the display.
        /// </summary>
        void AddObject(Registry registry, int layer, int graphicCode, int objCode, Point coord);

        /// <summary>
        /// Move an object to a new coordinate.
        /// </summary>
        void MoveObject(int layer, int objCode, Point newCoord);

        /// <summary>
        /// Rotate an object to a new angle.
        /// </summary>
        void RotateObject(int layer, int objCode, double newAngle);

        /// <summary>
        /// Remove an object from the display.
        /// </summary>
        void RemoveObject(int layer, int objCode);

        /// <summary>
        /// Change the graphic of an object.
        /// </summary>
        void ChangeObjectGraphic(int layer, int objCode, int graphicCode);

        /// <summary>
        /// Set the dimensions of the window and arena.
        /// </summary>
        void SetWindowDimensions(double windowWidth, double windowHeight, double arenaWidth, double arenaHeight);
    }
}
