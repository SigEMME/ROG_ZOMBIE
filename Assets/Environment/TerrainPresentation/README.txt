PROVA GAMEPLAY TERRENO - 02/10/2026

VISIBILITA ERBA - 03/10/2026
Durante la RUN e nella prova gameplay, TerrainGameplayBridge calibra Detail Distance
sulla camera 3D del terreno. Considera angoli visibili, altezza, zoom e aspect ratio,
con 5 m di margine piu una patch di dettagli. Usa il culling nativo del Terrain.
CullGrassByCamera e GrassVisibilityMargin permettono di disabilitarlo o regolare
il margine. La distanza originale viene ripristinata alla disattivazione/distruzione.
Nessuna modifica alla densita dipinta, ai MOB, ai collider o al TerrainData.
Il limite riguarda tutti i Paint Details del Terrain (attualmente i ciuffi), non gli
alberi o i prefab posizionati a mano. Non limita la visualizzazione in Scene View.
Con camera a 36 m, semi-altezza visibile 9 m e formato 16:9: circa 51,6 m,
invece di 80 m sul terreno attuale. Il guadagno FPS dipende dall'erba dipinta;
non e stato misurato con un benchmark della RUN.

SCENE E PROVA ATTUALE
La vecchia scena Terreno_GameplayTest e il relativo comando Editor sono stati rimossi.
Usare Assets/Scenes/PreGameplayLoopPrototype.unity oppure HubPrototype.
TerrainGameplayBridge e le altre risorse di questa cartella rimangono necessarie
al rendering e alle collisioni delle mappe C1_A1 e C1_A2 nel loop.
Per verificare il collegamento: ROG ZOMBIE > Tests > Verifica sequenza mappe C1.

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
Le capsule PG/MOB sono rese dalla camera del terreno: tetti e chiome le occultano tramite profondita.
Restano nel piano di combattimento i collider, i proiettili e gli effetti 2D.
Due camere attive: prestazioni e allineamento da verificare in Play Mode.
Il test mantiene HUD, raccolte e progressione del TEST ENGINE #2; non e' la RUN Hub completa.
Nessuna migrazione generale a Physics3D o modifica delle altre scene.

FILE
TerrainGameplayBridge.cs: collegamento visuale, ingombri, partenza libera, impostazioni runtime.
TerrainGameplayBackground.shader: visualizzazione della mappa sullo sfondo.
TestEngineBootstrap.cs: due collegamenti opzionali per geometria e partenza; comportamento standard invariato senza bridge.

VERIFICA ATTUALE
Usare il test ROG ZOMBIE > Tests > Verifica sequenza mappe C1.
Report: output/terrain-map-sequence-result.txt.
Le vecchie risorse di prova non utilizzate sono state rimosse da Assets.
Shader e componenti necessari al loop sono conservati qui con GUID invariati.
Lo shader della geometria provvisoria si trova in Assets/Environment/Shaders/TopDownEnvironment.shader.
