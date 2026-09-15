# BARRIERA — ritaglio OSTACOLI/MURI

Implementazione del 15/09/2026, secondo la conferma del proprietario: ritagliare grafica e collider, lasciando posizionabili le parti libere.

- La schermatura parte dal centro del rettangolo BARRIERA (7 × 1 m), secondo la regola delle aree. Le coperture vengono lette durante la mira e nuovamente al rilascio.
- Anteprima, collider poligonale, mesh visiva e volume di navigazione condividono lo stesso contorno. La barriera creata conserva tale forma per la propria durata.
- Solo il volume residuo viene verificato contro i PG. Collisione con PG/PG IA/DOWN: anteprima rossa, rilascio annullato, nessun CD. MORTE non blocca.
- Ritaglio completo: nessuna barriera; contorno rosso del rettangolo nominale per segnalare la posizione non valida. Nessun CD.
- Parti ritagliate: nessuna collisione, nessuna espulsione dei MOB/PET e nessuna schermatura aggiuntiva di proiettili o aree. La navigazione usa il volume residuo, con la normale distanza di sicurezza dell'agente.
- Durata, CD, dimensioni nominali e portata di piazzamento restano invariati.

## File

- `Assets/PreGameplayLoop/BarrierGeometry.cs` e `.meta`: contorno condiviso, campionamento angolare e angoli delle coperture, rimozione dei punti collineari.
- `PG01AbilityRuntime.cs`, `BarrierEffect.cs`, `PG04AreaVisual.cs`: anteprima, verifica PG, collider, grafica, durata e rimozione.
- `Assets/TestEngine/TestObstacle.cs`, `AttackGeometry.cs`, `TestNavigation.cs`: supporto della forma poligonale nelle collisioni, nella schermatura e nella navigazione; percorso esistente dei muri rettangolari conservato.
- `Assets/PreGameplayLoop/BonusMine.cs`: ricerca delle posizioni libere aggiornata al contorno effettivo.
- `Editor/BarrierMaskValidation.cs` e `.meta`, `Editor/AbilityValidation.cs`, `Editor/AreaEffectsValidation.cs`: verifiche mirate e adattamento dei controlli al collider poligonale.
- `Docs/ROG_ZOMBIE_GDD.md`: specifica aggiornata.

## Verifica effettiva

Compilazione runtime ed Editor riuscita. Restano i due warning CS0252 preesistenti in PG05Validation.cs (317 e 320). Controllo diff senza errori di spaziatura.

La verifica Unity non è stata completata. La finestra principale non è disponibile; l'Editor batch avviato nella copia temporanea `Builds/BarrierMaskValidationProject` è fallito nell'inizializzazione della licenza e nella connessione al Licensing Client. Il processo di prova è stato terminato. Log: `Builds/BarrierMask-batch-r1.log`. Nessun risultato PASS di queste nuove prove viene dichiarato.

Il verificatore batch controlla muri/ostacoli, rotazioni, porzioni libere e schermate, collider, navigazione, movimento, schermatura delle altre aree, forma completamente mascherata e durata. Restano da eseguire queste prove e una verifica visiva/interattiva dell'anteprima in Unity. Il contorno usa una discretizzazione angolare con campioni aggiuntivi presso gli angoli delle coperture; richiede conferma in engine anche nei contatti molto sottili o tangenti.

Nessuna build del gioco, commit o push.
