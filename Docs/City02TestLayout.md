# Terza AREA del test â€” LAYOUT-CITY02

Reference: `C:/Users/matti/Documents/ROG ZOMBIE/REFERENCE/LAYOUT-CITY02_test.png`.

Il bordo nero della reference corrisponde a 130 Ã— 130 m. Coordinate immagine trasformate in coordinate XY Unity: centro immagine dell'AREA (645,345), scala 130/690 m per pixel, asse Y invertito. Il bordo fisico mantiene lo spessore di 1 m giÃ  usato dal prototipo.

- MURI blu: 29 rettangoli che ricostruiscono le sagome ortogonali, comprese le concavitÃ .
- OSTACOLI rossi: 84 elementi rettangolari, con rotazioni per gli elementi diagonali e segmenti per le recinzioni piegate. I piccoli marker circolari rossi sono rappresentati dal loro ingombro rettangolare nel prototipo.
- SPAWN: (0, -59), in basso al centro, come confermato dal proprietario.
- USCITA: (0, 59), in alto al centro.
- Popolazione: 150 ZOMB01; totale C1 A3 del GDD, composizione semplificata ZOMB01 giÃ  adottata dal test. FIRST SPAWN 30% = 45.
- Sequenza: AREA 1 â†’ AREA 2 â†’ AREA 3 â†’ conclusione del test. Layout, spawn e uscita delle prime due aree conservati. Restano le normali scelte BONUS e regole di cambio AREA.

## File modificati

- `Assets/PreGameplayLoop/City02TestLayout.asset` e nuovo `.meta`: geometria della terza AREA.
- `LoopDefinition.cs`: configurazioni di layout e uscita per AREA; validazione della sequenza.
- `PreGameplayLoop.asset`: terzo totale e collegamento al nuovo layout.
- `LoopSession.cs`: selezione del layout e dell'uscita corrispondenti all'AREA corrente.
- `LoopHUD.cs`: numero totale di AREE letto dalla configurazione.
- `Editor/LoopValidation.cs`: la regressione storica dei due cicli usa esplicitamente due AREE nella propria configurazione di prova.

## Risultati effettivi

Compilazione runtime ed Editor riuscita. Restano due warning CS0252 preesistenti in PG05Validation.cs, righe 317 e 320. Diff senza errori di spaziatura.

`CITY02-static-validation-r1.txt`: controllo statico positivo su dimensioni, punti di ingresso/uscita e connessione tra essi, con griglia conservativa di 0,25 m e margine collider 0,35 m. Questo controllo non Ã¨ una prova della NavMesh Unity.

Play Mode, NavMesh effettiva, FIRST SPAWN e passaggio AREA 2 â†’ AREA 3 non ancora verificati: Editor/licenza Unity indisponibili. Nessuna build del gioco, commit o push.

Aggiornamento 16/09/2026: Unity disponibile. MOB-composition-validation-r1.txt PASS, 95 verifiche, 0 warning. Confermati NavMesh, collegamento SPAWN/USCITA, FIRST SPAWN e completamento dei tre cicli, incluso AREA 2 → AREA 3. Brain MOB disabilitati durante lo svuotamento delle ondate; gameplay manuale ancora da valutare.
