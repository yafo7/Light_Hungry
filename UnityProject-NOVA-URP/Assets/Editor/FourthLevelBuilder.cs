using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Builds only level four, preserving the existing URP/NOVA setup and earlier scenes.
public static class FourthLevelBuilder {
 const string Art="Assets/Art/Level04/",Geo="Assets/Art/Level04Geometry/",Mats="Assets/Materials/Level04/";
 const float PPU=80,Width=1859,Height=846;
 static string Work=>Environment.GetEnvironmentVariable("LIGHTFEAST_WORKSPACE")??Path.GetFullPath(Application.dataPath+"/../..");
 [Serializable]class Atlas {public int width,height;public AtlasProp[] props;}
 [Serializable]class AtlasProp {public string name;public float[] rect,pivot;}
 static Atlas atlas;static Transform backdrop,terrain,items,lights;static PhysicsMaterial2D friction;
 static Vector3 Px(float x,float y,float z=0)=>ReservoirMechanism.Px(x,y,z);

 [MenuItem("食光兽/搭建第四关：蓄水浅池")]
 public static void BuildScene(){
  if(EditorApplication.isPlaying)throw new Exception("Exit Play mode before building the scene.");
  EditorSceneManager.SaveOpenScenes();
  foreach(var dir in new[]{Art,Geo,Mats,Work+"/Validation/level04"})Directory.CreateDirectory(dir);
  string source=Work+"/art/level04-unity-assets-v01-2026-11-04/";
  foreach(var file in new[]{"clean-scene.png","props-atlas.png","runtime-atlas.json"})File.Copy(source+file,Art+file,true);
  File.Copy(Work+"/art/level03-rope-light-v07-2026-11-02/rope-hemp.png",Art+"rope-hemp.png",true);
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var file in new[]{"clean-scene.png","props-atlas.png","rope-hemp.png"}){
   var importer=(TextureImporter)AssetImporter.GetAtPath(Art+file);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=file!="clean-scene.png";
   importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
  }
  atlas=JsonUtility.FromJson<Atlas>(File.ReadAllText(Art+"runtime-atlas.json"));
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.78f,.79f,.73f);RenderSettings.fog=false;
  SetLayer(6,"Ground");SetLayer(11,"ReservoirOptics");SetLayer(12,"ReservoirSeed");
  friction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");
  backdrop=Group("01_PaintedGarden_Background");terrain=Group("02_Walkable_Foreground_And_Collision");items=Group("03_Independent_Interactive_Props");lights=Group("04_Effective_Light_And_BeastScale");
  SceneArt("Watercolor_Background",backdrop,new Rect(0,0,Width,Height),8);
  SceneArt("WaterSurface_Animated",backdrop,new Rect(0,0,Width,Height),6,"LightFeast/ReservoirWater");
  // Reuse the static paint as foreground occlusion: leaf tips that fold below ground disappear behind masonry.
  SceneArt("LeftBank_Occlusion",terrain,new Rect(0,565,370,281),.2f);
  SceneArt("MiddleIsland_Occlusion",terrain,new Rect(644,558,444,288),.2f);
  SceneArt("RightBank_Occlusion",terrain,new Rect(1415,565,444,281),.2f);
  Solid("LeftBank",new Rect(-30,565,400,340),6);
  Solid("MiddleIsland",new Rect(644,558,444,340),6);
  Solid("RightBank",new Rect(1415,565,480,340),6);
  Shelf("Jump_01_Low",650,749,417);Shelf("Jump_02_Right",720,820,345);Shelf("Jump_03_Left",680,773,281);
  Shelf("Upper_SeedTrough_And_OperatingDeck",760,1048,237);
  Solid("RootScreen_AboveSlit",new Rect(1100,-600,140,1050),6);
  Solid("RootScreen_BelowSlit",new Rect(1100,485,140,440),6);
  Solid("LeftWorldBoundary",new Rect(-35,-500,30,1500),6);Solid("RightWorldBoundary",new Rect(1863,-500,30,1500),6);

  var flow=new GameObject("FourthReservoir_FullLevel").AddComponent<ReservoirLevelFlow>();
  var mechanism=items.gameObject.AddComponent<ReservoirMechanism>();flow.mechanism=mechanism;
  flow.player=Player(Px(165,563,-1));flow.ability=flow.player.GetComponent<LightFormAbility>();mechanism.player=flow.player;mechanism.ability=flow.ability;
  flow.checkpoints=new[]{Point("CP0_DrainArrival",Px(165,563,-1),flow.transform),Point("CP1_MiddleIsland",Px(690,556,-1),flow.transform),Point("CP2_PumpShore",Px(1515,563,-1),flow.transform)};
  flow.finish=Point("PumpHouse_Exit",Px(1780,563,-1),flow.transform);
  mechanism.entryLanding=flow.checkpoints[1];mechanism.rightLanding=flow.checkpoints[2];
  mechanism.guideHandle=Point("Leaf_Handle_Interact",Px(1030,556),items);
  mechanism.seedPushPoint=Point("Upper_Right_SeedPush_Interact",Px(990,235),items);
  mechanism.mirrorHandle=Point("Mirror_Handle_Interact",Px(1525,563),items);
  mechanism.feedingPoint=Point("DarkScale_LightReceiver",Px(1671,346),lights);
  var receiver=Solid("FeedingScale_OpticalTarget",new Rect(1647,322,48,48),11,true);mechanism.feedingTarget=receiver.GetComponent<Collider2D>();mechanism.opticalMask=1<<11;
  mechanism.guide=Prop("guide","Folding_GuideLeaf",new Vector2(160,170),Px(830,440,.4f)).transform;mechanism.guideDownAngle=185;
  mechanism.shutter=Prop("shutter","WeightControlled_LeafShutter",new Vector2(100,138),Px(1163,310,.1f)).transform;mechanism.shutterTravel=.9f;
  mechanism.plate=Prop("plate","Pressure_Treadle",new Vector2(284,18),Px(1004,551,-.4f)).transform;
  var seed=Prop("seed","Heavy_Seedpod",new Vector2(48,50),Px(925,212,-.7f));mechanism.seed=seed.transform;
  seed.layer=12;var seedBody=seed.AddComponent<Rigidbody2D>();seedBody.bodyType=RigidbodyType2D.Kinematic;seedBody.interpolation=RigidbodyInterpolation2D.Interpolate;
  var seedCollider=seed.AddComponent<CircleCollider2D>();seedCollider.radius=.27f;seedCollider.sharedMaterial=friction;
  mechanism.seedRoute=new[]{Px(880,245,-.7f),Px(855,345,-.7f),Px(830,440,-.7f),Px(804,480,-.7f),Px(775,510,-.7f)};
  mechanism.seedTravelSeconds=2.6f;
  var fixedMirror=Prop("mirror","Fixed_Entrance_Reflector",new Vector2(26,68),Px(270,468,-.45f));fixedMirror.transform.rotation=Quaternion.Euler(0,0,-122.6f);
  mechanism.mirror=Prop("mirror","Adjustable_Terminal_Reflector",new Vector2(26,68),Px(1490,468,-.45f)).transform;mechanism.mirrorVisualAngleAtZero=90;mechanism.initialMirrorAngle=-12;
  var scale=Prop("scale","Beast_LowerBelly_TargetScale",new Vector2(105,96),Px(1671,346,-.3f),"LightFeast/ReservoirScale");mechanism.scaleArt=scale.GetComponent<Renderer>();
  mechanism.scaleGlow=Halo("FeedingScale_SoftGlow",mechanism.feedingPoint.position+Vector3.back*2,new Vector2(1.8f,1.6f),new Color(1,.8f,.35f,0));
  var burst=SpiritEffects.Embers(mechanism.feedingPoint);burst.name="Scale_FirstBreath_Particles";burst.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var bm=burst.main;bm.loop=false;bm.duration=1;bm.startLifetime=1.8f;bm.startSpeed=.25f;var be=burst.emission;be.rateOverTime=0;be.SetBursts(new[]{new ParticleSystem.Burst(0,24)});burst.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material("ScaleMotes","LightFeast/SoftLight",new Color(1,.8f,.45f,.6f));mechanism.awakening=burst;

  mechanism.entryPath=new GameObject("LightPath_FirstCrossing").AddComponent<LightPath>();mechanism.entryPath.transform.SetParent(lights);mechanism.entryPath.chargeRadius=.43f;mechanism.entryPath.travelSpeed=3.8f;
  mechanism.crossingPath=new GameObject("LightPath_WeightPuzzleCrossing").AddComponent<LightPath>();mechanism.crossingPath.transform.SetParent(lights);mechanism.crossingPath.chargeRadius=.43f;mechanism.crossingPath.travelSpeed=3.8f;
  mechanism.entryPath.nodes=new[]{Px(270,468),Px(710,468)};mechanism.entryPath.safeEnd=mechanism.entryLanding;
  mechanism.crossingPath.nodes=new[]{Px(710,468),Px(1090,468)};
  Beam("Sunlight_Soft",new[]{Px(105,105,-2.1f),Px(270,468,-2.1f)},.25f,new Color(1,.8f,.35f,.24f));
  Beam("Sunlight_Core",new[]{Px(105,105,-2.2f),Px(270,468,-2.2f)},.047f,new Color(1,.96f,.72f,.9f));
  mechanism.mainBeams=new[]{Beam("MainRay_Soft",new[]{Px(270,468),Px(1090,468)},.15f,new Color(1,.75f,.22f,.28f)),Beam("MainRay_Core",new[]{Px(270,468),Px(1090,468)},.035f,new Color(1,.97f,.78f,1))};
  mechanism.feedingBeams=new[]{Beam("FeedingRay_Soft",new[]{Px(1490,468),Px(1671,346)},.14f,new Color(1,.75f,.22f,.28f)),Beam("FeedingRay_Core",new[]{Px(1490,468),Px(1671,346)},.035f,new Color(1,.97f,.78f,1))};

  var pulleyL=Disc("Left_WeightPulley",new Rect(1018,108,55,55),Px(1045.5f,135.5f,.1f));
  var pulleyR=Disc("Right_ShutterPulley",new Rect(1107,88,55,55),Px(1134.5f,115.5f,.1f));mechanism.pulleys=new[]{pulleyL,pulleyR};
  var fixedRope=new[]{Px(1004,550,-.45f),Px(1065,550,-.45f),Px(1065,135,-.45f)}.Concat(Arc(1045,135,20,0,-90)).Concat(new[]{Px(1135,95,-.45f)}).Concat(Arc(1135,115,20,-90,0)).ToArray();
  Rope("Plate_To_Pulleys_HempRope",fixedRope);
  mechanism.shutterRope=Rope("Pulley_To_Shutter_MovingRope",new[]{Px(1155,115,-.45f),Px(1163,310,-.45f)});
  mechanism.shutterRopeTop=Point("Shutter_Rope_TopTangent",Px(1155,115,-.45f),items);

  var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6.6f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.20f,.17f);camera.nearClipPlane=.1f;camera.farClipPlane=100;cameraObject.transform.position=new Vector3(Width/PPU/2,Height/PPU/2,-40);cameraObject.AddComponent<AudioListener>();
  var rig=cameraObject.AddComponent<PotCamera>();rig.player=flow.player.transform;rig.view=camera;rig.canvasWidth=Width/PPU;rig.top=Height/PPU;rig.bottom=0;rig.playSize=3.65f;flow.cameraRig=rig;
  var sun=new GameObject("Spirit_KeyLight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.7f;sun.color=new Color(1,.94f,.79f);sun.transform.rotation=Quaternion.Euler(30,-30,0);
  PlayerSettings.runInBackground=true;PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=810;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
  EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/FourthReservoir.unity");
  var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path=="Assets/Scenes/FourthReservoir.unity"))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/FourthReservoir.unity",true));EditorBuildSettings.scenes=scenes.ToArray();
  AssetDatabase.SaveAssets();Debug.Log("FOURTH_RESERVOIR_SCENE_READY");
 }

 static Transform Group(string name)=>new GameObject(name).transform;
 static Transform Point(string name,Vector3 p,Transform parent){var g=new GameObject(name).transform;g.SetParent(parent);g.position=p;return g;}
 static Material Material(string name,string shader,Color color){var s=Shader.Find(shader);if(!s)throw new Exception("Missing shader "+shader);var m=new Material(s){name=name};if(m.HasProperty("_Color"))m.SetColor("_Color",color);return Asset(m,Mats+name+".mat");}
 static T Asset<T>(T value,string file)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(file);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}AssetDatabase.CreateAsset(value,file);return value;}
 static GameObject Quad(string name,Transform parent,Vector3 position,Vector2 size,Vector2 pivot,Rect uv,string texture,string shader){
  var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=position;
  var m=new Mesh{name=name};float l=-pivot.x*size.x,r=(1-pivot.x)*size.x,b=-pivot.y*size.y,t=(1-pivot.y)*size.y;
  m.vertices=new[]{new Vector3(l,b,0),new Vector3(r,b,0),new Vector3(l,t,0),new Vector3(r,t,0)};m.uv=new[]{new Vector2(uv.x,uv.y),new Vector2(uv.xMax,uv.y),new Vector2(uv.x,uv.yMax),new Vector2(uv.xMax,uv.yMax)};m.triangles=new[]{0,2,1,1,2,3};m.RecalculateNormals();m.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=Asset(m,Geo+name+".asset");
  var mat=Material(name,shader,Color.white);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+texture);g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
 }
 static GameObject SceneArt(string name,Transform parent,Rect rect,float z,string shader="LightFeast/PaintedLayer")=>Quad(name,parent,Px(rect.x,rect.yMax,z),new Vector2(rect.width/PPU,rect.height/PPU),Vector2.zero,new Rect(rect.x/Width,1-rect.yMax/Height,rect.width/Width,rect.height/Height),"clean-scene.png",shader);
 static GameObject Prop(string id,string name,Vector2 size,Vector3 pivot,string shader="LightFeast/InteractablePaint"){
  var p=atlas.props.First(a=>a.name==id);var q=p.rect;return Quad(name,items,pivot,size/PPU,new Vector2(p.pivot[0],p.pivot[1]),new Rect(q[0]/atlas.width,1-(q[1]+q[3])/atlas.height,q[2]/atlas.width,q[3]/atlas.height),"props-atlas.png",shader);
 }
 static GameObject Solid(string name,Rect rect,int layer,bool trigger=false){var g=new GameObject(name);g.transform.SetParent(terrain);g.transform.position=Px(rect.center.x,rect.center.y);g.layer=layer;var c=g.AddComponent<BoxCollider2D>();c.size=rect.size/PPU;c.isTrigger=trigger;c.sharedMaterial=friction;return g;}
 static void Shelf(string name,float left,float right,float y){var g=Solid(name,new Rect(left,y,right-left,8),6);var c=g.GetComponent<BoxCollider2D>();c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useSideFriction=false;e.useSideBounce=false;e.surfaceArc=180;}
 static PlayerMotor Player(Vector3 pos){var g=new GameObject("WickSpirit_Player");g.transform.position=pos;var b=g.AddComponent<Rigidbody2D>();b.gravityScale=3;b.freezeRotation=true;b.interpolation=RigidbodyInterpolation2D.Interpolate;b.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var c=g.AddComponent<CapsuleCollider2D>();c.size=new Vector2(.42f,.94f);c.offset=new Vector2(0,.49f);c.sharedMaterial=friction;var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab");if(!prefab)throw new Exception("Existing rigged spirit prefab not found");var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab);visual.transform.SetParent(g.transform,false);
  var motor=g.AddComponent<PlayerMotor>();motor.visual=visual.transform;motor.groundMask=1<<6;motor.speed=3.2f;motor.jumpSpeed=11;var ps=SpiritEffects.Embers(g.transform);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material("SpiritMotes","LightFeast/SoftLight",new Color(1,.83f,.48f,.65f));motor.hairEmitter=ps.transform;g.AddComponent<LightFormAbility>().rearmOnMovement=true;return motor;
 }
 static LineRenderer Beam(string name,Vector3[] positions,float width,Color color){var g=new GameObject(name);g.transform.SetParent(lights);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=true;l.positionCount=positions.Length;l.SetPositions(positions);l.widthMultiplier=width;l.numCapVertices=5;l.numCornerVertices=4;l.sharedMaterial=Material(name,"LightFeast/SoftLight",color);return l;}
 static LineRenderer Rope(string name,Vector3[] points){var l=Beam(name,points,.052f,Color.white);l.transform.SetParent(items);l.textureMode=LineTextureMode.Tile;var m=Material(name,"LightFeast/HempRope",new Color(.78f,.72f,.62f,1));m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"rope-hemp.png");m.SetFloat("_TileLength",.55f);l.sharedMaterial=m;var outline=l.gameObject.AddComponent<InteractionRopeGlow>();outline.glowMaterial=Material(name+"_PaleEdge","LightFeast/SoftLight",new Color(.95f,.98f,1,.16f));return l;}
 static Vector3[] Arc(float x,float y,float radius,float from,float to)=>Enumerable.Range(0,13).Select(i=>{float a=Mathf.Lerp(from,to,i/12f)*Mathf.Deg2Rad;return Px(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,-.45f);}).ToArray();
 static Transform Disc(string name,Rect crop,Vector3 position){
  var g=new GameObject(name);g.transform.SetParent(items);g.transform.position=position;var v=new Vector3[34];var uv=new Vector2[34];var tris=new int[32*3];v[0]=Vector3.zero;uv[0]=new Vector2(crop.center.x/Width,1-crop.center.y/Height);
  for(int n=0;n<=32;n++){float a=n/32f*Mathf.PI*2;float x=Mathf.Cos(a)*crop.width*.5f,y=Mathf.Sin(a)*crop.height*.5f;v[n+1]=new Vector3(x/PPU,y/PPU,0);uv[n+1]=new Vector2((crop.center.x+x)/Width,1-(crop.center.y-y)/Height);if(n<32){tris[n*3]=0;tris[n*3+1]=n+2;tris[n*3+2]=n+1;}}
  var mesh=new Mesh{name=name};mesh.vertices=v;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateBounds();mesh.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=Asset(mesh,Geo+name+".asset");var mat=Material(name,"LightFeast/PaintedLayer",Color.white);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"clean-scene.png");g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g.transform;
 }
 static Renderer Halo(string name,Vector3 position,Vector2 size,Color color){var r=SpiritEffects.Halo(name,lights,Vector3.zero,size,color);r.transform.position=position;r.GetComponent<MeshFilter>().sharedMesh=Asset(r.GetComponent<MeshFilter>().sharedMesh,Geo+name+".asset");var mat=Material(name,"LightFeast/SoftLight",color);mat.SetFloat("_Radial",1);r.sharedMaterial=mat;return r;}
 static void SetLayer(int n,string name){var s=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);s.FindProperty("layers").GetArrayElementAtIndex(n).stringValue=name;s.ApplyModifiedProperties();}
 [MenuItem("食光兽/构建第四关 Windows 试玩")]
 public static void BuildAll(){BuildScene();BuildPlayer();}
 public static void BuildPlayer(){var output=Work+"/Builds/Windows-Level04-NOVA";Directory.CreateDirectory(output);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output+"/LightFeast.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});Directory.CreateDirectory(Work+"/Validation/level04");File.WriteAllText(Work+"/Validation/level04/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+",\"bytes\":"+report.summary.totalSize+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Level04 player build failed");Debug.Log("FOURTH_RESERVOIR_BUILD_SUCCEEDED");}
 public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/FourthReservoir.unity");EditorApplication.delayCall+=()=>{var v=SceneView.lastActiveSceneView;if(v){v.in2DMode=true;v.pivot=new Vector3(11.6f,5.3f,0);v.size=7;}EditorApplication.ExecuteMenuItem("Window/General/Game");};}
}
