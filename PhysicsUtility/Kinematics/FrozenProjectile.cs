using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A projectile that does not move
    /// </summary>
    public class FrozenProjectile(Vector position, double mass) : Projectile(position, new(0, 0, 0), mass)
    {
        public override void Update(double timeIncrement)
        {
            // No changes in its position, velocity, or acceleration
        }
    }
}
