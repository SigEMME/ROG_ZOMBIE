# PG03 — gameplay loop prototype

## Configurazione e prova

Scena `Assets/Scenes/PreGameplayLoopPrototype.unity`, asset `Assets/PreGameplayLoop/PreGameplayLoop.asset`. Selected Player = PG03; scegliere una Selected PG03 Ability (Colpo Laser / Triplo Sparo) e una Selected PG03 Passive (Calibro Perforante / Punto Debole), indipendenti. Una nuova RUN acquisisce queste scelte; il cambio AREA conserva le istanze e le STATS della RUN. Configurazione iniziale predisposta: PG03 / COLPO LASER / CALIBRO PERFORANTE.

WASD: movimento; mouse: mira; LMB mantenuto: attacco base; Q: abilità quando disponibile. Passive automatiche, senza SLOT BONUS. HUD provvisorio con abilità, CD, durata, passiva, ATK e MOVE SPD. Il MARCHIO è indicato da un piccolo quadrato magenta sopra il MOB.

## Regole implementate

- STATS PG03: HP 80, ATK 50, DEF interna 90 (−10% mostrato), MOVE SPD 110 = 2,2 m/s, ATK SPD 60 = 0,6 attacchi/s, CD REDUCTION 100, RANGE 20 m. Asset base e arma esistenti riutilizzati; velocità proiettile iniziale 20 m/s, configurabile nell’arma.
- ATTACCO BASE: proiettile fisico dalla bocca dell’arma, direzione verso il cursore al momento dello sparo, ATK corrente, limite 20 m. PG alleati ignorati; MURI/OSTACOLI bloccano il proiettile.
- COLPO LASER: rettangolo istantaneo frontale 20 × 2 m a partire dal PG, orientato verso il cursore. 35 DANNO prima della DEF a ciascun MOB valido una sola volta. Ignora MURI/OSTACOLI. CD BASE 18 s dall’attivazione. Visuale rettangolare provvisoria; inclusione logica in base alla posizione del MOB, come nelle altre AREE del prototipo.
- TRIPLO SPARO: tre proiettili fisici da 40 DANNO ciascuno, RANGE 20 m, ai centri dei tre spicchi da 10° nel cono di 30°, come confermato dal proprietario. Ordine SINISTRA → CENTRO → DESTRA (+10°, 0°, −10°); emissioni a 0 / 0,5 / 1 s. Ogni emissione usa bocca e mira correnti; proiettili già emessi mantengono la propria direzione. CD BASE 10 s dall’attivazione. Non perforante; ATK e ATK SPD non cambiano danno e tempi dell’abilità.
- CALIBRO PERFORANTE: soltanto l’ATTACCO BASE attraversa i primi due MOB, danneggia il terzo e si arresta. DANNO pari al 100% / 70% / 40% del danno originale del proiettile sulle tre HIT, prima della DEF (ATK 50: 50 / 35 / 20). Conteggio separato per ogni proiettile, senza riduzioni concatenate. RANGE invariato; non si estende al TRIPLO SPARO.
- PUNTO DEBOLE: prima HIT dell’ATTACCO BASE su MOB non marchiato infligge il danno normale, poi applica DEF −20 punti interni (100 → 80, 115 → 95). La HIT successiva usa la DEF ridotta e consuma il MARCHIO; se è un ATTACCO BASE non lo riapplica, come confermato dal proprietario. Il terzo ATTACCO BASE può applicarlo nuovamente. Le abilità consumano un MARCHIO esistente, senza crearne uno. Il MARCHIO non riscrive la DEF persistente, non si cumula e viene rimosso dopo il danno prima delle notifiche della HIT.

Tutti i CD iniziano completi. CD REDUCTION applicata con le regole esistenti. BONUS/loading sospendono timer ed emissioni. Al CAMBIO AREA TRIPLO SPARO ancora attivo termina e il CD riparte completo; CD già in corso non attivo conserva il residuo, DISPONIBILE resta DISPONIBILE. I proiettili e i MOB della vecchia AREA vengono eliminati con essa. Nuova RUN: HP e STATS base ripristinati, BONUS precedenti eliminati, nuove selezioni acquisite.

## Integrazione

Nuovi file: PG03AbilityCatalog.cs e PG03Abilities.asset; PG03AbilityRuntime.cs, PG03AbilityInput.cs, LaserEffect.cs, PG03PassiveRuntime.cs, WeakPointMark.cs; TestEngine/ICombatHitEffect.cs; Editor/PG03Validation.cs, con i rispettivi .meta.

Integrazione limitata a LoopDefinition, LoopSession, LoopHUD e PreGameplayLoop.asset; PlayerWeapon/Projectile supportano un effetto opzionale della HIT dell’attacco base e una sostituzione opzionale delle perforazioni. Combatant supporta un modificatore temporaneo della HIT in ingresso, consumato dopo la mitigazione. Senza questi effetti i percorsi esistenti rimangono quelli precedenti. Nessuna nuova UI definitiva, nessun nuovo layer, nessuna modifica a Unity/pacchetti/Project Settings.

## Validazione

Menu `ROG ZOMBIE → Pre gameplay loop → Run PG03 tests`; per automazione `.pg03-test.request` contiene il percorso completo del report e va creato dopo la compilazione. La suite seleziona temporaneamente PG03 in memoria e ripristina la selezione precedente al ritorno in Edit Mode. La suite istanzia solo PG03 come personaggio giocabile e attraversa tutte e quattro le combinazioni. I PG alleati sono semplici sonde fisiche, senza asset o runtime PG01/PG02. IA e input sono disabilitati nelle condizioni controllate; le AREE vengono svuotate con morti di test per verificare il loop. La prova non misura bilanciamento o feeling manuale.

Il GDD non è stato modificato durante questa integrazione. Le due conferme del proprietario sono registrate sopra. Modifiche precedenti e .meta preservati; nessun commit o push.

Prima esecuzione Unity: `PG03-validation-r1.txt` PASS, 284 controlli, 865 warning. Dopo questa esecuzione sono state aggiunte le prove del consumo del MARCHIO con un proiettile reale di TRIPLO SPARO e del riavvio con emissioni ancora pendenti; il report conclusivo è `PG03-validation.txt`. Nessuna suite PG01 o PG02 è stata avviata.

Esito finale del 13/09/2026: `PG03-validation.txt` PASS, **306 controlli** e **877 warning**, Unity 6000.3.16f1. Compilazione runtime verificata alle 01:10:09 e ultimo harness editor alle 01:13:27. Tutte le quattro combinazioni PG03 superate, incluse le prove aggiunte di TRIPLO SPARO su MARCHIO e reset con colpi pendenti. Restano i warning Sprite Tiling / Full Rect della visualizzazione provvisoria. Test PG01/PG02 non eseguiti, come richiesto. La prova del feeling tramite comandi fisici resta manuale.

## Aggiornamento CALIBRO PERFORANTE e ATK SPD — 13/09/2026

ATK SPD BASE PG03 portato a 60 (0,6 attacchi/s; intervallo 100/60 s). CALIBRO PERFORANTE applica al danno originale del singolo proiettile i fattori 1 / 0,7 / 0,4 sulle prime tre HIT, prima della DEF e dell’arrotondamento finale. Con ATK 50 e DEF neutra i danni sono 50 / 35 / 20. Il conteggio è locale al proiettile: alleati e bersagli già colpiti non avanzano il conteggio. Un nuovo proiettile riparte dal 100%; le ABILITÀ non ricevono questi fattori.

Aggiornati asset PG03, PG03PassiveRuntime.cs, ICombatHitEffect.cs e Projectile.cs (indice della HIT trasmesso all’effetto), PG03Validation.cs, GDD e questo documento. Il test aggiunto usa un BONUS ATK reale (50 → 52,5) e DEF bersagli 115: danni finali 45 / 31 / 18, verificando riduzioni prima della DEF e ripartenza del conteggio. I report precedenti restano storici.

Esito effettivo: `PG03-calibro-atkspd60-validation.txt` PASS, **320 controlli**, 879 warning Sprite Tiling / Full Rect, Unity 6000.3.16f1. Compilazione riuscita; provate tutte e quattro le combinazioni PG03, senza eseguire suite PG01/PG02. Nessun nuovo test del feeling manuale. Nessun commit o push.

## COLPO LASER — DANNO 35 — 13/09/2026

Danno fisso ridotto da 50 a 35 prima della DEF. Catalogo, asset, GDD e test aggiornati; invariati CD BASE 18 s, AREA 20 × 2 m e attraversamento dei blocchi. Attesi 35 danni contro DEF 100 e 37 contro DEF 95 (MARCHIO su DEF persistente 115), con arrotondamento finale di 36,75. La suite ora forza PG03 solo in memoria e ripristina la scelta precedente dell’Inspector, senza salvare modifiche alla configurazione.

Verifica effettiva: `PG03-laser35-validation.txt` PASS, 320 controlli, 879 warning Sprite Tiling / Full Rect in Unity 6000.3.16f1. Compilazione runtime/editor riuscita; suite eseguita solo con PG03. Le prove del LASER confermano danni 35 e 37 nei casi descritti. Nessun commit o push.
