using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Serialization.Json;
using OsmToolkit.Serialization.Xml;

namespace OsmToolkit
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOsmToolkit(this IServiceCollection services)
        {
            services.TryAddTransient<IOsmXmlSerializer, OsmXmlSerializer>();
            services.TryAddTransient<IOsmXmlDeserializer, OsmXmlDeserializer>();
            services.TryAddTransient<IOsmJsonDeserializer, OsmJsonDeserializer>();
            services.TryAddTransient<IOsmJsonSerializer, OsmJsonSerializer>();
#pragma warning disable CS0618 // IOsmFinder is obsolete, but still publically needed.
            services.TryAddTransient<IOsmFinder<OsmEntity>, OsmEntityFinder>();
#pragma warning restore CS0618
            services.TryAddTransient<IOsmFinderV2<OsmEntity>, OsmEntityFinder>();
            services.TryAddTransient<IOsmValueFinder<OsmEntity>, OsmEntityFinder>();
            services.TryAddTransient<INearestNodesFinder, OsmEntityFinder>();
            services.TryAddTransient<IWithinDistanceFinder<OsmEntity>, OsmEntityFinder>();
            services.TryAddTransient<IShortestPathFinder, OsmEntityFinder>();
            services.AddMemoryCache(options => options.SizeLimit = OverpassOsmDataSource.DefaultCacheSizeLimit);
            services.TryAddTransient<IOsmDataSource, OverpassOsmDataSource>();
            services.TryAddTransient<ITagFilteredOsmDataSource, OverpassOsmDataSource>();
            services.TryAddTransient<IPlaceLookup, NominatimPlaceLookup>();

            return services;
        }
    }
}
