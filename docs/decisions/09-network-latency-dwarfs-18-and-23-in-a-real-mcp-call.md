# 009 — Network latency dwarfs both #18 and #23 in a real find_near_point call

## Context
Two issues were deliberately deferred rather than fixed when they were found. ADR-08 measured `GatherNodeRelatedEntities`'s cost in isolation against a real Fredrikstad extract - 10.9 ms/query for the full `FindNearByRadius` call, against 1.1 ms/query for the indexed node search alone - and scoped the gap as #23, "needs its own design pass." #18 was found while designing that same grid index: `OsmData` has no stable identity across independent fetches, so the per-instance `ConditionalWeakTable` cache behind `FindNearByRadius` can't help across two separate top-level fetches of the same area, "the most common real-world usage pattern (e.g. an MCP server handling repeated requests for the same area)" - #18's own words, never checked against an actual MCP call.

Both numbers came from the finder layer alone. Neither said anything about where that layer's cost sits inside a real `find_near_point` call, which also pays for a Nominatim geocoding lookup and an Overpass fetch before the finder ever runs - costs on a different order of magnitude that ADR-08's benchmark never touched.

## Decision
Added `FindNearPointBenchmarkManualTests` (`OsmToolkitTests/Mcp/FindNearPointBenchmarkManualTests.cs`, `TestCategory=ManualIntegration`), extending `GridIndexBenchmarkManualTests`'s pattern from the finder layer to the whole MCP-driven path. It drives the same sequence `FindNearPointHandler.FindAsync` does - geocode via `NominatimPlaceLookup`, fetch via `OverpassOsmDataSource`, then grid-index build, indexed node search, and Way/Relation gathering via `OsmEntityFinder` - with each step timed separately, and checks the result against a call through the real handler.

The figures below were captured against a real Nominatim lookup and a real Overpass fetch for "Gamlebyen, Fredrikstad" at a 700 m radius (the same bounds `FindNearPointHandler`'s own `BoundsFromRadius` computes), with the finder-layer steps measured against that same fetched dataset using the same isolation technique ADR-08 used - separate `OsmData` instances per timed comparison, since the grid index is cached per-instance. The public Overpass instance began rate-limiting repeated automated requests partway through this measurement session, so the network legs and the finder-layer legs were captured as two real runs against the same dataset rather than one uninterrupted test execution; `FindNearPointBenchmarkManualTests` reproduces the identical sequence end to end in one pass and is what to rerun for fresh numbers.

| | value |
|---|---|
| Geocoding lookup (Nominatim) | 174 ms |
| Overpass fetch (35,485 nodes, 2,870 ways, 24 relations) | 1,860 ms |
| Grid-index build (1st query, one-time cost - see #18) | 27.75 ms |
| Indexed node search alone | 0.161 ms/query |
| Way/Relation gathering (derived: full − node-only - see #23) | 5.289 ms/query |
| Full `FindNearByRadius` (steady state) | 5.451 ms/query |
| Finder execution (one full `find_near_point` call: `FindNearByRadius` + `FindNearbyNodes`) | 14.52 ms |

Geocoding and the Overpass fetch together take about 2.0 seconds. Against that, the entire finder-execution step - grid build, node search, gathering, *and* nearest-node selection combined - is 14.52 ms, under 1% of the call's total time. Isolating #18's specific cost, the one-time grid-index build, comes to about 1.4% of network time; #23's Way/Relation-gathering cost, measured the same way ADR-08 measured it, is smaller still at roughly 0.3%. Both are real, non-zero, measured costs - ADR-08 already showed #23 dominates the finder layer in isolation, and #18 is a real gap in `OsmData`'s identity - but neither is where a `find_near_point` call's latency actually goes. Geocoding and the Overpass fetch are.

Neither #18 nor #23 is fixed by this ticket, per its own acceptance criteria. Since neither measures as a real, worthwhile cost in this context, no follow-up issue is filed for either - the existing issues stay open, unscoped, and explicitly deprioritized rather than acted on.

## Alternatives considered
- **Fix #18 now (e.g. content-addressed caching so identical fetches reuse one grid index across independent MCP requests)**: rejected - there's no measured evidence it matters at the scale a single request pays for. It would trade a real design/breaking-change risk (see #18's own discussion of `OsmData` mutability) for a cost that reads as noise today.
- **Fix #23 now**: rejected for the same reason. ADR-08 showed it dominates the finder layer in isolation, but this measurement shows the finder layer itself is a rounding error next to `find_near_point`'s network legs. Worth revisiting only if that premise changes - Nominatim/Overpass latency drops, or a future tool runs many finder queries against one already-fetched `OsmData` instead of one query per call.
- **Skip measuring and reason from ADR-08's numbers alone**: rejected - the same argument ADR-08 made against trusting #20-#22's design without measuring it applies here. A finder-layer number alone says nothing about where that layer sits inside the call a real MCP client actually waits on; only measuring the whole path answered that.

## Consequences
`find_near_point`'s real-world latency is dominated by geocoding and the Overpass fetch - about 98% of a call in this measurement - not by anything in OsmToolkit's finder layer. #18 and #23 stay open and deprioritized until something changes that premise: #3's other planned tools revealing a different query shape (e.g. one that reuses a fetched `OsmData` across many finder calls, where #18's per-fetch rebuild and #23's per-call gathering scan would both be paid repeatedly instead of once), or the network legs above getting fast enough that the finder layer becomes visible again. `FindNearPointBenchmarkManualTests` is the instrument to rerun if either happens.
