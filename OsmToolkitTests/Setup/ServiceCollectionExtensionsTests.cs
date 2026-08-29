using Microsoft.Extensions.DependencyInjection;
using OsmToolkit.DataSources;
using OsmToolkit.Geocoding;

namespace OsmToolkit.Tests.Setup
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddOsmToolkit_RegistersIPlaceLookup_ResolvesToNominatimPlaceLookup()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOsmToolkit();
            var provider = services.BuildServiceProvider();

            // Act
            var placeLookup = provider.GetRequiredService<IPlaceLookup>();

            // Assert
            Assert.IsNotNull(placeLookup);
            Assert.AreEqual("NominatimPlaceLookup", placeLookup.GetType().Name);
        }

        [TestMethod]
        public void AddOsmToolkit_RegistersITagFilteredOsmDataSource_ResolvesToOverpassOsmDataSource()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOsmToolkit();
            var provider = services.BuildServiceProvider();

            // Act
            var dataSource = provider.GetRequiredService<ITagFilteredOsmDataSource>();

            // Assert
            Assert.IsNotNull(dataSource);
            Assert.AreEqual("OverpassOsmDataSource", dataSource.GetType().Name);
        }
    }
}
