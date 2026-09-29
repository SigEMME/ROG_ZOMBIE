# Multiplayer — punti da verificare

Registrato il 29/09/2026. Attività sospesa su richiesta del proprietario per riprendere la gestione ITEMS.

## Problema segnalato

- [ ] Il partecipante ha riscontrato grossi problemi di LAG durante la partita nella build CO-OP basata su v0.3.3, fra due PC in case diverse.
- [ ] Distinguere ritardo dei comandi, movimento a scatti e calo del frame rate; verificare se il ritardo cresce durante la RUN e se interessa anche l'host.

## Possibili cause individuate nel codice, da verificare

Riferimento: worktree `C:\Users\EMME\.codex\worktrees\multiplayer-v033\ROG_ZOMBIE`, cartella `Assets/MultiplayerPrototype`.

- [ ] `CoopSession.cs`: input del partecipante inviati a ogni fotogramma su canale affidabile ordinato. Verificare traffico e accumulo dei messaggi.
- [ ] `CoopVisuals.cs`: aggiornamenti visivi possono ritrasmettere geometrie complete anche quando cambia solo la posizione. Verificare banda, allocazioni e costo della ricostruzione delle mesh sul partecipante.
- [ ] `CoopVisuals.cs`: uscita/rientro nell'inquadratura causa rimozione e ricreazione delle repliche. Verificare picchi di lavoro e ritrasmissioni.
- [ ] Posizioni delle repliche e camera aggiornate senza interpolazione: verificare quanto contribuiscono agli scatti percepiti.
- [ ] `RelayConnection.cs`: verificare l'effetto delle grandi code di invio e della consegna affidabile degli aggiornamenti visivi sulla latenza.

Queste sono ipotesi tecniche supportate dalla lettura del codice, non cause confermate da misure della partita. Nessuna correzione applicata per questa segnalazione e nessun test eseguito. Non esportare build senza una nuova richiesta del proprietario.
