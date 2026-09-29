# MERCHANT — prototipo locale

Schermata basata su `MERCHANT_Test UI.jpg` e legenda del proprietario. Saldo G arancione, ritratto provvisorio nel pannello rosa, riquadro giallo predisposto per dialoghi, quattro slot verdi, INDIETRO grigio e sette righe nel catalogo blu con anteprima/prezzo/acquisto. Le sette icone ITEMS usano i PNG al 50% (627×627) in Assets/Resources/MerchantItems, nel catalogo e negli slot del PLAYER. MOLOTOV usa la prima versione approvata. Importazione Point, senza mipmap o compressione; proporzioni e trasparenza preservate. Il riquadro dialoghi mostra per ora soltanto l'esito delle transazioni, senza dialoghi narrativi inventati.

Nell'HUB di prova raggiungere MERCHANT sulla sinistra e premere F. Il movimento si ferma mentre la schermata è aperta. INDIETRO conserva saldo e contenuto degli slot.

## Economia del test

- 5000 G iniziali per sessione, come richiesto. Chiudere e riavviare il gioco/Play Mode crea una nuova sessione: nessun salvataggio permanente è implementato.
- QUEST rimandate; rimangono disponibili tutti i sette ITEMS e i quattro slot del prototipo precedente. Non sono upgrade acquistati.
- Prezzi dal GDD: MOLOTOV, GRANATA, SMOKE, TRAPPOLA 50 G; POZIONE CURATIVA e BOMBA VELENOSA 100 G; MINA ELETTRICA 150 G.
- Ogni click su ACQUISTA compra una unità nel primo slot disponibile, scandito da sinistra a destra. Uno slot occupato è disponibile solo per lo stesso ITEM se SCORTA ESPLOSIVA consente ancora di aggiungerlo. Uno slot vuoto precedente ha priorità.
- Click su uno slot occupato mostra il pulsante di vendita sullo slot. Ogni click su quel pulsante vende una sola unità al 50% del prezzo. L'ultima unità lascia lo slot vuoto.
- Acquisto bloccato se manca spazio o G. La PREPARAZIONE consente soltanto riordino tramite trascinamento; il selettore gratuito è rimosso dalla UI.
- Cambiando PG/PASSIVA e perdendo SCORTA ESPLOSIVA, gli eccessi vengono venduti al 50% e accreditati immediatamente, con avviso visibile.

## Limiti espliciti

Il saldo è quello del negozio nella sessione locale, non un nuovo sistema completo di progressione o un portafoglio multiplayer. La contabilizzazione del G guadagnato nella RUN e la conservazione degli ITEMS dopo BOSS restano da integrare: il prototipo termina a un confine di test, che `LoopSession` distingue esplicitamente da una vittoria. Il rientro dalla RUN continua a svuotare gli ITEMS come nel precedente test.

L'HUB esistente è una mappa IMGUI con posizione logica del PLAYER. Il MERCHANT ha ingombro logico non attraversabile nella mappa; la realizzazione dell'HUB fisico con NPC su OSTACOLO e TRIGGER_PG resta da completare. Non è stata trasformata la mappa in una nuova scena di gameplay.

## Verifica manuale in Unity

1. Aprire la scena HUB e avviare Play Mode; raggiungere MERCHANT a sinistra con WASD e premere F.
2. Verificare disposizione dei pannelli, saldo 5000 G e quattro slot vuoti.
3. Acquistare una GRANATA: saldo 4950 G, una unità nel primo slot.
4. Cliccare lo slot: appare la vendita ma saldo e quantità restano invariati. Cliccare il pulsante di vendita: saldo 4975 G, slot vuoto.
5. Riempire i quattro slot senza SCORTA: ulteriori acquisti sono disabilitati. INDIETRO e riapertura conservano il saldo.
6. Nella preparazione selezionare PG04 con SCORTA ESPLOSIVA; tornare al MERCHANT e acquistare tre GRANATE nello stesso slot. Ogni vendita rimuove una sola unità.
7. Cambiare PG/PASSIVA: verificare vendita degli eccessi, rimborso e avviso. In preparazione trascinare gli ITEMS per riordinarli; un click non apre più il selettore gratuito.

Validazione svolta: compilazione C# esterna con riferimenti Unity, senza errori. Warning relativi a campi serializzati già esistenti. Nessun test di gameplay o verifica visiva in Play Mode eseguiti; nessuna build esportata.
