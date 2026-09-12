# PG01 — ABILITÀ nel gameplay loop prototype

Riferimento: GDD ufficiale corrente `Docs/ROG_ZOMBIE_GDD.md`, sezioni 10.3, 10.4, 10.8 e 12, aggiornato con la revisione PESTONE RETTANGOLARE. I chiarimenti di posizionamento BARRIERA elencati sotto sono stati forniti dal proprietario durante il task.

## Configurazione e comandi

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`. Nell’asset `Assets/PreGameplayLoop/PreGameplayLoop.asset`, scegliere `Selected Ability`: Pestone oppure Barriera. Il catalogo `PG01Abilities.asset` contiene i valori delle due abilità. La scelta e i valori sono copiati all’inizio RUN: cambiare la configurazione durante Play ha effetto alla prossima RUN tramite Riprova test. Nessuna UI definitiva di selezione è stata aggiunta.

- PESTONE: premere Q; HITSCAN istantaneo sui MOB attivi nell’AREA RETTANGOLARE 3 × 7 m, 40 DANNO prima della DEF. Larghezza frontale costante 3 m e profondità 7 m. Il rettangolo inizia dal PG e si estende nella direzione PG→cursore; MURO/OSTACOLO schermano i singoli bersagli. Nessun collider/proiettile persistente. CD BASE 10 s dall’attivazione.
- BARRIERA: tenere Q per un’anteprima senza collisioni; centro sul cursore, limitato a 8 m dal PG. Lato da 7 m perpendicolare alla direzione PG→cursore, spessore 1 m. Rilasciare Q piazza il muro e avvia CD BASE 15 s. Durata 5 s. Sovrapposizione ad attori e geometria consentita: PG/MOB/PET già sovrapposti vengono spostati in una posizione libera esterna. Il muro è OSTACOLO, senza HP o ricevitore di DANNO. Perdita di focus o uscita dal gameplay annulla l’anteprima senza consumare il CD.

I cooldown iniziano completi a inizio RUN. CD FINALE = CD BASE × CD REDUCTION / 100, con STAT intera, minimo 10 e arrotondamento matematico del risultato al decimo. BONUS e caricamento sospendono il conteggio. Al cambio AREA: DISPONIBILE rimane disponibile; IN CD non attiva conserva il residuo; BARRIERA ancora attiva viene rimossa e riparte il CD completo con il modificatore attuale. Il cooldown parte allo spawn, non alla scadenza del muro. Con CD REDUCTION elevata possono esistere più barriere contemporaneamente: ciascuna conserva la propria durata.

## Struttura

- `PG01AbilityCatalog` + `PG01Abilities.asset`: dati separati dalla logica e dall’ATTACCO BASE.
- `AbilityCooldown`: conteggio e regole di cambio AREA; non dipende dall’input.
- `PG01AbilityRuntime`: una sola selezione per RUN, attivazione, anteprima, durata degli effetti e cleanup.
- `PG01AbilityInput`: adattatore Q del PG controllato direttamente; il runtime può essere richiamato da un futuro sistema di selezione/controllo senza introdurre tasti nella logica degli effetti.
- `PestoneEffect`: query istantanea e HIT tramite il sistema `Combatant` esistente, con arrotondamento matematico del DANNO FINALE dopo DEF. Un overload esplicito conserva invariato il comportamento degli attacchi preesistenti.
- `BarrierEffect`: collider, durata ed espulsione; il campionamento delle posizioni libere è un dettaglio tecnico del prototipo, non un nuovo RANGE di gioco.
- `LoopDefinition`, `LoopSession`, `LoopHUD`: configurazione, persistenza e diagnostica provvisoria.
- `TestNavigation`: ricostruzione alla comparsa/scadenza della BARRIERA, con box orientati. `SpawnManager` esclude posizioni dentro ostacoli temporanei.
- `TestVisuals`: proiettili classificati sui layer PROJECTILE_PG/PROJECTILE_MOB. Registrati i layer già definiti dal GDD PET e PROJECTILE_PG/MOB; aggiornate solo le coppie pertinenti ai nuovi slot nella matrice Physics2D.

## Verifiche

Menu `ROG ZOMBIE → Pre gameplay loop → Run PG01 ability tests`. Il test apre la scena del prototipo e ripristina la scena precedente; richiede una sola scena salvata. Le selezioni di test usano copie runtime degli asset. Il test automatico usa i comandi del runtime, disabilita IA e input reali per rendere ripetibili i bersagli e accelera le morti per attraversare le AREE.

Controlla: scelta unica e reset RUN; CD iniziale, consumo, riduzione, pausa e cambio AREA; bordi/bersagli/danno/ostruzione di PESTONE; anteprima, orientamento, limite 8 m, rilascio, dimensioni, durata, espulsione, collisioni PG/MOB/PET e proiettili, rimozione e nuovo utilizzo della BARRIERA. PET è una sonda Rigidbody2D sul layer PET: il gameplay PET completo resta escluso. Il risolutore fisico ammette la propria tolleranza di contatto; il test verifica che il PET non attraversi il muro.

Eseguire inoltre il test `Run engine smoke test` per la regressione del ciclo originale. La presenza dei test non costituisce prova dell’esito: usare i report dell’esecuzione consegnati con il task.

## Limiti e discrepanze documentali

- PESTONE è allineato al GDD: rettangolo con lato frontale 3 m e profondità 7 m, che sostituisce il precedente cono.
- Anteprima con Q, rilascio, limite 8 m, orientamento e spostamento degli attori sovrapposti sono chiarimenti del task non ancora riportati nel GDD, lasciato invariato come richiesto.
- Nessuna animazione o UI definitiva; PET, multiplayer e controllo PG IA non sono implementati da questo intervento.
- Discrepanza preesistente: gli attacchi legacy tramite `Combatant.Hit(float)` conservano decimali, mentre il GDD richiede il DANNO FINALE intero. PESTONE usa il nuovo overload con arrotondamento; l'eventuale adeguamento generale degli attacchi esistenti resta un intervento separato da decidere.
- Se il level design non lascia alcuno spazio libero per espellere un attore, viene segnalato un errore invece di inventare DANNO, distruzione o attraversamento. È una condizione da evitare/risolvere nella geometria del livello.

## PESTONE — SLOW 30% per 3 s

Il catalogo espone `PestoneSlowPercent = 30` e `PestoneSlowDuration = 3`, copiati a inizio RUN con gli altri dati. Solo una HIT valida applica SLOW ai MOB ancora attivi. Fuori rettangolo, dietro MURO/OSTACOLO o sui PG non viene applicato. Riapplicare PESTONE rinnova la durata, senza sommare la riduzione (GDD 10.6).

`Combatant` conserva separatamente la scadenza di SLOW e calcola la velocità di movimento corrente al 70%; `MobBrain` la usa sia sul percorso NavMesh sia nel movimento diretto. Le STATS non sono riscritte e gli aggiornamenti di MOVE SPD restano indipendenti. Il tempo di gioco sospende la durata durante le pause. SLOW non prolunga l'attivazione istantanea di PESTONE né cambia il suo CD. I MOB dell'AREA precedente vengono eliminati dal loop: nessuno stato SLOW passa ai nuovi MOB; l'inizializzazione dell'attore azzera lo stato.

I test verificano asset importato, bersagli validi/esclusi, spostamento effettivo, movimento AI, scadenza, rinnovo, pausa, modificatori preesistenti e cambiati durante SLOW, reset e nuovi MOB al CAMBIO AREA.

Verifica eseguita il 12/09/2026 in Unity 6000.3.16f1 sul checkout locale, dopo ricompilazione e importazione: Inspector conferma SLOW 30% / 3 s; nuove RUN automatiche. `PESTONE-SLOW-abilities-validation.txt`: PASS, 160 verifiche (22:27:40). `PESTONE-SLOW-loop-validation.txt`: PASS, 259 verifiche (22:28:08). Il test abilità conta 353 warning; nella Console sono presenti avvisi Sprite Tiling / Full Rect già osservati prima dell'aggiunta SLOW. Nessuna prova manuale di tastiera/mouse o valutazione del feeling; i test usano comandi runtime e una chiamata controllata al movimento AI.
