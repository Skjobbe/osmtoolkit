# 008 — Measuring the grid index found a different bottleneck than assumed

## Context
Issue #2 set a concrete reference point when it scoped the spatial-indexing work: a linear-scan `FindNearByRadius` over all of Norway reportedly took 144 minutes and 32 GB. Issues #19-#22 designed and shipped a grid-based spatial index against that problem (see ADR-07), and #20-#22 landed with full test coverage. But no one had actually measured the result against real data - the work was accepted as done on the strength of the design being sound, not on a number showing it worked.

## Decision
Added `GridIndexBenchmarkManualTests` (`OsmToolkitTests/Finders/GridIndexBenchmarkManualTests.cs`, `TestCategory=ManualIntegration`), which fetches a real Fredrikstad extract from the live Overpass API (220,667 nodes, 24,569 ways, 135 relations) and times the same `FindNearByRadius` search three ways: a linear scan re-implementing the pre-#20 algorithm, `GridNodeIndex.FindWithinRadius` in isolation, and the full production `FindNearByRadius` call.

The measurement changed the picture:

| | ms/query | vs. linear scan |
|---|---|---|
| Linear node scan (no index) | 15.8 | — |
| Indexed node search alone | 1.1 | **14.2x** |
| Full `FindNearByRadius` (indexed node search + Way/Relation gathering) | 10.9 | **1.4x** |

The node index itself works exactly as designed - a 14x speedup on the operation it targets. But `FindNearByRadius` as a whole is only 1.4x faster, because `GatherNodeRelatedEntities` does its own unindexed linear scan over every `Way` and `Relation` in the dataset on every call, to find the ones referencing a node the (now-fast) node search returned. That step was explicitly out of scope for #19-#22 ("Indexing Ways or Relations spatially" was excluded by design - see ADR-07's alternatives), on the assumption it was a minor detail next to the node scan. For a dataset with this many ways relative to nodes, it's now the dominant cost instead.

#2 is closed on the strength of this measurement: the node-search problem it named is solved, with a number to show it (14x). The Way/Relation-gathering cost the measurement surfaced is scoped as its own issue rather than silently folded into #2's closure, using the same 15.8 → 1.1 ms/query numbers as its baseline reference point - the same role the original 144-minute figure played for #2.

## Alternatives considered
- **Keep #2 open until Way/Relation gathering is also indexed**: rejected - #2's and #19's scope was specifically node search (see ADR-07); the Way/Relation cost is a distinct problem the measurement exposed, not a leftover part of the original one. Keeping #2 open to cover it would blur "done" for work that was, by its own stated scope, actually finished.
- **Skip measuring and trust the design**: this is literally the alternative this ADR argues against. A 14x node-search win and a 1.4x end-to-end win are both true at once; only measuring against real data - not just the unit-test fixtures used during #20-#22 - surfaced that they're different numbers, and that the gap matters at Fredrikstad's data density.

## Consequences
`FindNearByRadius`'s real-world cost on way-dense datasets is now dominated by `GatherNodeRelatedEntities`, not node lookup. Any future work on that new issue can use `GridIndexBenchmarkManualTests` as a regression instrument - rerun it before and after to confirm an actual improvement, the same discipline this ADR is applying retroactively to #20-#22. Consumers building on `IOsmDataSource` + `FindNearByRadius` today (including the MCP server planned in #3) should expect way/relation-heavy areas to see closer to 1.4x than 14x until that follow-up work lands.
