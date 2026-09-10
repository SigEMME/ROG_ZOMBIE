using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using RogZombie.TestEngine;

namespace RogZombie.PlayTests.EditorTools
{
    // Authoring only: no Play Mode, input simulation, tests or camera capture.
    public static class CityEnvironmentTools
    {
        const string Art="Assets/Art/Environment/CityMiniPack/";
        const string Generated=Art+"Generated/";
        const string ScenePath="Assets/Scenes/ENV_ZOMB01_PlayTest.unity";
        struct Vertex { public Vector2 Point, UV; public Vertex(Vector2 p,Vector2 uv){Point=p;UV=uv;} }

        public static void BuildBatch()
        {
            try { Build(); File.WriteAllText(Path.Combine(Path.GetTempPath(),"ROG_City_authoring.txt"),"Environment authored and scene saved. No tests or Play Mode executed."); }
            catch(Exception e){File.WriteAllText(Path.Combine(Path.GetTempPath(),"ROG_City_authoring.txt"),e.ToString());throw;}
        }

        [MenuItem("ROG ZOMBIE/Environment/Create city in saved ENV scene")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before authoring the environment.");
            for(int i=0;i<EditorSceneManager.sceneCount;i++)
                if(EditorSceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Open scene has unsaved changes; it is preserved.");
            var scene=EditorSceneManager.GetActiveScene().path==ScenePath?EditorSceneManager.GetActiveScene():EditorSceneManager.OpenScene(ScenePath);
            if(GameObject.Find("City environment")!=null)throw new InvalidOperationException("City already exists. Edit it in the scene; this command will not replace your layout.");
            var session=UnityEngine.Object.FindFirstObjectByType<ENVPlayTestSession>();
            if(session==null)throw new InvalidOperationException("ENV session is missing.");
            Import("Road_Tile",new Vector2(.5f,.5f)); Import("Sidewalk_Tile",new Vector2(.5f,.5f));
            Import("Car",new Vector2(133f/266f,47f/172f));
            Import("Concrete_Barrier",new Vector2(98f/195f,28f/153f));
            Import("Dumpster",new Vector2(88f/177f,31f/182f));
            Import("Fence",new Vector2(103f/203f,33f/178f));
            Import("Streetlight",new Vector2(42f/146f,21f/331f));
            if(!AssetDatabase.IsValidFolder(Generated.TrimEnd('/')))AssetDatabase.CreateFolder(Art.TrimEnd('/'),"Generated");
            var root=new GameObject("City environment").transform;
            var floors=new GameObject("Surfaces - clipped tile tops").transform; floors.SetParent(root,false);
            MakeFloor(floors,false);MakeFloor(floors,true);
            var props=new GameObject("Props - ground footprints").transform;props.SetParent(root,false);
            var material=session.Zombie.GetComponentInChildren<SpriteRenderer>().sharedMaterial;
            AddProp(props,"Abandoned car","Car",new Vector2(-3.8f,1.1f),.75f,material,
                new Vector2[]{new Vector2(-.8f,-.3f),new Vector2(-.1f,0),new Vector2(.7f,.35f)},
                new Vector2[]{new Vector2(.9f,.55f),new Vector2(1.1f,.7f),new Vector2(.9f,.55f)});
            foreach(var p in new[]{new Vector2(-.2f,2.3f),new Vector2(-4.5f,-2.3f)})
                AddProp(props,"Concrete barrier","Concrete_Barrier",p,.6f,material,
                    new Vector2[]{new Vector2(-.6f,.28f),Vector2.zero,new Vector2(.6f,-.28f)},
                    new Vector2[]{new Vector2(.7f,.35f),new Vector2(.7f,.35f),new Vector2(.7f,.35f)});
            AddProp(props,"Dumpster","Dumpster",new Vector2(2.8f,-2.4f),.48f,material,
                new Vector2[]{Vector2.zero},new Vector2[]{new Vector2(1.0f,.65f)});
            foreach(var p in new[]{new Vector2(3.7f,3f),new Vector2(5.6f,3.5f)})
                AddProp(props,"Fence","Fence",p,.65f,material,
                    new Vector2[]{new Vector2(-.65f,-.2f),Vector2.zero,new Vector2(.65f,.2f)},
                    new Vector2[]{new Vector2(.7f,.25f),new Vector2(.7f,.25f),new Vector2(.7f,.25f)});
            foreach(var p in new[]{new Vector2(-6f,-1f),new Vector2(5.8f,-1.8f)})
                AddProp(props,"Streetlight","Streetlight",p,.6f,material,
                    new Vector2[]{Vector2.zero},new Vector2[]{new Vector2(.24f,.2f)});
            AttachDepth(session.Player.gameObject,0f);
            AttachDepth(session.Zombie.gameObject,-.45f);
            // Keep boundary colliders and all actor/gameplay components; replace only presentation.
            foreach(var go in scene.GetRootGameObjects())
            {
                if(go.name=="Ground - technical only (environment assets missing)")UnityEngine.Object.DestroyImmediate(go);
                else if(go.name.EndsWith("boundary",StringComparison.OrdinalIgnoreCase))
                {var sr=go.GetComponent<SpriteRenderer>();if(sr!=null)sr.enabled=false;}
            }
            var camera=Camera.main;
            camera.orthographic=true;camera.orthographicSize=5.5f;
            camera.backgroundColor=new Color(.065f,.077f,.074f,1f);
            // Camera rotation and XY gameplay plane remain exactly as authored before.
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }

        static void Import(string kind,Vector2 pivot)
        {
            string path=Art+"RZ_City_"+kind+"_01.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null)throw new FileNotFoundException(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64;importer.alphaIsTransparency=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        static void AttachDepth(GameObject go,float feet)
        {
            if(go.GetComponent<SortingGroup>()==null)go.AddComponent<SortingGroup>();
            var depth=go.GetComponent<CityDepthSort>();if(depth==null)depth=go.AddComponent<CityDepthSort>();
            depth.FootOffset=feet;depth.Apply();
        }
        static void AddProp(Transform parent,string name,string kind,Vector2 position,float scale,Material material,Vector2[] offsets,Vector2[] sizes)
        {
            var root=new GameObject(name);root.transform.SetParent(parent,false);root.transform.position=position;
            var visual=new GameObject("Visual");visual.transform.SetParent(root.transform,false);visual.transform.localScale=Vector3.one*scale;
            var renderer=visual.AddComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"RZ_City_"+kind+"_01.png");
            renderer.sharedMaterial=material;renderer.spriteSortPoint=SpriteSortPoint.Pivot;
            for(int i=0;i<offsets.Length;i++)
            {
                var footprint=new GameObject("Ground footprint "+(i+1));footprint.transform.SetParent(root.transform,false);footprint.transform.localPosition=offsets[i];
                footprint.AddComponent<BoxCollider2D>().size=sizes[i];footprint.AddComponent<TestObstacle>().IsWall=false;
            }
            AttachDepth(root,0f);
        }
        static void MakeFloor(Transform parent,bool sidewalk)
        {
            string kind=sidewalk?"Sidewalk_Tile":"Road_Tile";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"RZ_City_"+kind+"_01.png");
            // Pixel coordinates measured on the supplied top faces (origin at PNG top-left).
            // Only UV/mesh geometry is cropped; source images are not edited or duplicated.
            Vector2[] pixels=sidewalk
                ?new[]{new Vector2(13,77),new Vector2(119,16),new Vector2(219,77),new Vector2(118,133)}
                :new[]{new Vector2(10,73),new Vector2(118,13),new Vector2(207,69),new Vector2(109,122)};
            var positions=new List<Vector3>();var uvs=new List<Vector2>();var indices=new List<int>();var colors=new List<Color>();
            Vector2[] diamond={new Vector2(-1.75f,0),new Vector2(0,1),new Vector2(1.75f,0),new Vector2(0,-1)};
            for(int i=-7;i<=7;i++)for(int j=-7;j<=7;j++)
            {
                if((Mathf.Abs(j)>1)!=sidewalk)continue;
                Vector2 center=new Vector2((i-j)*1.75f,(i+j));
                var polygon=new List<Vertex>();
                for(int k=0;k<4;k++)polygon.Add(new Vertex(center+diamond[k],new Vector2(pixels[k].x/texture.width,1f-pixels[k].y/texture.height)));
                polygon=Clip(polygon,Vector2.right,-8);polygon=Clip(polygon,Vector2.left,-8);
                polygon=Clip(polygon,Vector2.up,-5);polygon=Clip(polygon,Vector2.down,-5);
                if(polygon.Count<3)continue;
                int first=positions.Count;
                foreach(var v in polygon){positions.Add(new Vector3(v.Point.x,v.Point.y,.5f));uvs.Add(v.UV);colors.Add(Color.white);}
                for(int k=1;k<polygon.Count-1;k++){indices.Add(first);indices.Add(first+k);indices.Add(first+k+1);}
            }
            var mesh=new Mesh{name=kind+" clipped surface"};mesh.SetVertices(positions);mesh.SetUVs(0,uvs);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();mesh.RecalculateNormals();
            string meshPath=Generated+kind+"_Surface.asset";AssetDatabase.CreateAsset(mesh,meshPath);
            var material=new Material(Shader.Find("Sprites/Default")){name=kind+" surface",mainTexture=texture};
            AssetDatabase.CreateAsset(material,Generated+kind+"_Surface.mat");
            var go=new GameObject(kind+" surface");go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=-30000;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            AssetDatabase.SaveAssetIfDirty(mesh);AssetDatabase.SaveAssetIfDirty(material);
        }
        static List<Vertex> Clip(List<Vertex> source,Vector2 inward,float minimum)
        {
            var output=new List<Vertex>();
            for(int i=0;i<source.Count;i++)
            {
                var a=source[i];var b=source[(i+1)%source.Count];float da=Vector2.Dot(a.Point,inward)-minimum,db=Vector2.Dot(b.Point,inward)-minimum;
                if(da>=0)output.Add(a);
                if((da>=0)!=(db>=0)){float t=da/(da-db);output.Add(new Vertex(Vector2.Lerp(a.Point,b.Point,t),Vector2.Lerp(a.UV,b.UV,t)));}
            }
            return output;
        }
    }
}