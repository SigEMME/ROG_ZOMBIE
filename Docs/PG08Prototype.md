# PG08 — gameplay loop prototype

## Selezione e prova

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`. Nell'asset `Assets/PreGameplayLoop/PreGameplayLoop.asset` selezionare PG08, una `Selected PG08 Ability` e una `Selected PG08 Passive`. PG08 è predisposto con FILO SPINATO e TENACIA. Le scelte sono indipendenti, acquisite all'avvio RUN e conservate al CAMBIO AREA. Una modifica nell'Inspector viene acquisita con `Riprova test`.

WASD muove, mouse orienta, LMB mantenuto spara, Q attiva l'ABILITÀ. L'HUD mostra ABILITÀ, CD, durata, PASSIVA e stato, progressi TENACIA/RAGE e STATS effettive. L'avvio RUN applica il CD completo. Non vengono occupati SLOT BONUS.

## Implementazione

- ATTACCO BASE: HP100, ATK7, DEF+10%, MOVE90, ATK SPD650 (6,5/s), RANGE8m; proiettile fisico20m/s, non perforante, attraversa PG e si arresta su MOB/MURO/OSTACOLO. ATK SPD aggiornata a 650 su richiesta del proprietario.
- FILO SPINATO: fascia 3,5–4,5 m fissa per4s; dieci sezioni36°, ciascuna interamente scartata se interseca anche parzialmente un MURO/OSTACOLO. Un ostacolo nel foro non scarta sezioni. Geometria logica contro i box allineati agli assi del prototipo; cerchio del corpo MOB contro la fascia. Nessun collider fisico aggiunto all'anello.
- Contatto: SLOW35% immediato, primo tick10 prima della DEF dopo1s, poi ogni secondo intero di permanenza. Uscita interrompe e rientro avvia un nuovo conteggio. A permanenza completa, tick1/2/3/4s. Le sovrapposizioni condividono danno e SLOW, senza cumulo. CD16s all'attivazione anche con zero sezioni valide. Con CD ridotto possono coesistere più anelli.
- COLPI RESPINGENTI: per4s ogni HIT base mantiene il danno normale e avvia una propria respinta1,5m in0,25s con curva quadratica, partenza rapida e rallentamento finale. HIT ripetute/simultanee non eliminano le altre respinte. MURO/OSTACOLO interrompe lo spostamento; restano le collisioni del prototipo. CD10s alla fine dei4s.
- TENACIA: +0,2 punti DEF per HIT base, cap150HIT/+30punti, DEF totale limitata a190 (+90%). Le HIT ricevute, comprese quelle da0danni, azzerano il contatore. Il bonus resta separato dai dati persistenti.
- RAGE: conteggio5s dal primo colpo con LMB mantenuto; i normali intervalli ATK SPD non lo interrompono e le HIT non sono necessarie. +30% ATK e -20% MOVE SPD sono importi calcolati sulle STATS CORRENTI all'attivazione, poi rimossi senza perdere BONUS acquisiti. Rilascio, DOWN, STUN e CAMBIO AREA azzerano effetto e conteggio.
- CAMBIO AREA rimuove anelli, contatti, proiettili, respinte e passive; un'ABILITÀ ancora attiva riparte con CD intero, una inattiva mantiene il residuo. Nuova RUN azzera progressi e BONUS.

## Validazione

Suite opt-in: `ROG ZOMBIE > Pre gameplay loop > Run PG08 tests`. Richiede una sola scena salvata, entra in Play Mode, prova esclusivamente PG08 in tutte le quattro combinazioni e ripristina scena/configurazione. Nessuna suite PG01–PG07 eseguita. I test accelerano alcuni orologi tramite API di simulazione, usano proiettili reali e disabilitano input/IA per misure riproducibili; non valutano il bilanciamento o il feeling dei comandi.

Primo ciclo completo: `PG08-validation-r2.txt`, PASS, 314 controlli e 841 warning Sprite Tiling/Full Rect, senza errori Unity. Il primo tentativo (`r1`) aveva rilevato un proiettile residuo fra due prove: corretta l'attesa di isolamento nel test. Verifica finale delle parti definite: `PG08-validation-r3.txt`, PASS, 318 controlli e 841 warning Sprite Tiling/Full Rect, senza errori Unity. Include lo stato HUD degli anelli precedenti quando una nuova attivazione ha zero sezioni valide.

Esito completo dopo la conferma TENACIA: `PG08-validation-r4.txt`, **PASS — 330 controlli**, tutte le quattro combinazioni PG08, nessun errore Unity, 841 warning Sprite Tiling/Full Rect. Verificati danno della HIT che azzera TENACIA, danno della HIT successiva, cap DEF e conservazione dei BONUS persistenti. Nessun chiarimento PG08 rimasto in sospeso.

Conferma del proprietario: la HIT che azzera TENACIA beneficia ancora della DEF potenziata, nel limite del CAP 90%. Il bonus viene poi rimosso e le HIT successive usano la DEF senza TENACIA. L’ordine esistente di Combatant è conforme; aggiunti controlli sul danno della HIT di reset e di quella successiva, anche al CAP.

## Limiti

- Il prototipo conserva un solo PG attivo e ZOMB01. Nessuna UI definitiva, PARTY/IA PG, ITEMS o progressione completa.
- Non esiste ancora una fonte di STUN su PG nel prototipo: `PG08PassiveRuntime.SetStunned` è il punto di integrazione, verificato dalla suite, senza inventare nuove fonti o durate.
- Visuali provvisorie e collider MOB del prototipo. La respinta usa la stessa curva di MULTI SHOT, ma gestisce movimenti indipendenti per HIT; non aggiunge uno STUN.
- Nessuna modifica manuale a cartelle generate, pacchetti, Unity, URP o Project Settings; nessun commit/push.

## File della modifica PG08

Nuovi file in `Assets/PreGameplayLoop/`, con relativi `.meta`: `PG08AbilityCatalog.cs`, `PG08Abilities.asset`, `PG08AbilityInput.cs`, `PG08AbilityRuntime.cs`, `PG08PassiveRuntime.cs`, `PG08Push.cs`, `PG08WireGeometry.cs`, `PG08WireArea.cs`, `PG08WireContact.cs` e `Editor/PG08Validation.cs`.

File esistenti aggiornati: `Assets/PlayerData/PG08.asset`, `Assets/PreGameplayLoop/LoopDefinition.cs`, `LoopSession.cs`, `LoopHUD.cs`, `PreGameplayLoop.asset`, `Assets/TestEngine/Editor/TestEngineValidation.cs` (solo attesa ATK SPD PG08 da600 a200; questa suite generale non è stata eseguita), `Docs/ROG_ZOMBIE_GDD.md` (conferme fascia e sovrapposizioni), `Docs/PreGameplayLoopPrototype.md`. Le modifiche pregresse degli altri PG sono conservate.

La compilazione Editor mostra inoltre i due warning CS0252 preesistenti in `PG05Validation.cs:317` e `:320`. `git diff --check` segnala lo spazio finale già presente in `PreGameplayLoop.asset:14`, conservato. Non sono errori introdotti dai file PG08.

## Aggiornamento ATK SPD a 600

Aggiornati asset PG08 e GDD: 6 attacchi/s, intervallo teorico 1/6 s (circa 0,167 s). Nessun test eseguito per questa modifica. I report r1–r4 sono storici e riferiti ad ATK SPD 200; le aspettative di STAT/cadenza nei validatori devono essere aggiornate prima della prossima esecuzione.

## Aggiornamento FILO SPINATO a 5 m

Raggio esterno ridotto a 5 m, spessore invariato di 1 m: fascia da 4 a 5 m. Aggiornati catalogo, asset e GDD. Nessun test eseguito. I report precedenti si riferiscono al raggio di 7 m; le posizioni delle prove della fascia devono essere aggiornate prima della prossima esecuzione.

## Aggiornamento durate a 4 s e ATK SPD a 650

FILO SPINATO e COLPI RESPINGENTI durano ora 4 s. Il CD di COLPI RESPINGENTI resta 10 s e parte al termine dei 4 s; ogni singola respinta resta di 1,5 m in 0,25 s. ATK SPD PG08 è 650, pari a 6,5 attacchi/s. Aggiornati catalogo, asset e GDD. Nessun test eseguito: i report precedenti non validano questi nuovi valori. Aggiornare le aspettative di durata e cadenza dei validatori prima della prossima esecuzione.

## Aggiornamento SLOW FILO SPINATO al 35%

SLOW aumentato al 35% nel catalogo, nell’asset e nel GDD. Resta unico in sovrapposizione e termina all’uscita dalla fascia. Nessun test eseguito; i report precedenti si riferiscono allo SLOW del 25%. Aggiornare le aspettative di velocità nei validatori prima della prossima esecuzione.

## Aggiornamento raggio FILO SPINATO a 4,5 m

Raggio esterno aggiornato a 4,5 m nel catalogo, nell’asset e nel GDD; spessore invariato di 1 m, fascia da 3,5 a 4,5 m. Nessun test eseguito. I report precedenti non validano questo raggio; aggiornare le posizioni delle prove prima della prossima esecuzione.
