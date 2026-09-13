# PG01 — PASSIVE nel gameplay loop prototype

## Selezione e prova

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`. Nell'asset `Assets/PreGameplayLoop/PreGameplayLoop.asset`, scegliere `Selected Passive`: PASSIVA 1 — DEF +15% oppure PASSIVA 2 — ATK +15%. Il valore iniziale è PASSIVA 1. `Selected Ability` resta indipendente: tutte le combinazioni sono consentite.

Premere Play o `Riprova test` per acquisire la configurazione in una nuova RUN. Cambiare la selezione nell'Inspector durante Play non modifica la RUN corrente. Al cambio AREA la selezione rimane la stessa. Nessun comando attiva la passiva e nessuno SLOT BONUS viene usato.

L'HUD mostra la passiva selezionata, ATTIVA/INATTIVA, ATK effettivo e DEF percentuale effettiva. La condizione è HP strettamente inferiori al 50% degli HP MASSIMI correnti. A 50/100 è inattiva; a 49/100 è attiva. Cure, modifiche agli HP massimi e BONUS rivalutano immediatamente la condizione. In DOWN l'effetto è inattivo.

## Calcolo e separazione dei valori

- PASSIVA 1: +15 punti alla DEF interna corrente, fino a 190 (90% mitigazione). Chiarimento esplicito del proprietario: 115 diventa 130. La parte eccedente il cap non viene sottratta dalle STATS persistenti alla disattivazione.
- PASSIVA 2: ATK persistente corrente × 1,15 mentre attiva. Il BONUS ATK +5% opera sulle STATS persistenti: 20 diventa 21, ATK effettivo attivo 24,15; alla disattivazione rimane 21, come confermato dal proprietario.
- Nessuna scrittura dei contributi temporanei nelle STATS persistenti. Il bonus corrente viene ricalcolato dalla vista persistente senza accumulare precedenti attivazioni. I BONUS acquisiti mentre la passiva è attiva restano dopo una cura o un cambio AREA.
- La HIT che attraversa la soglia usa la DEF precedente alla HIT; la successiva usa il nuovo stato. Il DANNO finale viene arrotondato matematicamente dopo la DEF per le HIT ricevute dal PG01 del loop e il suo ATTACCO BASE. Le altre scene conservano il comportamento precedente.
- Il bonus ATK riguarda l'ATTACCO BASE. PESTONE continua a usare 40 DANNO dal proprio catalogo; BARRIERA non cambia.

`PG01PassiveRuntime` contiene selezione e condizione; `ICombatStatModifier` separa la vista effettiva dal codice delle singole passive. `Combatant.Stats`/`SetStats` restano il percorso persistente dei BONUS; `EffectiveStats` alimenta il calcolo della HIT. `LoopSession` crea una nuova passiva solo per una nuova RUN e conserva l'istanza tra AREE. Nessuna modifica al GDD ufficiale.

## Test in engine

Menu `ROG ZOMBIE → Pre gameplay loop → Run PG01 passive tests`. Richiede una sola scena salvata; crea copie runtime della configurazione e ripristina la scena precedente alla fine. Il report predefinito è `PG01-passives-validation.txt`. La richiesta automatica `.pg01-passives-test.request` contiene il percorso completo del report e va creata dopo la ricompilazione.

Controlli: entrambe le selezioni, indipendenza dall'abilità, soglie 51/50/49%, HIT che attraversa la soglia, attivazioni ripetute, cura, HP massimi e BONUS HP, BONUS ATK/DEF sotto effetto, cap DEF e recupero del valore persistente, HIT effettive in ingresso e uscita con arrotondamento finale, PESTONE invariato, BONUS e cura reali al cambio AREA, DOWN e reset RUN.

Rieseguire anche `Run PG01 ability tests` e `Run engine smoke test`. Queste prove disabilitano l'input e l'IA per controllare i casi; non sono prove di bilanciamento o del feeling manuale. Gli esiti effettivi vanno letti nei report dell'esecuzione.

## Esiti eseguiti — 12/09/2026

Unity 6000.3.16f1 ha ricompilato gli assembly runtime/editor alle 23:16:32/33 e importato i nuovi script con i rispettivi `.meta`. Verificata nell'Inspector la selezione della passiva separata dall'abilità.

- `PG01-passives-validation.txt`, ore 23:17:08: PASS, 135 verifiche, 425 warning.
- `PG01-passives-abilities-regression.txt`, ore 23:18:31: PASS, 160 verifiche, 353 warning.
- `PG01-passives-loop-regression.txt`, ore 23:18:48: PASS, 259 verifiche.

Nessun errore interrompe i test. Persistono gli avvisi Sprite Tiling / Full Rect già presenti nel prototipo. Verificato visivamente l'HUD in una RUN normale: nome della passiva, stato INATTIVA, ATK 20 e DEF 15% a piena salute, e disattivazione in DOWN. Lo stato ATTIVA e i suoi calcoli sono verificati dai test automatici; non è stata svolta una valutazione completa del feeling manuale. L'Editor è stato riportato fuori Play Mode.

File di implementazione modificati/aggiunti: `Assets/PreGameplayLoop/PG01PassiveRuntime.cs`, `Assets/PreGameplayLoop/LoopDefinition.cs`, `Assets/PreGameplayLoop/LoopSession.cs`, `Assets/PreGameplayLoop/LoopHUD.cs`, `Assets/PreGameplayLoop/PreGameplayLoop.asset`, `Assets/TestEngine/ICombatStatModifier.cs`, `Assets/TestEngine/Combatant.cs`, `Assets/TestEngine/CombatAttacks.cs`, `Assets/PreGameplayLoop/Editor/PassiveValidation.cs` (con `.meta` generati dall'Editor per i tre nuovi script). Documentazione: questo file e `Docs/PreGameplayLoopPrototype.md`. I tre report sopra sono aggiunti come evidenza. GDD, versione Unity, pacchetti e impostazioni di progetto invariati; nessun commit/push.
