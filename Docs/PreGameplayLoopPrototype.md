# Pre gameplay loop prototype

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

Riutilizzati PlayerMovement, PlayerAim, PlayerStats, PlayerDefinition, PlayerRuntime, PG01 ScriptableObject, PG01_Weapon, ZOMB01, Combatant, MobBrain, MobSeparation, TestNavigation, SpawnManager, PlayerWeapon e visuali test. PG02, PG03 e PG04 sono selezionabili; PG05–PG08 non vengono istanziati. Dettagli in `Docs/PG02Prototype.md`, `Docs/PG03Prototype.md` e `Docs/PG04Prototype.md`.

## Confini e valori

Il test è deliberatamente ridotto a un solo PG scelto fra PG01/PG02/PG03/PG04 e ZOMB01 al 100%, come richiesto. I totali 100/120 provengono dalla tabella C1 A1/A2; la composizione non rappresenta quella completa della campagna. Le due AREE riusano il layout tecnico esistente, con colore diverso e nuova generazione di MOB. L'uscita (12,0) è un posizionamento di test configurabile, non un valore di design definitivo.

Le PASSIVE PG01 sono disponibili tramite `Selected Passive` in `PreGameplayLoop.asset`, indipendente dall'ABILITÀ. La nuova RUN acquisisce la selezione; l'HUD mostra stato e STATS effettive. Dettagli e test in `Docs/PG01PassivesPrototype.md`.

Restano esclusi HUB, multiplayer, PG IA, altri PG/MOB, BOSS, CHEST/MEDI KIT, uso e acquisto ITEMS (PG04 dispone soltanto di controlli tecnici degli SLOT), passive PG05–PG08, effetti BONUS complessi ed EXP/LVL. I due slot BONUS abilità sono visibili ma disabilitati: questa è una schermata provvisoria, non l'intero sistema GDD 3+2. La selezione STATS distinta è una convenzione della UI di test, non una nuova regola definitiva di sorteggio. G viene conteggiato come diagnostica della RUN, senza economia persistente o regolamento del saldo a sconfitta.

Il GDD non fissa il raggio collider MOB: si conserva 0,35 m dal test esistente, con visuale provvisoria di 1 m. Non è un valore di bilanciamento approvato. Sono riusati separazione/steering e collisioni tramite sweep del test, senza cambiare la matrice Physics2D globale. Si registrano solo PG, MOB, MURO, OSTACOLO e TRIGGER_PG nei primi slot utente liberi. Per PG01 il cono ATTACCO BASE runtime usa 75°/4 m; le vecchie scene mantengono la geometria precedente. MOVE SPD usa la conversione GDD 100 = 2 m/s nello slice.

Nessuna modifica di GDD, PG asset, pacchetti, URP o versione Unity. Nessuna modifica manuale a Library/Temp/Logs/UserSettings. L'Editor può aggiornare autonomamente cache generate durante importazione/Play Mode. Nessun commit o push.

## Verifica

Il test automatico attraversa entrambe le popolazioni, controlla FIRST SPAWN, posizioni off-screen/NavMesh, una HIT reale del cono PG01, morte senza duplicati, rimpiazzi e cap, completamento, trigger, pausa, conferma singola, persistenza, cura 15%, secondo ciclo, reset e sconfitta. L'harness disabilita la IA durante lo svuotamento rapido e usa HIT di test per accelerare le morti: non misura il bilanciamento né sostituisce una partita manuale. L'esito effettivo è nel report consegnato, non va dedotto dalla sola presenza del test.

Passo successivo: prova manuale di mira, inseguimento/collisioni dell'orda e leggibilità uscita, poi integrare EXP/LVL usando solo GDD. Definire il collider definitivo tramite testing e completare la schermata BONUS 3+2 quando gli effetti entrano nello scope.
