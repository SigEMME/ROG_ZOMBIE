# HUB e schermate di preparazione — prototipo

## Avvio e utilizzo

Aprire `Assets/Scenes/HubPrototype.unity` in Unity 6000.3.16f1 e premere Play. Nell’HUB tecnico, WASD muove il quadrato blu; raggiungere EXIT e premere F per aprire PREPARAZIONE RUN.

1. Premere il primo BANNER. Gli altri tre sono bloccati e non creano PG IA.
2. Selezionare uno degli otto PG sbloccati, una ABILITÀ e una PASSIVA indipendenti. Cambiare PG azzera le due opzioni.
3. Il riquadro arancione mostra l’ultima ABILITÀ/PASSIVA selezionata; il testo lungo è scorrevole. Le STATS arrivano dagli asset PG; RANGE resta nascosto.
4. INDIETRO torna alla preparazione conservando le scelte ma senza conferma. Riaprire la selezione mantiene le scelte e richiede una nuova conferma.
5. CONFERMA nella selezione torna alla preparazione con stato CONFERMATO. CONFERMA PREPARAZIONE / AVVIA RUN acquisisce una copia della configurazione per il loop esistente.
6. INDIETRO dalla preparazione torna all’HUB e mantiene configurazione e stato di conferma. La conservazione vale nella sessione Play corrente, senza salvataggio su disco.

Il collegamento alla RUN usa il loop di due AREE già esistente; il pulsante di ritorno all’HUB è previsto a test completato o sconfitta. La scena originale del loop resta disponibile per l’avvio diretto dall’Inspector.

## File e separazione dei dati

- `RunPreparationSelection.cs`: scelta, conferma, navigazione e copia della configurazione RUN, senza modificare gli asset condivisi.
- `RunPreparationUI.cs`: layout provvisorio conforme ai riferimenti approvati, con adattamento uniforme alla finestra.
- `RunPreparationContent.json`: nomi e descrizioni ricavati dal GDD; da aggiornare insieme alle future modifiche di design.
- `HubPrototype.cs`: HUB tecnico, apertura della preparazione e collegamento al loop.
- `Editor/HubScreensValidation.cs`: test opt-in delle sole schermate.
- `Assets/Scenes/HubPrototype.unity`: nuova scena, senza modifiche alle Build Settings.

## Verifica effettiva

Eseguito in Unity 6000.3.16f1 il test dal menu `ROG ZOMBIE > Pre gameplay loop > Run HUB screen tests`. Richiede una sola scena salvata e non modificata. Lascia Play Mode aperto per l’ispezione visiva; alla sua chiusura ripristina la scena precedente.

Report `HUB-screens-validation-r1.txt` e `HUB-screens-validation-r2.txt`: entrambi PASS, 106 controlli, zero warning durante la prova. La seconda esecuzione verifica la versione finale dopo gli aggiustamenti di contrasto ed etichette. Compilazione riuscita; rimangono i warning preesistenti CS0252 in PG05Validation.cs (righe 317 e 320), estranei alle schermate.

Controllati: roster completo, tre BANNER bloccati, scelta indipendente ABILITÀ/PASSIVA, obbligo di entrambe, descrizione dell’ultima scelta, conservazione delle bozze, conferma esplicita, azzeramento delle opzioni cambiando PG e copia valida della configurazione per tutti gli otto PG. Verificata l’assenza di LoopSession e combattenti durante il test.

Ispezione visiva in Game View: SELEZIONE PG, PREPARAZIONE RUN con bozza e configurazione confermata, ritorno all’HUB. Play Mode terminato al termine della verifica.

## Limiti

HUB rappresentato da una superficie tecnica e un segnaposto, senza NPC, economia, gestione ITEMS, multiplayer o IA. Anteprime e BANNER PG usano segnaposto, non grafica definitiva. Lo SLOT ITEM vuoto è solo indicativo. Le etichette lunghe possono andare a capo nei pulsanti circolari.

Su richiesta, non sono stati eseguiti combattimenti, test delle abilità, caricamento effettivo della RUN, cambio AREA o ritorno da una RUN terminata: il collegamento è implementato ma questi percorsi non sono convalidati da questa prova delle schermate. Nessun aggiornamento di Unity o pacchetti e nessun commit o push.

## Build Windows di prova

Il proprietario ha confermato il funzionamento del percorso completo HUB → RUN → HUB tramite una propria prova manuale, successiva ai test delle schermate descritti sopra.

Esportazione ripetibile dal menu `ROG ZOMBIE > Pre gameplay loop > Build Windows HUB prototype`, fuori da Play Mode. Genera una build Windows x64 Development in `Builds/Windows-HUB-<data-ora>/`, con `HubPrototype.unity` come prima e unica scena inclusa. Il loop viene creato a runtime. Il metodo non cambia l’elenco delle scene nelle Build Settings né i parametri del giocatore. Esito, dimensioni e warning sono registrati in `Build-report.txt`; `Builds/Latest-HUB-build.txt` indica l’ultima cartella esportata.

Distribuire l’intera cartella della build, non il solo eseguibile. Estrarre lo ZIP prima di avviare `ROG_ZOMBIE.exe`. Per chiudere il prototipo usare Alt+F4.

Esportazione del 13/09/2026 completata: `Builds/Windows-HUB-20260913-170818/ROG_ZOMBIE.exe`, Windows x64 Development, Unity 6000.3.16f1. BuildReport: Succeeded, 0 errori, 0 warning, 167845723 byte, durata 2m13s. Avvio del player osservato tramite log fino all’inizializzazione di assembly, fisica e input, senza eccezioni gestite. Presente il messaggio diagnostico D3D12 `failed to query info queue interface (0x80004002)`. La finestra standalone non era raggiungibile dallo strumento di controllo: nessuna verifica visiva standalone attestata.

Durante l’esportazione Unity ha serializzato aggiornamenti automatici a profili URP, GraphicsSettings e PlayerSettings (preloaded input asset e batching). È rimasta anche la selezione PG08 PASSIVA 2 salvata dall’Editor in PreGameplayLoop.asset; non è stata ripristinata, per preservare le scelte presenti nella sessione. Nessuna modifica manuale ai valori di gameplay o aggiornamento di Unity/pacchetti.
