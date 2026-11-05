using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ThirdLevelBuilder {
 const string Art="Assets/Art/Level03/",Geo="Assets/Art/Level03Geometry/",Mats="Assets/Materials/Level03/";
 static string Work=>Environment.GetEnvironmentVariable("LIGHTFEAST_WORKSPACE")??Path.GetFullPath(Application.dataPath+"/../..");
 static Transform back,terrain,water,items,lighting;static PhysicsMaterial2D noFriction;
 static Vector3 Px(float x,float y,float z=0)=>DrainMechanism.Px(x,y,z);
 [MenuItem("食光兽/搭建第三关：排水暗渠")]
 public static void BuildScene(){
  if(EditorApplication.isPlaying)throw new Exception("Exit play mode first.");
  WickRigSetup.Prepare();
  EditorSceneManager.SaveOpenScenes();Directory.CreateDirectory(Art);Directory.CreateDirectory(Geo);Directory.CreateDirectory(Mats);Directory.CreateDirectory(Work+"/Validation/level03");
  string backup=Work+"/Validation/level03/ThirdDrain-arrival-before-v05.unity";if(!File.Exists(backup)&&File.Exists("Assets/Scenes/ThirdDrain.unity"))File.Copy("Assets/Scenes/ThirdDrain.unity",backup);
  string src=Work+"/art/level03-continuous-layers-v04-2026-10-31/";
  string[] names={"00-background.png","10-platforms.png","20-water.png","90-camera-roots.png","props-atlas.png"};
  foreach(string name in names){File.Copy(src+(name=="props-atlas.png"?"Props/":"Layers/")+name,Art+name,true);}
  File.Copy(Work+"/art/level03-rope-light-v07-2026-11-02/rope-hemp.png",Art+"rope-hemp.png",true);
  File.Copy(src+"layer-manifest.json",Art+"source-layer-manifest.json",true);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var name in names){var t=(TextureImporter)AssetImporter.GetAtPath(Art+name);t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.SaveAndReimport();}
  var ropeImport=(TextureImporter)AssetImporter.GetAtPath(Art+"rope-hemp.png");ropeImport.textureType=TextureImporterType.Default;ropeImport.alphaIsTransparency=true;ropeImport.mipmapEnabled=false;ropeImport.npotScale=TextureImporterNPOTScale.None;ropeImport.maxTextureSize=4096;ropeImport.textureCompression=TextureImporterCompression.Uncompressed;ropeImport.wrapMode=TextureWrapMode.Clamp;ropeImport.filterMode=FilterMode.Bilinear;ropeImport.SaveAndReimport();
  string[] checks=names.Select(n=>{string source=src+(n=="props-atlas.png"?"Props/":"Layers/")+n;return "{\"file\":\""+n+"\",\"identical\":"+(Hash(source)==Hash(Art+n)?"true":"false")+",\"sha256\":\""+Hash(Art+n)+"\"}";}).ToArray();File.WriteAllText(Work+"/Validation/level03/art-integrity.json","["+string.Join(",",checks)+"]");
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.58f,.62f,.58f);RenderSettings.fog=false;
  SetLayer(6,"Ground");SetLayer(10,"DrainOptics");
  noFriction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");
  back=Group("01_Background_World");water=Group("02_Water_Animated");terrain=Group("03_WalkableTerrain_XY");items=Group("04_Mechanisms_Independent");lighting=Group("05_LightPath_AndScale");
  ArtRect("Drain_Background",back,"00-background.png",new Rect(0,10,2170,702),2170,725,new Rect(0,0,2170,725),8);
  var rearWater=ArtRect("Waterfalls_AndRearWater",water,"20-water.png",new Rect(0,0,2170,725),2170,725,new Rect(0,0,2170,725),3,"LightFeast/DrainWater");rearWater.GetComponent<Renderer>().sharedMaterial.renderQueue=2900;
  ArtRect("MossPlatforms_Main",terrain,"10-platforms.png",new Rect(0,0,1775,725),2170,725,new Rect(0,0,1775,725),1);
  // A missing intermediate backtracking shelf is a native UV instance, not an edit of the painted source.
  ArtRect("B_ReturnShelf_Art",terrain,"10-platforms.png",new Rect(1095,350,105,95),2170,725,new Rect(1200,375,80,72),.6f);
  ArtRect("C_FinalPillar_1",terrain,"10-platforms.png",new Rect(1980,325,100,237),2170,725,new Rect(1985,414.4f,45,148),1);
  ArtRect("C_FinalPillar_2",terrain,"10-platforms.png",new Rect(1980,325,100,237),2170,725,new Rect(2050,370.2f,45,188),1);
  ArtRect("C_FinalPillar_3",terrain,"10-platforms.png",new Rect(1980,325,100,237),2170,725,new Rect(2110,325,60,237),1);
  Column("A_Entry",0,135,372);Column("A_UpperStep",158,231,357);Column("A_LeftWinchBank",247,455,424);
  Column("A_LowRecovery1",48,135,446);Column("A_LowRecovery2",160,232,447);Column("A_LowRecovery3",227,275,479);
  Column("A_RightLanding",700,813,417);Column("B_UpperWinchBank",790,1060,355);Column("B_DescentFoot",792,904,491);
  Column("B_LowerWalk",982,1276,519);Shelf("B_SmallStep",1195,1268,446);Column("B_TurnPillar_2",1269,1330,426);
  Shelf("B_ReturnShelf_3",1203,1276,391);Shelf("B_ReturnShelf_4",1100,1197,375);Shelf("B_UpperShelf_5",1196,1271,307);Shelf("B_UpperShelf_6",1278,1359,287);
  Column("B_TopRest",1362,1648,278);Column("C_FerryBank",1640,1715,400);Column("C_LowerRecovery",1695,1764,466);
  Column("C_FinalStep_1",1985,2030,430);Column("C_FinalStep_2",2050,2095,390);Column("C_FinalLanding",2110,2170,350);
  WorldBox("LeftBoundary",new Vector2(-.25f,8),new Vector2(.5f,40),terrain,6,false);WorldBox("RightBoundary",new Vector2(77.8f,8),new Vector2(.6f,40),terrain,6,false);
  // Optical wall pieces describe the painted pier's real opening. They do not become invisible walking walls.
  PixelBox("Pier_AboveAperture",new Rect(1155,-50,50,244),lighting,10,true);
  PixelBox("Pier_BelowAperture",new Rect(1155,238,50,480),lighting,10,true);
  var flow=new GameObject("ThirdDrain_FullLevel").AddComponent<DrainLevelFlow>();var mechanism=items.gameObject.AddComponent<DrainMechanism>();flow.mechanism=mechanism;
  // Five marked rope bends; the two old heads and the span over the counterweight are removed.
  foreach(float x in new[]{404f,521f,663f,696f,918f}){
   var pulley=Prop("WallPulley_"+x,new Rect(955,414,60,60),new Rect(x-14,98,28,28));
   CenterPivot(pulley,Px(x,112,.5f));
   var spin=pulley.AddComponent<DrainPulley>();spin.mechanism=mechanism;spin.direction=(x==404||x==663)?-1:1;
   Line("WallAnchor_"+x,new[]{Px(x-8,92,.9f),Px(x+8,92,.9f),Px(x,92,.9f),Px(x,112,.9f)},.075f,new Color(.29f,.24f,.16f,1),items,"Unlit/Color");
  }
  var bulb=Prop("Underground_LuminousBulb",new Rect(18,0,397,388),new Rect(673,-3,126,124));mechanism.bulbArt=bulb.GetComponent<Renderer>();
  Prop("Touch_RootKnot",new Rect(450,8,365,400),new Rect(88,304,51,56));mechanism.rootSwitch=Point("Root_Interact",Px(105,370),items);
  Prop("Winch_Left",new Rect(444,505,368,235),new Rect(379,383,64,42));Prop("Winch_Right",new Rect(444,505,368,235),new Rect(878,314,64,42));
  mechanism.leftWinch=Point("Left_Handle",Px(411,422),items);mechanism.rightWinch=Point("Right_Handle",Px(910,353),items);
  var mirror=Prop("HeightAdjustable_Reflector",new Rect(910,75,282,274),new Rect(708,405,56,54.4f));CenterPivot(mirror,Px(736,425,-.3f));mechanism.mirror=mirror.transform;
  var counter=Prop("Counterweight_Platform",new Rect(16,514,386,240),new Rect(510,280,164,102));CenterPivot(counter,Px(592,315,.5f));mechanism.counterweight=Moving(counter,152/28f,false);
  var ferry=Prop("Waterwheel_Ferry",new Rect(16,947,402,214),new Rect(1763,379,84,45));CenterPivot(ferry,Px(1805,396,.5f));var movement=Moving(ferry,78/28f,true);movement.start=Px(1805,396);movement.end=Px(1935,396);movement.period=7;
  var wheel=Prop("Waterwheel_Rotor",new Rect(448,844,368,359),new Rect(1810,438,134,131));CenterPivot(wheel,Px(1877,503.5f,.6f));mechanism.wheel=wheel.transform;
  Line("Ferry_BackRail",new[]{Px(1719,407,1.1f),Px(2010,407,1.1f)},.16f,new Color(.22f,.17f,.1f,1),items,"Unlit/Color");
  Line("Mirror_WallGuide",new[]{Px(710,155,.9f),Px(710,466,.9f)},.07f,new Color(.28f,.24f,.18f,1),items,"Unlit/Color");
  foreach(float y in new[]{160f,290f,460f})Line("Mirror_WallFixing_"+y,new[]{Px(704,y,1),Px(718,y,1)},.09f,new Color(.28f,.24f,.18f,1),items,"Unlit/Color");
  var leftRoute=new[]{Px(403,404,-.6f),Px(390,404,-.6f)}.Concat(RopeBend(404,180,270)).Concat(RopeBend(521,270,360)).ToArray();
  Line("Winch_Left_RootRope",leftRoute,.16f,Color.white,items,"LightFeast/HempRope");
  var rightRoute=new[]{Px(903,335,-.6f),Px(932,335,-.6f)}.Concat(RopeBend(918,0,-90)).Concat(RopeBend(663,270,180)).ToArray();
  Line("Winch_Right_RootRope",rightRoute,.16f,Color.white,items,"LightFeast/HempRope");
  Line("Mirror_TopBend_Rope",RopeBend(696,270,360),.16f,Color.white,items,"LightFeast/HempRope");
  mechanism.ropes=new[]{Line("Counterweight_Rope_L",new[]{Px(535,112),Px(535,295)},.045f,new Color(.59f,.46f,.28f,1),items,"Unlit/Color"),Line("Counterweight_Rope_R",new[]{Px(649,112),Px(649,295)},.045f,new Color(.59f,.46f,.28f,1),items,"Unlit/Color"),Line("Mirror_LiftingRope",new[]{Px(710,98),Px(710,405)},.04f,new Color(.59f,.46f,.28f,1),items,"Unlit/Color")};
  var scale=Prop("Beast_ExposedFeedingScale",new Rect(897,833,340,421),new Rect(2022,125,160,198));mechanism.scaleArt=scale.GetComponent<Renderer>();mechanism.feedingPoint=Point("Light_Receiver",Px(2075,215,-.8f),lighting);
  var target=mechanism.feedingPoint.gameObject.AddComponent<CircleCollider2D>();target.radius=.65f;target.isTrigger=true;target.gameObject.layer=10;mechanism.opticalTarget=target;mechanism.opticalMask=1<<10;
  mechanism.source=Point("Bulb_EmissionPoint",Px(736,112,-2.2f),lighting);
  var path=lighting.gameObject.AddComponent<LightPath>();path.nodes=new[]{mechanism.source.position,mirror.transform.position,Px(1155,425)};path.travelSpeed=4.2f;path.chargeRadius=.24f;path.beams=new[]{Line("Light_WarmHalo",path.nodes,.9f,new Color(1,.60f,.16f,.22f),lighting,"LightFeast/SoftLight"),Line("Light_GoldenShaft",path.nodes,.42f,new Color(1,.73f,.28f,.65f),lighting,"LightFeast/SoftLight"),Line("Light_BrightCore",path.nodes,.20f,new Color(1,.96f,.72f,1),lighting,"LightFeast/SoftLight")};mechanism.path=path;
  mechanism.bulbGlow=Halo("Bulb_Glow",lighting,Px(736,92,-2.1f),new Vector2(4.2f,4.5f),new Color(1,.7f,.25f,0));mechanism.scaleGlow=Halo("Scale_BreathingGlow",lighting,Px(2075,215,-2.1f),new Vector2(4.5f,5),new Color(1,.73f,.25f,0));
  var bursts=SpiritEffects.Embers(mechanism.feedingPoint);bursts.name="Scale_AwakeningParticles";bursts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=bursts.main;main.loop=false;main.duration=1;main.startLifetime=1.8f;main.startSpeed=.55f;var emission=bursts.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,40)});bursts.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material("ScaleParticles","LightFeast/SoftLight",new Color(1,.8f,.4f,.8f));mechanism.awakening=bursts;
  var frontWater=ArtRect("FrontWater_SubmergedEdges",water,"20-water.png",new Rect(0,542,2170,183),2170,725,new Rect(0,542,2170,183),-3,"LightFeast/DrainWater");frontWater.GetComponent<Renderer>().sharedMaterial.renderQueue=3010;
  flow.player=Player(Px(68,370,-1));flow.ability=flow.player.GetComponent<LightFormAbility>();
  flow.checkpoints=new[]{Point("CP0_Entry",Px(68,370,-1),flow.transform),Point("CP1_LeftWinch",Px(406,422,-1),flow.transform),Point("CP2_RightWinch",Px(907,353,-1),flow.transform),Point("CP3_UpperRest",Px(1415,276,-1),flow.transform),Point("CP4_FerryBank",Px(1677,398,-1),flow.transform)};
  flow.finish=Point("Finish_SafeLedge",Px(2130,348,-1),flow.transform);
  var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";cameraObject.transform.position=new Vector3(38.75f,6.41f,-40);var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=22;camera.backgroundColor=new Color(.025f,.045f,.053f);camera.clearFlags=CameraClearFlags.SolidColor;camera.nearClipPlane=.1f;camera.farClipPlane=100;cameraObject.AddComponent<AudioListener>();var rig=cameraObject.AddComponent<DrainCamera>();rig.player=flow.player.transform;rig.view=camera;flow.cameraRig=rig;
  var frame=ArtRect("06_CameraFixed_RootFrame",cameraObject.transform,"90-camera-roots.png",new Rect(0,0,1672,941),1672,941,new Rect(0,0,28,28),-38);CenterPivot(frame,frame.transform.position+new Vector3(.5f,.5f,0));frame.transform.SetParent(cameraObject.transform,false);frame.transform.localPosition=new Vector3(0,0,2);rig.frame=frame.transform;
  PlayerSettings.runInBackground=true;PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=810;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
  EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/ThirdDrain.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/FirstLight.unity",true),new EditorBuildSettingsScene("Assets/Scenes/SecondLight.unity",true),new EditorBuildSettingsScene("Assets/Scenes/ThirdDrain.unity",true)};AssetDatabase.SaveAssets();Debug.Log("THIRD_DRAIN_SCENE_READY");
 }
 static Transform Group(string name)=>new GameObject(name).transform;
 static Vector3[] RopeBend(float cx,float from,float to){return Enumerable.Range(0,13).Select(i=>{float a=Mathf.Lerp(from,to,i/12f)*Mathf.Deg2Rad;return Px(cx+14*Mathf.Cos(a),112+14*Mathf.Sin(a),-.6f);}).ToArray();}
 static Transform Point(string name,Vector3 p,Transform parent){var g=new GameObject(name).transform;g.SetParent(parent);g.position=p;return g;}
 static GameObject Prop(string name,Rect crop,Rect dest)=>ArtRect(name,items,"props-atlas.png",crop,1254,1254,dest,.5f,name=="Beast_ExposedFeedingScale"?"LightFeast/BeastScale":"LightFeast/InteractablePaint");
 static GameObject ArtRect(string name,Transform parent,string file,Rect crop,float tw,float th,Rect dest,float z,string shader="LightFeast/PaintedLayer"){
  var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=Px(dest.x,dest.yMax,z);float w=dest.width/28,h=dest.height/28;
  var mesh=new Mesh{name=name};mesh.vertices=new[]{Vector3.zero,new Vector3(w,0),new Vector3(0,h),new Vector3(w,h)};mesh.uv=new[]{new Vector2(crop.x/tw,1-crop.yMax/th),new Vector2(crop.xMax/tw,1-crop.yMax/th),new Vector2(crop.x/tw,1-crop.y/th),new Vector2(crop.xMax/tw,1-crop.y/th)};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Asset(mesh,Geo+name+".asset");g.AddComponent<MeshFilter>().sharedMesh=mesh;var mat=Material(name,shader,Color.white);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+file);g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
 }
 static void CenterPivot(GameObject g,Vector3 point){var f=g.GetComponent<MeshFilter>();var mesh=f.sharedMesh;Vector3 delta=g.transform.position-point;var v=mesh.vertices;for(int i=0;i<v.Length;i++)v[i]+=delta;mesh.vertices=v;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);g.transform.position=point;}
 static void Column(string name,float left,float right,float top){PixelBox(name,new Rect(left,top,right-left,565-top),terrain,6,false);}
 static void Shelf(string name,float left,float right,float top){var g=PixelBox(name,new Rect(left,top,right-left,7),terrain,6,false);var c=g.GetComponent<BoxCollider2D>();c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useSideFriction=false;e.useSideBounce=false;e.surfaceArc=180;}
 static GameObject PixelBox(string name,Rect r,Transform parent,int layer,bool trigger)=>WorldBox(name,Px(r.center.x,r.center.y),new Vector2(r.width/28,r.height/28),parent,layer,trigger);
 static GameObject WorldBox(string name,Vector2 center,Vector2 size,Transform parent,int layer,bool trigger){var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=center;g.layer=layer;var c=g.AddComponent<BoxCollider2D>();c.size=size;c.isTrigger=trigger;c.sharedMaterial=noFriction;return g;}
 static DrainMovingSurface Moving(GameObject g,float width,bool ferry){g.layer=6;var b=g.AddComponent<Rigidbody2D>();b.bodyType=RigidbodyType2D.Kinematic;b.interpolation=RigidbodyInterpolation2D.Interpolate;b.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var c=g.AddComponent<BoxCollider2D>();c.size=new Vector2(width,.27f);c.offset=new Vector2(0,-.135f);c.sharedMaterial=noFriction;c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.surfaceArc=180;var m=g.AddComponent<DrainMovingSurface>();m.ferry=ferry;return m;}
 static PlayerMotor Player(Vector3 pos){var g=new GameObject("WickSpirit_Player");g.transform.position=pos;var b=g.AddComponent<Rigidbody2D>();b.gravityScale=3;b.freezeRotation=true;b.interpolation=RigidbodyInterpolation2D.Interpolate;b.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var c=g.AddComponent<CapsuleCollider2D>();c.size=new Vector2(.42f,.94f);c.offset=new Vector2(0,.49f);c.sharedMaterial=noFriction;var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab");if(!prefab)throw new Exception("Missing user character prefab");var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab);visual.transform.SetParent(g.transform,false);var m=g.AddComponent<PlayerMotor>();m.visual=visual.transform;m.groundMask=1<<6;m.speed=6;m.jumpSpeed=13.5f;m.movingSurfaceSupport=true;var ps=SpiritEffects.Embers(g.transform);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material("WickEmbers","LightFeast/SoftLight",new Color(1,.8f,.4f,.8f));m.hairEmitter=ps.transform;g.AddComponent<LightFormAbility>();return m;}
 static LineRenderer Line(string name,Vector3[] points,float width,Color color,Transform parent,string shader){var g=new GameObject(name);g.transform.SetParent(parent);var l=g.AddComponent<LineRenderer>();l.positionCount=points.Length;l.SetPositions(points);l.widthMultiplier=width;l.useWorldSpace=true;l.numCapVertices=4;l.numCornerVertices=3;
  if(name.Contains("Rope")){l.widthMultiplier=.16f;l.textureMode=LineTextureMode.Tile;var mat=Material(name,"LightFeast/HempRope",new Color(.85f,.8f,.74f,1));mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"rope-hemp.png");mat.SetFloat("_TileLength",1.1f);l.sharedMaterial=mat;}
  else l.sharedMaterial=Material(name,shader,color);
  if(name.Contains("Rope")){var outline=g.AddComponent<InteractionRopeGlow>();outline.glowMaterial=Material("RopeSoftWhiteOutline","LightFeast/SoftLight",new Color(.92f,.97f,1,.26f));}
  return l;}
 static Renderer Halo(string name,Transform parent,Vector3 pos,Vector2 size,Color color){var r=SpiritEffects.Halo(name,parent,Vector3.zero,size,color);r.transform.position=pos;r.GetComponent<MeshFilter>().sharedMesh=Asset(r.GetComponent<MeshFilter>().sharedMesh,Geo+name+".asset");var m=Material(name,"LightFeast/SoftLight",color);m.SetFloat("_Radial",1);r.sharedMaterial=m;return r;}
 static Material Material(string name,string shader,Color color){var s=Shader.Find(shader);if(!s)throw new Exception("Missing shader "+shader);var m=new Material(s){name=name};m.SetColor("_Color",color);return Asset(m,Mats+name+".mat");}
 static T Asset<T>(T obj,string file)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(file);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);return old;}AssetDatabase.CreateAsset(obj,file);return obj;}
 static void SetLayer(int i,string name){var t=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);t.FindProperty("layers").GetArrayElementAtIndex(i).stringValue=name;t.ApplyModifiedProperties();}
 static string Hash(string file){using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();}
 [MenuItem("食光兽/构建第三关 Windows 试玩")]
 public static void BuildAll(){BuildScene();string dir=Work+"/Builds/Windows-Level03";Directory.CreateDirectory(dir);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),locationPathName=dir+"/LightFeast.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText(Work+"/Validation/level03/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+",\"bytes\":"+report.summary.totalSize+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("ThirdDrain build failed");Debug.Log("THIRD_DRAIN_BUILD_SUCCEEDED");}
 public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/ThirdDrain.unity");EditorApplication.delayCall+=()=>{var v=SceneView.lastActiveSceneView;if(v){v.in2DMode=true;v.rotation=Quaternion.identity;v.pivot=new Vector3(26,8,0);v.size=12;}EditorApplication.ExecuteMenuItem("Window/General/Game");};}
}
