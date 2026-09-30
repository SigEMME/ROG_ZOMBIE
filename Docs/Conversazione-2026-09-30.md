# Conversazione e ripresa del lavoro ROG ZOMBIE

Documento salvato il 30 settembre 2026 su richiesta del proprietario per riprendere il lavoro su un altro PC. Contiene un riepilogo della conversazione disponibile, lo stato verificato della repository e gli ultimi scambi relativi a BOSS01. **Non è una trascrizione integrale della chat**: non ricostruisce risposte mancanti, allegati o risultati di tool non conservati. Le immagini originali citate nella conversazione non sono incorporate in questo file.

## Come riprendere sull altro PC

1. Includere questo documento e le modifiche di progetto desiderate in un commit sul PC di origine e pubblicarlo con push. Salvare un file nella cartella della repository non lo pubblica automaticamente.
2. Sull'altro PC aggiornare la repository con pull, oppure clonarla se non presente.
3. Chiedere al nuovo assistente: «Leggi AGENTS.md, Docs/Conversazione-2026-09-30.md e Docs/ROG_ZOMBIE_GDD.md; controlla git status e diff prima di proseguire. Preserva le modifiche presenti.»
4. Per recuperare ogni messaggio originale occorre anche conservare un'esportazione della chat; questo documento serve come contesto operativo.

## Stato della repository al salvataggio

- Progetto Unity: ROG ZOMBIE; Unity 6000.3.16f1. Visuale attuale TOP-DOWN, successiva alla precedente impostazione isometrica.
- Ultimo commit locale osservato: `07508f0` — `Add boss encounter, preparation UI updates and textured urban area`.
- Sono presenti numerose modifiche non committate, incluse quelle precedenti al nuovo HUD. Non cancellarle né sovrascriverle.
- Nuovi sorgenti ancora non tracciati: `Assets/PreGameplayLoop/SprintRuntime.cs`, `LoopHUD.Run.cs`, `RunHudData.cs`, `Editor/RunHudValidation.cs`, con i rispettivi `.meta`.
- Sono presenti anche report di test non tracciati, `Assets/_Recovery/`, il relativo `.meta` e `output/`. Non sono stati cancellati o aggiunti automaticamente.
- Nessun commit, push o nuova build eseguito per gli ultimi interventi HUD e BOSS01 o per salvare questo documento.
- La verifica remota non è stata ripetuta per questo salvataggio: non assumere che il lavoro locale sia già su GitHub.

## Vincoli di lavoro

Il proprietario definisce le regole di gioco. Non inventare valori mancanti, non ribilanciare autonomamente e chiedere chiarimenti se necessari. Leggere il GDD e AGENTS.md, mantenere le modifiche esistenti e i `.meta`, evitare refactor estranei, aggiornamenti Unity/pacchetti e modifiche manuali a Library, Temp, Logs e UserSettings. Commit, push e build richiedono una richiesta pertinente; precedenti autorizzazioni a eseguirli non rendono automatiche le operazioni successive.

## Percorso del prototipo già discusso

La conversazione ha seguito l'integrazione dei PG01–PG08, le ABILITÀ BONUS, gli ITEMS, HUB e PREPARAZIONE RUN, PG IA, rianimazione, PAUSA, schermata iniziale, livelli urbani e passaggio alla visuale TOP-DOWN. Nel tempo il proprietario ha aggiornato ripetutamente valori e regole: per lo stato finale usare GDD, asset e documenti tecnici, non le prime richieste storiche.

- Il proprietario ha confermato manualmente il funzionamento HUB → RUN → HUB, della schermata iniziale, delle schermate PAUSA e del posizionamento/movimento IA in precedenti prove.
- Le versioni 0.3.1, 0.3.2 e 0.3.3 sono state nominate nella conversazione; 0.3.3 è stata dichiarata dopo il passaggio TOP-DOWN. Gli interventi qui riepilogati sono successivi: non è stata assegnata una nuova versione.
- Il party del prototipo supporta un PG controllato e fino a tre PG IA. La formazione è orientata rispetto alla mira; con tre IA forma un rombo con lati di 1 m e diagonali di circa 1,41 m. Gli IA condividono il comando di fuoco, rispettando il proprio ATK SPD; abilità con SPACE + 1/2/3. Sono state affrontate fluidità, passaggi stretti e impedimento al movimento del PLAYER.
- Le immagini di riferimento originali erano in `C:/Users/matti/Documents/ROG ZOMBIE/REFERENCE/`; tale percorso personale non è portabile. Gli asset effettivamente importati in Assets seguono invece la repository quando committati.
- Per ulteriori dettagli storici consultare i documenti dedicati in Docs: PG01–PG08, BonusAbilitiesPrototype, ItemsPrototype, CompanionPrototype, HubPrototype, TopDownStudy e i layout delle città.

## Decisioni recenti prima dell HUD

### Popolazione e FIRST SPAWN

- MOB della prima AREA: CITTÀ 1 = 130; CITTÀ 2 = 160; CITTÀ 3 = 190; CITTÀ 4 = 190; CITTÀ 5 = 220.
- Le altre AREE seguono la progressione percentuale già definita, non un numero fisso uguale alla prima.
- Posizionamento FIRST SPAWN: MOB almeno 25 m oltre il bordo visibile; se manca un punto valido si cerca più lontano. Il resto delle regole resta invariato.

### Crescita MOB durante la RUN

- Dopo ogni AREA ordinaria: +5% del valore BASE a HP, ATK, MOVE SPD e ATK SPD; crescita lineare (100 → 105 → 110), non composta.
- Arrotondamento all'intero più vicino; le metà sono arrotondate verso l'alto nell'implementazione.
- RANGE e DEF invariati. Si applica esclusivamente a ZOMB01–ZOMB05, incluso il danno dell'esplosione di ZOMB05.
- Il conteggio continua nelle CITTÀ successive e si azzera alla nuova RUN. Le AREE BOSS non incrementano il conteggio.
- Ogni BOSS ha un'AREA dedicata aggiuntiva dopo le AREE ordinarie della propria CITTÀ. Le AREE BONUS sono state eliminate dal GDD.

### SCATTO e MERCHANT

- SHIFT: raddoppia la velocità di movimento corrente per 2 s.
- Recupero fisso finale: 15 s, avviato quando termina lo SCATTO; la richiesta iniziale di 10 s è stata superata.
- Nel MERCHANT sono state invertite le posizioni di POZIONE CURATIVA e TRAPPOLA.

## Nuovo HUD della RUN

Reference: `HUD_Test.png`. Il proprietario ha confermato il terzo PG IA, l'assenza per ora dell'indicatore SCATTO, le icone provvisorie per PG/abilità e l'uso delle icone ITEMS già presenti nel MERCHANT.

- AZZURRO: EXP verticale dal basso verso l'alto, riferita al requisito del prossimo livello.
- NERO: ritratto PG PLAYER. Bordo HP completamente verde a piena salute; si svuota in senso orario dall'angolo in basso a destra: inferiore, sinistro, superiore, destro.
- GIALLO: ABILITÀ ATTIVA PLAYER, grigia durante il CD; il colore ritorna da sinistra verso destra con il recupero.
- ROSSO: PASSIVA PLAYER. Accesa per la durata dell'effetto, sempre accesa per le passive permanenti e flash di 0,5 s per i proc istantanei.
- VERDE CHIARO: tre SLOT BONUS, con la medesima visualizzazione del CD.
- GRIGIO: fino a tre ritratti PG IA a destra del PLAYER, con bordo HP analogo.
- BLU: abilità degli IA sopra i loro ritratti, con visualizzazione del CD.
- ROSA: quattro SLOT ITEM in basso a destra, con sprite MERCHANT, tasto e quantità.
- MARRONE: G della RUN in alto a destra.
- VIOLA: MOB previsti nell'AREA meno le KILL, compresi quelli ancora da generare; arriva a zero.
- VERDE SCURO: slot riservato inattivo in alto a sinistra.

Implementazione completata. L'HUD segue il PG attualmente controllato durante il subentro, offre nomi e valori al passaggio del mouse e conserva avvisi DOWN/RIANIMAZIONE e schermate BONUS. Il comando tecnico Riprova test resta nelle schermate di errore, sconfitta e fine test. La durata del CD memorizzata all'avvio evita che un upgrade alteri la proporzione del recupero già in corso. La visualizzazione non cambia danni, valori o tempi di gioco.

File principali: `Assets/PreGameplayLoop/LoopHUD.cs`, `LoopHUD.Run.cs`, `RunHudData.cs`, `AbilityCooldown.cs`, `BonusAbilityRuntime.cs`, runtime delle abilità PG e alcuni segnali di stato delle passive, `Assets/TestEngine/TestHUD.cs`. Test dedicato: `Assets/PreGameplayLoop/Editor/RunHudValidation.cs`. Documentazione aggiornata: sezione 29.5 del GDD e PreGameplayLoopPrototype.md.

### Verifiche HUD effettivamente svolte

- Compilazione C# runtime/editor superata.
- `RUN-HUD-validation-r3.txt`: PASS, 82 controlli in Unity 6000.3.16f1, zero warning. Comprende HP, EXP, CD, party di quattro PG, sette risorse ITEMS, stato delle passive, pausa, bonus e reset RUN.
- `HUD-loop-regression-r1.txt`: PASS, 394 asserzioni del gameplay loop in Play Mode, incluse transizioni e scelte BONUS. La fixture disabilita l'IA MOB: non equivale a una prova manuale completa di combattimento.
- Verifica visiva in Unity a finestra ridotta e massimizzata, con tre compagni, quattro ITEMS, HP ridotti, EXP parziale, bonus in CD e descrizione ITEM al passaggio del mouse.
- Prova terminata e scena HubPrototype ripristinata in Edit Mode.
- Limite deliberato: PG, abilità e passive hanno ancora icone provvisorie con sigle. Nessuna build generata.

## BOSS01

La scheda dettagliata di BOSS01 è in `Docs/Boss01Test.md`, `Assets/BossTest/BOSS01.asset` e nel relativo runtime; non era riportata integralmente nel GDD al momento della descrizione richiesta dall'utente. Non attribuire al GDD la scheda completa finché non vi viene consolidata.

BOSS della CITTÀ 1 dopo tre AREE ordinarie, arena di prova 60 × 40 m, collider diametro 5 m. HP 15000, ATK 80, DEF interna 100 (0% mitigazione), ATK SPD 50 (intervallo base 2 s), attivazione attacco base a 3 m tra bordi. Aggro sul PG attivo visibile più vicino; esclusi DOWN, MORTI e PET.

Sequenza speciali: 01 → 02 → 01 → 04. Intervallo calcolato dalla conclusione dello speciale, recupero incluso; per ATTACCO_02 dall'impatto dei massi. La carica riavvia il timer senza avanzare la sequenza.

### Ultima richiesta del proprietario

> riduci il tempo tra un attacco speciale e l'altro a 13s
> aumenta MOVE SPD a 95
> ATTACCO BASE esegui la HIT a 1s.
> ATTACCO_02 riduci il tempo per l'impatto a 2,5s
> ATTACCO_03 aumenta la velocità di movimento durante la carica a 10 m/s
> ATTACCO_04 aumenta la velocità dei proiettili a 13 m/s, aumenta la distanza percorsa dai proiettili a 15m

### Risultato implementato

| Parametro | Valore attuale |
|---|---|
| SpecialInterval | 13 s; nell'engine condiviso anche con l'avvio del primo speciale |
| MOVE SPD | 95, equivalente a 1,9 m/s |
| BaseHitAt | 1 s |
| RockDelay | 2,5 s |
| ChargeSpeed | 10 m/s |
| BileSpeed | 13 m/s |
| BileRange | 15 m dal bordo BOSS |

- BASE: arco 110°, profondità 4 m dal bordo; danno 80; fermo per 1,5 s, rotazione nel primo secondo, HIT a 1 s.
- ATTACCO_01: tre rami 2,5 × 12 m a 0°/±30°, preparazione 2 s, rotazione nel primo secondo, danno 70 una volta per PG, recupero 1 s.
- ATTACCO_02: fino a tre posizioni casuali di PG distinti fissate alla comparsa; cerchi raggio 3 m; impatto a 2,5 s, danno 40 e STUN 1 s; BOSS libero dopo 2 s. Impatti sovrapposti possono infliggere più HIT; STUN non sommato.
- ATTACCO_03: soglie 12000/9000/6000/3000 HP; aspetta l'attacco corrente, una sola carica se una HIT supera più soglie. Bersaglio casuale entro 18 m dai bordi, preparazione 1,5 s, direzione fissa, percorso 18 m a 10 m/s, larghezza 5 m. Danno 40 una volta per PG, respinta 3 m in 0,25 s; recupero 0,5 s oppure STUN 1,5 s urtando coperture.
- ATTACCO_04: cono 150°, dieci spicchi e dieci proiettili da sinistra a destra; primo sparo a 1 s, poi ogni 0,33 s. Velocità 13 m/s, percorso 15 m dal bordo, raggio proiettile 0,5 m, danno 30, non perforante. Veleno 5 HP/s per 3 s, applicazioni indipendenti; recupero 0,5 s.

I danni sono prima della DEF. Modificati esclusivamente `Assets/BossTest/BOSS01.asset`, `Assets/BossTest/BossDefinition.cs` e `Docs/Boss01Test.md` per questa richiesta. Compilazione runtime e controllo del diff superati; **nessuna nuova prova Play Mode del bilanciamento BOSS**, nessuna build o commit. I report HUD precedono quest'ultimo aggiornamento e non lo validano.

## Stato finale da cui continuare

HUD implementato e verificato; aggiornamento numerico BOSS01 implementato e compilato. L'ultima richiesta è conservare questa conversazione nella repository per il cambio PC. Prima di cambiare PC rimangono da fare commit e push dei file che il proprietario desidera trasferire. Questo documento non autorizza automaticamente nuove modifiche, test estesi, build o pubblicazioni.
