using OsmToolkit.Factories;
using OsmToolkit.Finders;

namespace OsmToolkit.FindersTests
{
    [TestClass()]
    public class OsmEntityFinderTests
    {
        private OsmData? _osmData;

        [TestInitialize()]
        public void Setup()
        {
            var user = new User(1, "JaneDoe");
            var factory = new OsmEntityFactory();

            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);

            // In-reach nodes
            var node1 = factory.CreateNode(1, true, 1, 1, DateTime.UtcNow, user,
                40.7128, -74.0060, new Dictionary<string, string> { { "highway", "traffic_signals" } });

            var node2 = factory.CreateNode(2, true, 1, 2, DateTime.UtcNow, user,
                40.7130, -74.0058, new Dictionary<string, string> { { "amenity", "bar" } });

            var node3 = factory.CreateNode(3, true, 1, 3, DateTime.UtcNow, user,
                40.7135, -74.0050);

            var node3_5 = factory.CreateNode(10, true, 1, 10, DateTime.UtcNow, user,
                 40.7134, -74.0056);

            // Connected but out-of-reach
            var node4 = factory.CreateNode(4, true, 1, 4, DateTime.UtcNow, user,
                40.7200, -74.0000);

            var node5 = factory.CreateNode(5, true, 1, 5, DateTime.UtcNow, user,
                40.7220, -73.9980);

            var node6 = factory.CreateNode(6, true, 1, 6, DateTime.UtcNow, user,
                40.7250, -73.9950, new Dictionary<string, string> { { "name", "Node Six" } });

            // Far disconnected node
            var nodeFar = factory.CreateNode(990, true, 1, 99, DateTime.UtcNow, user,
                41.5, -74.5, new Dictionary<string, string> { { "name", "FarAway" } });

            // Way within reach
            var way = factory.CreateWay(101, true, 1, 10, DateTime.UtcNow, user,
                new List<long> { node1.Id, node2.Id, node3.Id }, new Dictionary<string, string> { { "highway", "residential" } });

            var way2 = factory.CreateWay(110, true, 1, 10, DateTime.UtcNow, user,
                new List<long> { node3.Id, node3_5.Id },
                new Dictionary<string, string> { { "highway", "residential" } });

            // Way connecting to far but distant nodes
            var way102 = factory.CreateWay(102, true, 1, 11, DateTime.UtcNow, user,
                new List<long> { node3.Id, node4.Id, node5.Id });

            var way103 = factory.CreateWay(103, true, 1, 12, DateTime.UtcNow, user,
                new List<long> { node5.Id, node6.Id },
                new Dictionary<string, string> { { "highway", "service" } });

            // Truly far way
            var wayFar = factory.CreateWay(991, true, 1, 99, DateTime.UtcNow, user,
                new List<long> { nodeFar.Id, 7 });

            var wayFar2 = factory.CreateWay(991, true, 1, 99, DateTime.UtcNow, user,
                new List<long> { 8, 9 });

            // In-reach relation
            var relation = factory.CreateRelation(201, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.way, way.Id, "path"),
                    new Member(ReferenceType.node, node2.Id, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } });

            // Relation tied to way103 (beyond reach)
            var relationFarChain = factory.CreateRelation(202, true, 1, 21, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.way, way103.Id, "segment"),
                    new Member(ReferenceType.node, node6.Id, "stop")
                },
                new Dictionary<string, string> { { "type", "route" }, { "name", "FarRoute" } });

            var relationFar = factory.CreateRelation(993, true, 1, 99, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.way, wayFar.Id, "outer") },
                new Dictionary<string, string> { { "type", "boundary" } });

            _osmData = new OsmData(
                header,
                bounds,
                new[] { node1, node2, node3, node3_5, node4, node5, node6, nodeFar },
                new[] { way, way102, way103, wayFar, wayFar2, way2 },
                new[] { relation, relationFarChain, relationFar });
        }

        [TestMethod()]
        public void FindByTag_ValidKey_ShouldReturnNode()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindByTag(_osmData!, "amenity");

            var expectedIds = new[] { 2L };

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindByTag_WhenDataIsNullAndKeyIsValid_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(null!, "amenity");

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindByTag_WhenKeyIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(_osmData!, null!);

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void FindByTag_WhenKeyIsEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(_osmData!, "");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void FindByTag_ValidKeyAndValue_ShouldReturnNode()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindByTag(_osmData!, "amenity", "bar");

            var expectedIds = new[] { 2L };

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindByTag_WhenDataIsNullAndKeyAndValueAreValid_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(null!, "amenity", "bar");

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindByTag_WhenKeyIsNullWithValidValue_ShouldThrowArgumentException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(_osmData!, null!, "bar");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void FindByTag_WhenKeyIsEmptyStringWithValidValue_ShouldThrowArgumentException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTag(_osmData!, "", "bar");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void FindNearByRadius_ValidArguments_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            var results = finder.FindNearByPathDistance(_osmData!, lat, lon, range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            var expectedIds = new[] { 1L, 2L, 3L, 10L, 101L, 110L };

            var unexpectedIds = new[] { 4L, 5L, 6L, 7L, 103L, 202L, 991L, 992L, 993L };

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));

            foreach (var id in unexpectedIds)
                Assert.IsFalse(entities.Any(e => e.Id == id));

        }

        [TestMethod()]
        public void FindNearByRadius_ValidArgumentsWithNoEntities_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            var results = finder.FindNearByRadius(new OsmData(header, bounds), lat, lon, range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod()]
        public void FindNearByRadius_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            Action actual = () => finder.FindNearByRadius(null!, lat, lon, range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindNearByPathDistance_ValidArguments_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            var results = finder.FindNearByPathDistance(_osmData!, lat, lon, range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            var expectedIds = new[] { 1L, 2L, 3L, 10L, 101L, 102L, 110L, 201L };

            var unexpectedIds = new[] { 4L, 5L, 6L, 7L, 103L, 202L, 991L, 992L, 993L };

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));

            foreach (var id in unexpectedIds)
                Assert.IsFalse(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearByPathDistance_ValidArgumentsWithNoEntities_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            var results = finder.FindNearByPathDistance(new OsmData(header, bounds), lat, lon, range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod()]
        public void FindNearByPathDistance_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060, range = 200;
            Action actual = () => finder.FindNearByPathDistance(null!, lat, lon, range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }
    }
}