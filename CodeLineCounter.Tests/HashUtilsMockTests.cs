using CodeLineCounter.Utils;
using Moq;

namespace CodeLineCounter.Tests
{
    public class HashUtilsMockTests : TestBase
    {
        private readonly Mock<IHashUtils> _mockHashUtils;
        private readonly IHashUtils _originalImplementation;

        public HashUtilsMockTests()
        {
            _originalImplementation = HashUtils.Implementation;
            _mockHashUtils = new Mock<IHashUtils>();
            HashUtils.Implementation = _mockHashUtils.Object;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mockHashUtils.VerifyNoOtherCalls();
                HashUtils.Implementation = _originalImplementation;
            }
            
            base.Dispose(disposing);

        }

        [Fact]
        public void ComputeHash_WithMockedImplementation_CallsMockMethod()
        {
            // Arrange
            string input = "Test";
            string expected = "mocked-hash";
            _mockHashUtils.Setup(m => m.ComputeHash(input)).Returns(expected);

            // Act
            string result = HashUtils.ComputeHash(input);

            // Assert
            Assert.Equal(expected, result);
            _mockHashUtils.Verify(m => m.ComputeHash(input), Times.Once);
        }
    }
}