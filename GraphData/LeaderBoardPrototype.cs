using DongUtility;
using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// The prototype for a leader board.  Contains all the information needed to store it but does not draw anything directly
    /// </summary>
    public class LeaderBoardPrototype : IGraphPrototype
    {
        /// <summary>
        /// All the leader bars that go into the leaderboard
        /// </summary>
        public List<LeaderBarPrototype> Prototypes { get; } = [];
        public IGraphPrototype.GraphType GetGraphType() => IGraphPrototype.GraphType.LeaderBoard;

        public LeaderBoardPrototype(IEnumerable<LeaderBarPrototype> leaderBars)
        {
            Prototypes.AddRange(leaderBars);
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Prototypes.Count);
            foreach (var prototype in Prototypes)
            {
                bw.Write(prototype.Name);
                bw.Write(prototype.Color);
            }
        }

        internal LeaderBoardPrototype(BinaryReader br)
        {
            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string name = br.ReadString();
                Color color = br.ReadColor();
                Prototypes.Add(new LeaderBarPrototype(name, color));
            }
        }
    }
}
