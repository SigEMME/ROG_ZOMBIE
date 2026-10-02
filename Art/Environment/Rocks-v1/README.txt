MASSI LOW POLY — 10 MODELLI
Modelli originali ispirati alla reference; non una scansione.
Formato OBJ, triangolato, normali piatte e UV; un materiale condiviso.
54–96 facce triangolari per modello (intervallo richiesto: 50–100). Y verticale, base sul terreno a Y=0,
origine centrata in XZ. Dimensioni artistiche provvisorie in metri.

01 Monolite          1,4 x 2,8 x 1,15
02 Lastra inclinata  2,4 x 3,1 x 0,95
03 Masso piatto      3,4 x 0,85 x 2,5
04 Cuneo             1,9 x 1,4 x 1,35
05 Blocco            1,5 x 1,3 x 1,5
06 Scheggia          0,7 x 1,8 x 0,65
07 Arrotondato       1,8 x 1,6 x 1,7
08 Allungato         3,2 x 1,2 x 1,15
09 Piramidale        1,7 x 2,2 x 1,4
10 Ciottolo          0,8 x 0,55 x 0,7
Ordine: larghezza X, altezza Y, profondità Z.

UNITY
Asset in Assets/Environment/RocksLowPoly.
Attendere l'importazione, quindi trascinare gli OBJ nella scena,
oppure ROG ZOMBIE > Art > Crea prefab massi low poly.
Il comando crea i prefab e aggiunge solo i collider mancanti ai prefab esistenti.
Stone.mat usa URP/Lit e richiede luci 3D. GPU instancing abilitato.
StonePixel.png è importata a massimo 128 px, Point, mipmap attive,
senza compressione e senza alpha. Il PNG sorgente rimane ad alta risoluzione.
MeshCollider 3D statici non convex, aderenti alla mesh, non trigger, senza Rigidbody. Nessuna normal map, animazione o logica gameplay.
Nessuna modifica a scene, package o impostazioni di progetto.

ESPORTAZIONE
Lo ZIP contiene Assets/Environment/RocksLowPoly e relativo meta:
estrarre nella radice di un progetto Unity URP mantenendo la struttura.
Per altri programmi tenere insieme OBJ, Stone.mtl e StonePixel.png.
I prefab sono generati da Unity tramite il comando, non inclusi nello ZIP.

VERIFICA
10 mesh chiuse, ogni bordo condiviso da due triangoli, nessun triangolo
degenere; orientamento globale verso l'esterno verificato.
Script Editor compilato con le librerie Unity del progetto.
Anteprima renderizzata dagli OBJ con UV e texture; non catturata in Unity.
Le dimensioni dei riquadri dell'anteprima sono normalizzate per leggibilità.
Importazione e aspetto in Unity ancora da verificare.
Nessuna build o commit.