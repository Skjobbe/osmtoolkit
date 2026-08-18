# OsmToolkit

.NET library for reading, writing, searching, and analysing OpenStreetMap data.

## Language

**Bounds**:
A rectangular lat/lon area (`OsmCoordinateBounds`) that delimits a geographic slice of OSM data.
_Avoid_: Bbox, bounding box

**Overpass**:
An external OSM query service (Overpass API) that an `IOsmDataSource` fetches data from over HTTP, as an alternative to reading from file.
_Avoid_: Overpass API (use "Overpass" alone once the context is clear)

**Data source**:
A component (`IOsmDataSource`) that fetches OSM data from an external source and returns it already parsed as `OsmData` — as opposed to a `Deserializer`, which only parses data that's already been retrieved.
_Avoid_: Overpass client (covers more than just HTTP transport: also query building, caching, and guardrails)
