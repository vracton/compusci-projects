using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force that affects all projectiles in the simulation
    /// </summary>
    abstract public class GlobalForce(KinematicsEngine engine) : Force
    {
        /// <summary>
        /// The current time of the engine
        /// </summary>
        protected double Time => engine.Time;

        /// <summary>
        /// The current time increment of the engine
        /// </summary>
        protected double DeltaTime => engine.DeltaTime;

        /// <summary>
        /// The list of all projectiles, which some global forces will need, e.g. gravity
        /// </summary>
        protected IList<Projectile> Projectiles => engine.Projectiles;
        
        /// <summary>
        /// All the projectiles in the engine, including the preprocesed ones (that do not need forces applied)
        /// </summary>
        protected IEnumerable<Projectile> AllProjectiles => engine.AllProjectiles;

        /// <summary>
        /// Returns the actual force on a given projectile
        /// </summary>
        abstract protected Vector GetForce(Projectile proj);

        public override void AddForce(double deltaTime)
        {
            foreach (Projectile proj in Projectiles)
            {
                proj.AddForce(GetForce(proj));
            }
        }
    }
}
