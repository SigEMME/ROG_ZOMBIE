ROG ZOMBIE - EDIFICI RURALI v3
Tre modelli scenografici: Fienile_01_Annesso, Rimessa_02_Rurale, Casa_03_Abbandonata.
Stile comune: fattoria consumata, legno sbiadito e lamiera arrugginita.
Reference principali fornite dall'utente:
b86ea17e16fe48dfb6de2dca8e820895.jpg - tetto spezzato e annesso del fienile.
ebdceabc983962147d9a73adfe4d85a1.jpg - volume compatto della rimessa.
86af7808943207a999b96a2061c42abb.jpg - guida prioritaria per palette e invecchiamento.
Non sono inclusi campi, animali o recinzioni della reference.

MODELLI / DIMENSIONI X,Y,Z IN METRI
Fienile_01_Annesso: 486 triangoli; 9.020 x 7.555 x 8.620.
Rimessa_02_Rurale: 292 triangoli; 5.067 x 4.905 x 6.620.
Casa_03_Abbandonata: 358 triangoli; 5.866 x 5.440 x 7.620.
Dimensioni artistiche provvisorie, modificabili dalla scala in Unity.
Asse Y verticale, pivot al suolo sotto il corpo principale.
Geometrie esterne con porte e finestre chiuse; nessun interno navigabile.
Nessun collider, animazione, distruzione o gameplay.
Nessun inserimento automatico nelle scene.

OTTIMIZZAZIONE
OBJ triangolati con UV planari, normali esplicite, Read/Write disabilitato.
Due materiali condivisi: FarmAtlas URP/Lit opaco e Dark per vetri scuri.
Un atlas importato a massimo 512 px, Point, mipmaps, no compressione.
Coperture con spessore, UV orientate lungo le falde.
Materiali opachi e instancing abilitato. Nessuna texture normale o trasparenza.

UNITY
Asset gia' in Assets/Environment/FarmBuildingsLowPoly.
Attendere importazione, poi ROG ZOMBIE > Art > Crea prefab edifici rurali.
Il menu crea i nove prefab nella stessa cartella senza sovrascrivere quelli esistenti.
Trascinare i prefab nella scena. E' possibile trascinare direttamente anche gli OBJ.
Per trasferirli a un progetto URP copiare Assets dal pacchetto ZIP.
Controllare l'aspetto con la camera top-down del proprio progetto.

VERIFICA
Indici, UV, normali, triangoli e base dei modelli verificati.
Utility C# compilata con riferimenti Unity.
Edifici_preview.png mostra render software dei nove OBJ reali a 48 gradi.
Non e' uno screenshot Unity; importazione e resa nell'Editor da confermare.
Anteprima v3 a scala comune: stessa conversione metri/pixel per tutti i nove modelli.
Nessun commit o build. Nessun package, impostazione, GDD o scena modificati.

TEXTURE
Generata tramite image_gen incorporato, nessuna modifica raster successiva.
Originale conservato: exec-a5d3c9b2-642c-46b0-b363-c9056547a823.png.
Prompt:
Single square opaque game texture atlas, four EXACT equal quadrants at 50% width and height. Coherent gritty rustic abandoned farm aesthetic, retro 32-bit pixel art, chunky deliberate pixels and limited muted earth palette, NOT photorealistic. TOP LEFT: weathered desaturated reddish brown vertical timber planks with peeling paint, gray exposed grain, board seams. TOP RIGHT: very worn pale gray beige wooden horizontal clapboard siding with scratches and dark grain. BOTTOM LEFT: old gray corrugated sheet metal with large irregular patches of burnt sienna rust, narrow parallel vertical corrugations, flat face on, no holes. BOTTOM RIGHT: rough gray brown stone foundation masonry with large simple stones and mortar seams. Each quadrant fills its exact square with no margin. Flat albedo even lighting, no perspective, no objects, no labels, no lettering, no borders, no baked cast shadows. All four swatches visually belong to the same aged rural buildings in a top-down zombie game. Effective pixel detail 128x128 per quadrant; keep clear large material shapes, avoid fine noise.

REVISIONE v2
Texture tetto continua sull'intera copertura principale: non riparte piu ad ogni striscia.
Scala uniforme nello spazio UV, con un solo passaggio nel riquadro della lamiera per copertura.
Nella v2 le varianti erano cromatiche; nella v3 hanno geometrie e dimensioni differenti.
Due varianti per ciascuno dei tre edifici, suffissi:
_Salvia: pareti verde salvia, tetto grigio scuro ossidato.
_Azzurro: pareti azzurro polvere, tetto rosso ossidato.
Le varianti usano due nuovi atlas condivisi e mantengono il materiale Dark comune.
GUID originali preservati. Eseguire nuovamente il menu per creare i nuovi prefab.
Gli OBJ originali si aggiornano con la reimportazione.
Per rigenerare: build_models.py, build_variants.py, render_preview.py, validate_export.py.

PROVENIENZA TEXTURE VARIANTI
image_gen incorporato; reference FarmAtlas.png originale; nessuna elaborazione raster successiva.
Salvia: exec-67538f88-f113-482f-9dfc-1420f06bfec2.png
Prompt:
Create a color variant of this exact 2x2 texture atlas for the same low-poly rustic buildings. Preserve exact four equal quadrant boundaries at 50%, the pixel art style, material types, grain directions, flat lighting, and opaque square format. TOP LEFT change reddish wood paint to muted sage green with aged gray exposed wood. TOP RIGHT change pale horizontal siding to lighter faded sage green. BOTTOM LEFT change rusty metal to charcoal gray oxidized metal with subtle broad patches of brown rust; use only about 6 broad vertical corrugations across this quadrant, not many thin repeated stripes. BOTTOM RIGHT keep original gray brown masonry. Cohesive desaturated abandoned farm palette, distinct green wood and charcoal roof, large readable pixel clusters. No text borders objects shadows or perspective.

Azzurro: exec-4f198034-6d17-42fc-b65c-128539749ccb.png
Prompt:
Create a second color variant of this exact 2x2 game texture atlas, same aged farm pixel art style. Preserve exact equal quadrant boundaries at 50%, flat albedo lighting, material types and grain directions, opaque square. TOP LEFT muted dusty blue painted vertical wood, worn pale gray exposed grain. TOP RIGHT lighter faded powder blue horizontal clapboard wood. BOTTOM LEFT oxidized terracotta dark red corrugated metal roof, broad irregular rust stains and exposed muted gray patches, about 6 broad vertical corrugations across quadrant, avoid dense thin stripes. BOTTOM RIGHT keep original gray brown stone foundation. Desaturated weathered coherent retro 32-bit pixel art; large readable pixel clusters. No text borders perspective objects or cast shadows.

REVISIONE v3 - DIMENSIONI ARCHITETTONICHE
Originali invariati. Varianti ricostruite con campate aggiuntive, aperture di misura costante, annessi/verande e corpi principali piu ampi. Nessun rescale del modello. Importazione sempre scala 1.
Fienile_01_Annesso: 486 triangoli; X/Y/Z 9.02 x 7.555 x 8.62 m. Modello originale.
Rimessa_02_Rurale: 292 triangoli; X/Y/Z 5.067 x 4.905 x 6.62 m. Modello originale.
Casa_03_Abbandonata: 358 triangoli; X/Y/Z 5.866 x 5.44 x 7.62 m. Modello originale.
Fienile_01_Annesso_Salvia: 628 triangoli; X/Y/Z 11.562 x 7.557 x 12.62 m. 4 campate, corpo e annesso ampliati.
Rimessa_02_Rurale_Salvia: 332 triangoli; X/Y/Z 7.251 x 4.905 x 10.62 m. 3 campate e maggiore superficie di deposito.
Casa_03_Abbandonata_Salvia: 456 triangoli; X/Y/Z 7.654 x 5.43 x 11.31 m. 3 campate, veranda e nuove finestre.
Fienile_01_Annesso_Azzurro: 728 triangoli; X/Y/Z 13.558 x 7.558 x 16.62 m. 5 campate, due portoni, annesso lungo.
Rimessa_02_Rurale_Azzurro: 480 triangoli; X/Y/Z 9.441 x 4.906 x 15.61 m. 4 campate, due portoni, tettoia di carico.
Casa_03_Abbandonata_Azzurro: 556 triangoli; X/Y/Z 9.445 x 5.43 x 14.81 m. 4 campate, veranda ampia, due camini.
Dimensioni artistiche provvisorie. Nessun interno navigabile o collider aggiunto. Meta e riferimenti esistenti conservati.
