ROG ZOMBIE - CASSE LOW POLY v3
Reference: C:/Users/EMME/Pictures/4.png, esclusivamente i sei esempi cerchiati.
Cinque casse e un gruppo di sacchi su pallet.
Cassa_01_Rinforzata: 154 triangoli, 1.004 x 1.002 x 1.038 m.
Cassa_02_Invecchiata: 114 triangoli, 1.0 x 0.95 x 1.038 m.
Cassa_03_Lunga: 116 triangoli, 1.65 x 0.65 x 0.858 m.
Cassa_04_Danneggiata: 196 triangoli, 1.0 x 0.85 x 0.988 m.
Cassa_05_Lunga_Rotta: 184 triangoli, 1.65 x 0.68 x 0.938 m.
Sacchi_06_Pallet: 996 triangoli, 1.4 x 1.036 x 1.055 m.
Dimensioni X/Y/Z provvisorie artistiche, scalabili in Unity, non regole di gameplay.
Pivot alla base centrato in X/Z; asse Y verticale; scala importazione 1.
Un mesh e un materiale condiviso per modello; nessuna animazione, collider o logica distruttibile.
Revisione v3: proiezione UV planare sulle cornici trapezoidali, eliminata la deformazione a zig-zag sulla diagonale dei triangoli. Venature allineate al lato lungo delle tavole. Texture originale, geometria, conteggio triangoli e meta invariati rispetto a v2.
Revisione v2: coperchi integri a filo senza fessure perimetrali, shell semplificata e UV delle travi proporzionate. Sacchi: due coricati sovrapposti e uno verticale, profilo piu morbido e collo raccolto. GUID/meta conservati: gli OBJ esistenti si aggiornano tramite reimportazione.
Le aperture delle casse danneggiate sono geometriche. Mesh triangolate, normali esplicite.
Texture in stile retro 32-bit: atlas opaco, import Unity massimo 512px, Point e mipmap.
UV con margine dai bordi delle quattro regioni. Materiale URP/Lit opaco con instancing abilitato.
La reference e' interpretata come guida artistica, non riprodotta pixel per pixel.

USO UNITY
Gli asset sono gia' in Assets/Environment/CratesLowPoly.
Attendere importazione, poi ROG ZOMBIE > Art > Crea prefab casse low poly.
Il menu crea sei prefab nella stessa cartella; non sovrascrive prefab esistenti.
Trascinare un prefab nella scena. Anche gli OBJ possono essere trascinati direttamente.
Per esportazione esterna copiare la cartella Assets contenuta nello ZIP nel progetto destinatario URP.
Nessuna scena, impostazione, pacchetto o gameplay esistente modificato.
Nessuna build o commit eseguito.

VERIFICA
Geometria, UV, normali, indici, pivot e riferimenti meta controllati.
Nessun codice C# modificato nella revisione v2. Utility gia compilata nella v1.
Controllo copertura coperchi: 2601 campioni per ciascuna cassa integra, senza buchi.
Casse_preview.png e' un render software degli OBJ effettivi con texture, non screenshot Unity.
Importazione/render e menu prefab da verificare nell'Editor Unity.
Ogni oggetto nell'anteprima e' adattato al suo riquadro: non confronto in scala.

TEXTURE - PROVENIENZA
Tool incorporato image_gen, generazione singola atlas, nessuna elaborazione raster successiva.
Sorgente: exec-70da709e-c29b-4147-8556-fe13ac892523.png, conservata.
Prompt:
Create a single square opaque albedo TEXTURE ATLAS for very low poly wooden crates and burlap sacks in a retro 32-bit era top-down game. Four EXACT equal square quadrants meeting at x=50% y=50%, no padding, no borders, no labels. TOP LEFT: warm golden brown aged wood grain, straight parallel grain running vertically, subtle scratches and knots, no large boards frames or cross braces. TOP RIGHT: muted olive brown weathered wood grain vertically, desaturated old timber, subtle grain. BOTTOM LEFT: dull dark blue gray galvanized iron, subtle hammered texture and wear, no rivets or panels. BOTTOM RIGHT: tan golden burlap woven fabric tiny pixel crosshatch. Flat evenly lit material swatches, no perspective, no cast shadows, no objects. Crisp deliberate pixel art, limited earthy palette, visible pixel clusters, effective each quadrant 128x128 pixel detail. Fill every quadrant edge to edge. This will be mapped onto real 3D crate boards, corner brackets and sack meshes. No text, no watermark.
