# Controllo effetti ad area — 14/09/2026

Controllo del codice e regressione Play Mode delle aree implementate nel prototipo, dopo le correzioni del renderer condiviso e di FILO SPINATO. Nessuna ulteriore regressione emersa nella suite generale; nessuna nuova modifica al gameplay.

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
| FILO SPINATO | Dieci sezioni in spazio libero, mesh/materiale, foro centrale, tick non immediato, SLOW 40%, sovrapposizioni senza cumulo, pausa, uscita, rimozione al cambio AREA. Screenshot in Game View ispezionato. |
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
- La suite generale non ripete le vecchie aspettative sugli SPICCHI: usa la schermatura per bersaglio del GDD attuale. FILO SPINATO conserva le SEZIONI; LASER, FRECCE e CURA conservano le eccezioni.

## Limiti

GRANATA, MOLOTOV, TRAPPOLA, SMOKE e POZIONE CURATIVA non hanno ancora effetti di combattimento nel prototipo: il sistema ITEMS attuale gestisce SLOT e consumo. Non sono stati implementati o dichiarati testati qui. PG02 e le raffiche/proiettili multipli non sono aree di danno; i loro sistemi di lancio non vengono modificati da questo controllo.

I controlli non costituiscono una verifica di ogni combinazione di BONUS, bilanciamento, IA o prestazioni. La resa visiva è ispezionata in screenshot per FILO SPINATO e PYROMANIA; per le altre mesh la suite verifica componenti, triangoli, colori, UV, texture e opacità. Nessuna build esportata, commit o push.
