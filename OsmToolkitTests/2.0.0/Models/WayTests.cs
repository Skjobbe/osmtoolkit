using OsmToolkit;

namespace OsmToolkitTests._2._0._0.Models
{
    [TestClass()]
    public class WayTests
    {
        private User GetTestUser() => new User(1, "username");
        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 15, 30);

        [TestMethod()]
        public void IsOneWay_WhenOneWayIsYes_ShouldReturnTrue()
        {
            // Arrange
            User user = GetTestUser();
            Way way = new Way(1, true, 1, 1, _testTime, user, new List<long>() { 1, 2 }, 
                new Dictionary<string, string>() { { "oneway", "yes" } });

            // Act
            var actual = way.IsOneWay();

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsOneWay_WhenOneWayIsNo_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            Way way = new Way(1, true, 1, 1, _testTime, user, new List<long>() { 1, 2 },
                new Dictionary<string, string>() { { "oneway", "no" } });

            // Act
            var actual = way.IsOneWay();

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsOneWay_WhenMissingOneWayTag_ShouldReturnFalse()
        {
            // Arrange
            User user = GetTestUser();
            Way way = new Way(1, true, 1, 1, _testTime, user, new List<long>() { 1, 2 });

            // Act
            var actual = way.IsOneWay();

            // Assert
            Assert.IsFalse(actual);
        }
    }
}
