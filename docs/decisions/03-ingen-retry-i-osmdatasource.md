# 003 — Ingen automatisk retry i IOsmDataSource

## Kontekst
Overpass er en gratis fellestjeneste som returnerer HTTP 429 (rate limiting) og 504 (server travel/timeout) under belastning (ADR 001). Et HTTP-klientbibliotek ville typisk forventes å håndtere dette med retry/backoff, så fraværet av det er lett å anta er en forglemmelse uten en skriftlig begrunnelse.

## Beslutning
IOsmDataSource gjør ingen automatisk retry. Feil kastes umiddelbart som en standard `HttpRequestException` (via `EnsureSuccessStatusCode()`), som bærer HTTP-statuskoden i `StatusCode`-egenskapen. Konsumenten avgjør selv om og hvordan den vil implementere backoff.

## Alternativer vurdert
- Innebygd retry med backoff (f.eks. via Polly): skjuler potensielt lange forsinkelser for konsumenten og legger til en ny avhengighet, uten at biblioteket kjenner konsumentens toleranse for ventetid eller bruksmønster.

## Konsekvenser
Konsumenter som ønsker robusthet mot 429/504 må selv implementere retry rundt IOsmDataSource. Dette er en bevisst avgrensning i tråd med "ikke design for hypotetiske fremtidige krav", ikke en forglemmelse.
