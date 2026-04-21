namespace Arena
{
    /// <summary>
    /// Base class for the logical turn chosen by a MovingObject
    /// </summary>
    abstract public class Turn(MovingObject owner)
    {
        protected MovingObject Owner { get; } = owner;

        abstract public bool DoTurn();
    }
}
