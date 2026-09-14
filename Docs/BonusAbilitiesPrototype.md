# ABILITÀ BONUS — gameplay loop prototype

## Aggiornamento schermatura AREE — 14/09/2026

MURI/OSTACOLI schermano solo i bersagli con linea interrotta fra centro dell’AREA e centro del collider; i bersagli esposti entro la forma e il RAGGIO previsti ricevono la HIT completa prima della DEF, senza propagazione attorno agli angoli. La regola sostituisce gli SPICCHI anche per COLPO GROSSO, PIOGGIA DI GRANATE, PYROMANIA, LUCKY SHOT, ABILITÀ BONUS ad area e attacchi ad area dei MOB. SCIABOLATA resta un SEMICERCHIO. Le SEZIONI di FILO SPINATO/TRAPPOLA e le eccezioni esplicite restano invariate. Visuali ritagliate; PYROMANIA rivaluta coperture e visuale a ogni tick.

Nessun test, compilazione o build eseguito per questo aggiornamento. I report e le descrizioni storiche sotto precedono questa regola; le suite con aspettative sugli spicchi richiedono adeguamento prima di rieseguirle. GRANATA/MOLOTOV sono aggiornate nel GDD: il prototipo attuale offre gestione SLOT/consumo, non ancora i relativi effetti di combattimento.

## Prova in Unity

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`, oppure avviare una RUN dall’HUB. La RUN resta con un solo PG e il roster PG01–PG08 già disponibile.

Le KILL assegnano EXP: al LEVEL UP il gameplay si ferma e mostra tre BANNER distinti. Scegliere un BANNER per acquisire o potenziare un’ABILITÀ BONUS. Ogni nuova abilità occupa uno dei tre SLOT BONUS; gli UPGRADE non occupano altri SLOT. Non esiste un comando di attivazione manuale delle ABILITÀ BONUS.

A fine AREA sono utilizzabili anche i due BANNER ABILITÀ BONUS, insieme ai tre BANNER STATS. Selezionare una delle cinque proposte e confermare. Nei casi di UPGRADE senza candidati validi si propone una STAT secondo i pesi ufficiali e senza duplicare le STATS già presenti.

L’HUD provvisorio mostra le abilità possedute, il loro stato/CD e EXP/LVL. Il catalogo è configurabile tramite il campo Bonus Catalog di `PreGameplayLoop.asset`, collegato a `Assets/TestEngine/Data/BonusCatalog.asset`.

## Effetti integrati

- LANCIO COLTELLI: proiettili fisici a 20 m/s, separazione di 5° e simmetria rispetto al cursore per qualsiasi numero.
- PET: MOVE SPD 150, navigazione attorno ai blocchi, posizione a PG fermo entro raggio 3 m, ricerca dal PET entro 5 m e vincolo di allontanamento dal PG di 5 m anche sul percorso. Nessun HP; collisione con MOB, PET e geometria, nessuna collisione con PG/proiettili.
- AURA TOSSICA: HIT istantanea circolare, otto SPICCHI con blocco geometrico.
- RICOCHET: un solo sorteggio iniziale al 40% per HIT BASE valida, poi rimbalzi automatici sul più vicino entro 5 m dall’ultimo bersaglio. Può tornare su un bersaglio precedente, mai due volte consecutive sullo stesso. Danno dalla HIT originale prima della DEF, mitigazione separata per ogni bersaglio.
- MINE: lancio automatico a 1 m dietro al PG rispetto al cursore, ricerca del punto libero più vicino quando occupato, rinvio se nessun punto è valido; TRIGGER raggio 1 m, esplosione al contatto o dopo 5 s. CD dal lancio effettivo.
- SCUDO: mitigazione separata prima della DEF del PG, consumo alla prima HIT, un solo arrotondamento finale. Resta attivo al cambio AREA; termina in DOWN/MORTE.
- FIRE BULLET: +15% applicato esclusivamente alle HIT degli ATTACCHI BASE, BRUCIATURA fino a cinque istanze con durata rinnovata e tick ogni secondo. Non modifica le STATS persistenti né i danni delle altre abilità. CD dalla fine dell’effetto.
- TASER: danno e STUN con rinnovo, sei SPICCHI.
- REPULSE: danno e respinta di 3 m in 0,25 s, otto SPICCHI, arresto ai blocchi.
- SCIABOLATA: semicerchio frontale orientato al cursore, quattro SPICCHI e una HIT per bersaglio.

I CD BONUS partono all’acquisizione e non usano CD REDUCTION del PG. La riduzione CD si ferma al 90%; DEF SCUDO al 90%. Il relativo UPGRADE viene escluso al CAP. Incrementi sempre sul valore originale del parametro. I due UPGRADE DANNO di FIRE BULLET agiscono entrambi sulla BRUCIATURA (+10% e +50% del valore originale).

Inventario, conteggi UPGRADE, EXP/LVL e CD in corso sono mantenuti al cambio AREA. Gli effetti legati all’AREA vengono rimossi; gli effetti attivi terminati riavviano il loro CD, con l’eccezione di SCUDO. Una nuova RUN azzera questa progressione. EXP DROP aumenta secondo il GDD al cambio AREA; dopo la tabella LVL 1–11 continua la formula ufficiale.

## Verifica e limiti del prototipo

Menu `ROG ZOMBIE > Pre gameplay loop > Run BONUS ability tests`. La suite usa il Play Mode e fixture isolate, poi verifica il cambio AREA e il reset della RUN. Ripristina la selezione PG e la scena precedente; non salva modifiche di test negli asset.

Grafica, dimensioni dei collider dei COLTELLI (raggio 0,06 m) e PET (ActorRadius del prototipo, attualmente 0,35 m) sono riferimenti tecnici provvisori. La ricerca del punto MINA usa i bordi dei collider rettangolari, incluse intersezioni e rotazioni, con tolleranza di separazione di 0,002 m. Il posizionamento iniziale dei PET usa campioni nel raggio 3 m e riprova se occupato. Non è una UI definitiva né una verifica del multiplayer.

Nessuna nuova build esportata per questa integrazione. Gli esiti effettivi sono riportati nei file di validazione della sessione.

Il layer TRIGGER_MOB, già previsto dal GDD ma assente nei Project Settings, è stato aggiunto nello slot libero 16 per le MINE. Nessun layer esistente è stato spostato.

## Esiti del 14/09/2026

- `BONUS-validation-r10.txt`: PASS, 491 controlli in Unity 6000.3.16f1, zero warning/errori in Play Mode. Include tutte le dieci ABILITÀ BONUS, HIT BASE reali PG01–PG08, distinzione da danni fissi/secondari, CAP, BANNER, BRUCIATURA, MINE senza posizione valida, SCUDO, EXP multi-livello, passaggio AREA e reset RUN.
- `BONUS-loop-regression-r1.txt`: PASS, 251 asserzioni. Due cicli da 100 e 120 ZOMB01, FIRST SPAWN, ricambio, geometria ATTACCO BASE PG01, uscita, pausa/scelta, persistenza, cura al cambio AREA e sconfitta. Gli effetti BONUS sono disabilitati dal verificatore per isolare la regressione del loop; i LEVEL UP vengono completati dal verificatore.
- `BONUS-PG03-regression-r3.txt`: PASS, 320 controlli, zero warning/errori in Play Mode. Quattro combinazioni ABILITÀ/PASSIVA, proiettili reali, perforazioni, MARCHIO, COLPO LASER, TRIPLO SPARO e transizioni. La fixture rimuove i palazzi CITY e ricostruisce la NavMesh; i blocchi geometrici della suite restano quelli esplicitamente creati dalle prove.
- Compilazione runtime e codice Editor completata senza errori. Due warning CS0252 già presenti in `Editor/PG05Validation.cs`, righe 317 e 320, sui confronti fra riferimenti. Nessun nuovo warning del codice BONUS.
- Verificate le immagini delle schermate LEVEL UP e FINE AREA acquisite in Unity. Immagini locali in `Builds/BONUS-level-up.png` e `Builds/BONUS-end-area.png`.
- I tentativi intermedi sono conservati in `Builds/BonusValidation/`: comprendono le correzioni del verificatore, il layer MINE mancante, l’inizializzazione del percorso PET e la precisione del moltiplicatore EXP. I file PASS sopra riportati sono gli esiti finali delle rispettive suite.

Le prove non certificano il bilanciamento, la leggibilità a tutte le risoluzioni o la navigazione di gruppi numerosi di PET in ogni possibile strettoia. Il test resta single player; nessuna nuova build e nessun commit/push.

## File dell’integrazione

- Nuovi runtime: `Assets/PreGameplayLoop/BonusAbilityRuntime.cs`, `BonusBurn.cs`, `BonusMine.cs`, `BonusPet.cs`, con nuovi `.meta`.
- Loop e HUD: `Assets/PreGameplayLoop/LoopDefinition.cs`, `PreGameplayLoop.asset`, `LoopSession.cs`, `LoopHUD.cs`, `AreaExitTrigger.cs`.
- Inventario e progressione: `Assets/TestEngine/BonusInventory.cs`, `Data/BonusCatalog.asset`, `ExperienceProgression.cs`, `PlayerRuntime.cs`.
- HIT BASE e presentazione: `Assets/TestEngine/Combatant.cs`, `CombatAttacks.cs`, `PlayerWeapon.cs`, `Projectile.cs`, `TestVisuals.cs`; `Assets/PreGameplayLoop/PG03PassiveRuntime.cs`, `PG04AbilityRuntime.cs`, `PG04ExplosionEffect.cs`, `PG05AbilityRuntime.cs`, `PG06AbilityRuntime.cs`, `PG07AbilityRuntime.cs`, `PG08AbilityRuntime.cs`.
- Validazione: nuovo `Assets/PreGameplayLoop/Editor/BonusValidation.cs` e `.meta`; aggiornati `Editor/LoopValidation.cs` e `Editor/PG03Validation.cs`.
- Configurazione necessaria alle MINE: `ProjectSettings/TagManager.asset`, aggiunto soltanto TRIGGER_MOB.
- Documentazione: questo documento, richiamo in `Docs/PreGameplayLoopPrototype.md` e i tre rapporti PASS elencati sopra.

L’elenco riguarda questa integrazione: le altre modifiche già presenti nel repository sono state conservate. Il GDD non è stato modificato durante l’implementazione.
