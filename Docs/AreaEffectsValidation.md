# Controllo effetti ad area — 14/09/2026

Controllo del codice e regressione Play Mode delle aree implementate nel prototipo, dopo le correzioni del renderer condiviso e di FILO SPINATO. Aggiornamento successivo: FILO SPINATO usa ora la schermatura per bersaglio; PG01 ha HP base 120 e PG08 ATK base 8.

## Copertura

| Effetto | Controlli eseguiti |
| --- | --- |
| ATTACCO BASE PG01 | Lancio reale, danno pieno, coperture, esclusione dei bersagli posteriori. |
| PESTONE | Attivazione reale, danno, SLOW 30%, scadenza, nessun danno/SLOW sui bersagli schermati. |
| BARRIERA | Anteprima senza collider, rilascio, visuale e collider, schermatura delle aree, scadenza. |
| COLPO LASER | Rettangolo di danno, 35 danni, attraversamento coperture, esclusione posteriore, SpriteRenderer. |
| ATTACCO BASE PG05 | Lancio reale, danno, coperture, esclusione posteriore. |
| CURA AD AREA | Attivazione reale, cura del lanciatore e alleati, portata, esclusione MOB, attraversamento coperture, VITAMINA C. |
| LUCKY SHOT | Risoluzione dell’esplosione, danni a MOB esposti, coperture, portata, mesh/materiale. Il sorteggio probabilistico non fa parte di questa prova. |
| PIOGGIA DI FRECCE | HIT locale di raggio 0,25 m anche sulle coperture, anteprima, rilascio e rimozione del riferimento di distribuzione. |
| FILO SPINATO | Fascia in spazio libero, schermatura per bersaglio, rimozione delle coperture, MOB adiacente esposto, mesh/materiale, foro centrale, tick non immediato, SLOW 40%, sovrapposizioni senza cumulo, pausa, uscita, rimozione al cambio AREA. Screenshot in Game View ispezionato. |
| AURA TOSSICA, TASER, REPULSE | Danni esposti/schermati/fuori portata, STUN TASER, movimento graduale REPULSE e assenza di respinta sui MOB schermati, visuali e loro scadenza. |
| SCIABOLATA | Semicerchio frontale, danni, coperture, esclusione posteriore, mesh/materiale e scadenza. |
| MINE | Detonazione per prossimità e danni con coperture; visuale e scadenza del lampo. |
| ZOMB03 / ZOMB05 | Risoluzione runtime dell’impatto/esplosione, destinatari PG/MOB, danni, coperture, visuale ritagliata; morte ZOMB05 dopo esplosione. |
| PG04 / PYROMANIA | Suite dedicata: lancio reale, delay, danno base e tick a 0/1/2 s, scadenza, coperture, sovrapposizione, COLPO GROSSO, tutte le 10 esplosioni PIOGGIA DI GRANATE, reset RUN. |

## Esecuzione

Esiti effettivi in Unity 6000.3.16f1:

- `AREA-effects-validation-r1.txt`: PASS, 150 controlli, zero warning/errori durante la suite.
- `AREA-PG04-pyromania-validation-r1.txt`: PASS, 36 controlli, zero warning/errori durante la suite.
- Totale: 186 controlli superati. Il verificatore Editor compila; restano due warning CS0252 preesistenti in `PG05Validation.cs`. Il gameplay utilizza le assembly runtime già compilate e importate in Unity, senza modifiche runtime in questo controllo.
- `git diff --check` superato; Git segnala conversioni LF/CRLF su alcuni file già modificati.

- Menu `ROG ZOMBIE → Pre gameplay loop → Run AREA effects regression tests`.
- Suite PG04: `ROG ZOMBIE → Pre gameplay loop → Run PG04 PYROMANIA regression test`.
- Richiedono una sola scena salvata. Selezionano i PG soltanto per le prove e ripristinano scena/selezione al termine. Input e IA sono disabilitati nei fixture; i bersagli di prova sono isolati dalla popolazione della RUN. Per alcuni effetti viene invocata direttamente la risoluzione runtime, senza attendere CD o estrazioni casuali.
- La suite generale non ripete le vecchie aspettative sugli SPICCHI: usa la schermatura per bersaglio del GDD attuale. FILO SPINATO segue ora la schermatura dal centro fisso dell’anello; LASER, FRECCE e CURA conservano le eccezioni.

## Limiti

GRANATA, MOLOTOV, TRAPPOLA, SMOKE e POZIONE CURATIVA non hanno ancora effetti di combattimento nel prototipo: il sistema ITEMS attuale gestisce SLOT e consumo. Non sono stati implementati o dichiarati testati qui. PG02 e le raffiche/proiettili multipli non sono aree di danno; i loro sistemi di lancio non vengono modificati da questo controllo.

I controlli non costituiscono una verifica di ogni combinazione di BONUS, bilanciamento, IA o prestazioni. La resa visiva è ispezionata in screenshot per FILO SPINATO e PYROMANIA; per le altre mesh la suite verifica componenti, triangoli, colori, UV, texture e opacità. Nessuna build esportata, commit o push.

## Aggiornamento FILO SPINATO — schermatura per bersaglio

- `AREA-effects-validation-r3.txt`: PASS, 156 controlli, zero warning durante la suite in Unity 6000.3.16f1.
- `AREA-effects-validation-r2.txt`: primo tentativo fermato sul controllo di uscita dopo teletrasporto del MOB nel fixture; sincronizzati i collider prima della verifica, quindi rieseguita con successo l’intera suite.
- Compilazione runtime ed Editor riuscita; due warning CS0252 preesistenti nel verificatore PG05. La suite dedicata PYROMANIA non è stata rieseguita in questo aggiornamento.
- Rimossi i campi e la geometria residua delle sezioni di FILO SPINATO, preservando i .meta. Adeguati i controlli geometrici e le aspettative di schermatura in PG08Validation e AreaEffectsValidation; questa pulizia successiva non è stata sottoposta a una nuova esecuzione Play Mode. Il report r3 documenta la precedente esecuzione della schermatura.

## Test mirato FILO SPINATO — PG08

- `PG08-wire-validation-r1.txt`: PASS in Unity 6000.3.16f1, 37 controlli, zero warning/errori durante la prova. Eseguito esclusivamente FILO SPINATO di PG08 dopo la rimozione delle sezioni.
- Verificati MURI e OSTACOLI, bersagli esposti e schermati, DANNO e SLOW, ripristino dopo rimozione copertura, foro centrale e limite esterno, sovrapposizioni senza cumulo, pausa, uscita, scadenza a 5 s, cambio AREA e reset RUN.
- Screenshot `Builds/PG08-wire-only-visible.png` ispezionato: fascia continua e foro centrale visibili.
- Nessuna modifica al gameplay, nessuna build o commit.
