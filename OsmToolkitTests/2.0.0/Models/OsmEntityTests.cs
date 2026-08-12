using OsmToolkit;

namespace OsmToolkitTests._2._0._0.Models
{
    public class TestOsmEntity : OsmEntity
    {
        public TestOsmEntity(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, Dictionary<string, string>? tags = null)
            : base(id, visible, version, changeSet, timestamp, user, tags) { }
    }

    [TestClass()]
    public class OsmEntityTests
    {
        private User GetTestUser() => new User(1, "username");
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void HasTagKey_WhenMatchingKey_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTagKey("amenity");

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasTagKey_WhenNotMatchingKey_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTagKey("bus_stop");

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasAnyTagKey_WhenMatchingAnyKey_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAnyTagKey(new List<string>() { "amenity", "place" });

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasAnyTagKey_WhenNotMatchingAnyKey_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAnyTagKey(new List<string>() { "bus_stop", "place" });

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasAllTagKeys_WhenMatchingAllKeys_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAllTagKeys(new List<string>() { "amenity", "place" });

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasAllTagKeys_WhenNotMatchingAllKeys_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAllTagKeys(new List<string>() { "amenity", "name" });

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasTagValue_WhenMatchingValue_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTagValue("restaurant");

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasTagValue_WhenNotMatchingValue_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTagValue("country");

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasAnyTagValue_WhenMatchingAnyValue_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAnyTagValue(new List<string>() { "restaurant", "country" });

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasAnyTagValue_WhenNotMatchingAnyValue_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAnyTagValue(new List<string>() { "Norway", "country" });

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasAllTagValues_WhenMatchingAllValues_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAllTagValues(new List<string>() { "restaurant", "country" });

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasAllTagValues_WhenNotMatchingAllValues_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasAllTagValues(new List<string>() { "Norway", "state" });

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasTag_WhenMatchingKeyAndValue_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTag( "amenity", "restaurant" );

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void HasTag_WhenNotMatchingKeyAndValue_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual = osmEntity.HasTag("amenity", "restaurant");

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void HasAnyTag_WhenMatchingAnyTag_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual1 = osmEntity.HasAnyTag(("amenity", "restaurant"));
            var actual2 = osmEntity.HasAnyTag(new Dictionary<string, string>() 
            { 
                { "amenity", "restaurant" } 
            });
            var actual3 = osmEntity.HasAnyTag(new List<KeyValuePair<string, string>>() 
            { 
                new KeyValuePair<string, string>("amenity", "restaurant") 
            });

            // Assert
            Assert.IsTrue(actual1);
            Assert.IsTrue(actual2);
            Assert.IsTrue(actual3);
        }

        [TestMethod()]
        public void HasAnyTag_WhenNotMatchingAnyTag_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual1 = osmEntity.HasAnyTag(("name", "Norway"));
            var actual2 = osmEntity.HasAnyTag(new Dictionary<string, string>() 
            { 
                { "name", "Norway" } 
            });
            var actual3 = osmEntity.HasAnyTag(new List<KeyValuePair<string, string>>() 
            { 
                new KeyValuePair<string, string>("name", "Norway") 
            });

            // Assert
            Assert.IsFalse(actual1);
            Assert.IsFalse(actual2);
            Assert.IsFalse(actual3);
        }

        [TestMethod()]
        public void HasAllTags_WhenMatchingAllTags_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" }, { "place", "country" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual1 = osmEntity.HasAllTags(("amenity", "restaurant"), ("place", "country"));
            var actual2 = osmEntity.HasAllTags(new Dictionary<string, string>() 
            { 
                { "amenity", "restaurant" }, 
                { "place", "country" } 
            });
            var actual3 = osmEntity.HasAllTags(new List<KeyValuePair<string, string>>() 
            { 
                new KeyValuePair<string, string>("amenity", "restaurant"),
                new KeyValuePair<string, string>("place", "country")
            });

            // Assert
            Assert.IsTrue(actual1);
            Assert.IsTrue(actual2);
            Assert.IsTrue(actual3);
        }

        [TestMethod()]
        public void HasAllTags_WhenNotMatchingAllTags_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "cafe" }, { "place", "state" }, { "bus_stop", "no" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            var actual1 = osmEntity.HasAllTags(("ford", "no"), ("place", "country"));
            var actual2 = osmEntity.HasAllTags(new Dictionary<string, string>()
            {
                { "ford", "no" },
                { "place", "country" }
            });
            var actual3 = osmEntity.HasAllTags(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("ford", "no"),
                new KeyValuePair<string, string>("place", "country")
            });

            // Assert
            Assert.IsFalse(actual1);
            Assert.IsFalse(actual2);
            Assert.IsFalse(actual3);
        }
    }
}
