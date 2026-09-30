# BOSS 01 — scena di test locale

Aprire `Assets/Scenes/Boss01Test.unity`, attendere l'importazione e avviare Play manualmente.
La scena costruisce l'arena all'avvio: in Edit Mode sono presenti camera e configurazione.
Usare Game View 16:9. WASD, mouse, LMB, Q; comandi dei compagni invariati (SPACE + 1/2/3).
PG01 controllato e PG02/03/04 IA. Il test parte da **AREA 3 già completata**, senza MOB ordinari, vicino all'uscita originale. Riavvio con **Riprova test**: si torna a questo punto di AREA 3.

## Passaggio AREA 3 → AREA BOSS e presentazione

1. Avanzare verso il cerchio verde: tutto il gruppo vivo deve entrare nel trigger esistente di raggio 4 m.
2. Scegliere e confermare il BONUS di fine AREA per PG01 e per ciascuno dei tre PG IA, usando il flusso già presente.
3. Il gruppo viene trasferito all'ingresso dell'arena 60 x 40 m; camera ferma sui PG per **1 s**, gameplay sospeso e bandelle nere sopra e sotto.
4. **1,5 s**: camera dai PG al BOSS, con movimento morbido.
5. **2 s**: ruggito visivo provvisorio, dilatazione e ritorno alla dimensione normale della sola sprite. Nessun audio, attacco o variazione del collider.
6. **2 s**: camera si allontana realmente dal piano di gioco e si centra sull'arena, fino a visualizzare **55 m** orizzontali. Durante questo movimento le bandelle si ritirano progressivamente fino a scomparire.
7. Riprende il gameplay: PG e BOSS liberi, primo speciale dopo **13 s dall'avvio della BOSS FIGHT**, non dall'ingresso in AREA 3 né dall'inizio della presentazione.

I quattro tempi e l'altezza delle bandelle (10% dello schermo ciascuna come impostazione visiva iniziale) sono configurabili sul componente **BossTestScene** dell'oggetto **BOSS 01 test**. L'offset iniziale rispetto all'uscita AREA 3 e le posizioni nell'arena sono anch'essi configurabili. Durante la presentazione il tempo di gameplay è sospeso; solo camera e animazione visiva procedono. Movimento, mira, armi e abilità non possono essere usati. La camera di combattimento mantiene i 55 m e il pan a 3 m dal bordo già previsto.

Il passaggio riutilizza il cambio AREA esistente: conserva il gruppo, applica il recupero HP e gestisce gli effetti delle abilità come negli altri cambi AREA. Non occorre giocare le AREE 1–3: la fine di AREA 3 è simulata.

## Configurazione

`Assets/BossTest/BOSS01.asset` contiene dati indipendenti dal runtime, modificabili nell'Inspector.
HP 15000, ATK 80, DEF interna 100, MOVE SPD 95 = 1,9 m/s, ATK SPD 50 = intervallo 2 s.
Diametro collider 5 m. RANGE interno 300 rappresenta l'attivazione dell'attacco base a 3 m dai bordi.
Arena interna 60 x 40 m; camera mostra 55 m orizzontali, soglia pan 3 m, velocità del PG controllato.
Posizioni provvisorie di PG e BOSS configurabili sul componente BossTestScene; non sono level design definitivo.

## Attacchi e controlli manuali

- BASE: attivazione a 3 m tra bordi; arco 110°, profondità 4 m dal bordo BOSS. Fermo fino al termine a 1,5 s, ruota solo nel primo secondo, HIT a 1 s. Intervallo tra avvii invariato a 2 s: dopo il termine restano 0,5 s prima del successivo attacco base. Arco visualizzato brevemente alla HIT.
- 01: tre rami 2,5 x 12 m, angoli 0°/+30°/-30°. Riempimento rosso in 2 s, rotazione solo nel primo secondo, HIT simultanea 70 una volta per PG, recupero 1 s.
- 02: fino a tre posizioni di PG distinti casuali fissate alla comparsa. Cerchi raggio 3 m, riempimento dal centro in 2,5 s, impatti simultanei 40 + STUN 1 s. BOSS libero dopo 2 s. Sovrapposizione: più HIT, STUN non sommato.
- 03: a 12000/9000/6000/3000 HP (80%/60%/40%/20%), attende la fine dell'attacco corrente. Se un singolo colpo supera più soglie, una sola carica. Bersaglio casuale entro 18 m dai bordi; in mancanza lo cerca. Preparazione 1,5 s, direzione fissa; carica 18 m a 10 m/s, larghezza 5 m. Una HIT 40 per PG e respinta 3 m in 0,25 s. Recupero 0,5 s oppure STUN 1,5 s se urta muro/ostacolo, interrompendo la carica.
- 04: cono 150°, dieci spicchi e dieci proiettili da sinistra a destra rispetto al BOSS. Direzione fissa; primo sparo a 1 s, successivi ogni 0,33 s, ultimo a 3,97 s, recupero 0,5 s. Ogni spicchio si riempie dalla comparsa fino al proprio sparo. Proiettile: 13 m/s, percorso 15 m dal bordo BOSS, raggio collider 0,5 m. HIT 30, non perforante; veleno 5 HP/s per 3 s, istanze indipendenti senza cap specifico.

Sequenza speciale 01/02/01/04. Primo speciale dopo 13 s; successivi 13 s dopo conclusione, incluso recupero. Per 02 si conta dall'impatto dei massi. La carica azzera il timer, senza avanzare l'indice della sequenza.

Aggro: PG attivo più vicino, parità casuale; non seleziona PG invisibili/DOWN/MORTI né PET. Un'area già lanciata può colpire anche un PG diventato invisibile. Con nessun bersaglio segue il movimento casuale MOB definito nel GDD. Le HIT usano il calcolo DEF esistente.

Per osservare l'intera sequenza, iniziare senza sparare e schivare; successivamente danneggiare il BOSS per controllare le soglie HP. Il riquadro mostra HP, azione e timer. Provare anche la carica verso un muro e il pan della camera lungo tutti i bordi.

## Limiti del test

- Scena locale isolata con uscita AREA 3 e BONUS di fine AREA funzionanti. HubPrototype integra ora il BOSS dopo AREA 3; restano esclusi multiplayer, premi BOSS e avanzamento dopo la vittoria.
- Presentazione d'ingresso provvisoria con tempi approvati. BOSS, attacchi e massi hanno rappresentazione geometrica provvisoria; nessuna animazione artistica dei massi.
- Arena vuota con muri perimetrali. Non costituisce una validazione di navigazione BOSS attorno a un futuro livello con ostacoli interni.
- Con formato più stretto di 55:40 è matematicamente impossibile mostrare 55 m orizzontali mantenendo tutta l'inquadratura dentro i 40 m verticali. Per questo test usare 16:9.
- Valori approvati nella conversazione; GDD e AGENTS.md non modificati. La regola del veleno BOSS è specifica e non cambia il veleno PG05.
- Compilazione C# esterna all'Editor riuscita, con soli warning CS0649 preesistenti sui campi serializzati. Play Mode, rendering e importazione scena restano da verificare in Unity dall'utente. Nessuna build e nessun commit.

## File

Creati: `Assets/BossTest/BossDefinition.cs` (dati), `BOSS01.asset` (configurazione), `BossTestScene.cs` (inizializzazione e camera), `BossBrain.cs` (attacchi e aggro), `BossArea.cs` (indicatori e geometria HIT), `BossPlayerStatus.cs` (STUN e veleno indipendente), relativi `.meta`, `Assets/BossTest.meta`, `Assets/Scenes/Boss01Test.unity` e `.meta`, questo documento.

Modificati: `LoopSession.cs` (ramo opzionale AREA 3 completata e arena BOSS senza spawn ordinario), `Combatant.cs` (inoltro STUN e blocco movimento PG), `PlayerMovement.cs`, `PlayerAim.cs`, `PlayerWeapon.cs`, `CompanionFormation.cs`, `PartyCommands.cs` (rispetto dello STUN), `BonusAbilityRuntime.cs` e `ItemArea.cs` (inoltro STUN anche al BOSS). Gli altri test non aggiungono i componenti BOSS.

### Aggiornamento: transizione e camera

- Creato `Assets/PreGameplayLoop/LoopSession.BossTest.cs` con `.meta`: inizializza AREA 3 completata, collega uscita/bonus e attende la presentazione prima di abilitare il combattimento.
- Modificati `BossTestScene.cs` (geometrie separate, fasi, camera prospettica, presentazione solo visiva), `BossBrain.cs` (avvio esplicito del timer), `LoopSession.cs` (partenza e riavvio in AREA 3), `LoopHUD.cs` (stato della presentazione), `Boss01Test.unity` (parametri serializzati), questo documento.
- La copia runtime di LoopDefinition aggiunge un quarto indice per il BOSS. I metadati di popolazione dell'ultimo indice sono copiati solo per compatibilità con la validazione esistente: nessun MOB ordinario viene generato in questa simulazione.
- Controlli manuali: il BOSS non esiste in AREA 3; ingresso solo con gruppo raccolto; quattro scelte BONUS; PG immobili per tutta la presentazione; collider BOSS invariato durante il ruggito; visuale finale 55 m; attacchi e timer partono dopo la presentazione; Riprova test riporta ad AREA 3.
Durante la presentazione l'HUD di test è nascosto; ricompare al termine dell'allontanamento. Le bandelle sono sovrapposte e non cambiano il rapporto della camera né il calcolo dei 55 m.

## Integrazione HubPrototype

Aprire `Assets/Scenes/HubPrototype.unity`, selezionare normalmente PG, compagni, abilità, passive e ITEMS, quindi avviare la RUN. Completare AREA 1, AREA 2 e AREA 3 con i rispettivi MOB e bonus. Dopo i bonus di AREA 3 si carica AREA 4, dedicata al BOSS, con la stessa presentazione e gli stessi dati di `BOSS01.asset`.

Il componente BossTestScene distingue il test rapido (`StartAtCompletedArea3 = true`, scena Boss01Test) dalla RUN HUB (`false`). Nel secondo caso non sostituisce il party scelto, non avvicina lo spawn all'uscita e non salta i MOB delle prime tre aree. HP, ITEMS e progressione sono conservati dal normale cambio AREA. Riprova test nella RUN HUB riparte da AREA 1.

File aggiornati per il collegamento: `Assets/PreGameplayLoop/HubPrototype.cs`, `Assets/Scenes/HubPrototype.unity`, `Assets/BossTest/BossTestScene.cs`, `Assets/PreGameplayLoop/LoopSession.cs`, questo documento. Il riferimento Boss nell'Inspector di HubPrototype punta a `BOSS01.asset`. Nessuna modifica a package, Project Settings, AGENTS.md o GDD.

Verifica effettuata: compilazione C# riuscita con soli warning CS0649 già presenti. Il flusso completo resta da provare manualmente in Play Mode. Dopo la morte del BOSS non sono ancora introdotti premi o avanzamento: si può usare il menu pausa per tornare all'HUB.