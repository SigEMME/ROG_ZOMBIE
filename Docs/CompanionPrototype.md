# Prototipo PLAYER + fino a 3 PG IA

## Selezione e comandi

In `Assets/Scenes/HubPrototype.unity`, aprire PREPARAZIONE RUN e configurare il banner PLAYER 1. I tre banner PG IA permettono di aggiungere compagni distinti, ciascuno con una ABILITÀ e una PASSIVA proprie. Confermare tutti i membri abilitati e avviare la RUN. Ogni RIMUOVI IA libera solo il relativo posto; gli altri mantengono configurazione e numero del comando. INDIETRO conserva la bozza senza confermarla.

Nel prototipo diretto `PreGameplayLoopPrototype`, usare la sezione PG IA 1 di `Assets/PreGameplayLoop/PreGameplayLoop.asset`: Enable Companion, Companion Player, Companion Ability e Companion Passive (0 = prima, 1 = seconda). PG IA 2 e 3 sono configurabili in Additional Companions: Enabled, Player, Ability, Passive. La configurazione viene acquisita alla nuova RUN.

- WASD controlla il PLAYER; il compagno segue la formazione a 1 m dietro rispetto al cursore, adattando la velocità senza modificare MOVE SPD persistente.
- Ruotando il cursore, la posizione di formazione cambia gradualmente lungo un cerchio di raggio 1 m. Il parametro tecnico Aim Smooth Time del componente CompanionFormation è 0,25 s; la mira e gli attacchi restano immediati.
- LMB comanda entrambi gli ATTACCHI BASE verso lo stesso cursore, ciascuno con il proprio ATK SPD.
- Q controlla l'ABILITÀ del PG direttamente controllato. SPACE + 1/2/3 controlla quella del compagno nel relativo banner; per le abilità a rilascio mantenere la combinazione per l'anteprima e rilasciarla per attivare.
- PASSIVE e BONUS appartengono al singolo PG. Le scelte di LEVEL UP e fine AREA vengono mostrate separatamente al PLAYER responsabile. Nessuno SLOT ITEM per il compagno.
- L'HUD indica PG IA, HP, livello/EXP e PG sotto controllo diretto. HP, progressione e selezione restano al cambio AREA; Riprova test avvia una nuova RUN.

## Verifiche effettive — Unity 6000.3.16f1

- `COMPANION-validation-r8.txt`: PASS, 166 verifiche, 0 warning in Play Mode. Entrambe le ABILITÀ di PG01–PG08 provate come compagno, un solo IA per volta; attori e inventari indipendenti; sparo condiviso e cadenze individuali; SPACE + 1 con anteprima/rilascio BARRIERA; formazione a 1 m, avvio graduale senza salti, inversione della mira, posizione alternativa entro 1 m contro MURO, spazio occupato dai PG, allontanamento temporaneo e rientro; pausa; LEVEL UP simultanei; scelte individuali di fine AREA; persistenza e cura; reset RUN; cambio controllo in DOWN; requisiti PARTY all'uscita; sconfitta con entrambi DOWN.
- `HUB-companion-screens-validation-r3.txt`: PASS, 109 verifiche, 0 warning in Play Mode. Roster, conferme, bozze, esclusione del PG già assegnato, configurazione dei due PG e rimozione del compagno.
- Compilazione runtime ed Editor riuscita. Restano due warning CS0252 preesistenti in `PG05Validation.cs`, righe 317 e 320.
- Le prime prove fallite restano disponibili: r1 ha individuato l'inizializzazione troppo precoce di NavMeshPath, corretta spostandola in Awake; r4 usava il cursore prima dell'aggiornamento della telecamera della fixture, corretto prima delle prove finali.
- Nessuna build, commit o push eseguiti per questa integrazione.

## Limiti della prima integrazione

Il compagno rispetta NavMesh e collisioni: se la posizione prevista è occupata, cerca una destinazione libera entro 1 m dal PLAYER. Se tutte le posizioni locali sono occupate, può allontanarsi temporaneamente e cerca di rientrare a ogni aggiornamento. Non si teletrasporta e non prende decisioni autonome di sopravvivenza. Sono incluse le formazioni con due e tre IA; il multiplayer non è incluso.

Implementata RIANIMAZIONE: F mantenuto entro 2 m per 5 s; timer DOWN di 20 s congelato durante l'interazione. Rilasciando F o uscendo dal raggio, il progresso perde 1 s/s e il timer DOWN riprende. Anche il PG IA sotto controllo diretto può rianimare il PG originale. Ritorno al 50% HP massimi correnti e 2 s di invulnerabilità, con collisioni normali. I PG in MORTE tornano al cambio AREA al 50%, senza cura aggiuntiva del 15%.

### Verifica RESURREZIONE — 15/09/2026

- `RESURRECTION-companion-validation-r1.txt`: PASS, 199 verifiche, 0 warning in Play Mode (33 verifiche aggiunte e 166 di regressione del compagno).
- `RESURRECTION-loop-validation-r1.txt`: PASS, 251 verifiche del loop in Play Mode.
- Compilazione runtime ed Editor riuscita; restano due warning CS0252 già presenti in PG05Validation.cs, righe 317 e 320.
- Verificati timer DOWN, pausa/regressione/ripresa, soglia 2 m, recupero HP con massimo modificato, invulnerabilità e scadenza, collisioni DOWN/MORTE, ritorno del controllo, ripristino al cambio AREA e reset RUN. Le prove automatiche invocano lo stesso metodo usato dal comando F; non sostituiscono una prova manuale dell'interazione e dell'HUD. Il ripristino da MORTE al cambio AREA è verificato direttamente; la regressione del cambio AREA completo usa PG vivi.
- Meccanica integrata in ResurrectionRuntime.cs, Combatant.cs, LoopSession.cs, LoopSession.Party.cs e LoopHUD.cs. Test in Editor/AreaEffectsValidation.cs. GDD aggiornato con le due conferme del proprietario.
- Nessuna build, commit o push effettuati.

## Estensione a quattro PG — 28/09/2026

- Un IA: 1 m dietro; due IA: triangolo equilatero con lati di 1 m; tre IA: rombo con quattro lati di 1 m e diagonali di circa 1,41 m. I due compagni laterali arretrano di circa 0,71 m e si scostano di circa 0,71 m; il terzo arretra di circa 1,41 m.
- La rotazione segue gradualmente la mira. Le collisioni possono deformare temporaneamente la formazione; il raggio di ricerca locale del posto posteriore è 1,41 m. Nessun teletrasporto, nessuna modifica permanente a MOVE SPD.
- Tutti i membri hanno HP, abilità, passive, BONUS ed EXP separati. Uscita AREA, ricompense, trasferimento del controllo, cura/resurrezione e reset RUN gestiscono il party completo.
- F rianima esclusivamente il DOWN più vicino entro 2 m. A pari distanza la priorità tecnica è l'ordine del party; gli altri timer continuano. Il controllo passa al primo PG attivo nell'ordine di selezione e ritorna al PG originale quando torna attivo.

### Risultati dell'estensione

- `PARTY-four-validation-r4.txt`: PASS, 287 verifiche, 0 warning Play Mode. Include regressione del singolo compagno per tutti gli otto PG, formazione con tre IA e rotazione, triangolo con due IA, configurazioni parziali, comandi separati premuti/mantenuti/rilasciati, rianimazione del più vicino, cambio controllo, scelte individuali, cambio AREA e reset RUN.
- `PARTY-loop-validation-r1.txt`: PASS, 251 verifiche del loop preesistente.
- `PARTY-hub-validation-r1.txt`: PASS, 116 verifiche, 0 warning Play Mode. Verificati quattro banner, selezioni indipendenti e configurazione completa; controllo visivo dei quattro banner eseguito in Unity.
- Le prime due esecuzioni PARTY-four sono fallite nell'iniezione dei comandi di test, dopo aver superato le verifiche geometriche. Il verificatore ora elabora esplicitamente gli eventi Input System prima di leggere i tasti; r3 e r4 passano. Nessun errore di gameplay viene mascherato abbassando le aspettative.
- Compilazione riuscita; rimangono i due warning CS0252 preesistenti in PG05Validation.cs. I test automatici disabilitano i brain MOB e controllano scenari deterministici: resta utile una prova manuale prolungata nei passaggi affollati e con tutte le abilità simultanee. Nessuna nuova build, commit o push.

### Passaggi stretti — correzione del blocco davanti al PLAYER

Su segnalazione della prova manuale: quando un compagno si trova davanti alla direzione di movimento e non c'è spazio laterale per aggirare il PLAYER, segue temporaneamente il comando di movimento anche fuori formazione. Il PLAYER espone il movimento richiesto prima delle collisioni, così la fila può ripartire anche se il PLAYER è già bloccato. Il comportamento comprende i compagni in fila, preserva NavMesh e collisioni e riprende la formazione quando c'è spazio. Non cambia MOVE SPD persistente.

File interessati: `Assets/PlayerMovement.cs`, `Assets/PreGameplayLoop/CompanionFormation.cs`, verificatore `Editor/AreaEffectsValidation.cs` e GDD.

Compilazione runtime/Editor riuscita; due warning CS0252 preesistenti in PG05Validation. `PARTY-passage-validation-r2.txt` e `r3.txt`: PASS, 309 verifiche, 0 warning Play Mode. Incluse le regressioni precedenti (287) e le prove di corridoio con tre IA davanti, avanzamento del PLAYER, collisioni PG/MURI, pausa, conservazione STATS e recupero della formazione dopo la rimozione delle coperture. La prima esecuzione si è fermata nel test storico di input LMB simulato (0 colpi per entrambi), prima della prova del corridoio; le successive sono passate.

Il corridoio è una fixture deterministica con movimento richiesto fornito direttamente al metodo usato dalla formazione; resta da riprovare manualmente la situazione specifica segnalata nel livello. Nessuna build, commit o push.

### Conferma della prova manuale — 28/09/2026

Il proprietario ha confermato l’esito positivo della prova manuale di posizionamento e movimento dei PG IA, successiva alla correzione dei passaggi stretti.
