# Studio visivo TOP-DOWN

Versione ufficiale **0.3.3**, confermata dal proprietario: integrazione TOP-DOWN nei tre livelli del test, con palazzi di altezza variabile fra 5 e 10 m e palette della scena TopDownStudy. Versione registrata nel progetto Unity; nessuna build esportata in questa operazione.

Aprire `Assets/Scenes/TopDownStudy.unity` (oppure menu **ROG ZOMBIE > Top-down > Open visual study**) e premere Play.

- WASD: muovere il PG dimostrativo.
- Mouse: orientare il PG verso il cursore sul terreno.
- TAB: alternare camera prospettica e ortografica, con campo visivo equivalente sul terreno.
- R: tornare al punto iniziale.

La strada separa due palazzi di 5 e 10 metri, con finestre, parapetti e impianti sul tetto. Spostandosi lungo le facciate si osserva la variazione della prospettiva. Camera verticale, piano XY per il terreno, asse Z negativo per l'altezza. Le collisioni si riferiscono alla base: il tetto proiettato non allarga il collider. I volumi sono renderizzati con un materiale dedicato compatibile con il renderer URP 2D già configurato; nessun cambio globale della pipeline.

È una scena isolata per valutare la direzione visiva ispirata al riferimento GTA 2. Dimensioni, velocità e grafica sono parametri tecnici della demo, non statistiche approvate per la RUN. Il PG è un segnaposto tridimensionale; non usa i dati o le abilità del roster. Nessun combattimento, MOB, NavMesh, progressione, salvataggio o schermata HUB. Il contenuto viene creato premendo Play.

Limiti della demo: grafica procedurale provvisoria, nessuna ombra dinamica o trasparenza automatica dei palazzi; il PG può essere nascosto dalla proiezione di un edificio. Quest'ultimo comportamento permette di valutare il problema prima di scegliere una regola di visibilità. La scena isolata non è inclusa nella build.

Verificatore: creare `.top-down-study-test.request` nella root con il percorso del report; l'Editor in Edit Mode, con scena salvata, apre la demo ed esegue i controlli. Lascia Play Mode aperto per il controllo visivo.

Verifica del 28 settembre 2026: compilazione riuscita e 12 controlli Play Mode superati, zero warning durante la verifica (`TOP-DOWN-validation-r4.txt`). Controllate visivamente facciate e inquadratura; verificato TAB nella finestra Game. Movimento e collisioni verificati tramite il verificatore, non con una sessione manuale prolungata. La prima esecuzione aveva un'asserzione troppo restrittiva sullo scorrimento lungo la facciata, corretta prima delle successive esecuzioni PASS.

## Integrazione nei tre livelli del gameplay loop

Dopo la prova manuale positiva del proprietario, la presentazione TOP-DOWN è stata applicata alle tre AREE esistenti. Avviare normalmente `HubPrototype` oppure `PreGameplayLoopPrototype`: la grafica viene generata all'ingresso in ogni AREA.

- Palazzi con i colori di TopDownStudy: facciate grigio/beige, tetti grigio-verdi e dettagli scuri. Altezze del corpo distribuite da 5 a 10 m, stabili fra avvii; i rettangoli collegati dello stesso edificio condividono l'altezza. Tetto e ventilazione si aggiungono sopra il corpo come nella demo.
- Ostacoli rossi con volumi bassi; bordo esterno scuro. La classificazione MURO/OSTACOLO dei dati originali rimane invariata.
- Asfalto procedurale nello spazio libero, marciapiedi decorativi attorno ai palazzi e griglia da 1 m. Nessuna nuova strada modifica il layout o la percorribilità.
- Camera prospettica verticale a 36 m; il campo visivo conserva la precedente estensione di terreno visibile (`CameraSize`). Spawn off-screen e mira lavorano sul piano del terreno. L'HUB recupera la propria proiezione al rientro.
- Volumi esclusivamente grafici: collider 2D, NavMesh, SPAWN, USCITE, dimensioni 100/100/130 m e regole di schermatura mantengono i dati precedenti. Nessun aggiornamento di URP o pacchetti.

Restano provvisori i segnaposto PG/MOB e gli effetti. Non sono introdotte ombre dinamiche o una nuova regola di trasparenza dei palazzi. Nessuna build esportata.

Aggiornamento altezze e palette: compilazione riuscita e verifica sulle tre AREE PASS con 116 controlli, zero warning (`TOP-DOWN-heights-validation-r1.txt`). Controllati intervallo 5–10 m, colore delle facciate, geometria, spawn, transizioni e ritorno all'HUB.

Il verificatore `.top-down-levels-test.request` riceve il percorso del report e prova le tre geometrie reali con 10 MOB per AREA solo nella copia runtime; `.top-down-levels-visual.request` aggiunge una sosta visiva di 20 s per AREA. Primo esito negativo causato dal totale MOB della fixture non multiplo di 10, poi corretto. Seconda esecuzione: 36 controlli PASS, zero warning (`TOP-DOWN-levels-validation-r2.txt`).

Verifica visiva e terza esecuzione: 36 controlli PASS, zero warning (`TOP-DOWN-levels-validation-r3.txt`); osservati in Game View asfalto, griglia, facciate blu e ostacoli rossi nelle AREE 1 e 3 (AREA 2 usa la stessa geometria di AREA 1). Regressione del gameplay loop: 251 asserzioni PASS (`TOP-DOWN-loop-validation-r1.txt`), inclusi attacco PG01, spawn off-screen, NavMesh, cambio AREA, bonus, persistenza, riavvio e sconfitta. La fixture di regressione disabilita l'IA dei MOB durante lo svuotamento delle popolazioni: non sostituisce una prova manuale completa del combattimento.
