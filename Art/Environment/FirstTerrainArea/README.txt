MAPPE TERRENO - PRE GAMEPLAY LOOP
AREA 1: Assets/Scenes/C1_A1.unity, terreno 90 x 90 m.
Prefab: Assets/Environment/C1_Mappe/C1_A1_Terreno.prefab.
SPAWN sulla strada SUD: XY gameplay (-7, -39), XZ terreno (-7, -39).
USCITA sulla strada NORD: XY gameplay (8.3, 39), XZ terreno (8.3, 39).
Entrambi i punti sono al centro della strada, 6 m all'interno del bordo.

AREA 2: Assets/Scenes/C1_A2.unity, terreno 140 x 140 m.
Prefab: Assets/Environment/C1_Mappe/C1_A2_Terreno.prefab.
SPAWN e USCITA precedenti conservati: (34, -60) e (-62, 33).
AREA 3: layout di prova precedente, invariato.

Aprire HubPrototype, preparare la RUN e avviare; oppure usare PreGameplayLoopPrototype.
Ogni uscita mantiene le condizioni esistenti di completamento AREA e scelta ricompensa.
PG, BONUS, ITEMS e G mantengono le regole esistenti durante il cambio AREA.

AGGIORNAMENTO DA UNITY
Fuori dal Play Mode: ROG ZOMBIE > Mappe > Salva C1_A1 e C1_A2 e aggiorna PreGameplayLoop.
Salva le due scene se aperte e modificate; se chiuse usa la versione su disco.
Non salva altre scene. Attendere il messaggio in Console, quindi avviare una nuova RUN.
I prefab sono copie esportate delle scene: disposizione degli oggetti, luci e foschia
richiedono questo comando dopo le modifiche. TerrainData e materiali sono condivisi.

CONFIGURAZIONE
PreGameplayLoop.asset: FirstAreaTerrain, TerrainPlayerStart, TerrainExit;
SecondAreaTerrain, SecondTerrainPlayerStart, SecondTerrainExit; TerrainBackgroundShader.
Confini generati dalle dimensioni reali del terreno, spessore 1 m e altezza 2 m.
Luci, profilo colore e foschia sono inclusi; il riferimento della foschia al Terrain
viene rimappato durante l'esportazione. Il cambio AREA rimuove la presentazione precedente.
Limiti: nessuna gestione dei dislivelli; collider complessi proiettati in ingombri rettangolari.

VERIFICA
ROG ZOMBIE > Tests > Verifica sequenza mappe C1, con PreGameplayLoopPrototype aperta e salvata.
Il test verifica caricamento, FIRST SPAWN, percorsi, persistenza PG/G, pulizia atmosfera,
AREA 3 e reset RUN. Richiama direttamente le transizioni per isolare le mappe;
non sostituisce una RUN manuale completa di combattimento e ricompense.
Risultato in output/terrain-map-sequence-result.txt.
