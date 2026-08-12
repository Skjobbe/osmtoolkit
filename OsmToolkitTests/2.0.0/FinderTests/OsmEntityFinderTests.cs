using OsmToolkit;
using OsmToolkit.Factories;
using OsmToolkit.Finders;

namespace OsmToolkitTests._2._0._0.FinderTests
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

            var relation2 = factory.CreateRelation(203, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.way, way.Id, "path"),
                    new Member(ReferenceType.node, node1.Id, "stop"),
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
                new[] { relation, relation2, relationFarChain, relationFar });
        }

        [TestMethod()]
        public void FindByOsmId_WhenValidId_ShouldReturnOsmEntity()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var result = finder.FindByOsmId(_osmData!, 1);

            // Assert
            Assert.IsTrue(result is not null);
            Assert.IsTrue(result is OsmEntity);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsOfANode_ShouldReturnNodeId()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            var nodeId = _osmData!.Nodes.First().Id;

            // Act
            OsmEntity? result = finder.FindByOsmId(_osmData!, nodeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nodeId, result.Id);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsOfAWay_ShouldReturnWayId()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            var wayId = _osmData!.Ways.First().Id;

            // Act
            OsmEntity? result = finder.FindByOsmId(_osmData!, wayId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(wayId, result.Id);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsOfARelation_ShouldReturnRelationId()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            var relationId = _osmData!.Relations.First().Id;

            // Act

            OsmEntity? result = finder.FindByOsmId(_osmData!, relationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(relationId, result.Id);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsNotInData_ShouldReturnRelationId()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            var entityId = 1000;

            // Act
            OsmEntity? result = finder.FindByOsmId(_osmData!, entityId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void FindByOsmId_WhenDataIsNullAndIdIsValid_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByOsmId(null!, 1);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsZero_ShouldThrowArgumenOutOfRangeException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByOsmId(_osmData!, 0);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void FindByOsmId_WhenIdIsBelowZero_ShouldThrowArgumenOutOfRangeException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByOsmId(_osmData!, -1);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void FindByTags_WhenValidKeyAndEmptyValue_ShouldReturnOsmData()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("amenity", "");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

            // Assert
            Assert.IsTrue(results is not null);
            Assert.IsTrue(results is OsmData);
        }

        [TestMethod()]
        public void FindByTags_ValidKeyAndValue_ShouldReturnOsmData()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("amenity", "bar");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

            // Assert
            Assert.IsTrue(results is not null);
            Assert.IsTrue(results is OsmData);
        }

        [TestMethod()]
        public void FindByTags_WhenKeyIsAmenity_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("amenity", "");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

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
        public void FindByTags_WhenKeyIsHighway_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("highway", "");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

            var expectedIds = new[] { 101L, 110L, 103L };

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindByTags_WhenKeyIsTypeAndValueIsRoute_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("type", "route");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

            var expectedIds = new[] { 201L, 202L };

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindByTags_WhenKeysAreTypeAndNameAndValuesAreRouteAndFarRoute_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("type", "route");
            tags.Add("name", "FarRoute");

            // Act
            var results = finder.FindByTags(_osmData!, tags);

            var expectedIds = new[] { 202L };

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            foreach (var id in expectedIds)
                Assert.IsTrue(entities.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindByTags_WhenDataIsNullAndKeyIsValid_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("amenity", "");

            // Act
            Action actual = () => finder.FindByTags(null!, tags);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindByTags_WhenTagsIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual = () => finder.FindByTags(_osmData!, null!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindNearestNode_WhenValidArgumentsAtNodeCoordinate_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Node? actual1 = finder.FindNearestNode(_osmData!, lat, lon);
            Node? actual2 = finder.FindNearestNode(_osmData!, _osmData!.Nodes[0]);

            var expectedId = 1;

            // Assert
            Assert.IsNotNull(actual1);
            Assert.IsNotNull(actual2);
            Assert.AreEqual(expectedId, actual1.Id);
            Assert.AreEqual(expectedId, actual2.Id);
        }

        [TestMethod()]
        public void FindNearestNode_WhenValidArgumentsAreNotAtNodeCoordinate_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7129, lon = -74.0058;
            Node? actual = finder.FindNearestNode(_osmData!, lat, lon);

            var expectedId = 2;

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedId, actual.Id);
        }

        [TestMethod()]
        public void FindNearestNode_WhenValidArgumentsIncludeTags_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();
            var tags = new Dictionary<string, string>() { { "amenity", "bar" } };


            // Act
            double lat = 40.7130, lon = -74.0058;
            Node? actual1 = finder.FindNearestNode(_osmData!, lat, lon, tags);
            Node? actual2 = finder.FindNearestNode(_osmData!, _osmData!.Nodes[1], tags);

            var expectedId = 2;

            // Assert
            Assert.IsNotNull(actual1);
            Assert.IsNotNull(actual2);
            Assert.AreEqual(expectedId, actual1.Id);
            Assert.AreEqual(expectedId, actual2.Id);
        }

        [TestMethod()]
        public void FindNearestNode_WhenValidArgumentsButNoNodesInData_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Node? actual = finder.FindNearestNode(new OsmData(header, bounds), lat, lon);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void FindNearestNode_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Action actual1 = () => finder.FindNearestNode(null!, _osmData!.Nodes[0]);
            Action actual2 = () => finder.FindNearestNode(null!, _osmData!.Nodes[0], new Dictionary<string, string>());
            Action actual3 = () => finder.FindNearestNode(null!, lat, lon);
            Action actual4 = () => finder.FindNearestNode(null!, lat, lon, new Dictionary<string, string>());

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
            Assert.ThrowsException<ArgumentNullException>(actual3);
            Assert.ThrowsException<ArgumentNullException>(actual4);
        }

        [TestMethod()]
        public void FindNearestNode_WhenNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual1 = () => finder.FindNearestNode(_osmData!, null!);
            Action actual2 = () => finder.FindNearestNode(_osmData!, null!, new Dictionary<string, string>());

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
        }

        [TestMethod()]
        public void FindNearestNode_WhenTagsIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Action actual1 = () => finder.FindNearestNode(_osmData!, _osmData!.Nodes[0], null!);
            Action actual2 = () => finder.FindNearestNode(_osmData!, lat, lon, null!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenValidArgumentsWithLimitAsOne_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            var actual1 = finder.FindNearbyNodes(_osmData!, _osmData!.Nodes[0], 1);
            var actual2 = finder.FindNearbyNodes(_osmData!, lat, lon, 1);
            var actual3 = finder.FindNearbyNodes(_osmData!, _osmData!.Nodes[0], 1, new Dictionary<string, string>(), false, false);
            var actual4 = finder.FindNearbyNodes(_osmData!, lat, lon, 1, new Dictionary<string, string>(), false, false);

            var expectedId = 1;

            // Assert
            Assert.AreEqual(1, actual1.Count);
            Assert.AreEqual(1, actual2.Count);
            Assert.AreEqual(1, actual3.Count);
            Assert.AreEqual(1, actual4.Count);
            Assert.IsNotNull(actual1[0]);
            Assert.IsNotNull(actual2[0]);
            Assert.IsNotNull(actual3[0]);
            Assert.IsNotNull(actual4[0]);
            Assert.AreEqual(expectedId, actual1[0].Id);
            Assert.AreEqual(expectedId, actual2[0].Id);
            Assert.AreEqual(expectedId, actual3[0].Id);
            Assert.AreEqual(expectedId, actual4[0].Id);
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenLimitAsTwo_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            var actual = finder.FindNearbyNodes(_osmData!, lat, lon, 2, new Dictionary<string, string>(), true, true);

            var expectedIds = new[] { 1L, 2L };

            // Assert
            Assert.AreEqual(2, actual.Count);
            Assert.IsNotNull(actual[0]);
            Assert.IsNotNull(actual[1]);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenLimitAsThreeAndFarAwayCoordinate_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 41.5, lon = -74.5;
            var actual = finder.FindNearbyNodes(_osmData!, lat, lon, 3, new Dictionary<string, string>(), false, false);

            var expectedIds = new[] { 990L, 6L, 4L };

            // Assert
            Assert.AreEqual(3, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenLimitLargerThanNodes_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 41.5, lon = -74.5;
            var actual = finder.FindNearbyNodes(_osmData!, lat, lon, 10, new Dictionary<string, string>(), false, false);

            var expectedIds = new[] { 990L, 6L, 4L, 10L, 2L };

            // Assert
            Assert.AreEqual(5, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenLimitHigherThanNodeCount_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 41.5, lon = -74.5;
            var actual = finder.FindNearbyNodes(_osmData!, lat, lon, _osmData!.Nodes.Count+1, new Dictionary<string, string>(), true, true);

            var expectedIds = new[] { 1L, 2L, 3L, 4L, 5L, 6L, 10L, 990L};

            // Assert
            Assert.AreEqual(8, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenNotAllowSameWay_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            var relations = new List<Relation>()
            {
                new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.node, 1, "start"),
                    new Member(ReferenceType.node, 2, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            var actual = finder.FindNearbyNodes(new OsmData(header, bounds, nodes, ways, relations), lat, lon, 3, null, false, true);

            var expectedIds = new[] { 1L, 2L };

            // Assert
            Assert.AreEqual(2, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));

            Assert.IsFalse(actual.Any(e => e.Id == 3));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenNotAllowSameRelation_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            var relations = new List<Relation>()
            {
                new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.node, 1, "start"),
                    new Member(ReferenceType.node, 2, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            var actual = finder.FindNearbyNodes(new OsmData(header, bounds, nodes, ways, relations), lat, lon, 3, null, true, false);

            var expectedIds = new[] { 1L, 3L };

            // Assert
            Assert.AreEqual(2, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));

            Assert.IsFalse(actual.Any(e => e.Id == 2));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenNotAllowSameWayAndRelation_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            var relations = new List<Relation>()
            {
                new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.node, 1, "start"),
                    new Member(ReferenceType.node, 2, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            var actual = finder.FindNearbyNodes(new OsmData(header, bounds, nodes, ways, relations), lat, lon, 3, null, false, false);

            var expectedIds = new[] { 1L, 4L };
            var unexpectedIds = new[] { 2L, 3L };

            // Assert
            Assert.AreEqual(2, actual.Count);

            foreach (var id in expectedIds)
                Assert.IsTrue(actual.Any(e => e.Id == id));

            foreach (var id in unexpectedIds)
                Assert.IsFalse(actual.Any(e => e.Id == id));
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Action actual1 = () => finder.FindNearbyNodes(null!, _osmData!.Nodes[0], 1);
            Action actual2 = () => finder.FindNearbyNodes(null!, lat, lon, 1);
            Action actual3 = () => finder.FindNearbyNodes(null!, _osmData!.Nodes[0], 1, new Dictionary<string, string>(), false, false);
            Action actual4 = () => finder.FindNearbyNodes(null!, lat, lon, 1, new Dictionary<string, string>(), false, false);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
            Assert.ThrowsException<ArgumentNullException>(actual3);
            Assert.ThrowsException<ArgumentNullException>(actual4);
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual1 = () => finder.FindNearbyNodes(_osmData!, null!, 1);
            Action actual2 = () => finder.FindNearbyNodes(_osmData!, null!, 1, new Dictionary<string, string>(), false, false);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
        }

        [TestMethod()]
        public void FindNearbyNodes_WhenLimitIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double lat = 40.7128, lon = -74.0060;
            Action actual1 = () => finder.FindNearbyNodes(_osmData!, _osmData!.Nodes[0], 0);
            Action actual2 = () => finder.FindNearbyNodes(_osmData!, lat, lon, 0);
            Action actual3 = () => finder.FindNearbyNodes(_osmData!, _osmData!.Nodes[0], 0, new Dictionary<string, string>(), false, false);
            Action actual4 = () => finder.FindNearbyNodes(_osmData!, lat, lon, 0, new Dictionary<string, string>(), false, false);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual3);
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual4);
        }

        [TestMethod()]
        public void FindNearByRadius_WhenValidArguments_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            var results = finder.FindNearByRadius(_osmData!, _osmData!.Nodes[0], range);

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
        public void FindNearByRadius_WhenRelationJustHasNode_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            var relations = new List<Relation>()
            {
                new Relation(201, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.node, 2, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            var results = finder.FindNearByRadius(new OsmData(header, bounds, nodes, ways, relations), nodes[0], range);

            // Assert
            Assert.AreEqual(201, results.Relations[0].Id);

        }

        [TestMethod()]
        public void FindNearByRadius_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            Action actual = () => finder.FindNearByRadius(null!, _osmData!.Nodes[0], range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindNearByRadius_WhenNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var range = 200;
            Action actual = () => finder.FindNearByRadius(_osmData!, null!, range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindNearByPathDistance_WhenValidArguments_ShouldReturnExpectedResults()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            var results = finder.FindNearByPathDistance(_osmData!, _osmData!.Nodes[0], range);

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
        public void FindNearByPathDistance_WhenValidArgumentsWithNoEntities_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            var results = finder.FindNearByPathDistance(new OsmData(header, bounds), _osmData!.Nodes[0], range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod()]
        public void FindNearByPathDistance_WhenValidArgumentsWithNoPaths_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            double range = 200;
            var results = finder.FindNearByPathDistance(new OsmData(header, bounds, nodes, null!, null!), _osmData!.Nodes[0], range);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Nodes);
            entities.AddRange(results.Ways);
            entities.AddRange(results.Relations);

            // Assert
            Assert.AreEqual(1, entities.Count);
        }

        [TestMethod()]
        public void FindNearByPathDistance_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var range = 200;
            Action actual = () => finder.FindNearByPathDistance(null!, _osmData!.Nodes[0], range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindNearByPathDistance_WhenNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var range = 200;
            Action actual = () => finder.FindNearByPathDistance(_osmData!, null!, range);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidArgumentsWithNoEntities_ShouldThrowArgumentNullException()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds), _osmData!.Nodes[0], _osmData!.Nodes[0]);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidArgumentsCannotFindStartOrTargetNode_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>() { 
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058)
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, null, null), _osmData!.Nodes[0], _osmData!.Nodes[1]);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
            Assert.AreEqual("No valid path could be found.", results.Description);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidNonConnectedPaths_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>() 
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058)
            };
            var ways = new List<Way>() 
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 4 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1]);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
            Assert.AreEqual("No valid path could be found.", results.Description);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidConnectedPaths_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "residential" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "residential" } })
            };
            var relations = new List<Relation>()
            {
                new Relation(201, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.way, 101, "path"),
                    new Member(ReferenceType.node, 2, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } }),
                new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member>
                {
                    new Member(ReferenceType.node, 3, "start"),
                    new Member(ReferenceType.node, 4, "stop")
                },
                new Dictionary<string, string> { { "type", "route" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, relations), _osmData!.Nodes[0], _osmData!.Nodes[1]);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(2, results.Data.Ways.Count);
            Assert.AreEqual(2, results.Data.Relations.Count);
            Assert.IsNotNull(results.StartNode);
            Assert.IsNotNull(results.EndNode);
            Assert.AreEqual(nodes[0].Id, results.StartNode.Id);
            Assert.AreEqual(nodes[1].Id, results.EndNode.Id);
        }

        [TestMethod()]
        public void FindShortestPath_WhenNonValidConnectedPaths_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
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
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null!), _osmData!.Nodes[0], _osmData!.Nodes[1]);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
            Assert.AreEqual("No valid path could be found.", results.Description);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidConnectedPathsAndAvoidMotorwayOptionIsTrue_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "motorway" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "motorway" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Any, true);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null!), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
            Assert.AreEqual("No valid path could be found.", results.Description);
        }

        [TestMethod()]
        public void FindShortestPath_WhenValidConnectedPathsWithAccessNoTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3 }, new Dictionary<string, string> { { "highway", "road" }, { "access", "no" } }),
                new Way(110, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "access", "no" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null!), _osmData!.Nodes[0], _osmData!.Nodes[1]);

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(results.Data.Nodes);
            entities.AddRange(results.Data.Ways);
            entities.AddRange(results.Data.Relations);

            // Assert
            Assert.AreEqual(0, entities.Count);
            Assert.AreEqual("No valid path could be found.", results.Description);
        }

        [TestMethod()]
        public void FindShortestPath_WhenFootModeAndValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "foot", "yes" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Foot, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenFootModeAndNonValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "motorway" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "foot", "no" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Foot, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(0, results.Data.Nodes.Count);
            Assert.AreEqual(0, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenBicycleModeAndValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "bicycle", "yes" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Bicycle, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenBicycleModeAndNonValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "motorway" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "bicycle", "no" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Bicycle, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(0, results.Data.Nodes.Count);
            Assert.AreEqual(0, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenMopedModeAndValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "motor_vehicle", "yes" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Moped, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenMopedModeAndNonValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "motorway" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "motor_vehicle", "no" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Moped, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(0, results.Data.Nodes.Count);
            Assert.AreEqual(0, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenMotorCarModeAndValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 3, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "motor_vehicle", "yes" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.MotorCar, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenMotorCarModeAndNonValidTag_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "bridleway" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" }, { "motor_vehicle", "no" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.MotorCar, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(0, results.Data.Nodes.Count);
            Assert.AreEqual(0, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenOneWayAndFootMode_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "road" }, { "oneway", "yes" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Foot, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenOneWayAndAnyMode_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059),
                new Node(4, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0060),
                new Node(5, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 2, 3, 1 }, new Dictionary<string, string> { { "highway", "road" }, { "oneway", "yes" } }),
                new Way(102, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 1, 4, 5, 2 }, new Dictionary<string, string> { { "highway", "road" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();
            PathOptions pathOptions = new PathOptions(TravelMode.Any, false);

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], pathOptions);

            // Assert
            Assert.AreEqual(3, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
        }

        [TestMethod()]
        public void FindShortestPath_WhenRoundaboutAndNullOptions_ShouldReturnExpectedResults()
        {
            // Arrange
            var header = new OsmHeader(0.6, "UnitTestGen", "OSM Testers", "https://osm.test/copyright", "https://osm.test/license");
            var bounds = new OsmCoordinateBounds(40.0, -76.0, 42.0, -73.0);
            var user = new User(1, "JaneDoe");
            var nodes = new List<Node>()
            {
                new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060),
                new Node(2, true, 1, 1, DateTime.UtcNow, user, 40.7130, -74.0058),
                new Node(3, true, 1, 1, DateTime.UtcNow, user, 40.7129, -74.0059)
            };
            var ways = new List<Way>()
            {
                new Way(101, true, 1, 10, DateTime.UtcNow, user,
                    new List<long> { 3, 2, 1, 3 }, new Dictionary<string, string> { { "highway", "road" }, { "junction", "roundabout" } })
            };
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            var results = finder.FindShortestPath(new OsmData(header, bounds, nodes, ways, null), _osmData!.Nodes[0], _osmData!.Nodes[1], null!);

            // Assert
            Assert.AreEqual(2, results.Data.Nodes.Count);
            Assert.AreEqual(1, results.Data.Ways.Count);
            Assert.IsTrue(results.Data.Ways[0].Tags.ContainsKey("oneway"));
            Assert.AreEqual("yes", results.Data.Ways[0].Tags["oneway"]);
        }

        [TestMethod()]
        public void FindShortestPath_WhenDataIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual1 = () => finder.FindShortestPath(null!, _osmData!.Nodes[0], _osmData!.Nodes[0]);
            Action actual2 = () => finder.FindShortestPath(null!, 0, 0, 0, 0);
            Action actual3 = () => finder.FindShortestPath(null!, _osmData!.Nodes[0], _osmData!.Nodes[0], new PathOptions());
            Action actual4 = () => finder.FindShortestPath(null!, 0, 0, 0, 0, new PathOptions());

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
            Assert.ThrowsException<ArgumentNullException>(actual3);
            Assert.ThrowsException<ArgumentNullException>(actual4);
        }

        [TestMethod()]
        public void FindShortestPath_WhenStartNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual1 = () => finder.FindShortestPath(_osmData!, null!, _osmData!.Nodes[0]);
            Action actual2 = () => finder.FindShortestPath(_osmData!, null!, _osmData!.Nodes[0], new PathOptions());

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
        }

        [TestMethod()]
        public void FindShortestPath_WhenEndNodeIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmEntityFinder finder = new OsmEntityFinder();

            // Act
            Action actual1 = () => finder.FindShortestPath(_osmData!, _osmData!.Nodes[0], null!);
            Action actual2 = () => finder.FindShortestPath(_osmData!, _osmData!.Nodes[0], null!, new PathOptions());

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual1);
            Assert.ThrowsException<ArgumentNullException>(actual2);
        }
    }
}
