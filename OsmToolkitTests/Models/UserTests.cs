namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class UserTests
    {
        [TestMethod()]
        public void Constructor_WithValidArguments_ShouldCreateUser()
        {
            // Arrange & Act
            User user = new User(1, "username");

            // Assert
            Assert.AreEqual("username", user.Name);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod()]
        public void Constructor_WhenIdIsEqualToZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new User(0, "username");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenIdIsBelowZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new User(-1, "username");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenNameIsNull_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action actual = () => new User(1, null!);

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }

        [TestMethod()]
        public void Constructor_WhenNameIsEmptyString_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action actual = () => new User(1, "");

            // Assert
            Assert.ThrowsException<ArgumentException>(actual);
        }
    }
}