namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class RelationTests
    {
        private User GetTestUser() => new User(1, "username");
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void Constructor_WithTagsAndValidArguments_ShouldCreateRelationWithTags()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            List<Member> members = new List<Member>();
            members.Add(new Member(ReferenceType.node, 2, "from"));

            // Act
            Relation relation = new Relation(1, true, 1, 1, _testTime, user, members, tags);

            // Assert
            Assert.AreEqual(1, relation.Id);
            Assert.IsTrue(relation.Visible);
            Assert.AreEqual(1, relation.Version);
            Assert.AreEqual(1, relation.ChangeSet);
            Assert.AreEqual(_testTime, relation.Timestamp);
            Assert.AreEqual(user, relation.User);
            Assert.AreEqual(members[0], relation.Members[0]);
            Assert.AreEqual("restaurant", relation.Tags["amenity"]);
        }
        

        [TestMethod()]
        public void Constructor_WithoutTagsAndValidArguments_ShouldCreateRelationWithEmptyTags()
        {
            // Arrange
            User user = GetTestUser();
            List<Member> members = new List<Member>();
            members.Add(new Member(ReferenceType.node, 2, "from"));

            // Act
            Relation relation = new Relation(1, true, 1, 1, _testTime, user, members);

            // Assert
            Assert.AreEqual(1, relation.Id);
            Assert.IsTrue(relation.Visible);
            Assert.AreEqual(1, relation.Version);
            Assert.AreEqual(1, relation.ChangeSet);
            Assert.AreEqual(_testTime, relation.Timestamp);
            Assert.AreEqual(user, relation.User);
            Assert.AreEqual(members[0], relation.Members[0]);
            Assert.IsNotNull(relation.Tags);
        }

        [TestMethod()]
        public void Constructor_WhenMemberListIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new Relation(1, true, 1, 1, _testTime, user, null!);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(actual);
        }

        [TestMethod()]
        public void Constructor_WithLessThanOneElementInMemberList_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();
            List<Member> members = new List<Member>();

            // Act
            Action actual = () => new Relation(1, true, 1, 1, _testTime, user, members);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Members_AddValidMemberToExistingRelation_ShouldIncreaseCountAndContainNewMember()
        {
            // Arrange
            User user = GetTestUser();
            List<Member> members = new List<Member>();
            members.Add(new Member(ReferenceType.node, 2, "from"));
            var member1 = new Member(ReferenceType.way, 3, "via");

            // Act
            Relation relation = new Relation(1, true, 1, 1, _testTime, user, members);
            relation.Members.Add(member1);

            // Assert
            CollectionAssert.Contains(relation.Members.ToList(), member1);
            Assert.AreEqual(2, relation.Members.Count);
        }
    }
}