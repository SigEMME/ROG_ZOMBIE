PROVA GAMEPLAY TERRENO - 02/10/2026

SCENA
Assets/Scenes/Terreno_GameplayTest.unity
Copia della mappa salvata Assets/ROG_TerrainTest/Test_140x140/Test_Terreno_140x140.unity.
La scena sorgente non e' stata modificata. TerrainData e asset grafici restano condivisi:
non dipingere/modificare il terreno nella copia se si desidera preservare i dati originali.

COME PROVARE
Aprire Terreno_GameplayTest, oppure ROG ZOMBIE > Terreno > Apri prova gameplay PG e MOB.
Attendere importazione e compilazione, premere Play.
Attendere caricamento NavMesh e FIRST SPAWN.
WASD movimento, mouse mira, sinistro attacco.
HUD esistente: HP, conteggi MOB, selezione PG e Riprova.
PG01 iniziale, un solo personaggio. Nessun compagno IA.
MOB e statistiche da Assets/TestEngine/Data/TestAreaSettings.asset:
330 totali, FIRST SPAWN 35% = 116 contemporanei.
La copia runtime usa terreno 140x140 e conversione MOVE SPD 100 = 2 m/s.
Il punto iniziale e' il punto NavMesh libero piu vicino al centro.
Gli spawn mantengono le regole esistenti di distanza fuori schermo e raggiungibilita.

IMPLEMENTAZIONE E LIMITI
Il terreno e i modelli restano 3D XZ, resi da una camera prospettica su RenderTexture.
Vista verticale top-down, altezza 36 m come TopDownEnvironment.
Camera di combattimento alla stessa distanza dal piano XY, con identico campo visivo.
Il campo visivo deriva da CameraSize: mantiene la precedente copertura del terreno.
Il piano di sfondo resta dietro agli attori e riempie l'inquadratura prospettica.
Combattimento, movimento, proiettili, MOB e NavMesh riusano il prototipo XY.
Gli ostacoli sono box 2D orientati derivati dai collider 3D; capsule alberi proiettate sul tronco.
Sono ingombri conservativi: annessi e forme concave possono occupare spazi piu ampi del visibile.
Tutte le geometrie solide sono trattate come MURO per questa prova.
Nessuna navigazione negli edifici, nessuna gestione di dislivelli.
PG, MOB e proiettili sono sovrapposti alla vista del terreno: tetti/chiome non li occultano.
Due camere attive: prestazioni e allineamento da verificare in Play Mode.
Il test mantiene HUD, raccolte e progressione del TEST ENGINE #2; non e' la RUN Hub completa.
Nessuna migrazione generale a Physics3D o modifica delle altre scene.

FILE
TerrainGameplayBridge.cs: collegamento visuale, ingombri, partenza libera, impostazioni runtime.
TerrainGameplayBackground.shader: visualizzazione della mappa sullo sfondo.
Editor/TerrainGameplayTestTools.cs: menu apertura e controllo avvio su richiesta esplicita.
TestEngineBootstrap.cs: due collegamenti opzionali per geometria e partenza; comportamento standard invariato senza bridge.
SourceMapSnapshot.txt: hash della scena sorgente usata per il test.

VERIFICA
Compilazione runtime ed Editor riuscita. Warning CS0649 su campi serializzati preesistenti.
Verificati ID univoci, riferimenti nuovi nella scena e hash sorgente invariato.
Tentata verifica automatica Play Mode, non avviata dall'Editor durante questa sessione.
Nessuna richiesta automatica lasciata pendente.
Shader, resa visuale, navigazione e combattimento da confermare nell'Editor.
Nessuna build esportata, nessun commit.
