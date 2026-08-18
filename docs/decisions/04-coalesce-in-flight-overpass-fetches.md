# 004 — Coalesce concurrent Overpass fetches per cache instance

## Context
`OverpassOsmDataSource`'s cache lookup (`TryGetValue`) and cache population (`Set`) are two separate, unsynchronized steps (ADR 002). Concurrent `GetOsmDataAsync` calls for identical Bounds that both land during a cache miss previously both fell through to the network, doubling load against the shared Overpass endpoint. Found during review of the PR for issue #12; tracked as issue #13.

## Decision
In-flight fetches are tracked per Bounds key in a `ConcurrentDictionary<CacheKey, Lazy<Task<OsmData>>>`. This tracker is scoped to the specific `IMemoryCache` instance a given `OverpassOsmDataSource` is using, via a static `ConditionalWeakTable<IMemoryCache, ...>`, rather than a single process-wide dictionary or an instance field on `OverpassOsmDataSource` itself. This mirrors the cache-sharing decision in ADR 002: `OverpassOsmDataSource` is registered Transient, so an instance-scoped tracker would not coalesce fetches across separate DI resolutions — the exact scenario (concurrent per-request consumers) this feature targets. Scoping to the cache instance means the tracker is shared for exactly as long as, and by exactly whoever shares, the cache it backs, with no separate lifetime to manage.

Concurrent callers for the same Bounds share the same `Lazy<Task<OsmData>>`. The first caller's `CancellationToken` governs the underlying HTTP request; a later, coalesced caller that cancels only abandons its own await; it does not cancel the fetch other callers are still waiting on. The in-flight entry is removed once the fetch settles (success or failure), so the next call is a normal cache hit, a normal cache miss that starts a fresh fetch, or joins a new in-flight fetch as appropriate.

## Alternatives considered
- **Global static `ConcurrentDictionary` keyed only by Bounds, independent of the cache instance**: rejected — would coalesce fetches across two `OverpassOsmDataSource` instances that were deliberately constructed with different, independent caches (e.g. in tests), leaking coalescing behavior across otherwise-isolated instances.
- **A per-key `SemaphoreSlim` or lock around the whole "check cache, fetch, populate cache" sequence**: rejected — serializes Bounds that aren't actually contended, and needs its own lifetime/cleanup management of comparable complexity to the chosen approach for no added benefit.
- **Each caller keeps its own cancellable fetch, only sharing the HTTP send**: rejected as unneeded complexity — issue #13's acceptance criteria only asks for a single outbound request with later callers awaiting the first caller's result, not per-caller cancellation of a shared fetch.

## Consequences
A caller that cancels while coalesced onto another caller's in-flight fetch does not stop that fetch — cancellation is best-effort for the initiating caller only, not for the shared request.

If two `OverpassOsmDataSource` instances with different constructor-configured query parameters (e.g. `queryTimeoutSeconds`, `queryMaxSizeBytes`) happen to share the same underlying `IMemoryCache` (e.g. the default process-wide shared cache) and coalesce on the same Bounds at the same time, the Overpass query actually sent uses whichever instance's configuration won the race to create the in-flight entry. The other instance's query-specific parameters are silently not applied to that particular request. This is an accepted, narrow edge case: instances sharing a cache are expected to be configured consistently in practice.
