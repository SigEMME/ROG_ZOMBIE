# ROG ZOMBIE — Game Design Document

Versione consolidata — 11/09/2026 — aggiornamento SCONFITTA, CD REDUCTION, DEF/SCUDO, EFFETTI PERSISTENTI, PROF e CHEST  
Destinazione nel repository Unity: `Docs/ROG_ZOMBIE_GDD.md`  
Fonti: `ROG_ZOMBIE_WORLD_2026-09-08`, documento «ROG ZOMBIE — WORLD — Documento di riferimento»; GDD con revisione CD REDUCTION; conversazione «Funzionamento ASTRA» (`6a9fce64-149c-83ed-ade6-79f191001021`).

Questo GDD raccoglie integralmente le specifiche presenti nella fonte, organizzate in sezioni e tabelle Markdown. La base documentale più recente disponibile è il GDD aggiornato del 09/09/2026 (file locale salvato alle 20:22), derivato dal GDD dell’08/09/2026 con revisione CD REDUCTION. Le successive regole confermate nella conversazione «Funzionamento ASTRA», fino alle conferme sulla classificazione tecnica di ABILITÀ PG, PASSIVE PG, ABILITÀ BONUS, ITEMS e attacchi/effetti MOB, sugli stati logici, sulle collisioni DOWN/MORTE e ZOMB05, su PROJECTILE_MOB ↔ PROJECTILE_MOB, RIANIMAZIONE, NPC HUB e assenza di TAG tecnici dedicati, prevalgono sui dati precedenti incompatibili. Le note aggiunte per evidenziare lacune e ambiguità sono distinte dalle regole di gioco e non introducono nuove meccaniche.

## Stato delle specifiche e uso nello sviluppo

- **DEFINITO**: regole e valori esplicitamente stabiliti nelle sezioni 1–31, salvo indicazioni di provvisorietà o punti DA DEFINIRE.
- **DA DEFINIRE**: informazioni assenti, idee non definitive e ambiguità esplicitamente segnalate. Non costituiscono valori predefiniti da implementare.
- Conservare nomi, unità, percentuali, soglie e valori tabellari. Non correggere o ribilanciare automaticamente valori apparentemente insoliti.
- Le tabelle di MOB per AREA e di EXP richiesta sono riportate fedelmente. Dove una regola testuale non consente di riprodurle senza ambiguità, usare i valori espliciti per le voci documentate e lasciare DA DEFINIRE l’estensione della regola.
- La sezione 30 raccoglie il lavoro rimanente indicato dalla fonte; la sezione 32 esplicita ulteriori chiarimenti necessari emersi dalla lettura.
- Questa conversione conserva il contenuto consolidato disponibile; non attribuisce date o revisioni a modifiche per cui la fonte non fornisce una cronologia.

## Indice
- [1. CONCEPT](#sezione-1)
- [2. GAMEPLAY LOOP](#sezione-2)
- [3. MONDO DI GIOCO E LORE](#sezione-3)
- [4. CITTÀ E AREE](#sezione-4)
- [5. STRUTTURA DELLE AREE](#sezione-5)
- [6. SISTEMA DI AVANZAMENTO TRA LE AREE](#sezione-6)
- [7. HUB](#sezione-7)
- [8. PREPARAZIONE RUN](#sezione-8)
- [9. ITEMS](#sezione-9)
- [10. SISTEMA DI COMBATTIMENTO](#sezione-10)
- [11. ROSTER PG — STATS BASE](#sezione-11)
- [12. ABILITÀ E PASSIVE DEI PG](#sezione-12)
- [13. MOB](#sezione-13)
- [14. MOB PER AREA](#sezione-14)
- [15. SISTEMA DI SPAWN](#sezione-15)
- [16. SISTEMA DI DROP](#sezione-16)
- [17. SISTEMA DI PROGRESSIONE EXP / LVL](#sezione-17)
- [18. SISTEMA BONUS — REGOLE GENERALI](#sezione-18)
- [19. CHEST](#sezione-19)
- [20. LEVEL UP](#sezione-20)
- [21. COMPLETAMENTO AREA](#sezione-21)
- [22. ABILITÀ BONUS — STATS](#sezione-22)
- [23. RATE DI APPARIZIONE — NUOVE ABILITÀ BONUS](#sezione-23)
- [24. UPGRADE ABILITÀ BONUS — REGOLE](#sezione-24)
- [25. RATE DEI SINGOLI UPGRADE](#sezione-25)
- [26. MEDI KIT](#sezione-26)
- [27. PROF — UPGRADE PERMANENTI](#sezione-27)
- [28. DOWN / MORTE / RESURREZIONE](#sezione-28)
- [29. VITTORIA / SCONFITTA / FINE RUN](#sezione-29)
- [30. STATO DI SVILUPPO — PUNTI RIMANENTI](#sezione-30)
- [31. REGOLE CONSOLIDATE DA NON DIMENTICARE](#sezione-31)
- [32. Chiarimenti necessari — DA DEFINIRE](#sezione-32)

<a id="sezione-1"></a>

## 1. CONCEPT

- Genere: rogue-lite in pixel art con visuale isometrica.
- Atmosfera: post-apocalittica, dark e frenetica.
- Combattimento con armi umane realistiche contro zombie e altri MOB.
- CO-OP fino a 4 giocatori: requisito fondamentale del progetto.
- La PARTY è sempre composta da 4 PG. In SINGLE PLAYER i PG non controllati dal PLAYER sono gestiti dall'AI e seguono il giocatore.
- La progressione della RUN è lineare: non esiste una scelta del percorso tra le AREE.
- Obiettivo generale: riconquistare le città, eliminare le orde, trovare superstiti e cercare una soluzione/cura all'apocalisse.

<a id="sezione-2"></a>

## 2. GAMEPLAY LOOP

- HUB → selezione PARTY/configurazione → partenza RUN → attraversamento delle AREE → BOSS della città → scelta tra proseguire o tornare all'HUB.
- Ogni AREA contiene un numero definito di MOB. L'AREA viene completata eliminando tutti i MOB previsti.
- Una volta completata l'AREA, la PARTY raggiunge l'uscita, riceve la scelta BONUS di fine AREA e passa alla nuova AREA.
- Entrando in una nuova AREA non è possibile tornare indietro.
- Dopo la vittoria contro il BOSS della città la decisione di PROSEGUIRE nella città successiva oppure TORNARE ALL’HUB spetta all’HOST; solo nella schermata dell’HOST compare il tasto per tornare all’HUB.
- Se tutta la PARTY viene sconfitta, la RUN termina.

<a id="sezione-3"></a>

## 3. MONDO DI GIOCO E LORE

- L'apocalisse è stata causata da un esperimento scientifico.
- È trascorso circa 1 anno dall'inizio dell'apocalisse.
- Le città sono state abbandonate e invase dagli zombie.
- I superstiti vivono principalmente sottoterra.
- Possono esistere livelli/quest bonus sotterranei in cui liberare superstiti, completare obiettivi e sbloccare NPC o contenuti dell'HUB.
- Sono previsti zombie umani e animali e altre varianti di MOB.
- L'idea dello scienziato responsabile come possibile BOSS finale esiste, ma non è ancora definitiva.

<a id="sezione-4"></a>

## 4. CITTÀ E AREE

| CITTÀ | BIOMA / AMBIENTE | N° AREE |
| --- | --- | --- |
| CITTÀ 1 | Campagna | 3 |
| CITTÀ 2 | Residenziale / periferia | 4 |
| CITTÀ 3 | Industriale | 5 |
| CITTÀ 4 | Fogne / sotterraneo | 5 |
| CITTÀ 5 | Metropolitana | 6 |

- Totale attuale: 23 AREE.
- L'ultima AREA di ogni CITTÀ contiene il BOSS.
- Le AREE possono includere EVENTI SPECIALI, orde maggiori, MINI BOSS e obiettivi speciali.
- Il numero e la struttura generale delle città sono definiti come base di progetto, ma nomi propri, mappe e dettagli ambientali sono ancora da sviluppare.

<a id="sezione-5"></a>

## 5. STRUTTURA DELLE AREE

- Ogni AREA è una singola grande zona esplorabile, non una sequenza di stanze separate.
- Il PLAYER può muoversi liberamente all'interno dell'AREA.
- La porzione inizialmente visibile dell'AREA è libera da MOB.
- Per avanzare è necessario eliminare tutti i MOB previsti per l'AREA.
- Il passaggio alla nuova AREA blocca il ritorno alla precedente.
- MEDI KIT e altri elementi possono essere collocati casualmente per incentivare l'esplorazione.

<a id="sezione-6"></a>

## 6. SISTEMA DI AVANZAMENTO TRA LE AREE

- L’AREA è COMPLETATA quando l’ultimo MOB previsto entra effettivamente in MORTE; per ZOMB05 questo avviene dopo l’ESPLOSIONE.
- Si attiva l’USCITA. Se è OFF-SCREEN, un’ICONA DIREZIONALE ne indica la direzione; se è visibile non occorre un’indicazione aggiuntiva.
- Tutti i PG VIVI devono essere contemporaneamente nel TRIGGER dell’USCITA, di forma CIRCOLARE con RAGGIO 4 m. L’USCITA AREA usa TRIGGER_PG: rilevamento senza collisione fisica (sezione 10.9).
- Finché manca un PG VIVO non succede nulla: i PG possono entrare e uscire liberamente dal TRIGGER.
- I PG in DOWN impediscono il passaggio e devono essere RESUSCITATI prima. I PG in MORTE non devono raggiungere l’USCITA e non bloccano la transizione.
- Soddisfatta la condizione, si apre la SCELTA BONUS di fine AREA e il GAMEPLAY viene completamente BLOCCATO: niente movimento, attacchi, ABILITÀ o ITEMS.
- Ogni PLAYER sceglie 1 BONUS tra 5: 3 BONUS STATS BASE + 2 BONUS relativi alle ABILITÀ BONUS.
- Per ciascun PG IA, il BONUS DI FINE AREA è scelto dal PLAYER responsabile.
- Al completamento dell’AREA dopo il BOSS, la decisione tra PROSEGUIRE e TORNARE ALL’HUB spetta all’HOST. Solo nella schermata dell’HOST compare il tasto TORNA ALL’HUB.
- Dopo la scelta e CONFERMA di tutti i PLAYER, una breve SCHERMATA DI CARICAMENTO prepara NUOVA AREA e FIRST SPAWN, applicando le regole SPAWN. Nello stesso caricamento vengono determinate/generate CHEST e MEDI KIT secondo le sezioni 19 e 26.
- Terminato il caricamento, il FIRST SPAWN è già presente OFF-SCREEN e il GAMEPLAY riprende.
- I PG VIVI recuperano il 15% degli HP MASSIMI correnti, senza superarli. I PG resuscitati dal DOWN prima del passaggio rientrano in questa categoria.
- I PG in MORTE vengono RESUSCITATI automaticamente al 50% degli HP MASSIMI correnti: non ricevono anche il 15%.
- Quando il proprio PG viene RESUSCITATO al CAMBIO AREA, il PLAYER in SPETTATORE esce automaticamente da tale stato: la visuale torna al proprio PG e il PLAYER ne riprende il controllo diretto (sezione 28).
- Non è possibile tornare nell’AREA precedente. L’EXP DROP continua a crescere anche passando a una nuova CITTÀ e non si resetta.

<a id="sezione-7"></a>

## 7. HUB

- L'HUB è una piccola zona sicura ed esplorabile tra le RUN.
- MERCHANT: acquisto e gestione degli ITEMS tramite G.
- PROF: acquisto degli UPGRADE permanenti tramite G.
- Ogni PLAYER ha un saldo G personale; gli acquisti e gli UPGRADE sono personali per ogni PLAYER. L’ambito degli UPGRADE PROF è definito nella sezione 27.
- RECLUTATORE: NPC necessario per sbloccare nuovi PG; viene sbloccato al completamento di CITTÀ 1 e inserito permanentemente nell’HUB.
- Dal momento dello sblocco del RECLUTATORE, PG05, PG06, PG07 e PG08 sono tutti immediatamente acquistabili a 5000 G ciascuno, senza ordine obbligatorio. Ogni PG acquistato rimane permanentemente sbloccato.
- EXIT: accesso alla preparazione e partenza della RUN tramite TRIGGER_PG e interazione con F, senza blocco fisico.
- Gli NPC dell’HUB, inclusi RECLUTATORE, MERCHANT e PROF, hanno collisione fisica sul layer OSTACOLO e interazione con F tramite TRIGGER_PG. Ereditano le collisioni di OSTACOLO: bloccano PG e PET e collidono anche con PROJECTILE_PG e PROJECTILE_MOB. Nessun layer NPC o TAG dedicato.
- La preparazione mostra 4 BANNER PG per la PARTY.
- Il ROSTER contiene 8 PG. I PG confermati in SELEZIONE PG risultano BLOCKED; quelli non ancora sbloccati sono mostrati come silhouette.
- Tutti i PLAYER devono confermare la preparazione prima della partenza.

<a id="sezione-8"></a>

## 8. PREPARAZIONE RUN

### 8.1 ACCESSO E SCHERMATA PRINCIPALE

- Nell’HUB, interagendo con F nel TRIGGER_PG dell’USCITA/EXIT si apre direttamente PREPARAZIONE RUN.
- La PARTY è sempre di 4 PG. La schermata mostra contemporaneamente la configurazione di tutti e 4.
- I colori seguenti identificano gli elementi dei mockup di riferimento; non fissano la grafica definitiva.

| RIFERIMENTO | ELEMENTO / COMPORTAMENTO |
| --- | --- |
| NERO | Bordo dello schermo. |
| ROSSO | 4 BANNER PG, uno per posto della PARTY; click sul proprio BANNER apre SELEZIONE PG. |
| BLU | NOME PLAYER; per un PG IA viene mostrato il NOME PG. |
| VERDE | SLOT ITEMS disponibili per i PG controllati direttamente dai PLAYER: numero individuale in base agli sblocchi, da 1 a 4; può differire fra membri della PARTY. I PG IA non hanno ITEMS né SLOT ITEM utilizzabili. |
| GIALLO | SLOT ABILITÀ ATTIVA selezionata. |
| ROSA | SLOT PASSIVA ATTIVA selezionata. |
| GRIGIO | CONFERMA della preparazione, necessaria per avviare la RUN. |
| VIOLA | INDIETRO: chiude PREPARAZIONE RUN e riporta all’HUB, conservando le modifiche effettuate fino a quel momento. |

### 8.2 SELEZIONE PG — UX

- PG01, PG02, PG03 e PG04 sono disponibili fin dall’inizio; PG05, PG06, PG07 e PG08 sono inizialmente BLOCCATI.
- Dopo il completamento di CITTÀ 1, il RECLUTATORE permanente nell’HUB consente di acquistare immediatamente qualsiasi PG tra PG05–PG08 a 5000 G ciascuno, senza ordine obbligatorio. Lo sblocco di ciascun PG acquistato è permanente.

| RIFERIMENTO | ELEMENTO / COMPORTAMENTO |
| --- | --- |
| NERO | Bordo dello schermo. |
| ROSSO | CASELLE degli 8 PG del ROSTER. Click su una CASELLA aggiorna anteprima e informazioni del PG. PG non sbloccati in silhouette; PG occupati con ICONA BLOCKED. |
| VERDE | ANTEPRIMA: DESCRIZIONE a sinistra, ANTEPRIMA GRAFICA del PG a destra. |
| AZZURRO | Macro sezione STATS, ABILITÀ e PASSIVE. |
| BLU | STATS visibili: HP / ATK / DEF / MOVE SPD / ATK SPD / CD REDUCTION. RANGE è NASCOSTA e non viene mostrata. |
| GIALLO | 2 CASELLE ABILITÀ; quella cliccata si illumina e viene selezionata. |
| ROSA | 2 CASELLE PASSIVA; quella cliccata si illumina e viene selezionata. |
| ARANCIONE | DESCRIZIONE dell’ultima ABILITÀ/PASSIVA cliccata o selezionata. |
| VIOLA | INDIETRO: torna a PREPARAZIONE RUN conservando PG, ABILITÀ e PASSIVA selezionati, senza confermarli. La configurazione resta DA CONFERMARE e non consente la partenza; il PG resta libero fino alla CONFERMA. |
| GRIGIO | CONFERMA PG + ABILITÀ + PASSIVA e ritorno a PREPARAZIONE RUN. |

- Ogni PG dispone di 2 ABILITÀ e 2 PASSIVE: selezionarne esattamente 1 di ciascuna per la RUN. ABILITÀ con Q per il PG controllato direttamente; per i PG IA, SPACE BAR + numero 1–3 secondo l’ordine dei PG IA assegnati al PLAYER responsabile. PASSIVA automatica, identica per PG IA e PG controllati direttamente, senza occupare SLOT BONUS.
- CONFERMA è utilizzabile solo con 1 PG valido + 1 ABILITÀ + 1 PASSIVA selezionati.
- Cliccando la CASELLA di un altro PG si azzerano ABILITÀ e PASSIVA selezionate e si aggiornano le opzioni: entrambe devono essere scelte di nuovo.
- La CONFERMA registra la configurazione, rende il PG BLOCKED e riporta a PREPARAZIONE RUN; il BANNER mostra la configurazione confermata.

### 8.3 BLOCKED, SELEZIONI SIMULTANEE E RIAPERTURA

- La sola selezione/anteprima non riserva il PG. Diventa BLOCKED per gli altri alla CONFERMA in SELEZIONE PG.
- Due PLAYER possono configurare temporaneamente lo stesso PG: lo ottiene chi preme CONFERMA per primo.
- La CASELLA si aggiorna immediatamente con ICONA BLOCKED anche per chi lo stava configurando; il secondo PLAYER non può più confermarlo. Non è definita una sostituzione automatica con un altro PG.
- Riaprendo SELEZIONE PG dal proprio BANNER, il PG precedentemente confermato cessa immediatamente di essere BLOCKED. ABILITÀ e PASSIVA restano selezionate finché non si cambia PG.

### 8.4 ITEMS — DRAG & DROP

- Click e pressione mantenuta di LMB sull’ITEM → trascinamento su un altro SLOT ITEMS disponibile → rilascio di LMB.
- Destinazione vuota: spostamento. Destinazione occupata: scambio di posizione dei due ITEMS.
- È consentito solo il RIORDINO; contenuto e quantità non cambiano. Gli ITEMS non possono essere rimossi in questa schermata.
- Acquisto, vendita e liberazione degli SLOT avvengono al MERCHANT nell’HUB. Restano validi i limiti per SLOT e SCORTA ESPLOSIVA.

### 8.5 PLAYER E IA

- ORDINE PLAYER in partita: l’HOST è sempre PLAYER 1; gli altri PLAYER sono numerati secondo l’ordine di ingresso in partita.

| PLAYER | PG IA | RESPONSABILITÀ DELLA PREPARAZIONE IA |
| --- | --- | --- |
| 4 | 0 | Ogni PLAYER configura il proprio PG. |
| 3 | 1 | IA assegnata all’HOST. |
| 2 | 2 | 1 IA assegnata a ciascun PLAYER. |
| 1 | 3 | Tutte le IA fanno capo all’unico PLAYER, che gestisce e conferma anche i 3 PG IA; controlla direttamente 1 PG. |

- In caso di abbandono durante PREPARAZIONE, il PG del PLAYER uscente cessa di essere BLOCKED; il posto è sostituito da IA.
- La responsabilità IA viene riassegnata secondo il numero di PLAYER rimasti. Il PLAYER che riceve la nuova IA deve effettuare e CONFERMARE la relativa SELEZIONE PG.
- Durante la RUN, se un PLAYER abbandona o si disconnette, il PG che controllava direttamente diventa automaticamente PG IA. Questo PG e gli eventuali PG IA già di sua responsabilità vengono riassegnati agli altri PLAYER secondo le regole di ASSEGNAZIONE IA già stabilite, in base al nuovo numero di PLAYER presenti. Perdita dei progressi e trattamento del G del PLAYER uscente: sezione 29.3.

#### PG IA — EQUIPAGGIAMENTO, PROGRESSIONE E RESPONSABILITÀ

- I PG IA non hanno ITEMS né SLOT ITEM utilizzabili.
- L’ABILITÀ selezionata del PG IA viene attivata dal PLAYER responsabile con SPACE BAR + numero da 1 a 3, secondo l’ordine dei PG IA assegnati a quel PLAYER. Con un solo PG IA il comando è SPACE BAR + 1. L’IA non decide autonomamente quando attivarla.
- La PASSIVA selezionata funziona in modo identico e automatico rispetto a un PG controllato direttamente da un PLAYER.
- Il PLAYER responsabile sceglie i BONUS di LEVEL UP e i BONUS DI FINE AREA di ciascun PG IA assegnato.
- Il PG IA riceve automaticamente il proprio BONUS CHEST con le stesse regole degli altri PG, senza una scelta specifica.
- EXP, LVL, BONUS e progressione individuale durante la RUN sono identici a quelli degli altri PG.
- I PG IA non hanno volontà propria: non cercano autonomamente MEDI KIT o CHEST e possono usarli solo incontrandoli durante il normale movimento, secondo le rispettive regole di utilizzo.
- DOWN, RESURREZIONE e CAMBIO CONTROLLO sono definiti nella sezione 28.
- Il comportamento generale dell’IA resta DA DEFINIRE e sarà affrontato successivamente.

### 8.6 CONFERMA, ANNULLAMENTO E AVVIO RUN

- La CONFERMA di PREPARAZIONE RUN è distinta dalla CONFERMA di SELEZIONE PG.
- Dopo CONFERMA PREPARAZIONE, il PLAYER non può effettuare azioni/modifiche alla configurazione. Può ANNULLARE la CONFERMA per tornare a modificarla.
- La RUN parte quando tutti i PLAYER risultano contemporaneamente CONFERMATI, con le selezioni dei PG IA di loro competenza confermate.
- La PARTY viene bloccata e si apre una SCHERMATA DI CARICAMENTO: generazione AREA iniziale, MOB del FIRST SPAWN, prima CHEST garantita, eventuale seconda CHEST e MEDI KIT.
- Il FIRST SPAWN rispetta le sezioni 14–15; CHEST e MEDI KIT le sezioni 19 e 26. Terminato il caricamento, inizia il GAMEPLAY.
- La RUN parte da LVL 1; i CD delle ABILITÀ iniziano IN CD secondo la sezione 10.4.
- All’inizio di una RUN CO-OP si applica il BONUS CHEST RATE più alto tra i PLAYER presenti, secondo la sezione 19.
- Animazioni, suoni, aspetto definitivo delle icone e dimensioni dei pannelli restano da realizzare in sviluppo/testing.

<a id="sezione-9"></a>

## 9. ITEMS

**Stato: definitivamente CONFERMATO.**

- I PG IA non hanno ITEMS né SLOT ITEM utilizzabili; le regole degli SLOT ITEM seguenti riguardano i PG controllati direttamente dai PLAYER.

| ITEM | AREA | DURATA | EFFETTO | DANNO / CURA | COSTO |
| --- | --- | --- | --- | --- | --- |
| MOLOTOV | Circolare, raggio 5 m | 5 s | Area incendiata | 5 HP/s | 50 G |
| GRANATA | Circolare, raggio 2,5 m | Istantaneo | Esplosione | 40 HP | 50 G |
| SMOKE | Circolare, raggio 5 m | 4 s | Tutti i PG nell’AREA INVISIBILI | — | 50 G |
| POZIONE CURATIVA | Circolare, raggio 4 m | 4 s | Cura i PG nell’AREA; ignora i MOB | 10 HP/s | 100 G |
| TRAPPOLA | Rettangolare 1 × 4 m | 5 s | Slow 40% | 5 HP/s | 50 G |

- SLOT ITEM di partenza: 1.
- UPGRADE massimi SLOT ITEM: 3.
- SLOT ITEM massimi: 4.
- Gli ITEMS vengono acquistati dal MERCHANT e assegnati direttamente agli SLOT: non esiste un inventario separato.
- Per liberare uno SLOT, l'ITEM può essere venduto al MERCHANT al 50% del prezzo di acquisto.
- Gli ITEMS possono essere solamente riordinati durante la PREPARAZIONE RUN, tramite drag & drop LMB e scambio se lo SLOT è occupato (sezione 8.4).
- Gli ITEMS inutilizzati vengono persi in caso di sconfitta della RUN; vengono mantenuti se si torna volontariamente all'HUB dopo un BOSS.

- LIMITE GENERALE: ogni SLOT ITEM contiene normalmente 1 ITEM, per tutti i PG che dispongono di SLOT ITEM; i PG IA ne sono esclusi. Il limite riguarda il singolo SLOT, non il totale posseduto dal PG.
- Eccezione — PG04 / SCORTA ESPLOSIVA: fino a 3 GRANATE oppure 3 MOLOTOV dello stesso tipo per SLOT; gli altri ITEMS restano a 1 per SLOT.

### 9.1 UTILIZZO, MIRA E CONSUMO — CONFERMATO

- I tasti ITEM 1 / 2 / 3 / 4 corrispondono rispettivamente agli SLOT ITEM 1 / 2 / 3 / 4 del PG controllato dal PLAYER.
- L’ITEM viene utilizzato tramite il tasto dello SLOT corrispondente.
- Per gli ITEMS che richiedono MIRA/POSIZIONAMENTO, mantenendo premuto il tasto corrispondente viene mostrata l’ANTEPRIMA dell’AREA D’EFFETTO, posizionabile con il CURSORE; al rilascio del tasto l’ITEM viene utilizzato/lanciato.
- Dopo l’uso l’ITEM viene consumato e lo SLOT resta VUOTO. Rimane valida SCORTA ESPLOSIVA di PG04: ogni utilizzo consuma 1 unità e lo SLOT si svuota al consumo dell’ultima unità.

### 9.2 CENTRO, RANGE E ORIENTAMENTO — CONFERMATO

- Tutti gli ITEMS hanno come centro dell’AREA la posizione del CURSORE al rilascio, salvo il limite di RANGE della TRAPPOLA.
- MOLOTOV, GRANATA, SMOKE e POZIONE CURATIVA non hanno RANGE massimo di attivazione.
- TRAPPOLA: RANGE massimo 7 m dal PG. Se il CURSORE è entro il RANGE, il centro coincide con il CURSORE; se è oltre il RANGE, anteprima e posizionamento si fermano a 7 m dal PG lungo la direzione PG→CURSORE.
- La TRAPPOLA è orientata lungo la traiettoria PG→CURSORE, con il lato da 4 m come FRONTALE.

### 9.3 BERSAGLI ED EFFETTI — CONFERMATO

- Nessun FRIENDLY FIRE: gli effetti offensivi e di controllo degli ITEMS non colpiscono i PG; gli effetti benefici non influenzano i MOB.
- MOLOTOV: colpisce solo i MOB; il DANNO parte al contatto con l’AREA INCENDIATA e infligge 5 DANNO ogni secondo finché il MOB rimane nell’AREA. Durata dell’AREA: 5 s.
- GRANATA: una singola HIT da 40 DANNO a tutti i MOB presenti nell’AREA valida.
- SMOKE: rende INVISIBILI tutti i PG nell’AREA e segue integralmente le regole di INVISIBILITÀ già definite nel GDD.
- POZIONE CURATIVA: cura i PG nell’AREA di 10 HP ogni secondo per 4 s e ignora completamente i MOB.
- TRAPPOLA: colpisce solo i MOB nell’AREA valida, infliggendo 5 DANNO ogni secondo e SLOW 40% durante la permanenza; durata massima dell’AREA: 5 s.

### 9.4 AREE, MURI E OSTACOLI — CONFERMATO

| ITEM | GEOMETRIA E SUDDIVISIONE | MURI / OSTACOLI |
| --- | --- | --- |
| GRANATA | Circolare, raggio 2,5 m; 4 SPICCHI da 90°. | Influenzano l’AREA. |
| MOLOTOV | Circolare, raggio 5 m; 8 SPICCHI da 45°. | Influenzano l’AREA. |
| TRAPPOLA | Rettangolare 1 × 4 m; 4 SEZIONI da 1 × 1 m. | Influenzano l’AREA. |
| SMOKE | Circolare, raggio 5 m. | Nessun effetto sull’AREA. |
| POZIONE CURATIVA | Circolare, raggio 4 m. | Nessun effetto sull’AREA. |

- Per GRANATA, MOLOTOV e TRAPPOLA vale la stessa regola delle altre AREE: se un MURO/OSTACOLO intercetta anche parzialmente uno SPICCHIO/SEZIONE, quello SPICCHIO/SEZIONE viene eliminato integralmente e non genera alcun effetto. Gli altri SPICCHI/SEZIONI restano validi.
- La suddivisione è soltanto geometrica e non genera HIT aggiuntive.
- MURI/OSTACOLI non hanno effetto su SMOKE e POZIONE CURATIVA.
- Non è previsto alcun comportamento specifico aggiuntivo di lancio/posizionamento rispetto a MURI/OSTACOLI, oltre alle regole delle AREE sopra definite.

### 9.5 PROMEMORIA — SBLOCCHI TRAMITE QUEST

- Alcuni ITEMS acquistabili presso il MERCHANT devono essere sbloccati tramite QUEST.
- Quali ITEMS e quali QUEST restano DA DEFINIRE; questo promemoria non riapre le regole di funzionamento ITEMS confermate.

### 9.6 CLASSIFICAZIONE TECNICA ITEMS — CONFERMATO

I cinque ITEMS usano esclusivamente i layer esistenti AREA_EFFECT_MOB e AREA_EFFECT_PG, senza nuovi LAYER o TAG. Gli effetti e gli oggetti grafici non costituiscono ostacoli fisici. Restano invariati comandi, valori, tick, durata, bersagli e regole delle sezioni 9.1–9.4.

| ITEM | GESTIONE TECNICA |
| --- | --- |
| GRANATA | Nessun proiettile fisico; effetto istantaneo AREA_EFFECT_MOB nel punto scelto. I 4 SPICCHI da 90° sono validati logicamente contro MURO/OSTACOLO. |
| MOLOTOV | AREA_EFFECT_MOB persistente per 5 s, attraversabile da PG, MOB, PET e proiettili. Gli 8 SPICCHI da 45° sono validati logicamente contro MURO/OSTACOLO. |
| SMOKE | AREA_EFFECT_PG persistente per 4 s, senza collisione fisica; rileva solo PG e applica logicamente INVISIBILITÀ. Ignora MURO/OSTACOLO. |
| POZIONE CURATIVA | AREA_EFFECT_PG persistente per 4 s, senza collisione fisica; rileva solo PG e applica logicamente la CURA secondo i tick definiti. Ignora MURO/OSTACOLO. |
| TRAPPOLA | AREA_EFFECT_MOB persistente per 5 s, attraversabile; le 4 SEZIONI da 1 × 1 m sono validate logicamente contro MURO/OSTACOLO. Applica DANNO e SLOW ai MOB nelle sezioni valide. |

<a id="sezione-10"></a>

## 10. SISTEMA DI COMBATTIMENTO

- Movimento: W-A-S-D.
- Mira: mouse.
- ATTACCO BASE: LMB; tenendo premuto LMB si mantiene il fuoco continuo.
- INTERAZIONE: F.
- ITEMS del PG controllato: tasti 1 / 2 / 3 / 4 per i rispettivi SLOT; pressione mantenuta per ANTEPRIMA AREA e MIRA/POSIZIONAMENTO, rilascio per utilizzo/lancio, consumo secondo la sezione 9.1.
- ABILITÀ selezionata del PG controllato direttamente: Q.
- ABILITÀ dei PG IA: attivazione da parte del PLAYER responsabile con SPACE BAR + numero 1–3, secondo l’ordine dei PG IA assegnati.
- Munizioni infinite e nessun reload.
- Gli ATTACCHI BASE seguono il comportamento definito per la singola arma: AREA HITSCAN, HITSCAN + AREA IMPATTO oppure PROIETTILI FISICI, secondo la classificazione della sezione 10.1. Gli attacchi con proiettile fisico hanno un tempo di viaggio e non sono genericamente istantanei.
- Regola base del danno: DANNO finale = ATK × (1 − DEF%), arrotondato all’intero più vicino con arrotondamento matematico (18,4 → 18; 18,5 → 19).
- DEF massima: 90%, anche per la DEF propria dello SCUDO.
- SCUDO è un elemento separato dal PLAYER, con propria DEF%: mitiga per primo il DANNO in ingresso; il residuo viene poi mitigato dalla DEF del PLAYER. Arrotondare soltanto il DANNO FINALE dopo entrambe le mitigazioni.
- ATK SPD: 100 = 1 colpo/s.
- MOVE SPD: 100 = 2 m/s.
- CD REDUCTION: STAT modificatore del PG, senza unità in secondi; VALORE BASE neutro = 100. CD REDUCTION 100 = 100% del CD BASE della singola ABILITÀ selezionata.
- Ogni ABILITÀ mantiene il proprio CD BASE in secondi, definito nella relativa scheda PG.
- Formula: CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100).
- CD REDUCTION non può scendere sotto 10.
- La STAT CD REDUCTION usa un valore intero: applicare il valore pieno risultante dal calcolo con arrotondamento matematico all’intero più vicino; frazione <0,5 per difetto, frazione ≥0,5 per eccesso (94,4 → 94; 94,5 → 95). Questa regola è distinta dall’arrotondamento del CD FINALE al decimo di secondo.
- Il CD FINALE viene arrotondato al decimo di secondo con arrotondamento matematico: cifra successiva inferiore a 5 per difetto, da 5 in su per eccesso (6,51 s → 6,5 s; 6,58 s → 6,6 s).
- Esempio: CD BASE ABILITÀ 10 s e CD REDUCTION 90 → CD FINALE 9 s. Un valore CD REDUCTION più basso riduce il cooldown.
- RANGE: 100 = 1 m nel sistema interno.
- DEF interno: 100 corrisponde a 0% di modificatore mostrato al PLAYER; ogni +1/−1 punto interno equivale a +1/−1 punto percentuale. Percentuale mostrata = DEF interna − 100; per la formula del DANNO, DEF% = (DEF interna − 100) / 100. Esempi: DEF 115 = +15%; DEF 80 = −20%. Resta invariato il cap DEF 90%.
- I BONUS PROF si sommano alla DEF BASE del PG. I BONUS CHEST e i BONUS STATS DI FINE AREA sono calcolati sul VALORE ATTUALE della STAT al momento dell’acquisizione, comprensivo degli UPGRADE permanenti del PROF e di tutti i precedenti BONUS STATS acquisiti durante la RUN, sia da CHEST sia da FINE AREA; le PASSIVE influenzano le STATS CORRENTI al momento dell’applicazione.

### 10.1 ATTACCHI E PROIETTILI — REGOLE GENERALI

- LMB mantenuto premuto: l’ARMA attacca automaticamente rispettando ATK SPD.
- ATTACCHI/s = ATK SPD / 100; intervallo tra ATTACCHI = 100 / ATK SPD, in secondi.
- La mira segue il CURSORE del mouse, indipendentemente dalla direzione di MOVIMENTO.
- Salvo eccezioni specifiche, un PROIETTILE non definito PERFORANTE si arresta e scompare all’IMPATTO con il primo MOB, dopo aver applicato il DANNO.
- I PROIETTILI BALISTICI standard scompaiono al limite del proprio RANGE se non incontrano prima un bersaglio o un elemento che li blocca.
- Le proprietà specifiche delle singole armi e ABILITÀ prevalgono sulle regole generali.

#### CLASSIFICAZIONE ATTACCHI BASE

| PG | ARMA | TIPO ATTACCO BASE |
| --- | --- | --- |
| PG01 | SHOTGUN | AREA HITSCAN; CONO istantaneo, nessun singolo proiettile. |
| PG02 | ASSAULT RIFLE | PROIETTILE FISICO. |
| PG03 | SNIPER RIFLE | PROIETTILE FISICO. |
| PG04 | GRENADE LAUNCHER | HITSCAN + AREA IMPATTO; nessun proiettile fisico in volo. |
| PG05 | KNIFE | AREA HITSCAN; geometria già definita, HIT istantanea. |
| PG06 | REVOLVER | PROIETTILE FISICO. |
| PG07 | BOW | PROIETTILE FISICO. |
| PG08 | HEAVY MACHINE GUN | PROIETTILE FISICO. |

#### BALISTICA GENERALE — PROIETTILI FISICI DEI PG

- PROIETTILE BALISTICO significa PROIETTILE FISICO: esiste nel mondo di gioco e percorre una distanza; non è HITSCAN.
- PROJECTILE SPD è una STAT NASCOSTA dell’ARMA, espressa in m/s e non mostrata al PLAYER. Standard iniziale per PG02, PG03, PG06, PG07 e PG08: **20 m/s**.
- Esporre PROJECTILE SPD nell’INSPECTOR dell’ENGINE separatamente per ciascuna arma/PG, modificabile durante i test senza intervenire sul codice. 20 m/s è il valore iniziale di test, non un bilanciamento definitivo differenziato per arma.
- SPAWN dalla BOCCA DELL’ARMA, mediante un punto di generazione sulla sua posizione effettiva.
- Al momento dello sparo si determina la direzione dal punto di SPAWN alla posizione del CURSORE sul PIANO DI GIOCO.
- Traiettoria rettilinea sul PIANO DI GIOCO, senza componente verticale durante il volo. Dopo lo sparo la direzione resta fissa e non segue i movimenti successivi del CURSORE.
- Il CURSORE determina solo la direzione: se è più vicino del RANGE massimo, il PROIETTILE prosegue oltre quel punto sulla stessa traiettoria fino a collisione o RANGE.

| INCONTRO / CONDIZIONE | RISULTATO |
| --- | --- |
| MOB, proiettile standard | Applica HIT e DANNO, poi sparisce immediatamente. |
| MURO / OSTACOLO | Sparisce immediatamente. |
| PG alleato | Attraversa senza collisione, DANNO o altri effetti; non viene distrutto. |
| Altro PROIETTILE FISICO | Si attraversano senza alcuna collisione. |
| RANGE massimo | Sparisce immediatamente; la distanza percorsa non supera il RANGE dell’attacco/arma. |

- PERFORAZIONE: applica HIT/DANNO al MOB attraversato e prosegue finché sono disponibili PERFORAZIONI; sparisce quando colpisce l’ultimo MOB consentito. MURI/OSTACOLI e RANGE possono interromperlo prima.
- La PERFORAZIONE modifica la regola standard «HIT MOB → sparisce»; eventuali modificatori del DANNO seguono la specifica della PASSIVA. PG03 / CALIBRO PERFORANTE: 2 PERFORAZIONI → attraversa il 1° e il 2° MOB, danneggia il 3° e sparisce. DANNO della prima HIT 100%, seconda HIT 70% (−30%), terza HIT 40% (−60%) del danno originale del PROIETTILE, prima della DEF di ciascun bersaglio; RANGE invariato.
- PG01, PG04 e PG05 non usano questo volo fisico per l’ATTACCO BASE. Le ABILITÀ mantengono le regole specifiche delle proprie schede: la classificazione dell’ATTACCO BASE non ne cambia automaticamente la meccanica.
- Le collisioni di PROJECTILE_PG e PROJECTILE_MOB sono distinte e confermate nella sezione 10.9. I proiettili di entrambi i layer attraversano i PET senza HIT, DANNO, distruzione o deviazione del proiettile; ZOMB04 conserva le proprie altre regole (sezione 13.8).

### 10.2 MURI E OSTACOLI

- MURO: blocca MOVIMENTO e ATTACCHI; non può essere sorvolato.
- OSTACOLO: blocca MOVIMENTO e ATTACCHI normali; può essere sorvolato dai PROIETTILI con traiettoria PARABOLICA.
- Le eccezioni al blocco degli ATTACCHI sono definite nelle singole schede.
- La BARRIERA di PG01 eredita le collisioni del layer OSTACOLO (sezione 10.9), conservando durata e proprietà della propria ABILITÀ.

### 10.3 AREE CIRCOLARI E AREE ITEMS — MURI E OSTACOLI

- AREA_EFFECT_PG e AREA_EFFECT_MOB non hanno collisione fisica con MURO o OSTACOLO: queste coppie restano escluse dalla Layer Collision Matrix. Le verifiche geometriche di SPICCHI/SEZIONI contro MURI/OSTACOLI sono gestite dalla logica dell’ABILITÀ/AREA, conservando le regole ed eccezioni seguenti.

| ATTACCO / EFFETTO | SUDDIVISIONE | REGOLA |
| --- | --- | --- |
| PG04 — ATTACCO BASE | 4 SPICCHI da 90° | Eliminazione completa dello SPICCHIO intercettato. |
| PG04 — PIOGGIA DI GRANATE | 4 SPICCHI da 90° per singola esplosione | Stesse regole dell’ATTACCO BASE. |
| PG04 — COLPO GROSSO | 4 SPICCHI da 90° per singola esplosione | Stesse regole dell’ATTACCO BASE. |
| PG04 — PYROMANIA | 4 SPICCHI da 90° per AREA INCENDIATA | Stesse regole dell’ATTACCO BASE. |
| PG07 — LUCKY SHOT | 4 SPICCHI da 90° | Eliminazione completa dello SPICCHIO intercettato. |
| AURA TOSSICA | 8 SPICCHI | Regole delle AREE DI EFFETTO con MURI/OSTACOLI; eliminazione completa dello SPICCHIO intercettato. |
| MINE | 4 SPICCHI | Regole delle AREE DI EFFETTO con MURI/OSTACOLI; eliminazione completa dello SPICCHIO intercettato. |
| TASER | 6 SPICCHI | Regole delle AREE DI EFFETTO con MURI/OSTACOLI; eliminazione completa dello SPICCHIO intercettato. |
| REPULSE | 8 SPICCHI | Regole delle AREE DI EFFETTO con MURI/OSTACOLI; eliminazione completa dello SPICCHIO intercettato. |
| SCIABOLATA | 4 SPICCHI nell’AREA a SEMICERCHIO | Regole delle AREE DI EFFETTO con MURI/OSTACOLI; eliminazione completa dello SPICCHIO intercettato. |
| ZOMB03 — AREA DI DANNO | 8 SPICCHI da 45° | Eliminazione completa dello SPICCHIO intercettato. |
| ZOMB05 — ESPLOSIONE | 8 SPICCHI da 45° | Eliminazione completa dello SPICCHIO intercettato. |
| ITEM GRANATA | 4 SPICCHI da 90°; raggio 2,5 m | Eliminazione completa dello SPICCHIO intercettato. |
| ITEM MOLOTOV | 8 SPICCHI da 45°; raggio 5 m | Eliminazione completa dello SPICCHIO intercettato. |
| ITEM TRAPPOLA | 4 SEZIONI da 1 × 1 m; rettangolo 1 × 4 m | Eliminazione completa della SEZIONE intercettata; orientamento e RANGE secondo la sezione 9.2. |
| ITEM SMOKE / POZIONE CURATIVA | Nessun taglio dell’AREA da parte di MURI/OSTACOLI | MURI/OSTACOLI non hanno effetto. |
| PG07 — PIOGGIA DI FRECCE | Nessun taglio dell’AREA da parte di MURI/OSTACOLI | Le FRECCE ignorano MURI/OSTACOLI lungo la caduta e applicano DANNO nel cerchio di raggio 0,25 m centrato sul PUNTO D’IMPATTO. |

- Se un MURO/OSTACOLO intercetta anche parzialmente uno SPICCHIO, l’intero SPICCHIO viene eliminato e non genera HIT/DANNO. Gli altri SPICCHI restano validi.
- Gli SPICCHI definiscono esclusivamente la geometria; non generano HIT separate. Un bersaglio sul confine tra SPICCHI non riceve HIT aggiuntive per questo motivo.
- La regola a 4 SPICCHI di PIOGGIA DI GRANATE sostituisce la precedente eccezione che consentiva alle sue esplosioni di ignorare MURI/OSTACOLI.
- Questa suddivisione riguarda esclusivamente i casi elencati. FILO SPINATO usa le proprie 10 SEZIONI da 36° (sezione 12).

### 10.4 CD — INIZIO RUN E CAMBIO AREA

- All’INIZIO RUN i CD di TUTTE le ABILITÀ partono da capo: ogni ABILITÀ inizia IN CD e diventa disponibile al completamento del proprio CD FINALE.
- Al CAMBIO AREA un’ABILITÀ già DISPONIBILE rimane DISPONIBILE.
- Al CAMBIO AREA un’ABILITÀ già IN CD ma non più ATTIVA mantiene esattamente il CD residuo e continua il conteggio nella nuova AREA.
- Le ABILITÀ ancora ATTIVE terminano al CAMBIO AREA; i loro CD ripartono da capo applicando CD REDUCTION. Eventuali ATTACCHI potenziati rimanenti di COLPO GROSSO e FUOCO CURATIVO vengono persi.
- Eccezione — SCUDO: il CAMBIO AREA non lo disattiva (sezione 22.6).
- RIPARTENZA del CD significa attesa dell’intero CD FINALE; non significa rendere immediatamente disponibile l’ABILITÀ.
- Le specifiche terminazioni di EFFETTI e PASSIVE al CAMBIO AREA sono riportate nelle schede PG.

### 10.5 STATO DI INVISIBILITÀ

- I MOB non seguono i PG INVISIBILI e non iniziano nuovi ATTACCHI contro di loro. Gli ATTACCHI già in corso proseguono fino al termine.
- ATTACCO, ABILITÀ, uso di ITEM e INTERAZIONE annullano INVISIBILITÀ esclusivamente per il PG che compie l’AZIONE.
- MOVIMENTO e RESUSCITARE un alleato non annullano INVISIBILITÀ.
- Il BONUS MOVE SPD associato termina per il PG quando perde INVISIBILITÀ.
- Alla fine dell’effetto i MOB possono nuovamente rilevare, seguire e attaccare quel PG secondo le normali regole di targeting.

### 10.6 EFFETTI PERSISTENTI — REGOLA GENERALE PROVVISORIA

- Riapplicare lo stesso EFFETTO PERSISTENTE allo stesso bersaglio non lo cumula: la nuova applicazione rinnova la DURATA.
- Le regole specifiche esplicitamente definite prevalgono sempre su questa regola generale.
- BRUCIATURA e VELENO, incluso VELENO PG05, seguono la REGOLA GENERALE DANNI DA STATO (sezione 10.8): ISTANZE cumulative fino a 5, DURATA condivisa rinnovata a ogni applicazione, anche al CAP.
- PYROMANIA PG04: le AREE INCENDIATE possono sovrapporsi e i loro DANNI si sommano.
- VITAMINA C / FUOCO CURATIVO PG06: il BONUS non si cumula e una nuova applicazione rinnova la DURATA, come specificato nella scheda PG06.
- FIRE BULLET: ogni nuova HIT applica un’ISTANZA di BRUCIATURA secondo la REGOLA GENERALE DANNI DA STATO, con CAP di 5 ISTANZE e rinnovo della DURATA anche al CAP (sezioni 10.8 e 22.7).
- TASER: se un MOB riceve lo STUN quando è già sotto effetto di STUN, la DURATA si rinnova (sezione 22.8).

### 10.7 ATTRIBUZIONE KILL — REGOLA GENERALE

- Una KILL viene attribuita al PG quando un MOB muore per un DANNO causato da quel PG, da qualsiasi fonte di DANNO riconducibile al PG.

### 10.8 REGOLA GENERALE DANNI DA STATO — BRUCIATURA E VELENO

- BRUCIATURA e VELENO seguono la stessa REGOLA GENERALE DANNI DA STATO.
- Il primo tick di DANNO avviene 1 s dopo l’applicazione/rinnovo; i tick successivi avvengono ogni 1 s.
- Ogni nuova ISTANZA dello stesso STATO sullo stesso bersaglio rinnova la DURATA completa dello STATO e aumenta il DANNO complessivo fino al CAP.
- DANNO per tick = DANNO base dello STATO × numero ISTANZE.
- Massimo 5 ISTANZE dello stesso STATO sullo stesso bersaglio.
- Raggiunte 5 ISTANZE, ulteriori applicazioni non aumentano il DANNO ma rinnovano comunque la DURATA completa.
- Le ISTANZE condividono la DURATA rinnovata: non hanno DURATE indipendenti.
- Restano invariati i valori specifici di DANNO e DURATA definiti nelle singole schede.

### 10.9 LAYER E MATRICE DELLE COLLISIONI — CONFERMATO

#### DISTINZIONE PLAYER / PG E LAYER

- PLAYER identifica il giocatore umano/connesso e le sue responsabilità; PG identifica il personaggio fisico nel mondo di gioco, controllato da un PLAYER oppure da IA. Il layer PG comprende entrambi i casi.
- La distinzione PLAYER/PG non introduce un TAG o un layer fisico PLAYER. Per ora ROG ZOMBIE non utilizza TAG tecnici dedicati, inclusi TAG PG e MOB: l’identificazione avviene tramite LAYER + componenti/script + stati logici. I LAYER gestiscono collisioni e rilevamento, i componenti/script distinguono identità e comportamenti specifici. Nuovi TAG saranno introdotti solo se emergerà una necessità concreta durante lo sviluppo.

| LAYER | CONTENUTO / FUNZIONE |
| --- | --- |
| PG | Tutti i PG, controllati da PLAYER o IA. |
| MOB | ZOMB, MINI BOSS e BOSS. |
| MURO | Elementi classificati come MURO. |
| OSTACOLO | Elementi classificati come OSTACOLO; BARRIERA di PG01 e collider fisici degli NPC HUB ne ereditano le collisioni. |
| PROJECTILE_PG | Proiettili fisici generati dai PG. |
| PROJECTILE_MOB | Proiettili fisici generati dai MOB. |
| PET | PET; collisioni e chiarimenti nella sezione 22.2. |
| TRIGGER_PG | Rilevamento dei PG senza blocco fisico: CHEST, MEDI KIT, USCITA AREA, RIANIMAZIONE, EXIT HUB e interazione con NPC HUB. |
| TRIGGER_MOB | Rilevamento dei MOB senza blocco fisico, incluso il TRIGGER della MINE. |
| AREA_EFFECT_PG | Rilevamento/interazione degli effetti destinati ai PG. |
| AREA_EFFECT_MOB | Rilevamento/interazione degli effetti destinati ai MOB. |

#### COLLISIONI FISICHE E PROIETTILI

Ogni coppia è simmetrica ed è riportata una sola volta. SÌ abilita la collisione; per i proiettili gli effetti della HIT e le eccezioni restano quelli delle rispettive schede. NO indica assenza di collisione. Le coppie non riportate non ricevono un valore predefinito.

| COPPIA | COLLISIONE |
| --- | --- |
| PG ↔ PG | SÌ |
| PG ↔ MOB | SÌ |
| PG ↔ MURO | SÌ |
| PG ↔ OSTACOLO | SÌ |
| PG ↔ PROJECTILE_PG | NO |
| PG ↔ PROJECTILE_MOB | SÌ |
| MOB ↔ MOB | SÌ — collider fisico ridotto rispetto alla dimensione visiva, come nel TEST in engine, per rendere le orde più fluide. |
| MOB ↔ MURO | SÌ |
| MOB ↔ OSTACOLO | SÌ |
| MOB ↔ PROJECTILE_PG | SÌ |
| MOB ↔ PROJECTILE_MOB | NO |
| PROJECTILE_PG ↔ MURO | SÌ |
| PROJECTILE_PG ↔ OSTACOLO | SÌ |
| PROJECTILE_PG ↔ PROJECTILE_PG | NO |
| PROJECTILE_PG ↔ PROJECTILE_MOB | NO |
| PROJECTILE_MOB ↔ MURO | SÌ |
| PROJECTILE_MOB ↔ OSTACOLO | SÌ |
| PROJECTILE_MOB ↔ PROJECTILE_MOB | NO |
| PET ↔ PG | NO |
| PET ↔ MOB | SÌ |
| PET ↔ PET | SÌ |
| PET ↔ MURO | SÌ |
| PET ↔ OSTACOLO | SÌ |
| PET ↔ PROJECTILE_PG | NO |
| PET ↔ PROJECTILE_MOB | NO |

- MOB ↔ MOB sostituisce la precedente regola generale di assenza di collisione. Non è stata specificata una misura numerica del collider ridotto. ZOMB05 mantiene tutte le normali collisioni del layer MOB anche in PRE-ESPLOSIONE, inclusa MOB ↔ MOB SÌ; la precedente eccezione di attraversamento/sovrapposizione è eliminata.
- I PROJECTILE_PG e i PROJECTILE_MOB si attraversano senza interazione; PROJECTILE_PG ↔ PROJECTILE_PG e PROJECTILE_MOB ↔ PROJECTILE_MOB sono NO. I PROJECTILE_MOB si attraversano fra loro senza distruggersi, deviare o generare HIT/effetti reciproci.
- I PET non hanno una meccanica HP/DANNO. PROJECTILE_PG e PROJECTILE_MOB attraversano completamente i PET senza HIT, DANNO, distruzione o deviazione del proiettile: i PET non diventano scudi mobili.
- MURO ↔ MURO e MURO ↔ OSTACOLO sono esclusi dalle collisioni di gameplay da definire: riguardano elementi statici/inanimati del LEVEL DESIGN. Non viene introdotta un’interazione fisica fra essi.
- Restano valide le eccezioni già definite nelle singole schede, comprese traiettorie PARABOLICHE e SPRITE DI MORTE. Gli stati logici e le variazioni di collisione confermate sono descritti sotto; le coppie non riportate non ricevono valori impliciti.

#### TRIGGER E AREA_EFFECT — RILEVAMENTO SENZA COLLISIONE FISICA

In questa tabella SÌ significa rilevamento/interazione con il destinatario, senza bloccarne il movimento; NO esclude il rilevamento della coppia.

| COPPIA | RILEVAMENTO |
| --- | --- |
| PG ↔ TRIGGER_PG | SÌ |
| MOB ↔ TRIGGER_MOB | SÌ |
| PG ↔ TRIGGER_MOB | NO |
| MOB ↔ TRIGGER_PG | NO |
| PG ↔ AREA_EFFECT_PG | SÌ |
| MOB ↔ AREA_EFFECT_MOB | SÌ |
| PG ↔ AREA_EFFECT_MOB | NO |
| MOB ↔ AREA_EFFECT_PG | NO |

- AREA_EFFECT_PG e AREA_EFFECT_MOB rilevano/interagiscono rispettivamente solo con PG e MOB. Le verifiche contro MURO/OSTACOLO sono separate e gestite dalla logica dell’AREA (sezione 10.3): AREA_EFFECT_PG ↔ MURO, AREA_EFFECT_PG ↔ OSTACOLO, AREA_EFFECT_MOB ↔ MURO e AREA_EFFECT_MOB ↔ OSTACOLO non hanno collisione fisica e restano fuori dalla Layer Collision Matrix.
- CHEST e MEDI KIT sono completamente attraversabili, senza collisione fisica con PG o MOB; funzionano tramite TRIGGER_PG, con oggetto visivo e collider IsTrigger. Non viene introdotto un layer fisico dedicato CHEST/MEDI KIT né un layer unico ITEM / INTERACTABLE.
- La MINE è attraversabile fisicamente da PG e MOB, senza collisione fisica; il rilevamento del MOB avviene tramite TRIGGER_MOB. Restano validi il timer e le regole della sezione 22.5.
- La BARRIERA di PG01 eredita le collisioni di OSTACOLO: blocca PG, MOB, PROJECTILE_PG e PROJECTILE_MOB; PET ↔ OSTACOLO è SÌ. Mantiene le proprietà temporanea, invalicabile e indistruttibile della propria scheda.

- RIANIMAZIONE: TRIGGER_PG di raggio 2 m attorno al PG in DOWN, senza collisione fisica aggiuntiva; interazione con F e validità secondo la sezione 28.
- NPC HUB: collider fisico OSTACOLO + TRIGGER_PG per l’interazione con F; EXIT HUB usa TRIGGER_PG e F senza blocco fisico. Non servono nuovi layer NPC o INTERACTABLE.

#### STATI LOGICI E COLLISIONI — CONFERMATO

- ATTIVO, DOWN, MORTE, RESURREZIONE, INVISIBILITÀ, INVULNERABILITÀ, STUN, SLOW, RESPINTA, BRUCIATURA, VELENO, RAGE, TENACIA, MARCHIO, GHOSTING e VITAMINA C sono stati, transizioni o modificatori logici: nessun LAYER o TAG dedicato. Le caratteristiche specifiche sono gestite tramite componenti/script.
- DOWN: il PG mantiene il layer PG e tutte le sue normali collisioni fisiche. Le regole su azioni e bersagli restano quelle della sezione 28.
- MORTE PG: il layer PG resta invariato, ma tutte le collisioni sono disabilitate. La SPRITE DI MORTE rimane a terra ed è completamente priva di collisioni; PG, MOB, PET e proiettili la attraversano senza interazioni.
- RESURREZIONE: transizione logica che ripristina le normali collisioni del PG, mantenute anche durante i 2 s di INVULNERABILITÀ dopo la resurrezione. INVULNERABILITÀ modifica la gestione di HIT/DANNO, senza rendere il PG attraversabile.
- INVISIBILITÀ e GHOSTING mantengono il layer PG e le collisioni normali. STUN, SLOW e RESPINTA mantengono il layer originale PG/MOB e tutte le collisioni normali; RESPINTA è uno spostamento forzato che MURO/OSTACOLO possono interrompere secondo le regole esistenti.
- BRUCIATURA e VELENO sono danni da stato logici senza variazioni di layer/collisioni; restano validi tick, istanze, CAP e rinnovo della sezione 10.8. Non servono TAG Projectile_DOT o Projectile_Slow: gli effetti sono applicati dalla logica della HIT/abilità/ITEM.
- RAGE, TENACIA, MARCHIO e VITAMINA C non modificano layer o collisioni. SCUDO è una protezione logica associata al PG, senza collider o layer proprio: mitigazione e consumo delle HIT seguono la sezione 22.6.
- PRE-ESPLOSIONE ZOMB05 è uno stato logico che conserva layer MOB e tutte le collisioni normali, inclusa MOB ↔ MOB SÌ (sezione 13.9).
- La classificazione completa delle meccaniche usa esclusivamente i layer esistenti e verifiche logiche: ITEMS nella sezione 9.6, ABILITÀ/PASSIVE PG nella sezione 12, attacchi/effetti MOB nella sezione 13.10 e ABILITÀ BONUS nella sezione 22.11. Non introduce modifiche a valori, durate, bersagli o regole specifiche già definite.

<a id="sezione-11"></a>

## 11. ROSTER PG — STATS BASE

| PG | ARMA | HP | ATK | DEF | MOVE SPD | ATK SPD | CD REDUCTION | RANGE |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| PG01 | Shotgun | 100 | 20 | +15% | 100 | 85 | 100 | 4 m |
| PG02 | Assault Rifle | 100 | 10 | 0% | 100 | 400 | 100 | 10 m |
| PG03 | Sniper Rifle | 80 | 50 | −10% | 110 | 60 | 100 | 20 m |
| PG04 | Grenade Launcher | 100 | 25 | 0% | 100 | 65 | 100 | 7 m |
| PG05 | Knife | 100 | 30 | −10% | 130 | 150 | 100 | 2 m |
| PG06 | Revolver | 120 | 40 | 0% | 100 | 80 | 100 | 10 m |
| PG07 | Bow | 100 | 30 | 0% | 110 | 110 | 100 | 15 m |
| PG08 | Heavy Machine Gun | 100 | 7 | +10% | 90 | 650 | 100 | 8 m |

Nota: le STATS sopra riportano gli ultimi valori definiti esplicitamente nel progetto, incluse la revisione CD REDUCTION e le correzioni RANGE di PG03 e PG04. CD REDUCTION è una STAT modificatore con VALORE BASE 100 per PG01–PG08; i CD BASE in secondi restano esclusivamente nelle singole ABILITÀ e vengono modificati nel CD FINALE secondo la formula della sezione 10.

<a id="sezione-12"></a>

## 12. ABILITÀ E PASSIVE DEI PG

I valori CD in secondi riportati nelle schede seguenti sono i CD BASE delle singole ABILITÀ, aggiornati dove modificati esplicitamente nella conversazione. Il CD FINALE dipende dalla STAT CD REDUCTION del PG secondo la formula della sezione 10.

### PG01 — Shotgun

- ATTACCO BASE: AREA HITSCAN istantanea, senza singoli proiettili; CONO di portata 4 m e ampiezza 75°, orientato verso il CURSORE. Colpisce tutti i MOB validi nel CONO con ATK 20 per ciascuno, prima della DEF; il DANNO non è suddiviso e non vengono simulati singoli pallettoni. RANGE interno 400 = portata 4 m. MURI e OSTACOLI bloccano l’ATTACCO verso i MOB schermati. ATK SPD 85 = 0,85 ATTACCHI/s.

- ABILITÀ 1 — PESTONE: 40 DANNO; AREA RETTANGOLARE frontale 3 m × 7 m: larghezza frontale 3 m e profondità 7 m, a partire dal PG nella direzione PG→CURSORE. CD 10 s, avviato all’attivazione. Applica SLOW ai MOB colpiti: velocità di movimento ridotta del 30% per 3 s. La riapplicazione rinnova la durata senza cumulare lo stesso effetto, secondo le regole generali.
- ABILITÀ 2 — BARRIERA: piazza un muro invalicabile e indistruttibile di 7 m × 1 m; durata 5 s; CD 15 s, avviato all’attivazione. Eredita le collisioni del layer OSTACOLO (sezione 10.9).
- PASSIVA 1: con HP <50% ottiene +15% DEF; l'effetto termina quando gli HP tornano ≥50%.
- PASSIVA 2: con HP <50% ottiene +15% ATK; l'effetto termina quando gli HP tornano ≥50%.

### PG02 — Assault Rifle

- ATTACCO BASE: PROIETTILE BALISTICO non PERFORANTE, verso il CURSORE; ATK 10, RANGE 10 m, ATK SPD BASE 400 = 4 ATTACCHI/s. Segue le REGOLE GENERALI DEI PROIETTILI.

- ABILITÀ 1 — FUOCO RAPIDO: BONUS temporaneo +40% ATK SPD corrente; durata 4 s; CD 10 s, avviato al termine dei 4 s di DURATA. Senza altri modificatori, ATK SPD passa da 400 a 560 = 5,6 ATTACCHI/s. Alla scadenza viene rimosso soltanto il BONUS temporaneo, preservando gli altri modificatori e BONUS acquisiti.
- ABILITÀ 2 — FUOCO DI SOPPRESSIONE: raffica di 50 PROIETTILI FISICI in 3 s; 3 DANNO per PROIETTILE prima della DEF; RANGE 10 m. CONO frontale di ampiezza totale 50°, diviso in 5 SPICCHI da 10°, numerati da destra a sinistra. Ogni PROIETTILE è diretto lungo il CENTRO dello SPICCHIO corrente: −20°, −10°, 0°, +10°, +20° rispetto all’asse del CONO, nell’ordine da destra a sinistra. I PROIETTILI vengono sparati in successione, uno per lo SPICCHIO corrente, con sequenza 1-2-3-4-5-4-3-2-1-2-3-ecc., senza ripetere gli estremi e fino al 50° PROIETTILE. CD BASE 12 s, avviato all’attivazione. Sostituisce il precedente HITSCAN istantaneo a CONO da 30 DANNO.
- PASSIVA 1: con HP <30%, ogni KILL cura 1% degli HP MASSIMI correnti di PG02; non ha effetto a HP ≥30%.
- PASSIVA 2: ogni 15 KILL entra in RAGE per 2 s con +30% ATK. Al CAMBIO AREA il conteggio delle KILL resta; RAGE termina se attiva, rimuovendo soltanto il BONUS ATK temporaneo.

### PG03 — Sniper Rifle

- ATTACCO BASE: PROIETTILE BALISTICO non PERFORANTE di base; ATK 50, ATK SPD 60 = 0,6 ATTACCHI/s; RANGE interno 2000 = 20 m. Segue le REGOLE GENERALI DEI PROIETTILI.
- ABILITÀ 1 — COLPO LASER: sostituisce COLPO PERFORANTE. Emette istantaneamente un RAGGIO verso il CURSORE; AREA 20 × 2 m (portata 20 m, larghezza 2 m); 35 DANNO una sola volta a ciascun MOB nell’AREA; CD BASE 18 s, avviato all’attivazione. Attraversa MURI e OSTACOLI: eccezione alla REGOLA DI BLOCCO.
- ABILITÀ 2 — TRIPLO SPARO: 3 proiettili da 40 DANNO; cono totale 20 m / 30°; ogni proiettile copre 10°; sequenza SINISTRA → CENTRO → DESTRA; intervallo 0,5 s; CD BASE 10 s, avviato all’attivazione.
- PASSIVA 1 — CALIBRO PERFORANTE: rende PERFORANTI i PROIETTILI dell’ATTACCO BASE con 2 PERFORAZIONI. Il 1° e il 2° MOB ricevono DANNO e vengono attraversati; il 3° riceve DANNO e arresta il PROIETTILE. La prima HIT infligge il 100% del danno originale del PROIETTILE, la seconda il 70% (riduzione del 30%), la terza il 40% (riduzione del 60%), prima della DEF di ciascun bersaglio. Le riduzioni sono riferite al danno originale, non concatenate. Con ATK 50: 50 / 35 / 20 DANNO prima della DEF. RANGE invariato. MURI/OSTACOLI e limite RANGE possono interrompere prima la traiettoria.
- PASSIVA 2 — PUNTO DEBOLE: l’ATTACCO BASE applica un MARCHIO al MOB colpito; il MOB marchiato ha DEF −20%; la HIT successiva sul MOB marchiato rimuove il MARCHIO.

### PG04 — Grenade Launcher

#### ATTACCO BASE

- ATK 25; ATK SPD 65 = 0,65 ATTACCHI/s; RANGE interno 700 = 7 m.
- HITSCAN + AREA IMPATTO: il punto d’impatto è determinato al lancio nella direzione di mira, entro RANGE 7 m, senza PROIETTILE FISICO in volo. Se il CURSORE è oltre 7 m, la destinazione è limitata a 7 m nella sua direzione.
- Si applica l’AREA CIRCOLARE nel punto d’impatto. Resta la regola specifica di determinazione della destinazione: solo un MURO può anticipare l’impatto; OSTACOLI e MOB non lo anticipano. Il precedente volo PARABOLICO dell’ATTACCO BASE è sostituito dall’HITSCAN; le ABILITÀ conservano le proprie regole.
- La HIT dell’ATTACCO BASE avviene 0,3 s dopo il lancio, nel punto d’impatto determinato al lancio. Il ritardo si applica anche agli ATTACCHI BASE potenziati da COLPO GROSSO; PIOGGIA DI GRANATE conserva la propria sequenza. PYROMANIA si attiva all’esplosione, con primo tick immediato all’attivazione dell’AREA INCENDIATA.
- AREA ESPLOSIONE CIRCOLARE, RAGGIO 1,25 m: 25 DANNO a ciascun MOB valido prima della DEF.
- MURI/OSTACOLI interrompono l’AREA secondo la regola dei 4 SPICCHI da 90° (sezione 10.3).

#### ABILITÀ 1 — PIOGGIA DI GRANATE

- Genera direttamente 10 ESPLOSIONI, senza PROIETTILI, in posizioni RANDOM nell’AREA EFFETTO CIRCOLARE di RAGGIO 5 m.
- Centro: posizione del CURSORE all’attivazione; nessun limite di distanza da PG04.
- Sequenza CASUALE nell’arco complessivo di 3 s; CD BASE 12 s, avviato all’attivazione.
- Eredita esclusivamente ATK e AREA ESPLOSIONE dell’ATTACCO BASE: ATK 25 e RAGGIO 1,25 m per esplosione. Non eredita traiettoria, RANGE o IMPATTO del PROIETTILE.
- Le esplosioni possono sovrapporsi. Ogni esplosione applica indipendentemente il proprio DANNO; lo stesso MOB può ricevere HIT da più esplosioni.
- Ogni AREA di esplosione segue la regola dei 4 SPICCHI da 90°, con eliminazione completa degli SPICCHI intercettati da MURI/OSTACOLI.

#### ABILITÀ 2 — COLPO GROSSO

- Potenzia i successivi 4 ATTACCHI BASE: +80% ATK (25 → 45) e +50% RAGGIO ESPLOSIONE (1,25 → 1,875 m).
- Modifica esclusivamente ATK e RAGGIO ESPLOSIONE. Le altre STATS e regole restano invariate, inclusa la geometria a 4 SPICCHI.
- Nessuna DURATA massima: le cariche restano disponibili fino al consumo. Ogni ATTACCO effettuato consuma una carica, anche in caso di MISS.
- CD BASE 10 s, avviato al consumo del 4° ATTACCO.
- Al CAMBIO AREA le cariche residue vengono perse e, se l’ABILITÀ era ATTIVA, il CD riparte da capo. Se era DISPONIBILE, rimane DISPONIBILE. Se era già IN CD ma non più ATTIVA, mantiene esattamente il CD residuo e continua il conteggio nella nuova AREA.

#### PASSIVA 1 — SCORTA ESPLOSIVA

- Quando selezionata, permette di accumulare fino a 3 GRANATE oppure 3 MOLOTOV dello stesso tipo per SLOT ITEM.
- Influenza contemporaneamente entrambi i tipi di ITEM. Gli altri ITEMS restano a 1 per SLOT.

#### PASSIVA 2 — PYROMANIA

- Le AREE ESPLOSIONE diventano AREE INCENDIATE per 3 s, con 5 HP/s per AREA.
- Si applica ad ATTACCO BASE, PIOGGIA DI GRANATE e COLPO GROSSO.
- L’AREA INCENDIATA corrisponde all’AREA ESPLOSIONE: RAGGIO 1,25 m per ATTACCO BASE/PIOGGIA DI GRANATE; 1,875 m per COLPO GROSSO.
- Le AREE INCENDIATE possono sovrapporsi e i loro DANNI si sommano.
- MURI/OSTACOLI bloccano il DANNO secondo la regola dei 4 SPICCHI da 90°.

### PG05 — Knife

- ATTACCO BASE: AREA HITSCAN istantanea, senza PROIETTILI FISICI;  AREA a CONO orientata verso il CURSORE, portata 2 m / 135°; ATK 30 per ogni MOB valido nell’AREA, senza suddivisione; ATK SPD 150 = 1,5 ATTACCHI/s. MURI/OSTACOLI bloccano il DANNO verso i MOB schermati.

#### ABILITÀ 1 — INVISIBILITÀ

- Rende INVISIBILE l’intera PARTY per 3,5 s e aumenta MOVE SPD del 15% durante l’effetto; CD BASE 15 s, avviato all’attivazione.
- Applica le REGOLE GENERALI dello stato di INVISIBILITÀ (sezione 10.5), inclusa l’eccezione RESUSCITARE e l’interruzione individuale.

#### ABILITÀ 2 — COLTELLI AVVELENATI

- Lancia simultaneamente 8 COLTELLI a 360° attorno a PG05, distanziati di 45°. Uno segue esattamente il CURSORE; gli altri sono orientati rispetto a quello.
- PROIETTILI BALISTICI non PERFORANTI; RANGE 10 m; 30 DANNO per COLTELLO; CD BASE 10 s, avviato all’attivazione.
- Seguono le REGOLE GENERALI DEI PROIETTILI: il primo MOB colpito riceve DANNO e VELENO e arresta il COLTELLO; MURI/OSTACOLI bloccano il PROIETTILE.
- VELENO: DANNO base 5 HP/s, DURATA 3 s. Segue la REGOLA GENERALE DANNI DA STATO (sezione 10.8): DANNO per tick = 5 × numero ISTANZE, fino a 5 ISTANZE; ogni applicazione rinnova la DURATA completa di 3 s, anche al CAP. Primo tick 1 s dopo l’applicazione/rinnovo, poi ogni 1 s; le ISTANZE non hanno DURATE indipendenti.

#### PASSIVA 1 — GHOSTING

- Con HP <30% prima della HIT, una HIT ricevuta attiva INVISIBILITÀ e +15% MOVE SPD per 2 s. Con HP ≥30% non si attiva.
- Applica le stesse REGOLE GENERALI di INVISIBILITÀ.
- HIT ricevute durante l’effetto non lo riattivano e non rinnovano la DURATA.
- Sovrapposizione GHOSTING/INVISIBILITÀ su PG05: un solo BONUS +15% MOVE SPD; DURATA rinnovata usando quella dell’ultimo effetto applicato.
- Terminato l’effetto, la PASSIVA è subito disponibile per una nuova HIT che soddisfi le condizioni; nessun CD interno.

#### PASSIVA 2 — LAMA DI CICUTA

- Conta i MOB COLPITI dagli ATTACCHI BASE: +1 per ciascun MOB colpito, anche più incrementi nello stesso ATTACCO.
- Soglia 15; il contatore si ferma a 15 e l’eccedenza viene ignorata.
- Raggiunta la soglia, il successivo ATTACCO BASE applica VELENO 5 HP/s per 3 s a tutti i MOB colpiti nell’AREA ATTACCO, secondo la REGOLA GENERALE DANNI DA STATO (sezione 10.8).
- Dopo l’ATTACCO potenziato il contatore torna a 0 e inizia un nuovo ciclo. Il contatore si conserva al CAMBIO AREA.

### PG06 — Revolver

- ATTACCO BASE: PROIETTILE FISICO non PERFORANTE, senza proprietà speciali; ATK 40; ATK SPD 80 = 0,8 ATTACCHI/s; RANGE 10 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — CURA AD AREA

- Cura 40 HP ai PG ATTIVI entro RAGGIO 5 m, con centro PG06; PG06 è incluso tra i bersagli. I PG in DOWN o MORTE sono esclusi. CD BASE 20 s, avviato all’attivazione.
- MURI/OSTACOLI non bloccano la CURA.
- NO OVERHEAL. I PG al 100% HP restano bersagli validi, recuperano 0 HP e possono attivare VITAMINA C.

#### ABILITÀ 2 — FUOCO CURATIVO

- Potenzia i successivi 6 ATTACCHI BASE conservando il normale funzionamento e DANNO del REVOLVER.
- Ogni HIT su un MOB genera in aggiunta una CURA di 15 HP verso un PG ATTIVO entro 15 m da PG06; PG06 è incluso. I PG in DOWN o MORTE sono esclusi. Un MISS non genera CURA.
- Ogni ATTACCO effettuato consuma una carica, anche in caso di MISS. Nessuna DURATA massima; CD BASE 15 s, avviato al consumo del 6° ATTACCO.
- La selezione del destinatario viene rivalutata per ciascun ATTACCO. I PG con HP <100% hanno sempre priorità sui PG al 100% HP, che restano validi ma sono ultima scelta. Fra i PG sotto il 100% HP: percentuale HP più bassa → minor numero di HP effettivi → distanza minore da PG06 → scelta CASUALE in perfetta parità. Se tutti i PG validi sono al 100% HP: distanza minore da PG06 → scelta CASUALE in perfetta parità.
- MURI/OSTACOLI non impediscono la selezione né la CURA. NO OVERHEAL; un PG al 100% resta selezionabile ma recupera 0 HP. Se tutti i PG validi sono al 100%, la carica viene comunque consumata e nessuno recupera HP.
- Al CAMBIO AREA perde le cariche residue; se ancora ATTIVA, il CD riparte da capo. Se DISPONIBILE, rimane DISPONIBILE. Se già IN CD ma non più ATTIVA, mantiene esattamente il CD residuo e continua il conteggio nella nuova AREA.

#### PASSIVA 1 — ELEMOSINA

- Ogni KILL attribuita esclusivamente a PG06 secondo la regola generale della sezione 10.7, da qualsiasi fonte di DANNO riconducibile a PG06, effettua un roll del 3%; un successo genera 1 MEDI KIT fisico sul terreno.
- Qualsiasi PG della PARTY può raccoglierlo passandoci sopra. Viene usato immediatamente e non occupa SLOT ITEM.
- CURA base: 10% degli HP MASSIMI del PG che lo raccoglie; NO OVERHEAL. A HP pieni il TRIGGER ignora il PG e il MEDI KIT non viene consumato; valgono validità e priorità della sezione 26.
- DURATA infinita nell’AREA: rimane fino alla RACCOLTA oppure scompare al CAMBIO AREA.

#### PASSIVA 2 — VITAMINA C

- Un PG considerato bersaglio di una CURA delle ABILITÀ di PG06 riceve +35% del proprio ATK SPD BASE per 4 s, anche se a HP pieni e con CURA effettiva pari a 0.
- CURA AD AREA: tutti i PG considerati entro 5 m, incluso PG06. FUOCO CURATIVO: solo il destinatario selezionato dopo una HIT valida; un MISS non applica VITAMINA C.
- Il BONUS non si cumula; una nuova applicazione rinnova la DURATA a 4 s. Ogni PG gestisce individualmente la propria DURATA.
- Il MEDI KIT di ELEMOSINA non attiva VITAMINA C. Al CAMBIO AREA gli effetti VITAMINA C ancora attivi terminano.

### PG07 — Bow

- ATTACCO BASE: FRECCIA BALISTICA non PERFORANTE; ATK 30; ATK SPD 110 = 1,1 ATTACCHI/s; RANGE 15 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — MULTI SHOT

- 9 PROIETTILI BALISTICI non PERFORANTI generati simultaneamente; 20 DANNO per PROIETTILE; RANGE 7 m; CONO 80°; RESPINTA 5 m; PROJECTILE SPD 20 m/s; CD BASE 13 s, avviato immediatamente all’attivazione.
- Un PROIETTILE segue il CURSORE; gli altri sono distribuiti ogni 10°: −40°, −30°, −20°, −10°, 0°, +10°, +20°, +30°, +40°.
- Ogni PROIETTILE che colpisce genera una HIT separata e la DEF viene applicata separatamente a ogni HIT, anche se più PROIETTILI della stessa attivazione colpiscono lo stesso MOB.
- RESPINTA progressiva in 0,25 s, con partenza rapida e rallentamento finale. Massimo una RESPINTA per MOB per attivazione. La direzione è quella del primo PROIETTILE che genera HIT; in caso di HIT simultanee si usa la media delle direzioni dei PROIETTILI coinvolti.
- MURI/OSTACOLI bloccano i PROIETTILI e interrompono la RESPINTA prima dei 5 m quando incontrati.

#### ABILITÀ 2 — PIOGGIA DI FRECCE

- SISTEMA A RILASCIO: tenere premuto Q mostra l’anteprima dell’AREA EFFETTO, che segue il CURSORE; al rilascio di Q l’ABILITÀ si attiva nella posizione selezionata. L’anteprima non genera HIT e non avvia il CD; sequenza delle FRECCE e CD iniziano al rilascio.
- AREA BERSAGLIO CIRCOLARE, RAGGIO 3,5 m; centro sul CURSORE all’attivazione; RANGE di attivazione illimitato.
- 20 FRECCE distribuite in posizioni CASUALI nell’AREA durante 3 s. Il tempo è regolare: prima a T = 0, ultima a T = 3 s, intervallo 3/19 s (circa 0,158 s).
- La caduta delle FRECCE è solo rappresentazione grafica e non usa una velocità fisica di gameplay. La HIT avviene esattamente nel momento programmato per ciascuna FRECCIA. Non serve un’altezza fisica di SPAWN: basta generare graficamente la FRECCIA sopra il PUNTO D’IMPATTO. Ogni impatto genera un’AREA di DANNO CIRCOLARE di RAGGIO 0,25 m, con 10 HP di DANNO prima della DEF per ciascun MOB colpito.
- L’AREA è centrata sul PUNTO D’IMPATTO e colpisce tutti i MOB il cui corpo interseca il cerchio, una sola HIT per MOB per FRECCIA. Nessun MOB nell’AREA → MISS.
- Lo stesso MOB può ricevere più FRECCE; la DEF viene applicata separatamente a ciascuna HIT.
- MURI/OSTACOLI non intercettano la caduta e non tagliano l’AREA.
- CD BASE 15 s, avviato all’attivazione. Al CAMBIO AREA termina immediatamente, annulla le FRECCE non ancora generate e applica la regola generale del CD.

#### PASSIVA 1 — LUCKY SHOT

- Roll del 5% solo quando un ATTACCO BASE genera una HIT effettiva su un MOB; nessun roll in caso di MISS.
- Un successo genera una HIT AD AREA aggiuntiva pari al 30% dell’ATK di PG07; AREA CIRCOLARE di RAGGIO 2 m centrata sul MOB che ha generato il TRIGGER.
- Il DANNO risultante di LUCKY SHOT viene arrotondato all’intero più vicino con arrotondamento matematico: parte decimale <0,5 per difetto; ≥0,5 per eccesso. Il DANNO AD AREA è una HIT separata: la DEF si applica indipendentemente dall’ATTACCO BASE e individualmente a ogni MOB.
- Se l’ATTACCO BASE uccide il MOB, LUCKY SHOT può comunque attivarsi nel punto della sua MORTE e colpire gli altri MOB.
- MURI/OSTACOLI eliminano interamente gli SPICCHI intercettati secondo la geometria a 4 SPICCHI da 90°. Gli SPICCHI non generano HIT separate.
- La HIT di LUCKY SHOT non è un ATTACCO BASE e non genera ulteriori roll di LUCKY SHOT.

#### PASSIVA 2 — CONCENTRAZIONE

- Roll del 10% al lancio di ogni ATTACCO BASE, indipendentemente dalla futura HIT.
- Un successo rende la FRECCIA PERFORANTE: massimo 3 MOB sulla stessa traiettoria. Il primo MOB effettivamente colpito riceve il 100% del DANNO effettivo corrente dell’ATTACCO BASE; il secondo e il terzo il 50% dello stesso DANNO, calcolato prima della DEF, senza ulteriori riduzioni. Il DANNO risultante viene arrotondato all’intero più vicino con arrotondamento matematico: parte decimale <0,5 per difetto; ≥0,5 per eccesso.
- CONCENTRAZIONE propaga l’INTERA HIT dell’ATTACCO BASE: il primo, il secondo e il terzo MOB ricevono tutti gli eventuali effetti aggiuntivi associati alla HIT. La riduzione al 50% riguarda esclusivamente il DANNO, non gli altri effetti.
- La FRECCIA segue le REGOLE GENERALI DEI PROIETTILI: MURI/OSTACOLI la bloccano e il RANGE resta quello dell’ATTACCO BASE.
- CONCENTRAZIONE e LUCKY SHOT non possono essere attive insieme: ogni PG ha una sola PASSIVA selezionata per RUN.

### PG08 — Heavy Machine Gun

- ATTACCO BASE: PROIETTILE FISICO non PERFORANTE; ATK 7; ATK SPD 650 = 6,5 ATTACCHI/s; RANGE 8 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — FILO SPINATO

- ANELLO di RAGGIO ESTERNO 4,5 m e SPESSORE 1 m (fascia da 3,5 a 4,5 m), centrato sulla posizione di PG08 all’attivazione. Rimane fisso per 4 s e non segue PG08.
- Solo la fascia dell’ANELLO genera effetti: per ciascun MOB il conteggio del DANNO PERIODICO parte nel momento in cui entra in contatto con una SEZIONE/AREA valida; infligge 10 DANNO ogni secondo durante cui il MOB rimane nell’AREA. All’uscita il DANNO PERIODICO termina.
- SLOW 35% mentre il MOB è nella fascia; all’uscita lo SLOW cessa. Più applicazioni di FILO SPINATO non sommano lo SLOW né il DANNO: ogni MOB riceve un solo DANNO di 10 al secondo anche in sovrapposizione.
- Per un MOB già nella fascia di una SEZIONE valida all’attivazione, il conteggio del DANNO PERIODICO parte in quel momento e lo SLOW si applica immediatamente.
- Non è una barriera fisica: i MOB attraversano liberamente; i PG attraversano senza effetti; i PROIETTILI attraversano normalmente.
- 10 SEZIONI indipendenti da 36°. Alla generazione, se anche una sola parte di una SEZIONE interseca un MURO/OSTACOLO, l’intera SEZIONE viene eliminata.
- Nessun minimo di SEZIONI valide. Con 0/10 non viene generato FILO SPINATO, ma l’ABILITÀ è comunque UTILIZZATA.
- CD BASE 16 s, avviato all’attivazione anche con 0/10 SEZIONI.
- Al CAMBIO AREA tutte le SEZIONI scompaiono; DANNO e SLOW terminano e il CD segue la regola generale.

#### ABILITÀ 2 — COLPI RESPINGENTI

- Per 4 s ogni HIT di ATTACCO BASE applica il normale DANNO e RESPINTA di 1,5 m nella direzione del PROIETTILE che genera quella HIT. Nessuna HIT → nessuna RESPINTA.
- Ogni RESPINTA è progressiva in 0,25 s, con partenza rapida e rallentamento finale, come MULTI SHOT.
- Ogni nuova HIT applica una nuova RESPINTA completa e indipendente, anche allo stesso MOB; MURI/OSTACOLI interrompono lo spostamento.
- CD BASE 10 s, avviato al termine dei 4 s di DURATA.
- Se il CAMBIO AREA avviene durante i 4 s di COLPI RESPINGENTI, l’effetto termina immediatamente e da quel momento riparte l’intero CD.

#### PASSIVA 1 — TENACIA

- Esclusivamente ogni HIT effettiva generata dall’ATTACCO BASE di PG08 fornisce +0,2% DEF; BONUS massimo +30% (150 HIT). Ulteriori HIT al CAP non aumentano il BONUS. Altre fonti di DANNO/HIT attribuite a PG08 non incrementano TENACIA.
- QUALSIASI HIT ricevuta da PG08 azzera immediatamente tutto il BONUS TENACIA, indipendentemente dal DANNO/HP effettivamente persi, anche con DANNO finale 0. Le successive HIT effettive dell’ATTACCO BASE possono ricominciare l’accumulo; a BONUS 0% non c’è nulla da perdere.
- La HIT che azzera TENACIA viene mitigata dalla DEF comprensiva del BONUS TENACIA accumulato, rispettando il CAP DEF del 90%; il BONUS viene rimosso dopo il calcolo di quella HIT. Le HIT successive usano la DEF senza TENACIA, salvo nuovo accumulo.
- Al CAMBIO AREA il BONUS torna a 0%.
- La DEF BASE +10% rimane separata e non viene persa; al CAP, BASE + TENACIA = +40% prima di altri modificatori.

#### PASSIVA 2 — RAGE

- Dopo 5 s consecutivi di FUOCO CONTINUATIVO si attivano immediatamente +30% ATK e −20% MOVE SPD.
- Non è necessario generare HIT: si può sparare a vuoto. Gli intervalli ordinari fra colpi dovuti ad ATK SPD fanno parte del FUOCO CONTINUATIVO; ATK SPD non modifica la soglia temporale di 5 s.
- RAGE dura finché il fuoco continua. Rilasciare il comando di ATTACCO azzera il contatore e, se attiva, termina immediatamente RAGE.
- I modificatori +30% ATK e −20% MOVE SPD sono calcolati sui valori CORRENTI di ATK e MOVE SPD al momento dell’applicazione, includendo i BONUS guadagnati in partita, non esclusivamente sui VALORI BASE. Alla fine di RAGE vengono rimossi esclusivamente i suoi modificatori.
- DOWN e CAMBIO AREA terminano RAGE e azzerano il contatore, anche se al CAMBIO AREA il comando di ATTACCO è mantenuto.
- STUN interrompe immediatamente RAGE e azzera il conteggio dei 5 s di FUOCO CONTINUATIVO. Il comportamento per altri stati futuri sarà definito quando verranno aggiunti.

### CLASSIFICAZIONE TECNICA ABILITÀ PG — CONFERMATO

Tutte le 16 ABILITÀ usano i LAYER esistenti o verifiche logiche, senza nuovi TAG o LAYER. Il PG mantiene il layer PG; stati e modificatori non ne cambiano le collisioni. Restano valide le proprietà e le eccezioni delle singole schede.

| PG | ABILITÀ | GESTIONE TECNICA |
| --- | --- | --- |
| PG01 | PESTONE | AREA_EFFECT_MOB oppure verifica HITSCAN istantanea su AREA RETTANGOLARE frontale 3 m × 7 m (larghezza frontale 3 m, profondità 7 m) sui MOB; nessuna collisione fisica persistente. MURO/OSTACOLO bloccano secondo le regole dell’AREA. |
| PG01 | BARRIERA | Layer OSTACOLO: blocca PG, MOB, PET, PROJECTILE_PG e PROJECTILE_MOB. |
| PG02 | FUOCO RAPIDO | Modificatore logico temporaneo +40% ATK SPD corrente; proiettili PROJECTILE_PG e collisioni invariate. |
| PG02 | FUOCO DI SOPPRESSIONE | Raffica di 50 PROIETTILI FISICI su PROJECTILE_PG in 3 s, 3 DANNO per PROIETTILE, RANGE 10 m. Distribuzione sequenziale sui CENTRI dei 5 SPICCHI da 10° del CONO di 50°, da destra a sinistra e ritorno, senza ripetere gli estremi. I PROIETTILI seguono le regole generali applicabili dei proiettili fisici; non usa una query HITSCAN a CONO né un collider AREA_EFFECT_MOB. |
| PG03 | COLPO LASER | Verifica HITSCAN/AREA rettangolare 20 × 2 m sui MOB, tramite query logica o AREA_EFFECT_MOB; ignora MURO/OSTACOLO e non usa PROJECTILE_PG. |
| PG03 | TRIPLO SPARO | I 3 colpi sono PROJECTILE_PG con le collisioni standard. |
| PG04 | PIOGGIA DI GRANATE | Le 10 esplosioni sono AREA_EFFECT_MOB, senza proiettili fisici; validazione logica dei 4 SPICCHI contro MURO/OSTACOLO. |
| PG04 | COLPO GROSSO | Stato logico a 4 cariche; conserva la gestione dell’ATTACCO BASE. Esplosione AREA_EFFECT_MOB con logica degli SPICCHI; nessun cambio delle collisioni. |
| PG05 | INVISIBILITÀ | Stato logico del PG; layer e collisioni normali invariati. |
| PG05 | COLTELLI AVVELENATI | Gli 8 coltelli sono PROJECTILE_PG con collisioni standard; VELENO applicato logicamente alla HIT. |
| PG06 | CURA AD AREA | Effetto logico istantaneo sui PG entro 5 m, tramite AREA_EFFECT_PG o query logica; nessuna collisione fisica persistente. Ignora MURO/OSTACOLO. |
| PG06 | FUOCO CURATIVO | Stato logico a 6 cariche; i colpi restano PROJECTILE_PG. CURA logica dopo HIT valida, senza proiettile/collider/layer aggiuntivo; ricerca entro 15 m secondo le priorità definite. |
| PG07 | MULTI SHOT | I 9 proiettili sono PROJECTILE_PG con collisioni standard; RESPINTA applicata logicamente alla HIT. |
| PG07 | PIOGGIA DI FRECCE | Nessun proiettile fisico di gameplay; ogni impatto verifica i MOB nell’AREA CIRCOLARE di raggio 0,25 m. Ignora MURO/OSTACOLO. |
| PG08 | FILO SPINATO | AREA_EFFECT_MOB persistente nelle 10 SEZIONI definite; nessuna barriera fisica, attraversabile da PG, MOB e proiettili. MURO/OSTACOLO servono alla validazione iniziale delle SEZIONI. |
| PG08 | COLPI RESPINGENTI | Stato logico temporaneo; proiettili PROJECTILE_PG e collisioni invariate. RESPINTA applicata alla HIT e soggetta alle normali collisioni. |

### CLASSIFICAZIONE TECNICA PASSIVE PG — CONFERMATO

Nessuna PASSIVA richiede nuovi TAG o LAYER. Le modifiche logiche mantengono layer e collisioni originali; valori e condizioni restano quelli delle schede.

| PG | PASSIVA | GESTIONE TECNICA |
| --- | --- | --- |
| PG01 | PASSIVA 1 | Modificatore logico DEF; layer PG e collisioni invariati. |
| PG01 | PASSIVA 2 | Modificatore logico ATK; layer PG e collisioni invariati. |
| PG02 | PASSIVA 1 | CURA logica al verificarsi delle condizioni di HP/KILL; nessun collider aggiuntivo. |
| PG02 | PASSIVA 2 | Contatore logico delle KILL e stato RAGE; collisioni invariate. |
| PG03 | CALIBRO PERFORANTE | Modifica logica dei PROJECTILE_PG dell’ATTACCO BASE: 2 PERFORAZIONI e DANNO 100% / 70% / 40% sulle tre HIT; layer e collisioni standard invariati. |
| PG03 | PUNTO DEBOLE | MARCHIO logico sul MOB; nessuna modifica di layer/collisioni. |
| PG04 | SCORTA ESPLOSIVA | Modifica della capacità degli SLOT ITEM; nessuna conseguenza su layer/collisioni. |
| PG04 | PYROMANIA | AREE INCENDIATE AREA_EFFECT_MOB generate dalle esplosioni; verifica logica degli SPICCHI contro MURO/OSTACOLO, senza collisione fisica dell’AREA. |
| PG05 | GHOSTING | Combinazione logica di INVISIBILITÀ e modificatore MOVE SPD; layer/collisioni invariati. |
| PG05 | LAMA DI CICUTA | Contatore logico che applica VELENO ai MOB colpiti dall’ATTACCO BASE al raggiungimento della soglia. |
| PG06 | ELEMOSINA | Il MEDI KIT generato è attraversabile e usa TRIGGER_PG secondo le regole esistenti. |
| PG06 | VITAMINA C | Modificatore logico temporaneo ATK SPD; il rinnovo della DURATA non modifica le collisioni. |
| PG07 | LUCKY SHOT | HIT AD AREA tramite AREA_EFFECT_MOB o verifica logica; validazione dei 4 SPICCHI contro MURO/OSTACOLO. |
| PG07 | CONCENTRAZIONE | Modifica logica della FRECCIA PROJECTILE_PG, PERFORANTE quando il roll ha successo; nessun cambio layer. |
| PG08 | TENACIA | Contatore e modificatore logico DEF; nessuna modifica fisica, anche all’azzeramento del BONUS. |
| PG08 | RAGE | Stato logico temporaneo del PG; collisioni normali. |

<a id="sezione-13"></a>

## 13. MOB

| MOB | HP | ATK | DEF | MOVE SPD | ATK SPD | RANGE | G BASE | EXP BASE |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ZOMB01 | 60 | 15 | 0% | 90 | 70 | 1 m | 10 G | 10 EXP |
| ZOMB02 | 40 | 25 | 0% | 100 | 120 | 1 m | 15 G | 15 EXP |
| ZOMB03 | 110 | 20 | +10% | 70 | 50 | 2,5 m | 20 G | 20 EXP |
| ZOMB04 | 50 | 35 | 0% | 70 | 75 | 30 m | 15 G | 15 EXP |
| ZOMB05 | 200 | 35 | +10% | 70 | 70 | 1 m | 25 G | 25 EXP |

### 13.1 REGOLE GENERALI MOB — AGGRO

- Ogni MOB considera l’intera AREA e seleziona il PG attivo non-DOWN più vicino, secondo la REGOLA DELLA DISTANZA. I PG in DOWN sono esclusi; resta valida l’eccezione INVISIBILITÀ della sezione 10.5.
- In caso di parità di distanza, sceglie CASUALMENTE tra i PG a pari distanza. Al ricalcolo applica nuovamente la regola e, se la parità persiste, effettua una nuova scelta casuale.
- RICALCOLO AGGRO ogni 0,5 s, scaglionato tra i MOB per evitare calcoli simultanei di tutti i MOB.
- Tra due ricalcoli mantiene il BERSAGLIO corrente. Al ricalcolo cambia BERSAGLIO se un altro PG valido risulta il più vicino, anche se quello precedente è ancora entro RANGE.
- Se il BERSAGLIO corrente entra in DOWN, il ricalcolo è immediato. Da quel ricalcolo riparte il normale intervallo di 0,5 s.
- Il cambio BERSAGLIO non resetta l’intervallo dell’ATK SPD.

### 13.2 REGOLE GENERALI MOB — MOVIMENTO E COLLISIONI

- Il MOB calcola il PERCORSO percorribile più breve verso il BERSAGLIO, aggirando MURI/OSTACOLI quando esiste un percorso valido. Adegua il percorso al cambio BERSAGLIO.
- MOB ↔ MOB: collisione fisica SÌ, con collider ridotto rispetto alla dimensione visiva del MOB, come nel TEST in engine, per rendere le orde più fluide. Non sono state definite dimensioni numeriche del collider. Questa regola sostituisce la precedente assenza di collisione fisica tra MOB e vale anche per ZOMB05 in PRE-ESPLOSIONE, senza eccezioni di attraversamento/sovrapposizione.
- MOB ↔ PG: collisione fisica; si bloccano fisicamente e non si attraversano. La semplice collisione non infligge DANNO e non applica RESPINTA al PG.
- MOB ↔ MURI/OSTACOLI: collisione fisica attiva, con blocco del MOVIMENTO.
- MOB ↔ PROJECTILE_PG: SÌ; MOB ↔ PROJECTILE_MOB: NO. Matrice consolidata nella sezione 10.9.
- Il DANNO deriva da ATTACCHI/HIT secondo la scheda del MOB.
- BERSAGLIO irraggiungibile: il MOB continua a tentare di raggiungerlo, collidendo con l’OSTACOLO. Nessun TELEPORT, distruzione dell’OSTACOLO o abbandono dell’inseguimento; la selezione resta soggetta alla REGOLA AGGRO. Questa situazione è da evitare nel LEVEL DESIGN.
- Distanza di arresto e comportamento entro RANGE sono specifici del MOB.

### 13.3 REGOLE GENERALI MOB — REAZIONE A HIT / DANNO E STUN

- Ricevere HIT/DANNO non cambia di per sé il comportamento del MOB: non cambia BERSAGLIO, non provoca RICALCOLO AGGRO, non interrompe MOVIMENTO/inseguimento e non modifica il ciclo di ATTACCO.
- Gli effetti specifici della HIT, come SLOW o RESPINTA, si applicano secondo le rispettive regole. Le eccezioni devono essere esplicitamente definite.
- Eccezione — STUN: interrompe immediatamente ogni AZIONE in corso. Durante STUN il MOB non compie AZIONI.
- Al termine dello STUN tutte le AZIONI ripartono da capo; nessuna riprende dal punto in cui era stata interrotta.
- Il TASER applica STUN. I valori già presenti sotto la denominazione BLOCK sono conservati nella sezione 22 e nei relativi UPGRADE.

### 13.4 REGOLE GENERALI MOB — MORTE E SPRITE DI MORTE

- A HP ≤0 il MOB entra immediatamente in MORTE e interrompe MOVIMENTO, ATTACCO e ogni altra AZIONE, salvo ZOMB05: esegue prima PRE-ESPLOSIONE ed ESPLOSIONE secondo la sezione 13.9.
- Non è più un MOB attivo, non può più essere selezionato/interagire come tale e non può generare nuove HIT.
- Nel momento esatto della MORTE avviene il DROP automatico di G/EXP secondo la sezione 16.
- La SPRITE normale è sostituita dalla SPRITE DI MORTE, visibile per 3 s, poi scompare.
- La SPRITE DI MORTE non genera COLLISIONI né con PG né con altri MOB.

### 13.5 ZOMB01 — MOVIMENTO E ATTACCO

- Si muove continuamente verso il BERSAGLIO, anche durante gli ATTACCHI, secondo le REGOLE GENERALI MOB.
- BERSAGLIO entro RANGE 1 m + ATTACCO disponibile → 1 HIT immediata, ATK 15 con applicazione della DEF del PG.
- ATK SPD 70 = 0,7 ATTACCHI/s (intervallo 100/70 s). Uscire successivamente dal RANGE non annulla la HIT già ricevuta.
- Se il BERSAGLIO esce dal RANGE durante l’intervallo, continua l’inseguimento. Alla scadenza, se non può attaccare, l’ATTACCO resta PRONTO e viene effettuato appena il BERSAGLIO valido è entro RANGE.
- Il cambio BERSAGLIO non resetta l’intervallo ATK SPD.

### 13.6 ZOMB02 — MOVIMENTO E ATTACCO

- Segue le stesse regole di ZOMB01 per MOVIMENTO e ATTACCO. Non possiede ABILITÀ, PASSIVE o comportamenti speciali; si differenzia esclusivamente per le STATS.
- MOVE SPD 100; ATK SPD 120 = 1,2 ATTACCHI/s; ATK 25; RANGE 1 m. Ogni ATTACCO genera 1 HIT immediata con applicazione della DEF del PG.
- La collisione con il PG blocca fisicamente il movimento, ma AGGRO/inseguimento restano attivi; attacca ogni volta che l’ATTACCO è disponibile e il BERSAGLIO è entro RANGE.

### 13.7 ZOMB03 — MOVIMENTO E ATTACCO

#### MOVIMENTO

- Segue le REGOLE GENERALI MOB di AGGRO, percorso e collisioni.
- Avanza fino a 1 m dal BERSAGLIO, quindi si ferma per ATTACCARE.
- Una volta fermo rimane tale finché il BERSAGLIO è entro RANGE 2,5 m. Se supera 2,5 m, riprende l’inseguimento fino a tornare a 1 m, salvo il vincolo dei primi 0,5 s dell’ATTACCO.

#### CICLO DELL’ATTACCO

| TEMPO | COMPORTAMENTO |
| --- | --- |
| T = 0 s | Inizia l’ATTACCO e registra la posizione del PG BERSAGLIO come punto d’impatto fisso. |
| 0 ≤ T < 0,5 s | ZOMB03 resta immobile. Anche se il BERSAGLIO esce dal RANGE, non riprende ancora l’inseguimento. |
| T = 0,5 s | Genera un’AREA CIRCOLARE di RAGGIO 1,5 m nel punto fissato a T = 0; valuta le HIT e termina il vincolo di MOVIMENTO. |
| 0,5 ≤ T < 2 s | Può muoversi secondo le normali regole; attende la disponibilità dell’ATTACCO successivo. |
| T = 2 s | Nuovo ATTACCO disponibile (ATK SPD 50). Se le condizioni non sono soddisfatte, resta PRONTO fino a quando lo saranno. |

- I primi 0,5 s sono inclusi nell’intervallo di 2 s, non aggiunti dopo.
- Il punto d’impatto è la posizione registrata del PG, non la posizione di ZOMB03. Non segue il PG e non viene modificato da cambi di AGGRO successivi a T = 0.
- Alla generazione, tutti i PG attivi non-DOWN nell’AREA valida ricevono 1 HIT, ATK 20 con DEF calcolata individualmente. Un PG uscito dall’AREA non riceve HIT; altri PG entrati nell’AREA possono riceverla.
- I PG in DOWN non ricevono HIT. Se il BERSAGLIO originale entra in DOWN nei primi 0,5 s, il punto fissato non cambia e l’AREA viene comunque generata, salvo interruzioni dell’ATTACCO.
- Ogni PG può ricevere massimo 1 HIT per ATTACCO. Un PG esattamente a 1,5 m dal centro è incluso.

#### AREA, MURI E OSTACOLI

- AREA divisa in 8 SPICCHI da 45°; un MURO/OSTACOLO che intercetta anche parzialmente uno SPICCHIO elimina l’intero SPICCHIO e le sue HIT.
- Anche con il centro vicino a MURI/OSTACOLI l’AREA viene generata a T = 0,5 s e ogni SPICCHIO viene valutato indipendentemente.
- Gli SPICCHI servono solo alla geometria. Se il PG appartiene ad almeno uno SPICCHIO valido, riceve 1 HIT; dopo quella HIT la valutazione per quel PG è conclusa, anche sul confine fra SPICCHI.

#### INTERRUZIONI ED EFFETTI

- MORTE prima della generazione (T < 0,5 s): ATTACCO annullato, nessuna AREA.
- RESPINTA prima della generazione (T < 0,5 s): ATTACCO interrotto, nessuna AREA. La prima RESPINTA lo interrompe; eventuali successive non possono interrompere di nuovo lo stesso ATTACCO.
- A T = 0,5 s l’AREA viene generata; una volta generata, MORTE o RESPINTA non possono annullare la HIT già prodotta.
- SLOW nei primi 0,5 s non interrompe l’ATTACCO; rimane applicato e influenza il MOVIMENTO quando ZOMB03 torna a potersi muovere.
- STUN prima della generazione interrompe l’ATTACCO e impedisce l’AREA. Dopo STUN l’eventuale nuovo ATTACCO riparte da T = 0, secondo la REGOLA GENERALE.
- Non sono definiti una carica di materiale infetto o un’esplosione del corpo di ZOMB03: l’ATTACCO è la generazione dell’AREA nel punto registrato.

### 13.8 ZOMB04 — MOVIMENTO, ATTACCO E PROIETTILE BALISTICO

#### STATS E AGGRO

- HP 50; ATK 35; DEF 100 / 0%; MOVE SPD 70; ATK SPD 75; RANGE 3000 = 30 m; DROP base 15 EXP / 15 G.
- Segue la REGOLA GENERALE DI AGGRO della sezione 13.1, incluso il ricalcolo ogni 0,5 s scaglionato e quello immediato quando il BERSAGLIO entra in DOWN.

#### MOVIMENTO E POSIZIONE DI ATTACCO

- Segue il BERSAGLIO calcolando il PERCORSO percorribile più breve e cerca sempre una posizione raggiungibile con TRAIETTORIA LIBERA verso di lui.
- Se non è possibile raggiungere una posizione con TRAIETTORIA LIBERA, entra in IDLE e continua a ricalcolare. Questa è l’eccezione specifica al comportamento generale verso un BERSAGLIO irraggiungibile.
- Non si ferma e non ATTACCA appena entra nel RANGE massimo di 30 m. Nell’avvicinamento continua finché la distanza è >15 m; a 15 m si ferma e può iniziare ad ATTACCARE.
- Una volta in posizione di ATTACCO rimane fermo e può continuare ad ATTACCARE con distanza compresa tra 9 m e 30 m, estremi inclusi, purché la TRAIETTORIA sia LIBERA.
- Se la distanza diventa >30 m, riprende il MOVIMENTO per raggiungere una nuova posizione valida.
- Se la distanza diventa <9 m, si allontana fino ad almeno 15 m. Durante il MOVIMENTO, incluso l’arretramento, non può ATTACCARE.
- Prima di ogni nuovo ATTACCO verifica la TRAIETTORIA LIBERA. Se è ostruita si riposiziona; se non esiste una posizione raggiungibile valida entra in IDLE e continua a ricalcolare.
- Al cambio AGGRO, se è in MOVIMENTO ricalcola immediatamente PERCORSO e posizione con TRAIETTORIA LIBERA rispetto al nuovo BERSAGLIO. Se è fermo verifica di nuovo distanza e TRAIETTORIA: mantiene la posizione se valida, altrimenti si riposiziona.
- Restano valide le REGOLE GENERALI DI COLLISIONE.

#### ATTACCO E PROIETTILE

- ATTACCA esclusivamente da fermo. ATK SPD 75 = 0,75 ATTACCHI/s (intervallo 100/75 s).
- Inizio ATTACCO = sparo del PROIETTILE BALISTICO; con lo sparo l’ATTACCO è concluso. Non esiste una fase di preparazione con ATTACCO iniziato e PROIETTILE non ancora sparato.
- La HIT avviene alla collisione con un bersaglio valido, non al momento dello sparo. DANNO 35 prima dell’applicazione della DEF.
- Il PROIETTILE segue le REGOLE DI COLLISIONE, mantiene la TRAIETTORIA iniziale e non segue il PG né corregge il percorso in base ai suoi spostamenti.
- RANGE PROIETTILE = RANGE ZOMB04 = 30 m. Scompare al raggiungimento del RANGE massimo o alla prima COLLISIONE.
- Una volta sparato prosegue autonomamente: cambio AGGRO, MOVIMENTO, STUN, RESPINTA o MORTE successivi di ZOMB04 non lo modificano. Anche una successiva ostruzione della TRAIETTORIA non annulla il PROIETTILE già sparato, che continua secondo le proprie regole di collisione.
- PROJECTILE SPD ZOMB04 = 8 m/s. PROJECTILE SPD standard 20 m/s è confermata per gli ATTACCHI BASE fisici dei PG. Per PROJECTILE_MOB sono confermate collisioni con PG, MURO e OSTACOLO; nessuna collisione con MOB, PROJECTILE_PG o PET (sezione 10.9).

#### STATI

- STUN segue le REGOLE GENERALI: interrompe le AZIONI; al termine ripartono da capo.
- RESPINTA non annulla di per sé l’ATTACCO. Dopo lo spostamento ZOMB04 applica nuovamente le proprie REGOLE DI MOVIMENTO, riposizionandosi se necessario.
- HIT/DANNO, MORTE, SPRITE DI MORTE e DROP seguono le REGOLE GENERALI, con l’indipendenza del PROIETTILE già sparato sopra specificata.

### 13.9 ZOMB05 — ATTACCO, PRE-ESPLOSIONE ED ESPLOSIONE

#### STATS, AGGRO E MOVIMENTO

- HP 200; ATK 35; DEF 110 / +10%; MOVE SPD 70; ATK SPD 70; RANGE 100 = 1 m; DROP base 25 EXP / 25 G.
- Segue la REGOLA GENERALE DI AGGRO: ricalcolo ogni 0,5 s scaglionato; ricalcolo immediato se il BERSAGLIO entra in DOWN.
- Si muove continuamente verso il BERSAGLIO, anche mentre ATTACCA, calcolando il PERCORSO percorribile più breve.
- Se il BERSAGLIO è irraggiungibile continua a tentare di raggiungerlo collidendo con l’OSTACOLO, senza TELEPORT, distruzione dell’OSTACOLO o abbandono dell’inseguimento. Segue le REGOLE GENERALI DI COLLISIONE.

#### ATTACCO BASE E REAZIONI

- Quando il BERSAGLIO entra nel RANGE di 1 m può ATTACCARE: HIT immediata sul BERSAGLIO, 35 DANNO prima della DEF.
- ATK SPD 70 = 0,7 ATTACCHI/s (intervallo 100/70 s). Se il PG esce dal RANGE tra due ATTACCHI, ZOMB05 continua ad avvicinarsi e ATTACCA nuovamente quando le condizioni tornano valide.
- Una normale HIT/DANNO non modifica il comportamento. RESPINTA e SLOW applicano normalmente i rispettivi effetti.
- STUN interrompe ogni AZIONE; al termine le AZIONI ripartono da capo. La PRE-ESPLOSIONE costituisce l’eccezione descritta sotto.

#### PRE-ESPLOSIONE — 2 s

- A 0 HP non entra ancora in MORTE: interrompe immediatamente tutte le AZIONI e avvia la PRE-ESPLOSIONE di 2 s.
- Durante i 2 s non può MUOVERSI autonomamente né ATTACCARE. Non riceve DANNO ed è influenzabile soltanto dalla RESPINTA; STUN non ha effetto.
- Qualsiasi effetto valido di RESPINTA può spostarlo. Se la stessa fonte infligge DANNO, il DANNO viene ignorato ma la RESPINTA si applica.
- La RESPINTA e le HIT non interrompono, riavviano o modificano il timer. L’ESPLOSIONE avviene nella posizione occupata al termine dei 2 s.
- I PROIETTILI dei PG continuano a COLLIDERE con ZOMB05 e terminano secondo le proprie regole, senza infliggergli DANNO.
- Mantiene il layer MOB e tutte le normali COLLISIONI fino all’ESPLOSIONE, inclusa MOB ↔ MOB SÌ. Continua a bloccare i PG e a collidere con MURO/OSTACOLO secondo la matrice generale. La precedente eccezione che consentiva agli altri MOB di attraversarlo e sovrapporsi è eliminata.

#### ESPLOSIONE — T = 2 s

- AREA CIRCOLARE centrata sulla posizione attuale di ZOMB05; RAGGIO 4 m; 8 SPICCHI da 45°, secondo le regole geometriche dell’AREA di ZOMB03.
- Un MURO/OSTACOLO che intercetta anche parzialmente uno SPICCHIO elimina l’intero SPICCHIO. Gli SPICCHI definiscono la geometria, non HIT separate.
- Massimo 1 HIT per bersaglio valido nell’AREA: 50 DANNO ai PG ATTIVI; 25 DANNO ai MOB, ossia il 50% in meno, prima dell’applicazione della rispettiva DEF.
- I PG in DOWN non ricevono HIT. Il DANNO ai MOB è un’eccezione specifica per questa ESPLOSIONE.

#### ESPLOSIONI A CATENA, MORTE E DROP

- Se l’ESPLOSIONE porta un altro ZOMB05 a 0 HP, quest’ultimo avvia la propria PRE-ESPLOSIONE completa di 2 s. Ogni elemento della catena mantiene il proprio timer completo.
- Uno ZOMB05 già in PRE-ESPLOSIONE riceve 0 DANNO da altre ESPLOSIONI e il suo timer non cambia.
- Dopo l’ESPLOSIONE entra effettivamente in MORTE: interrompe le AZIONI, avviene il DROP automatico di EXP/G ai destinatari validi e compare la SPRITE DI MORTE secondo le REGOLE GENERALI. La SPRITE resta 3 s senza COLLISIONI con PG o MOB.
- Il trigger di sostituzione SPAWN avviene solo all’ingresso in MORTE dopo l’ESPLOSIONE, non al raggiungimento di 0 HP.
- Sequenza: **0 HP → PRE-ESPLOSIONE 2 s → ESPLOSIONE → MORTE → DROP + SPRITE DI MORTE + trigger SPAWN**. Lo SPAWN resta soggetto alle quantità residue dell’AREA.

### 13.10 CLASSIFICAZIONE TECNICA ATTACCHI / EFFETTI MOB — CONFERMATO

| MOB / EFFETTO | GESTIONE TECNICA |
| --- | --- |
| ZOMB01 / ZOMB02 / ZOMB05 — ATTACCO MELEE | HIT logica istantanea sul PG entro RANGE; nessun proiettile o AREA persistente e nessun nuovo layer. |
| ZOMB03 — AREA DI DANNO | AREA_EFFECT_PG istantanea nel punto fissato a T = 0, generata al momento previsto dal ciclo dell’ATTACCO; HIT sui PG validi. Gli 8 SPICCHI da 45° sono validati logicamente contro MURO/OSTACOLO; nessuna collisione fisica persistente. |
| ZOMB04 — PROIETTILE BALISTICO | PROJECTILE_MOB: collide con PG, MURO e OSTACOLO; attraversa MOB, PET, PROJECTILE_PG e PROJECTILE_MOB, secondo la matrice generale. |
| ZOMB05 — PRE-ESPLOSIONE | Stato logico sul layer MOB; mantiene tutte le collisioni normali, inclusa MOB ↔ MOB SÌ. |
| ZOMB05 — ESPLOSIONE | Verifica logica unica dell’AREA che interroga sia PG sia MOB validi, senza forzare l’effetto in un solo AREA_EFFECT_PG o AREA_EFFECT_MOB. Gli 8 SPICCHI da 45° sono validati logicamente contro MURO/OSTACOLO; restano invariati danni, destinatari e massimo di 1 HIT per bersaglio. |

Non sono introdotti TAG o LAYER aggiuntivi; restano valide tutte le regole specifiche delle sezioni 13.1–13.9.

<a id="sezione-14"></a>

## 14. MOB PER AREA

- MINI BOSS e BOSS non sono conteggiati nel totale/massimo dei MOB previsto per l’AREA.

| CITTÀ | A1 | A2 | A3 | A4 | A5 | A6 |
| --- | --- | --- | --- | --- | --- | --- |
| C1 | 100 | 120 | 150 | — | — | — |
| C2 | 120 | 150 | 180 | 220 | — | — |
| C3 | 150 | 180 | 220 | 270 | 330 | — |
| C4 | 150 | 180 | 220 | 270 | 330 | — |
| C5 | 180 | 220 | 270 | 330 | 400 | 480 |

- Progressione generale: ogni AREA successiva aumenta il numero di MOB di circa il 20%, con arrotondamento per eccesso alla decina secondo i valori fissati in tabella.
### 14.1 DISTRIBUZIONE ZOMB01–ZOMB05 PER CITTÀ / AREA

| CITTÀ | AREA | ZOMB01 | ZOMB02 | ZOMB03 | ZOMB04 | ZOMB05 |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| C1 | A1 | 75% | 20% | 5% | — | — |
| C1 | A2 | 65% | 25% | 10% | — | — |
| C1 | A3 | 55% | 30% | 15% | — | — |
| C2 | A1 | 75% | 20% | 5% | — | — |
| C2 | A2 | 60% | 25% | 10% | 5% | — |
| C2 | A3 | 51% | 28% | 14% | 7% | — |
| C2 | A4 | 42% | 31% | 18% | 9% | — |
| C3 | A1 | 60% | 25% | 10% | 5% | — |
| C3 | A2 | 51% | 28% | 14% | 7% | — |
| C3 | A3 | 42% | 31% | 18% | 9% | — |
| C3 | A4 | 33% | 34% | 22% | 11% | — |
| C3 | A5 | 32% | 33% | 21% | 10% | 4% |
| C4 | A1 | 53% | 15% | 14% | 10% | 8% |
| C4 | A2 | 44% | 17% | 18% | 12% | 9% |
| C4 | A3 | 35% | 19% | 22% | 14% | 10% |
| C4 | A4 | 26% | 21% | 26% | 16% | 11% |
| C4 | A5 | 17% | 23% | 30% | 18% | 12% |
| C5 | A1 | 50% | 16% | 14% | 11% | 9% |
| C5 | A2 | 41% | 17% | 18% | 13% | 11% |
| C5 | A3 | 32% | 18% | 22% | 15% | 13% |
| C5 | A4 | 23% | 19% | 26% | 17% | 15% |
| C5 | A5 | 14% | 20% | 30% | 19% | 17% |
| C5 | A6 | 5% | 21% | 34% | 21% | 19% |

- «—» indica tipo non presente nell’AREA. I valori riproducono la tabella approvata, senza correzioni automatiche.
- C5: correzione confermata; tutte le righe totalizzano 100%.
- C1 e C2_A1 restano invariati. Da C2_A2 a C2_A4, per ogni AREA: ZOMB01 −9 punti percentuali / ZOMB02 +3 / ZOMB03 +4 / ZOMB04 +2.
- C3_A1 riparte dalla composizione di C2_A2 e segue la stessa progressione fino ad A4. In C3_A5 ZOMB05 entra al 4%, sottraendo 1 punto percentuale a ciascuno degli altri quattro ZOMB rispetto a C3_A4.
- Da C4_A1, per ogni nuova AREA di C4: ZOMB01 −9 / ZOMB02 +2 / ZOMB03 +4 / ZOMB04 +2 / ZOMB05 +1 punti percentuali.
- Da C5_A1, per ogni nuova AREA di C5: ZOMB01 −9 / ZOMB02 +1 / ZOMB03 +4 / ZOMB04 +2 / ZOMB05 +2 punti percentuali.
- Le quantità devono essere intere; gli arrotondamenti sono a discapito di ZOMB01, che assorbe le correzioni necessarie per mantenere esatto il totale previsto. Questa regola sostituisce quella precedente che favoriva ZOMB01.

<a id="sezione-15"></a>

## 15. SISTEMA DI SPAWN

- MINI BOSS e BOSS non sono conteggiati né nel totale/massimo dei MOB dell’AREA né nel limite massimo dei MOB contemporaneamente presenti.
- Il punto di SPAWN di MINI BOSS e BOSS è un punto specifico stabilito in fase di LEVEL DESIGN. Tutto il resto del loro funzionamento rimane DA DEFINIRE.

### 15.1 FIRST SPAWN E COMPOSIZIONE

- FIRST SPAWN = 30% del totale MOB previsto nell’AREA: questi MOB sono già presenti quando i PG entrano. Il restante 70% viene generato progressivamente durante il combattimento.
- La porzione inizialmente visibile dell’AREA è libera da MOB. Il numero di MOB contemporaneamente presenti non supera il FIRST SPAWN.
- All’inizio dell’AREA si determinano le quantità totali di ogni tipo usando il totale MOB e le percentuali della sezione 14.1.
- La composizione del FIRST SPAWN segue le stesse percentuali dell’AREA, con l’eccezione di ZOMB05 descritta sotto.
- Le quantità sono sempre intere: arrotondamenti a discapito di ZOMB01, che assorbe le correzioni affinché il totale previsto rimanga esatto.
- ZOMB05 non può essere presente nel FIRST SPAWN, anche nelle AREE che lo prevedono. La sua quota iniziale viene redistribuita 50% a ZOMB01 e 50% a ZOMB02; in caso di divisione decimale l’unità eccedente va a ZOMB02, a discapito di ZOMB01.
- ZOMB03 e ZOMB04 mantengono la quantità calcolata per il FIRST SPAWN; il numero totale iniziale non cambia.
- I MOB già generati nel FIRST SPAWN vengono sottratti dalle quantità totali previste per tipo. L’esclusione iniziale di ZOMB05 non modifica la composizione finale dell’AREA: ZOMB05 compare solo negli SPAWN successivi.

### 15.2 SPAWN SUCCESSIVI — QUANTITÀ E PROBABILITÀ

- **1 MORTE → 1 nuovo SPAWN**, finché tutti i MOB previsti per l’AREA sono stati generati. Il trigger è l’ingresso effettivo in MORTE, non il solo raggiungimento di 0 HP.
- Per ZOMB05: **0 HP → PRE-ESPLOSIONE 2 s → ESPLOSIONE → MORTE → trigger SPAWN**.
- La selezione usa le quantità residue di ZOMB01–ZOMB05. Le quantità massime per tipo si riferiscono al totale generato nell’AREA, inclusi i MOB del FIRST SPAWN.
- La probabilità iniziale di selezionare ogni tipo è uguale alla sua percentuale prevista per l’AREA.
- Quando un tipo raggiunge la propria quantità massima viene escluso dagli SPAWN successivi e la sua probabilità diventa 0%.
- Le probabilità dei tipi ancora disponibili aumentano proporzionalmente alle loro percentuali originali, mantenendo il loro rapporto e riportando la somma al 100%. Il ricalcolo si ripete a ogni esaurimento di un tipo.
- Terminati tutti i quantitativi previsti non vengono generati altri MOB. Il totale finale di ciascun tipo deve corrispondere alla composizione dell’AREA.

### 15.3 POSIZIONAMENTO — PG ATTIVO, OFF-SCREEN E NAVMESH

- A ogni SPAWN viene scelto CASUALMENTE 1 PG ATTIVO come riferimento. I PG in DOWN o MORTE sono esclusi. La scelta si ripete per ogni MOB, anche se i PG sono separati.
- La posizione deve essere **OFF-SCREEN rispetto a tutti i PG ATTIVI**, non solo rispetto al PG scelto.
- La ricerca preferenziale avviene in prossimità del PG scelto, nella fascia da OFF-SCREEN a 5 m oltre il limite visibile.
- I 5 m non sono un limite massimo assoluto: se non esiste un punto valido nella fascia, la ricerca viene estesa progressivamente oltre 5 m e ricalcolata fino a trovare una posizione valida. OFF-SCREEN rimane obbligatorio.
- Il punto deve essere valido sulla NAVMESH e deve esistere un percorso NAVMESH da esso verso almeno un PG ATTIVO. I punti non validi o non raggiungibili vengono scartati.
- Sono ammessi punti dietro MURI/OSTACOLI se rispettano OFF-SCREEN e permettono di raggiungere almeno un PG ATTIVO tramite NAVMESH. Non è necessaria una linea di vista diretta.
- Il FIRST SPAWN segue le stesse regole di posizionamento. All’ingresso nella nuova AREA i PG sono tutti vicini.

<a id="sezione-16"></a>

## 16. SISTEMA DI DROP

- Alla MORTE del MOB, G ed EXP vengono assegnati immediatamente e automaticamente ai destinatari validi.
- Non sono lasciati fisicamente a terra e non richiedono RACCOLTA.
- EXP → PG; G → PLAYER. In entrambi i casi il valore è assegnato integralmente a ogni destinatario valido e non viene suddiviso.
- La regola di assegnazione automatica riguarda G/EXP; ELEMOSINA mantiene il proprio MEDI KIT fisico raccoglibile.

### 16.1 G

- G DROP base: ZOMB01 10 G; ZOMB02 15 G; ZOMB03 20 G; ZOMB04 15 G; ZOMB05 25 G.
- Ogni PLAYER che possiede almeno un PG non in MORTE riceve il 100% del valore G del MOB. Se tutti i suoi PG sono in MORTE, non riceve G. I PG in DOWN soddisfano la condizione di non essere in MORTE.
- Ogni PLAYER possiede un saldo G personale e indipendente. Il G viene accreditato integralmente nel saldo di ciascun PLAYER valido e non viene diviso né inserito in un saldo condiviso della PARTY.
- Gli acquisti e gli UPGRADE sono personali per ogni PLAYER.
- L’UPGRADE G DROP acquistato da un PLAYER modifica solo il G ricevuto da quel PLAYER; non modifica il G ricevuto dagli altri PLAYER.
- Esempio senza UPGRADE G DROP: se un MOB droppa 10 G, ciascun PLAYER valido riceve 10 G nel proprio saldo personale.
- In SINGLE PLAYER il G è relativo al PLAYER.
- In CO-OP il G viene assegnato integralmente a ciascun PLAYER valido secondo la condizione sopra.
- Il G viene mantenuto tra le RUN, salvo la penalità di sconfitta descritta più avanti.

### 16.2 EXP

- EXP DROP base iniziale uguale al G DROP base: ZOMB01 10 / ZOMB02 15 / ZOMB03 20 / ZOMB04 15 / ZOMB05 25.
- Ogni PG della PARTY non in MORTE riceve il 100% del valore EXP del MOB, inclusi i PG in DOWN. I PG in MORTE non ricevono EXP. Il valore non viene diviso fra i PG.
- A ogni passaggio in una NUOVA AREA, l'EXP DROP aumenta del 5% rispetto all'AREA precedente.
- La progressione dell'EXP DROP continua anche passando a una nuova CITTÀ e non si resetta.
- Il MOLTIPLICATORE EXP viene arrotondato per difetto eliminando i decimali; il valore arrotondato diventa il riferimento dell'AREA successiva.
- Dopo l'applicazione del moltiplicatore, l'EXP DROP effettiva del singolo MOB viene arrotondata per eccesso eliminando i decimali.

| AREA RUN | MOLTIPLICATORE EXP |
| --- | --- |
| 1 | 100% |
| 2 | 105% |
| 3 | 110% |
| 4 | 115% |
| 5 | 120% |
| 6 | 126% |
| 7 | 132% |
| 8 | 138% |
| 9 | 144% |
| 10 | 151% |

### 16.3 DESTINATARI VALIDI AL MOMENTO DEL DROP

| DESTINATARIO | CONDIZIONE | ASSEGNAZIONE |
| --- | --- | --- |
| PG | ATTIVO | 100% EXP |
| PG | DOWN | 100% EXP |
| PG | MORTE | Nessuna EXP |
| PLAYER | Almeno un proprio PG non in MORTE, anche se DOWN | 100% G |
| PLAYER | Tutti i propri PG in MORTE | Nessun G |

- Esempio senza UPGRADE G DROP: MOB con valore 20 EXP + 20 G, 4 PG validi e 2 PLAYER validi → 20 EXP a ciascun PG e 20 G nel saldo personale di ciascun PLAYER.

<a id="sezione-17"></a>

## 17. SISTEMA DI PROGRESSIONE EXP / LVL

- Ogni PG IA ha EXP, LVL, BONUS e progressione individuale durante la RUN identici a quelli degli altri PG.

- EXP necessaria per LVL 1 → 2: 100.
- Ogni livello successivo richiede il 15% di EXP in più rispetto al precedente.
- Il valore viene arrotondato alla decina più vicina con criterio matematico: dividere per 10 l’EXP richiesta risultante dal +15%, arrotondare il quoziente all’intero più vicino (frazione <0,5 per difetto; frazione ≥0,5 per eccesso) e moltiplicare per 10.
- I valori della tabella LVL 1 → 11 restano invariati; per i passaggi successivi si applicano il +15% rispetto al precedente e l’arrotondamento alla decina sopra definito.
- Nessun CAP di LVL durante la RUN.
- L'EXP e il LVL della RUN sono temporanei: vengono persi quando la RUN termina o quando si torna all'HUB dopo un BOSS.

| PASSAGGIO | EXP RICHIESTA |
| --- | --- |
| LVL 1 → 2 | 100 |
| LVL 2 → 3 | 120 |
| LVL 3 → 4 | 130 |
| LVL 4 → 5 | 150 |
| LVL 5 → 6 | 170 |
| LVL 6 → 7 | 200 |
| LVL 7 → 8 | 230 |
| LVL 8 → 9 | 270 |
| LVL 9 → 10 | 310 |
| LVL 10 → 11 | 350 |

<a id="sezione-18"></a>

## 18. SISTEMA BONUS — REGOLE GENERALI

- Ogni PG dispone di 3 SLOT BONUS.
- Una nuova ABILITÀ BONUS occupa 1 SLOT BONUS.
- Quando tutti e 3 gli SLOT sono occupati, il PG non può più ricevere nuove ABILITÀ BONUS: per questa categoria può scegliere soltanto UPGRADE delle ABILITÀ BONUS possedute. I BONUS STATS BASE non occupano SLOT BONUS e possono continuare a essere proposti, inclusi quelli di fine AREA.
- Un UPGRADE può comparire anche prima che i 3 SLOT siano pieni, purché il relativo BONUS sia già stato acquisito.
- Gli UPGRADE possono comparire solo per ABILITÀ BONUS già possedute.
- Le PASSIVE personali dei PG non occupano SLOT BONUS.

<a id="sezione-19"></a>

## 19. CHEST

- CHEST completamente attraversabili, senza collisione fisica con PG o MOB; rilevamento tramite collider IsTrigger sul layer TRIGGER_PG (sezione 10.9).

- Prima CHEST garantita in ogni AREA: probabilità di SPAWN 100%.
- Il limite di 1 CHEST per AREA è eliminato. CHEST RATE determina la probabilità di SPAWN di una seconda CHEST: +1 punto percentuale per acquisto dell’UPGRADE PROF, con CAP 100%.
- CHEST RATE è legato al PLAYER. All’inizio di una RUN CO-OP si applica il BONUS CHEST RATE più alto tra i PLAYER presenti.
- La CHEST assegna un BONUS a ogni PG della PARTY non in MORTE; ogni PG riceve il proprio BONUS casualmente e separatamente.
- Anche il PG IA riceve automaticamente il proprio BONUS CHEST con le stesse regole degli altri PG, senza una scelta specifica.
- I BONUS CHEST riguardano esclusivamente le STATS BASE.
- Il VALORE BASE di riferimento, comune ai BONUS CHEST e ai BONUS STATS DI FINE AREA, è il VALORE ATTUALE della STAT al momento dell’acquisizione, comprensivo degli UPGRADE permanenti del PROF e di tutti i precedenti BONUS STATS acquisiti durante la RUN, sia da CHEST sia da FINE AREA. Questa regola sostituisce quella precedente basata sulla STAT all’avvio della RUN. Le PASSIVE influenzano le STATS CORRENTI.
- STATS possibili: HP, ATK, DEF, MOVE SPD, ATK SPD, CD REDUCTION. RANGE escluso.
- Ogni BONUS CHEST aumenta la STAT del 5% del suo VALORE ATTUALE; il BONUS CD REDUCTION riduce invece la STAT CD REDUCTION del 5% del suo VALORE ATTUALE. La STAT CD REDUCTION risultante viene arrotondata matematicamente all’intero più vicino (<0,5 per difetto; ≥0,5 per eccesso), rispettando il minimo 10: per esempio 100 → 95 → 90 (95 × 0,95 = 90,25, arrotondato a 90). La riduzione si applica alla STAT, non direttamente ai secondi del CD BASE delle ABILITÀ; resta valido l’arrotondamento del CD FINALE al decimo di secondo.

| BONUS STAT CHEST | RATE |
| --- | --- |
| HP | 25% |
| ATK | 8% |
| DEF | 22% |
| MOVE SPD | 22% |
| ATK SPD | 8% |
| CD REDUCTION | 15% |

### GENERAZIONE, RACCOLTA E BONUS HP

- Durante la SCHERMATA DI CARICAMENTO di ogni NUOVA AREA si calcola lo SPAWN: prima CHEST garantita e verifica del CHEST RATE per l’eventuale seconda CHEST.
- Ogni CHEST viene posizionata CASUALMENTE in una zona raggiungibile dal NAVMESH, ad almeno 80 m dalla ZONA DI INIZIO e almeno 10 m da MEDI KIT e altre CHEST. Se la posizione non è valida, ricalcolare finché viene trovata una posizione valida.
- Un solo PG nel TRIGGER apre automaticamente la CHEST, anche in combattimento; non serve F. Forma e dimensioni del TRIGGER saranno definite in sviluppo/testing.
- I PG IA non cercano autonomamente CHEST: possono aprirle solo incontrandole durante il normale movimento.
- BONUS immediato a tutti i PG ATTIVI e in DOWN, indipendentemente dalla posizione nell’AREA; i PG in MORTE sono esclusi. Sorteggio individuale con i RATE della tabella.
- Ogni PLAYER riceve un AVVISO A SCHERMO relativo al BONUS del proprio PG: visibile, comprensibile e non invasivo. Nessuna pausa del GAMEPLAY.
- Dopo l’assegnazione la CHEST sparisce e non può essere riutilizzata.
- BONUS HP: l’aumento degli HP MASSIMI viene aggiunto nella stessa quantità agli HP correnti. L’incremento è il 5% degli HP MASSIMI attuali nel momento dell’acquisizione, comprensivi degli UPGRADE permanenti del PROF e dei precedenti BONUS STATS acquisiti durante la RUN da CHEST e FINE AREA.

<a id="sezione-20"></a>

## 20. LEVEL UP

- Raggiunta una soglia EXP, il GAMEPLAY va automaticamente e completamente in PAUSA. Durante la pausa non avvengono combattimenti, KILL o nuova assegnazione EXP.
- Per ogni PG interessato si generano sempre 3 BANNER; il PLAYER ne sceglie 1 e il BONUS è applicato immediatamente al relativo PG.
- Per ciascun PG IA, la scelta del BONUS di LEVEL UP spetta al PLAYER responsabile.
- I BANNER contengono NUOVE ABILITÀ BONUS oppure UPGRADE specifici di ABILITÀ BONUS già possedute. Non c’è una scelta intermedia di ABILITÀ o UPGRADE dopo il click sul BANNER.

### 20.1 GENERAZIONE BANNER

| SLOT BONUS OCCUPATI | NUOVA ABILITÀ | UPGRADE |
| --- | --- | --- |
| 0 | 100% | 0% |
| 1–2 | 50% | 50% |
| 3 | 0% | 100% |

- Il sorteggio della categoria avviene per ogni BANNER.
- NUOVA ABILITÀ: escludere le già possedute; mantenere i RATE originali (sezione 23) come pesi e normalizzare proporzionalmente al 100% fra le rimanenti.
- UPGRADE: scegliere prima una delle ABILITÀ possedute, pesandola con il RATE DI APPARIZIONE originale normalizzato sulle sole possedute; poi sorteggiare lo specifico UPGRADE con i RATE della sezione 25.
- Il BANNER mostra il risultato completo, per esempio «LANCIO COLTELLI — RANGE +15%».
- I 3 BANNER sono differenti: niente duplicati della stessa NUOVA ABILITÀ o dello stesso specifico UPGRADE. Sono ammessi più UPGRADE della stessa ABILITÀ se diversi (es. DANNO e RANGE).
- Scartare e rigenerare i duplicati fino a ottenere 3 scelte distinte. Non sono previsti POOL con meno di 3 scelte valide e non si mostrano meno di 3 BANNER.
- Lo stesso UPGRADE può essere acquisito nuovamente in scelte successive, senza CAP per ora, anche per gli UPGRADE SPECIALI. Ogni incremento usa il VALORE BASE originale.

### 20.2 MULTI LVL UP E RIPRESA

- Se una singola assegnazione EXP supera più soglie, calcolare tutti i LVL ottenuti e conservare l’EXP residua.
- Un’unica PAUSA contiene una SCELTA BONUS per ogni LVL guadagnato, in ordine.
- Applicare il BONUS di ciascun LVL prima di generare i 3 BANNER del successivo, aggiornando le opzioni disponibili.
- In CO-OP si riprende quando tutti i PLAYER hanno completato le scelte pendenti; in SINGLE PLAYER quando sono completate per tutti i PG. Completare tutti i MULTI LVL UP prima di riprendere.

<a id="sezione-21"></a>

## 21. COMPLETAMENTO AREA

- A fine AREA ogni PLAYER sceglie 1 BONUS tra 5 proposte.
- Per ciascun PG IA assegnato, il PLAYER responsabile sceglie il relativo BONUS DI FINE AREA tra le 5 proposte.
- La composizione dei 5 BONUS è fissa: 3 BONUS STATS BASE + 2 BONUS relativi alle ABILITÀ BONUS.
- I BONUS STATS DI FINE AREA seguono gli stessi RATE delle CHEST: HP 25%, ATK 8%, DEF 22%, MOVE SPD 22%, ATK SPD 8%, CD REDUCTION 15%.
- Ogni BONUS STATS DI FINE AREA applica lo stesso +5% delle CHEST sul VALORE ATTUALE della STAT al momento dell’acquisizione, comprensivo degli UPGRADE permanenti del PROF e di tutti i precedenti BONUS STATS acquisiti durante la RUN, sia da CHEST sia da FINE AREA.
- Per CD REDUCTION si applica una riduzione del 5% sul VALORE ATTUALE della STAT, con arrotondamento matematico del risultato all’intero più vicino e minimo 10; resta distinto l’arrotondamento del CD FINALE al decimo di secondo, secondo le sezioni 10 e 19.
- Il completamento AREA può quindi comprendere entrambe le categorie già definite.
- Dopo le scelte e la conferma di tutti i PLAYER, la PARTY passa alla nuova AREA, salvo il ritorno all’HUB scelto dall’HOST dopo il BOSS (sezione 29.1).
- Al completamento dell’AREA dopo il BOSS, la decisione tra PROSEGUIRE e TORNARE ALL’HUB spetta all’HOST. Solo nella schermata dell’HOST compare il tasto TORNA ALL’HUB.

<a id="sezione-22"></a>

## 22. ABILITÀ BONUS — STATS

| # | ABILITÀ BONUS | STATS BASE |
| --- | --- | --- |
| 1 | LANCIO COLTELLI | DANNO 10; RANGE 10 m; CD 6 s; N° COLTELLI 1; PROJECTILE SPD 20 m/s; suddivisione COLTELLI 45°. |
| 2 | PET | DANNO 10; ATK SPD 50; RANGE 2 m; ricerca MOB entro 5 m; N° PET 1; sempre attivo. |
| 3 | AURA TOSSICA | DANNO 10; RAGGIO 4 m; CD 7 s. |
| 4 | RICOCHET | DANNO rimbalzo 40% del danno originale; RANGE rimbalzo 5 m; N° RIMBALZI 1; probabilità di attivazione 40% per ogni HIT valida. |
| 5 | MINE | DANNO 10; RAGGIO 3 m; CD 7 s; esplode se calpestata o dopo 5 s; TRIGGER circolare, raggio 1 m. |
| 6 | SCUDO | Elemento separato con propria DEF 40%, massimo 90%; mitiga prima del PLAYER, poi il residuo viene mitigato dalla DEF del PLAYER (sezione 10). Dura fino alla prima HIT; CD 6 s dopo la scomparsa. |
| 7 | FIRE BULLET | ATK +15%; durata 3 s; BRUCIATURA 2 HP/s per 2 s; CD 12 s. |
| 8 | TASER | DANNO 5; RAGGIO 4 m; BLOCK 2 s (STUN); CD 12 s. |
| 9 | REPULSE | DANNO 5; RAGGIO 4 m; RESPINTA 3 m; CD 10 s. |
| 10 | SCIABOLATA | DANNO 15; semicerchio frontale; RANGE 4 m; CD 15 s. |

### 22.1 LANCIO COLTELLI — DEFINIZIONI CONFERMATE

- **1.1:** attivazione automatica secondo il proprio CD.
- **1.2:** un COLTELLO segue il CURSORE; gli altri seguono la suddivisione in gradi. I COLTELLI aggiuntivi sono distribuiti ogni 45° rispetto a quello orientato verso il CURSORE.
- **1.3:** PROIETTILI FISICI, velocità 20 m/s; seguono le regole dei PROIETTILI FISICI (sezione 10.1).

### 22.2 PET — DEFINIZIONI CONFERMATE

- **2.1:** il PET segue il PG e attacca automaticamente secondo le regole del suo RANGE e ATK SPD.
- **2.2:** segue i movimenti del PG; se il PG è fermo si posiziona in maniera CASUALE all'interno di una AREA di 3 m attorno al PG.
- **2.3:** il PET attacca il MOB più vicino in un RANGE di 5 m da lui; quando si trova a 2 m dal BERSAGLIO lo attacca.
- **2.4:** l'ATTACCO è una HIT istantanea sul BERSAGLIO selezionato.
- **2.5:** quando il PET è a 2 m dal BERSAGLIO lo attacca.
- **2.6:** evita MURI/OSTACOLI, sceglie e segue i BERSAGLI validi secondo RANGE e NAV MESH.
- **2.7:** i PET seguono tutti le stesse regole, collidono tra di loro.
- LAYER PET: PET ↔ PG NO; PET ↔ MOB SÌ; PET ↔ PET SÌ; PET ↔ MURO SÌ; PET ↔ OSTACOLO SÌ; PET ↔ PROJECTILE_PG NO; PET ↔ PROJECTILE_MOB NO.
- I PET non bloccano né vengono bloccati dai PG; non attraversano MOB, MURI o OSTACOLI e mantengono il movimento tramite NAV MESH già definito.
- I PET non hanno una meccanica HP/DANNO. I proiettili dei PG e dei MOB li attraversano completamente senza HIT, DANNO, distruzione o deviazione del proiettile. L’assenza di collisione evita che i PET funzionino come scudi mobili.

### 22.3 AURA TOSSICA — DEFINIZIONI CONFERMATE

- **3.1:** attivazione automatica secondo CD.
- **3.2:** il centro dell'AREA è posizionato sul PG.
- **3.3:** il DANNO è ISTANTANEO.
- **3.4:** l'effetto è ISTANTANEO.
- **3.5:** l'AREA DI EFFETTO è divisa in 8 SPICCHI e segue le regole delle AREE DI EFFETTO su MURI/OSTACOLI (sezione 10.3).

### 22.4 RICOCHET — DEFINIZIONI CONFERMATE

- **4.1:** probabilità di attivazione 40% per ogni HIT valida.
- **4.2:** il BERSAGLIO è il MOB più vicino nel RANGE di attivazione.
- **4.3:** il RANGE RIMBALZO è il RANGE entro il quale il RIMBALZO può verificarsi.
- **4.4:** il RIMBALZO non ha traiettoria; è un DANNO ISTANTANEO sul BERSAGLIO selezionato.
- **4.5:** MURI/OSTACOLI non interferiscono sul RIMBALZO.
- **4.6:** ogni RIMBALZO segue il comportamento del precedente.

### 22.5 MINE — DEFINIZIONI CONFERMATE

- **5.1:** ogni MINA viene generata a 1 m dal PG nella direzione opposta al CURSORE.
- **5.2:** le MINE restano sul terreno fino all'ATTIVAZIONE o alla scomparsa.
- **5.3:** le MINE hanno un TRIGGER circolare di raggio 1 m che si attiva quando un MOB ci entra in contatto. Il TRIGGER usa TRIGGER_MOB; le MINE sono attraversabili fisicamente da PG e MOB, senza collisione fisica.
- **5.4:** dopo 5 s, se la MINA non viene attivata, esplode automaticamente infliggendo DANNO nella sua AREA DI EFFETTO.
- **5.5:** i BERSAGLI validi sono tutti i MOB.
- **5.6:** l'AREA DI EFFETTO è divisa in 4 SPICCHI e segue le regole delle altre AREE DI EFFETTO con MURI/OSTACOLI (sezione 10.3).

### 22.6 SCUDO — DEFINIZIONI CONFERMATE

- **6.1:** allo scadere del CD lo SCUDO viene generato automaticamente.
- **6.2:** tutti i DANNI dei MOB sono HIT valide.
- **6.3:** la HIT che distrugge lo SCUDO viene mitigata dalla DEF dello SCUDO, poi infligge DANNO al PG secondo le regole generali.
- **6.4:** gli effetti che non infliggono DANNO non interagiscono con lo SCUDO.
- **6.5:** in DOWN e MORTE lo SCUDO viene disattivato; il CD riparte ma lo SCUDO non può essere riattivato fin quando il PG non è in stato ATTIVO. Il CAMBIO AREA non disattiva lo SCUDO.

### 22.7 FIRE BULLET — DEFINIZIONI CONFERMATE

- **7.1:** FIRE BULLET si attiva automaticamente allo scadere del CD.
- **7.2:** FIRE BULLET ha effetto solo sugli ATTACCHI BASE.
- **7.3:** ogni HIT affetta da FIRE BULLET applica BRUCIATURA al BERSAGLIO.
- **7.4:** ogni nuova HIT applica un’ISTANZA di BRUCIATURA secondo la REGOLA GENERALE DANNI DA STATO (sezione 10.8). DANNO base 2 HP/s e DURATA base 2 s invariati; DANNO per tick = DANNO base della BRUCIATURA × numero ISTANZE, fino a 5 ISTANZE. Ogni applicazione rinnova la DURATA completa, anche al CAP; primo tick 1 s dopo l’applicazione/rinnovo, poi ogni 1 s. Le ISTANZE non hanno DURATE indipendenti.
- **7.5:** UPGRADE DANNO ha effetto solo sul DANNO dell'effetto BRUCIATURA; il BONUS ATK di FIRE BULLET si somma all'ATK corrente del PG.
- **7.6:** quando FIRE BULLET è attivo, la BRUCIATURA viene applicata a tutti i MOB colpiti dall’ATTACCO BASE, inclusi gli ATTACCHI BASE ad AREA/multipli. Ogni MOB colpito riceve la propria istanza di BRUCIATURA.

### 22.8 TASER — DEFINIZIONI CONFERMATE

- **8.1:** attivazione automatica allo scadere del CD.
- **8.2:** il centro dell'AREA è posizionato sul PG.
- **8.3:** tutti i MOB ATTIVI all'interno dell'AREA DI EFFETTO sono BERSAGLI validi.
- **8.4:** il DANNO e lo STUN si applicano istantaneamente.
- **8.5:** l'AREA DI EFFETTO è divisa in 6 SPICCHI e segue le regole delle AREE DI EFFETTO con MURI/OSTACOLI (sezione 10.3).
- **8.6:** se un MOB riceve lo STUN quando è già sotto effetto di STUN, la DURATA si rinnova.

### 22.9 REPULSE — DEFINIZIONI CONFERMATE

- **9.1:** attivazione automatica allo scadere del CD.
- **9.2:** il centro dell'AREA è sul PG.
- **9.3:** tutti i MOB ATTIVI all'interno dell'AREA DI EFFETTO sono BERSAGLI validi.
- **9.4:** la direzione è calcolata dal PG verso il MOB colpito.
- **9.5:** l'AREA DI EFFETTO è divisa in 8 SPICCHI e segue le regole generali delle AREE DI EFFETTO con MURI/OSTACOLI (sezione 10.3).
- **9.6:** se durante la RESPINTA il MOB incontra un MURO/OSTACOLO si ferma.

### 22.10 SCIABOLATA — DEFINIZIONI CONFERMATE

- **10.1:** attivazione automatica allo scadere del CD.
- **10.2:** l'orientamento segue la posizione del CURSORE.
- **10.3:** semicerchio.
- **10.4:** tutti i MOB ATTIVI all'interno dell'AREA DI EFFETTO sono BERSAGLI validi.
- **10.5:** l’AREA a SEMICERCHIO è divisa in 4 SPICCHI e segue le regole generali delle AREE DI EFFETTO con MURI/OSTACOLI (sezione 10.3). Se un MURO/OSTACOLO intercetta anche parzialmente uno SPICCHIO, l’intero SPICCHIO viene eliminato e non genera HIT. Gli SPICCHI sono una suddivisione geometrica e non generano HIT aggiuntive.
- **10.6:** HIT istantanea.

### 22.11 CLASSIFICAZIONE TECNICA ABILITÀ BONUS — CONFERMATO

| ABILITÀ BONUS | GESTIONE TECNICA |
| --- | --- |
| LANCIO COLTELLI | PROJECTILE_PG con collisioni standard. |
| PET | Layer PET e collisioni definite nella sezione 22.2; nessun HP e nessuna collisione con i proiettili. |
| AURA TOSSICA | Effetto istantaneo tramite AREA_EFFECT_MOB o verifica logica; MURO/OSTACOLO gestiti tramite gli 8 SPICCHI. |
| RICOCHET | DANNO istantaneo logico sul MOB selezionato; nessun proiettile fisico aggiuntivo. |
| MINE | Oggetto attraversabile; rilevamento TRIGGER_MOB ed esplosione AREA_EFFECT_MOB. |
| SCUDO | Protezione logica associata al PG, senza collider o layer dedicato; mitigazione e consumo della HIT secondo la sezione 22.6. |
| FIRE BULLET | Stato logico temporaneo applicato agli ATTACCHI BASE; i proiettili fisici restano PROJECTILE_PG e BRUCIATURA è applicata logicamente alla HIT. Gli ATTACCHI BASE ad AREA/multipli conservano la propria classificazione e le regole della sezione 22.7. |
| TASER | AREA_EFFECT_MOB istantanea; DANNO e STUN sui MOB validi; 6 SPICCHI per MURO/OSTACOLO. |
| REPULSE | AREA_EFFECT_MOB istantanea; DANNO e RESPINTA; 8 SPICCHI. |
| SCIABOLATA | AREA_EFFECT_MOB o verifica HITSCAN a semicerchio; 4 SPICCHI e nessuna collisione fisica persistente. |

Nessun nuovo TAG o LAYER; valori, durate, frequenze e regole specifiche delle sezioni 22.1–22.10 restano invariati.

<a id="sezione-23"></a>

## 23. RATE DI APPARIZIONE — NUOVE ABILITÀ BONUS

| ABILITÀ BONUS | RATE |
| --- | --- |
| LANCIO COLTELLI | 16% |
| PET | 6% |
| AURA TOSSICA | 6% |
| RICOCHET | 6% |
| MINE | 16% |
| SCUDO | 16% |
| FIRE BULLET | 11% |
| TASER | 6% |
| REPULSE | 11% |
| SCIABOLATA | 6% |

<a id="sezione-24"></a>

## 24. UPGRADE ABILITÀ BONUS — REGOLE

- Gli UPGRADE percentuali vengono calcolati solo dove la STAT possiede un VALORE BASE numerico.
- Gli incrementi sono sempre calcolati sul VALORE BASE originale e non sul valore già potenziato.
- Gli UPGRADE, inclusi gli SPECIALI, sono acquisibili più volte nella stessa RUN; per ora nessun CAP massimo al numero di acquisizioni (sezione 20).
- DANNO: +10% del VALORE BASE.
- FIRE BULLET: UPGRADE DANNO ha effetto solo sul DANNO dell'effetto BRUCIATURA; il BONUS ATK di FIRE BULLET si somma all'ATK corrente del PG (sezione 22.7).
- RAGGIO: +10% del VALORE BASE.
- RANGE: +15% del VALORE BASE.
- ATK SPD: +10% del VALORE BASE.
- CD: −10% del VALORE BASE.

### 24.1 UPGRADE speciali

| ABILITÀ | UPGRADE SPECIALE | VALORE |
| --- | --- | --- |
| LANCIO COLTELLI | N° COLTELLI | +1 |
| PET | N° PET | +1 |
| RICOCHET | N° RIMBALZI | +1 |
| SCUDO | DEF DELLO SCUDO | +5 punti percentuali, fino al cap 90% |
| FIRE BULLET | DANNO BRUCIATURA | +50% del VALORE BASE |
| TASER | TEMPO BLOCK | +25% del VALORE BASE |
| REPULSE | RESPINTA | +20% del VALORE BASE |

<a id="sezione-25"></a>

## 25. RATE DEI SINGOLI UPGRADE

| ABILITÀ BONUS | UPGRADE / RATE |
| --- | --- |
| LANCIO COLTELLI | DANNO 30% · RANGE 40% · CD 20% · N° COLTELLI 10% |
| PET | DANNO 50% · ATK SPD 40% · N° PET 10% |
| AURA TOSSICA | DANNO 50% · RAGGIO 30% · CD 20% |
| RICOCHET | DANNO 80% · N° RIMBALZI 20% |
| MINE | DANNO 40% · RAGGIO 40% · CD 20% |
| SCUDO | DEF DELLO SCUDO 50% · CD 50% |
| FIRE BULLET | DANNO 40% · DANNO BRUCIATURA 40% · CD 20% |
| TASER | DANNO 50% · RAGGIO 30% · TEMPO BLOCK 5% · CD 15% |
| REPULSE | DANNO 50% · RAGGIO 30% · RESPINTA 5% · CD 15% |
| SCIABOLATA | DANNO 40% · RANGE 40% · CD 20% |

<a id="sezione-26"></a>

## 26. MEDI KIT

- MEDI KIT completamente attraversabili, senza collisione fisica con PG o MOB; rilevamento tramite collider IsTrigger sul layer TRIGGER_PG (sezione 10.9).

### GENERAZIONE NELL’AREA

- Durante il CARICAMENTO della NUOVA AREA viene sorteggiata la quantità: 1, 2 oppure 3 MEDI KIT, con probabilità esatta **1/3 ciascuno**.
- È sempre presente almeno 1 MEDI KIT; massimo 3 generati da questo sistema. Questa regola sostituisce il precedente 30% per punto di spawn.
- Ogni MEDI KIT viene generato in posizione CASUALE ad almeno 50 m dalla ZONA DI INIZIO. Ricalcolare ogni posizione finché è valida, come per CHEST.
- Il DROP di PG06 / ELEMOSINA resta una fonte aggiuntiva, con roll del 3% per KILL attribuita a PG06; non è incluso nel sorteggio iniziale 1–3.

### TRIGGER, CURA E PRIORITÀ

- Raccolta automatica entrando nel TRIGGER; non serve F, nessuna pausa. Forma e dimensioni del TRIGGER da definire in sviluppo/testing.
- I PG IA non cercano autonomamente MEDI KIT: possono usarli solo incontrandoli durante il normale movimento, con le stesse regole di validità del TRIGGER.
- I PG al 100% HP, in DOWN o in MORTE sono ignorati: non attivano né consumano il MEDI KIT.
- Senza un PG valido, il MEDI KIT resta disponibile per una raccolta successiva.
- Cura solo 1 PG valido nel TRIGGER, non tutta la PARTY. Con più PG validi contemporaneamente: HP% più bassa → HP effettivi più bassi → scelta CASUALE in perfetta parità.
- CURA BASE: 10% degli HP MASSIMI del destinatario, modificabile dagli UPGRADE PROF (+2 punti percentuali per acquisto: 10% → 12% → 14% ...).
- NO OVERHEAL: non superare il 100% HP; la cura eccedente è persa.
- Dopo la CURA immediata il MEDI KIT sparisce e non può essere consumato di nuovo.
- Le regole generali aggiornate di validità del TRIGGER sostituiscono la vecchia indicazione di consumo a HP pieni anche nel richiamo a ELEMOSINA; il suo DROP e la permanenza restano quelli specifici della sezione 12.

<a id="sezione-27"></a>

## 27. PROF — UPGRADE PERMANENTI

- Categorie definite: HP, ATK, DEF, MOVE SPD, ATK SPD, CD REDUCTION, CHEST RATE, G DROP, ITEM SLOT, MEDI KIT.
- Gli acquisti e gli UPGRADE permanenti sono personali per ogni PLAYER.
- HP, ATK, DEF, MOVE SPD, ATK SPD e CD REDUCTION sono legati al singolo PG.
- CHEST RATE, G DROP, ITEM SLOT e MEDI KIT sono legati al PLAYER.
- HP / ATK / MOVE SPD / ATK SPD / G DROP / MEDI KIT: nessun cap di acquisti.
- DEF: UPGRADE fino al cap DEF 90%. CD REDUCTION: UPGRADE fino al minimo 10. ITEM SLOT: massimo 3 UPGRADE.
- I BONUS PROF alla DEF si sommano alla DEF BASE del PG, modificando la base permanente con cui entra in RUN.
- I COSTI attuali restano invariati per ora e verranno ribilanciati in fase di TEST.
- HP / ATK / DEF / MOVE SPD / ATK SPD: ogni acquisto aggiunge 1% del VALORE BASE ORIGINALE.
- CD REDUCTION: ogni acquisto riduce la STAT CD REDUCTION dell'1% del suo VALORE BASE ORIGINALE. Con base 100 equivale a −1 punto per acquisto: 100 → 99 → 98. La riduzione è sempre calcolata sul VALORE BASE ORIGINALE, non sul valore già modificato, e non modifica il CD BASE in secondi delle ABILITÀ.
- Costi iniziali definiti: HP 50 G; ATK 50 G; DEF 50 G; MOVE SPD 50 G; ATK SPD 100 G; CD REDUCTION 100 G.
- Dopo ogni acquisto dello stesso UPGRADE, il costo aumenta del 10% con arrotondamento per difetto.
- CHEST RATE: costo iniziale 500 G; ogni acquisto aggiunge +1 punto percentuale alla probabilità di SPAWN di una seconda CHEST nell’AREA, fino al CAP 100%. La prima CHEST resta garantita al 100%. All’inizio di una RUN CO-OP si applica il BONUS CHEST RATE più alto tra i PLAYER presenti.
- G DROP: costo iniziale 500 G; ogni UPGRADE aumenta del 10% il VALORE BASE del G DROP e modifica esclusivamente il G ricevuto dal PLAYER che lo ha acquistato.
- ITEM SLOT: 3 UPGRADE massimi; costi 1000 G → 1100 G → 1210 G; SLOT da 1 a massimo 4.
- MEDI KIT: costo UPGRADE iniziale 250 G; ogni UPGRADE aggiunge +2 punti percentuali alla cura (10% → 12% → 14% ...); i costi successivi seguono +10% con arrotondamento per difetto.

<a id="sezione-28"></a>

## 28. DOWN / MORTE / RESURREZIONE

- A 0 HP un PG entra in stato DOWN.
- In DOWN il PG non può agire e i MOB non lo considerano un bersaglio. Mantiene il layer PG e tutte le normali collisioni fisiche; DOWN è uno stato logico senza TAG dedicato.
- Timer DOWN: 20 s. Alla scadenza, il PG entra in stato MORTE.
- In MORTE il PG mantiene il layer PG ma non ha alcuna collisione fisica. La SPRITE DI MORTE rimane a terra senza collisioni ed è attraversabile da PG, MOB, PET e proiettili senza interazioni. MORTE è uno stato logico, senza LAYER o TAG dedicato.
- Un altro PG sotto controllo diretto di un PLAYER può avviare la RIANIMAZIONE tenendo premuto F entro il TRIGGER_PG di raggio 2 m attorno al PG in DOWN, senza collisione fisica aggiuntiva. Il timer di RIANIMAZIONE avanza fino a 5 s; al raggiungimento dei 5 s il PG viene RESUSCITATO. Le RESURREZIONI eseguite dai PG sono consentite esclusivamente sotto controllo diretto di un PLAYER.
- Un PG controllato da PLAYER in DOWN può essere resuscitato solo da un altro PLAYER; un PG IA in DOWN può essere resuscitato solo da un PLAYER. Un PG IA non esegue autonomamente RESURREZIONI.
- Dopo la resurrezione, il PG IA rientra in formazione.
- Durante l’interazione di RIANIMAZIONE, il timer DOWN di 20 s è in PAUSA.
- Se l’interazione viene interrotta prima del completamento, il timer di RIANIMAZIONE regredisce verso 0 s e il timer DOWN riprende dal valore esatto in cui era stato congelato.
- Se la RIANIMAZIONE riprende prima che il relativo timer raggiunga 0 s, riparte dal valore residuo; durante l’interazione il timer DOWN torna in PAUSA.
- Nessun altro evento può interrompere la RIANIMAZIONE.
- Il PG resuscitato torna con il 50% degli HP MASSIMI correnti, inclusi eventuali BONUS temporanei.
- Dopo la resurrezione ottiene 2 s di invulnerabilità. La RESURREZIONE ripristina le normali collisioni del PG, che rimangono attive anche durante questi 2 s; RESURREZIONE e INVULNERABILITÀ sono gestite logicamente, senza nuovi LAYER o TAG.
- Quando non rimane nessun PG in stato ATTIVO, la PARTY è SCONFITTA e la RUN termina: una combinazione composta esclusivamente da PG in DOWN e/o MORTE determina la SCONFITTA.
- I PG in DOWN devono essere RESUSCITATI prima del passaggio AREA. I PG in MORTE vengono RESUSCITATI automaticamente nella nuova AREA al 50% degli HP MASSIMI correnti, senza cura aggiuntiva del 15% (sezione 6).

- DROP G/EXP: un PG in DOWN riceve EXP, un PG in MORTE non la riceve. Il PLAYER riceve G finché possiede almeno un PG non in MORTE (sezione 16).

### CAMBIO CONTROLLO IN DOWN

- SINGLE PLAYER — 1 PLAYER + 3 PG IA: quando il PG del PLAYER entra in DOWN, il PLAYER assume automaticamente il controllo del primo PG IA nella lista. Appena il proprio PG viene resuscitato, il PLAYER torna automaticamente a controllarlo.
- CO-OP — 2 PLAYER + 2 PG IA: ciascun PLAYER ha 1 PG IA assegnato. Quando il proprio PG entra in DOWN, il PLAYER assume automaticamente il controllo del proprio PG IA assegnato; quando il PG originale viene resuscitato, torna automaticamente a controllarlo.
- CO-OP — 3 PLAYER + 1 PG IA: solo l’HOST ha un PG IA assegnato. Quando il PG dell’HOST entra in DOWN, l’HOST assume automaticamente il controllo del PG IA e torna automaticamente al proprio PG quando viene resuscitato. Gli altri 2 PLAYER non hanno PG IA assegnati e non applicano questa regola.
- CO-OP — 4 PLAYER: non esistono PG IA; il cambio controllo non è applicabile.

### STATO SPETTATORE

- Se un PLAYER non ha più PG a disposizione ma la RUN è ancora ATTIVA, entra automaticamente in stato SPETTATORE.
- La visuale è bloccata su un altro PG disponibile, inizialmente scelto casualmente.
- LMB passa al PG precedente; RMB passa al PG successivo.
- Il ciclo è circolare e segue l’ORDINE PLAYER, saltando i PLAYER senza PG disponibile: dopo l’ultimo si torna al primo e viceversa.
- L’HOST è sempre PLAYER 1; gli altri PLAYER sono numerati secondo l’ordine di ingresso in partita.
- Lo SPETTATORE controlla esclusivamente la visuale e non assume il controllo del PG osservato.
- Quando il proprio PG viene RESUSCITATO al CAMBIO AREA, il PLAYER esce automaticamente da SPETTATORE, la visuale torna al proprio PG e il PLAYER ne riprende il controllo diretto. Si applicano le regole esistenti di resurrezione al CAMBIO AREA: 50% degli HP MASSIMI correnti, senza cura aggiuntiva del 15%.

<a id="sezione-29"></a>

## 29. VITTORIA / SCONFITTA / FINE RUN

### 29.1 Vittoria e ritorno volontario

- Dopo la vittoria contro il BOSS della CITTÀ, al completamento dell’AREA, la decisione tra PROSEGUIRE e TORNARE ALL’HUB spetta all’HOST.
- Solo nella schermata dell’HOST compare il tasto TORNA ALL’HUB.
- Se PROSEGUE: EXP, LVL, BONUS temporanei, ITEMS e G restano invariati e la RUN continua.
- Se TORNA VOLONTARIAMENTE ALL'HUB: EXP, LVL e BONUS temporanei vengono persi; G e ITEMS inutilizzati vengono mantenuti.

### 29.2 Sconfitta

- La SCONFITTA avviene quando non rimane nessun PG in stato ATTIVO, quindi quando tutti i PG sono in DOWN e/o MORTE.
- La RUN termina e si torna all'HUB.
- EXP, LVL della RUN, BONUS temporanei e ITEMS inutilizzati vengono persi.
- In caso di sconfitta si mantiene il 50% del G ottenuto durante quella RUN, arrotondato per difetto; la parte restante viene persa.
- Esempio: 101 G ottenuti nella RUN → 50 G mantenuti e 51 G persi.

### 29.3 Abbandono / disconnessione durante la RUN

- Se un PLAYER abbandona o si disconnette durante la RUN, perde i progressi di quella RUN.
- Il G guadagnato durante la RUN viene trattato secondo le regole della SCONFITTA: il PLAYER mantiene il 50% del G ottenuto durante quella RUN, arrotondato per difetto, e perde la parte restante.
- Il PG controllato direttamente dal PLAYER uscente diventa automaticamente PG IA.
- Questo PG e gli eventuali PG IA già di sua responsabilità vengono riassegnati agli altri PLAYER secondo le regole di ASSEGNAZIONE IA già stabilite nella sezione 8.5, in base al nuovo numero di PLAYER presenti.
- La RUN può continuare per gli altri PLAYER secondo le condizioni di SCONFITTA già definite.

<a id="sezione-30"></a>

## 30. STATO DI SVILUPPO — PUNTI RIMANENTI

**DA DEFINIRE / DA SVILUPPARE:** tutti i punti seguenti restano aperti secondo la fonte.

- Definire BOSS di ogni CITTÀ, relative STATS, fasi e meccaniche.
- Definire MINI BOSS, EVENTI SPECIALI e livelli/quest bonus. Per MINI BOSS e BOSS sono già definiti l’esclusione dal totale/massimo dei MOB dell’AREA e dal limite massimo dei MOB contemporaneamente presenti, e il punto di SPAWN specifico stabilito in fase di LEVEL DESIGN; tutto il resto rimane DA DEFINIRE.
- Definire nomi propri, mappe, layout e identità visiva dettagliata delle CITTÀ e delle AREE.
- Definire eventuali ulteriori MOB/varianti oltre ZOMB01–ZOMB05; la progettazione dei MOB è per ora conclusa con ZOMB05.
- Definire il comportamento generale AI dei PG, da affrontare successivamente; equipaggiamento, progressione, DOWN e cambio controllo dei PG IA sono definiti nelle sezioni 8 e 28. Definire le specifiche dei futuri MOB. AGGRO, MOVIMENTO/COLLISIONI, HIT/DANNO/STUN, MORTE/DROP e le schede ZOMB01–ZOMB05 sono consolidati nella sezione 13.
- SISTEMA DI SPAWN e distribuzione ZOMB01–ZOMB05 consolidati nelle sezioni 14–15. BALISTICA dei PG consolidata nella sezione 10.1; PROJECTILE SPD ZOMB04 = 8 m/s è consolidata nella sezione 13.8. LANCIO COLTELLI: PROIETTILI FISICI con PROJECTILE SPD 20 m/s, secondo la sezione 22.1. Resta da esplicitare l’applicazione dello standard PROJECTILE SPD ai proiettili degli altri MOB e alle ABILITÀ non precisate.
- Definire UI/HUD: HP, abilità, PASSIVA, SLOT BONUS, SLOT ITEM, EXP/LVL, G, indicatori DOWN e schermate di scelta.
- PREPARAZIONE RUN e logica BANNER LEVEL UP consolidate nelle sezioni 8 e 20; realizzare la grafica definitiva e completare le schermate BONUS dove non descritte.
- Ribilanciare i COSTI degli UPGRADE permanenti del PROF in fase di TEST; per ora restano quelli della sezione 27. Restano aperti solo gli aspetti PROF indicati nella sezione 32.
- Bilanciamento complessivo di EXP/LVL, G, MOB per AREA, danni, cure, cooldown e probabilità.
- Direzione Pixel Art, animazioni, VFX, SFX e musica.
- Prototipo Unity del movimento, mira, attacco, abilità, spawn, BONUS, HUB e loop RUN.
- Architettura tecnica multiplayer CO-OP fino a 4 giocatori, sincronizzazione e gestione host/client.
- Testing, performance, salvataggio/progressione permanente e build finale.

<a id="sezione-31"></a>

## 31. REGOLE CONSOLIDATE DA NON DIMENTICARE

- LAYER e coppie confermate: sezione 10.9. PG comprende personaggi controllati da PLAYER o IA; PLAYER è distinto dal personaggio fisico. Nessun TAG tecnico dedicato per ora: identificazione tramite LAYER + componenti/script + stati logici; TAG futuri solo in caso di necessità concreta durante lo sviluppo.
- Classificazione tecnica confermata senza nuovi LAYER: ABILITÀ/PASSIVE PG (sezione 12), ABILITÀ BONUS (22.11), ITEMS (9.6), attacchi/effetti MOB (13.10). Stati e modificatori non cambiano layer/collisioni, salvo le variazioni esplicite, in particolare MORTE PG.
- PG in DOWN mantiene layer PG e collisioni normali; PG in MORTE e relativa SPRITE DI MORTE a terra non hanno collisioni. RESURREZIONE ripristina le collisioni normali, mantenute durante INVULNERABILITÀ.
- ZOMB05 in PRE-ESPLOSIONE mantiene layer MOB e tutte le collisioni normali, inclusa MOB ↔ MOB SÌ; eliminata l’eccezione di attraversamento/sovrapposizione.
- NPC HUB: collisione fisica OSTACOLO + interazione con F tramite TRIGGER_PG. EXIT HUB: TRIGGER_PG + F, senza blocco fisico.
- PG ↔ PG/MOB/MURO/OSTACOLO/PROJECTILE_MOB SÌ; PG ↔ PROJECTILE_PG NO. MOB ↔ MOB SÌ con collider ridotto come nel TEST in engine; MOB ↔ PROJECTILE_MOB NO.
- PROJECTILE_PG e PROJECTILE_MOB collidono con MURO/OSTACOLO; PROJECTILE_PG ↔ PROJECTILE_PG NO, PROJECTILE_PG ↔ PROJECTILE_MOB NO e PROJECTILE_MOB ↔ PROJECTILE_MOB NO. Restano valide le eccezioni specifiche già definite.
- TRIGGER_PG rileva PG; TRIGGER_MOB rileva MOB; combinazioni incrociate NO, sempre senza blocco fisico. AREA_EFFECT_PG/AREA_EFFECT_MOB separati per destinatario; verifiche di SPICCHI/SEZIONI contro MURO/OSTACOLO nella logica dell’AREA.
- CHEST e MEDI KIT completamente attraversabili, tramite TRIGGER_PG; MINE senza collisione fisica con PG/MOB, tramite TRIGGER_MOB. BARRIERA PG01 eredita le collisioni di OSTACOLO.
- PET ↔ PG NO; PET ↔ MOB/PET/MURO/OSTACOLO SÌ; PET ↔ PROJECTILE_PG/PROJECTILE_MOB NO. Nessuna meccanica HP/DANNO per i PET: i proiettili li attraversano senza HIT, DANNO, distruzione o deviazione e i PET non fungono da scudi.

- Nome ufficiale progetto: ROG ZOMBIE.
- CO-OP fino a 4 giocatori.
- PARTY sempre composta da 4 PG.
- Tasto INTERAZIONE: F.
- Tasto ABILITÀ del PG controllato direttamente: Q. ABILITÀ PG IA: SPACE BAR + numero 1–3 secondo l’ordine dei PG IA assegnati, attivata dal PLAYER responsabile; PASSIVA identica e automatica.
- Ogni PG: 2 ABILITÀ ma 1 sola selezionata per RUN; 2 PASSIVE ma 1 sola selezionata per RUN.
- CD REDUCTION: STAT modificatore con VALORE BASE neutro 100 per tutti i PG01–PG08; un valore più basso riduce il cooldown.
- I CD BASE in secondi sono definiti esclusivamente nelle singole ABILITÀ. Formula: CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100). Esempio: CD BASE 10 s e CD REDUCTION 90 → CD FINALE 9 s.
- BONUS CHEST e BONUS STATS DI FINE AREA: stessi RATE (HP 25%, ATK 8%, DEF 22%, MOVE SPD 22%, ATK SPD 8%, CD REDUCTION 15%) e +5% sul VALORE ATTUALE della STAT al momento dell’acquisizione, comprensivo degli UPGRADE permanenti del PROF e di tutti i precedenti BONUS STATS acquisiti durante la RUN, sia da CHEST sia da FINE AREA.
- BONUS CHEST e BONUS STATS DI FINE AREA CD REDUCTION: riducono la STAT del 5% del VALORE ATTUALE, con arrotondamento matematico del risultato all’intero più vicino e minimo 10; per esempio 100 → 95 → 90. Non riducono direttamente i secondi del CD BASE.
- Ogni UPGRADE PROF CD REDUCTION riduce la STAT dell'1% del VALORE BASE ORIGINALE; con base 100 equivale a −1 punto per acquisto.
- CD REDUCTION minimo 10; CD FINALE arrotondato al decimo con arrotondamento matematico.
- STAT CD REDUCTION arrotondata matematicamente all’intero più vicino: frazione <0,5 per difetto; ≥0,5 per eccesso.
- DEF interna 100 = 0%; ogni ±1 punto interno = ±1 punto percentuale. BONUS PROF aggiunti alla DEF BASE; BONUS CHEST e BONUS STATS DI FINE AREA calcolati sul VALORE ATTUALE della STAT al momento dell’acquisizione, comprensivo degli UPGRADE permanenti del PROF e di tutti i precedenti BONUS STATS acquisiti durante la RUN, sia da CHEST sia da FINE AREA; PASSIVE sulle STATS CORRENTI.
- Avvio CD PG01–PG08 completamente definito nelle schede della sezione 12: all’attivazione, salvo FUOCO RAPIDO al termine dei suoi 4 s e COLPI RESPINGENTI al termine dei suoi 4 s, COLPO GROSSO al consumo del 4° ATTACCO e FUOCO CURATIVO al consumo del 6° ATTACCO. Restano valide le regole di INIZIO RUN e CAMBIO AREA.
- DEF massima 90%, inclusa la DEF propria dello SCUDO; SCUDO mitiga prima del PLAYER, poi il residuo è mitigato dalla DEF del PLAYER. DANNO FINALE arrotondato all’intero più vicino.
- EFFETTI PERSISTENTI: di default non si cumulano e rinnovano la DURATA; prevalgono le regole specifiche (sezione 10.6).
- REGOLA GENERALE DANNI DA STATO — BRUCIATURA e VELENO: primo tick 1 s dopo applicazione/rinnovo, poi ogni 1 s; DANNO per tick = DANNO base dello STATO × numero ISTANZE; massimo 5 ISTANZE dello stesso STATO sullo stesso bersaglio. Ogni applicazione rinnova la DURATA completa, anche al CAP; oltre 5 ISTANZE il DANNO non aumenta. Le ISTANZE non hanno DURATE indipendenti (sezione 10.8).
- Al CAMBIO AREA un’ABILITÀ già IN CD ma non più ATTIVA mantiene esattamente il CD residuo e continua il conteggio nella nuova AREA. Restano invariate le regole per DISPONIBILE, ATTIVA e le eccezioni già definite (sezione 10.4).
- Prima CHEST garantita al 100%; PROF CHEST RATE aggiunge +1 punto percentuale per acquisto alla probabilità della seconda CHEST, con CAP 100%. CHEST RATE è legato al PLAYER; all’inizio di una RUN CO-OP si applica il BONUS più alto tra i PLAYER presenti. Posizioni raggiungibili NAVMESH, almeno 80 m dalla ZONA DI INIZIO e 10 m da MEDI KIT e altre CHEST.
- Ogni PG: 3 SLOT BONUS.
- ABILITÀ BONUS: definizioni confermate 1.1–10.6 nelle sezioni 22.1–22.10. LANCIO COLTELLI: suddivisione 45°; MINE: TRIGGER circolare raggio 1 m; RICOCHET: probabilità di attivazione 40% per ogni HIT valida.
- SCUDO: DOWN e MORTE lo disattivano e fanno ripartire il CD; riattivazione solo con PG ATTIVO. Il CAMBIO AREA non lo disattiva.
- FIRE BULLET: ogni nuova HIT applica un’ISTANZA di BRUCIATURA secondo la REGOLA GENERALE DANNI DA STATO, aumentando il DANNO fino al CAP di 5 ISTANZE e rinnovando la DURATA completa anche al CAP; UPGRADE DANNO agisce solo sulla BRUCIATURA, BONUS ATK sommato all'ATK corrente del PG. La BRUCIATURA si applica a tutti i MOB colpiti dall’ATTACCO BASE, inclusi ATTACCHI BASE ad AREA/multipli.
- SCIABOLATA: AREA a SEMICERCHIO divisa in 4 SPICCHI, secondo le regole generali delle AREE DI EFFETTO con MURI/OSTACOLI; SPICCHIO anche parzialmente intercettato eliminato integralmente, senza HIT aggiuntive.
- PASSIVA 1 PG02: con HP <30%, ogni KILL cura 1% degli HP MASSIMI correnti di PG02; nessun effetto a HP ≥30%.
- EXP/LVL: +15% rispetto al precedente, arrotondato alla decina più vicina con criterio matematico (quoziente EXP/10 all’intero più vicino: frazione <0,5 per difetto; ≥0,5 per eccesso, poi ×10). Valori della tabella LVL 1 → 11 invariati; nessun CAP di LVL.
- MINI BOSS e BOSS esclusi dal totale/massimo MOB dell’AREA e dal limite massimo dei MOB contemporaneamente presenti; punto di SPAWN specifico stabilito in fase di LEVEL DESIGN.
- SLOT ITEM dei PG controllati direttamente: 1 iniziale, 3 UPGRADE massimi, 4 SLOT massimi. I PG IA non hanno ITEMS né SLOT ITEM utilizzabili.
- ITEMS definitivamente CONFERMATI: tasti 1 / 2 / 3 / 4 per i rispettivi SLOT; pressione mantenuta per anteprima AREA e mira/posizionamento, rilascio per utilizzo/lancio. Consumo di 1 ITEM per uso; SLOT vuoto all’esaurimento, conservando l’eccezione SCORTA ESPLOSIVA.
- Centro ITEMS sul CURSORE; nessun RANGE massimo salvo TRAPPOLA, limitata a 7 m lungo PG→CURSORE. TRAPPOLA orientata lungo PG→CURSORE con lato da 4 m come FRONTALE.
- ITEMS senza FRIENDLY FIRE: MOLOTOV, GRANATA e TRAPPOLA colpiscono solo i MOB; SMOKE rende INVISIBILI tutti i PG nell’AREA secondo le regole già definite; POZIONE CURATIVA cura i PG e ignora i MOB.
- AREE ITEMS: GRANATA raggio 2,5 m / 4 × 90°; MOLOTOV raggio 5 m / 8 × 45°; TRAPPOLA 1 × 4 m / 4 SEZIONI. SPICCHIO/SEZIONE anche parzialmente intercettato da MURI/OSTACOLI eliminato integralmente, senza HIT aggiuntive. SMOKE e POZIONE CURATIVA ignorano MURI/OSTACOLI. Nessun comportamento aggiuntivo di lancio/posizionamento.
- Promemoria MERCHANT: alcuni ITEMS devono essere sbloccati tramite QUEST; quali ITEMS e quali QUEST restano DA DEFINIRE.
- Ingresso nuova AREA: PG VIVI +15% HP MASSIMI; PG in MORTE resuscitati al 50% degli HP MASSIMI correnti, senza +15%; DOWN da resuscitare prima del passaggio.
- DOWN: a 0 HP; timer 20 s, poi MORTE. SCONFITTA quando non rimane nessun PG ATTIVO, quindi solo DOWN e/o MORTE.
- RIANIMAZIONE: solo tramite PG sotto controllo diretto di un PLAYER, con F entro TRIGGER_PG di raggio 2 m; timer 5 s; ritorno al 50% degli HP MASSIMI correnti e 2 s di invulnerabilità. Durante l’interazione il timer DOWN è in PAUSA; all’interruzione riprende dal valore congelato e il timer di RIANIMAZIONE regredisce verso 0. Riprendendo prima dello 0 si conserva il valore residuo. Nessun altro evento può interrompere la RIANIMAZIONE.
- SPETTATORE automatico se il PLAYER non ha più PG a disposizione e la RUN è ancora ATTIVA: visuale bloccata su un altro PG disponibile scelto inizialmente a caso; LMB precedente, RMB successivo, ciclo circolare in ORDINE PLAYER saltando chi non ha PG disponibile. HOST = PLAYER 1; altri numerati per ordine di ingresso. Controllo della sola visuale. Alla resurrezione del proprio PG al CAMBIO AREA, uscita automatica da SPETTATORE, visuale sul proprio PG e ripresa del controllo diretto.
- Sconfitta PARTY: si mantiene il 50% del G ottenuto nella RUN arrotondato per difetto; si perde la parte restante.
- USCITA / PASSAGGIO AREA: TRIGGER CIRCOLARE con RAGGIO 4 m; tutti i PG VIVI devono trovarsi contemporaneamente al suo interno (sezione 6).
- Al completamento dell’AREA dopo il BOSS, la decisione tra PROSEGUIRE e TORNARE ALL’HUB spetta all’HOST. Solo nella schermata dell’HOST compare il tasto TORNA ALL’HUB.
- ABBANDONO / DISCONNESSIONE durante la RUN: il PLAYER uscente perde i progressi della RUN; il G guadagnato segue le regole della SCONFITTA. Il PG controllato diventa PG IA e, insieme agli eventuali PG IA già di sua responsabilità, viene riassegnato agli altri PLAYER secondo le regole di ASSEGNAZIONE IA già stabilite (sezioni 8.5 e 29.3).

- RICALCOLO AGGRO: 0,5 s scaglionato; immediato se il BERSAGLIO entra in DOWN.
- HIT/DANNO non altera il comportamento del MOB di per sé; STUN interrompe ogni AZIONE e richiede una ripartenza da capo.
- MORTE MOB: DROP G/EXP automatico immediato; SPRITE DI MORTE per 3 s senza COLLISIONI con PG/MOB.
- EXP integrale a ogni PG non in MORTE, inclusi DOWN; G integrali nel saldo personale di ogni PLAYER con almeno un PG non in MORTE. Nessuna suddivisione. Acquisti e UPGRADE personali; G DROP modifica solo il G ricevuto dal PLAYER che lo ha acquistato.
- PROF: HP, ATK, DEF, MOVE SPD, ATK SPD e CD REDUCTION legati al singolo PG; CHEST RATE, G DROP, ITEM SLOT e MEDI KIT legati al PLAYER.
- AREE CIRCOLARI: PG04 e LUCKY SHOT 4 × 90°; ZOMB03 e ESPLOSIONE ZOMB05 8 × 45°; SPICCHIO intercettato eliminato integralmente, senza HIT separate.
- ZOMB03: punto d’impatto fissato sulla posizione del BERSAGLIO a T = 0; AREA generata a T = 0,5 s; nuovo ATTACCO disponibile a T = 2 s.

- ZOMB04: ATTACCO da fermo con PROIETTILE BALISTICO; posizione iniziale 15 m; arretramento sotto 9 m; ripresa dell’inseguimento oltre 30 m; PROJECTILE SPD = 8 m/s.
- ZOMB05: 0 HP → PRE-ESPLOSIONE 2 s → ESPLOSIONE → MORTE → DROP e trigger SPAWN; 50 DANNO ai PG ATTIVI / 25 ai MOB prima della DEF; RAGGIO 4 m.
- SPAWN: OFF-SCREEN da tutti i PG ATTIVI, NAVMESH valida e percorso verso almeno un PG ATTIVO; ZOMB05 escluso dal FIRST SPAWN con quota redistribuita 50/50 a ZOMB01/ZOMB02.

<a id="sezione-32"></a>

## 32. Chiarimenti necessari — DA DEFINIRE

Le voci seguenti sono note editoriali di verifica. Evidenziano ciò che la fonte non determina in modo sufficiente per l’implementazione; non modificano le specifiche delle sezioni precedenti.

| Ambito | Dati conservati | DA DEFINIRE |
| --- | --- | --- |
| Lore e vittoria finale | Esperimento scientifico; scienziato come possibile BOSS finale, non definitivo. | Identità e ruolo definitivo dello scienziato, cura, conclusione narrativa e comportamento dopo il BOSS della CITTÀ 5. |
| Città e contenuti opzionali | 5 CITTÀ, 23 AREE; percorso principale lineare; possibili quest sotterranee. | Nomi, mappe, accesso e rientro dalle quest bonus, loro rapporto con il conteggio delle AREE e l’aumento dell’EXP DROP. |
| PARTY e sblocchi | 4 PG, roster di 8; PG selezionati BLOCKED; non sbloccati in silhouette. PG01–PG04 disponibili fin dall’inizio; PG05–PG08 inizialmente BLOCCATI, tutti immediatamente acquistabili dal RECLUTATORE dopo CITTÀ 1 a 5000 G ciascuno, senza ordine obbligatorio e con sblocco permanente. | Il comportamento generale IA resta DA DEFINIRE e sarà affrontato successivamente. Equipaggiamento, progressione, responsabilità delle scelte e distribuzione dei PG IA sono definiti nella sezione 8; DOWN, RESURREZIONE e CAMBIO CONTROLLO nella sezione 28. |
| HUB e NPC | MERCHANT, PROF, EXIT; RECLUTATORE sbloccato al completamento di CITTÀ 1, inserito permanentemente nell’HUB e necessario per sbloccare nuovi PG (sezione 7); possibili NPC liberati nelle quest. | Identità, dialoghi, condizioni di sblocco e servizi degli altri NPC. |
| Attacchi base e ITEMS | Armi, ATK, RANGE, ATTACCHI ad AREA e PROIETTILI definiti nelle sezioni 10–12. I PROIETTILI FISICI dei PG attraversano gli alleati senza effetti (sezione 10.1). Gli ATTACCHI BASE PG01–PG08, il raggio delle esplosioni PG04 e le eccezioni sono descritti nelle sezioni 10–12. ITEMS definitivamente CONFERMATI: comandi, consumo, mira, RANGE, bersagli, assenza di FRIENDLY FIRE e interazione con MURI/OSTACOLI nelle sezioni 9 e 10.3. Alcuni ITEMS del MERCHANT richiedono sblocco tramite QUEST. | Quali ITEMS acquistabili presso il MERCHANT richiedono sblocco e quali QUEST li sbloccano. Le regole di funzionamento ITEMS non sono più DA DEFINIRE. |
| MOB, BOSS e spawn | Totali per AREA, distribuzione ZOMB01–ZOMB05, FIRST SPAWN 30%, arrotondamenti a discapito di ZOMB01, quantità residue, probabilità e OFF-SCREEN globale consolidati nelle sezioni 14–15. MINI BOSS e BOSS esclusi dal totale/massimo MOB dell’AREA e dal limite massimo dei MOB contemporaneamente presenti; punto di SPAWN specifico stabilito in fase di LEVEL DESIGN. | Tutto il resto del funzionamento di MINI BOSS e BOSS, comprese STATS, fasi e meccaniche. |
| PROF | Incrementi e prezzi nella sezione 27; nessun cap per HP, ATK, MOVE SPD, ATK SPD, G DROP, MEDI KIT; DEF massimo 90%, CD REDUCTION minimo 10; ITEM SLOT massimo 3 UPGRADE / 4 SLOT. HP, ATK, DEF, MOVE SPD, ATK SPD e CD REDUCTION legati al singolo PG; CHEST RATE, G DROP, ITEM SLOT e MEDI KIT legati al PLAYER. CHEST RATE +1 punto percentuale per acquisto, CAP 100%; in CO-OP, all’inizio RUN si applica il BONUS più alto tra i PLAYER presenti. | Costi da ribilanciare in TEST, invariati per ora. |
| DOWN e morte | A 0 HP: DOWN con timer di 20 s, poi MORTE. SCONFITTA con nessun PG ATTIVO. RIANIMAZIONE con F entro TRIGGER_PG di raggio 2 m, timer 5 s, ritorno al 50% degli HP MASSIMI correnti e 2 s di invulnerabilità; solo PG sotto controllo diretto di un PLAYER. Timer DOWN in pausa durante l’interazione; all’interruzione riprende dal valore congelato e il timer di RIANIMAZIONE regredisce verso 0, riprendendo dal residuo se riavviato prima dello 0. Nessun altro evento interrompe la RIANIMAZIONE. Rientro in formazione dei PG IA, CAMBIO CONTROLLO e SPETTATORE definiti nella sezione 28. | Velocità del regresso del timer di RIANIMAZIONE verso 0 s. |
| Passaggio AREA | TRIGGER USCITA CIRCOLARE con RAGGIO 4 m; tutti i PG VIVI devono trovarsi contemporaneamente al suo interno. Resurrezione automatica dei PG in MORTE al 50%, senza +15%; cura di ingresso del 15% ai VIVI; DOWN da resuscitare prima (sezione 6). | Nessun punto residuo relativo alla forma e alle dimensioni del TRIGGER USCITA. |
| Fine RUN | Dopo il BOSS decide l’HOST tra PROSEGUIRE e TORNARE ALL’HUB; solo nella sua schermata compare il tasto TORNA ALL’HUB. Ritorno volontario e sconfitta hanno effetti distinti. In caso di abbandono/disconnessione il PLAYER perde i progressi della RUN e mantiene il 50% del solo G guadagnato nella RUN, arrotondato per difetto, secondo la SCONFITTA. Il PG controllato diventa PG IA; questo PG e gli eventuali PG IA già di sua responsabilità vengono riassegnati agli altri PLAYER secondo le regole stabilite (sezioni 8.5 e 29). | Gestione tecnica dell’HOST uscente, già segnalata nelle note di consolidamento; nessuna modalità tecnica viene introdotta. |
| DANNI PERIODICI | FILO SPINATO: per ciascun MOB il conteggio parte al contatto con una SEZIONE/AREA valida; infligge 10 DANNO ogni secondo durante cui il MOB rimane nell’AREA e termina all’uscita. Per MOB già nella fascia valida all’attivazione, il conteggio parte in quel momento. MOLOTOV, TRAPPOLA e POZIONE CURATIVA: sezione 9.3. BRUCIATURA e VELENO: REGOLA GENERALE DANNI DA STATO (sezione 10.8), primo tick 1 s dopo applicazione/rinnovo e poi ogni 1 s; DANNO per tick = DANNO base dello STATO × numero ISTANZE, massimo 5 dello stesso STATO sullo stesso bersaglio; ogni applicazione rinnova la DURATA completa anche al CAP, senza DURATE indipendenti e senza ulteriore aumento del DANNO oltre il CAP. | Restano DA DEFINIRE soltanto le regole dei DANNI PERIODICI non esplicitate per eventuali altri effetti; la definizione di FILO SPINATO non viene estesa ad altre fonti. |
| CD al CAMBIO AREA | ABILITÀ DISPONIBILE resta disponibile; ABILITÀ ATTIVA termina e il suo CD riparte. ABILITÀ già IN CD ma non più ATTIVA mantiene esattamente il CD residuo e continua il conteggio nella nuova AREA. Eccezione — SCUDO: il CAMBIO AREA non lo disattiva (sezione 22.6). Per COLPI RESPINGENTI, se il CAMBIO AREA avviene durante i 4 s, l’effetto termina immediatamente e da quel momento riparte l’intero CD. | Nessun punto residuo relativo al CD già in corso al CAMBIO AREA. |
| LAYER e matrice delle collisioni | Coppie confermate nella sezione 10.9, inclusa PROJECTILE_MOB ↔ PROJECTILE_MOB NO; MOB ↔ MOB SÌ anche per ZOMB05 in PRE-ESPLOSIONE, senza la precedente eccezione. Classificazione tecnica di ABILITÀ/PASSIVE PG, ABILITÀ BONUS, ITEMS e attacchi/effetti MOB confermata. RIANIMAZIONE usa TRIGGER_PG; NPC HUB usano OSTACOLO + TRIGGER_PG. Nessun TAG tecnico dedicato per ora; stati logici secondo la sezione 10.9. | Dimensioni numeriche del collider ridotto dei MOB. |
| Dettagli tecnici delle AREE | Numero/ampiezza SPICCHI, eliminazione completa e assenza di HIT separate sono confermati. AREA_EFFECT_PG e AREA_EFFECT_MOB separati; MURO/OSTACOLO fuori dalla Layer Collision Matrix delle AREA_EFFECT, con verifica geometrica affidata alla logica dell’ABILITÀ/AREA. | Orientamento iniziale degli SPICCHI e modalità tecnica di implementazione delle verifiche geometriche non sono specificati. |

I punti di bilanciamento, produzione artistica, prototipazione, multiplayer, salvataggio, testing e build restano quelli elencati nella sezione 30. Nessun valore aggiuntivo è stabilito da queste note.

### NOTE DI CONSOLIDAMENTO — 10/09/2026

- Integrate anche le decisioni successive alla base su STATS MOB, C5, transizione AREA e LEVEL UP, oltre a CHEST, MEDI KIT, PREPARAZIONE RUN e BALISTICA.
- Il nuovo HITSCAN di PG04 sostituisce il precedente volo parabolico dell’ATTACCO BASE; restano le regole specifiche di destinazione e AREA già definite. Non sono introdotti nuovi criteri di intercettazione del raggio.
- Restano non specificati: gestione tecnica dello SPAWN del proiettile dentro un ostacolo o con CURSORE coincidente con la bocca dell’arma; dettagli di rilascio drag & drop fuori dagli SLOT e trasferimenti fra PG; gestione dell’HOST uscente. Non vengono assegnati comportamenti impliciti.
- La conferma finale di validità MEDI KIT aggiorna il vecchio consumo a HP pieni nel richiamo a ELEMOSINA. Non modifica roll del DROP, durata nell’AREA o esclusione da VITAMINA C.

### REVISIONE — 12/09/2026 — 00:09 — PG06 / ATTRIBUZIONE KILL

- Aggiornati i bersagli validi di CURA AD AREA e FUOCO CURATIVO, la priorità di FUOCO CURATIVO e il trattamento dei PG al 100% HP. Introdotta la regola generale di ATTRIBUZIONE KILL nella sezione 10.7 e applicata a ELEMOSINA; rimosso il relativo criterio dai punti DA DEFINIRE della sezione 32. Tutte le altre specifiche e gli altri punti non risolti restano invariati.

### REVISIONE — 12/09/2026 — 00:30 — PG07 / BOW

- MULTI SHOT: PROJECTILE SPD 20 m/s; CD BASE 10 s avviato immediatamente all’attivazione; ogni PROIETTILE che colpisce genera una HIT separata con DEF applicata separatamente, anche sullo stesso MOB. Sostituita la precedente somma dei DANNI prima della DEF.
- PIOGGIA DI FRECCE: caduta solo grafica, senza velocità fisica di gameplay né altezza fisica di SPAWN; FRECCIA generata graficamente sopra il PUNTO D’IMPATTO e HIT esattamente al momento programmato. Conservate le altre regole, incluse 20 FRECCE da T = 0 a T = 3 s.
- LUCKY SHOT: DANNO risultante arrotondato all’intero più vicino con arrotondamento matematico (<0,5 per difetto; ≥0,5 per eccesso).
- CONCENTRAZIONE: primo MOB al 100% del DANNO effettivo corrente dell’ATTACCO BASE; secondo e terzo al 50%, calcolato prima della DEF; stesso arrotondamento matematico e DEF separata per ogni MOB. Propaga l’INTERA HIT e tutti gli effetti aggiuntivi anche al secondo e terzo MOB: solo il DANNO è ridotto al 50%.

### REVISIONE — 12/09/2026 — 00:41 — PG08 / HEAVY MACHINE GUN

- FILO SPINATO: conteggio individuale dal contatto con una SEZIONE/AREA valida, 10 DANNO ogni secondo di permanenza e termine del DANNO PERIODICO all’uscita; SLOW 25% non cumulabile tra più applicazioni.
- COLPI RESPINGENTI: al CAMBIO AREA durante i 3 s, termine immediato dell’effetto e riavvio dell’intero CD da quel momento.
- TENACIA: solo le HIT effettive dell’ATTACCO BASE di PG08 incrementano il BONUS di +0,2% DEF, con CAP invariato +30% / 150 HIT; altre fonti escluse. QUALSIASI HIT ricevuta azzera il BONUS, indipendentemente dal DANNO/HP persi.
- RAGE: +30% ATK e −20% MOVE SPD calcolati sui valori CORRENTI al momento dell’applicazione, inclusi i BONUS guadagnati in partita. STUN interrompe RAGE e azzera il conteggio dei 5 s; altri stati futuri da definire quando aggiunti.
- Aggiornati i richiami pertinenti nella sezione 32, limitando la definizione dei DANNI PERIODICI a FILO SPINATO. Tutte le altre specifiche e gli altri punti DA DEFINIRE restano invariati.

### REVISIONE — 12/09/2026 — 00:47 — ZOMB04

- Confermata PROJECTILE SPD ZOMB04 = 8 m/s. Aggiornate la scheda ZOMB04 (sezione 13.8), la sezione 30 e la sezione 31; il valore non viene esteso ad altri MOB o ABILITÀ.

### REVISIONE — 12/09/2026 — 00:47 — SBLOCCO PG / RECLUTATORE

- PG01, PG02, PG03 e PG04 disponibili fin dall’inizio; PG05, PG06, PG07 e PG08 inizialmente BLOCCATI.
- Al completamento di CITTÀ 1 viene sbloccato il RECLUTATORE, NPC necessario per sbloccare nuovi PG, inserito permanentemente nell’HUB.
- Da quel momento PG05–PG08 sono tutti immediatamente acquistabili a 5000 G ciascuno, senza ordine obbligatorio. Ogni PG acquistato rimane permanentemente sbloccato.
- Integrate le regole nelle sezioni 7 e 8 e aggiornati i richiami nella sezione 32, rimuovendo soltanto i punti ora risolti. I dettagli di equipaggiamento/progressione dei PG IA ancora non definiti e tutti gli altri punti aperti restano invariati.

### REVISIONE — 12/09/2026 — 01:15 — PG IA / EQUIPAGGIAMENTO, PROGRESSIONE, DOWN E CAMBIO CONTROLLO

- PG IA senza ITEMS né SLOT ITEM utilizzabili; ABILITÀ attivata dal PLAYER responsabile con SPACE BAR + numero 1–3 secondo l’ordine dei PG IA assegnati; PASSIVA identica e automatica.
- LEVEL UP e BONUS DI FINE AREA scelti dal PLAYER responsabile; BONUS CHEST automatico con le stesse regole degli altri PG; EXP, LVL, BONUS e progressione individuale durante la RUN identici agli altri PG.
- RESURREZIONI eseguite esclusivamente da PG sotto controllo diretto di un PLAYER: PG controllato da PLAYER in DOWN resuscitabile solo da un altro PLAYER; PG IA in DOWN resuscitabile solo da un PLAYER, con rientro in formazione dopo la resurrezione.
- CAMBIO CONTROLLO automatico in DOWN: primo PG IA della lista in SINGLE PLAYER; proprio PG IA assegnato per ciascun PLAYER in CO-OP a 2; solo HOST in CO-OP a 3; non applicabile in CO-OP a 4. Ritorno automatico al proprio PG appena resuscitato.
- I PG IA non hanno volontà propria: non cercano autonomamente MEDI KIT/CHEST e possono usarli solo incontrandoli durante il normale movimento.
- Aggiornati i richiami pertinenti, inclusa la sezione 32, rimuovendo soltanto i punti ora risolti. Il comportamento generale IA resta DA DEFINIRE e sarà affrontato successivamente. Tutte le altre specifiche restano invariate.

### REVISIONE — 12/09/2026 — ITEMS DEFINITIVAMENTE CONFERMATI

- Base ufficiale: contenuto integrale di ROG_ZOMBIE_GDD.md comprendente la REVISIONE — 12/09/2026 — 01:15 — PG IA / EQUIPAGGIAMENTO, PROGRESSIONE, DOWN E CAMBIO CONTROLLO.
- Integrate esclusivamente le decisioni ITEMS nelle sezioni 9, 10/10.3, 31 e 32: comandi SLOT 1–4, anteprima durante la pressione e utilizzo al rilascio, consumo, centro sul CURSORE, RANGE e orientamento TRAPPOLA, bersagli, assenza di FRIENDLY FIRE, effetti e regole geometriche rispetto a MURI/OSTACOLI.
- Nessun comportamento specifico aggiuntivo di lancio/posizionamento rispetto a MURI/OSTACOLI. SMOKE e POZIONE CURATIVA ignorano MURI/OSTACOLI; per GRANATA, MOLOTOV e TRAPPOLA eliminazione integrale dello SPICCHIO/SEZIONE intercettato anche parzialmente.
- ITEMS definitivamente CONFERMATO. Mantenuto il promemoria: alcuni ITEMS del MERCHANT devono essere sbloccati tramite QUEST; quali ITEMS e quali QUEST restano DA DEFINIRE.
- Conservate le regole PG IA e SCORTA ESPLOSIVA, i costi e i valori già definiti. Verificato mediante confronto con la base ufficiale che tutte le parti estranee all’aggiornamento ITEMS sono invariate.

### REVISIONE — 12/09/2026 — DEF, MODIFICATORI, ARROTONDAMENTO STAT CD REDUCTION E AVVIO CD PG01–PG08

- Nuova versione ufficiale di riferimento di ROG_ZOMBIE_GDD.md, basata esattamente sulla precedente versione ufficiale con revisione «ITEMS DEFINITIVAMENTE CONFERMATI».
- Integrate esclusivamente le conferme successive: conversione completa DEF interna/percentuale mostrata; BONUS PROF sommati alla DEF BASE; BONUS CHEST riferiti alla STAT BASE all’avvio RUN, comprensiva del PROF; PASSIVE applicate alle STATS CORRENTI.
- STAT CD REDUCTION a valore intero con arrotondamento matematico (<0,5 per difetto; ≥0,5 per eccesso). Conservata la distinta regola del CD FINALE al decimo di secondo.
- Completato l’avvio CD di tutte le 16 ABILITÀ PG01–PG08. PESTONE, BARRIERA, FUOCO DI SOPPRESSIONE, COLPO LASER, TRIPLO SPARO, INVISIBILITÀ, COLTELLI AVVELENATI e CURA AD AREA: all’attivazione. FUOCO RAPIDO: al termine dei 3 s. PIOGGIA DI GRANATE: all’attivazione, confermato dall’utente durante questo aggiornamento.
- Invariati i casi già consolidati: COLPO GROSSO al consumo del 4° ATTACCO; FUOCO CURATIVO al consumo del 6°; COLPI RESPINGENTI al termine dei 3 s; FILO SPINATO, MULTI SHOT e PIOGGIA DI FRECCE all’attivazione. Conservate le regole di INIZIO RUN e CAMBIO AREA.
- Rimossi dalla sezione 32 i punti ora risolti su DEF e modificatori, arrotondamento della STAT CD REDUCTION, avvio cooldown e rapporto tra base RUN, PROF e CHEST. Gli altri punti DA DEFINIRE restano invariati.
- Verificato mediante confronto integrale con la base ufficiale che tutte le parti estranee a queste modifiche e ai relativi richiami sono invariate; nessun altro valore, costo, effetto o regola è stato modificato.

### REVISIONE — 12/09/2026 — ABILITÀ BONUS

- Nuova versione ufficiale di riferimento di ROG_ZOMBIE_GDD.md, basata esattamente sull'ultima versione ufficiale integrale con revisione «DEF, MODIFICATORI, ARROTONDAMENTO STAT CD REDUCTION E AVVIO CD PG01–PG08».
- Integrate esclusivamente le successive definizioni delle ABILITÀ BONUS confermate dall'utente: risposte 1.1–10.6, compresi PET 2.7 e SCUDO 6.5, e ultimi chiarimenti su LANCIO COLTELLI (suddivisione 45°), MINE (TRIGGER circolare raggio 1 m) e RICOCHET (probabilità di attivazione 40% per ogni HIT valida).
- Aggiornate la sezione 22 e le relative STATS, i richiami a MURI/OSTACOLI, CAMBIO AREA, EFFETTI PERSISTENTI, UPGRADE DANNO di FIRE BULLET, PROJECTILE SPD e regole consolidate nelle sezioni 10, 24, 30 e 31, oltre ai punti pertinenti della sezione 32.
- FIRE BULLET — punto 7.6 (interazione con ATTACCHI ad AREA/multipli) e SCIABOLATA — punto 10.5 (interazione con MURI/OSTACOLI) restano DA DEFINIRE.
- Conservati tutti i valori precedenti di DANNO, RANGE, RAGGIO, CD, DURATA, ATK SPD, DEF, RATE di apparizione e UPGRADE. La probabilità di attivazione di RICOCHET è distinta dal DANNO rimbalzo già pari al 40% del danno originale.
- Verificato mediante confronto integrale con la base ufficiale che tutte le parti estranee alle definizioni delle ABILITÀ BONUS e ai relativi richiami sono invariate. Nessun'altra specifica o punto DA DEFINIRE è stato modificato.


### REVISIONE — 12/09/2026 — FIRE BULLET, SCIABOLATA, PASSIVA PG02, MINI BOSS/BOSS, EXP/LVL E BONUS STATS

- Nuova versione ufficiale di riferimento di ROG_ZOMBIE_GDD.md, basata esattamente sull’ultima versione ufficiale integrale con revisione finale «ABILITÀ BONUS».
- Integrate esclusivamente le decisioni confermate dopo quella versione: FIRE BULLET 7.6 applica BRUCIATURA a tutti i MOB colpiti dall’ATTACCO BASE, inclusi ATTACCHI BASE ad AREA/multipli; SCIABOLATA 10.5 ha AREA a SEMICERCHIO divisa in 4 SPICCHI e segue le regole generali delle AREE DI EFFETTO con MURI/OSTACOLI.
- PASSIVA 1 PG02: con HP <30%, ogni KILL cura l’1% degli HP MASSIMI correnti di PG02; nessun effetto a HP ≥30%.
- MINI BOSS e BOSS esclusi dal totale/massimo MOB dell’AREA e dal limite massimo dei MOB contemporaneamente presenti; punto di SPAWN specifico stabilito in fase di LEVEL DESIGN. Tutto il resto del loro funzionamento rimane DA DEFINIRE.
- EXP/LVL: arrotondamento alla decina con criterio matematico per eccesso/difetto del quoziente EXP/10 all’intero più vicino, quindi moltiplicazione per 10; nessun CAP di LVL. Conservati integralmente i valori della tabella LVL 1 → 11.
- BONUS STATS DI FINE AREA: stessi RATE delle CHEST (HP 25%, ATK 8%, DEF 22%, MOVE SPD 22%, ATK SPD 8%, CD REDUCTION 15%) e stesso valore +5%.
- NUOVA REGOLA SOSTITUTIVA: BONUS CHEST e BONUS STATS DI FINE AREA calcolano il +5% sul VALORE ATTUALE della STAT nel momento dell’acquisizione, includendo UPGRADE permanenti PROF e tutti i precedenti BONUS STATS della RUN, da CHEST e FINE AREA. Per CD REDUCTION si riduce del 5% il valore attuale, arrotondando la STAT risultante matematicamente all’intero più vicino e rispettando il minimo 10; resta invariato l’arrotondamento del CD FINALE al decimo di secondo.
- Le note di revisione precedenti sono conservate come cronologia: le loro indicazioni sulla base iniziale RUN dei BONUS CHEST e sui punti FIRE BULLET 7.6 / SCIABOLATA 10.5 ancora DA DEFINIRE sono superate dalla presente revisione.
- Aggiornate le sezioni pertinenti e i richiami; rimossi dai DA DEFINIRE soltanto i punti risolti. Verificato mediante confronto integrale che tutto il resto è identico alla versione base, incluse le tabelle EXP e MOB, gli UPGRADE PROF e gli UPGRADE delle ABILITÀ BONUS.


### REVISIONE — 12/09/2026 — G PERSONALI, PROF, CHEST RATE, DOWN / MORTE / RIANIMAZIONE E SPETTATORE

- Nuova versione ufficiale di riferimento di ROG_ZOMBIE_GDD.md, basata sulla versione ufficiale integrale con revisione finale «FIRE BULLET, SCIABOLATA, PASSIVA PG02, MINI BOSS/BOSS, EXP/LVL E BONUS STATS».
- Integrate esclusivamente le successive decisioni confermate: saldo G, acquisti e UPGRADE personali; G DROP applicato solo al PLAYER acquirente; ambito PG/PLAYER degli UPGRADE PROF; CHEST RATE con CAP 100% e applicazione del BONUS più alto tra i PLAYER presenti all’inizio della RUN CO-OP.
- Consolidati DOWN a 0 HP, timer DOWN di 20 s e passaggio in MORTE, SCONFITTA senza PG ATTIVI, RIANIMAZIONE con TRIGGER di raggio 2 m e timer di 5 s, pausa/ripresa del timer DOWN e regressione/ripresa dal residuo del timer di RIANIMAZIONE. Nessun altro evento può interrompere la RIANIMAZIONE. Conservati chi può rianimare, 50% degli HP MASSIMI correnti e 2 s di invulnerabilità.
- Integrato SPETTATORE automatico con selezione iniziale casuale, LMB precedente / RMB successivo, ciclo circolare secondo ORDINE PLAYER e controllo della sola visuale. Alla resurrezione del proprio PG al CAMBIO AREA, uscita automatica da SPETTATORE e ripresa della visuale e del controllo diretto del proprio PG.
- Aggiornati i richiami pertinenti e la sezione 32, rimuovendo solo i punti risolti. La velocità di regressione del timer di RIANIMAZIONE resta DA DEFINIRE. Le precedenti note di revisione sono conservate come cronologia.
- Verificato mediante confronto con la base che tutte le altre parti sono invariate, comprese le tabelle EXP e MOB.

### REVISIONE — 12/09/2026 — TRIGGER USCITA AREA, HOST DOPO BOSS, ABBANDONO / DISCONNESSIONE, DANNI DA STATO E CD AL CAMBIO AREA

- Nuova versione ufficiale di riferimento di ROG_ZOMBIE_GDD.md, basata sulla versione ufficiale integrale con revisione finale «G PERSONALI, PROF, CHEST RATE, DOWN / MORTE / RIANIMAZIONE E SPETTATORE».
- Integrate esclusivamente le cinque decisioni confermate richieste: TRIGGER USCITA / PASSAGGIO AREA CIRCOLARE con RAGGIO 4 m; decisione PROSEGUIRE / TORNARE ALL’HUB dopo il BOSS riservata all’HOST, con tasto TORNA ALL’HUB visibile solo nella sua schermata; perdita dei progressi della RUN e trattamento del G secondo SCONFITTA in caso di abbandono/disconnessione, conversione del PG controllato in PG IA e riassegnazione secondo le regole esistenti.
- Consolidata la REGOLA GENERALE DANNI DA STATO per BRUCIATURA e VELENO: primo tick 1 s dopo applicazione/rinnovo, poi ogni 1 s; DANNO per tick = DANNO base dello STATO × numero ISTANZE; CAP di 5 ISTANZE dello stesso STATO sullo stesso bersaglio; DURATA completa rinnovata a ogni applicazione anche al CAP, senza aumento del DANNO oltre il CAP e senza DURATE indipendenti. Aggiornate le vecchie formulazioni incompatibili del VELENO, incluso PG05 / COLTELLI AVVELENATI, conservando i valori specifici.
- Consolidato il mantenimento esatto del CD residuo per un’ABILITÀ già IN CD ma non più ATTIVA al CAMBIO AREA, con prosecuzione del conteggio nella nuova AREA. Invariate le regole per DISPONIBILE, ATTIVA e le eccezioni già definite.
- Aggiornati sezioni pertinenti, richiami, regole consolidate e sezione 32. Tutte le altre parti sono invariate, comprese le tabelle e le precedenti note di revisione, conservate come cronologia.

### REVISIONE — 12/09/2026 — LAYER, MATRICE DELLE COLLISIONI E PET

- Base integrale: versione ufficiale con revisione finale «TRIGGER USCITA AREA, HOST DOPO BOSS, ABBANDONO / DISCONNESSIONE, DANNI DA STATO E CD AL CAMBIO AREA».
- Integrate esclusivamente le successive decisioni sui LAYER e sulle collisioni della conversazione «Funzionamento ASTRA»: distinzione tecnica PLAYER/PG, coppie di PG, MOB e PROJECTILE, separazione TRIGGER_PG/TRIGGER_MOB e AREA_EFFECT_PG/AREA_EFFECT_MOB, rilevamento distinto dal blocco fisico e verifiche geometriche delle AREE affidate alla logica dell’ABILITÀ/AREA.
- MOB ↔ MOB ora SÌ con collider ridotto come nel TEST in engine per rendere le orde più fluide, in sostituzione della precedente regola generale NO; MOB ↔ PROJECTILE_MOB NO. Nessuna dimensione del collider viene inventata.
- Consolidati CHEST e MEDI KIT completamente attraversabili, MINE senza collisione fisica con PG/MOB e TRIGGER_MOB, BARRIERA di PG01 che eredita OSTACOLO.
- Consolidate tutte le sette coppie dei PET, incluso PET ↔ PG NO. I PET non hanno meccanica HP/DANNO e i proiettili dei PG e dei MOB li attraversano senza HIT, DANNO, distruzione o deviazione; non funzionano da scudi mobili.
- Aggiornate sezioni pertinenti, regole consolidate e sezione 32. Le coppie mancanti non sono completate; uso/elenco dei TAG restano DA CONFERMARE. Segnalato senza risolverlo il rapporto tra la nuova regola generale MOB ↔ MOB e la precedente formulazione di ZOMB05 in PRE-ESPLOSIONE.
- Conservati integralmente tutti gli altri contenuti, valori, tabelle e note di revisione precedenti. Verificato il confronto con la base integrale: nessuna modifica esterna alle integrazioni sopra elencate e ai relativi richiami.


### REVISIONE — 12/09/2026 — CLASSIFICAZIONE TECNICA, STATI LOGICI, COLLISIONI E TAG

- Usato esattamente il file ufficiale integrale con revisione finale «LAYER, MATRICE DELLE COLLISIONI E PET» come base. Integrate esclusivamente le successive decisioni confermate nella conversazione «Funzionamento ASTRA».
- Consolidata la classificazione tecnica di tutte le ABILITÀ PG, PASSIVE PG, ABILITÀ BONUS, ITEMS e attacchi/effetti MOB tramite LAYER esistenti e verifiche logiche; nessun nuovo layer. L’ESPLOSIONE di ZOMB05 usa una verifica logica unica su PG e MOB validi.
- Confermati stati, transizioni e modificatori logici senza LAYER/TAG dedicati. DOWN mantiene le collisioni normali; MORTE PG e relativa SPRITE DI MORTE a terra non hanno collisioni. RESURREZIONE ripristina le collisioni normali, mantenute anche durante INVULNERABILITÀ.
- Confermato PROJECTILE_MOB ↔ PROJECTILE_MOB = NO, senza HIT o effetti reciproci. ZOMB05 conserva le normali collisioni MOB anche in PRE-ESPLOSIONE, inclusa MOB ↔ MOB SÌ; eliminata la vecchia eccezione di attraversamento/sovrapposizione.
- RIANIMAZIONE usa TRIGGER_PG senza collisione fisica aggiuntiva. NPC HUB: collider OSTACOLO + TRIGGER_PG per interazione con F. EXIT HUB: TRIGGER_PG + F, senza blocco fisico.
- Per ora nessun TAG tecnico dedicato, inclusi PG e MOB: identificazione tramite LAYER + componenti/script + stati logici. Questa decisione finale supera le precedenti valutazioni sui singoli TAG; nuovi TAG solo se emergerà necessità concreta durante lo sviluppo.
- Aggiornate le sezioni pertinenti e la sezione 31; rimossi dalla sezione 32 i dubbi risolti su TAG e collisioni. Conservati i punti ancora aperti, incluse le dimensioni numeriche del collider MOB, la velocità di regressione del timer di RIANIMAZIONE e i dettagli geometrici delle AREE non specificati.
- Valori e contenuti estranei all’aggiornamento, tabelle di bilanciamento e note di revisione precedenti conservati invariati.

### REVISIONE — 12/09/2026 — PG01 ATTACCO BASE

- Aggiornato esclusivamente PG01 ATTACCO BASE: AREA a CONO, RANGE 4 m (interno 400), AMPIEZZA 75°, in sostituzione del precedente cono 3 m × 90°. Aggiornate la tabella STATS BASE e la scheda PG01.
- Restano invariati AREA HITSCAN istantanea, bersagli, DANNO, ATK SPD, orientamento verso il CURSORE, blocco dei MOB schermati da MURI/OSTACOLI e tutti gli altri aspetti dell’attacco e del PG01.

### REVISIONE — 12/09/2026 — PG01 PESTONE RETTANGOLARE

- PESTONE usa un’AREA RETTANGOLARE frontale 3 m × 7 m: il lato frontale è largo 3 m e la profondità è 7 m. Sostituisce la precedente geometria a cono.
- Invariati 40 DANNO, CD BASE 10 s dall’attivazione, Q, bersagli MOB, schermatura MURO/OSTACOLO, CD REDUCTION e regole di INIZIO RUN/CAMBIO AREA. Nessuna modifica a BARRIERA o ATTACCO BASE.


### REVISIONE — 12/09/2026 — PG01 PESTONE SLOW

- PESTONE applica ai MOB colpiti SLOW del 30% per 3 s. La riapplicazione rinnova la durata senza cumulare lo stesso effetto, secondo le regole generali.
- Invariati AREA RETTANGOLARE frontale 3 m × 7 m (larghezza 3 m, profondità 7 m), 40 DANNO e CD BASE 10 s.

### REVISIONE — 12/09/2026 — CORREZIONE EDITORIALE DELLA NUMERAZIONE

- Mantenuta ATTRIBUZIONE KILL come sezione 10.7; rinumerate REGOLA GENERALE DANNI DA STATO — BRUCIATURA E VELENO da 10.7 a 10.8 e LAYER E MATRICE DELLE COLLISIONI da 10.8 a 10.9. Aggiornati tutti e soli i riferimenti interni interessati. Nessuna modifica a valori, regole, testo di design o altre numerazioni.

### REVISIONE — 12/09/2026 — PG02 ATK SPD BASE E FUOCO RAPIDO

- Confermato ATK SPD BASE PG02 = 400, equivalente a 4 ATTACCHI/s, in sostituzione del precedente 150.
- FUOCO RAPIDO applica un BONUS temporaneo +30% ATK SPD corrente, in sostituzione del precedente valore fisso 300. Senza altri modificatori: 400 × 1,30 = 520, equivalente a 5,2 ATTACCHI/s.
- Invariati DURATA 3 s e CD BASE 10 s, avviato al termine della DURATA. Alla scadenza viene rimosso soltanto il BONUS temporaneo, preservando gli altri modificatori e BONUS acquisiti.
- Aggiornate tabella STATS PG, scheda PG02 e classificazione tecnica di FUOCO RAPIDO. Nessuna modifica alle altre regole, ai valori degli altri PG o alle revisioni precedenti.

### REVISIONE — 13/09/2026 — PG02 FUOCO RAPIDO +40% PER 4 s

- FUOCO RAPIDO: BONUS temporaneo +40% ATK SPD corrente per 4 s. Questa revisione supera i precedenti +30% e DURATA 3 s riportati nelle note storiche.
- ATK SPD BASE PG02 invariato a 400; senza altri modificatori, FUOCO RAPIDO porta ATK SPD a 560 = 5,6 ATTACCHI/s.
- CD BASE invariato a 10 s, avviato al termine dei 4 s di DURATA. Alla scadenza viene rimosso soltanto il BONUS temporaneo, preservando gli altri modificatori e BONUS acquisiti.
- Aggiornate scheda PG02, classificazione tecnica e regola consolidata di avvio CD. COLPI RESPINGENTI conserva DURATA 3 s; nessuna modifica alle altre meccaniche. Revisioni precedenti conservate come storico.

### REVISIONE — 13/09/2026 — PG02 FUOCO DI SOPPRESSIONE A PROIETTILI FISICI

- FUOCO DI SOPPRESSIONE sostituisce il precedente HITSCAN a CONO da 30 DANNO con una raffica di 50 PROIETTILI FISICI in 3 s, ciascuno da 3 DANNO prima della DEF.
- Ampiezza totale del CONO 50°, divisa in 5 SPICCHI da 10°. Numerazione da destra a sinistra e sequenza di sparo 1-2-3-4-5-4-3-2-1-2-3-ecc., senza duplicare gli estremi, fino a 50 PROIETTILI complessivi.
- Confermati RANGE 10 m e CD BASE 12 s, avviato all’attivazione. I valori intermedi discussi di 60 PROIETTILI e 2 DANNO per PROIETTILE sono superati.
- Aggiornate scheda PG02 e classificazione tecnica su PROJECTILE_PG. Nessuna modifica a FUOCO RAPIDO, alle PASSIVE o agli altri PG; revisioni precedenti conservate come storico.

### REVISIONE — 13/09/2026 — PG02 CENTRO SPICCHI E TEST PROTOTIPO OK

- Conferma del proprietario: ogni PROIETTILE di FUOCO DI SOPPRESSIONE segue il CENTRO dello SPICCHIO corrente. Direzioni relative all’asse del CONO: −20°, −10°, 0°, +10°, +20°, poi ritorno senza ripetere gli estremi.
- Invariati 50 PROIETTILI in 3 s, 3 DANNO per PROIETTILE prima della DEF, RANGE 10 m e CD BASE 12 s dall’attivazione. Aggiornate scheda PG02 e classificazione tecnica; nessuna modifica alle altre regole.
- Il proprietario ha comunicato «test su PG02 esito OK» dopo l’aggiornamento del prototipo. Registrato l’esito positivo della prova utente; dettagli dei test automatici e limiti tecnici in Docs/PG02Prototype.md. La conferma riguarda il prototipo e non introduce ulteriori regole di gameplay.

### REVISIONE — 13/09/2026 — PG03 CD ABILITÀ E INTERVALLO TRIPLO SPARO

- COLPO LASER: CD BASE modificato da 10 s a 18 s, sempre avviato all’attivazione.
- TRIPLO SPARO: CD BASE modificato da 15 s a 10 s, sempre avviato all’attivazione; intervallo tra i colpi modificato da 0,3 s a 0,5 s.
- Modifiche confermate dal proprietario. Danni, numero di proiettili, geometria, RANGE, sequenza e tutte le altre regole restano invariati. Revisioni precedenti conservate come storico.

### REVISIONE — 13/09/2026 — PG03 MOVE SPD 110

- MOVE SPD BASE di PG03 modificato da 100 a 110 su richiesta del proprietario. Aggiornati tabella STATS e asset PG03 nel repository; tutte le altre STATS e regole restano invariate.

### REVISIONE — 13/09/2026 — PG03 CALIBRO PERFORANTE E ATK SPD 60

- ATK SPD BASE PG03 da 70 a 60: 0,6 ATTACCHI/s, intervallo 100/60 s.
- CALIBRO PERFORANTE: prima HIT al 100%, seconda con DANNO ridotto del 30% e terza del 60% rispetto al danno originale del PROIETTILE, prima della DEF. Con ATK 50: 50 / 35 / 20. Il conteggio delle HIT riparte per ogni PROIETTILE; non si estende alle ABILITÀ.
- Invariati 2 PERFORAZIONI, arresto sul terzo MOB, RANGE, blocco da MURI/OSTACOLI e tutte le altre regole. Revisioni precedenti conservate come storico.

### REVISIONE — 13/09/2026 — COLPO LASER DANNO 35

- DANNO di COLPO LASER ridotto da 50 a 35 prima della DEF, su richiesta del proprietario. Invariati AREA 20 × 2 m, una HIT per MOB, attraversamento di MURI/OSTACOLI e CD BASE 18 s dall’attivazione.

### REVISIONE — 13/09/2026 — PG04 ATK, ATK SPD E PIOGGIA DI GRANATE

- ATK BASE PG04 da 30 a 25; ATK SPD da 75 a 70 (0,7 ATTACCHI/s). COLPO GROSSO +80% porta ATK base 25 a 45.
- RAGGIO dell’AREA EFFETTO di PIOGGIA DI GRANATE da 7 m a 5 m. RAGGIO delle singole esplosioni invariato a 1,5 m; RANGE dell’ATTACCO BASE invariato a 7 m.

### REVISIONE — 13/09/2026 — PG04 ATK SPD 65 E RAGGIO ATTACCO BASE 1,25 m

- ATK SPD PG04 da 70 a 65 (0,65 ATTACCHI/s); RAGGIO ATTACCO BASE da 1,5 m a 1,25 m.
- Per ereditarietà, le singole esplosioni di PIOGGIA DI GRANATE hanno RAGGIO 1,25 m; COLPO GROSSO applica +50% e raggiunge 1,875 m. PYROMANIA segue le rispettive geometrie. AREA EFFETTO di PIOGGIA DI GRANATE invariata a RAGGIO 5 m.

### REVISIONE — 13/09/2026 — PG04 DELAY ATTACCO BASE

- Inserito delay di 0,3 s tra lancio dell’ATTACCO BASE e HIT, su richiesta del proprietario. Nessun proiettile fisico introdotto; cadenza e consumo delle cariche restano legati al lancio.
