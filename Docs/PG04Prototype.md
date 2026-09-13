# PG04 nel gameplay loop prototype

## Selezione e prova

Aprire `Assets/Scenes/PreGameplayLoopPrototype.unity`. In `Assets/PreGameplayLoop/PreGameplayLoop.asset` scegliere Selected Player = PG04, Selected PG04 Ability (PioggiaDiGranate / ColpoGrosso) e Selected PG04 Passive (ScortaEsplosiva / Pyromania). Una nuova RUN acquisisce le selezioni, mantenute fra AREE. WASD movimento, mouse mira, LMB attacco, Q abilità. HUD provvisorio con scelta, cooldown, durata/cariche e passiva. Riprova test azzera la RUN.

PG04 usa HP 100, ATK 25, DEF 100 interna, MOVE SPD 100, ATK SPD 65 e RANGE 700 (7 m). Destinazione fissata al lancio al cursore entro 7 m dal PG, HIT dopo 0,3 s, anticipato solo da MURO; raggio 1,25 m con quattro spicchi da 90° interamente esclusi se intercettati da MURO/OSTACOLO.

PIOGGIA DI GRANATE: 10 esplosioni casuali in 3 s, entro 5 m dal cursore iniziale senza limite di distanza dal PG; CD 12 s dall'attivazione. Eredita ATK corrente e raggio dell'attacco base. COLPO GROSSO: quattro attacchi con ATK +80% e raggio +50%; anche MISS consuma cariche. Nessuna scadenza; CD 10 s dal quarto attacco. Le STATS persistenti restano indipendenti dal potenziamento. CAMBIO AREA termina effetti attivi e ne riavvia il CD; conserva CD residuo se già inattivi e disponibilità se pronti.

PYROMANIA: ogni esplosione crea un'area incendiata di pari geometria, durata 3 s. Chiarimento del proprietario: primo tick immediato all'attivazione, poi ogni secondo sul clock dell'area, indipendente dall'ingresso dei MOB. Tick a t=0,1,2; scadenza a t=3. Ogni tick applica 5 danni prima della DEF e dell'arrotondamento finale. Le aree sovrapposte applicano danni indipendenti. Pausa sospende i timer; CAMBIO AREA e reset rimuovono le aree.

SCORTA ESPLOSIVA: fino a 3 GRANATE o MOLOTOV dello stesso tipo per SLOT, contemporaneamente per entrambi i tipi; altri ITEMS massimo 1. PG04 Test Item Slots configura 1–4 SLOT tecnici, default 1. Il pannello temporaneo carica GRANATA/MOLOTOV e consuma una unità senza lanciare ITEMS. Inventario conservato fra AREE, vuoto alla nuova RUN. Acquisto, lancio, effetti ITEMS e progressione SLOT non sono implementati.

## File dell'integrazione PG04

- `Assets/PreGameplayLoop/PG04AbilityCatalog.cs`, `PG04Abilities.asset`: valori e scelte.
- `PG04AbilityRuntime.cs`, `PG04AbilityInput.cs`: attacco, abilità e comando Q.
- `PG04ExplosionEffect.cs`, `PG04AreaVisual.cs`, `PG04BurningArea.cs`: danno e visuali provvisorie.
- `PG04ItemSlots.cs`: capacità e consumo tecnico della SCORTA.
- `LoopDefinition.cs`, `LoopSession.cs`, `LoopHUD.cs`, `PreGameplayLoop.asset`: selezione, lifecycle e HUD.
- `Assets/TestEngine/PlayerWeapon.cs`: collegamento dell'attacco PG04.
- `Assets/PlayerData/PG04.asset`, `Assets/TestEngine/Data/PG04_Weapon.asset`: RANGE 7 m e delay HIT 0,3 s.
- `Assets/PreGameplayLoop/Editor/PG04Validation.cs`: test esclusivi PG04.
- Questo documento e `Docs/PreGameplayLoopPrototype.md`.

I nuovi asset/script hanno il proprio .meta; quelli esistenti sono preservati. GDD aggiornato per la successiva modifica richiesta di ATK 25, ATK SPD 65 e raggio PIOGGIA DI GRANATE 5 m.

## Verifica

Menu ROG ZOMBIE → Pre gameplay loop → Run PG04 tests. Richiede una sola scena salvata, esegue le quattro combinazioni in Play Mode e ripristina scena e selezione. Disabilita IA/input durante le prove automatiche; non misura bilanciamento o feeling dei comandi. Verifica danni reali, geometria, cadenza, tick immediato/periodico, ingresso/uscita, sovrapposizioni, pause, cariche, bonus persistenti, SLOT, CAMBIO AREA, DOWN e reset RUN. Non esegue suite di altri PG.

Esito effettivo aggiornato (ATK 25, ATK SPD 65, raggio base 1,25 m, PIOGGIA DI GRANATE raggio 5 m): `PG04-spd65-radius125-validation.txt`, Unity 6000.3.16f1, PASS, 422 controlli, 821 warning durante la suite, nessun errore/exception registrato. Compilate Assembly-CSharp e Assembly-CSharp-Editor. Console verificata dopo la suite: zero errori; warning ripetuto Sprite Tiling / sprite non Full Rect nelle visuali provvisorie esistenti. Il contatore Console cumulativo include anche messaggi fuori dalla suite. `git diff --check` superato; Git segnala conversioni LF/CRLF su file già modificati. Non eseguite suite PG01/PG02/PG03. Restano da valutare manualmente feeling, bilanciamento e leggibilità in partita; il pannello tecnico a quattro SLOT richiede spazio orizzontale sufficiente.

Aggiornamento richiesto: ATK SPD 65; raggio base e singole esplosioni PIOGGIA DI GRANATE 1,25 m; COLPO GROSSO 1,875 m. Verifica Unity completata: 422 controlli superati, solo PG04, nessun errore, 821 warning registrati.

## Delay ATTACCO BASE 0,3 s

La HIT e l’esplosione avvengono 0,3 s dopo il lancio. Posizione, ATK e raggio sono acquisiti al lancio; bersagli e spicchi validi vengono valutati alla HIT. Vale anche per COLPO GROSSO; cariche e avvio del CD restano legati al lancio. PYROMANIA nasce all’esplosione e applica subito il primo tick. PIOGGIA DI GRANATE conserva la sua temporizzazione. Pausa sospende il ritardo; CAMBIO AREA e reset rimuovono le HIT pendenti.

Nessun test eseguito per questo aggiornamento, come richiesto. I report precedenti riguardano la versione senza delay; la suite PG04 contiene ancora verifiche di impatto istantaneo e richiede adeguamento prima della prossima esecuzione.
