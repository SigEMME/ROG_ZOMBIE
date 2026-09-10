# TEST ENGINE #2 — guida alla prova

## Tuning confermato post test — versione corrente

Questi valori dell'ultimo allegato prevalgono sui valori storici descritti sotto; il GDD non è stato modificato.

| Dato | Valore corrente |
| --- | --- |
| PG02 ATK SPD | 400 = 4 attacchi/s |
| PG04 RANGE | 600 interno = 6 m |
| PG04 ATK SPD / delay | 75 / 0,5 s, invariati |
| PG05 ATK SPD / RANGE | 150 / 2 m, invariati |
| PG08 ATK SPD | 600 = 6 attacchi/s |
| ZOMB02 MOVE SPD | 120 |
| ZOMB03 MOVE SPD | 90 |
| ZOMB04 PROJECTILE SPD / RANGE | 8 m/s / 2000 interno = 20 m |
| ZOMB05 MOVE SPD | 100 |
| GLOBAL MOVE SPD BASE | 3 m/s, invariato |

KNIFE: la HIT ora rileva l'intersezione del collider circolare del MOB con il semicerchio visualizzato, anziché richiedere che il centro del MOB sia dentro. Include offset e scala del collider; origine, orientamento, raggio e grafica restano invariati. Il controllo degli ostacoli resta attivo. La HIT è istantanea: entrare nell'area dopo lo sparo, durante il flash di 0,15 s, non genera una HIT aggiuntiva.

File aggiornati in questa fase: `Assets/PlayerData/PG02.asset`, `PG04.asset`, `PG08.asset`; `Assets/TestEngine/Data/ZOMB02.asset`, `ZOMB03.asset`, `ZOMB04.asset`, `ZOMB05.asset`; `Assets/TestEngine/AttackGeometry.cs`, `CombatAttacks.cs`, `Editor/TestEngineValidation.cs`; questa guida. Nessun nuovo file di progetto.

Verifiche: compilazione runtime ed Editor riuscita; eseguiti 2160 controlli sulla funzione C# effettiva del KNIFE, con intersezioni/esclusioni al bordo curvo, al diametro e all'angolo, in 360 orientamenti. Verificati i valori degli asset e gli hash dei sistemi da preservare. Nessuna NullReferenceException nel log Unity controllato; non equivale a una nuova verifica Play, non eseguita in questa fase.

In Unity: attendere l'importazione, avviare TestEngine2 e una nuova Riprova; selezionare PG05 e colpire MOB con collider parzialmente dentro il bordo, totalmente fuori, di lato e dietro, anche ruotando la mira. I bersagli schermati dagli ostacoli restano esclusi. Verificare PG02 a 4 attacchi/s, PG08 a 6, PG04 a 6 m con delay 0,5 s e ZOMB04 con proiettili a 8 m/s e RANGE 20 m. L'Inspector conserva i controlli esistenti.

Discrepanza fuori scope preservata: il GDD descrive KNIFE come cono a 135°, mentre il test attuale visualizza un semicerchio. L'ultima richiesta impone di mantenere la forma attuale: non è stata convertita a cono. Nessuna modifica alla logica AI; le condizioni che già dipendono dalla STAT RANGE di ZOMB04 leggono ora il valore 20 m. Nessun commit.

## Correzioni per il prossimo test

Selezionare `TEST ENGINE 2` nella Hierarchy: il suo Inspector raggruppa GLOBAL MOVE SPD BASE (3 m/s), ATK SPD dei singoli PG, ZOMB04 PROJECTILE SPD (15 m/s), PG04 GRENADE HIT DELAY (0,5 s), FIRST SPAWN %, TOTAL MOB AREA, MAX MOB ACTIVE e FORCE TEST CHEST NEAR PLAYER.

- Velocità globale, ATK SPD del PG attivo e velocità dei prossimi proiettili possono essere provati durante Play. L'ATK SPD del PG attivo cambia soltanto la copia runtime. Gli altri PG e i parametri condivisi modificano asset e possono persistere dopo Play.
- Popolazione e toggle CHEST si applicano alla prossima Riprova. MAX MOB ACTIVE è derivato dal FIRST SPAWN, non un secondo limite indipendente.
- Show Damage Numbers e Show Spawn Counters disattivano i nuovi elementi di debug. I numeri mostrano gli HP effettivamente sottratti, compreso il limite degli HP rimasti in caso di overkill.
- PG04 memorizza punto, danno e raggio allo sparo; dopo il delay colpisce i MOB presenti in quel punto. Muovere il mouse o il bersaglio non sposta l'esplosione.
- I MOB usano steering locale, scorrimento tangenziale e separazione graduale; il componente MobBrain mantiene le proprie decisioni. Spacing e Separation Speed sono parametri tecnici di prova, non nuove STATS. La fluidità nelle strettoie richiede verifica visiva.
- Lo spawn accetta una sola inizializzazione e una sola notifica per MOB morto; riserva una sostituzione per morte fino al totale. I contatori iniziali con i dati salvati sono 99/99 attivi, 99/330 spawnati, 0 morti.
- In pausa i proiettili restano le stesse istanze e non elaborano movimento, collisioni o consumo del RANGE. MEDI KIT, mira e generazione BANNER non sono stati modificati in questa correzione.

File nuovi di questa correzione (con `.meta`): `MobSeparation.cs`, `DamageNumbersDebug.cs`, `Editor/TestTuningInspector.cs`, in `Assets/TestEngine/`.

File aggiornati: `Assets/PlayerMovement.cs`; in `Assets/TestEngine/`: `TestAreaSettings.cs`, `WeaponDefinition.cs`, `PlayerWeapon.cs`, `Projectile.cs`, `Combatant.cs`, `SpawnManager.cs`, `TestEngineBootstrap.cs`, `TestHUD.cs`, `Editor/TestEngineValidation.cs`, `Data/TestAreaSettings.asset`, `Data/PG04_Weapon.asset`; questa guida.

Verifica della correzione: compilazione C# runtime ed Editor con librerie Unity riuscita. I tentativi di verifica nell'Editor hanno rilevato valori caricati diversi dagli asset su disco (MOVE SPD e RANGE PG05 1,5 invece di 2 m); nessun valore aperto è stato sovrascritto. Il nuovo smoke test Play non risulta completato. Salvare consapevolmente il proprio tuning, fermare Play e avviare il menu di test con la configurazione prevista; i controlli delle STATS originali richiedono i dati di riferimento. Gli esiti storici in fondo si riferiscono alla fase precedente.

Prova manuale: verificare WASD e diagonali a base 3; variare ATK SPD; sparare PG04 e spostare un MOB dentro/fuori dall'impatto durante i 0,5 s; osservare l'orda presso gli ostacoli; uccidere un MOB e verificare una sola sostituzione; raggiungere LVL UP con proiettili in volo e verificarne la ripresa; raccogliere la CHEST vicina e un MEDI KIT con HP mancanti. Nessun commit automatico.

Scena: `Assets/Scenes/TestEngine2.unity`.
Aprire direttamente la scena e premere Play. Attendere il caricamento della NavMesh, del FIRST SPAWN e degli oggetti.

## Comandi e verifiche

- WASD: movimento indipendente dalla mira. Nel prossimo test MOVE SPD 100 = 3 m/s: (MOVE SPD / 100) × GLOBAL MOVE SPD BASE, diagonali normalizzate. SampleScene conserva la conversione originale se non assegni TestSettings.
- Mouse: mira. LMB singolo o mantenuto: ATTACCO BASE secondo ATK SPD. Nessun reload.
- Pulsanti PG01–PG08: riavviano la prova con quel PG. Non sono PREPARAZIONE RUN.
- Riprova: ricostruisce la stessa AREA. Nessuna transizione fra AREE.
- PG DOWN: termina la prova per il PG singolo; utilizzare Riprova. Non è implementata una regola nuova per la scadenza del DOWN.
- CHEST gialla: raccolta automatica, avviso senza pausa. MEDI KIT verde acqua: cura automatica solo con HP mancanti.
- LVL UP: pausa e tre BANNER distinti. Ogni scelta aggiorna inventario/upgrade prima della scelta successiva.

La UI non espone la STAT RANGE del PG né PROJECTILE SPD. Il testo degli UPGRADE BONUS può nominare RANGE come richiesto dalla specifica dei BANNER.

## Dati configurabili

`Assets/PlayerData/PG01.asset`–`PG08.asset`: STATS BASE originali; RANGE PG03 2000 (20 m), PG04 700 (7 m).

`Assets/TestEngine/Data/PG01_Weapon.asset`–`PG08_Weapon.asset`: tipo di attacco, PROJECTILE SPD, perforazioni, geometria.
PG fisici a 20 m/s; PG03 senza perforazioni automatiche. Impostare Penetrations = 2 per verificare tre HIT successive; questo parametro di test non seleziona una PASSIVA.

`Assets/TestEngine/Data/ZOMB01.asset`–`ZOMB05.asset`: STATS senza CD REDUCTION, drop, AGGRO e parametri dei comportamenti. ZOMB04 usa 15 m/s, come confermato dall'owner.

`Assets/TestEngine/Data/TestAreaSettings.asset`: popolazione C3_A5 (330 MOB, 32/33/21/10/4%), FIRST SPAWN 30% (99 MOB), fascia OFF-SCREEN 5 m, soglie EXP, CHEST RATE 5%, cura MEDI KIT 10%, geometria e trigger temporanei.
Il FIRST SPAWN è 33 ZOMB01, 35 ZOMB02, 21 ZOMB03, 10 ZOMB04, 0 ZOMB05. Le quantità residue garantiscono il totale finale per tipo.

`Assets/TestEngine/Data/BonusCatalog.asset`: dieci ABILITÀ BONUS, valori BASE disponibili, pesi di apparizione, upgrade e relativi pesi. Le copie runtime contengono i conteggi degli upgrade: i valori derivati usano sempre la base originale.

Modificare gli asset fuori da Play e riavviare la prova. In Play, Combatant espone STATS e HP runtime separati dagli asset. Le modifiche agli asset effettuate durante Play possono persistere. Per la CHEST usare Force Test Chest Near Player: attivo per default, garantisce una CHEST vicina; disattivato ripristina probabilità e distanza originali.

## Debug

Sull'oggetto TEST ENGINE 2 attivare Show Debug prima di Play. In Scene view attivare Gizmos: NavMesh, bersagli AGGRO, aree speciali e ultimi punti SPAWN vengono mostrati. PlayerAim ha anche Show Aim Gizmos; Combatant ha Show Collider; PlayerWeapon ha Show Debug. I componenti sono ispezionabili nella gerarchia runtime.

La griglia temporanea ha passo 5 m. La camera segue il PG; il piano di gameplay resta XY. La NavMesh usa il piano XZ interno di Unity, con conversione dei punti XY↔XZ, senza cambiare pipeline o package. Il livello usa box allineati agli assi: le verifiche di spicchio contro i box sono geometriche, non campionamenti a raggi.

## Architettura e file

Tutti i nuovi componenti sono in `Assets/TestEngine/`:

| File | Funzione |
| --- | --- |
| CombatStats.cs | STATS comuni PG/MOB, conversioni e formula danno. |
| Combatant.cs | HP runtime, HIT, DOWN/PRE-ESPLOSIONE/MORTE, collisioni di movimento. |
| PlayerRuntime.cs | Copia runtime dei dati PG e BONUS CHEST. |
| MobDefinition.cs, WeaponDefinition.cs, TestAreaSettings.cs, BonusCatalog.cs | Dati Inspector separati dai consumatori runtime. |
| PlayerWeapon.cs, Projectile.cs, CombatAttacks.cs, AttackGeometry.cs | Input attacco, volo, HIT e geometrie. |
| TestObstacle.cs, TestNavigation.cs, MobBrain.cs | Muri/ostacoli, NavMesh e comportamenti MOB. |
| SpawnManager.cs | Quantità, FIRST SPAWN, posizionamento, sostituzioni e drop automatici. |
| ExperienceProgression.cs, BonusInventory.cs | EXP residua, coda LVL UP, sorteggi e acquisizione/upgrade BONUS. |
| AreaPickup.cs | Trigger CHEST/MEDI KIT, scelta del destinatario e consumo singolo. |
| TestEngineBootstrap.cs | Assemblaggio della singola AREA e riavvio della prova. |
| TestHUD.cs, TestVisuals.cs | UI temporanea e presentazione geometrica. |
| Editor/TestEngineValidation.cs | Controlli dati/regole e smoke test Unity dal menu ROG ZOMBIE. |

File preesistenti estesi: PlayerMovement legge le STATS runtime quando presenti e mantiene il funzionamento originale in SampleScene; PlayerAim espone il punto cursore e si ferma in DOWN/pausa. Gli asset PG03/PG04 sono allineati al nuovo GDD.

## Regole confermate e limiti espliciti

- DEF% = (DEF interna − 100) / 100. DANNO = ATK × (1 − DEF%). Gli asset PG conservano la percentuale originale; il runtime la converte in DEF interna. Nessun cap inventato: configurazioni che produrrebbero danni negativi sono segnalate come non definite e la HIT viene rifiutata.
- ZOMB04 PROJECTILE SPD = 15 m/s; CD REDUCTION esclusa dai MOB, come confermato nella conversazione.
- Per PG01 la richiesta TEST ENGINE #2 specifica larghezza 3 m e profondità RANGE: viene usata quella geometria, che ha priorità sul cono a 90° riportato nel GDD aggiornato.
- Le ABILITÀ BONUS vengono acquisite e potenziate nei dati, ma i loro effetti di combattimento non sono attivi: attivazione, mira, tick e alcuni comportamenti sono ancora DA DEFINIRE nel GDD. In particolare non è inventata una base DANNO per FIRE BULLET. Anche la UI dichiara questo limite.
- Le soglie EXP approvate arrivano a LVL 11. Dopo quella soglia l'EXP residua resta memorizzata e viene emesso un avviso; non sono inventate soglie successive.
- La prova ha un solo PG controllato; cambio PG = nuova prova. Nessun multiplayer, compagno IA, abilità/passiva personale, preparazione RUN o transizione.
- Posizioni/layout, dimensioni dei placeholder, griglia, durata dei flash, raggi collider e orientamento iniziale degli spicchi (+X) sono scelte tecniche temporanee di test, non nuove STATS di bilanciamento.
- G è un contatore della prova, senza acquisti o salvataggio permanente.

## Test ripetibili

Menu Unity `ROG ZOMBIE > Test Engine 2 > Validate data and rules`: controlla importazione, STATS, formula DEF, geometrie, quote, eleggibilità/duplicati dei BANNER, incrementi non composti e stati HP.

`Validate and smoke test`: richiede scene salvate e Play fermo; apre TestEngine2, verifica NavMesh, 99 MOB iniziali, esclusione ZOMB05, MEDI KIT raggiungibili a ≥50 m, CHEST vicina quando forzata, otto attacchi inclusa esplosione ritardata PG04, riavvio, sostituzione singola dopo MORTE duplicata e sospensione/ripresa degli stessi proiettili PG/MOB durante LVL UP. Al termine torna alla scena precedente. Report e immagine della camera vengono scritti nella cartella temporanea Windows (`ROG_ZOMBIE_TestEngine2_validation.txt`, `ROG_ZOMBIE_TestEngine2.png`).

Verifica manuale finale: provare gli otto attacchi, colpire MOB e muri, sparare oltre il cursore vicino, impostare temporaneamente perforazioni, osservare ZOMB03/ZOMB04 e la catena ZOMB05, raccogliere gli oggetti e completare più scelte LVL UP.

## Esito verifiche di questa implementazione

- Compilazione finale C# runtime ed Editor riuscita con le librerie Unity 6000.3.16f1; warning trattati come errori, escluso CS0649 come nella configurazione Unity dei campi serializzati.
- Prima esecuzione Unity batch: 3.477 controlli dati/regole superati.
- Primo smoke test in Play Mode superato: NavMesh, 99 MOB iniziali, esclusione ZOMB05 dal FIRST SPAWN, 1–3 MEDI KIT raggiungibili ad almeno 50 m, pre-esplosione ed esplosione ZOMB05.
- Successivamente sono stati aggiunti i test HIT effettivi per tutti gli otto PG e del riavvio come PG08, oltre a piccole correzioni di navigazione/debug. Il relativo nuovo avvio di Unity è stato rifiutato nel pannello autorizzazioni: questi controlli estesi sono compilati ma non ancora eseguiti. Avviarli dal menu indicato sopra.
- Nessun commit. GDD, AGENTS.md, package e impostazioni di progetto non sono stati modificati dall'implementazione; le modifiche già presenti nel working tree sono state preservate.
