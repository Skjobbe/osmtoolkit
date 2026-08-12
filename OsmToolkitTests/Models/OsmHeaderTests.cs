namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class OsmHeaderTests
    {
        [TestMethod()]
        public void Constructor_WithValidArguments_ShouldCreateOsmHeader()
        {
            // Arrange & Act
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");

            // Assert
            Assert.AreEqual(0.4, header.Version);
            Assert.AreEqual("test-generator", header.Generator);
            Assert.AreEqual("test-copyright", header.Copyright);
            Assert.AreEqual("http://test_attr.org", header.AttributionUrl);
            Assert.AreEqual("http://test_license.org", header.LicenseUrl);
        }
        

        [TestMethod()]
        public void Constructor_WhenVersionIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmHeader(0, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenVersionIsNegative_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new OsmHeader(-1, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenGeneratorIsNull_ShouldCreateOsmHeaderWithEmptyGenerator()
        {
            // Arrange & Act
            OsmHeader header = new OsmHeader(0.4, null, "test-copyright", "http://test_attr.org", "http://test_license.org");

            // Assert
            Assert.AreEqual("", header.Generator);
        }

        [TestMethod()]
        public void Constructor_WhenCopyrightIsNull_ShouldCreateOsmHeaderWithEmptyCopyright()
        {
            // Arrange & Act
            OsmHeader header = new OsmHeader(0.4, "test-generator", null, "http://test_attr.org", "http://test_license.org");

            // Assert
            Assert.AreEqual("", header.Copyright);
        }

        [TestMethod()]
        public void Constructor_WhenAttributionIsNull_ShouldCreateOsmHeaderWithEmptyAttribution()
        {
            // Arrange & Act
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", null, "http://test_license.org");

            // Assert
            Assert.AreEqual("", header.AttributionUrl);
        }

        [TestMethod()]
        public void Constructor_WhenLicenseIsNull_ShouldCreateOsmHeaderWithEmptyLicense()
        {
            // Arrange & Act
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", null);

            // Assert
            Assert.AreEqual("", header.LicenseUrl);
        }
    }
}