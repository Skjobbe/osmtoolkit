using OsmToolkit;

namespace OsmToolkitTests._2._0._0.Models
{
    [TestClass()]
    public class OsmPathTests
    {

        [TestMethod()]
        public void Constructor_WithDataAndTotalDistance_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var data = new OsmData(header, bounds);

            // Act
            var osmPath = new OsmPath(data, 0);

            // Assert
            Assert.AreEqual(data, osmPath.Data);
            Assert.AreEqual(0, osmPath.TotalDistance);
            Assert.IsNull(osmPath.Description);
            Assert.IsNull(osmPath.StartNode);
            Assert.IsNull(osmPath.EndNode);
        }

        [TestMethod()]
        public void Constructor_WithStartNodeInData_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var node = new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                node,
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 })
            };
            var data = new OsmData(header, bounds, nodes, ways, null!);

            // Act
            var osmPath = new OsmPath(data, 0, node, null!);

            // Assert
            Assert.IsNotNull(osmPath.StartNode);
            Assert.IsNull(osmPath.EndNode);
        }

        [TestMethod()]
        public void Constructor_WithStartNodeNotInData_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            var nodes = new List<Node>()
            {
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 })
            };
            var data = new OsmData(header, bounds, nodes, ways, null!);

            // Act
            var osmPath = new OsmPath(data, 0, node, null!);

            // Assert
            Assert.IsNull(osmPath.StartNode);
        }

        [TestMethod()]
        public void Constructor_WithEndNodeInData_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var node = new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058);
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                node,
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 })
            };
            var data = new OsmData(header, bounds, nodes, ways, null!);

            // Act
            var osmPath = new OsmPath(data, 0, null!, node);

            // Assert
            Assert.IsNull(osmPath.StartNode);
            Assert.IsNotNull(osmPath.EndNode);
        }

        [TestMethod()]
        public void Constructor_WithEndNodeNotInData_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var node = new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058);
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 })
            };
            var data = new OsmData(header, bounds, nodes, ways, null!);

            // Act
            var osmPath = new OsmPath(data, 0, null!, node);

            // Assert
            Assert.IsNull(osmPath.EndNode);
        }

        [TestMethod()]
        public void Constructor_WithDescription_ShouldCreateOsmPath()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var data = new OsmData(header, bounds);

            // Act
            var osmPath1 = new OsmPath(data, 0, "Test.");
            var osmPath2 = new OsmPath(data, 0, null, null, "Test.");

            // Assert
            Assert.AreEqual("Test.", osmPath1.Description);
            Assert.AreEqual("Test.", osmPath2.Description);
        }

        [TestMethod()]
        public void Constructor_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            Action actual = () => new OsmPath(null!, 0);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenTotalDistanceIsLowerThanZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var data = new OsmData(header, bounds);

            // Arrange & Act
            Action actual = () => new OsmPath(data, -1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }
    }
}
