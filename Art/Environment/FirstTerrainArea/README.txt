PRIMA AREA - TEST_TERRENO
La RUN di HubPrototype usa Area01_Terreno.prefab per AREA 1.
Origine: scena salvata Assets/ROG_TerrainTest/Test_140x140/Test_Terreno_140x140.unity.
Dimensione: 140 x 140 m.
Spawn gruppo: XY gameplay (34, -60), corrispondente a XZ terreno (34, -60).
Uscita: XY gameplay (-62, 33), sul termine della strada in alto a sinistra.
Confine provvisorio: quattro muri, spessore 1 m, altezza 2 m, con collider.
Materiale: Boundary.mat.

Aprire HubPrototype, selezionare i PG, confermare la preparazione RUN.
Verificare spawn sulla strada bassa, movimento/collisioni e camera prospettica.
Completare AREA 1 per aprire l'uscita sulla strada in alto a sinistra.
La transizione alle altre aree mantiene il flusso esistente.

Dati configurabili su PreGameplayLoop.asset: FirstAreaTerrain, TerrainBackgroundShader,
TerrainPlayerStart, TerrainExit.
Il prefab fotografa gli oggetti della scena salvata; modifiche successive alla disposizione
in Test_Terreno non si trasferiscono automaticamente. TerrainData e asset restano condivisi.
Il collegamento riusa TerrainGameplayBridge: rendering 3D, combattimento e collisioni XY.
Restano i limiti del test precedente: nessuna gestione dei dislivelli e nessuna occlusione
di PG/MOB sotto chiome e tetti. Collider complessi proiettati in ingombri rettangolari.
Nessuna variazione a statistiche, conteggi MOB o regole di apertura dell'uscita.

VERIFICA
Compilazione runtime ed Editor riuscita; warning CS0649 preesistenti runtime.
Unity ha esportato il prefab e verificato la navigazione con 1428 collider della mappa:
spawn valido, uscita valida, percorso completo fra i due.
Play Mode della RUN completa e prova multiplayer non eseguiti.
Nessuna build esportata, nessun commit.
