using GraphControl;
using GraphData;
using MotionVisualizer3D;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Thermodynamics;
using static WPFUtility.UtilityFunctions;

namespace Visualizer.Thermodynamics
{
    class ThermodynamicsDriver
    {
        private const string AmmoniaName = "NH3";
        private const string HydrochloricAcidName = "HCl";
        private const string AmmoniumChlorideName = "NH4Cl";
        private const double AmmoniaMass = 2.8279810583504195e-26;
        private const double HydrochloricAcidMass = 6.054491492822562e-26;
        private const double AmmoniumChlorideMass = 8.882389524219624e-26;

        static internal void Run()
        {
            const double containerSize = 50;

            // level 1
            //Color color = Colors.LightSeaGreen;
            //const int nParticles = 1000;
            //const double deltaTime = .01;
            //const string name = "Molecule";
            //const double minSpeed = 580;
            //const double maxSpeed = 650;
            //const double mass = 3.0 * 1e-26;

            // level 2
            //Color color = Colors.LightSeaGreen;
            //const int nParticles = 1000;
            //const double deltaTime = .01;
            //const string name = "Molecule";
            //const double mass = 3.0 * 1e-26;
            //double temp = 273.15;

            // level 3
            const int particlesPerReactant = 500;
            const double deltaTime = 2.5e-4;
            const double temp = 300;

            var cont = new ParticleContainer(containerSize);

            // level 1
            //var info = new ParticleInfo(name, mass, ConvertColor(color));
            //var generator = new FlatGenerator(cont, minSpeed, maxSpeed);

            // level 2
            //var info = new ParticleInfo(name, mass, ConvertColor(color));

            // level 3
            cont.RegisterParticleType(AmmoniaName, AmmoniaMass, ConvertColor(Colors.LightSkyBlue));
            cont.RegisterParticleType(HydrochloricAcidName, HydrochloricAcidMass, ConvertColor(Colors.OrangeRed));
            cont.RegisterParticleType(AmmoniumChlorideName, AmmoniumChlorideMass, ConvertColor(Colors.Ivory));

            double sum = 0.0;
            double maxY = 0.0;
            double maxX = 0;
            // rough estimate of max speed to consider for distribution
            for (int i = 0; sum <= 0.999; i += 10)
            {
                //double y = MaxBoltzGenerator.GetProb(mass, temp, i); //level 2
                double y = MaxBoltzGenerator.GetProb(HydrochloricAcidMass, temp, i); //level 3
                if (y > maxY)
                {
                    maxY = y;
                }
                sum += y * 10;
                maxX = i;
            }
            var generator = new MaxBoltzGenerator(cont, temp, maxX, maxY);

            // level 1 & 2
            //cont.Dictionary.AddParticle(info);
            //cont.AddRandomParticles(generator, name, nParticles);

            // level 3
            cont.AddRandomParticles(generator, AmmoniaName, particlesPerReactant,
                new DongUtility.Range(0, 22.5), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));
            cont.AddRandomParticles(generator, HydrochloricAcidName, particlesPerReactant,
                new DongUtility.Range(27.5, containerSize), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));

            var visualization = new ThermodynamicsVisualization(cont)
            {
                BoxColor = Colors.IndianRed,
                StopTime = 2.5 // level 3
            };

            GraphControl.Timeline.MaximumPoints = 20000;

            var viz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = deltaTime,
                TimeScale = 1
            };


            const int histogramBins = 50;

            // level 1
            //viz.Manager.AddHist(histogramBins, ConvertColor(Colors.BlueViolet), () => cont.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            //viz.Manager.AddText("Temperature (K)", ConvertColor(Colors.CadetBlue), () => cont.Temperature.ToString());

            // level 2
            //viz.Manager.AddSingleGraph("Temperature", ConvertColor(Colors.CornflowerBlue), () => visualization.Time, () => cont.Temperature, "Time (s)", "Temperature (K)");
            //viz.Manager.AddHist(histogramBins, ConvertColor(Colors.BlueViolet), () => cont.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            //viz.Manager.AddText("Temperature (K)", ConvertColor(Colors.CadetBlue), () => cont.Temperature.ToString());

            // level 3
            viz.Manager.AddGraph(
                [
                    new GraphDataManager.TimelineInfo(
                        new TimelinePrototype(AmmoniaName, ConvertColor(Colors.LightSkyBlue)),
                        new GraphDataManager.BasicFunctionPair(() => visualization.Time, () => cont.GetNParticles(AmmoniaName))),
                    new GraphDataManager.TimelineInfo(
                        new TimelinePrototype(HydrochloricAcidName, ConvertColor(Colors.OrangeRed)),
                        new GraphDataManager.BasicFunctionPair(() => visualization.Time, () => cont.GetNParticles(HydrochloricAcidName))),
                    new GraphDataManager.TimelineInfo(
                        new TimelinePrototype(AmmoniumChlorideName, ConvertColor(Colors.Gray)),
                        new GraphDataManager.BasicFunctionPair(() => visualization.Time, () => cont.GetNParticles(AmmoniumChlorideName)))
                ],
                "Time (s)",
                "Particle count");
            viz.Manager.AddSingleGraph("Temperature", ConvertColor(Colors.CornflowerBlue), () => visualization.Time, () => cont.Temperature, "Time (s)", "Temperature (K)");
            viz.Manager.AddHist(histogramBins, ConvertColor(Colors.BlueViolet), () => cont.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            viz.Manager.AddText("Temperature (K)", ConvertColor(Colors.CadetBlue), () => cont.Temperature.ToString("F1"));

            viz.Show();
        }
    }
}
