using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json
{
    /// <summary>
    /// Represents a deserializer that parses OSM Json data into <see cref="OsmData"/> objects.
    /// </summary>
    public interface IOsmJsonDeserializer : IOsmDeserializer
    {
    }
}
