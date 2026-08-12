namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class WayTests
    {
        private User GetTestUser() => new User(1, "username");
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void Constructor_WithTagsAndValidArguments_ShouldCreateWayWithTags()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "highway", "tertiary" } };
            List<long> nodeReferenceIds = new List<long>();
            nodeReferenceIds.Add(1);
            nodeReferenceIds.Add(2);

            // Act
            Way way = new Way(1, true, 1, 1, _testTime, user, nodeReferenceIds, tags);

            // Assert
            Assert.AreEqual(1, way.Id);
            Assert.AreEqual(true, way.Visible);
            Assert.AreEqual(1, way.Version);
            Assert.AreEqual(1, way.ChangeSet);
            Assert.AreEqual(_testTime, way.Timestamp);
            Assert.AreEqual("username", way.User!.Name);
            Assert.AreEqual(1, way.User.Id);
            Assert.AreEqual(1, way.NodeReferenceIds.First());
            Assert.AreEqual(2, way.NodeReferenceIds.Last());
            Assert.AreEqual("tertiary", way.Tags["highway"]);
        }
        

        [TestMethod()]
        public void Constructor_WithoutTagsAndValidArguments_ShouldCreateWayWithEmptyTags()
        {
            // Arrange
            User user = GetTestUser();
            List<long> nodeReferenceIds = new List<long>();
            nodeReferenceIds.Add(2);
            nodeReferenceIds.Add(3);

            // Act
            Way way = new Way(1, true, 1, 1, _testTime, user, nodeReferenceIds);

            // Assert
            Assert.AreEqual(1, way.Id);
            Assert.AreEqual(2, way.NodeReferenceIds.First());
            Assert.AreEqual(3, way.NodeReferenceIds.Last());
            Assert.IsNotNull(way.Tags);
        }

        [TestMethod()]
        public void Constructor_WhenNodeReferanceIdIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Way(1, true, 1, 1, _testTime, user, null!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhitLessThanTwoNodeReferanceIds_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();
            List<long> nodeReferenceIds = new List<long>();
            nodeReferenceIds.Add(1);

            // Act
            Action actual = () => new Way(1, true, 1, 1, _testTime, user, nodeReferenceIds);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }
    }
}