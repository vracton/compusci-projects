using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry.Geometry3D
{
    internal class Quadrilateral(IEnumerable<Point> points) : Polygon(points)
    {
        public Quadrilateral(Point p1, Point p2, Point p3, Point p4) 
            : this([p1, p2, p3, p4])
        { }
    }
}
