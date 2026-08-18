# 001 — Overpass som dynamisk datakilde

## Kontekst
Biblioteket leser i dag OSM-data fra fil på disk. En konsument som skal
spørre om vilkårlige steder kan ikke forhåndslaste alt.

## Beslutning
Innfører `IOsmDataSource` med en Overpass-implementasjon som henter et
bbox over HTTP ved behov. Cache i minnet for å unngå gjentatte kall.

## Alternativer vurdert
- Fil-basert med forhåndsnedlasting: skalerer ikke, krever manuelt arbeid.
- Egen OSM-database (PostGIS): for tungt for dette formålet nå.

## Konsekvenser
Biblioteket får en nettverksavhengighet. Må håndtere timeout, rate limiting
og at Overpass er en gratis fellestjeneste med bruksbegrensninger.