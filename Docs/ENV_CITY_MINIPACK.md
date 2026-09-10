# Environment urbano — ENV_ZOMB01_PlayTest

Realizzazione nella sola scena `Assets/Scenes/ENV_ZOMB01_PlayTest.unity`.

## Composizione

- Strada diagonale e marciapiedi su area 16×10, con zona iniziale centrale libera per PG01/ZOMB01.
- Otto props: una auto abbandonata, due barriere in cemento, un cassonetto, due sezioni di recinzione e due lampioni.
- Camera ortografica sul piano XY esistente, senza cambi di inclinazione; ampiezza ortografica 5,5 e bordo esterno scuro.
- Il ground tecnico è sostituito dalle superfici del pack. I collider dei limiti originali sono conservati; eventuali renderer tecnici dei limiti sono nascosti.

Il pack sorgente è `C:\Users\EMME\Documents\ROG ZOMBIE\ASSET\GENERATI\ROG_ZOMBIE_City_MiniPack`. I sette PNG sono copiati una sola volta in Assets; nessun pixel è stato modificato e nessuna nuova grafica è stata generata.

## Superfici e limite del pack

Strada e marciapiede sono immagini di blocchi estrusi, non tessere seamless già pronte. Due mesh statiche riutilizzano la texture originale e selezionano con UV la superficie superiore, escludendo bordo verticale e ombra esterna. Le tessere sono accostate geometricamente e tagliate sul perimetro dell'area; non è usato ROAD_OSTACOLI.png o un unico fondale illustrato.

Crepe, segnaletica e variazioni di colore sono incorporate nelle immagini e si ripetono. La composizione non introduce curve, incroci, edifici o altre varianti mancanti. L'accostamento visivo finale non è stato provato: la valutazione in scena è lasciata all'utente.

## Importazione, profondità e ingombri

Sprite singoli con alpha, filtro Point, 64 pixel/unità, mipmap disattivate e compressione disattivata. I pivot dei props sono impostati al centro dell'appoggio a terra; le scale sono scelte di composizione della scena di test, non STATS gameplay.

`CityDepthSort` aggiorna soltanto lo SortingGroup in base alla Y della base. Raggruppa corpo e ombra di ZOMB01 e mantiene separati grafica, trasformazioni e collider. L'offset visivo dei piedi di ZOMB01 è −0,45 rispetto alla radice esistente; PG01 usa la propria radice. I personaggi conservano posizione iniziale, scala, animazioni, statistiche e logica precedente, senza reazione HIT.

I collider dei props sono piccoli BoxCollider2D con TestObstacle, collocati sul terreno. Auto, barriera e recinzione usano più rettangoli per approssimare la base diagonale; il lampione blocca soltanto la sua piccola base, non tutta la sagoma alta. Tutti i props sono OSTACOLI nella scena; i limiti originali mantengono il tipo MURO. La TestNavigation già esistente li raccoglie quando si avvia la sessione. Non sono state aggiunte IA, logiche di attacco o regole di navigazione.

## File creati

- `Assets/Art/Environment/CityMiniPack/`: i sette PNG `RZ_City_*_01.png` del pack e relativi `.meta`.
- `Assets/Art/Environment/CityMiniPack/Generated/Road_Tile_Surface.asset` e `Sidewalk_Tile_Surface.asset`: mesh delle superfici.
- Nella stessa cartella, `Road_Tile_Surface.mat` e `Sidewalk_Tile_Surface.mat`: materiali delle texture originali, con relativi `.meta`.
- `Assets/ENVPlayTest/CityDepthSort.cs` e `.meta`: ordinamento visivo.
- `Assets/ENVPlayTest/Editor/CityEnvironmentTools.cs` e `.meta`: costruzione dell'ambiente, senza funzioni di test.
- Nuove cartelle environment e relativi `.meta`.
- Questa guida.

## File modificati

- `Assets/Scenes/ENV_ZOMB01_PlayTest.unity`: ambiente, grouping visivo dei personaggi e composizione della camera.
- `Assets/ENVPlayTest/ENVPlayTestSession.cs`: testo del pannello aggiornato.

Il comando Editor di costruzione non è necessario per giocare. Se l'ambiente esiste già, si ferma per non sovrascrivere la disposizione modificata dall'utente. Per il tuning del layout modificare gli oggetti nella scena.

## Test lasciati all'utente

Come richiesto, per questa operazione non sono stati eseguiti test, Play Mode, input simulati, smoke test, controlli di compilazione separati o acquisizioni renderizzate della scena. Unity è stato usato esclusivamente per importare/costruire e salvare. Prima dell'istruzione erano stati soltanto ispezionati i PNG sorgenti e il codice; non erano state avviate prove dell'environment. Nessuno strumento di test temporaneo è stato aggiunto.

Per la tua prova: aprire la scena e lasciare terminare l'importazione; osservare superficie/scala, passare davanti e dietro ai props, provare gli ingombri e lasciare inseguire ZOMB01 intorno agli ostacoli. Mirando al corpo in linea libera, controllare il danno senza reazione HIT e la DEATH al colpo letale. Reset e toggle AI restano disponibili.

Nessun commit. Scene validate, GDD, AGENTS.md, statistiche, package e Render Pipeline non sono stati modificati da questa realizzazione.