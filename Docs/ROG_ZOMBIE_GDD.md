# ROG ZOMBIE — Game Design Document

Versione consolidata — 10/09/2026 — aggiornamento completo fino alla BALISTICA GENERALE  
Destinazione nel repository Unity: `Docs/ROG_ZOMBIE_GDD.md`  
Fonti: `ROG_ZOMBIE_WORLD_2026-09-08`, documento «ROG ZOMBIE — WORLD — Documento di riferimento»; GDD con revisione CD REDUCTION; conversazione «Funzionamento ASTRA» (`6a9fce64-149c-83ed-ade6-79f191001021`).

Questo GDD raccoglie integralmente le specifiche presenti nella fonte, organizzate in sezioni e tabelle Markdown. La base documentale più recente disponibile è il GDD aggiornato del 09/09/2026 (file locale salvato alle 20:22), derivato dal GDD dell’08/09/2026 con revisione CD REDUCTION. Le successive regole confermate nella conversazione «Funzionamento ASTRA», fino alla conferma finale della BALISTICA GENERALE, prevalgono sui dati precedenti incompatibili. Le note aggiunte per evidenziare lacune e ambiguità sono distinte dalle regole di gioco e non introducono nuove meccaniche.

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
- Dopo la vittoria contro il BOSS della città il PLAYER può proseguire nella città successiva oppure tornare volontariamente all'HUB.
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
- Tutti i PG VIVI devono essere contemporaneamente nel TRIGGER dell’USCITA. Forma e dimensioni sono da definire in sviluppo/testing.
- Finché manca un PG VIVO non succede nulla: i PG possono entrare e uscire liberamente dal TRIGGER.
- I PG in DOWN impediscono il passaggio e devono essere RESUSCITATI prima. I PG in MORTE non devono raggiungere l’USCITA e non bloccano la transizione.
- Soddisfatta la condizione, si apre la SCELTA BONUS di fine AREA e il GAMEPLAY viene completamente BLOCCATO: niente movimento, attacchi, ABILITÀ o ITEMS.
- Ogni PLAYER sceglie 1 BONUS tra 5: 3 BONUS STATS BASE + 2 BONUS relativi alle ABILITÀ BONUS.
- Dopo la scelta e CONFERMA di tutti i PLAYER, una breve SCHERMATA DI CARICAMENTO prepara NUOVA AREA e FIRST SPAWN, applicando le regole SPAWN. Nello stesso caricamento vengono determinate/generate CHEST e MEDI KIT secondo le sezioni 19 e 26.
- Terminato il caricamento, il FIRST SPAWN è già presente OFF-SCREEN e il GAMEPLAY riprende.
- I PG VIVI recuperano il 15% degli HP MASSIMI correnti, senza superarli. I PG resuscitati dal DOWN prima del passaggio rientrano in questa categoria.
- I PG in MORTE vengono RESUSCITATI automaticamente al 50% degli HP MASSIMI: non ricevono anche il 15%.
- Non è possibile tornare nell’AREA precedente. L’EXP DROP continua a crescere anche passando a una nuova CITTÀ e non si resetta.

<a id="sezione-7"></a>

## 7. HUB

- L'HUB è una piccola zona sicura ed esplorabile tra le RUN.
- MERCHANT: acquisto e gestione degli ITEMS tramite G.
- PROF: acquisto degli UPGRADE permanenti tramite G.
- EXIT: accesso alla preparazione e partenza della RUN.
- La preparazione mostra 4 BANNER PG per la PARTY.
- Il ROSTER contiene 8 PG. I PG confermati in SELEZIONE PG risultano BLOCKED; quelli non ancora sbloccati sono mostrati come silhouette.
- Tutti i PLAYER devono confermare la preparazione prima della partenza.

<a id="sezione-8"></a>

## 8. PREPARAZIONE RUN

### 8.1 ACCESSO E SCHERMATA PRINCIPALE

- Nell’HUB, interagendo con il TRIGGER dell’USCITA/EXIT si apre direttamente PREPARAZIONE RUN.
- La PARTY è sempre di 4 PG. La schermata mostra contemporaneamente la configurazione di tutti e 4.
- I colori seguenti identificano gli elementi dei mockup di riferimento; non fissano la grafica definitiva.

| RIFERIMENTO | ELEMENTO / COMPORTAMENTO |
| --- | --- |
| NERO | Bordo dello schermo. |
| ROSSO | 4 BANNER PG, uno per posto della PARTY; click sul proprio BANNER apre SELEZIONE PG. |
| BLU | NOME PLAYER; per un PG IA viene mostrato il NOME PG. |
| VERDE | SLOT ITEMS disponibili: numero individuale in base agli sblocchi, da 1 a 4; può differire fra membri della PARTY. |
| GIALLO | SLOT ABILITÀ ATTIVA selezionata. |
| ROSA | SLOT PASSIVA ATTIVA selezionata. |
| GRIGIO | CONFERMA della preparazione, necessaria per avviare la RUN. |

### 8.2 SELEZIONE PG — UX

| RIFERIMENTO | ELEMENTO / COMPORTAMENTO |
| --- | --- |
| NERO | Bordo dello schermo. |
| ROSSO | CASELLE degli 8 PG del ROSTER. Click su una CASELLA aggiorna anteprima e informazioni del PG. PG non sbloccati in silhouette; PG occupati con ICONA BLOCKED. |
| VERDE | ANTEPRIMA: DESCRIZIONE a sinistra, ANTEPRIMA GRAFICA del PG a destra. |
| AZZURRO | Macro sezione STATS, ABILITÀ e PASSIVE. |
| BLU | STATS visibili: HP / ATK / DEF / MOVE SPD / ATK SPD / CD REDUCTION. RANGE è NASCOSTA e non viene mostrata. |
| GIALLO | 2 CASELLE ABILITÀ; quella cliccata si illumina e viene selezionata. |
| ROSA | 2 CASELLE PASSIVA; quella cliccata si illumina e viene selezionata. |
| VIOLA | DESCRIZIONE dell’ultima ABILITÀ/PASSIVA cliccata o selezionata. |
| GRIGIO | CONFERMA PG + ABILITÀ + PASSIVA e ritorno a PREPARAZIONE RUN. |

- Ogni PG dispone di 2 ABILITÀ e 2 PASSIVE: selezionarne esattamente 1 di ciascuna per la RUN. ABILITÀ con Q; PASSIVA automatica, senza occupare SLOT BONUS.
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

| PLAYER | PG IA | RESPONSABILITÀ DELLA PREPARAZIONE IA |
| --- | --- | --- |
| 4 | 0 | Ogni PLAYER configura il proprio PG. |
| 3 | 1 | IA assegnata all’HOST. |
| 2 | 2 | 1 IA assegnata a ciascun PLAYER. |
| 1 | 3 | Tutte le IA fanno capo all’unico PLAYER, che gestisce e conferma anche i 3 PG IA; controlla direttamente 1 PG. |

- In caso di abbandono durante PREPARAZIONE, il PG del PLAYER uscente cessa di essere BLOCKED; il posto è sostituito da IA.
- La responsabilità IA viene riassegnata secondo il numero di PLAYER rimasti. Il PLAYER che riceve la nuova IA deve effettuare e CONFERMARE la relativa SELEZIONE PG.

### 8.6 CONFERMA, ANNULLAMENTO E AVVIO RUN

- La CONFERMA di PREPARAZIONE RUN è distinta dalla CONFERMA di SELEZIONE PG.
- Dopo CONFERMA PREPARAZIONE, il PLAYER non può effettuare azioni/modifiche alla configurazione. Può ANNULLARE la CONFERMA per tornare a modificarla.
- La RUN parte quando tutti i PLAYER risultano contemporaneamente CONFERMATI, con le selezioni dei PG IA di loro competenza confermate.
- La PARTY viene bloccata e si apre una SCHERMATA DI CARICAMENTO: generazione AREA iniziale, MOB del FIRST SPAWN, eventuale CHEST e MEDI KIT.
- Il FIRST SPAWN rispetta le sezioni 14–15; CHEST e MEDI KIT le sezioni 19 e 26. Terminato il caricamento, inizia il GAMEPLAY.
- La RUN parte da LVL 1; i CD delle ABILITÀ iniziano IN CD secondo la sezione 10.4.
- Animazioni, suoni, aspetto definitivo delle icone e dimensioni dei pannelli restano da realizzare in sviluppo/testing.

<a id="sezione-9"></a>

## 9. ITEMS

| ITEM | AREA | DURATA | EFFETTO | DANNO / CURA | COSTO |
| --- | --- | --- | --- | --- | --- |
| MOLOTOV | Circolare, raggio 5 m | 5 s | Area incendiata | 5 HP/s | 50 G |
| GRANATA | Circolare, raggio 2,5 m | Istantaneo | Esplosione | 40 HP | 50 G |
| SMOKE | Circolare, raggio 5 m | 4 s | PG invisibile | — | 50 G |
| POZIONE CURATIVA | Circolare, raggio 4 m | 4 s | Cura nell'area | 10 HP/s | 100 G |
| TRAPPOLA | Rettangolare 1 × 4 m | 5 s | Slow 40% | 5 HP/s | 50 G |

- SLOT ITEM di partenza: 1.
- UPGRADE massimi SLOT ITEM: 3.
- SLOT ITEM massimi: 4.
- Gli ITEMS vengono acquistati dal MERCHANT e assegnati direttamente agli SLOT: non esiste un inventario separato.
- Per liberare uno SLOT, l'ITEM può essere venduto al MERCHANT al 50% del prezzo di acquisto.
- Gli ITEMS possono essere solamente riordinati durante la PREPARAZIONE RUN, tramite drag & drop LMB e scambio se lo SLOT è occupato (sezione 8.4).
- Gli ITEMS inutilizzati vengono persi in caso di sconfitta della RUN; vengono mantenuti se si torna volontariamente all'HUB dopo un BOSS.

- LIMITE GENERALE: ogni SLOT ITEM contiene normalmente 1 ITEM, per tutti i PG. Il limite riguarda il singolo SLOT, non il totale posseduto dal PG.
- Eccezione — PG04 / SCORTA ESPLOSIVA: fino a 3 GRANATE oppure 3 MOLOTOV dello stesso tipo per SLOT; gli altri ITEMS restano a 1 per SLOT.

<a id="sezione-10"></a>

## 10. SISTEMA DI COMBATTIMENTO

- Movimento: W-A-S-D.
- Mira: mouse.
- ATTACCO BASE: LMB; tenendo premuto LMB si mantiene il fuoco continuo.
- INTERAZIONE: F.
- ABILITÀ selezionata: Q.
- Munizioni infinite e nessun reload.
- Gli ATTACCHI BASE seguono il comportamento definito per la singola arma: AREA HITSCAN, HITSCAN + AREA IMPATTO oppure PROIETTILI FISICI, secondo la classificazione della sezione 10.1. Gli attacchi con proiettile fisico hanno un tempo di viaggio e non sono genericamente istantanei.
- Regola base del danno: DANNO finale = ATK × (1 − DEF%).
- ATK SPD: 100 = 1 colpo/s.
- MOVE SPD: 100 = 2 m/s.
- CD REDUCTION: STAT modificatore del PG, senza unità in secondi; VALORE BASE neutro = 100. CD REDUCTION 100 = 100% del CD BASE della singola ABILITÀ selezionata.
- Ogni ABILITÀ mantiene il proprio CD BASE in secondi, definito nella relativa scheda PG.
- Formula: CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100).
- Esempio: CD BASE ABILITÀ 10 s e CD REDUCTION 90 → CD FINALE 9 s. Un valore CD REDUCTION più basso riduce il cooldown.
- RANGE: 100 = 1 m nel sistema interno.
- DEF interno: 100 corrisponde a 0% di modificatore mostrato al PLAYER.

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
- La PERFORAZIONE modifica esclusivamente la regola standard «HIT MOB → sparisce». PG03 / CALIBRO PERFORANTE: 2 PERFORAZIONI → attraversa il 1° e il 2° MOB, danneggia il 3° e sparisce; DANNO e RANGE invariati.
- PG01, PG04 e PG05 non usano questo volo fisico per l’ATTACCO BASE. Le ABILITÀ mantengono le regole specifiche delle proprie schede: la classificazione dell’ATTACCO BASE non ne cambia automaticamente la meccanica.
- Non estendere ai PROIETTILI dei MOB le collisioni alleate dei PG: ZOMB04 conserva le proprie regole (sezione 13.8).

### 10.2 MURI E OSTACOLI

- MURO: blocca MOVIMENTO e ATTACCHI; non può essere sorvolato.
- OSTACOLO: blocca MOVIMENTO e ATTACCHI normali; può essere sorvolato dai PROIETTILI con traiettoria PARABOLICA.
- Le eccezioni al blocco degli ATTACCHI sono definite nelle singole schede.

### 10.3 AREE CIRCOLARI — MURI E OSTACOLI

| ATTACCO / EFFETTO | SUDDIVISIONE | REGOLA |
| --- | --- | --- |
| PG04 — ATTACCO BASE | 4 SPICCHI da 90° | Eliminazione completa dello SPICCHIO intercettato. |
| PG04 — PIOGGIA DI GRANATE | 4 SPICCHI da 90° per singola esplosione | Stesse regole dell’ATTACCO BASE. |
| PG04 — COLPO GROSSO | 4 SPICCHI da 90° per singola esplosione | Stesse regole dell’ATTACCO BASE. |
| PG04 — PYROMANIA | 4 SPICCHI da 90° per AREA INCENDIATA | Stesse regole dell’ATTACCO BASE. |
| PG07 — LUCKY SHOT | 4 SPICCHI da 90° | Eliminazione completa dello SPICCHIO intercettato. |
| ZOMB03 — AREA DI DANNO | 8 SPICCHI da 45° | Eliminazione completa dello SPICCHIO intercettato. |
| ZOMB05 — ESPLOSIONE | 8 SPICCHI da 45° | Eliminazione completa dello SPICCHIO intercettato. |
| PG07 — PIOGGIA DI FRECCE | Nessun taglio dell’AREA da parte di MURI/OSTACOLI | Le FRECCE ignorano MURI/OSTACOLI lungo la caduta e verificano solo il PUNTO D’IMPATTO. |

- Se un MURO/OSTACOLO intercetta anche parzialmente uno SPICCHIO, l’intero SPICCHIO viene eliminato e non genera HIT/DANNO. Gli altri SPICCHI restano validi.
- Gli SPICCHI definiscono esclusivamente la geometria; non generano HIT separate. Un bersaglio sul confine tra SPICCHI non riceve HIT aggiuntive per questo motivo.
- La regola a 4 SPICCHI di PIOGGIA DI GRANATE sostituisce la precedente eccezione che consentiva alle sue esplosioni di ignorare MURI/OSTACOLI.
- Questa suddivisione riguarda esclusivamente i casi elencati. FILO SPINATO usa le proprie 10 SEZIONI da 36° (sezione 12).

### 10.4 CD — INIZIO RUN E CAMBIO AREA

- All’INIZIO RUN i CD di TUTTE le ABILITÀ partono da capo: ogni ABILITÀ inizia IN CD e diventa disponibile al completamento del proprio CD FINALE.
- Al CAMBIO AREA un’ABILITÀ già DISPONIBILE rimane DISPONIBILE.
- Le ABILITÀ ancora ATTIVE terminano al CAMBIO AREA; i loro CD ripartono da capo applicando CD REDUCTION. Eventuali ATTACCHI potenziati rimanenti di COLPO GROSSO e FUOCO CURATIVO vengono persi.
- RIPARTENZA del CD significa attesa dell’intero CD FINALE; non significa rendere immediatamente disponibile l’ABILITÀ.
- Le specifiche terminazioni di EFFETTI e PASSIVE al CAMBIO AREA sono riportate nelle schede PG.

### 10.5 STATO DI INVISIBILITÀ

- I MOB non seguono i PG INVISIBILI e non iniziano nuovi ATTACCHI contro di loro. Gli ATTACCHI già in corso proseguono fino al termine.
- ATTACCO, ABILITÀ, uso di ITEM e INTERAZIONE annullano INVISIBILITÀ esclusivamente per il PG che compie l’AZIONE.
- MOVIMENTO e RESUSCITARE un alleato non annullano INVISIBILITÀ.
- Il BONUS MOVE SPD associato termina per il PG quando perde INVISIBILITÀ.
- Alla fine dell’effetto i MOB possono nuovamente rilevare, seguire e attaccare quel PG secondo le normali regole di targeting.

<a id="sezione-11"></a>

## 11. ROSTER PG — STATS BASE

| PG | ARMA | HP | ATK | DEF | MOVE SPD | ATK SPD | CD REDUCTION | RANGE |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| PG01 | Shotgun | 100 | 20 | +15% | 100 | 85 | 100 | 3 m |
| PG02 | Assault Rifle | 100 | 10 | 0% | 100 | 150 | 100 | 10 m |
| PG03 | Sniper Rifle | 80 | 50 | −10% | 100 | 70 | 100 | 20 m |
| PG04 | Grenade Launcher | 100 | 30 | 0% | 100 | 75 | 100 | 7 m |
| PG05 | Knife | 100 | 30 | −10% | 130 | 150 | 100 | 2 m |
| PG06 | Revolver | 120 | 40 | 0% | 100 | 80 | 100 | 10 m |
| PG07 | Bow | 100 | 30 | 0% | 110 | 110 | 100 | 15 m |
| PG08 | Heavy Machine Gun | 100 | 7 | +10% | 90 | 200 | 100 | 8 m |

Nota: le STATS sopra riportano gli ultimi valori definiti esplicitamente nel progetto, incluse la revisione CD REDUCTION e le correzioni RANGE di PG03 e PG04. CD REDUCTION è una STAT modificatore con VALORE BASE 100 per PG01–PG08; i CD BASE in secondi restano esclusivamente nelle singole ABILITÀ e vengono modificati nel CD FINALE secondo la formula della sezione 10.

<a id="sezione-12"></a>

## 12. ABILITÀ E PASSIVE DEI PG

I valori CD in secondi riportati nelle schede seguenti sono i CD BASE delle singole ABILITÀ, aggiornati dove modificati esplicitamente nella conversazione. Il CD FINALE dipende dalla STAT CD REDUCTION del PG secondo la formula della sezione 10.

### PG01 — Shotgun

- ATTACCO BASE: AREA HITSCAN istantanea, senza singoli proiettili; CONO di portata 3 m e ampiezza 90°, orientato verso il CURSORE. Colpisce tutti i MOB validi nel CONO con ATK 20 per ciascuno, prima della DEF; il DANNO non è suddiviso e non vengono simulati singoli pallettoni. RANGE interno 300 = portata 3 m. MURI e OSTACOLI bloccano l’ATTACCO verso i MOB schermati. ATK SPD 85 = 0,85 ATTACCHI/s.

- ABILITÀ 1 — PESTONE: 40 DANNO; cono frontale 3 m × 5 m; CD 10 s.
- ABILITÀ 2 — BARRIERA: piazza un muro invalicabile e indistruttibile di 7 m × 1 m; durata 5 s; CD 15 s.
- PASSIVA 1: con HP <50% ottiene +15% DEF; l'effetto termina quando gli HP tornano ≥50%.
- PASSIVA 2: con HP <50% ottiene +15% ATK; l'effetto termina quando gli HP tornano ≥50%.

### PG02 — Assault Rifle

- ATTACCO BASE: PROIETTILE BALISTICO non PERFORANTE, verso il CURSORE; ATK 10, RANGE 10 m, ATK SPD 150 = 1,5 ATTACCHI/s. Segue le REGOLE GENERALI DEI PROIETTILI.

- ABILITÀ 1 — FUOCO RAPIDO: ATK SPD 300 (3 colpi/s); durata 3 s; CD 10 s.
- ABILITÀ 2 — FUOCO DI SOPPRESSIONE: 30 DANNO; cono frontale 10 m / 45°; CD 12 s.
- PASSIVA 1: con HP <30%, ogni KILL cura 1% HP; non ha effetto a HP ≥30%.
- PASSIVA 2: ogni 15 KILL entra in RAGE per 2 s con +30% ATK.

### PG03 — Sniper Rifle

- ATTACCO BASE: PROIETTILE BALISTICO non PERFORANTE di base; ATK 50, ATK SPD 70 = 0,7 ATTACCHI/s; RANGE interno 2000 = 20 m. Segue le REGOLE GENERALI DEI PROIETTILI.
- ABILITÀ 1 — COLPO LASER: sostituisce COLPO PERFORANTE. Emette istantaneamente un RAGGIO verso il CURSORE; AREA 20 × 2 m (portata 20 m, larghezza 2 m); 50 DANNO una sola volta a ciascun MOB nell’AREA; CD BASE 10 s. Attraversa MURI e OSTACOLI: eccezione alla REGOLA DI BLOCCO.
- ABILITÀ 2 — TRIPLO SPARO: 3 proiettili da 40 DANNO; cono totale 20 m / 30°; ogni proiettile copre 10°; sequenza SINISTRA → CENTRO → DESTRA; intervallo 0,3 s; CD BASE 15 s.
- PASSIVA 1 — CALIBRO PERFORANTE: rende PERFORANTI i PROIETTILI dell’ATTACCO BASE con 2 PERFORAZIONI. Il 1° e il 2° MOB ricevono DANNO e vengono attraversati; il 3° riceve DANNO e arresta il PROIETTILE. DANNO e RANGE dell’ATTACCO BASE restano invariati. MURI/OSTACOLI e limite RANGE possono interrompere prima la traiettoria.
- PASSIVA 2 — PUNTO DEBOLE: l’ATTACCO BASE applica un MARCHIO al MOB colpito; il MOB marchiato ha DEF −20%; la HIT successiva sul MOB marchiato rimuove il MARCHIO.

### PG04 — Grenade Launcher

#### ATTACCO BASE

- ATK 30; ATK SPD 75 = 0,75 ATTACCHI/s; RANGE interno 700 = 7 m.
- HITSCAN + AREA IMPATTO: il punto d’impatto è determinato istantaneamente nella direzione di mira, entro RANGE 7 m, senza PROIETTILE FISICO in volo. Se il CURSORE è oltre 7 m, la destinazione è limitata a 7 m nella sua direzione.
- Si applica l’AREA CIRCOLARE nel punto d’impatto. Resta la regola specifica di determinazione della destinazione: solo un MURO può anticipare l’impatto; OSTACOLI e MOB non lo anticipano. Il precedente volo PARABOLICO dell’ATTACCO BASE è sostituito dall’HITSCAN; le ABILITÀ conservano le proprie regole.
- AREA ESPLOSIONE CIRCOLARE, RAGGIO 1,5 m: 30 DANNO a ciascun MOB valido prima della DEF.
- MURI/OSTACOLI interrompono l’AREA secondo la regola dei 4 SPICCHI da 90° (sezione 10.3).

#### ABILITÀ 1 — PIOGGIA DI GRANATE

- Genera direttamente 10 ESPLOSIONI, senza PROIETTILI, in posizioni RANDOM nell’AREA EFFETTO CIRCOLARE di RAGGIO 7 m.
- Centro: posizione del CURSORE all’attivazione; nessun limite di distanza da PG04.
- Sequenza CASUALE nell’arco complessivo di 3 s; CD BASE 12 s.
- Eredita esclusivamente ATK e AREA ESPLOSIONE dell’ATTACCO BASE: ATK 30 e RAGGIO 1,5 m per esplosione. Non eredita traiettoria, RANGE o IMPATTO del PROIETTILE.
- Le esplosioni possono sovrapporsi. Ogni esplosione applica indipendentemente il proprio DANNO; lo stesso MOB può ricevere HIT da più esplosioni.
- Ogni AREA di esplosione segue la regola dei 4 SPICCHI da 90°, con eliminazione completa degli SPICCHI intercettati da MURI/OSTACOLI.

#### ABILITÀ 2 — COLPO GROSSO

- Potenzia i successivi 4 ATTACCHI BASE: +80% ATK (30 → 54) e +50% RAGGIO ESPLOSIONE (1,5 → 2,25 m).
- Modifica esclusivamente ATK e RAGGIO ESPLOSIONE. Le altre STATS e regole restano invariate, inclusa la geometria a 4 SPICCHI.
- Nessuna DURATA massima: le cariche restano disponibili fino al consumo. Ogni ATTACCO effettuato consuma una carica, anche in caso di MISS.
- CD BASE 10 s, avviato al consumo del 4° ATTACCO.
- Al CAMBIO AREA le cariche residue vengono perse e, se l’ABILITÀ era ATTIVA, il CD riparte da capo. Se era DISPONIBILE, rimane DISPONIBILE.

#### PASSIVA 1 — SCORTA ESPLOSIVA

- Quando selezionata, permette di accumulare fino a 3 GRANATE oppure 3 MOLOTOV dello stesso tipo per SLOT ITEM.
- Influenza contemporaneamente entrambi i tipi di ITEM. Gli altri ITEMS restano a 1 per SLOT.

#### PASSIVA 2 — PYROMANIA

- Le AREE ESPLOSIONE diventano AREE INCENDIATE per 3 s, con 5 HP/s per AREA.
- Si applica ad ATTACCO BASE, PIOGGIA DI GRANATE e COLPO GROSSO.
- L’AREA INCENDIATA corrisponde all’AREA ESPLOSIONE: RAGGIO 1,5 m per ATTACCO BASE/PIOGGIA DI GRANATE; 2,25 m per COLPO GROSSO.
- Le AREE INCENDIATE possono sovrapporsi e i loro DANNI si sommano.
- MURI/OSTACOLI bloccano il DANNO secondo la regola dei 4 SPICCHI da 90°.

### PG05 — Knife

- ATTACCO BASE: AREA HITSCAN istantanea, senza PROIETTILI FISICI;  AREA a CONO orientata verso il CURSORE, portata 2 m / 135°; ATK 30 per ogni MOB valido nell’AREA, senza suddivisione; ATK SPD 150 = 1,5 ATTACCHI/s. MURI/OSTACOLI bloccano il DANNO verso i MOB schermati.

#### ABILITÀ 1 — INVISIBILITÀ

- Rende INVISIBILE l’intera PARTY per 3,5 s e aumenta MOVE SPD del 15% durante l’effetto; CD BASE 15 s.
- Applica le REGOLE GENERALI dello stato di INVISIBILITÀ (sezione 10.5), inclusa l’eccezione RESUSCITARE e l’interruzione individuale.

#### ABILITÀ 2 — COLTELLI AVVELENATI

- Lancia simultaneamente 8 COLTELLI a 360° attorno a PG05, distanziati di 45°. Uno segue esattamente il CURSORE; gli altri sono orientati rispetto a quello.
- PROIETTILI BALISTICI non PERFORANTI; RANGE 10 m; 30 DANNO per COLTELLO; CD BASE 10 s.
- Seguono le REGOLE GENERALI DEI PROIETTILI: il primo MOB colpito riceve DANNO e VELENO e arresta il COLTELLO; MURI/OSTACOLI bloccano il PROIETTILE.
- VELENO: 5 HP/s per 3 s per applicazione. Le applicazioni si sommano e ciascuna mantiene la propria DURATA indipendente; una nuova applicazione non rinnova le precedenti.

#### PASSIVA 1 — GHOSTING

- Con HP <30%, una HIT ricevuta attiva INVISIBILITÀ e +15% MOVE SPD per 2 s. Con HP ≥30% non si attiva.
- Applica le stesse REGOLE GENERALI di INVISIBILITÀ.
- HIT ricevute durante l’effetto non lo riattivano e non rinnovano la DURATA.
- Terminato l’effetto, la PASSIVA è subito disponibile per una nuova HIT che soddisfi le condizioni; nessun CD interno.

#### PASSIVA 2 — LAMA DI CICUTA

- Conta i MOB COLPITI dagli ATTACCHI BASE: +1 per ciascun MOB colpito, anche più incrementi nello stesso ATTACCO.
- Soglia 15; il contatore si ferma a 15 e l’eccedenza viene ignorata.
- Raggiunta la soglia, il successivo ATTACCO BASE applica VELENO 5 HP/s per 3 s a tutti i MOB colpiti nell’AREA ATTACCO.
- Dopo l’ATTACCO potenziato il contatore torna a 0 e inizia un nuovo ciclo.

### PG06 — Revolver

- ATTACCO BASE: PROIETTILE FISICO non PERFORANTE, senza proprietà speciali; ATK 40; ATK SPD 80 = 0,8 ATTACCHI/s; RANGE 10 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — CURA AD AREA

- Cura 40 HP ai PG entro RAGGIO 5 m, con centro PG06; PG06 è incluso tra i bersagli. CD BASE 20 s.
- MURI/OSTACOLI non bloccano la CURA.
- NO OVERHEAL. I PG al 100% HP vengono considerati bersagli, ma recuperano 0 HP.

#### ABILITÀ 2 — FUOCO CURATIVO

- Potenzia i successivi 6 ATTACCHI BASE conservando il normale funzionamento e DANNO del REVOLVER.
- Ogni HIT su un MOB genera in aggiunta una CURA di 15 HP verso un PG entro 15 m da PG06; PG06 è incluso. Un MISS non genera CURA.
- Ogni ATTACCO effettuato consuma una carica, anche in caso di MISS. Nessuna DURATA massima; CD BASE 15 s, avviato al consumo del 6° ATTACCO.
- La selezione del destinatario viene rivalutata per ciascun ATTACCO: percentuale HP più bassa → minor numero di HP effettivi → distanza minore da PG06 → scelta CASUALE in caso di ulteriore parità.
- MURI/OSTACOLI non bloccano la CURA. NO OVERHEAL; un PG al 100% resta selezionabile ma recupera 0 HP. Se tutti i PG validi sono al 100%, la carica viene comunque consumata e nessuno recupera HP.
- Al CAMBIO AREA perde le cariche residue; se ancora ATTIVA, il CD riparte da capo. Se DISPONIBILE, rimane DISPONIBILE.

#### PASSIVA 1 — ELEMOSINA

- Ogni KILL attribuita esclusivamente a PG06 effettua un roll del 3%; un successo genera 1 MEDI KIT fisico sul terreno.
- Qualsiasi PG della PARTY può raccoglierlo passandoci sopra. Viene usato immediatamente e non occupa SLOT ITEM.
- CURA base: 10% degli HP MASSIMI del PG che lo raccoglie; NO OVERHEAL. A HP pieni il TRIGGER ignora il PG e il MEDI KIT non viene consumato; valgono validità e priorità della sezione 26.
- DURATA infinita nell’AREA: rimane fino alla RACCOLTA oppure scompare al CAMBIO AREA.

#### PASSIVA 2 — VITAMINA C

- Un PG considerato bersaglio di una CURA delle ABILITÀ di PG06 riceve +25% del proprio ATK SPD BASE per 4 s, anche se a HP pieni e con CURA effettiva pari a 0.
- CURA AD AREA: tutti i PG considerati entro 5 m, incluso PG06. FUOCO CURATIVO: solo il destinatario selezionato dopo una HIT valida; un MISS non applica VITAMINA C.
- Il BONUS non si cumula; una nuova applicazione rinnova la DURATA a 4 s. Ogni PG gestisce individualmente la propria DURATA.
- Il MEDI KIT di ELEMOSINA non attiva VITAMINA C. Al CAMBIO AREA gli effetti VITAMINA C ancora attivi terminano.

### PG07 — Bow

- ATTACCO BASE: FRECCIA BALISTICA non PERFORANTE; ATK 30; ATK SPD 110 = 1,1 ATTACCHI/s; RANGE 15 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — MULTI SHOT

- 9 PROIETTILI BALISTICI non PERFORANTI generati simultaneamente; 20 DANNO per PROIETTILE; RANGE 7 m; CONO 80°; RESPINTA 5 m; CD BASE 10 s.
- Un PROIETTILE segue il CURSORE; gli altri sono distribuiti ogni 10°: −40°, −30°, −20°, −10°, 0°, +10°, +20°, +30°, +40°.
- Lo stesso MOB può ricevere più HIT: i DANNI della stessa attivazione vengono sommati e la DEF viene applicata una sola volta al DANNO TOTALE.
- Massimo una RESPINTA per MOB per attivazione. La direzione è quella del primo PROIETTILE che genera HIT; in caso di HIT simultanee si usa la media delle direzioni dei PROIETTILI coinvolti.
- MURI/OSTACOLI bloccano i PROIETTILI e interrompono la RESPINTA prima dei 5 m quando incontrati.

#### ABILITÀ 2 — PIOGGIA DI FRECCE

- AREA BERSAGLIO CIRCOLARE, RAGGIO 6 m; centro sul CURSORE all’attivazione; RANGE di attivazione illimitato.
- 20 FRECCE distribuite in posizioni CASUALI nell’AREA durante 3 s. Il tempo è regolare: prima a T = 0, ultima a T = 3 s, intervallo 3/19 s (circa 0,158 s).
- Ogni FRECCIA è un singolo PROIETTILE, non un’esplosione: genera al massimo 1 HIT su 1 MOB, con 10 HP di DANNO prima della DEF.
- Verifica esclusivamente il PUNTO D’IMPATTO: nessun MOB → MISS; più MOB sovrapposti → scelta CASUALE di un solo MOB.
- Lo stesso MOB può ricevere più FRECCE; la DEF viene applicata separatamente a ciascuna HIT.
- MURI/OSTACOLI non intercettano la caduta e non tagliano l’AREA.
- CD BASE 15 s, avviato all’attivazione. Al CAMBIO AREA termina immediatamente, annulla le FRECCE non ancora generate e applica la regola generale del CD.

#### PASSIVA 1 — LUCKY SHOT

- Roll del 2% solo quando un ATTACCO BASE genera una HIT effettiva su un MOB; nessun roll in caso di MISS.
- Un successo genera una HIT AD AREA aggiuntiva pari al 30% dell’ATK di PG07; AREA CIRCOLARE di RAGGIO 3 m centrata sul MOB che ha generato il TRIGGER.
- Il MOB originale è incluso. Il DANNO AD AREA è una HIT separata: la DEF si applica indipendentemente dall’ATTACCO BASE e individualmente a ogni MOB.
- Se l’ATTACCO BASE uccide il MOB, LUCKY SHOT può comunque attivarsi nel punto della sua MORTE e colpire gli altri MOB.
- MURI/OSTACOLI eliminano interamente gli SPICCHI intercettati secondo la geometria a 4 SPICCHI da 90°. Gli SPICCHI non generano HIT separate.
- La HIT di LUCKY SHOT non è un ATTACCO BASE e non genera ulteriori roll di LUCKY SHOT.

#### PASSIVA 2 — CONCENTRAZIONE

- Roll del 5% al lancio di ogni ATTACCO BASE, indipendentemente dalla futura HIT.
- Un successo rende la FRECCIA PERFORANTE: massimo 3 MOB sulla stessa traiettoria. Il primo MOB effettivamente colpito riceve il 100% del DANNO; il secondo e il terzo il 50%, senza ulteriori riduzioni.
- La DEF viene applicata separatamente a ciascuna HIT. La MORTE del primo MOB non interrompe la perforazione.
- La FRECCIA segue le REGOLE GENERALI DEI PROIETTILI: MURI/OSTACOLI la bloccano e il RANGE resta quello dell’ATTACCO BASE.
- CONCENTRAZIONE e LUCKY SHOT non possono essere attive insieme: ogni PG ha una sola PASSIVA selezionata per RUN.

### PG08 — Heavy Machine Gun

- ATTACCO BASE: PROIETTILE FISICO non PERFORANTE; ATK 7; ATK SPD 200 = 2 ATTACCHI/s; RANGE 8 m. Segue le REGOLE GENERALI DEI PROIETTILI.

#### ABILITÀ 1 — FILO SPINATO

- ANELLO di RAGGIO 7 m e SPESSORE 1 m, centrato sulla posizione di PG08 all’attivazione. Rimane fisso per 3 s e non segue PG08.
- Solo la fascia dell’ANELLO genera effetti: 10 HP/s secondo la REGOLA GENERALE DEI DANNI PERIODICI richiamata nella conversazione e SLOW 25% mentre il MOB è nella fascia. All’uscita DANNO e SLOW cessano.
- Un MOB già nella fascia di una SEZIONE valida all’attivazione riceve immediatamente gli effetti.
- Non è una barriera fisica: i MOB attraversano liberamente; i PG attraversano senza effetti; i PROIETTILI attraversano normalmente.
- 10 SEZIONI indipendenti da 36°. Alla generazione, se anche una sola parte di una SEZIONE interseca un MURO/OSTACOLO, l’intera SEZIONE viene eliminata.
- Nessun minimo di SEZIONI valide. Con 0/10 non viene generato FILO SPINATO, ma l’ABILITÀ è comunque UTILIZZATA.
- CD BASE 16 s, avviato all’attivazione anche con 0/10 SEZIONI.
- Al CAMBIO AREA tutte le SEZIONI scompaiono; DANNO e SLOW terminano e il CD segue la regola generale.

#### ABILITÀ 2 — COLPI RESPINGENTI

- Per 3 s ogni HIT di ATTACCO BASE applica il normale DANNO e RESPINTA di 1,5 m nella direzione del PROIETTILE che genera quella HIT. Nessuna HIT → nessuna RESPINTA.
- Ogni nuova HIT applica una nuova RESPINTA completa e indipendente, anche allo stesso MOB; MURI/OSTACOLI interrompono lo spostamento.
- CD BASE 10 s, avviato al termine dei 3 s di DURATA.
- Al CAMBIO AREA l’effetto termina e il CD segue la regola generale.

#### PASSIVA 1 — TENACIA

- Ogni HIT effettiva generata da PG08 fornisce +0,2% DEF; BONUS massimo +30% (150 HIT). Ulteriori HIT al CAP non aumentano il BONUS.
- Una HIT ricevuta da PG08 azzera immediatamente tutto il BONUS TENACIA. Le successive HIT generate possono ricominciare l’accumulo; a BONUS 0% non c’è nulla da perdere.
- Al CAMBIO AREA il BONUS torna a 0%.
- La DEF BASE +10% rimane separata e non viene persa; al CAP, BASE + TENACIA = +40% prima di altri modificatori.

#### PASSIVA 2 — RAGE

- Dopo 5 s consecutivi di FUOCO CONTINUATIVO si attivano immediatamente +30% ATK e −20% MOVE SPD.
- Non è necessario generare HIT: si può sparare a vuoto. Gli intervalli ordinari fra colpi dovuti ad ATK SPD fanno parte del FUOCO CONTINUATIVO; ATK SPD non modifica la soglia temporale di 5 s.
- RAGE dura finché il fuoco continua. Rilasciare il comando di ATTACCO azzera il contatore e, se attiva, termina immediatamente RAGE.
- I modificatori si aggiungono a quelli già presenti. Alla fine di RAGE vengono rimossi esclusivamente i suoi modificatori.
- DOWN e CAMBIO AREA terminano RAGE e azzerano il contatore, anche se al CAMBIO AREA il comando di ATTACCO è mantenuto.

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
- MOB ↔ MOB: nessuna collisione fisica; possono attraversarsi e sovrapporsi. Gli altri MOB non ostacolano il percorso.
- MOB ↔ PG: collisione fisica; si bloccano fisicamente e non si attraversano. La semplice collisione non infligge DANNO e non applica RESPINTA al PG.
- MOB ↔ MURI/OSTACOLI: collisione fisica attiva, con blocco del MOVIMENTO.
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
- PROJECTILE SPD standard 20 m/s è confermata per gli ATTACCHI BASE fisici dei PG. L’applicazione esplicita a ZOMB04 resta DA DEFINIRE; non vengono trasferite automaticamente le collisioni dei proiettili alleati.

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
- Mantiene la COLLISIONE fisica con i PG e ne blocca il MOVIMENTO fino all’ESPLOSIONE. Gli altri MOB possono continuare ad attraversarlo e sovrapporsi.

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

<a id="sezione-14"></a>

## 14. MOB PER AREA

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
- Il G è COMUNE ai PLAYER validi: se un MOB droppa 10 G, ciascuno riceve 10 G; il valore non viene diviso.
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

- Esempio: MOB con valore 20 EXP + 20 G, 4 PG validi e 2 PLAYER validi → 20 EXP a ciascun PG e 20 G a ciascun PLAYER.

<a id="sezione-17"></a>

## 17. SISTEMA DI PROGRESSIONE EXP / LVL

- EXP necessaria per LVL 1 → 2: 100.
- Ogni livello successivo richiede il 15% di EXP in più rispetto al precedente.
- Il valore viene arrotondato alla decina secondo la regola già adottata nel progetto.
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

- Massimo 1 CHEST per AREA.
- Probabilità base di comparsa CHEST: 5%.
- La CHEST assegna un BONUS a ogni PG della PARTY non in MORTE; ogni PG riceve il proprio BONUS casualmente e separatamente.
- I BONUS CHEST riguardano esclusivamente le STATS BASE.
- STATS possibili: HP, ATK, DEF, MOVE SPD, ATK SPD, CD REDUCTION. RANGE escluso.
- Ogni BONUS CHEST modifica la STAT del 5% del VALORE BASE; il BONUS CD REDUCTION riduce la STAT CD REDUCTION del 5% del suo VALORE BASE. Con base 100 equivale a −5 punti: per esempio 100 → 95. La riduzione si applica alla STAT, non direttamente ai secondi del CD BASE delle ABILITÀ.

| BONUS STAT CHEST | RATE |
| --- | --- |
| HP | 25% |
| ATK | 8% |
| DEF | 22% |
| MOVE SPD | 22% |
| ATK SPD | 8% |
| CD REDUCTION | 15% |

### GENERAZIONE, RACCOLTA E BONUS HP

- Durante il CARICAMENTO di ogni NUOVA AREA si verifica il CHEST RATE della RUN (base 5%). Un esito positivo genera la CHEST: massimo 1 per AREA.
- Posizione CASUALE ad almeno 50 m dalla ZONA DI INIZIO; se non valida, ricalcolare finché viene trovata una posizione valida.
- Un solo PG nel TRIGGER apre automaticamente la CHEST, anche in combattimento; non serve F. Forma e dimensioni del TRIGGER saranno definite in sviluppo/testing.
- BONUS immediato a tutti i PG ATTIVI e in DOWN, indipendentemente dalla posizione nell’AREA; i PG in MORTE sono esclusi. Sorteggio individuale con i RATE della tabella.
- Ogni PLAYER riceve un AVVISO A SCHERMO relativo al BONUS del proprio PG: visibile, comprensibile e non invasivo. Nessuna pausa del GAMEPLAY.
- Dopo l’assegnazione la CHEST sparisce e non può essere riutilizzata.
- BONUS HP: l’aumento degli HP MASSIMI viene aggiunto nella stessa quantità agli HP correnti. Con base 100: 60/100 → 65/105 → 70/110; a HP pieni 100/100 → 105/105. L’incremento resta il 5% del VALORE BASE originale, non del valore già potenziato.

<a id="sezione-20"></a>

## 20. LEVEL UP

- Raggiunta una soglia EXP, il GAMEPLAY va automaticamente e completamente in PAUSA. Durante la pausa non avvengono combattimenti, KILL o nuova assegnazione EXP.
- Per ogni PG interessato si generano sempre 3 BANNER; il PLAYER ne sceglie 1 e il BONUS è applicato immediatamente al relativo PG.
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
- La composizione dei 5 BONUS è fissa: 3 BONUS STATS BASE + 2 BONUS relativi alle ABILITÀ BONUS.
- Il completamento AREA può quindi comprendere entrambe le categorie già definite.
- Dopo le scelte e la conferma di tutti i PLAYER, la PARTY passa alla nuova AREA.

<a id="sezione-22"></a>

## 22. ABILITÀ BONUS — STATS

| # | ABILITÀ BONUS | STATS BASE |
| --- | --- | --- |
| 1 | LANCIO COLTELLI | DANNO 10; RANGE 10 m; CD 6 s; N° COLTELLI 1. |
| 2 | PET | DANNO 10; ATK SPD 50; RANGE 2 m; ricerca MOB entro 5 m; N° PET 1; sempre attivo. |
| 3 | AURA TOSSICA | DANNO 10; RAGGIO 4 m; CD 7 s. |
| 4 | RICOCHET | DANNO rimbalzo 40% del danno originale; RANGE rimbalzo 5 m; N° RIMBALZI 1. |
| 5 | MINE | DANNO 10; RAGGIO 3 m; CD 7 s; esplode se calpestata o dopo 5 s. |
| 6 | SCUDO | RIDUZIONE DANNO 40%; dura fino alla prima HIT; CD 6 s dopo la scomparsa. |
| 7 | FIRE BULLET | ATK +15%; durata 3 s; BRUCIATURA 2 HP/s per 2 s; CD 12 s. |
| 8 | TASER | DANNO 5; RAGGIO 4 m; BLOCK 2 s (STUN); CD 12 s. |
| 9 | REPULSE | DANNO 5; RAGGIO 4 m; RESPINTA 3 m; CD 10 s. |
| 10 | SCIABOLATA | DANNO 15; semicerchio frontale; RANGE 4 m; CD 15 s. |

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
| SCUDO | RIDUZIONE DANNO | +5 punti percentuali |
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
| SCUDO | RIDUZIONE DANNO 50% · CD 50% |
| FIRE BULLET | DANNO 40% · DANNO BRUCIATURA 40% · CD 20% |
| TASER | DANNO 50% · RAGGIO 30% · TEMPO BLOCK 5% · CD 15% |
| REPULSE | DANNO 50% · RAGGIO 30% · RESPINTA 5% · CD 15% |
| SCIABOLATA | DANNO 40% · RANGE 40% · CD 20% |

<a id="sezione-26"></a>

## 26. MEDI KIT

### GENERAZIONE NELL’AREA

- Durante il CARICAMENTO della NUOVA AREA viene sorteggiata la quantità: 1, 2 oppure 3 MEDI KIT, con probabilità esatta **1/3 ciascuno**.
- È sempre presente almeno 1 MEDI KIT; massimo 3 generati da questo sistema. Questa regola sostituisce il precedente 30% per punto di spawn.
- Ogni MEDI KIT viene generato in posizione CASUALE ad almeno 50 m dalla ZONA DI INIZIO. Ricalcolare ogni posizione finché è valida, come per CHEST.
- Il DROP di PG06 / ELEMOSINA resta una fonte aggiuntiva, con roll del 3% per KILL attribuita a PG06; non è incluso nel sorteggio iniziale 1–3.

### TRIGGER, CURA E PRIORITÀ

- Raccolta automatica entrando nel TRIGGER; non serve F, nessuna pausa. Forma e dimensioni del TRIGGER da definire in sviluppo/testing.
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
- HP / ATK / DEF / MOVE SPD / ATK SPD: ogni acquisto aggiunge 1% del VALORE BASE ORIGINALE.
- CD REDUCTION: ogni acquisto riduce la STAT CD REDUCTION dell'1% del suo VALORE BASE ORIGINALE. Con base 100 equivale a −1 punto per acquisto: 100 → 99 → 98. La riduzione è sempre calcolata sul VALORE BASE ORIGINALE, non sul valore già modificato, e non modifica il CD BASE in secondi delle ABILITÀ.
- Costi iniziali definiti: HP 50 G; ATK 50 G; DEF 50 G; MOVE SPD 50 G; ATK SPD 100 G; CD REDUCTION 100 G.
- Dopo ogni acquisto dello stesso UPGRADE, il costo aumenta del 10% con arrotondamento per difetto.
- CHEST RATE: costo iniziale 500 G; probabilità base CHEST 5%; incremento precedentemente definito come +1% del BASE per UPGRADE.
- G DROP: costo iniziale 500 G; ogni UPGRADE aumenta del 10% il VALORE BASE del G DROP.
- ITEM SLOT: 3 UPGRADE massimi; costi 1000 G → 1100 G → 1210 G; SLOT da 1 a massimo 4.
- MEDI KIT: costo UPGRADE iniziale 250 G; ogni UPGRADE aggiunge +2 punti percentuali alla cura (10% → 12% → 14% ...); i costi successivi seguono +10% con arrotondamento per difetto.

<a id="sezione-28"></a>

## 28. DOWN / MORTE / RESURREZIONE

- A 0 HP un PG entra in stato DOWN.
- In DOWN il PG non può agire e i MOB non lo considerano un bersaglio.
- Durata DOWN: 20 s.
- Un altro PG può resuscitarlo tenendo premuto F per 5 s.
- Se F viene rilasciato prima del completamento, il progresso della resurrezione torna indietro.
- Il PG resuscitato torna con il 50% degli HP MASSIMI correnti, inclusi eventuali BONUS temporanei.
- Dopo la resurrezione ottiene 2 s di invulnerabilità.
- Se tutti i PG sono contemporaneamente in DOWN, la PARTY è SCONFITTA e la RUN termina.
- I PG in DOWN devono essere RESUSCITATI prima del passaggio AREA. I PG in MORTE vengono RESUSCITATI automaticamente nella nuova AREA al 50% HP MASSIMI, senza cura aggiuntiva del 15% (sezione 6).

- DROP G/EXP: un PG in DOWN riceve EXP, un PG in MORTE non la riceve. Il PLAYER riceve G finché possiede almeno un PG non in MORTE (sezione 16).

<a id="sezione-29"></a>

## 29. VITTORIA / SCONFITTA / FINE RUN

### 29.1 Vittoria e ritorno volontario

- Dopo la vittoria contro il BOSS della CITTÀ il PLAYER può PROSEGUIRE oppure TORNARE ALL'HUB.
- Se PROSEGUE: EXP, LVL, BONUS temporanei, ITEMS e G restano invariati e la RUN continua.
- Se TORNA VOLONTARIAMENTE ALL'HUB: EXP, LVL e BONUS temporanei vengono persi; G e ITEMS inutilizzati vengono mantenuti.

### 29.2 Sconfitta

- La sconfitta avviene quando tutti i PG sono contemporaneamente in DOWN.
- La RUN termina e si torna all'HUB.
- EXP, LVL della RUN, BONUS temporanei e ITEMS inutilizzati vengono persi.
- Nuova regola: in caso di sconfitta viene perso il 50% del G ottenuto durante quella RUN; il restante 50% viene mantenuto.
- La gestione esatta dell'arrotondamento del 50% G, in caso di valore dispari, è DA DEFINIRE.

<a id="sezione-30"></a>

## 30. STATO DI SVILUPPO — PUNTI RIMANENTI

**DA DEFINIRE / DA SVILUPPARE:** tutti i punti seguenti restano aperti secondo la fonte.

- Definire BOSS di ogni CITTÀ, relative STATS, fasi e meccaniche.
- Definire MINI BOSS, EVENTI SPECIALI e livelli/quest bonus.
- Definire nomi propri, mappe, layout e identità visiva dettagliata delle CITTÀ e delle AREE.
- Definire eventuali ulteriori MOB/varianti oltre ZOMB01–ZOMB05; la progettazione dei MOB è per ora conclusa con ZOMB05.
- Definire il comportamento AI dei PG in SINGLE PLAYER e le specifiche dei futuri MOB. AGGRO, MOVIMENTO/COLLISIONI, HIT/DANNO/STUN, MORTE/DROP e le schede ZOMB01–ZOMB05 sono consolidati nella sezione 13.
- SISTEMA DI SPAWN e distribuzione ZOMB01–ZOMB05 consolidati nelle sezioni 14–15. BALISTICA dei PG consolidata nella sezione 10.1; resta da esplicitare l’applicazione dello standard PROJECTILE SPD ai proiettili dei MOB e alle ABILITÀ non precisate.
- Definire UI/HUD: HP, abilità, PASSIVA, SLOT BONUS, SLOT ITEM, EXP/LVL, G, indicatori DOWN e schermate di scelta.
- PREPARAZIONE RUN e logica BANNER LEVEL UP consolidate nelle sezioni 8 e 20; realizzare la grafica definitiva e completare le schermate BONUS dove non descritte.
- Definire le regole di stacking ancora non specificate. Sono già confermati lo stacking con DURATE indipendenti del VELENO di COLTELLI AVVELENATI e il DANNO additivo delle AREE di PYROMANIA; non estenderli automaticamente ad altri effetti.
- Definire i valori/limiti finali degli UPGRADE permanenti del PROF dove non ancora fissati.
- Definire l'arrotondamento del G perso/mantenuto in caso di sconfitta con importi dispari.
- Bilanciamento complessivo di EXP/LVL, G, MOB per AREA, danni, cure, cooldown e probabilità.
- Direzione Pixel Art, animazioni, VFX, SFX e musica.
- Prototipo Unity del movimento, mira, attacco, abilità, spawn, BONUS, HUB e loop RUN.
- Architettura tecnica multiplayer CO-OP fino a 4 giocatori, sincronizzazione e gestione host/client.
- Testing, performance, salvataggio/progressione permanente e build finale.

<a id="sezione-31"></a>

## 31. REGOLE CONSOLIDATE DA NON DIMENTICARE

- Nome ufficiale progetto: ROG ZOMBIE.
- CO-OP fino a 4 giocatori.
- PARTY sempre composta da 4 PG.
- Tasto INTERAZIONE: F.
- Tasto ABILITÀ: Q.
- Ogni PG: 2 ABILITÀ ma 1 sola selezionata per RUN; 2 PASSIVE ma 1 sola selezionata per RUN.
- CD REDUCTION: STAT modificatore con VALORE BASE neutro 100 per tutti i PG01–PG08; un valore più basso riduce il cooldown.
- I CD BASE in secondi sono definiti esclusivamente nelle singole ABILITÀ. Formula: CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100). Esempio: CD BASE 10 s e CD REDUCTION 90 → CD FINALE 9 s.
- BONUS CHEST CD REDUCTION: riduce la STAT del 5% del VALORE BASE; con base 100 equivale a 100 → 95, non a una riduzione diretta in secondi.
- Ogni UPGRADE PROF CD REDUCTION riduce la STAT dell'1% del VALORE BASE ORIGINALE; con base 100 equivale a −1 punto per acquisto.
- Eventuali cap/minimi e arrotondamenti di CD REDUCTION e del CD FINALE restano DA DEFINIRE.
- Ogni PG: 3 SLOT BONUS.
- SLOT ITEM: 1 iniziale, 3 UPGRADE massimi, 4 SLOT massimi.
- Ingresso nuova AREA: PG VIVI +15% HP MASSIMI; PG in MORTE resuscitati al 50%, senza +15%; DOWN da resuscitare prima del passaggio.
- Sconfitta PARTY: perdita del 50% del G ottenuto nella RUN.

- RICALCOLO AGGRO: 0,5 s scaglionato; immediato se il BERSAGLIO entra in DOWN.
- HIT/DANNO non altera il comportamento del MOB di per sé; STUN interrompe ogni AZIONE e richiede una ripartenza da capo.
- MORTE MOB: DROP G/EXP automatico immediato; SPRITE DI MORTE per 3 s senza COLLISIONI con PG/MOB.
- EXP integrale a ogni PG non in MORTE, inclusi DOWN; G integrali a ogni PLAYER con almeno un PG non in MORTE. Nessuna suddivisione.
- AREE CIRCOLARI: PG04 e LUCKY SHOT 4 × 90°; ZOMB03 e ESPLOSIONE ZOMB05 8 × 45°; SPICCHIO intercettato eliminato integralmente, senza HIT separate.
- ZOMB03: punto d’impatto fissato sulla posizione del BERSAGLIO a T = 0; AREA generata a T = 0,5 s; nuovo ATTACCO disponibile a T = 2 s.

- ZOMB04: ATTACCO da fermo con PROIETTILE BALISTICO; posizione iniziale 15 m; arretramento sotto 9 m; ripresa dell’inseguimento oltre 30 m; VELOCITÀ PROIETTILE DA DEFINIRE.
- ZOMB05: 0 HP → PRE-ESPLOSIONE 2 s → ESPLOSIONE → MORTE → DROP e trigger SPAWN; 50 DANNO ai PG ATTIVI / 25 ai MOB prima della DEF; RAGGIO 4 m.
- SPAWN: OFF-SCREEN da tutti i PG ATTIVI, NAVMESH valida e percorso verso almeno un PG ATTIVO; ZOMB05 escluso dal FIRST SPAWN con quota redistribuita 50/50 a ZOMB01/ZOMB02.

<a id="sezione-32"></a>

## 32. Chiarimenti necessari — DA DEFINIRE

Le voci seguenti sono note editoriali di verifica. Evidenziano ciò che la fonte non determina in modo sufficiente per l’implementazione; non modificano le specifiche delle sezioni precedenti.

| Ambito | Dati conservati | DA DEFINIRE |
| --- | --- | --- |
| Lore e vittoria finale | Esperimento scientifico; scienziato come possibile BOSS finale, non definitivo. | Identità e ruolo definitivo dello scienziato, cura, conclusione narrativa e comportamento dopo il BOSS della CITTÀ 5. |
| Città e contenuti opzionali | 5 CITTÀ, 23 AREE; percorso principale lineare; possibili quest sotterranee. | Nomi, mappe, accesso e rientro dalle quest bonus, loro rapporto con il conteggio delle AREE e l’aumento dell’EXP DROP. |
| PARTY e sblocchi | 4 PG, roster di 8; PG selezionati BLOCKED; non sbloccati in silhouette. | PG inizialmente disponibili e condizioni di sblocco; dettagli di equipaggiamento/progressione degli IA non esplicitati. Distribuzione IA e responsabilità di selezione/conferma sono definite nella sezione 8. |
| HUB e NPC | MERCHANT, PROF, EXIT; possibili NPC liberati nelle quest. | Identità, dialoghi, condizioni di sblocco e servizi degli altri NPC. |
| Attacchi base e ITEMS | Armi, ATK, RANGE, ATTACCHI ad AREA e PROIETTILI definiti nelle sezioni 10–12. | Uso e tasti degli ITEMS, bersagli degli effetti ancora non esplicitati e friendly fire degli effetti non coperti dalle regole specifiche. I PROIETTILI FISICI dei PG attraversano gli alleati senza effetti (sezione 10.1). Gli ATTACCHI BASE PG01–PG08, il raggio delle esplosioni PG04 e le eccezioni sono ora descritti nelle sezioni 10–12. |
| DEF e modificatori | DEF interno 100 = modificatore mostrato 0%; danno = ATK × (1 − DEF%). | Conversione completa fra DEF interna e percentuale mostrata, combinazione di bonus DEF e riduzioni come SCUDO, limiti e arrotondamento del danno. |
| CD REDUCTION — regola consolidata | STAT modificatore del PG con VALORE BASE neutro 100; 100 = 100% del CD BASE ABILITÀ. CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100). Ogni ABILITÀ mantiene il proprio CD BASE in secondi. CHEST riduce la STAT di 5 punti con base 100; ogni UPGRADE PROF la riduce di 1 punto, calcolato sul VALORE BASE ORIGINALE. | Eventuali cap/limiti minimi o finali della STAT CD REDUCTION e regole di arrotondamento della STAT e del CD FINALE. |
| Avvio del cooldown | CD BASE delle ABILITÀ conservati nelle schede PG. | Avvio del CD per le ABILITÀ non ancora precisate. COLPO GROSSO e FUOCO CURATIVO lo avviano al consumo dell’ultimo ATTACCO; COLPI RESPINGENTI al termine della DURATA; FILO SPINATO e PIOGGIA DI FRECCE all’attivazione. |
| Effetti e passive | Valori e condizioni conservati integralmente nelle schede PG. | Stacking e refresh non ancora esplicitati per ciascun effetto, criterio generale di attribuzione delle KILL e base dell’1% HP curato dalla PASSIVA 1 di PG02. FUOCO CURATIVO e SCORTA ESPLOSIVA sono definiti nella sezione 12. |
| ABILITÀ BONUS | 10 abilità con STATS, RATE e UPGRADE. | Attivazione, mira e comportamento non esplicitati, frequenza del danno di AURA TOSSICA e interpretazione dell’UPGRADE DANNO di FIRE BULLET, che riporta ATK +15% anziché un danno base autonomo. |
| MOB, BOSS e spawn | Totali per AREA, distribuzione ZOMB01–ZOMB05, FIRST SPAWN 30%, arrotondamenti a discapito di ZOMB01, quantità residue, probabilità e OFF-SCREEN globale consolidati nelle sezioni 14–15. | Inclusione di BOSS/MINI BOSS nei totali e nel limite simultaneo. |
| EXP per livello | +15% per livello e tabella LVL 1 → 11. | Regola esatta di arrotondamento «alla decina»: la fonte rinvia a una regola precedente senza esplicitarla e la tabella non è riproducibile con un unico arrotondamento semplice applicato ricorsivamente. Non sostituire i valori tabellari; definire il calcolo oltre le voci presenti, il livello massimo. L’EXP residua e i MULTI LVL UP sono definiti nella sezione 20. |
| BONUS di fine AREA | 3 proposte STATS BASE + 2 proposte ABILITÀ BONUS; scelta di 1 su 5 per PLAYER. | Valori e RATE delle proposte STATS di fine AREA, non esplicitamente equiparati a quelli CHEST; destinatari e gestione delle scelte per i PG AI. |
| CHEST e HP | BONUS CHEST del 5% del valore base; cura all’ingresso AREA del 15% HP massimi correnti. | Rapporto fra valore base, UPGRADE permanenti e BONUS temporanei dove non specificato. Per CHEST è definito: incremento HP MASSIMI aggiunto anche agli HP correnti (sezione 19). |
| G e acquisti | Ogni PLAYER valido riceve l’intero G DROP; validità definita nella sezione 16; il G è detto COMUNE. | Saldo condiviso spendibile o saldi individuali accreditati in parallelo, titolarità degli acquisti/UPGRADE e applicazione dei potenziamenti G DROP in CO-OP. |
| PROF | Incrementi, prezzi iniziali e ITEM SLOT massimo 4 definiti. | Cap e ambito di applicazione degli UPGRADE permanenti; conferma dell’incremento CHEST RATE, riportato come «precedentemente definito» a +1% del BASE. Non convertirlo automaticamente in +1 punto percentuale. |
| DOWN e morte | DOWN di 20 s; resurrezione con F per 5 s; ritorno al 50% HP massimi e 2 s di invulnerabilità. | Stato e possibilità di recupero alla scadenza dei 20 s, sconfitta con combinazioni di PG morti e DOWN, velocità/modalità del regresso se F viene rilasciato, distanza di interazione, interruzioni e comportamento dell’AI. |
| Passaggio AREA | Resurrezione automatica dei PG in MORTE al 50%; cura di ingresso del 15% ai VIVI; DOWN da resuscitare prima. | Solo forma e dimensioni del TRIGGER, demandate a sviluppo/testing. DOWN da resuscitare prima; MORTE al 50% senza +15%; VIVI +15%: sezione 6. |
| Fine RUN | Ritorno volontario dopo BOSS e sconfitta hanno effetti distinti; si perde il 50% del solo G guadagnato nella RUN in caso di sconfitta. | Arrotondamento del G dispari, decisione di proseguire/tornare in CO-OP, abbandono/disconnessione e relativo trattamento della RUN. |
| DANNI PERIODICI | FILO SPINATO richiama esplicitamente la REGOLA GENERALE DEI DANNI PERIODICI; 10 HP/s e applicazione immediata ai MOB già nella fascia sono confermati. | Il testo completo della regola generale non è presente nelle fonti recuperate: frequenza e calcolo dei tick non vengono ricostruiti. |
| CD al CAMBIO AREA | ABILITÀ DISPONIBILE resta disponibile; ABILITÀ ATTIVA termina e il suo CD riparte. | Il caso di un’ABILITÀ già IN CD ma non più ATTIVA non è esplicitato nella formulazione finale confermata. |
| TAG degli elementi | Proposta di distinguere gli elementi mediante TAG. | DA CONFERMARE: uso dei TAG, elenco e soluzione tecnica non sono stati approvati. |
| Dettagli tecnici delle AREE | Numero/ampiezza SPICCHI, eliminazione completa e assenza di HIT separate sono confermati. | Orientamento iniziale degli SPICCHI e modalità tecnica delle verifiche geometriche non sono specificati. |

I punti di bilanciamento, produzione artistica, prototipazione, multiplayer, salvataggio, testing e build restano quelli elencati nella sezione 30. Nessun valore aggiuntivo è stabilito da queste note.

### NOTE DI CONSOLIDAMENTO — 10/09/2026

- Integrate anche le decisioni successive alla base su STATS MOB, C5, transizione AREA e LEVEL UP, oltre a CHEST, MEDI KIT, PREPARAZIONE RUN e BALISTICA.
- Il nuovo HITSCAN di PG04 sostituisce il precedente volo parabolico dell’ATTACCO BASE; restano le regole specifiche di destinazione e AREA già definite. Non sono introdotti nuovi criteri di intercettazione del raggio.
- Restano non specificati: gestione tecnica dello SPAWN del proiettile dentro un ostacolo o con CURSORE coincidente con la bocca dell’arma; dettagli di rilascio drag & drop fuori dagli SLOT e trasferimenti fra PG; gestione dell’HOST uscente. Non vengono assegnati comportamenti impliciti.
- La conferma finale di validità MEDI KIT aggiorna il vecchio consumo a HP pieni nel richiamo a ELEMOSINA. Non modifica roll del DROP, durata nell’AREA o esclusione da VITAMINA C.
