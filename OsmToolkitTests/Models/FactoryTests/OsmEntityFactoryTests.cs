using OsmToolkit.Factories;

namespace OsmToolkit.ModelTests.FactoryTests
{
    [TestClass()]
    public class OsmEntityFactoryTests
    {
        private User GetTestUser() => new User(1, "username");

        [TestMethod()]
        public void CreateNode_ValidArgumentsWithTags_ReturnsNodeInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };

            var actual = factory.CreateNode(1, true, 1, 1, DateTime.UtcNow, user, 0, 0, tags);

            Assert.IsTrue(actual is Node);
        }

        [TestMethod()]
        public void CreateNode_ValidArgumentsWithoutTags_ReturnsNodeInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();

            var actual = factory.CreateNode(1, true, 1, 1, DateTime.UtcNow, user, 0, 0);

            Assert.IsTrue(actual is Node);
        }

        [TestMethod()]
        public void CreateWay_ValidArgumentsWithTags_ReturnsWayInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "highway", "tertiary" } };
            List<long> nodeIdReferences = new List<long>();
            nodeIdReferences.Add(2);
            nodeIdReferences.Add(3);

            var actual = factory.CreateWay(1, true, 1, 1, DateTime.UtcNow, user, nodeIdReferences, tags);

            Assert.IsTrue(actual is Way);
        }

        [TestMethod()]
        public void CreateWay_ValidArgumentsWithoutTags_ReturnsWayInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();
            List<long> nodeIdReferences = new List<long>();
            nodeIdReferences.Add(2);
            nodeIdReferences.Add(3);

            var actual = factory.CreateWay(1, true, 1, 1, DateTime.UtcNow, user, nodeIdReferences);

            Assert.IsTrue(actual is Way);
        }

        [TestMethod()]
        public void CreateRelation_ValidArgumentsWithTags_ReturnsRelationInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            List<Member> members = new List<Member>();
            members.Add(new Member(ReferenceType.node, 2, "from"));

            var actual = factory.CreateRelation(1, true, 1, 1, DateTime.UtcNow, user, members, tags);

            Assert.IsTrue(actual is Relation);
        }

        [TestMethod()]
        public void CreateRelation_ValidArgumentsWithoutTags_ReturnsRelationInstance()
        {
            OsmEntityFactory factory = new OsmEntityFactory();
            User user = GetTestUser();
            List<Member> members = new List<Member>();
            members.Add(new Member(ReferenceType.node, 2, "from"));

            var actual = factory.CreateRelation(1, true, 1, 1, DateTime.UtcNow, user, members);

            Assert.IsTrue(actual is Relation);
        }
    }
}