using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
namespace LightHungry.EditorTools {
    public static class BuildInteractionLab {
        const string Root="Assets/LightHungry/";
        static GameObject playerPrefab,mirrorPrefab,railPrefab,boxPrefab,beastPrefab,platePrefab,leverPrefab,sunPrefab,flashPrefab,groundPrefab,spikesPrefab,wallPrefab,lensPrefab,wideLensPrefab;
        static Sprite Sprite(string folder,string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/"+folder+"/"+name+".png");
        static Sprite[] Frames(string name,int count){var a=new Sprite[count];for(int i=0;i<count;i++)a[i]=Sprite("Player",name+"_"+i.ToString("00"));return a;}
        static SpriteRenderer Art(GameObject parent,string folder,string name,Vector2 pos,float scale=1,int order=5){
            var child=new GameObject(name);child.transform.SetParent(parent.transform,false);child.transform.localPosition=pos;child.transform.localScale=Vector3.one*scale;
            var sr=child.AddComponent<SpriteRenderer>();sr.sprite=Sprite(folder,name);sr.sortingOrder=order;return sr;
        }
        static BoxCollider2D Box(GameObject go,Vector2 size,Vector2 offset,bool trigger=false){var c=go.AddComponent<BoxCollider2D>();c.size=size;c.offset=offset;c.isTrigger=trigger;return c;}
        static GameObject Save(GameObject go,string name){go.name=name;var saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return saved;}
        static GameObject Put(GameObject prefab,string name,Vector2 pos){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.name=name;go.transform.position=pos;return go;}
        [MenuItem("Light Hungry/Build fresh InteractionLab (replaces generated assets)")]
        public static void Create(){
            EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode2D;
            PlayerSettings.productName="light hungry";PlayerSettings.companyName="Light Hungry Workshop";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.runInBackground=true;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers=tags.FindProperty("layers");string[] names={"World","Player","Pushable","Optics","Receiver","Triggers"};for(int i=0;i<names.Length;i++)layers.GetArrayElementAtIndex(8+i).stringValue=names[i];tags.ApplyModifiedProperties();
            AssetDatabase.Refresh();
            foreach(string path in AssetDatabase.GetAllAssetPaths()){
                if(!path.StartsWith(Root+"Art/")||!path.EndsWith(".png"))continue;
                var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=256;
                var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=path.Contains("background")||path.EndsWith("mirror_head.png")?new Vector2(.5f,.5f):new Vector2(.5f,.0625f);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);
                imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Bilinear;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=4096;imp.npotScale=TextureImporterNPOTScale.None;imp.SaveAndReimport();
            }
            MakePrefabs();MakeScene();AssetDatabase.SaveAssets();
            Debug.Log("LIGHT_HUNGRY_LAB_CREATED");
        }
        static void MakePrefabs(){
            var go=new GameObject("Player");go.layer=9;var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=3.5f;rb.constraints=RigidbodyConstraints2D.FreezeRotation;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=Box(go,new Vector2(.78f,1.38f),new Vector2(0,.72f));var mat=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root+"Prefabs/PlayerFriction.physicsMaterial2D");if(!mat){mat=new PhysicsMaterial2D("Player no friction"){friction=0,bounciness=0};AssetDatabase.CreateAsset(mat,Root+"Prefabs/PlayerFriction.physicsMaterial2D");}collider.sharedMaterial=mat;
            var p=go.AddComponent<PlayerController>();p.groundMask=(1<<8)|(1<<10);p.visual=Art(go,"Player","player_idle_00",Vector2.zero);
            p.idle=Frames("player_idle",4);p.walk=Frames("player_walk",8);p.jump=Frames("player_jump",6);p.push=Frames("player_push",4);p.swallow=Frames("player_swallow",2);p.reflect=Frames("player_reflect",2);p.charged=Frames("player_stored",2);p.release=Frames("player_release",2);playerPrefab=Save(go,"Player");
            go=new GameObject("PlaneMirror");go.layer=11;var mirror=go.AddComponent<MirrorSurface>();
            mirror.mirrorArt=Art(go,"Props","mirror_head",Vector2.zero).transform;
            Art(go,"Props","mirror_stand",new Vector2(.33f,-1.18f),.83f,4);
            var face=new GameObject("Optical face");face.layer=11;face.transform.SetParent(go.transform,false);mirror.face=Box(face,new Vector2(1.4f,.08f),Vector2.zero,true);mirror.Apply();mirrorPrefab=Save(go,"PlaneMirror");
            go=new GameObject("RailDevice");go.layer=8;rb=go.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.interpolation=RigidbodyInterpolation2D.Interpolate;Box(go,new Vector2(2.1f,.22f),new Vector2(0,.52f));Art(go,"Props","rail_carriage",Vector2.zero,1.4f);go.AddComponent<RailDevice>();railPrefab=Save(go,"RailDevice");
            go=new GameObject("PushableBox");go.layer=10;rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=3.5f;rb.constraints=RigidbodyConstraints2D.FreezeRotation;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;Box(go,new Vector2(1.52f,1.4f),new Vector2(0,.72f));Art(go,"Props","wooden_crate",Vector2.zero);go.AddComponent<PushableBox>();boxPrefab=Save(go,"PushableBox");
            go=new GameObject("HungryBeast");go.layer=12;var circle=go.AddComponent<CircleCollider2D>();circle.radius=.47f;circle.isTrigger=true;var beast=go.AddComponent<BeastReceiver>();beast.visual=Art(go,"Beast","beast_hungry",new Vector2(.85f,-2.2f),2.4f);beast.hungry=Sprite("Beast","beast_hungry");beast.feeding=Sprite("Beast","beast_feeding");beast.fed=Sprite("Beast","beast_fed");beastPrefab=Save(go,"HungryBeast");
            go=new GameObject("PressurePlate");go.layer=13;Box(go,new Vector2(1.45f,.2f),new Vector2(0,.18f),true);var plate=go.AddComponent<PressurePlate>();plate.visual=Art(go,"Props","pressure_plate_up",Vector2.zero,.9f);plate.up=Sprite("Props","pressure_plate_up");plate.down=Sprite("Props","pressure_plate_down");platePrefab=Save(go,"PressurePlate");
            go=new GameObject("PulleyLever");go.layer=13;Box(go,new Vector2(1.2f,1.4f),new Vector2(0,.7f),true);var lever=go.AddComponent<RailLever>();lever.visual=Art(go,"Props","lever_off",Vector2.zero);lever.left=Sprite("Props","lever_off");lever.right=Sprite("Props","lever_on");leverPrefab=Save(go,"PulleyLever");
            go=new GameObject("Sunlight");var window=Art(go,"Environment","window_frame",new Vector2(0,-.6f),2);window.sortingOrder=1;go.AddComponent<LightEmitter>();sunPrefab=Save(go,"Sunlight");
            go=new GameObject("Flashlight");go.layer=13;Art(go,"Props","flashlight",new Vector2(-.65f,-.6f),.8f);Box(go,new Vector2(1.4f,.8f),new Vector2(-.5f,-.25f),true);var light=go.AddComponent<LightEmitter>();var toggle=go.AddComponent<SwitchableLight>();toggle.source=light;flashPrefab=Save(go,"Flashlight");
            go=new GameObject("Platform");go.layer=8;Box(go,new Vector2(1.72f,.5f),new Vector2(0,-.25f));Art(go,"Environment","platform_middle",new Vector2(0,-.5f));groundPrefab=Save(go,"Platform");
            go=new GameObject("Wall");go.layer=8;Box(go,new Vector2(1.3f,1.3f),new Vector2(0,.65f));Art(go,"Environment","stone_block",Vector2.zero);wallPrefab=Save(go,"Wall");
            go=new GameObject("Spikes");go.layer=13;Box(go,new Vector2(1.6f,.58f),new Vector2(0,.3f),true);Art(go,"Environment","spikes",Vector2.zero);go.AddComponent<Hazard>();spikesPrefab=Save(go,"Spikes");
            lensPrefab=MakeLens("ConvexLens","convex_lens",.55f);wideLensPrefab=MakeLens("ConcaveLens","concave_lens",1.8f);
            MakePickup("ReflectUnlock",true);MakePickup("SwallowUnlock",false);
            go=new GameObject("LightReceiver");go.layer=12;circle=go.AddComponent<CircleCollider2D>();circle.radius=.35f;circle.isTrigger=true;var receiver=go.AddComponent<BeastReceiver>();receiver.isGoal=false;receiver.requiredSeconds=1;Art(go,"Props","pressure_plate_up",new Vector2(0,-.3f),.5f);Save(go,"LightReceiver");
            go=new GameObject("BeamSystem");var system=go.AddComponent<BeamSystem>();system.opticalMask=(1<<8)|(1<<9)|(1<<10)|(1<<11)|(1<<12);Save(go,"BeamSystem");
            go=new GameObject("LabSession");go.AddComponent<LabSession>();Save(go,"LabSession");
            go=new GameObject("RailMirrorAssembly");var rail=(GameObject)PrefabUtility.InstantiatePrefab(railPrefab,go.transform);var m=(GameObject)PrefabUtility.InstantiatePrefab(mirrorPrefab,rail.transform);m.transform.localPosition=new Vector3(0,1.7f,0);Save(go,"RailMirrorAssembly");
        }
        static GameObject MakeLens(string name,string art,float mult){var go=new GameObject(name);go.layer=11;Box(go,new Vector2(.22f,1.35f),new Vector2(0,.8f),true);Art(go,"Props",art,Vector2.zero);go.AddComponent<LensSurface>().widthMultiplier=mult;return Save(go,name);}
        static void MakePickup(string name,bool reflect){var go=new GameObject(name);go.layer=13;Box(go,Vector2.one,Vector2.up*.6f,true);var pickup=go.AddComponent<AbilityPickup>();pickup.unlockReflection=reflect;Art(go,"Player",reflect?"player_reflect_01":"player_stored_01",Vector2.zero,.65f);Save(go,name);}
        static void Floor(string name,float x,float y,float width){
            var go=Put(groundPrefab,name,new Vector2(x,y));UnityEngine.Object.DestroyImmediate(go.GetComponentInChildren<SpriteRenderer>().gameObject);go.GetComponent<BoxCollider2D>().size=new Vector2(width,.5f);
            int count=Mathf.CeilToInt(width/1.72f);float unit=width/count;
            for(int i=0;i<count;i++){var sr=Art(go,"Environment","platform_middle",new Vector2(-width*.5f+unit*(i+.5f),-.5f));sr.transform.localScale=new Vector3(unit/1.72f,1,1);}
        }
        static void MakeScene(){
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.14f,.19f);camera.gameObject.AddComponent<AudioListener>();
            var bg=new GameObject("Background");var sr=bg.AddComponent<SpriteRenderer>();sr.sprite=Sprite("Environment","stone_room_background");sr.sortingOrder=-20;bg.transform.localScale=new Vector3(32/(sr.sprite.bounds.size.x),18/sr.sprite.bounds.size.y,1);
            Floor("Floor west",-7.7f,-5.8f,14.6f);Floor("Floor east",8.3f,-5.8f,13.4f);Floor("Pit base",.6f,-7,2);
            Floor("Step",-13,-4.6f,2);Floor("Sun ledge",-10.7f,-3.3f,4.2f);Floor("Central ledge",-5,-3.8f,3);
            Floor("Lens bench",-.5f,.1f,6);Floor("Lens step",-5,-1.6f,2);
            var player=Put(playerPrefab,"Player",new Vector2(-14.4f,-5.7f)).GetComponent<PlayerController>();
            Put(sunPrefab,"Sunlight",new Vector2(-14,-2.4f));
            var assembly=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/RailMirrorAssembly.prefab");
            var railGo=Put(assembly,"Rail mirror",new Vector2(6,-4.1f));var rail=railGo.GetComponentInChildren<RailDevice>();
            var beast=Put(beastPrefab,"Hungry beast",new Vector2(9,2.5f)).GetComponent<BeastReceiver>();
            Put(boxPrefab,"Box",new Vector2(-7.4f,-5.7f));
            var left=Put(platePrefab,"Left pressure button",new Vector2(3.9f,-5.8f)).GetComponent<PressurePlate>();
            var right=Put(platePrefab,"Right pressure button",new Vector2(6,-5.8f)).GetComponent<PressurePlate>();rail.leftPlate=left;rail.rightPlate=right;
            Put(leverPrefab,"Pulley control",new Vector2(2.2f,-5.8f)).GetComponent<RailLever>().target=rail;
            Art(railGo,"Props","pulley_wheel",new Vector2(-2.3f,1.1f),.9f);
            var track=new GameObject("Rail guide").AddComponent<LineRenderer>();BeamSystem.ConfigureLine(track,.055f,new Color(.4f,.55f,.65f));track.sortingOrder=0;track.positionCount=2;track.SetPosition(0,new Vector3(5,-4.05f,0));track.SetPosition(1,new Vector3(10,-4.05f,0));
            Put(spikesPrefab,"Spikes",new Vector2(.6f,-6.8f));
            Put(flashPrefab,"Bench flashlight",new Vector2(-2,.95f));
            Put(lensPrefab,"Bench convex lens",new Vector2(-.8f,.1f));Put(wideLensPrefab,"Bench concave lens",new Vector2(1,.1f));
            var receiver=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/LightReceiver.prefab");Put(receiver,"Bench receiver",new Vector2(2.4f,.95f));
            Put(mirrorPrefab,"Practice mirror",new Vector2(-4,-4.6f));
            var pickup=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/ReflectUnlock.prefab");Put(pickup,"Reflection pickup",new Vector2(-9,-5.8f));pickup=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/SwallowUnlock.prefab");Put(pickup,"Swallow pickup",new Vector2(-10.7f,-3.3f));
            for(int side=-1;side<=1;side+=2){var edge=new GameObject("Boundary");edge.layer=8;edge.transform.position=new Vector3(side*15.3f,0,0);Box(edge,new Vector2(.5f,20),Vector2.zero);}
            var beamPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/BeamSystem.prefab");var beams=Put(beamPrefab,"Beam System",Vector2.zero).GetComponent<BeamSystem>();
            var sessionPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/LabSession.prefab");var session=Put(sessionPrefab,"Lab Session",Vector2.zero).GetComponent<LabSession>();session.player=player;session.goal=beast;session.beams=beams;
            EditorSceneManager.SaveScene(scene,Root+"Scenes/InteractionLab.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/InteractionLab.unity",true)};
        }
        public static void CreateAndBuild(){Create();Build();}
        public static void ValidateAndBuild(){
            foreach(var entry in EditorBuildSettings.scenes){
                if(!entry.enabled)continue;
                var scene=EditorSceneManager.OpenScene(entry.path,OpenSceneMode.Single);
                foreach(var root in scene.GetRootGameObjects()){
                    if(root.GetComponentsInChildren<TextMesh>(true).Length>0||root.GetComponentsInChildren<Canvas>(true).Length>0)
                        throw new Exception("Unexpected text or Canvas in playable scene: "+entry.path);
                    foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                        if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)
                            throw new Exception("Missing script on "+transform.name);
                }
            }
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{Root+"Prefabs"})){
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if(prefab.GetComponentsInChildren<TextMesh>(true).Length>0||prefab.GetComponentsInChildren<Canvas>(true).Length>0)
                    throw new Exception("Unexpected text or Canvas in prefab: "+prefab.name);
                foreach(var transform in prefab.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)
                        throw new Exception("Missing script in prefab "+prefab.name);
            }
            Debug.Log("LIGHT_HUNGRY_TEXT_FREE_VALIDATED");
            Build();
        }
        [MenuItem("Light Hungry/Build Windows prototype")]
        public static void Build(){
            Directory.CreateDirectory("Builds/Windows");var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows/LightHungry.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
            Debug.Log("LIGHT_HUNGRY_BUILD_OK");
        }
    }
}
