# PG05 nel gameplay loop prototype

## Selezione e comandi

Aprire Assets/Scenes/PreGameplayLoopPrototype.unity. In Assets/PreGameplayLoop/PreGameplayLoop.asset scegliere Selected Player PG05, Selected PG05 Ability (Invisibilita / ColtelliAvvelenati) e Selected PG05 Passive (Ghosting / LamaDiCicuta). WASD movimento, mouse mira, LMB attacco, Q abilità. Una nuova RUN acquisisce la configurazione; le selezioni persistono fra AREE. HUD provvisorio con CD, invisibilità, velocità e contatore CICUTA.

## Regole implementate

HP100, ATK30, DEF−10%, MOVE SPD130 (2,6 m/s), ATK SPD150 (1,5/s), RANGE2,5 m. Attacco a cono135° istantaneo, 30 danni per MOB prima della DEF. MURI/OSTACOLI schermano i bersagli. LAMA DI CICUTA conta ogni MOB colpito fino a15; l'attacco successivo applica VELENO a tutti i bersagli validi e azzera il contatore. Contatore conservato al CAMBIO AREA e azzerato alla nuova RUN.

INVISIBILITÀ: PARTY attiva, durata3,5 s, MOVE SPD+15%, CD15 s dall'attivazione. I MOB non selezionano PG invisibili; attacchi già avviati proseguono. Attacco e abilità interrompono l'invisibilità individuale. Movimento non la interrompe. GHOSTING: HP strettamente sotto35% DOPO il danno della HIT (inclusa la HIT che attraversa la soglia), invisibilità e MOVE+15% per2 s; HIT durante GHOSTING non rinnova la durata, nessun CD interno. Sovrapposizione con INVISIBILITÀ: bonus unico, durata dell'ultimo effetto applicato. Modificatore separato dalle STATS persistenti, pausa sospende i timer, CAMBIO AREA termina l'invisibilità.

COLTELLI AVVELENATI: 8 proiettili simultanei a45°, uno diretto al cursore; danno fisso30, RANGE10 m, non perforanti, CD10 s all'attivazione. VELENO5 HP/s per3 s: primo tick dopo1 s, poi tick a2 e3; fino a5 istanze con durata e clock rinnovati a ogni applicazione, anche al cap. DEF e arrotondamento applicati al danno finale; attribuzione KILL al PG sorgente.

## File

- Assets/PreGameplayLoop: PG05AbilityCatalog.cs, PG05Abilities.asset, PG05AbilityRuntime.cs, PG05AbilityInput.cs, PG05Invisibility.cs, PG05Poison.cs, PG05KnifeHit.cs, Editor/PG05Validation.cs.
- LoopDefinition.cs, LoopSession.cs, LoopHUD.cs, PreGameplayLoop.asset: configurazione e integrazione.
- Assets/TestEngine: Combatant.cs, PlayerWeapon.cs, MobBrain.cs; Assets/PlayerMovement.cs: stato invisibile, notifica azioni, targeting e movimento temporaneo.
- Docs/ROG_ZOMBIE_GDD.md: tre chiarimenti del proprietario; questo documento e PreGameplayLoopPrototype.md.

Nuovi .meta inclusi; preesistenti preservati. La selezione PG04/PYROMANIA già presente è conservata nel suo campo dedicato. PG05 è selezionato per la nuova prova; nessuna modifica PG06. Nessun commit/push.

## Verifica e limiti

Menu ROG ZOMBIE → Pre gameplay loop → Run PG05 tests. Richiede una sola scena salvata. Quattro combinazioni; altri PG non istanziati. Bersagli alleati tecnici per la PARTY, IA/input disabilitati durante le verifiche controllate. Nessuna prova PG06 o di altri PG.

Il prototipo non comprende ancora uso ITEMS, interazioni generiche o rianimazione; Combatant.NotifyAction espone il collegamento per le azioni che devono interrompere l'effetto. Nessuna simulazione di queste funzioni mancanti. Restano visuali provvisorie, assenza di multiplayer/PG IA e valutazione manuale del feeling. Velocità20 m/s e raggio collider0,06 m dei coltelli riusano la configurazione tecnica dell'arma.

Validazione effettiva: PG05-validation-r1.txt, Unity6000.3.16f1, PASS, 318 controlli, 869 warning registrati, nessun errore/exception. Compilazione runtime/editor riuscita. Warning Sprite Tiling / Full Rect osservato nelle visuali provvisorie. Verificate le quattro combinazioni e il ciclo AREA/reset RUN solo con PG05; i risultati non costituiscono verifica del feeling manuale.

## Aggiornamento RANGE 2,5 m e GHOSTING 35% dopo HIT

RANGE interno PG05 aumentato a 250 (2,5 m). GHOSTING valuta HP <35% degli HP MASSIMI correnti dopo il danno, includendo la HIT che attraversa la soglia. Il controllo usa l’evento DamageApplied già esistente, filtrato su PG05; una HIT letale non attiva invisibilità. Durata, BONUS velocità e regole di rinnovo invariati. Aggiornate le aspettative dei test esistenti senza eseguirli. Nessun test o build effettuato su richiesta; i precedenti risultati rimangono storici.
