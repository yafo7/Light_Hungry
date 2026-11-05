using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

public static class FifthLevelBuilder {
 const string Art="Assets/Art/Level05/",Geo="Assets/Art/Level05Geometry/",Mats="Assets/Materials/Level05/";
 static string Work=>Path.GetFullPath(Application.dataPath+"/../..");
 static Transform bg,terrain,props;static PhysicsMaterial2D friction;
 static Vector3 P(float x,float y,float z=0)=>FifthPumpFlow.Px(x,y,z);
 static Rect grate=new Rect(50,227,425,116),glass=new Rect(558,227,421,116),wheel=new Rect(588,581,362,365),mirror=new Rect(133,575,250,382);
 [MenuItem("食光兽/搭建第五关：废弃泵房")]
 public static void BuildScene(){
  foreach(var d in new[]{Art,Geo,Mats,Work+"/Validation/level05"})Directory.CreateDirectory(d);
  foreach(var file in new[]{"background.png","props-atlas.png"})File.Copy(Work+"/art/level05-unity-assets-v01-2026-11-04/"+file,Art+file,true);
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var file in new[]{"background.png","props-atlas.png"}){var t=(TextureImporter)AssetImporter.GetAtPath(Art+file);t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();}
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  bg=new GameObject("01_Background_NoCollision").transform;terrain=new GameObject("02_Horizontal_Walkable_XY").transform;props=new GameObject("03_Interactive_Mechanisms").transform;
  friction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");
  Paint("PumpHouse_Background",bg,new Rect(0,0,1536,1024),new Rect(0,0,1536,1024),"background.png",8,false);
  var water=Paint("Sump_Animated",bg,new Rect(0,934,1536,90),new Rect(0,934,1536,90),"background.png",6,false);water.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("LightFeast/PumpWater");
  var f=new GameObject("FifthPumpHouse_Flow").AddComponent<FifthPumpFlow>();
  Solid("EntryBank",new Rect(0,900,315,140),6);Solid("RightBank",new Rect(1345,750,191,274),6);
  Deck("A_Glass_Maintenance_Catwalk",650,1345,750,true);
  Deck("C_Valve_Service_Balcony",1140,1278,385,false);
  Deck("D_Glass_ObservationFloor",1090,1536,260,true);
  Strip("D_LeftGlassWall",new Rect(1090,28,9,232));Strip("D_GlassRoof",new Rect(1090,24,446,8));
  Solid("D_LeftGlassWall",new Rect(1090,28,8,232),10);Solid("D_Roof",new Rect(1090,24,446,8),10);
  Paint("B_ValveInspection_Cutaway",props,new Rect(265,320,321,230),new Rect(1068,80,416,350),"props-atlas.png",0,true);
  Solid("B_Floor",new Rect(265,540,321,8),6);Solid("B_Left",new Rect(265,320,9,220),6);Solid("B_Roof",new Rect(265,320,321,8),6);
  Solid("B_Right_AboveLightSlit",new Rect(578,320,8,185),6);Solid("B_Right_BelowLightSlit",new Rect(578,535,8,13),6);
  var stepRects=new[]{new Rect(1294,620,150,42),new Rect(1209,521,150,42),new Rect(1309,444,135,37)};
  f.steps=new Transform[3];f.stepColliders=new Collider2D[3];
  for(int j=0;j<3;j++){var r=stepRects[j];var g=Paint("FoldOut_InspectionStep_"+j,props,r,grate,"props-atlas.png",-.5f,true);f.steps[j]=g.transform;var c=Solid("StepCollision_"+j,new Rect(r.x,r.y,r.width,7),6);c.usedByEffector=true;var e=c.gameObject.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useSideFriction=false;e.useSideBounce=false;e.surfaceArc=180;f.stepColliders[j]=c;c.enabled=false;}
  f.landings=new[]{Point("Entry",188,899),Point("A_Safe",1118,749),Point("B_Safe",430,539),Point("C_Safe",1205,384),Point("D_Safe",1265,259)};
  f.mirrors=new Transform[4];var ps=new[]{FifthPumpFlow.M0,FifthPumpFlow.A,FifthPumpFlow.Q,FifthPumpFlow.D};
  for(int j=0;j<4;j++){var pivot=new GameObject("M"+j+"_OpticalPivot").transform;pivot.SetParent(props);pivot.position=ps[j];Paint("Mirror_"+j,pivot,new Rect(-25,-40,50,80),mirror,"props-atlas.png",-1,true,true);f.mirrors[j]=pivot;}
  f.wheels=new[]{Wheel("K_A_Aim",1190,735),Wheel("K_B_Steps",380,520),Wheel("K_C_SuspendedMirror",1205,367),Wheel("K_D_FeedingMirror",1330,242)};
  Shaft("B_To_Step_Linkage",new[]{P(380,520,1),P(600,520,1),P(635,520,1),P(635,290,1),P(1455,290,1),P(1455,620,1)},.037f,new Color(.40f,.29f,.16f));
  Shaft("C_To_M2_Linkage",new[]{P(1205,367,.6f),P(1135,350,.6f),P(868,350,.6f),P(868,430,.6f)},.037f,new Color(.42f,.3f,.17f));
  var gate=Paint("Root_ExitGate",props,new Rect(1450,140,70,120),new Rect(1100,555,345,410),"props-atlas.png",-1,true);f.rootGate=gate.transform;f.exitBlock=Solid("Exit_RootCollision",new Rect(1465,100,12,160),6);
  f.scaleGlow=SpiritEffects.Halo("FedScale_Glow",props,P(1420,96,-1),new Vector2(.65f,.5f),new Color(1,.8f,.32f,.65f));f.scaleGlow.enabled=false;
  var player=new GameObject("WickSpirit_Player");player.transform.position=f.landings[0].position+Vector3.back;
  var body=player.AddComponent<Rigidbody2D>();body.gravityScale=3;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var capsule=player.AddComponent<CapsuleCollider2D>();capsule.size=new Vector2(.42f,.94f);capsule.offset=new Vector2(0,.49f);capsule.sharedMaterial=friction;
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab"));visual.transform.SetParent(player.transform,false);
  f.player=player.AddComponent<PlayerMotor>();f.player.visual=visual.transform;f.player.speed=3.2f;f.player.jumpSpeed=11;f.player.groundMask=(1<<6)|(1<<10);
  var embers=SpiritEffects.Embers(player.transform);f.player.hairEmitter=embers.transform;f.ability=player.AddComponent<LightFormAbility>();f.ability.rearmOnMovement=true;f.ability.restoreSurfaces=1<<10;
  f.path=new GameObject("ONE_Source_EffectiveOpticalPath").AddComponent<LightPath>();f.path.chargeRadius=.32f;f.path.travelSpeed=4.8f;
  f.path.beams=new[]{Shaft("GoldenLight_Halo",new Vector3[2],.13f,new Color(1,.65f,.14f,.25f)),Shaft("GoldenLight_Core",new Vector3[2],.035f,new Color(1,.94f,.60f,1))};f.RefreshOptics();
  var cam=new GameObject("Main Camera");cam.tag="MainCamera";var view=cam.AddComponent<Camera>();view.orthographic=true;view.orthographicSize=6.4f;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.17f,.16f);cam.transform.position=new Vector3(9.6f,6.4f,-40);cam.AddComponent<AudioListener>();f.cameraRig=cam.AddComponent<PotCamera>();f.cameraRig.view=view;f.cameraRig.player=player.transform;f.cameraRig.canvasWidth=19.2f;f.cameraRig.bottom=0;f.cameraRig.top=12.8f;f.cameraRig.playSize=4.4f;
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/FifthPumpHouse.unity");
  var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path.EndsWith("FifthPumpHouse.unity")))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/FifthPumpHouse.unity",true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();Debug.Log("FIFTH_SCENE_READY");
 }
 static Transform Point(string name,float x,float y){var t=new GameObject(name).transform;t.SetParent(props);t.position=P(x,y);return t;}
 static Transform Wheel(string name,float x,float y){var t=Point(name,x,y);Paint(name+"_Paint",t,new Rect(-22,-22,44,44),wheel,"props-atlas.png",-1,true,true);return t;}
 static void Strip(string name,Rect r){Paint(name,terrain,r,new Rect(563,231,410,38),"props-atlas.png",-.25f,true);}
 static void Deck(string name,float l,float r,float y,bool translucent){Paint(name,terrain,new Rect(l,y,r-l,translucent?60:(r-l)*116/425),translucent?glass:grate,"props-atlas.png",0,true);Solid(name+"_Collider",new Rect(l,y,r-l,8),translucent?10:6);}
 static BoxCollider2D Solid(string name,Rect r,int layer){var g=new GameObject(name);g.transform.SetParent(terrain);g.layer=layer;g.transform.position=P(r.center.x,r.center.y);var c=g.AddComponent<BoxCollider2D>();c.size=r.size/80;c.sharedMaterial=friction;return c;}
 static GameObject Paint(string name,Transform parent,Rect r,Rect crop,string tex,float z,bool interactive,bool local=false){
  var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=local?parent.position+new Vector3(r.x/80,-r.yMax/80,z):P(r.x,r.yMax,z);
  var m=new Mesh{name=name};float w=r.width/80,h=r.height/80;m.vertices=new[]{Vector3.zero,new Vector3(w,0),new Vector3(0,h),new Vector3(w,h)};m.uv=new[]{new Vector2(crop.x/1536,1-crop.yMax/1024),new Vector2(crop.xMax/1536,1-crop.yMax/1024),new Vector2(crop.x/1536,1-crop.y/1024),new Vector2(crop.xMax/1536,1-crop.y/1024)};m.triangles=new[]{0,2,1,1,2,3};m.RecalculateBounds();m.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=Save(m,Geo+name+".asset");var mat=Mat(name,interactive?"LightFeast/InteractablePaint":"LightFeast/PaintedLayer",Color.white);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+tex);g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
 }
 static Material Mat(string n,string shader,Color color){var m=new Material(Shader.Find(shader));m.SetColor("_Color",color);return Save(m,Mats+n+".mat");}
 static T Save<T>(T obj,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);return old;}AssetDatabase.CreateAsset(obj,path);return obj;}
 static LineRenderer Shaft(string name,Vector3[] nodes,float width,Color color){var l=new GameObject(name).AddComponent<LineRenderer>();l.transform.SetParent(props);l.useWorldSpace=true;l.positionCount=nodes.Length;l.SetPositions(nodes);l.widthMultiplier=width;l.numCapVertices=4;l.numCornerVertices=4;l.sharedMaterial=Mat(name,name.StartsWith("GoldenLight")?"LightFeast/SoftLight":"LightFeast/PaintedLayer",color);return l;}
 public static void BuildAll(){BuildScene();BuildPlayer();}
 public static void BuildPlayer(){var dir=Work+"/Builds/Windows-Level05-NOVA";Directory.CreateDirectory(dir);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.StandaloneWindows64,locationPathName=dir+"/LightFeast.exe"});File.WriteAllText(Work+"/Validation/level05/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Fifth build failed");}
 public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/FifthPumpHouse.unity");EditorApplication.ExecuteMenuItem("Window/General/Game");}
}
