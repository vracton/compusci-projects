using PointGenerator;

var generator = new GeneratorYOURNAMES();

var signalPoint = generator.MakePoint(true);
var backgroundPoint = generator.MakePoint(false);

var manyPoints = generator.MakeDataSet(1000, .5);

//var genSet = new GeneratorSet(

//    );

//const string basePath = @"C:\Users\pdong\OneDrive - imsa.edu\IMSA\Classes\CompuSci\Spring 2025\Project 3 data\";

//genSet.CreateEverything(basePath, 1000, 10000);

