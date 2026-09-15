# Composizione MOB del test â€” CITTÃ€ 1

Le tre AREE usano le percentuali GDD, con arrotondamento per eccesso di ZOMB02/ZOMB03 e resto assegnato a ZOMB01 tramite SpawnManager.Allocate esistente.

| AREA | Percentuali ZOMB01/02/03 | Totali ZOMB01/02/03 | FIRST SPAWN ZOMB01/02/03 |
|---|---|---|---|
| 1 | 75 / 20 / 5 | 75 / 20 / 5 | 22 / 6 / 2 |
| 2 | 65 / 25 / 10 | 78 / 30 / 12 | 23 / 9 / 4 |
| 3 | 55 / 30 / 15 | 82 / 45 / 23 | 24 / 14 / 7 |

ZOMB01 verde; ZOMB02 arancione; ZOMB03 viola. Medesimo segnaposto circolare, raggio e CircleCollider2D di ZOMB01 per tutti. STATS, comportamento, attacco e DROP derivano dalla definizione del singolo MOB. ZOMB02 MOVE SPD corretto da 120 a 100 e ZOMB03 da 90 a 70 per corrispondere al GDD. ZOMB04/05 non previsti in queste AREE e quindi non sorteggiati.

File: LoopDefinition.cs (distribuzioni e validazione), PreGameplayLoop.asset (configurazione), LoopSession.cs (spawn del tipo corretto e colore), ZOMB02.asset/ZOMB03.asset (MOVE SPD). AreaEffectsValidation.cs include la prova mirata avviabile con .mob-composition-test.request. La vecchia regressione LoopValidation.cs conserva esplicitamente la propria fixture ZOMB01-only per non alterare le asserzioni storiche su EXP/G.

Verifiche: compilazione runtime ed Editor riuscita; due warning CS0252 preesistenti in PG05Validation.cs. Controllo statico di percentuali, arrotondamenti, FIRST SPAWN, riferimenti asset e MOVE SPD superato: MOB-composition-static-r1.txt. Unity tornato disponibile: MOB-composition-validation-r1.txt PASS, 95 verifiche, 0 warning in Play Mode. Verificati tutti e tre i cicli reali: FIRST SPAWN e quote complete per tipo, colori, sprite/collider identici, HP/MOVE SPD, DROP G, NavMesh e passaggi AREA fino alla conclusione della terza. I brain dei MOB sono disabilitati dal verificatore durante lo svuotamento delle ondate: questa prova non verifica il comportamento di combattimento. Restano i due warning di compilazione PG05 preesistenti. Nessuna build, commit o push.

Test mirato in Editor/AreaEffectsValidation.cs, richiesta .mob-composition-test.request.
