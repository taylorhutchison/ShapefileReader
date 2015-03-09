using System;
using System.IO;
using GISData.GeometryTypes;
using GISData.ShapefileFormat;
using GISData.ShapefileOutput;


//The namespace GISData contains the ShapefileMain, ShapefileHeader, and ShapefileRecord types
//As well as the ShapefileReader and ShapefileTasks
//GISData also contains the geometry primitives (i.e. point, line, polygon, etc.)
namespace GISData
{

    class Program
    {

        static void Main()
        {

            try
            {
                ShapefileReader shpfile = ShapefileReaderManager.ReadShapefile(@"C:\csharp\ShapefileManager\TestData\Polygon");
                var Geometries = ShapefileGeometeryFactory.ReaderToGeometryCollection(shpfile);
                ShapefileInfoOutput.DisplayGeometryRecords(Geometries);
                Console.WriteLine("Hello");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }



            Console.ReadLine();
        }
    }
}
