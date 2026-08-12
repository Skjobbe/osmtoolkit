namespace OsmToolkit.Tests
{
    [TestClass()]
    public class OsmDataTests
        
    {
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);
        private User GetTestUser() => new User(1, "username");
        private List<Node>? _nodes;
        private List<Way>? _ways;
        private List<Relation>? _relations;
        private List<Member>? _members;

        [TestInitialize]
        public void Setup()
        {
            var user = GetTestUser();
            var node1 = new Node(1, true, 1, 1, _testTime, user, 10, 20);
            var node2 = new Node(2, true, 1, 1, _testTime, user, 10, 20);

            List<Node> nodes = new List<Node>();
            nodes.Add(node1);
            nodes.Add(node2);
            _nodes = nodes;

            List<long> nodeReferenceIds = new List<long>();
            nodeReferenceIds.Add(node1.Id);
            nodeReferenceIds.Add(node2.Id);

            var way = new Way(1, true, 1, 1, _testTime, user, nodeReferenceIds);
            List<Way> ways = new List<Way>();
            _ways = ways;

            var member1 = new Member(ReferenceType.node, node1.Id);
            var member2 = new Member(ReferenceType.node, node2.Id);

            List<Member> members = new List<Member>();
            members.Add(member1);
            members.Add(member2); 
            _members = members;

            var relation = new Relation(1, true, 1, 1, _testTime, user, members);
            List<Relation> relations = new List<Relation>();
            _relations = relations;
        }

        [TestMethod()]
        public void Constructor_WithoutOsmEntitiesAndValidArguments_ShouldCreateOsmDataWhitoutOsmEntity()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);

            // Act
            OsmData data = new OsmData(header, bounds);

            // Assert
            Assert.AreEqual(0.4, data.Header.Version);   
            Assert.AreEqual(bounds, data.Bounds);

        }

        [TestMethod()]
        public void Constructor_WithOsmEntitiesAndValidArguments_ShouldCreateOsmDataWithOsmEntity()
        {
            // Arrange
            User user = GetTestUser();
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);

            // Act
            OsmData data = new OsmData(header, bounds, _nodes, _ways, _relations);

            // Assert
            Assert.AreEqual(header, data.Header);
            Assert.AreEqual(bounds, data.Bounds);
            CollectionAssert.AreEqual(_nodes, data.Nodes.ToList());
            CollectionAssert.AreEqual(_ways, data.Ways.ToList());
            CollectionAssert.AreEqual(_relations, data.Relations.ToList());

        }

        [TestMethod()]
        public void Constructor_WhenHeaderIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(0, 0, 1, 1);

            // Act
            Action actual = () => new OsmData(null!, bounds);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenNodesAreNullOrEmpty_ShouldCreateOsmDataWithEmptyListOfNodes()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);

            // Act
            var osmData =  new OsmData(header, bounds, null, _ways, _relations);

            // Assert
            Assert.IsNotNull(osmData);
        }

        [TestMethod()]
        public void Constructor_WhenWaysAreNullOrEmpty_ShouldCreateOsmDataWithEmptyListOfWays()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);

            // Act
            var osmData = new OsmData(header, bounds, _nodes, null, _relations);

            // Assert
            Assert.IsNotNull(osmData);
        }

        [TestMethod()]
        public void Constructor_WhenRelationsAreNullOrEmpty_ShouldCreateOsmDataWithEmptyListOfRelations()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);

            // Act
            var osmData = new OsmData(header, bounds, _nodes, _ways, null);

            // Assert
            Assert.IsNotNull(osmData);
        }

        [TestMethod()]
        public void Constructor_WhenAddingNewNodeToList_ShouldAddNewNode()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);
            var user = GetTestUser();
            var node = new Node(3, true, 1, 1, _testTime, user, 10, 20);

            // Act
            var osmData = new OsmData(header, bounds, _nodes, _ways, null);
            osmData.Nodes.Add(node);

            // Assert
            CollectionAssert.Contains(osmData.Nodes.ToList(), node);
        }

        [TestMethod()]
        public void Constructor_WhenAddingNewWayToList_ShouldAddNewWay()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);
            var user = GetTestUser();
            List<long> nodeReferenceIds = new List<long>();
            nodeReferenceIds.Add(1);
            nodeReferenceIds.Add(2);
            var way = new Way(1, true, 1, 1, _testTime, user, nodeReferenceIds);

            // Act
            var osmData = new OsmData(header, bounds, _nodes, _ways, null);
            osmData.Ways.Add(way);

            // Assert
            CollectionAssert.Contains(osmData.Ways.ToList(), way);
        }

        [TestMethod()]
        public void Constructor_WhenAddingNewRelationToList_ShouldAddNewRelation()
        {
            // Arrange
            OsmHeader header = new OsmHeader(0.4, "test-generator", "test-copyright", "http://test_attr.org", "http://test_license.org");
            OsmCoordinateBounds bounds = new OsmCoordinateBounds(10, 10, 20, 20);
            var user = GetTestUser();
            var relation = new Relation(1, true, 1, 1, _testTime, user, _members!);

            // Act
            var osmData = new OsmData(header, bounds, _nodes, _ways, null);
            osmData.Relations.Add(relation);

            // Assert
            CollectionAssert.Contains(osmData.Relations.ToList(), relation);
        }
    }
}