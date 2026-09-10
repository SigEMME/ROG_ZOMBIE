# ZOMB01 — presentazione senza reazione HIT

Scena: `Assets/Scenes/ENV_ZOMB01_PlayTest.unity`.

## Comportamento corrente

ZOMB01 usa IDLE, WALK, ATTACK, DEATH e SHADOW. Un colpo non letale riduce gli HP ma **non attiva animazioni di reazione, non interrompe e non riavvia lo stato corrente**. Un colpo letale attiva DEATH. ATTACK rimane collegata agli attacchi reali di ZOMB01.

| Stato | Frame | Riproduzione |
| --- | --- | --- |
| IDLE | 4 | Loop a 8fps, ciclo 0,5s |
| WALK | 8 | Loop a 8fps, ciclo 1s |
| ATTACK | 6 | Una sequenza per attacco reale, durata adattata al cooldown esistente |
| DEATH | 6 | 0,75s, poi ultimo frame fino alla rimozione gameplay a 3s |
| SHADOW | 1 | Renderer separato sotto i piedi |

Il clip IDLE originale a 60fps è conservato; soltanto il suo stato Animator usa velocità 8/60. Posizione locale del corpo zero e scala uniforme 0,3. Pivot, PNG e importazioni restano invariati. Nessuna nuova grafica o environment.

## Correzione richiesta: rimozione della reazione HIT

- `Zomb01Presentation.cs`: rimossi riferimento HitClip, ascolto di DamageApplied, callback di reazione, trigger Hit e blocco visivo dell'ATTACK durante HIT. Rimangono osservazione del movimento, evento attacco e stato MORTE.
- `ZOMB01.controller`: rimossi stato HIT, parametro Hit e transizioni collegate; rimangono quattro stati.
- `ENV_ZOMB01_PlayTest.unity`: rimosso il riferimento serializzato HitClip.
- `Zomb01PlayTestTools.cs`: non genera più clip/stato/trigger/transizioni HIT.
- `Zomb01Validation.cs`: verifica danno senza interruzione/riavvio di IDLE, WALK e ATTACK, oltre alla DEATH.
- Eliminati `Assets/Art/MOB/ZOMB01/ZOMB01_HIT.anim` e relativo `.meta`: erano stati creati esclusivamente durante l'integrazione precedente e non avevano altri riferimenti. Tutti i PNG sorgenti in `HIT/` e i loro `.meta` sono conservati.

La logica danno/HP, MobBrain, gli altri MOB, statistiche, cooldown, collisioni e timer di rimozione non sono stati modificati da questa correzione.

## Prova in Unity

Aprire la scena, attendere l'importazione e premere Play. WASD per muovere PG01, mouse e sinistro per attaccare. `AI ZOMB01 attiva` alterna il comportamento esistente e il bersaglio fermo; `Ripristina attori` funziona anche dopo la rimozione del cadavere.

1. Disattivare AI, mirare al corpo e sparare: HP60→40, IDLE continua normalmente.
2. Riattivare AI e colpire durante WALK o ATTACK: HP scendono senza reazione animata o riavvio della sequenza.
3. Portare HP a zero: DEATH resta nella posa finale fino alla scomparsa; ripristinare la prova.

Menu `ROG ZOMBIE > ZOMB01 > Validate animations and mouse input in Play`: richiede scena salvata e Play fermo. Non parte automaticamente. La verifica usa focus e mouse sintetici attraverso PlayerAim/PlayerWeapon per il primo colpo; per controllare precisamente WALK e ATTACK applica un danno non letale tramite Combatant.Hit e confronta stato e avanzamento dell'Animator al frame successivo. I test di DEATH usano nuovamente l'input sintetico di PG01.

Report nella cartella temporanea Windows: `ROG_ZOMB01_validation.txt`. Immagini camera: `ROG_ZOMB01_IDLE.png`, `_WALK.png`, `_ATTACK.png`, `_DEATH.png`. L'immagine `_HIT.png` di una vecchia verifica, se presente in Temp, è storica e non rappresenta il comportamento corrente.

Il menu di costruzione degli asset è disponibile per ricostruire i collegamenti; non occorre usarlo per giocare. Nessun commit automatico.
## Esito della correzione senza HIT

Verifica Unity 6000.3.16f1 in copia isolata: **PASS completo**. Compilazione runtime/Editor riuscita; nessuna NullReferenceException nel log.

- Mouse sintetico attraverso mira/attacco invariati: HP60→40, IDLE rimane attiva e non riparte da zero.
- Durante WALK e ATTACK: danno non letale tramite Combatant.Hit, HP ridotti, stesso stato Animator e fase della sequenza non azzerata al frame successivo.
- Otto frame WALK e sei ATTACK ancora osservati; due attacchi reali dell'AI con intervallo 1,428645s e HP PG01 74,5.
- Colpi letali tramite input sintetico: DEATH con tutti i sei frame, posa finale mantenuta, collider disabilitato e rimozione osservata a 2,998618s (timer gameplay 3s). Reset HP60 senza duplicati.

Il mouse fisico non è stato verificato. Nessun commit. I test non partono automaticamente e non alterano la scena dell'Editor aperto.