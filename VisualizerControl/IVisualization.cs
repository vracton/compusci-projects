using VisualizerBaseClasses;

namespace VisualizerControl
{
    /// <summary>
    /// Provides an interface for a kinematics engine or world class to work with the visualizer.
    /// This class should keep track of projectiles and forces and do all updates when told to.
    /// </summary>
    public interface IVisualization : IEngine<Visualizer, VisualizerCommand>
    {
        // Currently there is nothing here, but this interface is useful for type safety
        // Maybe we can add some visualization-specific methods later
    }
}
