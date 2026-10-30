using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace LightHungry.EditorTools {
    // Explicit migration command. Opening the level editor never rebuilds assets or scenes.
    public static class WatercolorWorkshop {
        const string Root="Assets/LightHungry/",Prefabs=Root+"Prefabs/";
        public static readonly string[] Palette={"Player","HungryBeast","Platform","Wall","PushableBox","PushCart","PlaneMirror","DepthMirror","PressurePlate","DeviceSwitch","Gate","AutomaticElevator","PulleyRope","Sunlight","Moonlight","Flashlight","Firelight","Glass","Grille","Spikes","Background","LightBeam"};
        static Sprite S(string n,int i)=>WatercolorAssets.Frame(n,i);
        static Sprite[] Range(string n,int start,int count)=>WatercolorAssets.Frames(n).Skip(start).Take(count).ToArray();
        static GameObject New(string name,int layer=13){var g=new GameObject(name);g.layer=layer;g.AddComponent<FaceMembership>();return g;}
        static SpriteRenderer Art(GameObject root,Sprite sprite,float width,float y=0,string name="Artwork",int order=0){
            var a=new GameObject(name);a.transform.SetParent(root.transform,false);a.transform.localPosition=new Vector3(0,y,-.06f);var r=a.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;a.transform.localScale=Vector3.one*(width/Mathf.Max(.001f,sprite.bounds.size.x));return r;
        }
        static BoxCollider2D Box(GameObject g,Vector2 size,Vector2 center,bool trigger=false){var c=g.AddComponent<BoxCollider2D>();c.size=size;c.offset=center;c.isTrigger=trigger;return c;}
        static void Save(GameObject g){PrefabUtility.SaveAsPrefabAsset(g,Prefabs+g.name+".prefab");Object.DestroyImmediate(g);}
        static void Dynamic(GameObject g){var b=g.GetComponent<Rigidbody2D>();if(!b)b=g.AddComponent<Rigidbody2D>();b.freezeRotation=true;b.gravityScale=3.5f;b.mass=2;}
        static MirrorSurface Mirror(GameObject g,bool depth){
            var m=g.AddComponent<MirrorSurface>();m.continuousRotation=true;m.depthRotation=depth;m.rotationAngle=depth?45:-45;m.rotationSpeed=45;
            var pedestal=Art(g,S("props",3),1.1f);pedestal.transform.localScale=new Vector3(pedestal.transform.localScale.x,pedestal.transform.localScale.y*.5f,1);
            m.mirrorArt=Art(g,S("props",2),.8f,.35f,"Mirror surface",3).transform;
            // Face pivot carries the true optical plane, separate from decorative pedestal.
            var pivot=new GameObject("Optical face");pivot.layer=11;pivot.transform.SetParent(g.transform,false);pivot.transform.localPosition=Vector3.up*.9f;m.opticalPivot=pivot.transform;
            m.face=Box(pivot,new Vector2(1.5f,.08f),Vector2.zero,true);m.Apply();
            Box(g,new Vector2(.9f,.25f),Vector2.up*.125f);return m;
        }
        static void MakePrefabs(){
            var g=New("Player",9);var p=g.AddComponent<PlayerController>();p.groundMask=(1<<8)|(1<<10);p.reflectUnlocked=p.swallowUnlocked=false;var pc=g.GetComponent<BoxCollider2D>();pc.size=new Vector2(.72f,1.4f);pc.offset=Vector2.up*.72f;Dynamic(g);
            p.visual=Art(g,S("motion",0),1.0f,0,"Spirit",10);p.visual.color=new Color(1,1,1,.78f);p.idle=Range("motion",0,4);p.walk=Range("motion",4,8);p.jump=Range("motion",12,4);p.push=Range("interaction",4,4);
            var presentation=g.AddComponent<PlayerPresentation>();presentation.idle=p.idle;presentation.walk=p.walk;presentation.jump=p.jump;presentation.push=p.push;presentation.pull=Range("interaction",8,4);presentation.dissolve=Range("interaction",12,4);presentation.lookUp=S("interaction",0);presentation.lookDown=S("interaction",1);presentation.interact=S("interaction",2);
            g.AddComponent<LightTravel>().photonFrames=Range("beast",4,4);var exposure=g.AddComponent<LightExposure>();exposure.secondsRequired=2;exposure.travelSpeed=p.speed*2;g.AddComponent<PlayerCarry>();g.AddComponent<ScreenBreakEffect>();g.GetComponent<FaceMembership>().faces=RoomFaces.All;Save(g);
            g=New("HungryBeast",12);var beast=g.AddComponent<BeastReceiver>();beast.visual=Art(g,S("beast",0),2.0f);beast.hungry=S("beast",0);beast.feeding=S("beast",2);beast.fed=S("beast",3);var sense=g.AddComponent<CircleCollider2D>();sense.radius=.55f;sense.offset=Vector2.up*1.1f;sense.isTrigger=true;Save(g);
            g=New("Platform",8);Box(g,new Vector2(3.2f,.5f),Vector2.down*.25f);Art(g,S("terrain",1),3.2f,-1.4f);Save(g);
            g=New("Wall",8);Box(g,new Vector2(1.4f,3),Vector2.up*1.5f);var art=Art(g,S("terrain",4),1.4f);art.transform.localScale=new Vector3(art.transform.localScale.x,3/art.sprite.bounds.size.y,1);Save(g);
            foreach(string kind in new[]{"PushableBox","PushCart"}){g=New(kind,10);Dynamic(g);g.AddComponent<PushableBox>();Box(g,new Vector2(1.15f,1.0f),Vector2.up*.5f);Art(g,S("props",kind=="PushableBox"?0:1),kind=="PushableBox"?1.15f:1.7f);Save(g);}
            foreach(string kind in new[]{"PlaneMirror","DepthMirror"}){g=New(kind,10);Dynamic(g);g.AddComponent<PushableBox>();Mirror(g,kind=="DepthMirror");Save(g);}
            g=New("PressurePlate");var plate=g.AddComponent<PressurePlate>();plate.visual=Art(g,S("props",4),1.15f);plate.up=S("props",4);plate.down=S("props",5);Box(g,new Vector2(1.1f,.3f),Vector2.up*.15f,true);Save(g);
            g=New("DeviceSwitch");var sw=g.AddComponent<DeviceSwitch>();sw.visual=Art(g,S("props",6),.65f);sw.offSprite=S("props",6);sw.onSprite=S("props",7);Box(g,Vector2.one,Vector2.up*.5f,true);Save(g);
            g=New("Gate",8);var device=g.AddComponent<SpaceDevice>();device.action=DeviceAction.SetActive;var door=Art(g,S("props",10),1.5f,0,"Door");door.gameObject.layer=8;var gate=door.gameObject.AddComponent<BoxCollider>();gate.center=Vector3.up*door.sprite.bounds.size.y*.5f;gate.size=new Vector3(door.sprite.bounds.size.x,door.sprite.bounds.size.y,.8f/door.transform.localScale.x);device.gate=gate;device.movingPart=door.transform;Save(g);
            g=New("AutomaticElevator");var lift=g.AddComponent<AutomaticLift>();var platform=new GameObject("Moving platform");platform.layer=8;platform.transform.SetParent(g.transform,false);var solid=platform.AddComponent<BoxCollider>();solid.center=Vector3.down*.16f;solid.size=new Vector3(2.2f,.32f,.85f);Art(platform,S("props",9),2.2f,-.3f);lift.platform=platform.transform;Box(g,new Vector2(2.2f,3.6f),Vector2.up*1.7f,true);Save(g);
            g=New("PulleyRope");var pulley=g.AddComponent<PulleyRope>();pulley.wheel=Art(g,S("props",8),1.0f,3.8f,"Pulley wheel").transform;pulley.ropeEnd=Art(g,S("terrain",9),.22f,.3f,"Rope").transform;Box(g,new Vector2(1.1f,1.6f),Vector2.up*.8f,true);
            var load=New("Hoisted mirror",11);load.transform.SetParent(g.transform,false);load.transform.localPosition=new Vector3(1.8f,.4f,0);Object.DestroyImmediate(load.GetComponent<FaceMembership>());var mirror=Mirror(load,false);Object.DestroyImmediate(load.GetComponents<BoxCollider2D>().First());pulley.load=load.transform;Save(g);
            foreach(string kind in new[]{"Sunlight","Moonlight","Flashlight","Firelight"}){
                g=New(kind);var emitter=g.AddComponent<LightEmitter>();emitter.useSpatialDirection=true;emitter.spatialDirection=Vector3.right;emitter.beamWidth=.075f;
                Art(g,S(kind=="Sunlight"||kind=="Moonlight"?"terrain":"props",kind=="Sunlight"?6:kind=="Moonlight"?7:kind=="Flashlight"?14:15),kind=="Flashlight"?1.0f:kind=="Firelight"?.8f:1.4f,-.5f);
                if(kind=="Flashlight"){var button=g.AddComponent<DeviceSwitch>();button.lightTarget=emitter;button.state=false;emitter.active=false;Box(g,new Vector2(1.2f,1.0f),Vector2.zero,true);}Save(g);
            }
            foreach(string kind in new[]{"Glass","Grille","Spikes"}){g=New(kind,kind=="Spikes"?13:8);g.AddComponent<OpticalPassThrough>();var size=kind=="Spikes"?new Vector2(1.5f,.35f):new Vector2(.7f,2.0f);Box(g,size,Vector2.up*size.y*.5f,kind=="Spikes");Art(g,S("props",kind=="Glass"?12:kind=="Grille"?11:13),kind=="Spikes"?1.5f:1.2f);if(kind=="Spikes")g.AddComponent<Hazard>();Save(g);}
            g=New("Background");var bg=Art(g,AssetDatabase.LoadAssetAtPath<Sprite>(WatercolorAssets.Folder+"background.png"),32,0,"Watercolor backdrop",-100);g.transform.position=Vector3.forward*2;Save(g);
        }
        static GameObject Put(string kind,Vector3 position,int face=1,string name=null){
            var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+kind+".prefab"));g.transform.position=position;g.transform.rotation=Quaternion.Euler(0,CameraViewController.Yaw(face),0);g.GetComponent<FaceMembership>().faces=kind=="Player"?RoomFaces.All:FaceMembership.ForView(face);if(name!=null)g.name=name;var l=g.GetComponent<LightEmitter>();if(l)l.spatialDirection=g.transform.rotation*Vector3.right;return g;
        }
        static void RecordAll(){foreach(var c in Object.FindObjectsOfType<Component>())if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        static void Floor(GameObject floor){floor.transform.localScale=Vector3.one;floor.GetComponent<BoxCollider2D>().size=new Vector2(28.8f,.5f);floor.GetComponentInChildren<SpriteRenderer>().enabled=false;for(int i=0;i<9;i++){var tile=Art(floor,S("terrain",i==0?0:i==8?2:1),3.2f,-1.4f,"Stone tile "+i);tile.transform.localPosition+=Vector3.right*((i-4)*3.2f);}}
        public static void CreateEmpty(string path){
            if(System.IO.File.Exists(path))throw new Exception("Scene already exists: "+path);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Setup();
            for(int face=1;face<=4;face++){var rotation=Quaternion.Euler(0,CameraViewController.Yaw(face),0);var floor=Put("Platform",new Vector3(0,-5,0),face,"Floor "+face);Floor(floor);var bg=Put("Background",rotation*Vector3.forward*5,face,"Backdrop "+face);RoomBackgrounds.Assign(bg,face);}
            var p=Put("Player",new Vector3(-4,-4.95f,0)).GetComponent<PlayerController>();var session=Object.FindObjectOfType<LabSession>();session.player=p;Object.FindObjectOfType<CameraViewController>().player=p;RecordAll();EditorSceneManager.SaveScene(scene,path);
            var entries=EditorBuildSettings.scenes.ToList();if(!entries.Any(e=>e.path==path)){entries.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=entries.ToArray();}
        }
        static void Setup(){
            var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.79f,.84f,.86f);camera.gameObject.AddComponent<AudioListener>();camera.gameObject.AddComponent<CameraViewController>();camera.gameObject.AddComponent<FaceVisibility>();
            var world=new GameObject("Continuous world").AddComponent<SpatialLabWorld>();world.flatArtwork=true;world.platformDepth=.9f;world.spriteMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialSprite.mat");world.actorMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialActor.mat");world.solidMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialSolid.mat");
            var session=new GameObject("Lab Session").AddComponent<LabSession>();session.startWithReflection=session.startWithSwallow=false;session.beams=session.gameObject.AddComponent<BeamSystem>();session.beams.opticalMask=(1<<8)|(1<<9)|(1<<10)|(1<<11)|(1<<12)|(1<<13);session.gameObject.AddComponent<LightRouteReview>().automatic=false;
            string matPath=Root+"Materials/WatercolorBeam.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(Shader.Find("LightHungry/WatercolorBeam"));AssetDatabase.CreateAsset(mat,matPath);}session.beams.beamStyle=mat;
        }
        [MenuItem("Light Hungry/Migrate approved watercolor workshop")]
        public static void Install(){
            WatercolorAssets.Import();MakePrefabs();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Setup();
            // One continuous workshop, four display memberships. Native world positions never change on a view turn.
            foreach(int face in new[]{1,2,3,4}){
                var rot=Quaternion.Euler(0,CameraViewController.Yaw(face),0);
                var floor=Put("Platform",rot*new Vector3(0,-5,0),face,"Floor "+face);Floor(floor);
                var bg=Put("Background",rot*Vector3.forward*5,face,"Backdrop "+face);bg.transform.localScale=Vector3.one;
            }
            var player=Put("Player",new Vector3(-13,-4.95f,0)).GetComponent<PlayerController>();var session=Object.FindObjectOfType<LabSession>();session.player=player;Object.FindObjectOfType<CameraViewController>().player=player;
            Put("PushableBox",new Vector3(-10.7f,-4.95f,0));Put("PushCart",new Vector3(-7.8f,-4.95f,0));
            Put("PlaneMirror",new Vector3(-4.8f,-4.95f,0),1,"Push and rotate mirror");
            var sun=Put("Sunlight",new Vector3(-13,2.7f,0),1,"Window sunlight").GetComponent<LightEmitter>();sun.spatialDirection=new Vector3(1,-.38f,0).normalized;
            var gate=Put("Gate",new Vector3(10.5f,-5,0)).GetComponent<SpaceDevice>();var switchA=Put("DeviceSwitch",new Vector3(8.5f,-4.95f,0),1,"Gate switch").GetComponent<DeviceSwitch>();switchA.target=gate;
            var plate=Put("PressurePlate",new Vector3(6.8f,-4.95f,0),1,"Gate pressure button").GetComponent<PressurePlate>();var circuit=gate.gameObject.AddComponent<DeviceCircuit>();circuit.output=gate;circuit.inputs.Add(new CircuitInput{source=switchA});circuit.inputs.Add(new CircuitInput{source=plate});switchA.target=null;
            Put("AutomaticElevator",new Vector3(2,-4.95f,0));Put("PulleyRope",new Vector3(-1,-4.95f,0));
            var upper=Put("Platform",new Vector3(5,-1.9f,0),1,"Upper exit platform");upper.transform.localScale=new Vector3(1.6f,1,1);
            var flashlight=Put("Flashlight",new Vector3(-3.5f,-1.1f,0),1,"Timed flashlight");flashlight.GetComponent<DeviceSwitch>().activeSeconds=6;
            var depth=Put("DepthMirror",new Vector3(8,-1.95f,0),1,"Cross face mirror").GetComponent<MirrorSurface>();depth.rotationAngle=45;depth.destinationView=4;depth.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;depth.GetComponent<FaceMembership>().faces=RoomFaces.Front|RoomFaces.Right;depth.Apply();
            var crossing=Put("Flashlight",new Vector3(6,-1.0f,0),1,"Cross face flashlight");crossing.GetComponent<DeviceSwitch>().state=false;
            var step=Put("Platform",new Vector3(11,-3.4f,0),1,"Step to mirrors");
            // Right face has a reachable route where photon transmission and manual exits can be tried.
            var sideFloor=Put("Platform",new Vector3(8,-2,6),4,"Crossing arrival floor");sideFloor.transform.localScale=new Vector3(3,1,1);
            Put("Glass",new Vector3(8,-2,3.2f),4);Put("Grille",new Vector3(8,-2,6.8f),4);Put("Spikes",new Vector3(8,-2,9.0f),4);
            var wall=Put("Wall",new Vector3(8,-2,12),4,"Photon stopping wall");
            var sideMirror=Put("PlaneMirror",new Vector3(8,-2,10),4,"Side steering mirror").GetComponent<MirrorSurface>();sideMirror.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;sideMirror.rotationAngle=45;sideMirror.Apply();
            var beast=Put("HungryBeast",new Vector3(8,1.2f,10),4,"Workshop goal").GetComponent<BeastReceiver>();session.goal=beast;
            Put("Moonlight",new Vector3(3.5f,3.3f,0),2);Put("Firelight",new Vector3(-5,-4.95f,0),3);
            // Each side retains objects and simulation even when its renderers are hidden.
            RecordAll();EditorSceneManager.SaveScene(scene,Root+"Scenes/InteractionLab.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/InteractionLab.unity",true)};AssetDatabase.SaveAssets();Selection.activeObject=null;Undo.ClearAll();Debug.Log("WATERCOLOR_WORKSHOP_INSTALLED prefabs="+Palette.Length);
        }
    }
}
