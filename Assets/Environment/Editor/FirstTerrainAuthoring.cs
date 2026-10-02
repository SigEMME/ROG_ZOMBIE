using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ROGZOMBIE.EnvironmentArt {
[InitializeOnLoad] public static class FirstTerrainAuthoring {
 const string Request="Art/Environment/FirstTerrainArea/export-v2.request";
 static FirstTerrainAuthoring(){EditorApplication.update+=Poll;}
 static void Poll(){
 if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
 File.Delete(Request);
 try{Export();}catch(Exception e){File.WriteAllText("Art/Environment/FirstTerrainArea/error.txt",e.ToString());}
 }
 public static void Export(){
 const string path="Assets/ROG_TerrainTest/Test_140x140/Test_Terreno_140x140.unity";
 var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;
 if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 if(scene.isDirty)throw new InvalidOperationException("Salvare Test_Terreno prima dell'esportazione.");
 var root=new GameObject("AREA01_Test_Terreno");
 try {
 foreach(var obj in scene.GetRootGameObjects()){
 if(obj.GetComponent<Camera>()!=null||obj.GetComponent<Light>()!=null||obj.name=="Plane")continue;
 UnityEngine.Object.Instantiate(obj,root.transform);
 }
 var terrain=root.GetComponentInChildren<Terrain>();if(terrain==null)throw new Exception("Terrain mancante");
 var center=terrain.transform.position+terrain.terrainData.size*.5f;
 var data=terrain.terrainData;var alpha=data.GetAlphamaps(0,0,data.alphamapWidth,data.alphamapHeight);
 int road=Array.FindIndex(data.terrainLayers,l=>l.name.IndexOf("Asfalto",StringComparison.OrdinalIgnoreCase)>=0);
 var img=new Texture2D(data.alphamapWidth,data.alphamapHeight,TextureFormat.RGB24,false);
 for(int y=0;y<data.alphamapHeight;y++)for(int x=0;x<data.alphamapWidth;x++){
 float a=road<0?0:alpha[y,x,road];img.SetPixel(x,y,a>.4f?Color.gray:new Color(.15f,.4f,.12f));}
 img.Apply();File.WriteAllBytes("Art/Environment/FirstTerrainArea/roads.png",img.EncodeToPNG());UnityEngine.Object.DestroyImmediate(img);

 var materialPath="Assets/Environment/FirstTerrainArea/Boundary.mat";
 var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
 if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.35f,.36f,.38f);AssetDatabase.CreateAsset(material,materialPath);}
 Vector3[] positions={new Vector3(-70,1,0),new Vector3(70,1,0),new Vector3(0,1,-70),new Vector3(0,1,70)};
 for(int i=0;i<4;i++){var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Confine provvisorio "+i;wall.transform.SetParent(root.transform);wall.transform.position=positions[i];wall.transform.localScale=i<2?new Vector3(1,2,140):new Vector3(140,2,1);wall.GetComponent<Renderer>().sharedMaterial=material;}
 var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Environment/FirstTerrainArea/Area01_Terreno.prefab");
 var definition=AssetDatabase.LoadAssetAtPath<RogZombie.PreGameplayLoop.LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
 definition.FirstAreaTerrain=prefab;
 definition.TerrainBackgroundShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/TerrainGameplayTest/TerrainGameplayBackground.shader");
 definition.TerrainPlayerStart=new Vector2(34,-60);
 definition.TerrainExit=new Vector2(-62,33);
 EditorUtility.SetDirty(definition);AssetDatabase.SaveAssets();
 // Validate a path using the same conservative collider footprint convention as the runtime bridge.
 var sources=new System.Collections.Generic.List<UnityEngine.AI.NavMeshBuildSource>();
 sources.Add(new UnityEngine.AI.NavMeshBuildSource{shape=UnityEngine.AI.NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(new Vector3(0,-.2f,0),Quaternion.identity,Vector3.one),size=new Vector3(140,.4f,140),area=0});
 foreach(var c in root.GetComponentsInChildren<Collider>()){
 if(c is TerrainCollider||c.isTrigger||!c.enabled||c.bounds.size.y<.15f)continue;
 Bounds local;
 if(c is BoxCollider box)local=new Bounds(box.center,box.size);
 else if(c is MeshCollider mesh&&mesh.sharedMesh!=null)local=mesh.sharedMesh.bounds;
 else if(c is CapsuleCollider capsule)local=new Bounds(capsule.center,new Vector3(capsule.radius*2,capsule.height,capsule.radius*2));
 else continue;
 var v=c.transform.TransformPoint(local.center)-center;var right=c.transform.TransformVector(Vector3.right);
 float angle=Mathf.Atan2(right.z,right.x)*Mathf.Rad2Deg;
 var ext=Vector3.zero;
 foreach(var axis in new[]{Vector3.right*local.extents.x,Vector3.up*local.extents.y,Vector3.forward*local.extents.z}){
 var e=Quaternion.Euler(0,angle,0)*c.transform.TransformVector(axis);ext+=new Vector3(Mathf.Abs(e.x),Mathf.Abs(e.y),Mathf.Abs(e.z));}
 sources.Add(new UnityEngine.AI.NavMeshBuildSource{shape=UnityEngine.AI.NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(new Vector3(v.x,1,v.z),Quaternion.Euler(0,-angle,0),Vector3.one),size=new Vector3(ext.x*2,2,ext.z*2),area=1});
 }
 var settings=UnityEngine.AI.NavMesh.GetSettingsByIndex(0);settings.agentRadius=definition.GeometryForArea(0).ActorRadius;settings.overrideVoxelSize=true;settings.voxelSize=settings.agentRadius/3;settings.minRegionArea=0;
 var nav=UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(144,10,144)),Vector3.zero,Quaternion.identity);
 var handle=UnityEngine.AI.NavMesh.AddNavMeshData(nav);
 try{
 var a=new Vector3(34,0,-60);var b=new Vector3(-62,0,33);var route=new UnityEngine.AI.NavMeshPath();
 bool startOK=UnityEngine.AI.NavMesh.SamplePosition(a,out var ah,.1f,UnityEngine.AI.NavMesh.AllAreas);
 bool exitOK=UnityEngine.AI.NavMesh.SamplePosition(b,out var bh,.1f,UnityEngine.AI.NavMesh.AllAreas);
 bool connected=startOK&&exitOK&&UnityEngine.AI.NavMesh.CalculatePath(ah.position,bh.position,UnityEngine.AI.NavMesh.AllAreas,route)&&route.status==UnityEngine.AI.NavMeshPathStatus.PathComplete;
 File.WriteAllText("Art/Environment/FirstTerrainArea/navigation.txt",$"Spawn {startOK}; exit {exitOK}; connected {connected}; obstacles {sources.Count-1}");
 if(!connected)throw new Exception("Percorso spawn/uscita non valido.");
 }finally{handle.Remove();UnityEngine.Object.DestroyImmediate(nav);}

 File.WriteAllText("Art/Environment/FirstTerrainArea/export.txt","Size "+data.size+" center "+center+" roadLayer "+road);
 }finally{UnityEngine.Object.DestroyImmediate(root);if(opened)EditorSceneManager.CloseScene(scene,true);}
 }
}}
