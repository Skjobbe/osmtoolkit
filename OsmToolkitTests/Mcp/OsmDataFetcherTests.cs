using OsmToolkit.DataSources;
using OsmToolkit.Mcp.Tools;

namespace OsmToolkit.Tests.Mcp
{
    /// <summary>
    /// Fakes <see cref="IOsmDataSource"/> directly, throwing whatever exception a test wires up, so
    /// <see cref="OsmDataFetcher"/>'s wrapping behavior can be verified without going through a real
    /// (or fake-HTTP-backed) <see cref="OverpassOsmDataSource"/>.
    /// </summary>
    internal sealed class ThrowingOsmDataSource : IOsmDataSource
    {
        private readonly Exception _exception;

        public ThrowingOsmDataSource(Exception exception) => _exception = exception;

        public Task<OsmData> GetOsmDataAsync(OsmCoordinateBounds bounds, CancellationToken cancellationToken = default)
            => throw _exception;
    }

    [TestClass]
    public class OsmDataFetcherTests
    {
        private static readonly OsmCoordinateBounds Bounds = new(10.0, 20.0, 10.5, 20.5);

        [TestMethod]
        public async Task FetchAsync_WhenDataSourceSucceeds_ReturnsData()
        {
            // Arrange
            var expected = new OsmData(new OsmHeader(0.6));
            var dataSource = new SucceedingOsmDataSource(expected);

            // Act
            var result = await OsmDataFetcher.FetchAsync(dataSource, Bounds, CancellationToken.None);

            // Assert
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public async Task FetchAsync_WhenHttpRequestExceptionThrown_ThrowsOsmDataUnavailableExceptionWrappingIt()
        {
            // Arrange
            var httpException = new HttpRequestException("Response status code does not indicate success: 504.");
            var dataSource = new ThrowingOsmDataSource(httpException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<OsmDataUnavailableException>(
                () => OsmDataFetcher.FetchAsync(dataSource, Bounds, CancellationToken.None));
            Assert.AreSame(httpException, exception.InnerException);
        }

        [TestMethod]
        public async Task FetchAsync_WhenOverpassQueryFailedExceptionThrown_ThrowsOsmDataUnavailableExceptionWrappingIt()
        {
            // Arrange
            var overpassException = new OverpassQueryFailedException("runtime error: Query timed out.");
            var dataSource = new ThrowingOsmDataSource(overpassException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<OsmDataUnavailableException>(
                () => OsmDataFetcher.FetchAsync(dataSource, Bounds, CancellationToken.None));
            Assert.AreSame(overpassException, exception.InnerException);
        }

        [TestMethod]
        public async Task FetchAsync_WhenInvalidOperationExceptionThrown_ThrowsOsmDataUnavailableExceptionWrappingIt()
        {
            // Arrange
            var parseException = new InvalidOperationException("Overpass returned a response that could not be parsed as JSON.");
            var dataSource = new ThrowingOsmDataSource(parseException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<OsmDataUnavailableException>(
                () => OsmDataFetcher.FetchAsync(dataSource, Bounds, CancellationToken.None));
            Assert.AreSame(parseException, exception.InnerException);
        }

        [TestMethod]
        public async Task FetchAsync_WhenUnrelatedExceptionThrown_PropagatesUnwrapped()
        {
            // Arrange
            var dataSource = new ThrowingOsmDataSource(new ArgumentOutOfRangeException("bounds"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => OsmDataFetcher.FetchAsync(dataSource, Bounds, CancellationToken.None));
        }

        private sealed class SucceedingOsmDataSource : IOsmDataSource
        {
            private readonly OsmData _data;

            public SucceedingOsmDataSource(OsmData data) => _data = data;

            public Task<OsmData> GetOsmDataAsync(OsmCoordinateBounds bounds, CancellationToken cancellationToken = default)
                => Task.FromResult(_data);
        }
    }
}
