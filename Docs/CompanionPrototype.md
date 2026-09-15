# Prototipo PLAYER + 1 PG IA

## Selezione e comandi

In `Assets/Scenes/HubPrototype.unity`, aprire PREPARAZIONE RUN e configurare il banner PLAYER 1. Il secondo banner permette di aggiungere un PG IA diverso dal PLAYER, con una ABILITÀ e una PASSIVA proprie. Confermare entrambi e avviare la RUN. RIMUOVI IA permette di giocare ancora con il solo PLAYER; i posti 3 e 4 restano bloccati. INDIETRO conserva la bozza senza confermarla.

Nel prototipo diretto `PreGameplayLoopPrototype`, usare la sezione Compagno IA di prova di `Assets/PreGameplayLoop/PreGameplayLoop.asset`: Enable Companion, Companion Player, Companion Ability e Companion Passive (0 = prima, 1 = seconda). La configurazione viene acquisita alla nuova RUN.

- WASD controlla il PLAYER; il compagno segue la formazione a 1 m dietro rispetto al cursore, adattando la velocità senza modificare MOVE SPD persistente.
- Ruotando il cursore, la posizione di formazione cambia gradualmente lungo un cerchio di raggio 1 m. Il parametro tecnico Aim Smooth Time del componente CompanionFormation è 0,25 s; la mira e gli attacchi restano immediati.
- LMB comanda entrambi gli ATTACCHI BASE verso lo stesso cursore, ciascuno con il proprio ATK SPD.
- Q controlla l'ABILITÀ del PG direttamente controllato. SPACE + 1 controlla quella del compagno; per le abilità a rilascio mantenere la combinazione per l'anteprima e rilasciarla per attivare.
- PASSIVE e BONUS appartengono al singolo PG. Le scelte di LEVEL UP e fine AREA vengono mostrate separatamente al PLAYER responsabile. Nessuno SLOT ITEM per il compagno.
- L'HUD indica PG IA, HP, livello/EXP e PG sotto controllo diretto. HP, progressione e selezione restano al cambio AREA; Riprova test avvia una nuova RUN.

## Verifiche effettive — Unity 6000.3.16f1

- `COMPANION-validation-r8.txt`: PASS, 166 verifiche, 0 warning in Play Mode. Entrambe le ABILITÀ di PG01–PG08 provate come compagno, un solo IA per volta; attori e inventari indipendenti; sparo condiviso e cadenze individuali; SPACE + 1 con anteprima/rilascio BARRIERA; formazione a 1 m, avvio graduale senza salti, inversione della mira, posizione alternativa entro 1 m contro MURO, spazio occupato dai PG, allontanamento temporaneo e rientro; pausa; LEVEL UP simultanei; scelte individuali di fine AREA; persistenza e cura; reset RUN; cambio controllo in DOWN; requisiti PARTY all'uscita; sconfitta con entrambi DOWN.
- `HUB-companion-screens-validation-r3.txt`: PASS, 109 verifiche, 0 warning in Play Mode. Roster, conferme, bozze, esclusione del PG già assegnato, configurazione dei due PG e rimozione del compagno.
- Compilazione runtime ed Editor riuscita. Restano due warning CS0252 preesistenti in `PG05Validation.cs`, righe 317 e 320.
- Le prime prove fallite restano disponibili: r1 ha individuato l'inizializzazione troppo precoce di NavMeshPath, corretta spostandola in Awake; r4 usava il cursore prima dell'aggiornamento della telecamera della fixture, corretto prima delle prove finali.
- Nessuna build, commit o push eseguiti per questa integrazione.

## Limiti della prima integrazione

Il compagno rispetta NavMesh e collisioni: se la posizione prevista è occupata, cerca una destinazione libera entro 1 m dal PLAYER. Se tutte le posizioni locali sono occupate, può allontanarsi temporaneamente e cerca di rientrare a ogni aggiornamento. Non si teletrasporta e non prende decisioni autonome di sopravvivenza. Le formazioni con due/tre IA e il multiplayer non sono inclusi.

Implementata RIANIMAZIONE: F mantenuto entro 2 m per 5 s; timer DOWN di 20 s congelato durante l'interazione. Rilasciando F o uscendo dal raggio, il progresso perde 1 s/s e il timer DOWN riprende. Anche il PG IA sotto controllo diretto può rianimare il PG originale. Ritorno al 50% HP massimi correnti e 2 s di invulnerabilità, con collisioni normali. I PG in MORTE tornano al cambio AREA al 50%, senza cura aggiuntiva del 15%.

### Verifica RESURREZIONE — 15/09/2026

- `RESURRECTION-companion-validation-r1.txt`: PASS, 199 verifiche, 0 warning in Play Mode (33 verifiche aggiunte e 166 di regressione del compagno).
- `RESURRECTION-loop-validation-r1.txt`: PASS, 251 verifiche del loop in Play Mode.
- Compilazione runtime ed Editor riuscita; restano due warning CS0252 già presenti in PG05Validation.cs, righe 317 e 320.
- Verificati timer DOWN, pausa/regressione/ripresa, soglia 2 m, recupero HP con massimo modificato, invulnerabilità e scadenza, collisioni DOWN/MORTE, ritorno del controllo, ripristino al cambio AREA e reset RUN. Le prove automatiche invocano lo stesso metodo usato dal comando F; non sostituiscono una prova manuale dell'interazione e dell'HUD. Il ripristino da MORTE al cambio AREA è verificato direttamente; la regressione del cambio AREA completo usa PG vivi.
- Meccanica integrata in ResurrectionRuntime.cs, Combatant.cs, LoopSession.cs, LoopSession.Party.cs e LoopHUD.cs. Test in Editor/AreaEffectsValidation.cs. GDD aggiornato con le due conferme del proprietario.
- Nessuna build, commit o push effettuati.
