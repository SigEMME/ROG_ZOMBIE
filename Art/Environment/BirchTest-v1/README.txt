BETULLA TEST 01 — TOP-DOWN
Modello originale ispirato alla reference, non una scansione.
Tronco e rami poligonali: 188 triangoli.
Chioma composta soltanto da 33 piani: 66 triangoli.
Totale 254 triangoli; Y verticale, origine alla base sul terreno.
Dimensioni artistiche provvisorie: larghezza 5,57 m, altezza 7,47 m,
profondità 4,48 m. Nessun parametro di gameplay modificato.

UNITY
Assets/Environment/BirchLowPolyTest.
Trascinare Betulla_Test_01.obj in scena oppure:
ROG ZOMBIE > Art > Crea prefab betulla di test.
I prefab già presenti non vengono sovrascritti.
URP/Lit, 2 materiali: corteccia opaca, fogliame Alpha Clipping 0,5,
doppia faccia, ZWrite attivo. Instancing abilitato.
Texture Point con mipmap: corteccia 128 px, fogliame 256 px.
Preservazione copertura alpha sul fogliame; PNG sorgenti non ridimensionati.
Nessuna normal map, animazione, vento, collider o script runtime.
Nessuna modifica a scene, package o Project Settings.

ESPORTAZIONE
ZIP con struttura Assets e meta: estrarre alla radice di un progetto URP.
OBJ, Tree.mtl e due PNG disponibili anche per altri programmi.
Fuori Unity configurare alpha clipping e doppia faccia manualmente.

VERIFICA
Anteprima dai dati OBJ effettivi, texture e UV; non catturata in Unity.
Confronto: inclinata 55 gradi, geometria piani, verticale 90 gradi.
Normali del fogliame rivolte verso alto; triangoli non degeneri.
Indici, normali e conteggio triangoli verificati; comando Editor compilato.
Importazione, resa, luci e ombre nell'Editor Unity ancora da verificare.
Nessuna build o commit.

TEXTURE — TOOL IMAGE_GEN INTEGRATO
Fogliame, prompt:
Game texture foliage card, one isolated loose spray cluster of small pointed
oval serrated BIRCH leaves on TRUE transparent background. Rounded asymmetric
cluster slightly drooping, thin fine dark twigs mostly concealed, small gaps
through foliage, leafy uneven perimeter. Olive yellow green leaves, sage
highlights, deep muted green shadows. Retro 32-bit pixel art, clearly blocky
pixel clusters equivalent to 128x128 enlarged nearest neighbor, flat orthographic
texture for low poly top-down tree planes. Leaves are small teardrop ovate birch
leaves, NOT lobed oak leaves or pine needles. Fills almost all square with
transparent margins. No whole tree, no trunk, no ground, no background haze,
no cast shadows, no glow, no writing. Clean cutout alpha for alpha clipping.
Original hand painted game style, restrained contrast.

Corteccia, prompt:
Seamless tileable BIRCH BARK diffuse albedo game texture, square filled edge
to edge. Pale ivory silver grey bark with sparse short irregular horizontal
charcoal black lenticel marks and small peeling patches, recognizable white
birch. Retro 32-bit pixel art low resolution, crisp chunky pixel clusters
equivalent to 128x128 nearest-neighbor upscale, muted ivory grey sage palette,
subtle flat color variation. No vertical wood grain, no planks, no whole tree,
no 3D perspective, no lighting gradients, no cast shadow, no text, no border.
Flat evenly lit surface texture designed to wrap around a slim low poly tree
trunk and repeat seamlessly vertically and horizontally.