STACCIONATE LOW POLY — v1

Contenuto: quattro modelli OBJ originali ispirati ai riferimenti.
Piena 204 triangoli; Aperta 132; Rinforzata 132; Danneggiata 228.
Ogni OBJ contiene una mesh triangolata, UV e normali piatte.
Un materiale URP/Lit opaco condiviso; instancing abilitato.
WoodPixel.png: sorgente generata, importazione Unity limitata a 128 pixel,
filtro Point, mipmap attive, nessuna normal map o trasparenza.
La risoluzione del PNG sorgente è superiore a quella usata in Unity.

Scala artistica provvisoria: larghezza 4 m, altezza massima 1,85 m.
Asse verticale Y; pannello lungo X; spessore lungo Z; pivot alla base.
MeshCollider 3D statici non convex aderenti alla mesh, non trigger, senza Rigidbody. Nessuno script runtime o modifica alle scene.
Non costituisce una definizione delle dimensioni degli ostacoli di gameplay.

USO IN UNITY
1. Attendere l'importazione di Assets/Environment/FencesLowPoly.
2. I quattro OBJ possono essere trascinati direttamente nella scena.
3. Per prefab separati: ROG ZOMBIE > Art > Crea prefab staccionate low poly.
4. I prefab vengono creati nella sottocartella Prefabs; quelli esistenti
   ricevono solo i collider mancanti, preservando le modifiche artistiche.
5. Se il materiale automatico dell'OBJ non è risolto, assegnare Wood.mat;
   il comando prefab assegna esplicitamente questo materiale.
6. Usare luce 3D con il materiale Lit. Ridimensionare nel Transform.

VERIFICA
Indici e triangoli non degeneri verificati; normali coerenti.
Comando Editor compilato con le librerie Unity del progetto.
Importazione, resa della texture e prefab da verificare nell'Editor.
models-geometry-preview.png mostra la geometria a colore uniforme,
non è una cattura di Unity né un'anteprima della texture finale.
Nessuna build o commit.