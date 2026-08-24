using OsmToolkit.Geocoding;

namespace OsmToolkit.Tests.Geocoding
{
    [TestClass]
    public class RateGateTests
    {
        [TestMethod]
        public async Task WaitForTurnAsync_FirstCall_DoesNotWait()
        {
            // Arrange
            var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var sut = new RateGate(TimeSpan.FromSeconds(1))
            {
                UtcNow = () => now,
                Delay = (_, _) => throw new InvalidOperationException("Should not delay on the first call.")
            };

            // Act & Assert
            await sut.WaitForTurnAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task WaitForTurnAsync_SecondCallWithinInterval_WaitsRemainingTime()
        {
            // Arrange
            var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan? requestedDelay = null;
            var sut = new RateGate(TimeSpan.FromSeconds(1))
            {
                UtcNow = () => now,
                Delay = (duration, _) => { requestedDelay = duration; return Task.CompletedTask; }
            };

            // Act
            await sut.WaitForTurnAsync(CancellationToken.None);
            now = now.AddMilliseconds(300);
            await sut.WaitForTurnAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(TimeSpan.FromMilliseconds(700), requestedDelay);
        }

        [TestMethod]
        public async Task WaitForTurnAsync_SecondCallAfterIntervalElapsed_DoesNotWait()
        {
            // Arrange
            var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var sut = new RateGate(TimeSpan.FromSeconds(1))
            {
                UtcNow = () => now,
                Delay = (_, _) => throw new InvalidOperationException("Should not delay once the interval has elapsed.")
            };

            // Act & Assert
            await sut.WaitForTurnAsync(CancellationToken.None);
            now = now.AddSeconds(2);
            await sut.WaitForTurnAsync(CancellationToken.None);
        }
    }
}
