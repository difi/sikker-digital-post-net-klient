#Sikker Digital Post .NET klient

Dette er en .NET-klient for sending av sikker digital post for det offentlige. Formålet for klienten er å forenkle integrasjonen 
som må utføres av avsendervirksomheter. For mer informasjon om sikker digital post, se [her](http://begrep.difi.no/SikkerDigitalPost/).

**NB: Klienten er under utvikling, og vil per dags dato ikke kunne brukes til å sende digital post.**

#Getting started

##NuGet-pakke

Klienten er tilgjengelig som en NuGet-pakke. Denne vil oppdateres jevnlig etter hvert som ny funksjonalitet legges til.

For å installere NuGet-pakken, gjør følgende:

1. Velg "TOOLS -> NuGet Package Manager -> Manage Nuget Packages for Solution..."
2. Søk etter "Sikker Digital Post Klientbibliotek".
3. Siden NuGet-pakken for dette prosjektet er en pre-release, må du sørge for at det står "Include Prerelease" i drop-down menyen rett over søkeresuløtatene (der det står "Stable Only").
4. Velg "Sikker Digital Post Klientbibliotek" og trykk "Install".

##Eksempelkode

Det er satt opp et eksempelprosjekt som viser bruk av klienten til å definere de ulike entitetene som må opprettes før sending av digital post. 
Dette prosjektet finner du under SikkerDigitalPost.Net.KlientDemo. Per dags dato er det kun Program.cs som er i bruk i eksempelprosjektet.