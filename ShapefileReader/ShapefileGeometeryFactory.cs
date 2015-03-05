using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GISData.GeometryTypes;

namespace GISData.ShapefileFormat
{
    //This static class is used to transform byte records or other geometry representations derived from a shapefile
    //into GeometryType objects (i.e. Point, Multipoint, Polyline, and Polygon).
    public static class ShapefileGeometeryFactory
    {
        //Every "poly" type (i.e. Polyline, Polygon, etc.) has two attributes: number of parts, and number of total points.
        //This small struct is used to better keep track of that information by giving a more qualified name to these
        //variables instead of having free-floating ints.
        private struct PolyRecordFields
        {
            public int NumParts;
            public int NumPoints;

            public PolyRecordFields(byte[] recordContents)
            {
                //Number of parts is an int at byte 36 in the record header.
                this.NumParts = BitConverter.ToInt32(recordContents, 36);
                //Number of total points is an int at byte 40 in the record header.
                this.NumPoints = BitConverter.ToInt32(recordContents, 40);
            }
        }
        
        //A "poly" type can be broken into parts. The number of parts is an integer stored at the 36th byte of a shape record.
        //The contents of the record (i.e. the actual point geometries that make up a polyline or polygon) are stored consecutively in an array.
        //The index that tells you where in the content array each part starts is stored starting at byte 44. This function takes the number
        //of parts and starts at byte 44 moving up 4 bytes (the size of an int) according to the number of parts (obtained at byte 36).
        //The array of int that is returned will always start with 0, meaning the first "part" of this poly-type geometry will begin at
        //byte numParts * 4 + 48.
        private static int[] GetRecordPartOffsets(int numParts, byte[] recordContents)
        {
            int[] recordOffsets = new int[numParts];
            for (int i = 0; i < numParts; i++)
            {
                recordOffsets[i] = BitConverter.ToInt32(recordContents, (i * 4) + 44);
            }
            return recordOffsets;
        }

        //Since all "poly" type geometries are constructed out of points it is easiest to convert all the records into point types
        //then later break them into their individual parts (if necessary) and convert them from points to polylines.
        //This function loops through all bytes of the record contents and creates new Point types. It is necessary to know
        //the number of points in the record as this sets the upper limit of the loop. It is also necessary to know the number
        //of parts because this establishes the amount of bytes to skip (at least 48 bytes).
        private static Point[] GetPolyPoints(int numPoints, int numParts, byte[] recordContents)
        {
            //bytesToSkip refers to the number of bytes before the actual geometries (i.e. doubles representing x and y geometry) are found.
            //8 bytes must be added to bytesToSkip to find the Y geometry.
            int bytesToSkip = 44 + (4 * numParts);

            Point[] points = new Point[numPoints];
            for (int j = 0; j < numPoints; j++)
            {
                points[j] = new Point(BitConverter.ToDouble(recordContents, (j * 16) + bytesToSkip), BitConverter.ToDouble(recordContents, (j * 16) + bytesToSkip+8));
            }
            return points;
        }

        //For "poly" type geometries, this function returns an array of the multipoint parts of a record.
        //This multipoint part array is used in the construction of the "poly" type geometry.
        private static Multipoint[] GetMultipointParts(byte[] recordContents)
        {
            
            //Get the number of parts and number of points and stored inside the PolyRecordFields struct.
            PolyRecordFields recordNums = new PolyRecordFields(recordContents);

            //Get all the points in the record.
            Point[] points = GetPolyPoints(recordNums.NumPoints, recordNums.NumParts, recordContents);

            //The points will be divided up into parts and assigned an index here.
            Multipoint[] multiparts = new Multipoint[recordNums.NumParts];

            //Get the index of each part.
            int[] partIndex = GetRecordPartOffsets(recordNums.NumParts, recordContents);

            //Loop through all the parts indicies and copy the points of that part to the multipart array.
            for (int n = 0; n < partIndex.Length; n++)
            {
                //Find the number of points between the current index and the next index.
                //If this is the last index then find the number of points between it and the length of the point array.
                int numPointsInPart = n < partIndex.Length - 1 ? partIndex[n + 1] - partIndex[n] : recordNums.NumPoints - partIndex[n];

                //Copy points to new point array sized for that part.
                Point[] partPoints = new Point[numPointsInPart];
                partPoints = points.Skip(partIndex[n]).Take(numPointsInPart).ToArray<Point>();

                //Create a new multipoint object from the new point array and store it at the current part index.
                multiparts[n] = new Multipoint(partPoints);
            }
            return multiparts;
        }

        private static Multipoint[] GetMultipointParts(ShapefileRecord record)
        {
            return GetMultipointParts(record.RecordContents);
        }

        //This generic function takes in a list of shapefile records and 
        //creates an array of geometries based on the generic type.
        private static T[] GetPolyRecords<T>(List<ShapefileRecord> records)
        {
            T[] poly = new T[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                //Activator.CreateInstance is used to create instances of types not known till runtime.
                //It takes a type and an array of parameters that are passed to the constructor.
                poly[i] = (T)Activator.CreateInstance(typeof(T),new object[] {GetMultipointParts(records[i])});
            }
            return poly;
        }

        //Takes a list of records and calls the appropriate factory function based on the type.
        public static Object[] RecordsToGeometry(List<ShapefileRecord> records, ShapefileReaderManager.ShapeTypes shapeType)
        {
            if (records != null && shapeType!= ShapefileReaderManager.ShapeTypes.Null)
            {
                if (shapeType == ShapefileReaderManager.ShapeTypes.Point)
                {
                    Object[] points = RecordsToPointGeometry(records);
                    return points;
                }
                else if (shapeType == ShapefileReaderManager.ShapeTypes.MultiPoint)
                {
                    Object[] multipoints = RecordsToMultipointGeometry(records);
                    return multipoints;
                }
                else if (shapeType == ShapefileReaderManager.ShapeTypes.PolyLine)
                {
                    Object[] polylines = RecordsToPolylineGeometry(records);
                    return polylines;
                }
                else if (shapeType == ShapefileReaderManager.ShapeTypes.Polygon)
                {
                    Object[] polygons = RecordsToPolygonGeometry(records);
                    return polygons;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //Function overload that allows for the integer representation of a ShapefileMain instead of an Enum
        public static Object[] RecordsToGeometry(List<ShapefileRecord> records, int shapeType)
        {
            return RecordsToGeometry(records, (ShapefileReaderManager.ShapeTypes)shapeType);
        }

        //Factory function to transform records into an array of Points.
        public static Point[] RecordsToPointGeometry(List<ShapefileRecord> records){
            Point[] points = new Point[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                points[i] = new Point(BitConverter.ToDouble(records[i].RecordContents, 4), BitConverter.ToDouble(records[i].RecordContents, 12));
            }
            return points;
        }

        //Factory function to transform records into an array of Multipoints.
        public static Multipoint[] RecordsToMultipointGeometry(List<ShapefileRecord> records)
        {
            Multipoint[] multipoints = new Multipoint[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                int numPoints = BitConverter.ToInt32(records[i].RecordContents,36);
                Point[] points = new Point[numPoints];
                for (int j = 0; j < numPoints; j++)
                {
                    points[j] = new Point(BitConverter.ToDouble(records[i].RecordContents, (j * 16) + 40), BitConverter.ToDouble(records[i].RecordContents, (j * 16) + 48));
                }
                multipoints[i] = new Multipoint(points);
            }
            return multipoints;
        }

        //The process for creating Polyline and Polygon types is almost identical because
        //a polygon is essentially a closed polyline.
        public static Polyline[] RecordsToPolylineGeometry(List<ShapefileRecord> records)
        {
            return GetPolyRecords<Polyline>(records);
        }

        //The process for creating Polyline and Polygon types is almost identical because
        //a polygon is essentially a closed polyline.
        public static Polygon[] RecordsToPolygonGeometry(List<ShapefileRecord> records)
        {
            return GetPolyRecords<Polygon>(records);
        }

        public static Point CreatePoint(double x, double y)
        {
            return new Point(x, y);
        }

        public static Point CreatePoint(byte[] buffer)
        {
            if (buffer.Length >= 20)
            {
                return new Point(BitConverter.ToDouble(buffer, 4), BitConverter.ToDouble(buffer, 12));
            }
            else
            {
                throw new ArgumentException("Byte buffer was an invalid size to create a point shape. Buffer size provided was "+buffer.Length.ToString());
            }
        }
        
        public static Multipoint CreateMultipoint(Point[] points)
        {
            return new Multipoint(points);
        }

        public static Multipoint CreateMultipoint(byte[] buffer)
        {
            if (buffer.Length >= 56)
            {
                int numPoints = BitConverter.ToInt32(buffer, 36);
                Point[] pointsFromBuffer = new Point[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    pointsFromBuffer[i] = new Point(BitConverter.ToDouble(buffer, (i * 16) + 40), BitConverter.ToDouble(buffer, (i * 16) + 48));
                }
                return new Multipoint(pointsFromBuffer);
            }
            else
            {
                throw new ArgumentException("Byte buffer was an invalid size to create a multipoint shape. Buffer size provided was " + buffer.Length.ToString());
            }
        }

        public static Polyline CreatePolyline(byte[] buffer)
        {
            if (buffer.Length >= 60)
            {
                return new Polyline(GetMultipointParts(buffer));
            }
            else
            {
                throw new ArgumentException("Byte buffer was an invalid size to create a polyline shape. Buffer size provided was " + buffer.Length.ToString());
            }
        }

        public static Polygon CreatePolygon(byte[] buffer)
        {
            if (buffer.Length >= 60)
            {
                return new Polygon(GetMultipointParts(buffer));
            }
            else
            {
                throw new ArgumentException("Byte buffer was an invalid size to create a polygon shape. Buffer size provided was " + buffer.Length.ToString());
            }
        }

        public static Type GetGeometryType (ShapefileReader shpReader){
            if (shpReader != null)
            {
                var geometryType = shpReader.ShapeType.ToString();
                switch (geometryType)
                {
                    case "Point":
                        return typeof(Point);
                    case "Multipoint":
                        return typeof(Multipoint);
                    case "Polyline":
                        return typeof(Polyline);
                    case "Polygon":
                        return typeof(Polygon);
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static T[] ReaderToGeometryCollection<T>(ShapefileReader shpReader, Func<byte[], T> CreateGeometry)
        {

            List<T> records = new List<T>();
            shpReader.OpenComponentFile("shp");
            while (true)
            {
                byte[] nextRecord = shpReader.GetNextRecord();
                if (nextRecord != null)
                {
                    records.Add(CreateGeometry(nextRecord));
                }
                else
                {
                    shpReader.CloseFileStream();
                    return records.ToArray();
                }
            }
        }

        public static object[] ReaderToGeometryCollection(ShapefileReader shpReader)
        {
            if (shpReader != null && shpReader.Header != null)
            {
                var geometryType = shpReader.ShapeType.ToString();
                switch (geometryType)
                {
                    case "Point":
                        return ReaderToGeometryCollection<Point>(shpReader, CreatePoint);
                    case "Multipoint":
                        return ReaderToGeometryCollection<Multipoint>(shpReader, CreateMultipoint);
                    case "Polyline":
                        return ReaderToGeometryCollection<Polyline>(shpReader, CreatePolyline);
                    case "Polygon":
                        return ReaderToGeometryCollection<Polygon>(shpReader, CreatePolygon);
                    case "Null":
                        return null;
                    default:
                        throw new Exception("Unknown geometry type \"" + geometryType + "\" provided for shapefile at " + shpReader.Path);
                }
            }
            else
            {
                throw new NullReferenceException("Shapefile reader has not been initialized correctly or the main header file has not been read. Cannot determine geometry type.");
            }
        }
    }
}
