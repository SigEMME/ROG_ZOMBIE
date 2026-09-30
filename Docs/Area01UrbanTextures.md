# AREA_01 — superfici urbane
Implementazione del riferimento AREA_01_layout_Texture e anteprima texture.

- AREA_01: 144 x 144 m. Layout separato Area01UrbanLayout.asset; AREA_02 conserva CityTestLayout.
- Cinque isolati; carreggiate interne e perimetrali di 8 m, marciapiedi degli isolati di 3 m.
- Cordolo visivo di 20 cm incluso nei 3 m, non un nuovo ostacolo fisico.
- Fascia esterna di chiusura di 2 m oltre le strade perimetrali.
- 66 elementi originali conservati: coordinate e dimensioni adattate per tratti, senza modificare rotazioni o classificazione MURO/OSTACOLO. Un edificio centrale rialzato di 6 cm per tenerlo nel mattonellato.
- Partenza (-3,-65), uscita (-3,65).
- Texture ripetute in coordinate mondo: asfalto e mattonellato ogni 4 m; marciapiede ogni 3 m. Scala grafica provvisoria.
- Cordolo usa la striscia centrale opaca della texture originale; raccordi geometrici ad angolo retto.
- Shader esistente esteso con percorso texture opzionale; presentazione delle altre aree invariata.
- Compilazione C# riuscita; warning CS0649 preesistenti. Shader, importazione, resa e NavMesh non verificati in Play Mode.
- Avviare HubPrototype, preparare una RUN ed entrare in AREA_01. Verificare texture, cordoli, passaggi tra edifici e percorso fino all'uscita.
- Nessuna build, modifica GDD/AGENTS, package o commit.
Scala aggiornata: asfalto 4x4 m invariato; intera texture marciapiede 1x1 m; cordolo 20 cm con ciascuno dei quattro segmenti lungo 1 m (striscia completa 4 m); mattonellato modulo continuo 4x4 m con 8 colonne e 18 file sfalsate.
