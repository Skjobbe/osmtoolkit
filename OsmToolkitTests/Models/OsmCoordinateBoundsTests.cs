namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class OsmCoordinateBoundsTests
    {
        [TestMethod()]
        public void Constructor_WithValidArguments_ShouldCreateOsmCoordinateBounds()
        {
            // Arrange & Act
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(0, 0, 1, 1);

            // Assert
            Assert.AreEqual(0, bounds.MinimumLatitude);
            Assert.AreEqual(0, bounds.MinimumLongitude);
            Assert.AreEqual(1, bounds.MaximumLatitude);
            Assert.AreEqual(1, bounds.MaximumLongitude);
        }

        [TestMethod()]
        public void Constructor_MinimumLatitudeBelowNegative90_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(-91, 0, 1, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLatitudeAbovePositive90_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(91, 0, 1, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MaximumLatitudeBelowNegative90_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, -91, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MaximumLatitudeAbovePositive90_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, 91, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLongitudeBelowNegative180_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, -181, 1, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLongitudeAbovePositive180_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 181, 1, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MaximumLongitudeBelowNegative180_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, 1, -181);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MaximumLongitudeAbovePositive180_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, 1, 181);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLatitudeEqualsMaximumLatitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, 0, 1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLatitudeAboveMaximumLatitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(1, 0, 0, 0);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLongitudeEqualsMaximumLongitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 0, 1, 0);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_MinimumLongitudeAboveMaximumLongitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmCoordinateBounds(0, 1, 1, 0);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }
    }
}