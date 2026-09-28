# ITEMS nel prototipo

Tutti i sette ITEMS sono sbloccati nel test. Nella PREPARAZIONE RUN, i quattro slot del PLAYER aprono un selettore gratuito con click; trascinare uno slot sopra un altro sposta/scambia tipo e quantità. PG IA senza ITEMS. Il selettore consente anche VUOTO; SCORTA ESPLOSIVA permette quantità fino a 3 per GRANATA/MOLOTOV, altrimenti il limite è 1.

La RUN copia la configurazione confermata. Tieni premuto 1/2/3/4 per l'anteprima e rilascia per usare una unità. SPACE + 1/2/3 resta riservato alle abilità dei PG IA. Pausa, perdita del focus, cambio AREA e indisponibilità del PG annullano l'anteprima senza consumo. Nessun uso dal PG IA quando viene controllato temporaneamente dopo il DOWN del PG principale.

Gli ITEMS rimanenti persistono fra AREE. Riprova test ricrea il carico iniziale della RUN. Il ritorno all'HUB da questa RUN di prova svuota gli ITEMS: è possibile riselezionarli gratuitamente. MERCHANT, economia, QUEST, sblocchi permanenti e rientro post-BOSS non sono simulati. Il cambio PG/PASSIVA riduce le quantità oltre la nuova capacità e mostra un avviso; la vendita economica degli eccessi resta fuori da questo selettore gratuito.

Implementati valori, schermatura, tick e bersagli della sezione 9 del GDD. MOLOTOV/TRAPPOLA/POZIONE emettono tick a 0,1,2… prima della scadenza; i nuovi ingressi attendono il tick dell'AREA. Le AREE sommano danni e cure come confermato; lo SLOW di TRAPPOLA è unico. SMOKE termina subito all'uscita e può tornare 1 s dopo un'azione che lo interrompe. VELENO riusa il sistema PG05 con tick ritardato e cap di cinque istanze. MINA ELETTRICA mostra trigger e raggio di esplosione, attende il MOB e poi 1 s prima di danno/STUN.

Grafica provvisoria: colori e forme geometriche. Aggiunti i due layer AREA_EFFECT_PG e AREA_EFFECT_MOB previsti dal GDD; gli oggetti ITEM non hanno collider fisici. Nessuna build o modifica della versione.

Verificatore: `.items-test.request` nella root, contenente il percorso del report, con Unity in Edit Mode e scena salvata. Esegue i test in una RUN temporanea dall'HUB.

Validazione del 28/09/2026, Unity 6000.3.16f1: ITEMS 63 controlli PASS (`ITEMS-validation-r3.txt`), gameplay loop 251 PASS (`ITEMS-loop-regression-r1.txt`), PG04/PYROMANIA 36 PASS (`ITEMS-PG04-regression-r2.txt`), schermate HUB 121 PASS (`ITEMS-hub-regression-r1.txt`). Zero warning nei verificatori ITEMS, PG04/PYROMANIA e HUB. Compilazione runtime/editor riuscita; selettore verificato anche visivamente in Play Mode.

Il vecchio verificatore completo PG04 (`ITEMS-PG04-regression-r1.txt`) passa i primi 30 controlli, inclusa SCORTA ESPLOSIVA, poi fallisce perché verifica l'impatto subito dopo lo sparo senza attendere il ritardo di 0,3 s già presente. Non è stato modificato; la regressione aggiornata PG04/PYROMANIA passa. Le prime due esecuzioni ITEMS hanno rilevato layer mancanti e sincronizzazione delle posizioni fisiche, corretti prima del PASS finale.
