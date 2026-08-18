# 002 — Injisert singleton IMemoryCache som cache for IOsmDataSource

> **Rettet 2026-08-18**: Denne ADR-en beskrev opprinnelig den motsatte konklusjonen av det som faktisk ble besluttet i grillingen (spørsmål 3). Innholdet under er korrigert til å reflektere den faktiske beslutningen; det er ingen ny avveining mot en tidligere gyldig beslutning, men en retting av en feilregistrering.

## Kontekst
IOsmDataSource cacher Overpass-svar i minnet for å unngå gjentatte kall (ADR 001). Bbox-verdier er brukerstyrte og ubegrensede — det finnes ingen naturlig endelig nøkkelrom å cache mot. `OverpassOsmDataSource` registreres Transient i `AddOsmToolkit()`, i tråd med alle andre registreringer i biblioteket.

## Beslutning
Cachen er en singleton `IMemoryCache`, registrert via `services.AddMemoryCache(options => options.SizeLimit = OverpassOsmDataSource.DefaultCacheSizeLimit)` i `AddOsmToolkit()`, og injisert i `OverpassOsmDataSource` som en valgfri konstruktørparameter — samme "valgfri avhengighet, fornuftig standardverdi"-mønster som brukes for `HttpClient`, deserializer og logger. Ved direkte konstruksjon uten en DI-container (ingen `IMemoryCache` injisert) faller implementasjonen tilbake til en internt eid, statisk delt `IMemoryCache`-instans, samme mønster som den eksisterende statiske `SharedHttpClient`-instansen — slik fungerer biblioteket fortsatt uten oppsett rett ut av boksen.

`OverpassOsmDataSource` eier ikke selv cache-instansen og implementerer derfor ikke `IDisposable`.

Cache-oppføringer utløper etter en konfigurerbar varighet (standard: 15 minutter) og vektes med antall elementer (noder + veier + relasjoner) etter deserialisering, mot en total størrelsesgrense (standard: 200 000). En enkeltoppføring hvis vekt overstiger grensen blir ikke en feil — den blir ganske enkelt ikke cachet (`Set()` fullfører normalt, men et påfølgende oppslag gir cache-miss).

Cache-nøkkelen er avledet internt fra de fire bounds-verdiene med eksakt likhet (ingen avrunding eller fuzzy matching). Den eksisterende offentlige bounds-typen endres ikke for å legge til verdilikhet — nøkkelen bygges og eies utelukkende internt i datakilden.

## Alternativer vurdert
- **Dedikert, internt eid `MemoryCache`-instans konstruert per instans av `OverpassOsmDataSource`** (det denne ADR-en opprinnelig — feilaktig — beskrev som valgt løsning): vurdert og forkastet. Fordi `OverpassOsmDataSource` er registrert Transient, ville en privat, instanseid cache bli gjenskapt tom ved hver DI-oppløsning i vanlige konsumpsjonsmønstre (f.eks. en scoped per-request-tjeneste som injiserer `IOsmDataSource`) — dette ville i praksis gjøre selve cache-funksjonen virkningsløs. I tillegg, dersom instansen løses direkte fra en langlevd rot-scope, spores og disposes transiente `IDisposable`-tjenester av DI-containeren først når rot-scopet selv disposes — en klassisk "captive dependency"-lekkasje, siden ingenting ellers ville kalt `Dispose()` på den private cachen underveis.
- **Håndrullet ConcurrentDictionary uten utløp**: enkel, men ubegrenset minnevekst gitt vilkårlige bbox-nøkler — reell minnelekkasje-risiko.

## Konsekvenser
Ny NuGet-avhengighet (Microsoft.Extensions.Caching.Memory) for pakkens konsumenter.

Dette er en delt, generell `IMemoryCache`-instans: dersom en vertsapplikasjon også registrerer sin egen `IMemoryCache` via `AddMemoryCache()` (til eget, urelatert bruk), deler den samme singleton-instansen som OsmToolkits Overpass-cache, siden `AddMemoryCache()` bruker `TryAdd`-semantikk. Siden OsmToolkit setter `SizeLimit` på denne delte cachen, må *alle* vertsapplikasjonens egne `Set()`-kall på samme cache oppgi `Size`, ellers kaster de en `InvalidOperationException`. Dette er en bevisst, akseptert konsekvens av denne beslutningen — ikke en forglemmelse — men bør tas med i vurderingen for enhver vertsapplikasjon som selv bruker `IMemoryCache` til generell caching.

Cache-oppføringers vekt (nodes + ways + relations etter parsing) er rent minnestyring internt i cachen, ikke beskyttelse mot at et enkeltsvar er for stort. Den beskyttelsen ligger i `[maxsize]` i selve Overpass-spørringen (se implementasjon), som stopper et for stort svar før det i det hele tatt kommer over nettverket.

Datakilden returnerer alltid en grunn kopi av cachet `OsmData` ved oppslag, aldri instansen som faktisk ligger lagret i cachen — dermed kan en kallers mutasjon av sitt eget returnerte resultat ikke korrumpere den cachede oppføringen for andre kallere.
