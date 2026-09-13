# PG06 nel gameplay loop prototype

## Selezione e comandi

Aprire Assets/Scenes/PreGameplayLoopPrototype.unity. In Assets/PreGameplayLoop/PreGameplayLoop.asset selezionare PG06, Selected PG06 Ability (CuraAdArea / FuocoCurativo) e Selected PG06 Passive (Elemosina / VitaminaC). Una nuova RUN acquisisce la configurazione; le selezioni si conservano fra AREE. WASD movimento, mouse mira, LMB attacco, Q abilità. HUD provvisorio con CD, cariche, passiva, durata VITAMINA C, ATK SPD effettivo e DROP.

## Regole

HP120, ATK40, DEF0%, MOVE SPD100, ATK SPD80 (0,8 attacchi/s), RANGE10 m. Proiettile fisico non perforante; MURI/OSTACOLI bloccano il colpo, alleati attraversati senza danno.

CURA AD AREA: 40 HP a tutti i PG attivi entro5 m, incluso PG06, ignorando MURI/OSTACOLI; DOWN/MORTE esclusi, nessun overheal, HP pieni validi. CD20 s all'attivazione.

FUOCO CURATIVO: sei attacchi base, carica consumata al lancio anche su MISS; ogni HIT valida aggiunge cura15 HP a un PG attivo entro15 m dalla posizione attuale di PG06. Destinatario selezionato alla HIT: HP% più bassa, HP effettivi più bassi, distanza minore, casuale in parità. PG feriti prioritari. Chiarimento del proprietario: se tutti a HP pieni, il più vicino, casuale in parità. Nessun overheal; cura e selezione ignorano ostacoli. CD15 s dal sesto lancio, cariche senza scadenza. CAMBIO AREA cancella cariche attive e ne riavvia il CD completo, altrimenti conserva CD residuo/disponibilità.

ELEMOSINA: roll3% per KILL attribuita a PG06 da qualunque fonte, una sola volta alla morte effettiva. Successo genera un MEDI KIT sulla posizione del MOB ucciso. Raccolta automatica da un PG attivo ferito della PARTY, 10% HP massimi correnti, nessun overheal/SLOT. Più PG sovrapposti: HP% minore, HP effettivi minori, casuale in parità. HP pieni/DOWN/MORTE ignorati. Durata illimitata nell'AREA, rimozione al cambio/reset. Trigger circolare provvisorio raggio0,5 m su TRIGGER_PG, IsTrigger, senza blocco fisico; rilevazione della sovrapposizione con il corpo del PG, anche senza Rigidbody dinamico. Il GDD lascia forma/dimensioni al development/testing. Generazione ordinaria di MEDI KIT e UPGRADE PROF fuori scope.

VITAMINA C: ogni bersaglio valido delle cure ABILITÀ riceve +35% del proprio ATK SPD BASE per4 s, anche con cura effettiva0. La base è acquisita all'inizializzazione del PG, separata dalle STATS correnti. Su PG06: 80 → 108 (1,08 attacchi/s). Bonus non cumulativo, nuova applicazione rinnova4 s; timer individuali e sospesi in pausa. Bonus persistenti conservati all'attivazione/disattivazione. MEDI KIT non applicano VITAMINA C. CAMBIO AREA termina i bonus.

## File

- Assets/PreGameplayLoop: PG06AbilityCatalog.cs, PG06Abilities.asset, PG06AbilityRuntime.cs, PG06AbilityInput.cs, PG06Healing.cs, PG06Vitamin.cs, PG06Medikit.cs, Editor/PG06Validation.cs, con nuovi .meta.
- LoopDefinition.cs, LoopSession.cs, LoopHUD.cs, PreGameplayLoop.asset: selezione e lifecycle.
- Assets/TestEngine/Combatant.cs: snapshot iniziale STATS e modificatore supporto separato dagli altri effetti.
- Docs/ROG_ZOMBIE_GDD.md: chiarimento sulla selezione a HP pieni; PreGameplayLoopPrototype.md e questo documento.

Asset PG06 e arma già corretti, riutilizzati senza cambiare valori. Modifiche PG05 e altre preesistenti preservate. Nessun aggiornamento Unity/pacchetti, nessuna modifica manuale alle cartelle generate, nessun commit/push.

## Verifica e limiti

Menu ROG ZOMBIE → Pre gameplay loop → Run PG06 tests. Una sola scena salvata; quattro combinazioni in Play Mode, ripristino selezione/scena. Solo PG06; bersagli alleati tecnici con STATS PG06 per verificare PARTY, IA/input disabilitati durante prove controllate. Test di danno, cadenza, blocchi, cure, priorità, HP pieni/DOWN, cariche/MISS, vitamin/bonus, drop e raccolta, pausa, cambio AREA, reset. Nessuna suite degli altri PG.

Il gameplay resta a un solo PG controllabile; nessun multiplayer o PG IA completo. Visuali provvisorie e feeling/bilanciamento da verificare manualmente. Gli UPGRADE PROF e la generazione ordinaria MEDI KIT non sono implementati in questo slice.

Esito precedente, con VITAMINA C al 25%: PG06-validation-r1.txt, Unity6000.3.16f1, PASS, 382 controlli sulle quattro combinazioni, 853 warning registrati e nessun errore/exception. Compilazione runtime/editor riuscita. Warning Sprite Tiling / Full Rect osservato nelle visuali provvisorie. Test effettuati solo su PG06; non verificano feeling/bilanciamento manuale. Il controllo diff segnala soltanto uno spazio finale preesistente in PreGameplayLoop.asset (m_EditorClassIdentifier).

VITAMINA C aggiornata al +35% dell’ATK SPD BASE su richiesta del proprietario. Nessun test eseguito su questa modifica; adeguati i valori attesi dei controlli sulle abilità. Il report precedente non verifica il nuovo bonus.
