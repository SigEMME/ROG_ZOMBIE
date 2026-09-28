using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie.TopDown
{
    // Isolated visual study: XY remains the ground plane, negative Z represents height.
    public sealed class TopDownStudy : MonoBehaviour
    {
        public Shader StudyShader;
        public float CameraHeight = 36, FieldOfView = 50, DemoMoveSpeed = 6;
        public Camera View { get; private set; }
        public Transform Player { get; private set; }
        public bool Perspective => View != null && !View.orthographic;
        public Vector2 AimPoint { get; private set; }
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Rect> footprints = new List<Rect>();
        private Vector2 input;
        private Vector3 cameraVelocity;
        private Material concrete, trim, asphalt, paint, roof;
        private Transform aim;
        private const float Radius = .35f;

        private void Start()
        {
            if (StudyShader == null || !StudyShader.isSupported) { Debug.LogError("Top-down study shader unavailable."); enabled=false; return; }
            var cameraObject = new GameObject("Study camera"); cameraObject.transform.SetParent(transform);
            View=cameraObject.AddComponent<Camera>(); View.tag="MainCamera";
            View.clearFlags=CameraClearFlags.SolidColor; View.backgroundColor=new Color(.055f,.065f,.07f);
            View.nearClipPlane=.1f; View.farClipPlane=150; View.fieldOfView=FieldOfView;
            concrete=Mat(new Color(.42f,.4f,.34f),true); trim=Mat(new Color(.24f,.28f,.28f));
            asphalt=Mat(new Color(.13f,.15f,.16f)); paint=Mat(new Color(.75f,.69f,.45f)); roof=Mat(new Color(.32f,.36f,.36f));
            Box("Ground",new Vector3(0,0,.15f),new Vector3(54,64,.2f),trim);
            Box("Street",new Vector3(0,0,0),new Vector3(9,64,.08f),asphalt);
            for(int y=-30;y<=30;y+=4) Box("Road marking",new Vector3(0,y,-.06f),new Vector3(.12f,1.8f,.02f),paint);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Sidewalk",new Vector3(side*5.1f,0,-.08f),new Vector3(1.2f,64,.15f),Mat(new Color(.47f,.48f,.43f)));
                for(int y=-30;y<=30;y+=2) Box("Paving joint",new Vector3(side*5.1f,y,-.17f),new Vector3(1.2f,.035f,.01f),trim);
            }
            Building(new Vector2(-11,3),new Vector2(10,12),5);
            Building(new Vector2(11,3),new Vector2(10,12),10);
            var actor=new GameObject("PG study - no gameplay stats"); actor.transform.SetParent(transform); Player=actor.transform;
            actor.AddComponent<CircleCollider2D>().radius=Radius;
            var body=Box("Torso",new Vector3(0,0,-.55f),new Vector3(.64f,.45f,.65f),Mat(new Color(.25f,.62f,.4f))); body.transform.SetParent(Player,false);
            var head=Box("Head",new Vector3(0,.04f,-1.05f),new Vector3(.34f,.34f,.3f),Mat(new Color(.78f,.64f,.46f))); head.transform.SetParent(Player,false);
            var gun=Box("Aim direction",new Vector3(0,.55f,-.5f),new Vector3(.13f,.65f,.15f),trim); gun.transform.SetParent(Player,false);
            aim=Box("Ground cursor",new Vector3(0,0,-.09f),new Vector3(.2f,.2f,.025f),paint).transform;
            ResetView();
        }
        private Material Mat(Color color,bool facade=false)
        { var m=new Material(StudyShader); m.SetColor("_BaseColor",color); m.SetFloat("_Facade",facade?1:0); materials.Add(m); return m; }
        private GameObject Box(string name,Vector3 center,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(transform,false);
            go.transform.localPosition=center; go.transform.localScale=size; Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().sharedMaterial=material; return go;
        }
        private void Building(Vector2 center,Vector2 size,float height)
        {
            footprints.Add(new Rect(center-size/2,size));
            var go=Box("Building "+height+"m",new Vector3(center.x,center.y,-height/2),new Vector3(size.x,size.y,height),concrete);
            // Collider is the footprint at ground level, never the projected roof silhouette.
            var collider=new GameObject("Ground footprint"); collider.transform.SetParent(transform); collider.transform.position=center;
            collider.AddComponent<BoxCollider2D>().size=size;
            Box("Roof",new Vector3(center.x,center.y,-height-.08f),new Vector3(size.x,size.y,.16f),roof);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Parapet",new Vector3(center.x+side*(size.x/2-.12f),center.y,-height-.35f),new Vector3(.24f,size.y,.6f),trim);
                Box("Parapet",new Vector3(center.x,center.y+side*(size.y/2-.12f),-height-.35f),new Vector3(size.x,.24f,.6f),trim);
            }
            Box("Rooftop ventilation",new Vector3(center.x-1,center.y+2,-height-.5f),new Vector3(2,2,1),trim);
            for(int i=0;i<5;i++) Box("Vent grille",new Vector3(center.x-1.8f+i*.4f,center.y+2,-height-1.02f),new Vector3(.12f,1.8f,.04f),roof);
        }
        public bool CanStand(Vector2 point)
        {
            if(Mathf.Abs(point.x)>25 || Mathf.Abs(point.y)>30) return false;
            foreach(var rect in footprints)
            { Vector2 closest=new Vector2(Mathf.Clamp(point.x,rect.xMin,rect.xMax),Mathf.Clamp(point.y,rect.yMin,rect.yMax)); if((point-closest).sqrMagnitude<Radius*Radius) return false; }
            return true;
        }
        public void Move(Vector2 delta)
        {
            // Small steps avoid tunnelling even when testing unusually low frame rates.
            int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.15f)); delta/=steps;
            for(int i=0;i<steps;i++)
            { Vector2 p=Player.position; if(CanStand(p+new Vector2(delta.x,0))) p.x+=delta.x; if(CanStand(p+new Vector2(0,delta.y))) p.y+=delta.y; Player.position=p; }
        }
        public void ToggleProjection()
        { View.orthographic=!View.orthographic; View.orthographicSize=CameraHeight*Mathf.Tan(FieldOfView*.5f*Mathf.Deg2Rad); }
        public void ResetView()
        { Player.position=new Vector3(0,-2,0); View.transform.SetPositionAndRotation(new Vector3(0,-2,-CameraHeight),Quaternion.identity); cameraVelocity=Vector3.zero; }
        public bool GroundPoint(Vector2 screen,out Vector3 point)
        { var ray=View.ScreenPointToRay(screen); var plane=new Plane(Vector3.forward,Vector3.zero); bool hit=plane.Raycast(ray,out float t); point=hit?ray.GetPoint(t):Vector3.zero; return hit; }
        private void Update()
        {
            if(View==null || Player==null || !Application.isFocused) return;
            var k=Keyboard.current; input=Vector2.zero;
            if(k!=null)
            {
                input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
            }
            Move(Vector2.ClampMagnitude(input,1)*DemoMoveSpeed*Time.deltaTime);
        }
        private void LateUpdate()
        {
            if(View==null || Player==null) return;
            View.transform.position=Vector3.SmoothDamp(View.transform.position,new Vector3(Player.position.x,Player.position.y,-CameraHeight),ref cameraVelocity,.15f);
            if(Mouse.current!=null && GroundPoint(Mouse.current.position.ReadValue(),out var point))
            {
                AimPoint=point; aim.position=new Vector3(point.x,point.y,-.09f);
                Vector2 direction=AimPoint-(Vector2)Player.position;
                if(direction.sqrMagnitude>.001f) Player.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            }
        }
        private void OnGUI()
        {
            // Handle demo shortcuts as GUI events, including short key presses in Game View.
            var e = Event.current;
            if (View != null && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Tab) { ToggleProjection(); e.Use(); }
                else if (e.keyCode == KeyCode.R) { ResetView(); e.Use(); }
            }
            GUI.Box(new Rect(12,12,520,90),"ROG ZOMBIE | STUDIO TOP-DOWN");
            GUI.Label(new Rect(24,38,500,22),"WASD: movimento | Mouse: mira | TAB: prospettiva / ortografica | R: reset");
            GUI.Label(new Rect(24,62,500,24),(Perspective?"PROSPETTIVA":"ORTOGRAFICA")+" | Palazzi: 5 m / 10 m | Demo grafica, senza combattimento");
        }
        private void OnDestroy() { foreach(var m in materials) if(m!=null) Destroy(m); }
    }
}
