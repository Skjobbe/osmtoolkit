namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class NodeTests
    {
        private User GetTestUser() => new User(1, "username");

        [TestMethod()]
        public void Constructor_WithValidArgumentsWithTags_ShouldCreateNode()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            var dateTime = new DateTime(2025, 1, 1, 12, 15, 30);

            // Act
            Node node = new Node(1, true, 1, 1, dateTime, user, 0, 0, tags);

            // Assert
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual(true, node.Visible);
            Assert.AreEqual(1, node.Version);
            Assert.AreEqual(1, node.ChangeSet);
            Assert.AreEqual(dateTime, node.Timestamp);
            Assert.AreEqual(user, node.User);
            Assert.AreEqual("restaurant", node.Tags["amenity"]);
            Assert.AreEqual(0, node.Latitude);
            Assert.AreEqual(0, node.Longitude);
        }

        [TestMethod()]
        public void Constructor_WithValidArgumentsWithoutTags_ShouldCreateNodeWithEmptyTags()
        {
            // Arrange
            User user = GetTestUser();
            var dateTime = new DateTime(2025, 1, 1, 12, 15, 30);

            // Act
            Node node = new Node(1, true, 1, 1, dateTime, user, 0, 0);

            // Assert
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual(true, node.Visible);
            Assert.AreEqual(1, node.Version);
            Assert.AreEqual(1, node.ChangeSet);
            Assert.AreEqual(dateTime, node.Timestamp);
            Assert.AreEqual(user, node.User);
            Assert.AreEqual(0, node.Latitude);
            Assert.AreEqual(0, node.Longitude);
            Assert.IsNotNull(node.Tags);

        }

        [TestMethod()]
        public void Constructor_WithLatitudeBelowNegative90_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Node(1, true, 1, 1, DateTime.Now, user, -91, 0, null);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WithLatitudeAbovePositive90_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Node(1, true, 1, 1, DateTime.Now, user, 91, 0, null);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WithLongitudeBelowNegative180_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Node(1, true, 1, 1, DateTime.Now, user, 0, -181, null);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WithLongitudeAbovePositive180_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Node(1, true, 1, 1, DateTime.Now, user, 0, 181, null);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }
    }
}