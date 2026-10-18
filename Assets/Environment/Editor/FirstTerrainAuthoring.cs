using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ROGZOMBIE.EnvironmentArt {
[InitializeOnLoad] public static class FirstTerrainAuthoring {
 const string Request="Art/Environment/FirstTerrainArea/export-v2.request";
 const string UpdateMenu="ROG ZOMBIE/Mappe/Salva C1_A1 e C1_A2 e aggiorna PreGameplayLoop";
 [MenuItem(UpdateMenu)]
 public static void UpdateLoopMap(){
 try{
 foreach(var path in new[]{"Assets/Scenes/C1_A1.unity","Assets/Scenes/C1_A2.unity"}){
 var source=SceneManager.GetSceneByPath(path);
 if(source.IsValid()&&source.isLoaded&&source.isDirty&&!EditorSceneManager.SaveScene(source))
 throw new InvalidOperationException("Salvataggio di "+path+" non riuscito. Mappa non aggiornata.");
 }
 Export();
 Debug.Log("C1_A1 e C1_A2 aggiornate in PreGameplayLoop. Avviare una nuova RUN per caricare le mappe.");
 if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Mappa PreGameplayLoop aggiornata"));
 }catch(Exception e){Debug.LogError("Aggiornamento mappa non riuscito: "+e.Message);}
 }
 [MenuItem(UpdateMenu,true)]
 static bool CanUpdateLoopMap()=>!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating;
 static FirstTerrainAuthoring(){EditorApplication.update+=Poll;}
 static void Poll(){
 if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
 File.Delete(Request);
 try{Export();}catch(Exception e){File.WriteAllText("Art/Environment/FirstTerrainArea/error.txt",e.ToString());}
 }
 public static void Export(){
 ExportArea(0);ExportArea(1);
 }
 public static void ExportArea(int areaIndex){
 if(areaIndex<0||areaIndex>1)throw new ArgumentOutOfRangeException(nameof(areaIndex));
 string mapName=areaIndex==0?"C1_A1":"C1_A2";
 string path="Assets/Scenes/"+mapName+".unity";
 string report="Art/Environment/FirstTerrainArea/"+mapName+"-";
 var previousScene=SceneManager.GetActiveScene();
 var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;
 if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 if(scene.isDirty)throw new InvalidOperationException("Salvare "+mapName+" prima dell'esportazione.");
 SceneManager.SetActiveScene(scene);
 var sourceRoots=scene.GetRootGameObjects();
 var root=new GameObject(mapName);
 try {
 foreach(var obj in sourceRoots){
 // Keep the source map's lighting: the loop's Global Light 2D cannot light 3D materials.
 if(obj.GetComponent<Camera>()!=null||obj.name=="Plane")continue;
 UnityEngine.Object.Instantiate(obj,root.transform);
 }
 var terrain=root.GetComponentInChildren<Terrain>();if(terrain==null)throw new Exception("Terrain mancante");
 // Roots are cloned separately: explicitly reconnect references across those roots.
 foreach(var mist in root.GetComponentsInChildren<GroundMist>(true))mist.terrain=terrain;
 var lighting=root.AddComponent<MapAmbientLighting>();
 lighting.ambient=RenderSettings.ambientLight;lighting.mode=RenderSettings.ambientMode;
 foreach(var light in root.GetComponentsInChildren<Light>())if(light.type==LightType.Directional)lighting.sun=light;
 var center=terrain.transform.position+terrain.terrainData.size*.5f;
 var data=terrain.terrainData;var alpha=data.GetAlphamaps(0,0,data.alphamapWidth,data.alphamapHeight);
 int road=Array.FindIndex(data.terrainLayers,l=>l.name.IndexOf("Asfalto",StringComparison.OrdinalIgnoreCase)>=0);
 var img=new Texture2D(data.alphamapWidth,data.alphamapHeight,TextureFormat.RGB24,false);
 for(int y=0;y<data.alphamapHeight;y++)for(int x=0;x<data.alphamapWidth;x++){
 float a=road<0?0:alpha[y,x,road];img.SetPixel(x,y,a>.4f?Color.gray:new Color(.15f,.4f,.12f));}
 img.Apply();File.WriteAllBytes(report+"roads.png",img.EncodeToPNG());UnityEngine.Object.DestroyImmediate(img);

 var materialPath="Assets/Environment/C1_Mappe/C1_Confine.mat";
 var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
 if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.35f,.36f,.38f);AssetDatabase.CreateAsset(material,materialPath);}
 Vector3[] positions={new Vector3(-data.size.x/2,1,0),new Vector3(data.size.x/2,1,0),new Vector3(0,1,-data.size.z/2),new Vector3(0,1,data.size.z/2)};
 for(int i=0;i<4;i++){var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Confine provvisorio "+i;wall.transform.SetParent(root.transform);wall.transform.position=new Vector3(center.x,terrain.transform.position.y,center.z)+positions[i];wall.transform.localScale=i<2?new Vector3(1,2,data.size.z):new Vector3(data.size.x,2,1);wall.GetComponent<Renderer>().sharedMaterial=material;}
 var definition=AssetDatabase.LoadAssetAtPath<RogZombie.PreGameplayLoop.LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
 // Validate a path using the same conservative collider footprint convention as the runtime bridge.
 var sources=new System.Collections.Generic.List<UnityEngine.AI.NavMeshBuildSource>();
 sources.Add(new UnityEngine.AI.NavMeshBuildSource{shape=UnityEngine.AI.NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(new Vector3(0,-.2f,0),Quaternion.identity,Vector3.one),size=new Vector3(data.size.x,.4f,data.size.z),area=0});
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
 var settings=UnityEngine.AI.NavMesh.GetSettingsByIndex(0);settings.agentRadius=definition.GeometryForArea(areaIndex).ActorRadius;settings.overrideVoxelSize=true;settings.voxelSize=settings.agentRadius/3;settings.minRegionArea=0;
 var nav=UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(data.size.x+4,10,data.size.z+4)),Vector3.zero,Quaternion.identity);
 var handle=UnityEngine.AI.NavMesh.AddNavMeshData(nav);
 try{
 var start=definition.TerrainStartForArea(areaIndex);var exit=areaIndex==0?definition.TerrainExit:definition.SecondTerrainExit;
 var a=new Vector3(start.x,0,start.y);var b=new Vector3(exit.x,0,exit.y);var route=new UnityEngine.AI.NavMeshPath();
 bool startOK=UnityEngine.AI.NavMesh.SamplePosition(a,out var ah,.1f,UnityEngine.AI.NavMesh.AllAreas);
 bool exitOK=UnityEngine.AI.NavMesh.SamplePosition(b,out var bh,.1f,UnityEngine.AI.NavMesh.AllAreas);
 bool connected=startOK&&exitOK&&UnityEngine.AI.NavMesh.CalculatePath(ah.position,bh.position,UnityEngine.AI.NavMesh.AllAreas,route)&&route.status==UnityEngine.AI.NavMeshPathStatus.PathComplete;
 File.WriteAllText(report+"navigation.txt",$"Spawn {start} valid {startOK}; exit {exit} valid {exitOK}; connected {connected}; obstacles {sources.Count-1}");
 if(!connected)throw new Exception("Percorso spawn/uscita non valido.");
 }finally{handle.Remove();UnityEngine.Object.DestroyImmediate(nav);}

 var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Environment/C1_Mappe/"+(mapName+"_Terreno")+".prefab");
 if(areaIndex==0)definition.FirstAreaTerrain=prefab;else definition.SecondAreaTerrain=prefab;
 definition.TerrainBackgroundShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Environment/TerrainPresentation/TerrainGameplayBackground.shader");
 EditorUtility.SetDirty(definition);AssetDatabase.SaveAssetIfDirty(definition);
 File.WriteAllText(report+"export.txt","Size "+data.size+" center "+center+" roadLayer "+road);
 }finally{UnityEngine.Object.DestroyImmediate(root);if(previousScene.IsValid()&&previousScene.isLoaded)SceneManager.SetActiveScene(previousScene);if(opened)EditorSceneManager.CloseScene(scene,true);}
 }
}}
