using Microsoft.Extensions.DependencyInjection;
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
    }
}
