using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightHungry.EditorTools {
    public static class LevelKit {
        public const string Root="Assets/LightHungry/";
        public const string Folder=Root+"Prefabs/LevelKit/";
        public static readonly string[] Items={"Platform3D","Wall3D","SpaceMirror","PortableMirror","LightLaunchPad","LightLanding","DeviceSwitch","DevicePlate","Gate","MovingPlatform","CarryBox","Hazard3D","Sunlight","Flashlight","HungryBeast","Player"};
        static Material Mat=>AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialSolid.mat");
        public static GameObject Shape(GameObject root,string name,Vector3 p,Vector3 size,Color color,PrimitiveType type=PrimitiveType.Cube,bool collider=false){
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(root.transform,false);g.transform.localPosition=p;g.transform.localScale=size;
            if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            var r=g.GetComponent<Renderer>();var m=new Material(Mat);m.color=color;
            string path=Root+"Materials/Kit_"+ColorUtility.ToHtmlStringRGB(color)+".mat";var saved=AssetDatabase.LoadAssetAtPath<Material>(path);if(!saved){AssetDatabase.CreateAsset(m,path);saved=m;}else UnityEngine.Object.DestroyImmediate(m);r.sharedMaterial=saved;return g;
        }
        public static void EnsureAssets(){
            if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder(Root+"Prefabs","LevelKit");
            foreach(string kind in Items){if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/"+kind+".prefab")||AssetDatabase.LoadAssetAtPath<GameObject>(Folder+kind+".prefab"))continue;
                var g=new GameObject(kind);g.layer=13;Color stone=new Color(.28f,.36f,.4f),gold=new Color(.9f,.65f,.2f),cyan=new Color(.4f,.8f,.8f);
                if(kind=="Platform3D"||kind=="Wall3D"){g.layer=8;Vector3 size=kind=="Platform3D"?new Vector3(4,.6f,4):new Vector3(1,4,4);g.AddComponent<BoxCollider>().size=size;Shape(g,"Stone",Vector3.zero,size,stone);}
                if(kind=="SpaceMirror"||kind=="PortableMirror"){
                    g.layer=11;var m=g.AddComponent<SpaceMirror>();var face=Shape(g,"Mirror face",Vector3.zero,new Vector3(1.4f,1.4f,.08f),cyan);m.face=face.transform;var c=face.AddComponent<BoxCollider>();c.isTrigger=true;face.layer=11;
                    Shape(g,"Mirror foot",Vector3.down*.8f,new Vector3(1.5f,.15f,.7f),gold);m.Apply();
                    if(kind=="PortableMirror"){g.AddComponent<CarryableProp>().floorOffset=.85f;g.transform.position=Vector3.zero;}
                }
                if(kind=="LightLaunchPad"){
                    g.AddComponent<LightLaunchPad>();var c=g.AddComponent<BoxCollider>();c.center=Vector3.up*.4f;c.size=new Vector3(1,.8f,1);c.isTrigger=true;
                    Shape(g,"Launch base",Vector3.up*.12f,new Vector3(1,.24f,1),gold);Shape(g,"Core",Vector3.up*.55f,Vector3.one*.3f,cyan,PrimitiveType.Sphere);
                }
                if(kind=="LightLanding"){
                    var l=g.AddComponent<LightLanding>();g.AddComponent<SphereCollider>().isTrigger=true;g.GetComponent<SphereCollider>().radius=.4f;
                    var spawn=new GameObject("Entity spawn");spawn.transform.SetParent(g.transform,false);spawn.transform.localPosition=Vector3.down*.95f;l.spawn=spawn.transform;
                    Shape(g,"Landing core",Vector3.zero,Vector3.one*.23f,cyan,PrimitiveType.Sphere);
                    for(int i=0;i<4;i++)Shape(g,"Landing bracket",Quaternion.Euler(0,0,i*90)*Vector3.up*.45f,new Vector3(.25f,.06f,.12f),gold);
                }
                if(kind=="DeviceSwitch"){
                    var s=g.AddComponent<DeviceSwitch>();var c=g.AddComponent<BoxCollider>();c.center=Vector3.up*.5f;c.isTrigger=true;
                    Shape(g,"Switch base",Vector3.up*.1f,new Vector3(.8f,.2f,.8f),stone);s.handle=Shape(g,"Handle",Vector3.up*.55f,new Vector3(.1f,.7f,.1f),gold).transform;
                }
                if(kind=="DevicePlate"){
                    var p=g.AddComponent<DevicePlate>();var c=g.AddComponent<BoxCollider>();c.center=Vector3.up*.15f;c.size=new Vector3(1.5f,.3f,1.5f);c.isTrigger=true;
                    p.top=Shape(g,"Plate top",Vector3.up*.08f,new Vector3(1.4f,.15f,1.4f),gold).transform;
                }
                if(kind=="Gate"||kind=="MovingPlatform"){
                    var d=g.AddComponent<SpaceDevice>();d.action=kind=="Gate"?DeviceAction.SetActive:DeviceAction.Move;
                    var part=Shape(g,"Moving part",Vector3.zero,kind=="Gate"?new Vector3(.5f,3,2):new Vector3(3,.4f,3),stone,PrimitiveType.Cube,true);part.layer=8;d.movingPart=part.transform;d.gate=part.GetComponent<Collider>();
                    if(kind=="MovingPlatform"){var b=part.AddComponent<Rigidbody>();b.isKinematic=true;b.useGravity=false;}
                }
                if(kind=="CarryBox"){
                    g.layer=10;g.AddComponent<CarryableProp>();var c=g.AddComponent<BoxCollider>();c.center=Vector3.up*.65f;c.size=Vector3.one*1.2f;
                    Shape(g,"Box",Vector3.up*.65f,Vector3.one*1.2f,new Color(.45f,.3f,.16f));var b=g.AddComponent<Rigidbody>();b.constraints=RigidbodyConstraints.FreezeRotation;
                }
                if(kind=="Hazard3D"){
                    var c=g.AddComponent<BoxCollider>();c.isTrigger=true;c.size=new Vector3(2,.4f,2);g.AddComponent<SpaceHazard>();
                    for(int i=0;i<5;i++)Shape(g,"Spike",new Vector3((i-2)*.35f,0,0),new Vector3(.25f,.4f,.25f),new Color(.7f,.3f,.25f));
                }
                PrefabUtility.SaveAsPrefabAsset(g,Folder+kind+".prefab");UnityEngine.Object.DestroyImmediate(g);
            }
            AssetDatabase.SaveAssets();
            string hazardPath=Folder+"Hazard3D.prefab";
            var hazard=PrefabUtility.LoadPrefabContents(hazardPath);
            if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(hazard)>0){GameObjectUtility.RemoveMonoBehavioursWithMissingScript(hazard);if(!hazard.GetComponent<SpaceHazard>())hazard.AddComponent<SpaceHazard>();PrefabUtility.SaveAsPrefabAsset(hazard,hazardPath);}
            PrefabUtility.UnloadPrefabContents(hazard);
        }
        public static GameObject Put(string kind,Vector3 position,Transform parent=null){var p=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+kind+".prefab");if(!p)p=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/"+kind+".prefab");if(!p)throw new Exception("Missing kit prefab: "+kind);var g=(GameObject)PrefabUtility.InstantiatePrefab(p);g.transform.SetParent(parent,true);g.transform.position=position;return g;}
        [MenuItem("Light Hungry/Install light travel kit in existing room")]
        public static void InstallRoom(){
            EnsureAssets();var scene=EditorSceneManager.OpenScene(Root+"Scenes/InteractionLab.unity",OpenSceneMode.Single);
            if(!GameObject.Find("Light Travel Workshop")){
                var root=new GameObject("Light Travel Workshop").transform;
                Put("LightLaunchPad",new Vector3(-13,-5.8f,0),root).name="Workshop launch";
                var mirror=Put("SpaceMirror",new Vector3(-10,-4.85f,0),root).GetComponent<SpaceMirror>();mirror.name="Workshop depth mirror";mirror.travelView=4;
                var l=Put("LightLanding",new Vector3(-10,-4.85f,9),root).GetComponent<LightLanding>();l.name="Workshop side landing";l.view=4;
                Put("Platform3D",new Vector3(-10,-6.1f,9),root);
                var back=Put("LightLaunchPad",new Vector3(-10,-5.8f,7.4f),root).GetComponent<LightLaunchPad>();back.name="Workshop return launch";back.localDirection=Vector3.back;
                l=Put("LightLanding",new Vector3(-13,-4.85f,0),root).GetComponent<LightLanding>();l.name="Workshop front landing";
                Put("CarryBox",new Vector3(-12,-5.7f,-2),root);
                var gate=Put("Gate",new Vector3(-8,-4.3f,9),root).GetComponent<SpaceDevice>();
                Put("DeviceSwitch",new Vector3(-10,-5.8f,10.3f),root).GetComponent<DeviceSwitch>().target=gate;
                var lift=Put("MovingPlatform",new Vector3(-6,-5.5f,9),root).GetComponent<SpaceDevice>();
                Put("DevicePlate",new Vector3(-7,-5.8f,-2),root).GetComponent<DevicePlate>().target=lift;
                Put("PortableMirror",new Vector3(-5,-4.8f,-2),root);
                Put("Hazard3D",new Vector3(12,-5.6f,-3),root);
            }
            var depthMirror=GameObject.Find("Workshop depth mirror");if(depthMirror){depthMirror.GetComponent<SpaceMirror>().travelView=0;EditorUtility.SetDirty(depthMirror.GetComponent<SpaceMirror>());}
            var launch=GameObject.Find("Workshop launch");launch.transform.position=new Vector3(-14.4f,-5.8f,0);launch.GetComponent<LightLaunchPad>().lightOffset=Vector3.up*1.6f;
            depthMirror.transform.position=new Vector3(-10,-4.2f,0);
            var side=GameObject.Find("Workshop side landing");side.transform.position=new Vector3(-10,-4.2f,9);side.GetComponent<LightLanding>().spawn.localPosition=Vector3.down*1.6f;
            var front=GameObject.Find("Workshop front landing");front.transform.position=new Vector3(-14.4f,-4.2f,0);front.GetComponent<LightLanding>().spawn.localPosition=Vector3.down*1.6f;
            GameObject.Find("Workshop return launch").GetComponent<LightLaunchPad>().lightOffset=Vector3.up*1.6f;
            var workshop=GameObject.Find("Light Travel Workshop");
            foreach(var d in workshop.GetComponentsInChildren<SpaceDevice>())d.transform.position=d.action==DeviceAction.SetActive?new Vector3(-10,-4.3f,10.8f):new Vector3(-10,-5.5f,13);
            foreach(var s in workshop.GetComponentsInChildren<DeviceSwitch>())s.transform.position=new Vector3(-10,-5.8f,8.5f);
            foreach(var component in GameObject.Find("Light Travel Workshop").GetComponentsInChildren<Component>()){if(!component)continue;EditorUtility.SetDirty(component);PrefabUtility.RecordPrefabInstancePropertyModifications(component);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("LIGHT_TRAVEL_KIT_INSTALLED");
        }
        public static void InstallAndBuild(){InstallRoom();BuildInteractionLab.ValidateAndBuild();}
        public static void NewLevel(string path){
            if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Background.prefab")){WatercolorWorkshop.CreateEmpty(path);return;}
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var c=new GameObject("Main Camera").AddComponent<Camera>();c.tag="MainCamera";c.orthographic=true;c.orthographicSize=9;c.gameObject.AddComponent<AudioListener>();
            var v=c.gameObject.AddComponent<CameraViewController>();v.targetCamera=c;
            c.gameObject.AddComponent<FaceVisibility>();
            var world=new GameObject("Spatial Lab").AddComponent<SpatialLabWorld>();
            world.spriteMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialSprite.mat");world.actorMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialActor.mat");world.solidMaterial=Mat;world.backdropMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/SpatialBackdrop.mat");
            var p=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Player.prefab"));p.transform.position=new Vector3(-4,.1f,0);v.player=p.GetComponent<PlayerController>();
            var beams=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/BeamSystem.prefab"));
            var session=new GameObject("Lab Session").AddComponent<LabSession>();session.player=v.player;session.beams=beams.GetComponent<BeamSystem>();session.startWithReflection=false;session.startWithSwallow=false;
            session.gameObject.AddComponent<LightRouteReview>();
            var beast=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/HungryBeast.prefab"));beast.transform.position=new Vector3(5,2,0);session.goal=beast.GetComponent<BeastReceiver>();
            var sun=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Sunlight.prefab"));sun.transform.position=new Vector3(-6,2,0);
            var floor=Put("Platform3D",new Vector3(0,-.3f,0));floor.transform.localScale=new Vector3(4,1,3);
            foreach(var item in new[]{p,beast,sun,floor}){var membership=item.AddComponent<FaceMembership>();membership.faces=item==p?RoomFaces.All:RoomFaces.Front;EditorUtility.SetDirty(membership);PrefabUtility.RecordPrefabInstancePropertyModifications(membership);}
            EditorSceneManager.SaveScene(scene,path);
            var entries=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);if(!entries.Exists(e=>e.path==path)){entries.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=entries.ToArray();}
        }
    }
}
