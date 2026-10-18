using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class GrassPrefabSetup
    {
        public const string Root = "Assets/C1/Environment/GrassLowPoly";
        public static readonly string[] Names = { "Erba_01_Bassa", "Erba_02_Aperta", "Erba_03_Alta" };

        [MenuItem("ROG ZOMBIE/Art/Crea ciuffi erba C1")]
        public static void Create()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Grass.mat");
            if (material == null)
            {
                var shader = Shader.Find("ROG ZOMBIE/Grass Low Poly");
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/C1/Terrain/Textures/Texture_Prato_Chiaro.png");
                if (shader == null || texture == null) throw new InvalidOperationException("Shader URP o texture prato mancanti.");
                material = new Material(shader) { name = "Grass", enableInstancing = true };
                material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", new Color(.85f, .95f, .7f, 1));
                AssetDatabase.CreateAsset(material, Root + "/Grass.mat");
            }
            for (int variant = 0; variant < Names.Length; variant++)
            {
                string name = Names[variant];
                if (AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".prefab") != null) continue;
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/" + name + ".asset");
                if (mesh == null)
                {
                    mesh = CreateMesh(variant);
                    mesh.name = name;
                    AssetDatabase.CreateAsset(mesh, Root + "/" + name + ".asset");
                }
                var go = new GameObject(name);
                try
                {
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = true;
                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    PrefabUtility.SaveAsPrefabAsset(go, Root + "/" + name + ".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
        }

        // Bent, tapered ribbons have a visible footprint even with a vertical camera.
        // Both sides are rendered by the material, without transparent overlapping cards.
        private static Mesh CreateMesh(int variant)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            int blades = 7 + variant * 2;
            float height = .15f + variant * .1f;
            var random = new System.Random(710 + variant);
            for (int blade = 0; blade < blades; blade++)
            {
                float angle = (blade * 137.5f + variant * 19) * Mathf.Deg2Rad;
                var outward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var across = new Vector3(-outward.z, 0, outward.x);
                float h = height * (blade == 0 ? 1 : .6f + .4f * (float)random.NextDouble());
                float spread = h * (variant == 1 ? .95f : .65f);
                float width = .035f + variant * .008f;
                var origin = outward * (.012f + .016f * (float)random.NextDouble());
                int start = vertices.Count;
                for (int row = 0; row < 3; row++)
                {
                    float t = row * .36f;
                    var center = origin + Vector3.up * (h * t) + outward * (spread * t * t);
                    float half = width * .5f * (1 - t * .75f);
                    vertices.Add(center - across * half);
                    vertices.Add(center + across * half);
                    uv.Add(new Vector2(.35f, .25f + t * .3f));
                    uv.Add(new Vector2(.55f, .25f + t * .3f));
                }
                vertices.Add(origin + Vector3.up * h + outward * spread);
                uv.Add(new Vector2(.45f, .55f));
                for (int row = 0; row < 2; row++)
                {
                    int a = start + row * 2;
                    triangles.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                }
                triangles.AddRange(new[] { start + 4, start + 6, start + 5 });
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static DetailPrototype Prototype(GameObject prefab)
        {
            return new DetailPrototype
            {
                prototype = prefab, usePrototypeMesh = true, useInstancing = true,
                renderMode = DetailRenderMode.VertexLit,
                minWidth = .85f, maxWidth = 1.15f, minHeight = .85f, maxHeight = 1.15f,
                healthyColor = Color.white, dryColor = Color.white,
                noiseSpread = .2f, holeEdgePadding = 1
            };
        }

        [MenuItem("ROG ZOMBIE/Art/Aggiungi ciuffi C1 al Terrain selezionato")]
        public static void AddToTerrain()
        {
            var terrain = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (terrain == null || terrain.terrainData == null)
            {
                Debug.LogWarning("Seleziona il GameObject Terrain nella gerarchia.");
                return;
            }
            Create();
            var data = terrain.terrainData;
            var details = new List<DetailPrototype>(data.detailPrototypes);
            foreach (string name in Names)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".prefab");
                if (!details.Exists(p => p.prototype == prefab)) details.Add(Prototype(prefab));
            }
            bool initializeDetails = data.detailResolution == 0;
            if (details.Count == data.detailPrototypes.Length && !initializeDetails) return;
            Undo.RegisterCompleteObjectUndo(data, "Aggiungi ciuffi C1");
            // A Terrain created without details has a zero-sized density map: painting
            // silently does nothing. Never resize an existing map, which would erase it.
            if (initializeDetails) data.SetDetailResolution(512, 16);
            if (details.Count != data.detailPrototypes.Length) data.detailPrototypes = details.ToArray();
            EditorUtility.SetDirty(data);
            Debug.Log("Ciuffi C1 disponibili in Paint Details. La vegetazione gia dipinta e conservata.");
        }
    }
}
