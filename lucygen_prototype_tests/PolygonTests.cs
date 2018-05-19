using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Lucygen.UnitTests
{
    class PolygonTests
    {
        [TestCase(new int[] { 2, 3, 1 }, true)]
        [TestCase(new int[] { 1, 4, 0 }, true)]
        [TestCase(new int[] { 0, 2, 5 }, true)]
        [TestCase(new int[] { 1, 6, 7 }, false)]
        [TestCase(new int[] { 0, 8, 9 }, false)]
        [TestCase(new int[] { 4, 10, 11 }, false)]

        public void IsNeighborOf_PolygonArgument_ValidReturn(int[] polyCoords, bool expected)
        {
            //Arrange
            Polygon subjectPoly = MakePolygon(0, 1, 2);
            Polygon neighborPoly = MakePolygon(polyCoords[0], polyCoords[1], polyCoords[2]);

            //Act
            bool result = neighborPoly.IsNeighborOf(subjectPoly);

            //Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    
        public static Polygon MakePolygon(int a, int b, int c)
        {
            return new Polygon(a, b, c);
        }
    }
}
