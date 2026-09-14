# PG04 nel gameplay loop prototype

## Correzione PYROMANIA e regressione in Unity — 14/09/2026

L’eccezione `MissingComponentException` del renderer dell’esplosione interrompeva `EmitExplosion` prima della creazione di PYROMANIA. In `PG04AreaVisual` il recupero/aggiunta di MeshFilter e MeshRenderer usa ora il confronto null di Unity, conservando colori, UV e texture espliciti della mesh. Danni, durata e schermatura non cambiano.

Verificatore dedicato: `ROG ZOMBIE → Pre gameplay loop → Run PG04 PYROMANIA regression test`, file `Assets/PreGameplayLoop/Editor/PG04PyromaniaValidation.cs`. Usa solo PG04 e ripristina scena e selezione al termine. Verifica attacchi reali, 25 danni base + 5 immediati di PYROMANIA, tick successivi a 1 e 2 secondi, scadenza a 3 secondi, coperture, sovrapposizione, COLPO GROSSO, tutte le 10 esplosioni di PIOGGIA DI GRANATE e reset RUN. La richiesta da file importa gli script aggiornati prima dell’avvio. Lo screenshot `Builds/PG04-pyromania-visible.png` viene acquisito al secondo tick, dopo il lampo iniziale.

Esito effettivo: `PG04-pyromania-validation-r2.txt`, PASS, Unity 6000.3.16f1, 36 controlli, zero errori/warning durante la prova. Screenshot del secondo tick ispezionato: area rossa persistente e numero di danno 5 visibili. Compilazione runtime/Editor riuscita; due warning CS0252 preesistenti in PG05Validation.cs, senza eseguire test PG05. Anche la prima esecuzione (`r1`) ha superato 36 controlli.

I vecchi report sugli SPICCHI non verificano questa versione. Nessuna build esportata.

## Aggiornamento schermatura AREE — 14/09/2026

MURI/OSTACOLI schermano solo i bersagli con linea interrotta fra centro dell’AREA e centro del collider; i bersagli esposti entro la forma e il RAGGIO previsti ricevono la HIT completa prima della DEF, senza propagazione attorno agli angoli. La regola sostituisce gli SPICCHI anche per COLPO GROSSO, PIOGGIA DI GRANATE, PYROMANIA, LUCKY SHOT, ABILITÀ BONUS ad area e attacchi ad area dei MOB. SCIABOLATA resta un SEMICERCHIO. Le SEZIONI di FILO SPINATO/TRAPPOLA e le eccezioni esplicite restano invariate. Visuali ritagliate; PYROMANIA rivaluta coperture e visuale a ogni tick.

Nessun test, compilazione o build eseguito per questo aggiornamento. I report e le descrizioni storiche sotto precedono questa regola; le suite con aspettative sugli spicchi richiedono adeguamento prima di rieseguirle. GRANATA/MOLOTOV sono aggiornate nel GDD: il prototipo attuale offre gestione SLOT/consumo, non ancora i relativi effetti di combattimento.

## Selezione e prova

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`. In `Assets/PreGameplayLoop/PreGameplayLoop.asset` scegliere Selected Player = PG04, Selected PG04 Ability (PioggiaDiGranate / ColpoGrosso) e Selected PG04 Passive (ScortaEsplosiva / Pyromania). Una nuova RUN acquisisce le selezioni, mantenute fra AREE. WASD movimento, mouse mira, LMB attacco, Q abilità. HUD provvisorio con scelta, cooldown, durata/cariche e passiva. Riprova test azzera la RUN.

PG04 usa HP 100, ATK 25, DEF 100 interna, MOVE SPD 100, ATK SPD 65 e RANGE 700 (7 m). Destinazione fissata al lancio al cursore entro 7 m dal PG, HIT dopo 0,3 s, anticipato solo da MURO. L’ATTACCO BASE ordinario ha raggio 1,25 m e verifica la linea fra centro esplosione e centro collider MOB: danno completo prima della DEF se libero, nessuna HIT se schermato da MURO/OSTACOLO. PIOGGIA DI GRANATE, COLPO GROSSO e PYROMANIA mantengono i quattro spicchi da 90°.

## Verifica rapida della schermatura ATTACCO BASE — 14/09/2026

Menu `ROG ZOMBIE → Pre gameplay loop → Run PG04 base occlusion quick test`. La prova seleziona esclusivamente PG04, disabilita IA/input e ripristina scena e selezione precedenti. Report `PG04-occlusion-validation-r1.txt`: PASS, Unity 6000.3.16f1, 25 controlli, zero warning/errori durante il test. Verificati lancio reale, delay 0,3 s, MOB esposti/schermati nello stesso quadrante, limite raggio, centro collider, DEF, singola HIT per bersaglio, ostacoli ruotati, impatto sul bordo MURO, mesh ritagliata e COLPO GROSSO ancora a spicchi. Compilazione runtime/Editor riuscita; restano due warning CS0252 preesistenti in PG05Validation.cs, senza eseguire test PG05. Nessuna build esportata.

Il riferimento grafico dell’esplosione base è ritagliato tramite raggi angolari e raggi aggiuntivi ai vertici dei blocchi; la curva è approssimata graficamente. Il danno usa la verifica diretta per ciascun MOB, indipendente dal campionamento grafico. Geometria supportata: BoxCollider2D dei MURI/OSTACOLI del prototipo, anche ruotati. Non effettuata una valutazione manuale di feeling o bilanciamento. I report storici sotto si riferiscono alle rispettive versioni.

PIOGGIA DI GRANATE: tenere Q mostra l’anteprima di raggio 4 m sul cursore; al rilascio partono CD e sequenza di 10 esplosioni casuali in 3 s, entro 4 m dal cursore iniziale senza limite di distanza dal PG; CD 12 s dall'attivazione. Eredita ATK corrente e raggio dell'attacco base. COLPO GROSSO: quattro attacchi con ATK +80% e raggio +50%; anche MISS consuma cariche. Nessuna scadenza; CD 10 s dal quarto attacco. Le STATS persistenti restano indipendenti dal potenziamento. CAMBIO AREA termina effetti attivi e ne riavvia il CD; conserva CD residuo se già inattivi e disponibilità se pronti.

PYROMANIA: ogni esplosione crea un'area incendiata di pari geometria, durata 3 s. Chiarimento del proprietario: primo tick immediato all'attivazione, poi ogni secondo sul clock dell'area, indipendente dall'ingresso dei MOB. Tick a t=0,1,2; scadenza a t=3. Ogni tick applica 5 danni prima della DEF e dell'arrotondamento finale. Le aree sovrapposte applicano danni indipendenti. Pausa sospende i timer; CAMBIO AREA e reset rimuovono le aree.

SCORTA ESPLOSIVA: fino a 3 GRANATE o MOLOTOV dello stesso tipo per SLOT, contemporaneamente per entrambi i tipi; altri ITEMS massimo 1. PG04 Test Item Slots configura 1–4 SLOT tecnici, default 1. Il pannello temporaneo carica GRANATA/MOLOTOV e consuma una unità senza lanciare ITEMS. Inventario conservato fra AREE, vuoto alla nuova RUN. Acquisto, lancio, effetti ITEMS e progressione SLOT non sono implementati.

## File dell'integrazione PG04

- `Assets/PreGameplayLoop/PG04AbilityCatalog.cs`, `PG04Abilities.asset`: valori e scelte.
- `PG04AbilityRuntime.cs`, `PG04AbilityInput.cs`: attacco, abilità e comando Q.
- `PG04ExplosionEffect.cs`, `PG04AreaVisual.cs`, `PG04BurningArea.cs`: danno e visuali provvisorie.
- `PG04ItemSlots.cs`: capacità e consumo tecnico della SCORTA.
- `LoopDefinition.cs`, `LoopSession.cs`, `LoopHUD.cs`, `PreGameplayLoop.asset`: selezione, lifecycle e HUD.
- `Assets/TestEngine/PlayerWeapon.cs`: collegamento dell'attacco PG04.
- `Assets/PlayerData/PG04.asset`, `Assets/TestEngine/Data/PG04_Weapon.asset`: RANGE 7 m e delay HIT 0,3 s.
- `Assets/PreGameplayLoop/Editor/PG04Validation.cs`: test esclusivi PG04.
- Questo documento e `Docs/PreGameplayLoopPrototype.md`.

I nuovi asset/script hanno il proprio .meta; quelli esistenti sono preservati. GDD aggiornato per la successiva modifica richiesta di ATK 25, ATK SPD 65 e raggio PIOGGIA DI GRANATE 5 m.

## Verifica

Menu ROG ZOMBIE → Pre gameplay loop → Run PG04 tests. Richiede una sola scena salvata, esegue le quattro combinazioni in Play Mode e ripristina scena e selezione. Disabilita IA/input durante le prove automatiche; non misura bilanciamento o feeling dei comandi. Verifica danni reali, geometria, cadenza, tick immediato/periodico, ingresso/uscita, sovrapposizioni, pause, cariche, bonus persistenti, SLOT, CAMBIO AREA, DOWN e reset RUN. Non esegue suite di altri PG.

Esito effettivo aggiornato (ATK 25, ATK SPD 65, raggio base 1,25 m, PIOGGIA DI GRANATE raggio 5 m): `PG04-spd65-radius125-validation.txt`, Unity 6000.3.16f1, PASS, 422 controlli, 821 warning durante la suite, nessun errore/exception registrato. Compilate Assembly-CSharp e Assembly-CSharp-Editor. Console verificata dopo la suite: zero errori; warning ripetuto Sprite Tiling / sprite non Full Rect nelle visuali provvisorie esistenti. Il contatore Console cumulativo include anche messaggi fuori dalla suite. `git diff --check` superato; Git segnala conversioni LF/CRLF su file già modificati. Non eseguite suite PG01/PG02/PG03. Restano da valutare manualmente feeling, bilanciamento e leggibilità in partita; il pannello tecnico a quattro SLOT richiede spazio orizzontale sufficiente.

Aggiornamento richiesto: ATK SPD 65; raggio base e singole esplosioni PIOGGIA DI GRANATE 1,25 m; COLPO GROSSO 1,875 m. Verifica Unity completata: 422 controlli superati, solo PG04, nessun errore, 821 warning registrati.

## Delay ATTACCO BASE 0,3 s

La HIT e l’esplosione avvengono 0,3 s dopo il lancio. Posizione, ATK e raggio sono acquisiti al lancio; bersagli e spicchi validi vengono valutati alla HIT. Vale anche per COLPO GROSSO; cariche e avvio del CD restano legati al lancio. PYROMANIA nasce all’esplosione e applica subito il primo tick. PIOGGIA DI GRANATE conserva la sua temporizzazione. Pausa sospende il ritardo; CAMBIO AREA e reset rimuovono le HIT pendenti.

Nessun test eseguito per questo aggiornamento, come richiesto. I report precedenti riguardano la versione senza delay; la suite PG04 contiene ancora verifiche di impatto istantaneo e richiede adeguamento prima della prossima esecuzione.

## Raggio PIOGGIA DI GRANATE 4 m

Ridotto il raggio di distribuzione da 5 a 4 m in asset, valore predefinito e descrizione HUB/GDD. Il raggio delle singole esplosioni resta 1,25 m. Verificata la coerenza dei dati e il collegamento runtime a RainRadius; nessuna nuova prova Unity o esportazione della build. I risultati precedenti restano riferiti ai valori indicati nei rispettivi report.

## PIOGGIA DI GRANATE — attivazione a rilascio

Input Q come PIOGGIA DI FRECCE: pressione avvia la mira solo ad ABILITÀ pronta, mantenimento aggiorna l’anteprima sul cursore, rilascio attiva. Anteprima puramente visiva senza collider o HIT: mostra la zona di distribuzione di raggio 4 m; le singole esplosioni conservano la propria geometria. Perdita focus, disabilitazione, stato non giocabile e CAMBIO AREA annullano la mira senza attivare. COLPO GROSSO resta a pressione. Aggiornati suggerimento HUD e descrizione HUB/GDD; preservate le modifiche precedenti. Nessun test o build eseguito su richiesta.

## Riferimento grafico AREA dopo il lancio

PIOGGIA DI GRANATE mostra un disco trasparente di raggio 4 m nel punto scelto all’attivazione, per tutta la sequenza di 3 s. Il riferimento resta fisso, non segue il cursore, e mostra l’area di distribuzione degli impatti. È solo grafico, senza collider o danno; termina con la sequenza e viene rimosso su DOWN, CAMBIO AREA, disabilitazione o distruzione. In pausa resta visibile con la sequenza sospesa. Nessun test Unity o build eseguito; verifica visiva ancora da effettuare.
