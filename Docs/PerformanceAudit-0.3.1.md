# Controllo prestazioni — 0.3.1

Data: 15 settembre 2026. Nessuna ottimizzazione gameplay applicata e nessuna build generata o sostituita.

## Esito

La prima priorità emersa è la ricostruzione grafica delle aree schermate, seguita dalla scalabilità dei MOB e dalla ricerca delle posizioni alternative del compagno. Queste misure non dimostrano ancora un guadagno ottenibile né costituiscono una valutazione degli FPS della build distribuita.

## Ambiente e metodo

- Unity 6000.3.16f1, **Editor Play Mode**; finestra di gioco 1534 × 664, VSync 0, targetFrameRate -1.
- Intel Core i5-13450HX, 16 processori logici, circa 16 GB RAM; NVIDIA GeForce RTX 5050 Laptop GPU.
- Tempo frame, Main Thread, GC Allocated In Frame e Total Used Memory acquisiti tramite ProfilerRecorder. I valori includono il contesto Editor e non isolano CPU/GPU della build standalone.
- Scenari MOB: geometria della città, due PG fermi con HP elevati solo nella fixture, input e formazione automatica disabilitati, ZOMB01 con movimento/attacco attivi. Nessuno sparo del PLAYER. 30 frame di riscaldamento e 180 frame misurati per scenario.
- I tentativi di generare 30 e 100 MOB hanno prodotto rispettivamente **26 e 82 MOB**, perché alcuni punti non erano campionabili sulla NavMesh. Le etichette CITY_30_MOB e CITY_100_MOB_STRESS nei log indicano il numero richiesto; le tabelle qui riportano quello effettivo.
- Microprove: 5 chiamate di riscaldamento e 60 misurate, una per frame, con Stopwatch attorno alla sola chiamata. Distruzione differita degli oggetti, rendering GPU e resto del frame non sono compresi nel tempo della chiamata.
- Non sono prove prolungate di temperatura, stabilità o perdite di memoria; non simulano una RUN completa con tutte le combinazioni di abilità.

## Risultati frame — acquisizione r3

| MOB effettivi | Media frame | Mediana | P95 | Massimo |
|---|---:|---:|---:|---:|
| 0 | 2,93 ms | 2,75 ms | 4,69 ms | 10,19 ms |
| 26 | 3,19 ms | 3,09 ms | 4,23 ms | 6,26 ms |
| 82 | 8,81 ms | 9,54 ms | 10,98 ms | 11,35 ms |

P95 indica il valore sotto cui ricade circa il 95% dei campioni. Main Thread segue lo stesso andamento. Le variazioni tra acquisizioni, anche a scena vuota, confermano che non va attribuito al gameplay ogni picco dell'Editor.

Il contatore GC per frame è valido e registra in media circa 20,0 / 22,8 / 32,3 kB nei tre scenari. Sono valori aggregati, non attribuibili a un singolo sistema. Total Used Memory è circa 1,31 GiB nel contesto Editor: **non è il consumo della build del gioco**.

## Risultati per chiamata — acquisizione r3

| Operazione | Mediana | P95 |
|---|---:|---:|
| Ricerca IA, posizione libera | 0,025 ms | 0,078 ms |
| Ricerca IA, posizione dietro un MURO | 0,233 ms | 0,329 ms |
| Ricerca IA, 16 PG di prova occupano lo spazio locale | 1,386 ms | 1,659 ms |
| Movimento IA con cursore in rotazione | 0,053 ms | 0,146 ms |
| Grafica area raggio 4 m, nessuna copertura | 0,224 ms | 0,360 ms |
| Grafica area raggio 4 m, 4 coperture sintetiche | 1,829 ms | 2,122 ms |
| Grafica area raggio 4 m, 12 coperture sintetiche | 6,115 ms | 6,564 ms |
| Grafica area raggio 4 m, spawn della città reale | **6,644 ms** | **7,376 ms** |
| Creazione flash area con 12 coperture | 6,226 ms | 7,008 ms |

La fixture alla posizione di spawn ha 4 coperture entro 4 m, ma conserva tutti gli ostacoli della città. Il codice di schermatura percorre la lista globale degli ostacoli per ogni raggio; il confronto con le 4 coperture sintetiche indica che questo percorso merita un intervento prioritario. Il tempo riportato è **per ricostruzione**, non un costo fisso applicato a ogni frame. Più ricostruzioni nello stesso frame possono però sommarsi.

Le prime due acquisizioni confermano l'ordine di grandezza dei casi ripetuti: circa 0,23 ms per la ricerca IA contro MURO, 1,35–1,39 ms con spazio locale occupato e circa 6 ms per il visual con 12 coperture. Il caso città reale e il caso a 4 coperture sono stati aggiunti in r3.

### Contatore allocazioni non utilizzabile

`GC.GetAllocatedBytesForCurrentThread()` restituisce 0 anche dopo un'allocazione di controllo da 65.536 byte. Pertanto le allocazioni delle singole chiamate sono **NON DISPONIBILI**. Gli zeri nei log r1/r2 sono invalidati dalla calibrazione r3 e non dimostrano assenza di garbage. Questo limite non invalida i tempi Stopwatch o il distinto contatore ProfilerRecorder delle allocazioni per frame.

## Interventi consigliati, non ancora applicati

1. **Aree schermate:** selezionare una sola volta le coperture rilevanti per la ricostruzione e riutilizzare i loro dati geometrici; riutilizzare mesh, materiali e buffer. Conservare l'aggiornamento quando una copertura nasce, sparisce o cambia. Non modificare i controlli dei danni per ottimizzare soltanto la grafica. Riferimenti: `PG04AreaVisual.InitializeOccluded` / `RenderMesh`, `AttackGeometry.VisibleDistance`.
2. **MOB:** profilare separatamente movimento, separazione e fisica. `MobSeparation` esamina ripetutamente tutti i combattenti per ciascun MOB; valutare una ricerca locale dei vicini e buffer riutilizzabili per le query fisiche. Il costo individuale di questi metodi non è ancora stato isolato dalle misure frame.
3. **Compagno:** eliminare la seconda chiamata Path dopo una candidatura già validata; riutilizzare la soluzione ancora valida; limitare il lavoro ripetuto della ricerca estesa senza alterare la preferenza entro 1 m e l'eccezione autorizzata. L'algoritmo locale prova fino a 360 punti e quello esteso può esaminarne molti di più: il caso massimo teorico non è stato misurato.
4. **Oggetti temporanei:** valutare pooling dopo le ottimizzazioni sopra. Il confronto visual/flash suggerisce che, nella fixture misurata, gran parte del costo immediato è nella ricostruzione dell'area, non nella sola creazione del GameObject.

Confrontare ogni intervento sullo stesso scenario, poi rieseguire i test su schermatura, aree persistenti, cambio coperture e formazione. Una conferma degli FPS e del costo GPU richiederà una sessione di profiling della build standalone; non è stata esportata una nuova build per questo controllo.

## File e verifiche

- Aggiunti `Assets/PreGameplayLoop/Editor/PerformanceAudit.cs` e relativo `.meta`: strumento esclusivamente Editor, non incluso nel player.
- Report grezzi: `PERFORMANCE-audit-0.3.1-r1.txt`, `r2.txt`, `r3.txt`; tutti completati senza errori o warning durante l'acquisizione.
- Compilazione del verificatore riuscita; restano i due warning CS0252 preesistenti in `PG05Validation.cs`.
- Nessuna modifica al GDD, alle statistiche persistenti o alla logica runtime durante questo controllo. Nessun commit/push.
- Per ripetere: salvare la scena, uscire dal Play Mode, aggiornare gli script; creare `.performance-audit.request` nella radice del repository contenente il percorso completo di un nuovo report. Il verificatore apre la scena di prova, esegue le acquisizioni, esce dal Play Mode e ripristina la scena precedente.
