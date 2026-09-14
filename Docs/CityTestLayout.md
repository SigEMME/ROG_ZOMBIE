# Layout città per le due AREE del prototipo

Reference approvata: `C:/Users/matti/Documents/ROG ZOMBIE/REFERENCE/LAYOUT-CITY_test.png`.

## Configurazione

`Assets/PreGameplayLoop/CityTestLayout.asset` è il nuovo GeometryTemplate di `PreGameplayLoop.asset`. Entrambe le AREE, anche partendo dall’HUB, usano lo stesso layout 100 × 100 m. La scena tecnica TestEngine conserva il proprio template precedente.

La scala deriva dal quadrato interno della reference: x274–884, y18–628, 610 pixel equivalenti a 100 m. Il centro del mondo corrisponde a (579,323) nell’immagine; l’asse verticale dell’immagine è invertito nel mondo Unity. Misure e posizioni sono una ricostruzione del disegno, non quote esecutive.

- 24 rettangoli MURO blu compongono gli edifici, incluse le forme concave a U e a L.
- 42 OSTACOLI rossi compongono auto, recinzioni e barriere. Le auto inclinate mantengono la rotazione anche nei collider e nella NavMesh.
- SPAWN PG: (-0,3; -46,4) m, indicato in verde.
- USCITA: (-1,6; 45,5) m, indicata in giallo; lieve adattamento rispetto al centro del cerchio disegnato per contenere il trigger esistente di raggio 4 m. Restano l’apertura a fine AREA e l’indicazione verde quando disponibile.
- Pavimento con griglia 1 × 1 m, mantenuta nelle due AREE. Popolazioni 100/120, regole di spawn, progressione, abilità e valori dei PG preservati.

Il riepilogo visivo della geometria è `Docs/CityTestLayout.svg`.

## Implementazione

Aggiunta Rotation alle singole ObstaclePlacement (default 0 per i dati precedenti). LoopSession applica posizione, dimensioni, categoria e rotazione. TestEngine supporta lo stesso campo senza cambiare il suo layout salvato. TestNavigation supportava già i box ruotati.

Gli effetti suddivisi in spicchi e FILO SPINATO trasformano ora le query nel riferimento non ruotato dell’ostacolo prima dell’intersezione. Questo evita di usare l’ingombro rettangolare allargato delle auto inclinate, mantenendo le regole di blocco esistenti. Nessun aggiornamento Unity/pacchetti o modifica manuale alle cartelle generate.

## Verifica e limiti

Compilazione C# runtime riuscita senza errori o warning usando il compilatore e i riferimenti della Unity 6000.3.16f1 installata; output diagnostico separato in Builds/CityCompile. Non equivale a un’esecuzione in Play Mode.

Controllo geometrico statico superato: SPAWN e USCITA liberi e collegati su raster di 0,25 m con raggio attore 0,35 m; percorso trovato di circa 98,75 m. Report locale: Builds/City-layout-check.txt. Non è un test della NavMesh Unity né delle abilità. Il controllo NavMesh già presente nel gameplay loop segnalerà un errore se SPAWN e USCITA non risultano collegati al caricamento.

Play Mode, FIRST SPAWN, movimento/inseguimento dei MOB e resa visiva del nuovo layout restano da verificare nell’Editor. Le suite precedenti con bersagli fissi possono interferire con i nuovi edifici e vanno eseguite su geometria controllata. Nessuna build Windows generata; lo ZIP precedente non contiene il nuovo layout. Nessun commit o push.
