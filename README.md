# OsmToolkit

**OsmToolkit** is a .NET library for reading, writing, and filtering geographic data from [OpenStreetMap (OSM)](https://www.openstreetmap.org/). It provides an API for working with `.osm` XML and Json files, and supports operations on nodes, ways, and relations through the internal `OsmData` model.

## ⚠️Warning!⚠️
Deserializing and using large files will require lots of RAM. We did some testing deserializing the entirity of Norway and it successfully deserialized after 144 minutes, using all of our 32GB of RAM and a lot of the side exchange ram. It is possible, but very slow (estimating 200,000,000 nodes, 12,000,000 ways and 750,000 relations.), will attempt to optimize in the future if possible.
Using it for smaller files works just fine.

## Getting started

### 1. Install manually using provided NuGet

- Add the OsmToolkit.dll file as a referance in Visual Studio.

### 2. Register in your DI container

```csharp
var services = new ServiceCollection();
services.AddOsmToolkit();
```
## Quick example usage

### Deserialize a `.osm` file

```csharp
var provider = services.BuildServiceProvider();
var deserializer = provider.GetRequiredService<IOsmXmlDeserializer>();

var data = await deserializer.DeserializeFromFileAsync("map.osm");
```

### Filter entities by tag

```csharp
var finder = provider.GetRequiredService<IOsmFinder<OsmEntity>>();
var result = finder.FindByTag(data, "amenity");

foreach (var node in result.Nodes)
{
    Console.WriteLine($"{node.Id} @ {node.Latitude}, {node.Longitude}");
}
```

### Serialize the modified data back to file

```csharp
var serializer = provider.GetRequiredService<IOsmXmlSerializer>();
await serializer.SerializeToFileAsync(data, "output.osm");
```

---

## Library Structure

`OsmToolkit` — Core data structures: `Node`, `Way`, `Relation`, `OsmEntity`, etc.
- `.Serialization` — Interfaces and classes for Json and XML serialization/deserialization.
- `.Finders` — Filter logic for querying entities based on tags, coordinates and distance. (The filters on some of the methods like FindShortestPath are based on norwegian laws.).

---

## Example Use Cases

- Import and analyze OSM map data
- Extract all roads, buildings, or amenities in a region
- Preprocess map data for GIS applications
- Convert filtered OSM content to other formats (e.g., GeoJSON — planned)

---

## Logging 

OsmToolkit uses structured logging via `ILogger<T>`. If a logger is injected into the serializer or deserializer, internal operations and errors will be logged automatically. If no logger is provided, a `NullLogger` is used to ensure stable operation.

---

## Authors

This library was developed by Group 8 as part of the course "Rammeverk og .NET" at [HiØ](https://www.hiof.no):

- Ole Sander Skjørberg
- Mathias Hem
- Christian Øyvind Glåmseter
- Erling Kristoffer Næsset Arnesen

---

## Documentation

For detailed API documentation, usage examples and architecture diagrams, see the full PDF documentation.
