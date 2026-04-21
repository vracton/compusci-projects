using Geometry.Geometry2D;

namespace Arena
{
    /// <summary>
    /// An arena object that can move
    /// </summary>
    abstract public class MovingObject : ArenaObject
    {
        public override bool IsUpdating => true;

        private Turn? turn = null;

        public MovingObject(int graphicCode, int layer, double width, double height)
            : base(graphicCode, layer, width, height)
        { }

        public MovingObject(int graphicCode, int layer, Shape2D shape)
            : base(graphicCode, layer, shape)
        { }

        /// <summary>
        /// Called at the beginning of each turn
        /// </summary>
        abstract protected void UserDefinedBeginningOfTurn();
        /// <summary>
        /// Called at the end of each turn
        /// </summary>
        abstract protected void UserDefinedEndOfTurn();
        abstract protected Turn? UserDefinedChooseAction();

        /// <summary>
        /// Called at the beginning of each turn. The redundancy is a just-in-case feature.
        /// </summary>
        public void BeginningOfTurn()
        {
            UserDefinedBeginningOfTurn();
        }

        /// <summary>
        /// Chooses an action for the object to take.
        /// </summary>
        /// <returns>True if it was able to determine a turn. Currently not used (either foresight or a vestigial structure)</returns>
        public bool ChooseAction()
        {
            turn = UserDefinedChooseAction();
            return true;
        }

        /// <summary>
        /// Executes the action that was chosen.
        /// </summary>
        public void ExecuteAction()
        {
            //if (turn != null)
            //{
                DoTurn(turn);
            //}
        }

        /// <summary>
        /// Executes the action that was chosen.
        /// </summary>
        /// <returns>True if it worked successfully</returns>
        abstract protected bool DoTurn(Turn? turn);

        /// <summary>
        /// Called at the end of each turn. The redundancy is a just-in-case feature.
        /// </summary>
        public void EndOfTurn()
        {
            UserDefinedEndOfTurn();
        }
    }
}
