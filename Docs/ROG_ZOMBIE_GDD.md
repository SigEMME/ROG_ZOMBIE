# ROG ZOMBIE — Game Design Document

Versione consolidata — 08/09/2026 — include la revisione CD REDUCTION  
Destinazione nel repository Unity: `Docs/ROG_ZOMBIE_GDD.md`  
Fonte: `ROG_ZOMBIE_WORLD_2026-09-08`, documento «ROG ZOMBIE — WORLD — Documento di riferimento».

Questo GDD raccoglie integralmente le specifiche presenti nella fonte, organizzate in sezioni e tabelle Markdown. I dati consolidati del documento dell’08/09/2026 prevalgono sulle versioni precedenti. Le note aggiunte per evidenziare lacune e ambiguità sono distinte dalle regole di gioco e non introducono nuove meccaniche.

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

- Dopo l'eliminazione di tutti i MOB, la PARTY deve raggiungere l'uscita.
- Quando tutti i PG ALIVE sono vicini all'uscita, viene aperta la scelta BONUS di fine AREA.
- Ogni PLAYER sceglie 1 BONUS tra 5 proposte.
- Dopo la conferma di tutti i PLAYER, la PARTY passa alla nuova AREA.
- Entrando in una nuova AREA ogni PG recupera il 15% dei propri HP MASSIMI correnti, senza superare il massimo.
- I PG in DOWN vengono resuscitati automaticamente nel passaggio alla nuova AREA secondo le regole di resurrezione.
- L'EXP DROP continua a crescere tra le AREE anche quando si passa a una nuova CITTÀ: non si resetta.

<a id="sezione-7"></a>

## 7. HUB

- L'HUB è una piccola zona sicura ed esplorabile tra le RUN.
- MERCHANT: acquisto e gestione degli ITEMS tramite G.
- PROF: acquisto degli UPGRADE permanenti tramite G.
- EXIT: accesso alla preparazione e partenza della RUN.
- La preparazione mostra 4 BANNER PG per la PARTY.
- Il ROSTER contiene 8 PG. I PG già selezionati risultano BLOCKED; quelli non ancora sbloccati sono mostrati come silhouette.
- Tutti i PLAYER devono confermare la preparazione prima della partenza.

<a id="sezione-8"></a>

## 8. PREPARAZIONE RUN

- Ogni PLAYER seleziona il proprio PG.
- Ogni PG dispone di 2 ABILITÀ: il PLAYER ne seleziona 1 sola per la RUN.
- Ogni PG dispone di 2 PASSIVE: il PLAYER ne seleziona 1 sola per la RUN.
- L'ABILITÀ selezionata viene attivata con il tasto Q.
- La PASSIVA selezionata è automatica e non occupa SLOT BONUS.
- Gli ITEMS vengono assegnati agli SLOT ITEM disponibili.
- Configurazione concettuale: PG → ABILITÀ 1/2 → PASSIVA 1/2 → ITEMS → CONFERMA.

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
- Gli ITEMS possono essere riordinati durante la PREPARAZIONE RUN.
- Gli ITEMS inutilizzati vengono persi in caso di sconfitta della RUN; vengono mantenuti se si torna volontariamente all'HUB dopo un BOSS.

<a id="sezione-10"></a>

## 10. SISTEMA DI COMBATTIMENTO

- Movimento: W-A-S-D.
- Mira: mouse.
- ATTACCO BASE: LMB; tenendo premuto LMB si mantiene il fuoco continuo.
- INTERAZIONE: F.
- ABILITÀ selezionata: Q.
- Munizioni infinite e nessun reload.
- Gli attacchi base sono istantanei, senza tempo di viaggio del proiettile, salvo comportamenti specifici definiti per arma/abilità.
- Regola base del danno: DANNO finale = ATK × (1 − DEF%).
- ATK SPD: 100 = 1 colpo/s.
- MOVE SPD: 100 = 2 m/s.
- CD REDUCTION: STAT modificatore del PG, senza unità in secondi; VALORE BASE neutro = 100. CD REDUCTION 100 = 100% del CD BASE della singola ABILITÀ selezionata.
- Ogni ABILITÀ mantiene il proprio CD BASE in secondi, definito nella relativa scheda PG.
- Formula: CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100).
- Esempio: CD BASE ABILITÀ 10 s e CD REDUCTION 90 → CD FINALE 9 s. Un valore CD REDUCTION più basso riduce il cooldown.
- RANGE: 100 = 1 m nel sistema interno.
- DEF interno: 100 corrisponde a 0% di modificatore mostrato al PLAYER.

<a id="sezione-11"></a>

## 11. ROSTER PG — STATS BASE

| PG | ARMA | HP | ATK | DEF | MOVE SPD | ATK SPD | CD REDUCTION | RANGE |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| PG01 | Shotgun | 100 | 20 | +15% | 100 | 85 | 100 | 3 m |
| PG02 | Assault Rifle | 100 | 10 | 0% | 100 | 150 | 100 | 10 m |
| PG03 | Sniper Rifle | 80 | 50 | −10% | 100 | 70 | 100 | 5 m |
| PG04 | Grenade Launcher | 100 | 30 | 0% | 100 | 75 | 100 | 5 m |
| PG05 | Knife | 100 | 30 | −10% | 130 | 150 | 100 | 2 m |
| PG06 | Revolver | 120 | 40 | 0% | 100 | 80 | 100 | 10 m |
| PG07 | Bow | 100 | 30 | 0% | 110 | 110 | 100 | 15 m |
| PG08 | Heavy Machine Gun | 100 | 7 | +10% | 90 | 200 | 100 | 8 m |

Nota: le STATS sopra riportano gli ultimi valori definiti esplicitamente nel progetto, inclusa la revisione CD REDUCTION. CD REDUCTION è una STAT modificatore con VALORE BASE 100 per PG01–PG08; i CD BASE in secondi restano esclusivamente nelle singole ABILITÀ e vengono modificati nel CD FINALE secondo la formula della sezione 10.

<a id="sezione-12"></a>

## 12. ABILITÀ E PASSIVE DEI PG

I valori CD in secondi riportati nelle schede seguenti sono i CD BASE delle singole ABILITÀ; restano invariati. Il CD FINALE dipende dalla STAT CD REDUCTION del PG secondo la formula della sezione 10.

### PG01 — Shotgun

- ABILITÀ 1 — PESTONE: 40 DANNO; cono frontale 3 m × 5 m; CD 10 s.
- ABILITÀ 2 — BARRIERA: piazza un muro invalicabile e indistruttibile di 7 m × 1 m; durata 5 s; CD 15 s.
- PASSIVA 1: con HP <50% ottiene +15% DEF; l'effetto termina quando gli HP tornano ≥50%.
- PASSIVA 2: con HP <50% ottiene +15% ATK; l'effetto termina quando gli HP tornano ≥50%.

### PG02 — Assault Rifle

- ABILITÀ 1 — FUOCO RAPIDO: ATK SPD 300 (3 colpi/s); durata 3 s; CD 10 s.
- ABILITÀ 2 — FUOCO DI SOPPRESSIONE: 30 DANNO; cono frontale 10 m / 45°; CD 12 s.
- PASSIVA 1: con HP <30%, ogni KILL cura 1% HP; non ha effetto a HP ≥30%.
- PASSIVA 2: ogni 15 KILL entra in RAGE per 2 s con +30% ATK.

### PG03 — Sniper Rifle

- ABILITÀ 1 — COLPO PERFORANTE: 100 DANNO; attraversa i MOB sulla traiettoria; CD 8 s.
- ABILITÀ 2 — TRIPLO SPARO: 3 proiettili da 40 DANNO; cono totale 20 m / 30°; ogni proiettile copre 10°; sequenza SINISTRA → CENTRO → DESTRA; intervallo 0,3 s; CD 15 s.
- PASSIVA 1: l'ATTACCO BASE perfora il primo MOB colpito.
- PASSIVA 2: l'ATTACCO BASE applica un MARCHIO; il MOB marchiato ha DEF −20%; la HIT successiva sul MOB marchiato rimuove il MARCHIO.

### PG04 — Grenade Launcher

- ABILITÀ 1 — PIOGGIA DI GRANATE: 10 granate con le stesse STATS dell'ATTACCO BASE, lanciate RANDOM in un'AREA EFFETTO circolare di raggio 7 m; CD 12 s.
- ABILITÀ 2 — COLPO GROSSO: i successivi 4 ATTACCHI BASE ottengono +80% ATK e +50% RAGGIO esplosione; CD 10 s.
- PASSIVA 1: può accumulare +1 ITEM GRANATA oppure +1 ITEM MOLOTOV aggiuntivo.
- PASSIVA 2: le AREE delle granate usate dall'ABILITÀ selezionata vengono incendiate per 3 s e infliggono 5 HP/s. Vale con entrambe le ABILITÀ, ma non con il normale ATTACCO BASE.

### PG05 — Knife

- ABILITÀ 1 — INVISIBILITÀ: rende invisibile l'intera PARTY e aumenta MOVE SPD del 15%; durata 2 s; CD 15 s.
- ABILITÀ 2 — COLTELLI AVVELENATI: lancia 8 coltelli in direzioni diverse; RANGE 10 m; 30 DANNO; VELENO 5 HP/s per 3 s; CD 10 s.
- PASSIVA 1: con HP <30%, quando riceve una HIT diventa invisibile per 1 s; non ha effetto a HP ≥30%.
- PASSIVA 2: ogni 10 ATTACCHI BASE, il successivo applica VELENO: 5 HP/s per 3 s.

### PG06 — Revolver

- ABILITÀ 1 — CURA AD AREA: cura 40 HP ai PG entro raggio 4 m; CD 20 s.
- ABILITÀ 2 — FUOCO CURATIVO: i successivi 6 ATTACCHI curano di 15 HP il PG con meno HP entro 15 m; PG06 è incluso tra i possibili bersagli; CD 15 s. Se non ci sono alleati nel raggio e PG06 è al 100% HP, la cura non ha effetto su nessuno.
- PASSIVA 1: a ogni KILL il MOB ha 3% di probabilità di rilasciare un MEDI KIT.
- PASSIVA 2: dopo una cura tramite le sue ABILITÀ, PG06 e/o i PG effettivamente curati ottengono +20% del proprio ATK SPD BASE per 3 s. L'effetto non si cumula; una nuova cura rinnova la durata a 3 s.

### PG07 — Bow

- ABILITÀ 1 — MULTI SHOOT: 25 DANNO; cono frontale 7 m / 90°; respinta 5 m; CD 10 s.
- ABILITÀ 2 — PIOGGIA DI FRECCE: area bersaglio circolare, raggio 5 m; 15 HP/s; durata 3 s; CD 12 s.
- PASSIVA 1: ogni ATTACCO BASE ha 2% di probabilità di infliggere DANNO AD AREA pari al 30% dell'ATK entro 3 m dal MOB colpito.
- PASSIVA 2: ogni ATTACCO BASE ha 5% di probabilità di colpire fino a 2 MOB aggiuntivi sulla stessa traiettoria.

### PG08 — Heavy Machine Gun

- ABILITÀ 1 — FILO SPINATO: crea un ANELLO di raggio 7 m e spessore 1 m; 10 HP/s; slow 25%; durata 3 s; CD 16 s.
- ABILITÀ 2 — COLPI RESPINGENTI: per 3 s ogni ATTACCO BASE respinge il MOB colpito di 1,5 m; CD 10 s.
- PASSIVA 1: ogni KILL fornisce +1% DEF fino a +30%; quando PG08 riceve una HIT tutto il BONUS DEF accumulato viene perso.
- PASSIVA 2: dopo 5 s di fuoco continuativo entra in RAGE: +30% ATK e −20% MOVE SPD. RAGE permane finché continua a sparare senza interruzioni.

<a id="sezione-13"></a>

## 13. MOB

| MOB | HP | ATK | DEF | MOVE SPD | ATK SPD | RANGE | G BASE | EXP BASE |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ZOMB01 | 50 | 15 | 0% | 60 | 50 | 1 m | 10 G | 10 EXP |
| ZOMB02 | 40 | 25 | 0% | 75 | 70 | 1 m | 15 G | 15 EXP |
| ZOMB03 | 100 | 20 | +10% | 50 | 50 | 2 m | 20 G | 20 EXP |

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
- Distribuzione A1 di ogni CITTÀ: 75% ZOMB01 / 20% ZOMB02 / 5% ZOMB03.
- Ogni AREA successiva della stessa CITTÀ: ZOMB01 −10 punti percentuali, ZOMB02 +5, ZOMB03 +5.
- La distribuzione riparte dalla configurazione A1 quando inizia una nuova CITTÀ.
- Quando l'arrotondamento delle quantità crea una differenza, viene favorito ZOMB01 mantenendo il totale esatto.

<a id="sezione-15"></a>

## 15. SISTEMA DI SPAWN

- FIRST SPAWN = 30% del totale MOB previsto nell'AREA.
- La porzione inizialmente visibile dell'AREA è libera da MOB.
- Per ogni MOB ucciso viene fatto spawnare 1 nuovo MOB, finché non è stato raggiunto il totale previsto dell'AREA.
- Il numero di MOB contemporaneamente presenti non supera il FIRST SPAWN.
- I nuovi MOB vengono generati OFF-SCREEN e in prossimità della PARTY.

<a id="sezione-16"></a>

## 16. SISTEMA DI DROP

### 16.1 G

- G DROP base: ZOMB01 10 G; ZOMB02 15 G; ZOMB03 20 G.
- Il G è COMUNE ai PLAYER: se un MOB droppa 10 G, ogni PLAYER riceve 10 G; il valore non viene diviso.
- In SINGLE PLAYER il G è relativo al PLAYER.
- In CO-OP il G è COMUNE a tutti i PLAYER.
- Il G viene mantenuto tra le RUN, salvo la penalità di sconfitta descritta più avanti.

### 16.2 EXP

- EXP DROP base iniziale uguale al G DROP base: 10 / 15 / 20.
- L'EXP è COMUNE a tutti i PG della PARTY.
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
- Quando tutti e 3 gli SLOT sono occupati, il PG non può più ricevere nuove ABILITÀ BONUS: può scegliere soltanto UPGRADE delle ABILITÀ BONUS possedute.
- Un UPGRADE può comparire anche prima che i 3 SLOT siano pieni, purché il relativo BONUS sia già stato acquisito.
- Gli UPGRADE possono comparire solo per ABILITÀ BONUS già possedute.
- Le PASSIVE personali dei PG non occupano SLOT BONUS.

<a id="sezione-19"></a>

## 19. CHEST

- Massimo 1 CHEST per AREA.
- Probabilità base di comparsa CHEST: 5%.
- La CHEST assegna un BONUS a ogni PG della PARTY; ogni PG riceve il proprio BONUS casualmente.
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

<a id="sezione-20"></a>

## 20. LEVEL UP

- Al raggiungimento di un nuovo LVL il gioco viene automaticamente messo in PAUSA.
- Vengono mostrati 3 BANNER con le scelte disponibili.
- I BONUS di LVL UP riguardano le ABILITÀ BONUS e gli UPGRADE delle ABILITÀ BONUS già possedute.
- In CO-OP il gioco riprende quando tutti i PLAYER hanno effettuato la propria scelta.
- In SINGLE PLAYER il gioco riprende quando sono state effettuate le scelte per tutti i PG.

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
| 8 | TASER | DANNO 5; RAGGIO 4 m; BLOCK 2 s; CD 12 s. |
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

- All'interno delle AREE possono comparire MEDI KIT.
- Probabilità di comparsa: 30% per ciascun possibile punto di spawn.
- Possono comparire più MEDI KIT contemporaneamente nella stessa AREA.
- Il posizionamento è casuale per incentivare l'esplorazione.
- Cura base: 10% degli HP MASSIMI del PG.
- La quantità di cura può essere potenziata presso il PROF.
- PG06 PASSIVA 1 aggiunge inoltre un 3% di probabilità di drop MEDI KIT a ogni KILL.

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
- I PG ancora in DOWN al passaggio in una nuova AREA vengono resuscitati automaticamente secondo la regola di resurrezione.

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
- Definire ulteriori MOB/varianti oltre ZOMB01, ZOMB02 e ZOMB03.
- Definire il comportamento AI dei PG in SINGLE PLAYER e l'AI dettagliata dei MOB.
- Definire UI/HUD: HP, abilità, PASSIVA, SLOT BONUS, SLOT ITEM, EXP/LVL, G, indicatori DOWN e schermate di scelta.
- Definire la UX completa della PREPARAZIONE RUN e delle schermate BONUS.
- Definire eventuali limiti/cap massimi per gli UPGRADE delle ABILITÀ BONUS.
- Definire la probabilità generale con cui, quando gli SLOT BONUS non sono pieni, un BANNER ABILITÀ BONUS propone una nuova ABILITÀ oppure un UPGRADE già disponibile.
- Definire le regole di duplicazione nei BANNER: se la stessa ABILITÀ/UPGRADE può comparire più volte nella stessa scelta.
- Definire eventuali regole di stacking per VELENO, BRUCIATURA e altri effetti persistenti non ancora specificate.
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
- Ingresso nuova AREA: cura 15% HP MASSIMI.
- Sconfitta PARTY: perdita del 50% del G ottenuto nella RUN.

<a id="sezione-32"></a>

## 32. Chiarimenti necessari — DA DEFINIRE

Le voci seguenti sono note editoriali di verifica. Evidenziano ciò che la fonte non determina in modo sufficiente per l’implementazione; non modificano le specifiche delle sezioni precedenti.

| Ambito | Dati conservati | DA DEFINIRE |
| --- | --- | --- |
| Lore e vittoria finale | Esperimento scientifico; scienziato come possibile BOSS finale, non definitivo. | Identità e ruolo definitivo dello scienziato, cura, conclusione narrativa e comportamento dopo il BOSS della CITTÀ 5. |
| Città e contenuti opzionali | 5 CITTÀ, 23 AREE; percorso principale lineare; possibili quest sotterranee. | Nomi, mappe, accesso e rientro dalle quest bonus, loro rapporto con il conteggio delle AREE e l’aumento dell’EXP DROP. |
| PARTY e sblocchi | 4 PG, roster di 8; PG selezionati BLOCKED; non sbloccati in silhouette. | PG inizialmente disponibili, condizioni di sblocco, gestione dei PG AI in CO-OP con meno di 4 PLAYER, assegnazione di equipaggiamento e scelte ai PG AI. |
| HUB e NPC | MERCHANT, PROF, EXIT; possibili NPC liberati nelle quest. | Identità, dialoghi, condizioni di sblocco e servizi degli altri NPC. |
| Attacchi base e ITEMS | Armi, ATK, RANGE e regola generale di attacco istantaneo definiti. | Geometria e bersagli degli attacchi delle singole armi, raggio base delle esplosioni di PG04, eccezioni al colpo istantaneo, uso e tasti degli ITEMS, bersagli di danni/cure/invisibilità e friendly fire. |
| DEF e modificatori | DEF interno 100 = modificatore mostrato 0%; danno = ATK × (1 − DEF%). | Conversione completa fra DEF interna e percentuale mostrata, combinazione di bonus DEF e riduzioni come SCUDO, limiti e arrotondamento del danno. |
| CD REDUCTION — regola consolidata | STAT modificatore del PG con VALORE BASE neutro 100; 100 = 100% del CD BASE ABILITÀ. CD FINALE = CD BASE ABILITÀ × (CD REDUCTION / 100). Ogni ABILITÀ mantiene il proprio CD BASE in secondi. CHEST riduce la STAT di 5 punti con base 100; ogni UPGRADE PROF la riduce di 1 punto, calcolato sul VALORE BASE ORIGINALE. | Eventuali cap/limiti minimi o finali della STAT CD REDUCTION e regole di arrotondamento della STAT e del CD FINALE. |
| Avvio del cooldown | CD BASE delle ABILITÀ conservati nelle schede PG. | Momento di avvio del CD delle abilità che potenziano attacchi successivi. |
| Effetti e passive | Valori e condizioni conservati integralmente nelle schede PG. | Stacking e refresh non esplicitati, attribuzione delle KILL, base dell’1% HP curato dalla PASSIVA 1 di PG02, selezione per HP assoluti o percentuali di FUOCO CURATIVO, gestione dell’ITEM aggiuntivo di PG04. |
| ABILITÀ BONUS | 10 abilità con STATS, RATE e UPGRADE. | Attivazione, mira e comportamento non esplicitati, frequenza del danno di AURA TOSSICA e interpretazione dell’UPGRADE DANNO di FIRE BULLET, che riporta ATK +15% anziché un danno base autonomo. |
| MOB, BOSS e spawn | Totali per AREA, distribuzioni e FIRST SPAWN al 30% definiti. | Inclusione di BOSS/MINI BOSS nei totali e nel limite simultaneo, arrotondamento completo delle quantità per tipo, distanze OFF-SCREEN in presenza di PLAYER separati. |
| EXP per livello | +15% per livello e tabella LVL 1 → 11. | Regola esatta di arrotondamento «alla decina»: la fonte rinvia a una regola precedente senza esplicitarla e la tabella non è riproducibile con un unico arrotondamento semplice applicato ricorsivamente. Non sostituire i valori tabellari; definire il calcolo oltre le voci presenti, il livello massimo e la gestione dell’EXP eccedente. |
| BONUS di fine AREA | 3 proposte STATS BASE + 2 proposte ABILITÀ BONUS; scelta di 1 su 5 per PLAYER. | Valori e RATE delle proposte STATS di fine AREA, non esplicitamente equiparati a quelli CHEST; destinatari e gestione delle scelte per i PG AI. |
| CHEST e HP | BONUS CHEST del 5% del valore base; cura all’ingresso AREA del 15% HP massimi correnti. | Effetto degli aumenti di HP massimi sugli HP correnti e rapporto fra valore base, UPGRADE permanenti e BONUS temporanei. |
| G e acquisti | Ogni PLAYER riceve l’intero G DROP; il G è detto COMUNE. | Saldo condiviso spendibile o saldi individuali accreditati in parallelo, titolarità degli acquisti/UPGRADE e applicazione dei potenziamenti G DROP in CO-OP. |
| PROF | Incrementi, prezzi iniziali e ITEM SLOT massimo 4 definiti. | Cap e ambito di applicazione degli UPGRADE permanenti; conferma dell’incremento CHEST RATE, riportato come «precedentemente definito» a +1% del BASE. Non convertirlo automaticamente in +1 punto percentuale. |
| DOWN e morte | DOWN di 20 s; resurrezione con F per 5 s; ritorno al 50% HP massimi e 2 s di invulnerabilità. | Stato e possibilità di recupero alla scadenza dei 20 s, sconfitta con combinazioni di PG morti e DOWN, velocità/modalità del regresso se F viene rilasciato, distanza di interazione, interruzioni e comportamento dell’AI. |
| Passaggio AREA | Resurrezione automatica dei PG ancora DOWN e cura di ingresso del 15%. | Ordine e cumulabilità della resurrezione al 50% con la cura del 15%, distanza richiesta all’uscita e gestione dei PG eventualmente morti. |
| Fine RUN | Ritorno volontario dopo BOSS e sconfitta hanno effetti distinti; si perde il 50% del solo G guadagnato nella RUN in caso di sconfitta. | Arrotondamento del G dispari, decisione di proseguire/tornare in CO-OP, abbandono/disconnessione e relativo trattamento della RUN. |

I punti di bilanciamento, produzione artistica, prototipazione, multiplayer, salvataggio, testing e build restano quelli elencati nella sezione 30. Nessun valore aggiuntivo è stabilito da queste note.
