# Ottimizzazione successiva alla build 0.3.1

15 settembre 2026. Modifiche alla working copy; nessuna nuova build, commit o push. La build 0.3.1 già esportata rimane invariata.

## Cambiamenti

- **Aree:** raccolta dei dati delle coperture una volta per ricostruzione, anziché per ogni raggio. Mesh, materiale e liste vengono riutilizzati. La raccolta si ripete a ogni aggiornamento richiesto: MURI/OSTACOLI spostati, disabilitati, rimossi o ricreati continuano a essere considerati. La funzione usata per i danni continua a leggere la geometria corrente.
- **MOB:** lettura dei vicini una volta per operazione di steering, mantenendo l'ordine originale e le formule di separazione. Le due successive proiezioni riutilizzano i dati letti. Nessun cambiamento a velocità, distanze o aggro.
- **Movimento:** query CircleCast su una lista riutilizzabile, senza array nuovo a ogni spostamento. Conservati maschera dei layer e comportamento dei trigger delle query precedenti; la lista può crescere, senza tagliare i risultati.
- **PG IA:** campionamento iniziale NavMesh condiviso tra i candidati, conservazione del percorso del candidato vincente senza ricalcolarlo, buffer riutilizzabile per gli angoli del percorso. Restano la formazione fluida a 1 m, il ricalcolo locale e l'allontanamento temporaneo autorizzato.

## Prestazioni misurate

Confronto tra `PERFORMANCE-audit-0.3.1-r3.txt` e `PERFORMANCE-optimized-0.3.1-r1.txt`, usando lo stesso verificatore, PC, risoluzione e scenari. Le cifre sono mediane di misure **Editor Play Mode**, non FPS della build standalone. Metodo e limiti completi in `PerformanceAudit-0.3.1.md`.

| Operazione/scenario | Prima | Dopo | Riduzione osservata |
|---|---:|---:|---:|
| Ricostruzione area raggio 4 m allo spawn della città | 6,644 ms/chiamata | 0,518 ms/chiamata | 92,2% |
| Ricostruzione area con 4 coperture sintetiche | 1,829 ms/chiamata | 0,466 ms/chiamata | 74,5% |
| Ricostruzione area con 12 coperture sintetiche | 6,115 ms/chiamata | 1,278 ms/chiamata | 79,1% |
| Frame con 82 MOB effettivi | 9,541 ms/frame | 5,963 ms/frame | 37,5% |
| Ricerca IA con spazio locale occupato | 1,386 ms/chiamata | 1,370 ms/chiamata | Sostanzialmente invariata |

Nel caso con 82 MOB il P95 del frame passa da 10,984 a 6,892 ms. Le misure del compagno in condizioni normali sono inferiori al decimo di millisecondo e sensibili al rumore dell'Editor: non sono una base sufficiente per promettere un guadagno percentuale generale dell'IA.

Il contatore aggregato GC per frame nello scenario con 82 MOB scende in media da circa 32,3 a 26,0 kB. Include Editor e altri sistemi; non identifica le allocazioni del solo movimento. Il contatore delle allocazioni per singola chiamata continua a fallire la calibrazione, quindi non vengono riportati falsi valori zero.

## Verifiche effettuate

| Report | Esito |
|---|---|
| `OPTIMIZATION-area-regression-r2.txt` | PASS, 193 verifiche, 0 warning |
| `OPTIMIZATION-pyromania-regression-r1.txt` | PASS, 36 verifiche, 0 warning |
| `OPTIMIZATION-companion-regression-r1.txt` | PASS, 166 verifiche, 0 warning |
| `OPTIMIZATION-loop-regression-r1.txt` | PASS, 251 verifiche |
| `PERFORMANCE-optimized-0.3.1-r1.txt` | Acquisizione completata, 0 warning |

Totale: 646 verifiche nelle quattro suite funzionali. Sono inclusi il confronto di 3.600 raggi fra geometria corrente e snapshot grafico, 128 casi di steering contro l'algoritmo precedente, equivalenza dei risultati delle query fisiche, riutilizzo di mesh/materiali, cambio delle coperture, danni, tick, sovrapposizioni, cambio AREA e reset RUN. Il test PYROMANIA ha prodotto anche uno screenshot controllato visivamente, con area incendiata visibile.

La prima regressione delle aree si è fermata su un'aspettativa obsoleta di PG05: ATK era ancora atteso a 30 anziché 25. Corretto il risultato atteso a 475 HP residui su un bersaglio da 500. Aggiornato anche il vecchio controllo del reset PG01 da 100 a 120 HP; la suite del loop forza esplicitamente la modalità con un solo PG nella propria configurazione temporanea. Nessun valore gameplay è stato modificato per far passare i test.

Compilazione riuscita. Restano i due warning CS0252 preesistenti nel verificatore `PG05Validation.cs`. Il controllo del diff non segnala errori di whitespace; Git segnala le conversioni LF/CRLF già presenti nella working copy.

## File modificati per questa ottimizzazione

- `Assets/PreGameplayLoop/PG04AreaVisual.cs`
- `Assets/TestEngine/AttackGeometry.cs`
- `Assets/TestEngine/MobSeparation.cs`
- `Assets/TestEngine/Combatant.cs`
- `Assets/PreGameplayLoop/CompanionFormation.cs`
- Verificatori `Assets/PreGameplayLoop/Editor/AreaEffectsValidation.cs` e `LoopValidation.cs`.
- Nuovo `Assets/PreGameplayLoop/Editor/OptimizationChecks.cs`, con `.meta`, e report/documentazione associati.

Le altre modifiche già presenti sono state preservate. GDD, versione Unity, pacchetti e regole gameplay non sono stati cambiati durante questa ottimizzazione.

## Limiti residui

- La ricerca IA in condizioni completamente occupate resta costosa e può estendersi molto oltre i candidati locali. Il suo caso massimo teorico non è stato misurato; queste modifiche eliminano duplicazioni senza introdurre un limite temporale che cambi la risposta della formazione.
- La ricerca dei vicini MOB conserva una scansione globale: non è ancora un indice spaziale, benché il lavoro ripetuto sia ridotto.
- Gli effetti nuovi continuano a creare oggetti. Non è stato introdotto pooling generalizzato.
- Le misure non includono profiling GPU isolato, una RUN completa con tutte le abilità simultanee o prove termiche prolungate. Non equivalgono a una certificazione delle prestazioni standalone.
