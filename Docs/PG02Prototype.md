# PG02 nel gameplay loop prototype

## Configurazione e comandi

Scena: `Assets/Scenes/PreGameplayLoopPrototype.unity`. Nell'asset `Assets/PreGameplayLoop/PreGameplayLoop.asset` scegliere `Selected Player = PG02`, `Selected PG02 Ability` e `Selected PG02 Passive`. Ogni scelta è singola. I campi PG01 sono mantenuti per poter tornare a PG01; per PG02 vengono usate soltanto le selezioni PG02.

Le quattro combinazioni sono FUOCO RAPIDO + PASSIVA 1, FUOCO RAPIDO + PASSIVA 2, FUOCO DI SOPPRESSIONE + PASSIVA 1 e FUOCO DI SOPPRESSIONE + PASSIVA 2. Sono tutte configurazioni valide. Play / Riprova test acquisisce le scelte per la nuova RUN; modifiche di configurazione durante Play non cambiano il PG, l'abilità o la passiva della RUN corrente.

WASD muove, mouse orienta, LMB mantenuto spara, Q attiva l'abilità selezionata. Le passive non richiedono input né SLOT BONUS. Un solo PG controllato, ZOMB01 come unico MOB; nessuna party o UI definitiva. L'HUD mostra PG, HP, abilità/CD/durata, passiva, KILL personali, progresso RAGE e STATS effettive.

## Valori e comportamento

L'asset PG02 esistente resta invariato: HP 100, ATK 10, DEF mostrata 0% convertita una sola volta in interna 100, MOVE SPD 100 (2 m/s), ATK SPD 400 (4/s), CD REDUCTION 100, RANGE interno 1000 (10 m). Arma PG02 esistente: proiettile a 20 m/s, non perforante, primo MOB valido o MURO/OSTACOLO lo arrestano, PG alleati ignorati. Danno e cadenza usano le STATS effettive al momento dello sparo.

`PG02Abilities.asset` contiene i valori configurabili delle due abilità, copiati a inizio RUN:

- FUOCO RAPIDO: +40% ATK SPD corrente, 400 → 560 (5,6 attacchi/s) senza altri modificatori, durata 4 s. CD BASE 10 s avviato alla fine della durata. Non può essere riattivata durante l'effetto. Un BONUS ATK SPD permanente porta 400 → 420; durante FUOCO RAPIDO vale 588, alla scadenza rimane 420.
- FUOCO DI SOPPRESSIONE: 50 proiettili fisici in 3 s, ciascuno da 3 DANNO prima della DEF e dell’arrotondamento finale. RANGE 10 m; cono 50° diviso in cinque spicchi da 10°. Centro dello spicchio confermato dal proprietario: -20°, -10°, 0°, +10°, +20°, +10°, 0°, -10°, poi ripete. Primo colpo all’attivazione e cinquantesimo a 3 s, intervalli uniformi di 3/49 s; recupero delle emissioni dovute nei frame lenti senza perdita di colpi. CD BASE 12 s dall’attivazione, continua durante la raffica; nessuna riattivazione finché attiva, anche con CD ridotto. Spawn dalla bocca attuale e mira corrente; traiettoria fissa dopo lo sparo. Velocità e raggio provengono dall’arma PG02 (20 m/s iniziali), nessuna perforazione. Primo MOB, MURO o OSTACOLO arrestano il proiettile; PG alleati ignorati. ATK/RAGE e ATK SPD non modificano danno o numero dei colpi. L’attacco base conserva il proprio funzionamento con LMB.
- PASSIVA 1: sotto il 30% degli HP massimi correnti, ciascuna KILL personale cura l'1% degli HP massimi correnti; la soglia viene rivalutata per ogni KILL di una HIT multipla. Il confronto conserva l'esclusione del valore esatto 30% anche con la precisione float.
- PASSIVA 2: ogni 15 KILL personali attiva RAGE per 2 s, +30% ATK corrente. Nuovo gruppo di 15: durata rinnovata senza cumulo. BONUS ATK persistenti restano separati e non vengono persi alla scadenza. RAGE non incrementa il danno fisso di FUOCO DI SOPPRESSIONE.

CD iniziali completi; CD REDUCTION e arrotondamento riusano `AbilityCooldown`. BONUS/loading sospendono i timer. Al CAMBIO AREA, FUOCO RAPIDO attivo termina e riparte il CD finale completo; un CD già in corso ma non attivo conserva il residuo, DISPONIBILE resta DISPONIBILE. Come confermato dal proprietario, le KILL personali e il progresso verso 15 restano, mentre RAGE termina. Una nuova RUN azzera KILL, effetti e BONUS di quella precedente.

## Attribuzione e modificatori

`Combatant.Hit` accetta la sorgente; la HIT letale conserva il PG responsabile e `Die` emette il credito una sola volta alla morte effettiva. `Projectile` conserva sorgente e criterio di arrotondamento dallo spawn alla HIT. `SuppressionEffect` propaga il PG sorgente. Morti dirette di test e HIT senza sorgente non danno credito a PG02; il contatore globale di popolazione non alimenta le passive. La sorgente è anche propagata dalle HIT frontali esistenti e da PESTONE, senza alterarne il danno.

`Combatant.Stats` rimane persistente; `EffectiveStats` compone separatamente modificatore dell'abilità e della passiva mediante il contratto già introdotto per PG01. Nessuna riscrittura o sottrazione distruttiva delle STATS quando terminano FUOCO RAPIDO/RAGE. Il supporto aggiunto riguarda le fonti presenti nel prototipo; altre future fonti dovranno propagare esplicitamente la propria sorgente.

## GDD e verifica

Confrontato il GDD del repository con il file corretto fornito: le sole differenze iniziali erano ATK SPD BASE PG02 400, FUOCO RAPIDO +30% corrente e la relativa revisione. Il file corretto è stato adottato integralmente; aggiunta soltanto la successiva conferma del proprietario su persistenza KILL e termine RAGE al CAMBIO AREA nella PASSIVA 2 di PG02. Le altre regole non sono state modificate.

Menu `ROG ZOMBIE → Pre gameplay loop → Run PG02 tests`. Il test usa copie runtime della configurazione, attraversa tutte le combinazioni e ripristina la scena precedente; richiede una sola scena salvata. Per automazione, `.pg02-test.request` contiene il percorso completo del report e deve essere creato dopo la ricompilazione verificata. Rieseguire `Run PG01 ability tests`, `Run PG01 passive tests` e `Run engine smoke test` per regressione.

I test disabilitano IA e input per controllare le condizioni e usano morti di test per attraversare le AREE. Verificano anche spari e HIT reali: non sono solo controlli sui valori dell'asset. Non costituiscono una prova di bilanciamento o del feeling manuale. Gli esiti effettivi sono riportati nei report di esecuzione.

## Esiti effettivi — 12/09/2026

Unity 6000.3.16f1: compilazione runtime ed editor verificata dopo importazione; ultima correzione runtime compilata alle 23:44:52 e test editor alle 23:46:34.

| Suite | Report finale | Esito | Ora locale |
| --- | --- | --- | --- |
| PG02, quattro combinazioni | `PG02-validation.txt` (copia del report finale `PG02-validation-r3.txt`) | PASS, 386 verifiche, 755 warning | 23:48:36 |
| Abilità PG01 | `PG02-PG01-abilities-regression.txt` | PASS, 160 verifiche, 353 warning | 23:49:54 |
| Passive PG01 | `PG02-PG01-passives-regression.txt` | PASS, 135 verifiche, 425 warning | 23:50:24 |
| Gameplay loop | `PG02-loop-regression.txt` | PASS, 259 verifiche | 23:51:04 |

La prima esecuzione ha rilevato e consentito di correggere il confronto float della soglia esatta 30%. Un secondo tentativo, conservato in `PG02-validation-r2.txt`, è fallito perché il cursore reale ruotava la bocca dell'arma nel test: la prova finale disabilita anche PlayerAim e usa un orientamento controllato. Il report valido conclusivo è quello r3/copia `PG02-validation.txt`.

Persistono i warning Sprite Tiling / Full Rect della visualizzazione provvisoria. Nessuna eccezione interrompe le suite finali. Osservati in Play Mode HUD PG02, ATK SPD 400 (4/s), timer, selezione passiva e KILL personali; non eseguita una prova completa del feeling tramite LMB/Q fisici. L'Editor è fuori Play Mode, con la configurazione salvata PG01 preservata. Per giocare PG02 basta cambiare Selected Player come descritto sopra.

## Aggiornamento FUOCO RAPIDO — 13/09/2026

Applicato il GDD aggiornato fornito dal proprietario limitatamente a FUOCO RAPIDO: +40% ATK SPD corrente per 4 s, CD BASE 10 s dalla fine dell'effetto. Aggiornati catalogo, asset e aspettative dei test, aggiunta la misura della durata effettiva. La configurazione selezionata dal proprietario resta preservata; per provare l'aggiornamento selezionare FUOCO RAPIDO e iniziare una nuova RUN.

Compilazione runtime/editor verificata in Unity 6000.3.16f1. `PG02-rapid-40-4-validation.txt`: PASS, 388 verifiche e 755 warning catturati. La suite Play Mode attraversa tutte e quattro le combinazioni PG02: spari reali a 5,6/s, durata misurata 4,001/4,002 s, BONUS persistente 420 → 588 → 420, cooldown, coesistenza con RAGE, cambio AREA e reset RUN. Console finale senza errori; restano i warning Sprite Tiling / Full Rect. Le suite separate PG01 e gameplay loop non sono state rieseguite per questo aggiornamento; gli esiti del 12/09 sopra restano storici.

Storico del solo aggiornamento FUOCO RAPIDO (superato dall’intervento seguente): FUOCO DI SOPPRESSIONE conservava l'implementazione precedente descritta sopra. La nuova versione a 50 proiettili del GDD aggiornato non fa parte di questo intervento. Nessuna modifica al GDD in questo aggiornamento dell'engine.

## File di questo intervento

- Nuovi: `Assets/PreGameplayLoop/PG02AbilityCatalog.cs`, `PG02Abilities.asset`, `PG02AbilityRuntime.cs`, `PG02AbilityInput.cs`, `PG02PassiveRuntime.cs`, `SuppressionEffect.cs`, `Editor/PG02Validation.cs`, con i rispettivi `.meta`.
- Integrazione: `Assets/PreGameplayLoop/LoopDefinition.cs`, `LoopSession.cs`, `LoopHUD.cs`, `PreGameplayLoop.asset`.
- HIT, sorgente e STATS effettive: `Assets/TestEngine/Combatant.cs`, `Projectile.cs`, `PlayerWeapon.cs`, `CombatAttacks.cs`, `TestVisuals.cs`; `Assets/PreGameplayLoop/PestoneEffect.cs`, `PG01AbilityRuntime.cs`.
- Harness di regressione adattati alla selezione del PG: `Assets/PreGameplayLoop/Editor/AbilityValidation.cs`, `PassiveValidation.cs`, `LoopValidation.cs`.
- Documentazione: `Docs/ROG_ZOMBIE_GDD.md`, `Docs/PreGameplayLoopPrototype.md`, questo file e i report elencati sopra.

L'integrazione delle passive PG01 e gli altri file non committati preesistenti sono preservati. I suoi file ancora non tracciati, come `PG01PassiveRuntime.cs` e `ICombatStatModifier.cs`, restano presenti e vengono riutilizzati. Asset base PG02/arma, impostazioni di progetto, Unity, pacchetti e Render Pipeline invariati. Nessun commit o push; nessuna modifica manuale a Library, Temp, Logs o UserSettings.

## Aggiornamento FUOCO DI SOPPRESSIONE — 13/09/2026

Sostituito il vecchio HITSCAN con la raffica fisica descritta sopra. Riutilizzati collisioni, attribuzione delle KILL e arrotondamento del sistema proiettili esistente. Il cambio AREA interrompe la raffica e riparte dal CD finale completo; i proiettili dell’AREA vengono rimossi con essa. BONUS/loading sospendono emissioni e timer. La nuova RUN acquisisce la selezione e riparte senza raffica attiva.

File aggiornati: PG02AbilityCatalog.cs, PG02Abilities.asset, PG02AbilityRuntime.cs, PG02AbilityInput.cs, SuppressionEffect.cs, TestVisuals.cs (restituisce il proiettile creato), Editor/PG02Validation.cs e questo documento. Tutti i .meta preservati. GDD invariato.

Validazione in Unity 6000.3.16f1: compilazione runtime ed editor riuscita. La prima suite aggiornata ha superato 813 controlli; dopo l’aggiunta dei casi limite, `PG02-suppression-validation.txt` riporta PASS, 820 controlli e 929 warning Sprite Tiling / Full Rect. Verificati tutti i 50 centri e tempi di emissione, mira aggiornata, traiettorie fisse dopo lo sparo, pausa, durata e CD, collisioni, RANGE, danni fissi e arrotondamento per HIT, RAGE, KILL personali, cambio AREA, CD ridotto senza sovrapposizione e riavvio con proiettili in volo. Sono inclusi i test di FUOCO RAPIDO e di entrambe le passive PG02. Nessuna prova completa del feeling manuale; visuali e warning degli sprite restano quelli provvisori del prototipo.

Regressioni eseguite dopo questa modifica: `PG02-suppression-PG01-abilities.txt` PASS, 160 controlli e 353 warning; `PG02-suppression-loop.txt` PASS, 259 controlli. La suite separata delle passive PG01 non è stata rieseguita. Configurazione salvata PG02 / FUOCO DI SOPPRESSIONE / PASSIVA 2 preservata. Nessun commit o push.

## Conferma del proprietario — 13/09/2026

Il proprietario ha comunicato «test su PG02 esito OK» dopo la consegna di FUOCO RAPIDO aggiornato e FUOCO DI SOPPRESSIONE a proiettili fisici. Registrata la prova utente con esito positivo; le precedenti note sulla mancata prova manuale descrivono lo stato al momento delle rispettive consegne. Non sono stati forniti dettagli sui singoli casi provati manualmente.

Aggiornato il GDD ufficiale con la direzione al centro degli spicchi e questa conferma. Restano validi i report automatici già eseguiti: PG02 820 controlli, abilità PG01 160, gameplay loop 259, tutti PASS. I warning Sprite Tiling / Full Rect e i limiti della visualizzazione provvisoria restano presenti. Questo aggiornamento è solo documentale: nessuna nuova esecuzione dei test Unity, nessuna modifica al codice o agli asset.
