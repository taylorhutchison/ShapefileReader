using System;
using GISData.ShapefileFormat;
using GISData.GeometryTypes;

namespace GISData.ShapefileOutput
{
    static class ShapefileInfoOutput
    {
        private static void OutputPoints(Point[] points)
        {
            Console.WriteLine("Points: ");
            for (int i = 0; i < points.Length; i++)
            {
                Console.WriteLine("\tX: {0}, Y: {1}", points[i].X, points[i].Y);
            }
        }
        private static void OutputMultipoints(Multipoint[] multipoints)
        {
            for (int i = 0; i < multipoints.Length; i++)
            {
                Console.WriteLine("Multipoint containing {0} points: ",multipoints[i].Points.Length);
                OutputPoints(multipoints[i].Points);
            }
        }
        private static void OutputPolylines(Polyline[] polylines)
        {
            for (int i = 0; i < polylines.Length; i++)
            {
                Console.WriteLine("Polyline containing {0} part: ", polylines[i].Parts.Length);
                OutputMultipoints(polylines[i].Parts);
            }
        }
        private static void OutputPolygons(Polygon[] polygons)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                Console.WriteLine("Polygon containing {0} part: ", polygons[i].Parts.Length);
                OutputMultipoints(polygons[i].Parts);
            }
        }
        //Simple output method to test the contents of a shapefile header.
        public static void DisplayHeaderInfo(ShapefileHeader shpfileheader)
        {
            if (shpfileheader != null)
            {
                
                Console.WriteLine("Shapefile Type: {0}", ((ShapefileReaderManager.ShapeTypes)shpfileheader.ShapeType).ToString());
                Console.WriteLine("Size: {0} ({1}MB)", shpfileheader.FileLength, (double)shpfileheader.FileLength * 2 / 1000000);
                Console.WriteLine("Bounds: {0} {1} {2} {3}", shpfileheader.XMin, shpfileheader.YMin, shpfileheader.XMax, shpfileheader.YMax);
                Console.WriteLine("Z & M Bounds: {0} {1} {2} {3}", shpfileheader.ZMin, shpfileheader.ZMax, shpfileheader.MMin, shpfileheader.MMax);
            }
            else
            {
                throw new NullReferenceException("Shapefile Header is null when trying to access properties for display inside DisplayHeaderInfo.");
            }
        }

        public static void DisplayGeometryRecords(Object[] records)
        {
            if (records.GetType() == typeof(Point[]))
            {
                OutputPoints((Point[])records);
            }
            else if(records.GetType()==typeof(Multipoint[])){
                OutputMultipoints((Multipoint[])records);
            }
            else if (records.GetType() == typeof(Polyline[]))
            {
                OutputPolylines((Polyline[])records);
            }
            else if (records.GetType() == typeof(Polygon[]))
            {
                OutputPolygons((Polygon[])records);
            }
            else
            {
                Console.WriteLine("Type \"{0}\" not recognized.", records.GetType());
            }
        }
    }
}
