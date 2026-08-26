# 011 — Overpass/HTTP fetch failures surface as MCP tool errors

## Context
Manual verification of #27 (`route_between_points`) hit an out-of-coverage query and returned a clean "no path found" result via `RouteResult.Description` - no exception. The same session then pointed `find_near_point` and `search_by_tags_in_area` at real places and both threw unhandled exceptions straight through the MCP tool boundary.

Investigation (tracked in #30) traced this to the Overpass layer, not the finder layer - `GridNodeIndex` and `OsmEntityFinder` already handle a genuinely empty `OsmData` gracefully. What none of the three MCP tools handled was Overpass **failing to respond successfully at all**: a non-success HTTP status (`HttpRequestException`), an unparseable response body, or a server-side query failure signaled via Overpass's `remark` field (both surfacing as `InvalidOperationException`, later split into `OverpassQueryFailedException` for the `remark` case by #29/ADR-010). `route_between_points`'s graceful result comes from `FindShortestPath`'s own empty-result contract, not from anything the other two tools lack - so it was exposed to the same gap, just not yet observed.

## Decision
Overpass/HTTP fetch failures surface as an MCP tool error (`IsError: true`), not a graceful in-band result - symmetric with how `PlaceNotFoundException` already works today. No tool's return shape changes: `FindNearPointHandler`/`SearchByTagsInAreaHandler` keep returning plain lists; `RouteResult.Description` keeps meaning only "no path in the fetched data," unrelated to this failure mode.

Shape:
- `OsmDataUnavailableException`, owned by `OsmToolkit.Mcp` (not the core library, so the core library's own exceptions stay meaningful for non-MCP consumers), wraps `HttpRequestException`, `OverpassQueryFailedException`, and `InvalidOperationException` (the unparseable-body case) with a clean, calling-model-readable message instead of the raw exception text.
- Wrapping happens in one shared helper, `OsmDataFetcher.FetchAsync`, called from all three handlers (`FindNearPointHandler`, `SearchByTagsInAreaHandler`, `RouteBetweenPointsHandler`) in place of calling `IOsmDataSource.GetOsmDataAsync` directly - one place to change the message rather than three call sites that could drift.
- The MCP SDK's existing unhandled-exception-to-`IsError`-result behavior (already relied on for `PlaceNotFoundException`) does the rest; no new transport-level handling was needed.

## Alternatives considered
- **A graceful "couldn't answer" result carrying a `Description`, matching `RouteResult`'s shape**: rejected because it would require reshaping two tools' return types for a case that's already well-represented as a failed call via the MCP protocol's own error mechanism.

## Consequences
A client calling any of the three tools now gets a clean, readable error message on an Overpass fetch failure instead of a raw `HttpRequestException`/`InvalidOperationException` dump - after #29's retry (ADR-010) is exhausted for the transient cases, or immediately for the unparseable-body case, which isn't retried.

Any future MCP tool that fetches OSM data via `IOsmDataSource` should route through `OsmDataFetcher.FetchAsync` rather than calling `GetOsmDataAsync` directly, to stay consistent with this contract.
