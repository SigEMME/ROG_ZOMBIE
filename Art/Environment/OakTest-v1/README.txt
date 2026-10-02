QUERCIA TEST 01 — REVISIONE TOP-DOWN
Modello singolo originale: tronco e rami poligonali, chioma su soli piani.
180 triangoli legno + 24 piani fogliame (48 triangoli) = 228 triangoli.
Altezza artistica provvisoria 6,14 m, chioma larga circa 6,72 m.
Y verticale, base sul terreno, origine al piede del tronco.
Due materiali URP/Lit: Bark opaco, Foliage alpha clipping 0,5,
doppia faccia e ZWrite attivo. Instancing abilitato.
Texture Point con mipmap: Bark 128 px, Foliage 256 px;
sul fogliame conservazione della copertura alpha.
PNG sorgenti mantenuti alla risoluzione generata.

UNITY
Asset: Assets/Environment/OakLowPolyTest.
Trascinare Quercia_Test_01.obj in scena oppure usare:
ROG ZOMBIE > Art > Crea prefab quercia di test.
Il comando non sovrascrive prefab esistenti.
Nessun collider, vento, animazione o script runtime.
Nessuna modifica a scene, package o Project Settings.
Per importare lo ZIP estrarre la struttura Assets nella radice del progetto.
Per altri software: OBJ, Tree.mtl e PNG insieme; configurare alpha clipping
e materiale double-sided manualmente se necessario.

VERIFICA
Anteprima renderizzata dal modello OBJ con le sue UV e texture reali,
non una cattura Unity. Confronto: inclinazione 55 gradi / piani / verticale 90 gradi. Piani del fogliame rivolti verso alto: uno orizzontale e due inclinati per gruppo. Rimossi i piani verticali; tronco e rami invariati.
Triangoli non degeneri verificati durante generazione.
Comando Editor compilato. Resa, luci e ombre in Unity ancora da verificare.
Nessuna build o commit.

TEXTURE
Corteccia riutilizzata dall'abete. Fogliame creato con image_gen integrato.
Prompt:
A single oak foliage cluster texture for a flat game foliage card. Dense
rounded irregular oval cluster of broad lobed OAK LEAVES, clearly deciduous
leaves not pine needles. Entire cluster centered occupying 90 percent of
the square. Scalloped leafy perimeter with small transparent holes and
protruding leaves, TRUE transparent background outside and between gaps.
Retro 32-bit game pixel art, bold readable pixel clusters, limited muted
olive and forest green palette with warm sage highlights, equivalent to
128x128 pixel artwork enlarged nearest-neighbor. Flat orthographic foliage
texture, light only inside foliage, no surrounding shadows, no glow, no
text, no trunk, no whole tree, no branches protruding. Overall low contrast
texture with subtle center shade and lighter leaf tips. Designed for
alpha-clipped double sided planes assembled into a broad rounded oak canopy.