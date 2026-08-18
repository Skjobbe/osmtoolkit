# 005 — Single JSON parse for the Overpass remark check and deserialization

## Context
On every successful fetch, the Overpass response body was parsed as JSON up to three times: once by `OverpassOsmDataSource`'s remark check (`JsonDocument.Parse`), once as a shape-probing `JsonDocument.ParseAsync` inside `OsmJsonDeserializer.DeserializeAsync`, and once more via `JsonSerializer.DeserializeAsync` in the same method. For a large Bounds near the configured server-side memory ceiling, this multiplied CPU and transient allocation cost for no functional benefit. Found during review of the PR for issue #12; tracked as issue #14.

The public `IOsmJsonDeserializer` interface only exposes string/stream-based deserialize methods. Adding a `JsonDocument`-based method to it was not an option: `IOsmJsonDeserializer` is part of the published package's public surface, and this project's rule is that new methods go into new, small interfaces rather than existing published ones (see `CLAUDE.md`, "no breaking changes").

## Decision
The concrete `OsmJsonDeserializer` class gained a new `internal Deserialize(JsonDocument document)` method. Its own stream-based path (`DeserializeAsync`, `DeserializeFromStreamAsync`, `DeserializeFromFileAsync`) now parses the input into one `JsonDocument` and calls this same method, collapsing its own former double parse (probe, then a second parse to deserialize) into one.

`OverpassOsmDataSource` parses the response body once. When its injected deserializer is the concrete `OsmJsonDeserializer` (checked via `_deserializer is OsmJsonDeserializer`), it hands that parsed `JsonDocument` straight to `Deserialize(JsonDocument)`, reusing it for both the remark check and the conversion to `OsmData` — one parse total for the whole successful-fetch path. Only a caller-supplied, non-default `IOsmJsonDeserializer` implementation — which only has the public interface's string-based `DeserializeAsync` — falls back to the previous behavior of the data source parsing the body separately for the remark check and letting the deserializer parse it again itself.

## Alternatives considered
- **Add a `JsonDocument`-based method to the public `IOsmJsonDeserializer` interface**: rejected — breaking-change risk on a published interface, and against the project's "new methods go into new, small interfaces" rule.
- **A new small internal-only interface for the `JsonDocument`-based path, injected alongside `IOsmJsonDeserializer`**: rejected as unnecessary ceremony — the concrete-type check is confined entirely to `OverpassOsmDataSource`'s private implementation and never leaks into any public surface; in the only case that matters (no custom deserializer supplied), it's always the same concrete class, so a second injectable dependency for it added no real flexibility.
- **Fork or duplicate the Overpass-shape detection and DTO-conversion logic directly inside `OverpassOsmDataSource`**: rejected — issue #14 explicitly ruled this out, to avoid maintaining Overpass-shape detection logic in two places.

## Consequences
A caller who supplies a custom `IOsmJsonDeserializer` (anything other than the built-in `OsmJsonDeserializer`) does not get the single-parse optimization — their response body is still parsed twice: once by the data source's remark check, once inside their own deserializer. Accepted as a narrow trade-off, since custom deserializer injection here is a test/extensibility seam, not a documented, exercised consumer path.
