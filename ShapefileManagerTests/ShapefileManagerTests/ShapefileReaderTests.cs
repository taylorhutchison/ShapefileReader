using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GISData.GeometryTypes;

namespace ShapefileManagerTests
{
    [TestClass]
    public class ShapefileReaderTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Point p = new Point(1, 2);
            Assert.AreEqual(5, p.X);
        }
    }
}
