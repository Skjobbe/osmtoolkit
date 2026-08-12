namespace OsmToolkit.Tests.Models
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
        private readonly DateTime _testTime  = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void Constructor_WithTagsAndValidArguments_ShouldCreateOsmEntity()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };

            // Act
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Assert
            Assert.AreEqual(1, osmEntity.Id);
            Assert.AreEqual(true, osmEntity.Visible);
            Assert.AreEqual(1, osmEntity.Version);
            Assert.AreEqual(1, osmEntity.ChangeSet);
            Assert.AreEqual(_testTime, osmEntity.Timestamp);
            Assert.AreEqual(user, osmEntity.User);
            Assert.AreEqual("restaurant", osmEntity.Tags["amenity"]);
        }

        [TestMethod()]
        public void Constructor_WithTagsAsNull_ShouldCreateOsmEntityWithEmptyTags()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, null);

            // Assert
            Assert.AreEqual(1, osmEntity.Id);
            Assert.IsNotNull(osmEntity.Tags);
        }

        [TestMethod()]
        public void Constructor_WithoutTags_ShouldCreateOsmEntityWithEmptyTags()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user);

            // Assert
            Assert.AreEqual(1, osmEntity.Id);
            Assert.IsNotNull(osmEntity.Tags);
        }

        [TestMethod()]
        public void Constructor_WithIdEqualToZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new TestOsmEntity(0, true, 1, 1, _testTime, user);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WithIdBelowZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            User user = GetTestUser();

            // Act
            Action actual = () => new TestOsmEntity(-1, true, 1, 1, _testTime, user);

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void AddTag_ValidKeyAndValue_ShouldAddTag()
        {
            // Arrange
            User user = GetTestUser();
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user);

            // Act
            osmEntity.AddTag("amenity", "restaurant");

            // Assert
            Assert.AreEqual("restaurant", osmEntity.Tags["amenity"]);
        }

        [TestMethod()]
        public void AddTag_WithExistingKey_ShouldUpdateTagValue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            osmEntity.AddTag("amenity", "bar");

            // Assert
            Assert.AreEqual("bar", osmEntity.Tags["amenity"]);
        }

        [TestMethod()]
        public void AddTag_WithValidKeyAndNullValue_ShouldStoreEmptyString()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            osmEntity.AddTag("amenity2", null!);

            // Assert
            Assert.AreEqual(osmEntity.Tags["amenity2"], string.Empty);
        }

        [TestMethod()]
        public void AddTag_WithValidKeyAndEmptyValue_ShouldStoreEmptyString()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            osmEntity.AddTag("amenity2", "");

            // Assert
            Assert.AreEqual(osmEntity.Tags["amenity2"], string.Empty);
        }

        [TestMethod()]
        public void AddTag_WithKeyIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            User user = GetTestUser();
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user);

            // Act
            Action actual = () => osmEntity.AddTag(null!, "restaurant");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void AddTag_WhenKeyIsEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            User user = GetTestUser();
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user);

            // Act
            Action actual = () => osmEntity.AddTag("", "restaurant");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void RemoveTag_WhenKeyExists_ShouldRemoveTagAndReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            var tags = new Dictionary<string, string> { { "amenity", "restaurant" } };
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user, tags);

            // Act
            bool actual = osmEntity.RemoveTag("amenity");

            // Assert
            Assert.IsTrue(actual);
            CollectionAssert.DoesNotContain(tags, "amenity");
        }

        [TestMethod()]
        public void RemoveTag_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            TestOsmEntity osmEntity = new TestOsmEntity(1, true, 1, 1, _testTime, user);

            // Act
            bool actual = osmEntity.RemoveTag("amenity");

            // Assert
            Assert.IsFalse(actual);
        }
    }
}