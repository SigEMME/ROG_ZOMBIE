# Pre gameplay loop prototype

## Aggiornamento schermatura AREE — 14/09/2026

MURI/OSTACOLI schermano solo i bersagli con linea interrotta fra centro dell’AREA e centro del collider; i bersagli esposti entro la forma e il RAGGIO previsti ricevono la HIT completa prima della DEF, senza propagazione attorno agli angoli. La regola sostituisce gli SPICCHI anche per COLPO GROSSO, PIOGGIA DI GRANATE, PYROMANIA, LUCKY SHOT, ABILITÀ BONUS ad area e attacchi ad area dei MOB. SCIABOLATA resta un SEMICERCHIO. Le SEZIONI di FILO SPINATO/TRAPPOLA e le eccezioni esplicite restano invariate. Visuali ritagliate; PYROMANIA rivaluta coperture e visuale a ogni tick.

Nessun test, compilazione o build eseguito per questo aggiornamento. I report e le descrizioni storiche sotto precedono questa regola; le suite con aspettative sugli spicchi richiedono adeguamento prima di rieseguirle. GRANATA/MOLOTOV sono aggiornate nel GDD: il prototipo attuale offre gestione SLOT/consumo, non ancora i relativi effetti di combattimento.

Base: `6fa6b25` — GDD ufficiale `Docs/ROG_ZOMBIE_GDD.md`, revisione finale CLASSIFICAZIONE TECNICA, STATI LOGICI, COLLISIONI E TAG (12/09/2026).

## Avvio

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity` in Unity 6000.3.16f1 e premere Play. WASD muove il PG selezionato, mouse orienta, LMB attacca; Q usa l'ABILITA selezionata. Per BARRIERA, tenere Q mostra l'anteprima e rilasciare piazza il muro. Dettagli in Docs/PG01AbilitiesPrototype.md. Il pulsante Riprova test ripristina la RUN senza dipendere dalle Build Settings.

Il caricamento costruisce NavMesh e FIRST SPAWN off-screen. A1: 100 MOB, 30 iniziali. A2: 120 MOB, 36 iniziali. Una morte effettiva genera un rimpiazzo finché il totale è esaurito. All'ultima morte l'uscita diventa verde; raggiungere il cerchio di raggio 4 m. La freccia compare quando il centro uscita è off-screen. Scegliere una delle tre STATS e confermare. La seconda AREA conserva PG, selezioni ABILITÀ/PASSIVA, STATS, HP e G; il passaggio cura il 15% degli HP massimi correnti. Dopo il secondo BONUS il test termina, senza simulare vittoria RUN o BOSS. Il PG in DOWN termina il test perché non resta alcun PG attivo.

## Struttura

- `LoopDefinition` / `PreGameplayLoop.asset`: riferimenti agli asset esistenti, totali delle due AREE e posizione uscita. Geometria/collider/camera riusano TestAreaSettings; la copia runtime evita modifiche agli asset condivisi.
- `LoopSession`: ciclo Loading → Combat → AreaComplete → Bonus → Transition → seconda AREA → Finished; rami Defeat/Error; gestione della durata di vita di oggetti, navigazione e dati runtime.
- `ISpawnWorld`: piccolo contratto per riusare SpawnManager anche nella nuova scena. Il vecchio TestEngineBootstrap continua a soddisfarlo senza cambiare comportamento.
- `AreaExitTrigger`: collider IsTrigger su TRIGGER_PG e controllo logico della distanza per gli attori esistenti mossi senza Rigidbody dinamico. Nessun TAG aggiunto.
- `AreaStatBonus`: pesi GDD, tre proposte STATS provvisorie distinte, incremento 5% sul valore corrente, HP correnti aumentati dello stesso incremento, DEF limitata al cap 90%, CD REDUCTION arrotondata con minimo 10. Il CD REDUCTION conservato nella sessione alimenta il cooldown dell'ABILITA PG01 selezionata.
- `LoopHUD`: contatori, stato, direzione uscita, scelta e conferma, ripristino.
- `LoopValidation`: test opt-in in Play Mode, menu ROG ZOMBIE → Pre gameplay loop → Run engine smoke test. Richiede una sola scena salvata/non modificata e ripristina quella precedente. Non modificare scene durante il test.

Riutilizzati PlayerMovement, PlayerAim, PlayerStats, PlayerDefinition, PlayerRuntime, PG01 ScriptableObject, PG01_Weapon, ZOMB01, Combatant, MobBrain, MobSeparation, TestNavigation, SpawnManager, PlayerWeapon e visuali test. PG02, PG03, PG04, PG05, PG06, PG07 e PG08 sono selezionabili. Dettagli in `Docs/PG02Prototype.md`, `Docs/PG03Prototype.md` e `Docs/PG04Prototype.md` e `Docs/PG05Prototype.md` e `Docs/PG06Prototype.md` e `Docs/PG07Prototype.md` e `Docs/PG08Prototype.md`.

## Confini e valori

Il test è deliberatamente ridotto a un solo PG scelto fra PG01/PG02/PG03/PG04/PG05/PG06/PG07/PG08 e ZOMB01 al 100%, come richiesto. I totali 100/120 provengono dalla tabella C1 A1/A2; la composizione non rappresenta quella completa della campagna. Le due AREE usano il nuovo layout città 100 × 100 m ricostruito dalla reference del proprietario, con nuova generazione di MOB per AREA. SPAWN (-0,3; -46,4) m e USCITA (-1,6; 45,5) m. Dettagli in `Docs/CityTestLayout.md`.

Le PASSIVE PG01 sono disponibili tramite `Selected Passive` in `PreGameplayLoop.asset`, indipendente dall'ABILITÀ. La nuova RUN acquisisce la selezione; l'HUD mostra stato e STATS effettive. Dettagli e test in `Docs/PG01PassivesPrototype.md`.

Restano esclusi HUB completo (è disponibile il supporto tecnico descritto sotto), multiplayer, PG IA, altri PG/MOB, BOSS, CHEST e generazione ordinaria MEDI KIT (ELEMOSINA PG06 implementata), uso e acquisto ITEMS (PG04 dispone soltanto di controlli tecnici degli SLOT), effetti BONUS complessi ed EXP/LVL. I due slot BONUS abilità sono visibili ma disabilitati: questa è una schermata provvisoria, non l'intero sistema GDD 3+2. La selezione STATS distinta è una convenzione della UI di test, non una nuova regola definitiva di sorteggio. G viene conteggiato come diagnostica della RUN, senza economia persistente o regolamento del saldo a sconfitta.

Il GDD non fissa il raggio collider MOB: si conserva 0,35 m dal test esistente, con visuale provvisoria circolare corrispondente al diametro del collider (0,7 m). Non è un valore di bilanciamento approvato. Sono riusati separazione/steering e collisioni tramite sweep del test, senza cambiare la matrice Physics2D globale. Si registrano solo PG, MOB, MURO, OSTACOLO e TRIGGER_PG nei primi slot utente liberi. Per PG01 il cono ATTACCO BASE runtime usa 75°/4 m; le vecchie scene mantengono la geometria precedente. MOVE SPD usa la conversione GDD 100 = 2 m/s nello slice.

Nessuna modifica di GDD, PG asset, pacchetti, URP o versione Unity. Nessuna modifica manuale a Library/Temp/Logs/UserSettings. L'Editor può aggiornare autonomamente cache generate durante importazione/Play Mode. Nessun commit o push.

## Verifica

Il test automatico attraversa entrambe le popolazioni, controlla FIRST SPAWN, posizioni off-screen/NavMesh, una HIT reale del cono PG01, morte senza duplicati, rimpiazzi e cap, completamento, trigger, pausa, conferma singola, persistenza, cura 15%, secondo ciclo, reset e sconfitta. L'harness disabilita la IA durante lo svuotamento rapido e usa HIT di test per accelerare le morti: non misura il bilanciamento né sostituisce una partita manuale. L'esito effettivo è nel report consegnato, non va dedotto dalla sola presenza del test.

Passo successivo: prova manuale di mira, inseguimento/collisioni dell'orda e leggibilità uscita, poi integrare EXP/LVL usando solo GDD. Definire il collider definitivo tramite testing e completare la schermata BONUS 3+2 quando gli effetti entrano nello scope.

## HUB di prova — schermate implementate

Nel nuovo HUB di prova è disponibile una RUN con un solo PG controllabile e l’intero ROSTER PG01–PG08 sbloccato. È una configurazione tecnica del prototipo: le regole complete di PARTY e sblocco del GDD restano quelle del gioco.

La schermata PREPARAZIONE RUN segue il mockup `PREPARAZIONE-RUN_test.png` fornito dal proprietario: quattro BANNER visibili, soltanto il primo utilizzabile; gli altri tre bloccati e non interattivi, senza generare compagni IA. Il pulsante viola INDIETRO torna all’HUB conservando le modifiche effettuate fino a quel momento. Alla riapertura della preparazione la configurazione viene mantenuta.

Aprire `Assets/Scenes/HubPrototype.unity` e premere Play. WASD muove il segnaposto; raggiungere EXIT e premere F. La SELEZIONE PG mantiene le scelte usando INDIETRO, senza confermarle. Occorre confermare PG, ABILITÀ e PASSIVA prima di avviare dalla PREPARAZIONE RUN. Il riquadro arancione descrive l’ultima ABILITÀ/PASSIVA selezionata.

Verifica rapida in Unity: 106 controlli superati, zero warning durante la prova; controllo visivo delle schermate e navigazione completato. Nessuna RUN o prova di combattimento avviata. I dettagli, i limiti e le istruzioni del test sono in `Docs/HubPrototype.md`.

## Griglia metrica del pavimento

Le AREE del gameplay loop e la scena tecnica TestEngine mostrano una griglia 1 m × 1 m, allineata alle coordinate intere del mondo. Linee sottili semitrasparenti sopra il pavimento e sotto attori, ostacoli ed effetti. Generata insieme alla geometria di ciascuna AREA e rimossa con essa; nessun collider o modifica a movimento e distanze. Nel TestEngine sostituisce la precedente griglia da 5 m. Nessun test Unity o build eseguito; resa visiva da verificare.

## Riferimento visivo alle cure ricevute

Le cure effettive mostrano +N HP verde sopra il PG per 1,2 s, con salita e dissolvenza. Il valore corrisponde agli HP realmente recuperati dopo il limite degli HP massimi; cure a HP pieni non mostrano +0. Include cure PG02, PG06, MEDI KIT e CAMBIO AREA, tramite evento HealingApplied separato dalla logica visiva. Gli aumenti STATS tramite BONUS HP non sono classificati come cure. Nel loop il componente vive a livello RUN per mostrare anche la cura fra AREE; viene rimosso tornando all’HUB e i riferimenti a PG distrutti vengono scartati. Disponibile anche nel TestEngine, indipendente dall’opzione dei numeri danno. Nessun test Unity o build eseguito; resa visiva da verificare.

## MOB circolari

I segnaposto MOB nel gameplay loop e nel TestEngine sono cerchi colorati di diametro pari a 2 × ActorRadius. Con il valore corrente 0,35 m, visuale e collider hanno diametro 0,7 m; nel loop sostituiscono il precedente quadrato visivo di 1 m. Il CircleCollider2D e i CircleCast del movimento erano già circolari e restano invariati, così come NavMesh e separazione fra MOB. Nessun BoxCollider aggiunto e nessuna riduzione del raggio fisico. La modifica grafica non dimostra la risoluzione di eventuali incastri di navigazione, che restano da verificare in Play Mode.

## ABILITÀ BONUS nel loop

La precedente esclusione di EXP/LVL e ABILITÀ BONUS è superata: ora sono disponibili LEVEL UP, dieci ABILITÀ BONUS, UPGRADE e i due BANNER dedicati di FINE AREA. Regole, uso e limiti: [BonusAbilitiesPrototype.md](BonusAbilitiesPrototype.md). Nessuna build esportata per questa integrazione.
