namespace OsmToolkit.Tests.Models
{
    [TestClass()]
    public class MemberTests
    {
        [TestMethod()]
        public void Constructor_WithValidArguments_ShouldCreateMember()
        {
            // Arrange & Act

            Member member = new Member(ReferenceType.node, 1, "from");

            // Assert

            Assert.AreEqual(ReferenceType.node, member.Type);
            Assert.AreEqual(1, member.ReferenceId);
            Assert.AreEqual("from", member.Role);
        }
     

        [TestMethod()]
        public void Constructor_WithValidArgumentsAndRoleAsNull_ShouldCreateMember()
        {
            // Arrange & Act
            Member member = new Member(ReferenceType.node, 1, null);

            // Assert
            Assert.AreEqual(ReferenceType.node, member.Type);
            Assert.AreEqual(1, member.ReferenceId);
            Assert.AreEqual(string.Empty, member.Role);
        }

        [TestMethod()]
        public void Constructor_WithRoleAsNull_ShouldSetRoleToEmptyString()
        {
            // Arrange & Act
            Member member = new Member(ReferenceType.node, 1, null);

            // Assert
            Assert.AreEqual(string.Empty, member.Role);
        }

        [TestMethod()]
        public void Constrctor_WithValidArgumentsWithoutRole_ShouldCreateMember()
        {
            // Arrange & Act
            Member member = new Member(ReferenceType.node, 1);

            // Assert
            Assert.AreEqual(ReferenceType.node, member.Type);
            Assert.AreEqual(1, member.ReferenceId);
            Assert.AreEqual(string.Empty, member.Role);
        }

        [TestMethod()]
        public void Constructor_WhenReferenceIdIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new Member(ReferenceType.node, 0, "from");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }


        [TestMethod()]
        public void Constructor_WhenReferenceIdIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            Action actual = () => new Member(ReferenceType.node, -1, "from");

            // Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(actual);
        }
    }
}