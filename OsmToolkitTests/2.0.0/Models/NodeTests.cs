using OsmToolkit;

namespace OsmToolkitTests._2._0._0.Models
{
    [TestClass()]
    public class NodeTests
    {
        private User GetTestUser() => new User(1, "username");
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void IsPartOf_WhenIsPartOfWay_ShouldReturnTrue()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way way = new Way(101, true, 1, 10, DateTime.UtcNow, user, new List<long> { 1, 2 });

            // Act
            var actual = node.IsPartOf(way);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenIsNotPartOfWay_ShouldReturnFalse()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way way = new Way(101, true, 1, 10, DateTime.UtcNow, user, new List<long> { 2, 3 });

            // Act
            var actual = node.IsPartOf(way);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenWayIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way? way = null;

            // Act
            Action actual = () => node.IsPartOf(way!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenIsPartOfRelation_ShouldReturnTrue()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Relation relation = new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.node, 1) });

            // Act
            var actual = node.IsPartOf(relation);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenIsNotPartOfRelation_ShouldReturnFalse()
        {
            // Arrange
            var user = GetTestUser();
            var node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            var relation = new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.node, 2) });

            // Act
            var actual = node.IsPartOf(relation);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenRelationIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var user = GetTestUser();
            var node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Relation? relation = null;

            Action actual = () => node.IsPartOf(relation!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenIsPartOfWayAndRelation_ShouldReturnTrue()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way way = new Way(101, true, 1, 10, DateTime.UtcNow, user, new List<long> { 1, 2 });
            Relation relation = new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.node, 1) });

            // Act
            var actual = node.IsPartOf(way, relation);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenIsNotPartOfWayOrRelation_ShouldReturnFalse()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way way = new Way(101, true, 1, 10, DateTime.UtcNow, user, new List<long> { 2, 3 });
            Relation relation = new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.node, 2) });

            // Act
            var actual = node.IsPartOf(way, relation);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenWayIsNullButRelationIsNotNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way? way = null;
            Relation relation = new Relation(202, true, 1, 20, DateTime.UtcNow, user,
                new List<Member> { new Member(ReferenceType.node, 1) });

            // Act
            Action actual = () => node.IsPartOf(way!, relation);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void IsPartOf_WhenWayIsNotNullButRelationIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var user = GetTestUser();
            Node node = new Node(1, true, 1, 1, DateTime.UtcNow, user, 40.7128, -74.0060);
            Way way = new Way(101, true, 1, 10, DateTime.UtcNow, user, new List<long> { 1, 2 });
            Relation? relation = null;

            // Act
            Action actual = () => node.IsPartOf(way, relation!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

    }
}
