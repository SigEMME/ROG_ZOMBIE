# PG07 nel gameplay loop prototype

## Aggiornamento schermatura AREE — 14/09/2026

MURI/OSTACOLI schermano solo i bersagli con linea interrotta fra centro dell’AREA e centro del collider; i bersagli esposti entro la forma e il RAGGIO previsti ricevono la HIT completa prima della DEF, senza propagazione attorno agli angoli. La regola sostituisce gli SPICCHI anche per COLPO GROSSO, PIOGGIA DI GRANATE, PYROMANIA, LUCKY SHOT, ABILITÀ BONUS ad area e attacchi ad area dei MOB. SCIABOLATA resta un SEMICERCHIO. Le SEZIONI di FILO SPINATO/TRAPPOLA e le eccezioni esplicite restano invariate. Visuali ritagliate; PYROMANIA rivaluta coperture e visuale a ogni tick.

Nessun test, compilazione o build eseguito per questo aggiornamento. I report e le descrizioni storiche sotto precedono questa regola; le suite con aspettative sugli spicchi richiedono adeguamento prima di rieseguirle. GRANATA/MOLOTOV sono aggiornate nel GDD: il prototipo attuale offre gestione SLOT/consumo, non ancora i relativi effetti di combattimento.

## Avvio

Aprire Assets/Scenes/PreGameplayLoopPrototype.unity. In Assets/PreGameplayLoop/PreGameplayLoop.asset selezionare PG07, Selected PG07 Ability (MultiShot / PioggiaDiFrecce), Selected PG07 Passive (LuckyShot / Concentrazione). WASD movimento, mouse mira, LMB attacco base, Q abilità; per PIOGGIA DI FRECCE tenere Q per l’anteprima e rilasciare per attivare. Nuova RUN acquisisce selezioni, conservate al CAMBIO AREA. HUD temporaneo con CD, frecce pioggia e contatori diagnostici di roll/attivazioni.

HP100, ATK30, DEF0%, MOVE SPD110, ATK SPD110 (1,1/s), RANGE15 m. Freccia fisica standard20 m/s, non perforante di base. Collider proiettile0,06 m riusato dall'asset tecnico. Nessun cambiamento ad asset PG07/arma.

## Abilità e passive

MULTI SHOT:9 proiettili simultanei, angoli−40°...+40° ogni10°, centro al cursore; danno20 prima della DEF, RANGE7 m, velocità20 m/s. CD13 s all'attivazione. Ogni proiettile applica una HIT indipendente. Una sola RESPINTA5 m progressiva in0,25 s per MOB per volley; per HIT simultanee si sommano e normalizzano le direzioni. Tempo d'impatto stimato nel passo di simulazione tramite distanza della sweep/velocità; aggregazione in LateUpdate con tolleranza numerica0,00001 s. MURI/OSTACOLI interrompono proiettili e respinta. Riutilizzato lo spostamento con controllo collisioni del prototipo.

PIOGGIA DI FRECCE:20 punti casuali entro3,5 m dal cursore iniziale, attivabile a distanza illimitata. Prima HIT a0, ultima a3 s, intervallo3/19 s. PUNTO d'impatto contro il collider dei MOB: zero bersagli MISS, sovrapposti scelta casuale singola, danno10 per HIT prima della DEF. Ignora MURI/OSTACOLI; caduta rappresentata da un segno verticale provvisorio sopra l'impatto, nessuna velocità fisica. CD15 s all'attivazione. Pausa sospende la sequenza; CAMBIO AREA annulla i punti futuri e riavvia il CD se attiva.

LUCKY SHOT: roll5% solo su HIT valida di attacco base, anche letale. HIT aggiuntiva raggio2 m, danno30% ATK corrente, arrotondato matematicamente, poi DEF e arrotondamento finale per ciascun bersaglio. Quattro spicchi90°, spicchio intercettato da MURO/OSTACOLO interamente rimosso. Nessuna catena di roll.

CONCENTRAZIONE: roll10% al lancio, anche MISS; successo rende perforante per massimo3 MOB, danni100%/50%/50%, metà danno arrotondata prima della DEF. Gli effetti aggiuntivi della HIT sono delegati per ciascun bersaglio con il danno relativo. RANGE e blocco da MURI/OSTACOLI invariati. Nessuna passiva estesa alle abilità. Le STATS persistenti non vengono riscritte.

## File

Assets/PreGameplayLoop: PG07AbilityCatalog.cs, PG07Abilities.asset, PG07AbilityInput.cs, PG07AbilityRuntime.cs, PG07MultiVolley.cs, Editor/PG07Validation.cs e .meta. LoopDefinition.cs, LoopSession.cs, LoopHUD.cs, PreGameplayLoop.asset per la selezione e lifecycle. Assets/TestEngine/Projectile.cs espone il tempo d'impatto della HIT, senza cambiare il percorso di risoluzione degli altri PG. Riutilizzata PG04AreaVisual come visuale degli spicchi LUCKY SHOT, senza istanziare PG04. Questo documento e PreGameplayLoopPrototype.md.

Modifiche precedenti preservate; nessuna modifica GDD per questa integrazione (valori già aggiornati dal proprietario), nessun commit/push, nessun aggiornamento Unity/pacchetti o modifica manuale delle cartelle generate.

## Test e limiti

Menu ROG ZOMBIE → Pre gameplay loop → Run PG07 tests. Quattro combinazioni in Play Mode, solo PG07; alleati tecnici con STATS PG07, IA/input disabilitati durante controlli deterministici. Include probabilità con seed controllati, HIT/MISS, danni/DEF, perforazione ed effetti aggiuntivi, spicchi, nove proiettili e respinta simultanea, pioggia regolare/punti/sovrapposizione, pausa, AREA, reset. I seed verificano i rami di successo/fallimento, non stimano statisticamente la frequenza a lungo termine.

Visuali provvisorie, un solo PG controllabile, multiplayer e PG IA esclusi. HIT programmate eseguite al primo frame utile, con recupero degli eventi scaduti in caso di frame lento. Il feeling e il bilanciamento richiedono prova manuale.

Esito precedente, con LUCKY SHOT raggio3 m: PG07-validation-r1.txt, Unity6000.3.16f1, PASS, 342 controlli sulle quattro combinazioni, 947 warning registrati, nessun errore/exception. Compilazione runtime/editor riuscita. Nessuna suite PG01–PG06 eseguita. Rimane lo spazio finale preesistente di PreGameplayLoop.asset alla chiave m_EditorClassIdentifier, segnalato dal controllo diff.

LUCKY SHOT aggiornato a raggio2 m su richiesta del proprietario. Nessun test eseguito su questa modifica; adeguato il bersaglio esterno della verifica del raggio. Il report precedente non verifica il nuovo valore.

## Aggiornamento respinta e pioggia

MULTI SHOT: respinta progressiva in0,25 s con interpolazione quadratica ease-out, massimo5 m, interrotta da MURI/OSTACOLI. Conservata una respinta per MOB per attivazione e media delle direzioni simultanee. La durata del componente volley comprende anche il completamento della respinta.

PIOGGIA DI FRECCE: ogni impatto crea un cerchio di raggio0,25 m; tutti i MOB con collider intersecante ricevono10 danni prima della DEF, una sola volta per freccia. Sostituisce il precedente bersaglio singolo casuale al punto. Restano invariati raggio di distribuzione3,5 m,20 frecce in3 s, CD e ignorare MURI/OSTACOLI.

Nessun test eseguito su questo aggiornamento. Il report precedente e le verifiche di spostamento immediato/bersaglio singolo della suite si riferiscono al comportamento precedente e richiedono adeguamento prima della prossima esecuzione.

MULTI SHOT: CD BASE aggiornato a13 s, sempre avviato all’attivazione. Nessun test eseguito per questa modifica; i report precedenti non verificano il nuovo CD.

## PIOGGIA DI FRECCE a rilascio

Tenere Q mostra un cerchio ciano di raggio3,5 m sul cursore, senza collider o danno; il rilascio attiva la pioggia nella posizione corrente e avvia il CD. MULTI SHOT resta a pressione singola. Anteprima disponibile solo con abilità pronta; pausa, DOWN, cambio AREA, reset, perdita di focus o disabilitazione del controllo la annullano senza attivare l’abilità. Nessun test eseguito su questa modifica.

## Riferimento grafico AREA dopo il lancio

PIOGGIA DI FRECCE mostra un disco trasparente di raggio 3,5 m nel punto scelto all’attivazione, per tutta la sequenza di 3 s. Il riferimento resta fisso, non segue il cursore, e mostra l’area di distribuzione degli impatti. È solo grafico, senza collider o danno; termina con la sequenza e viene rimosso su DOWN, CAMBIO AREA, disabilitazione o distruzione. In pausa resta visibile con la sequenza sospesa. Nessun test Unity o build eseguito; verifica visiva ancora da effettuare.
