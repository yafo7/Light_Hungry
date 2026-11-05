using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

public static class SixthLevelBuilder {
 const string Art="Assets/Art/Level06/",Geo="Assets/Art/GardenReusableGeometry/",Mats="Assets/Materials/GardenReusable/",Prefabs="Assets/Prefabs/GardenReusable/";
 static string Work=>Path.GetFullPath(Application.dataPath+"/../..");
 static Transform bg,terrain,props;static PhysicsMaterial2D friction;
 static Vector3 P(float x,float y,float z=0)=>SixthGreenhouseFlow.Px(x,y,z);
 // Atlas UVs are pixels measured in the original alpha image. Updated after art review.
 static readonly Rect BankCrop=new Rect(28,159,464,232),GlassCrop=new Rect(544,210,447,156),StepCrop=new Rect(1079,208,388,154),ClothCrop=new Rect(173,529,186,468),FlankCrop=new Rect(536,537,455,455),ArchCrop=new Rect(1084,539,388,453);
 static readonly Rect MirrorCrop=new Rect(148,83,245,395),BaseCrop=new Rect(606,172,325,259),WinchCrop=new Rect(1066,146,410,284),TrayCrop=new Rect(15,688,482,178),PulleyCrop=new Rect(637,577,268,359),LeverCrop=new Rect(1149,575,263,312);
 [MenuItem("食光兽/搭建第六关：温室苗床")]
 public static void BuildScene(){
  foreach(var d in new[]{Art,Geo,Mats,Prefabs,Work+"/Validation/level06"})Directory.CreateDirectory(d);
  foreach(var file in new[]{"background.png","terrain-atlas.png","props-atlas.png"})File.Copy(Work+"/art/level06-unity-assets-v01-2026-11-05/"+file,Art+file,true);
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var file in new[]{"background.png","terrain-atlas.png","props-atlas.png"}){var t=(TextureImporter)AssetImporter.GetAtPath(Art+file);t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();}
  BuildReusablePrefabs();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  bg=new GameObject("01_Background_NoCollision").transform;terrain=new GameObject("02_Horizontal_Foreground_Collisions").transform;props=new GameObject("03_Reusable_Interactive_Props").transform;
  friction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"background.png");Paint("Greenhouse_Background",bg,new Rect(0,0,1774,887),new Rect(0,0,tex.width,tex.height),"background.png",8,false);
  var f=new GameObject("SixthGreenhouse_CombinedPuzzle").AddComponent<SixthGreenhouseFlow>();
  Instance("Bank",terrain,0,710,580/500f,220/300f);Instance("Bank",terrain,1450,710,324/500f,220/300f);
  Solid("LeftBank_Collision",terrain,new Rect(0,710,580,220),6);Solid("RightBank_Collision",terrain,new Rect(1450,710,324,220),6);
  Instance("GlassDeck",terrain,310,275,270/400f,1);Instance("GlassDeck",terrain,1325,275,449/400f,1);
  Solid("C_GlassFloor",terrain,new Rect(310,275,270,8),10);Solid("D_GlassFloor",terrain,new Rect(1325,275,449,8),10);
  f.stepColliders=new Collider2D[2];var steps=new[]{new Rect(1090,735,70,33),new Rect(1180,710,70,33)};
  for(int j=0;j<2;j++){var r=steps[j];Instance("TerracottaStep",terrain,r.x,r.y,r.width/200,r.height/100);f.stepColliders[j]=Solid("ReturnStep_Collision_"+j,terrain,new Rect(r.x,r.y,r.width,6),6);}
  f.tray=Instance("NurseryTray",props,960,300,1,1).transform;
  var trayBody=f.tray.gameObject.AddComponent<Rigidbody2D>();trayBody.bodyType=RigidbodyType2D.Kinematic;trayBody.useFullKinematicContacts=true;
  f.trayCollider=LocalSolid("Tray_Walkable_Collision",f.tray,new Rect(-110,0,220,8),6);
  f.mirrors=new GardenReusableProp[4];f.mirrors[0]=Mirror("A_ReaimMirror",SixthGreenhouseFlow.A,props);f.mirrors[1]=Mirror("C_SlitCalibrationMirror",SixthGreenhouseFlow.C,props);
  f.mirrors[2]=Mirror("P_FixedRelayMirror",SixthGreenhouseFlow.HighP,f.tray);f.mirrors[3]=Mirror("D_FeedingMirror",SixthGreenhouseFlow.D,props);
  f.wheels=new Transform[5];f.wheels[0]=Handle("A_AimHandle",280,690);f.wheels[1]=Instance("GardenWinch",props,1540,685,1,1).transform;f.wheels[2]=Handle("K_FixedCurtainRelease",900,745);f.wheels[3]=Handle("C_CalibrationHandle",445,255);f.wheels[4]=Handle("D_FeedingHandle",1475,255);
  f.landings=new[]{Point("A_Safe",255,709),Point("B_Safe",1540,709),Point("C_Safe",420,274),Point("D_Safe",1450,274)};
  // A two-piece rendering and two-piece collider share exactly the same aperture.
  float leftHole=225+(620-420)*30f/540,rightHole=255+(1220-960)*(-30f)/490;
  Baffle("F_LeftSlit",620,leftHole);Baffle("Right_MissAbsorber",1220,rightHole);
  // One weathered greenhouse rail and four straps visually fasten both shades.
  // Art only: the exact optical apertures and their colliders remain unchanged.
  Paint("Shade_OverheadSupportRail",props,new Rect(550,8,740,16),new Rect(615,258,280,20),"terrain-atlas.png",-.75f,false);
  foreach(float x in new[]{606f,628f,1206f,1228f})Paint("Shade_SuspensionStrap_"+x,props,new Rect(x,18,8,25),new Rect(197,532,18,42),"terrain-atlas.png",-.8f,false);
  // Rolled shade stays above the A-C beam; its cloth lowers into that beam.
  var roller=Instance("Pulley",props,290,350,.65f,.65f);roller.name="G_RollShade_Spool";
  var shadeRoot=new GameObject("G_OpaqueRollShade").transform;shadeRoot.SetParent(props);shadeRoot.position=P(220,365,-.7f);f.curtain=shadeRoot;
  Paint("G_ShadeCloth",shadeRoot,new Rect(0,0,130,220),ClothCrop,"terrain-atlas.png",-.6f,true,true);
  f.curtainBlock=LocalSolid("G_ShadeCollision",shadeRoot,new Rect(0,0,130,220),6);
  Rope("K_To_G_CurtainCord",new[]{P(302,350,.4f),P(302,640,.4f),P(900,710,.4f),P(900,738,.4f)},.045f);
  Instance("Pulley",props,862,80,1,1);Instance("Pulley",props,1058,80,1,1);
  f.trayRopes=new[]{Rope("Tray_LeftBraidedRope",new[]{P(862,80,-.5f),P(862,300,-.5f)},.07f),Rope("Tray_RightBraidedRope",new[]{P(1058,80,-.5f),P(1058,300,-.5f)},.07f)};
  Rope("Winch_To_Roof_Pulley",new[]{P(1540,670,.4f),P(1540,65,.4f),P(862,65,.4f)},.07f);
  Paint("Ivory_LivingRibFlank",props,new Rect(1510,0,264,240),FlankCrop,"terrain-atlas.png",1,false);
  var scale=Paint("Target_DormantGrayScale",props,new Rect(1610,70,92,66),new Rect(720,690,110,120),"terrain-atlas.png",-.5f,false);
  // A softly curved, overlapping leaf-scale follows the neighbouring ivory scales.
  Shape(scale,new[]{new Vector2(.01f,.96f),new Vector2(.06f,.65f),new Vector2(.2f,.36f),new Vector2(.44f,.15f),new Vector2(.74f,.04f),new Vector2(1,0),new Vector2(.96f,.28f),new Vector2(.83f,.56f),new Vector2(.61f,.78f),new Vector2(.32f,.93f)},92,66,new Rect(720,690,110,120),"terrain-atlas.png");f.scaleSurface=scale.GetComponent<Renderer>();
  f.scaleGlow=SpiritEffects.Halo("Fed_RibScale_Glow",props,P(1660,105,-1),new Vector2(.85f,.95f),new Color(1,.8f,.3f,.7f));f.scaleGlow.enabled=false;
  Instance("RootArch",props,1660,145,1.1f,1.1f);f.rootGate=new GameObject("Exit_ClosedLeafCurtain").transform;f.rootGate.SetParent(props);f.rootGate.position=P(1718,170,-1);
  Paint("Exit_LeafCurtainPaint",f.rootGate,new Rect(-23,0,46,105),ClothCrop,"terrain-atlas.png",-1,true,true);f.exitBlock=LocalSolid("Exit_LeafCollision",f.rootGate,new Rect(-10,0,20,105),6);
  BuildPlayerObject(f);
  f.path=new GameObject("ONE_Source_EffectiveOpticalPath").AddComponent<LightPath>();f.path.chargeRadius=.32f;f.path.travelSpeed=4.8f;
  f.path.beams=new[]{Line("GoldenLight_Halo",new Vector3[2],.14f,new Color(1,.65f,.16f,.3f),true),Line("GoldenLight_Core",new Vector3[2],.04f,new Color(1,.95f,.65f,1),true)};
  var cam=new GameObject("Main Camera");cam.tag="MainCamera";var view=cam.AddComponent<Camera>();view.orthographic=true;view.orthographicSize=8.87f;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.18f,.15f);cam.transform.position=new Vector3(17.74f,8.87f,-40);cam.AddComponent<AudioListener>();f.cameraRig=cam.AddComponent<PotCamera>();f.cameraRig.view=view;f.cameraRig.player=f.player.transform;f.cameraRig.canvasWidth=35.48f;f.cameraRig.bottom=0;f.cameraRig.top=17.74f;f.cameraRig.playSize=5.5f;
  f.RefreshOptics();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/SixthGreenhouse.unity");
  var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path.EndsWith("SixthGreenhouse.unity")))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/SixthGreenhouse.unity",true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
  AssetDatabase.ExportPackage("Assets/Prefabs/GardenReusable",Work+"/art/level06-unity-assets-v01-2026-11-05/GardenReusable.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);Debug.Log("SIXTH_SCENE_READY");
 }
 static void BuildPlayerObject(SixthGreenhouseFlow f){
  var go=new GameObject("WickSpirit_Player");go.transform.position=f.landings[0].position+Vector3.back;
  var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var capsule=go.AddComponent<CapsuleCollider2D>();capsule.size=new Vector2(.42f,.94f);capsule.offset=new Vector2(0,.49f);capsule.sharedMaterial=friction;
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab"));visual.transform.SetParent(go.transform,false);
  f.player=go.AddComponent<PlayerMotor>();f.player.visual=visual.transform;f.player.speed=3.2f;f.player.jumpSpeed=11;f.player.groundMask=(1<<6)|(1<<10);
  var embers=SpiritEffects.Embers(go.transform);f.player.hairEmitter=embers.transform;f.ability=go.AddComponent<LightFormAbility>();f.ability.rearmOnMovement=true;f.ability.restoreSurfaces=1<<10;f.ability.shortDropRestore=true;f.ability.shortDropDistance=2.2f;
 }
 static void BuildReusablePrefabs(){
  Make("Bank","bank",new Rect(0,-14.2f,500,300),BankCrop,"terrain-atlas.png");
  Make("GlassDeck","glassDeck",new Rect(0,-20,400,100),GlassCrop,"terrain-atlas.png");
  Make("TerracottaStep","terracottaStep",new Rect(0,-9.74f,200,100),StepCrop,"terrain-atlas.png");
  Make("NurseryTray","tray",new Rect(-110,-32.86f,220,81.25f),TrayCrop,"props-atlas.png");
  Make("GardenWinch","winch",new Rect(-55,-51,110,76),WinchCrop,"props-atlas.png");
  Make("Pulley","pulley",new Rect(-24,-38,48,64.3f),PulleyCrop,"props-atlas.png");
  Make("Lever","lever",new Rect(-12,-17,24,34),LeverCrop,"props-atlas.png");
  Make("RootArch","rootArch",new Rect(0,0,100,118),ArchCrop,"terrain-atlas.png");
  var root=new GameObject("GardenMirror");var info=root.AddComponent<GardenReusableProp>();info.kind="mirror";
  Paint("GardenMirror_Base",root.transform,new Rect(-22,-6,44,56),BaseCrop,"props-atlas.png",0,true,true);
  Paint("GardenMirror_ShortMountingArm",root.transform,new Rect(-4,-2,8,30),new Rect(756,228,30,77),"props-atlas.png",-.2f,false,true);
  var pivot=new GameObject("RotatingMirrorHead").transform;pivot.SetParent(root.transform,false);info.movingPart=pivot;
  Paint("GardenMirror_Head",pivot,new Rect(-19.2f,-31,38.4f,62),MirrorCrop,"props-atlas.png",-.4f,true,true);
  var axle=Paint("GardenMirror_PivotPin",root.transform,new Rect(-4,-4,8,8),new Rect(751,764,42,42),"props-atlas.png",-.45f,false,true);
  Shape(axle,Enumerable.Range(0,16).Select(i=>new Vector2(.5f+Mathf.Cos(i*Mathf.PI/8)*.5f,.5f+Mathf.Sin(i*Mathf.PI/8)*.5f)).ToArray(),8,8,new Rect(751,764,42,42),"props-atlas.png");
  PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"GardenMirror.prefab");UnityEngine.Object.DestroyImmediate(root);
 }
 static void Make(string name,string kind,Rect r,Rect crop,string tex){var g=new GameObject(name);g.AddComponent<GardenReusableProp>().kind=kind;Paint(name+"_Paint",g.transform,r,crop,tex,0,true,true);PrefabUtility.SaveAsPrefabAsset(g,Prefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(g);}
 static GameObject Instance(string name,Transform parent,float x,float y,float sx,float sy){var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+name+".prefab"));g.transform.SetParent(parent);g.transform.position=P(x,y,-.3f);g.transform.localScale=new Vector3(sx,sy,1);return g;}
 static GardenReusableProp Mirror(string name,Vector3 pos,Transform parent){var g=Instance("GardenMirror",parent,pos.x*50,887-pos.y*50,1,1);g.name=name;return g.GetComponent<GardenReusableProp>();}
 static Transform Handle(string name,float x,float y){var g=Instance("Lever",props,x,y,1,1);g.name=name;return g.transform;}
 static Transform Point(string name,float x,float y){var t=new GameObject(name).transform;t.SetParent(props);t.position=P(x,y);return t;}
 static void Baffle(string name,float x,float y){
  var upper=new Rect(x-32,0,64,y-10);var lower=new Rect(x-32,y+10,64,620-y-10);
  foreach(var r in new[]{upper,lower}){string key=name+(r.y==0?"_Upper":"_Lower");Paint(key,terrain,r,ClothCrop,"terrain-atlas.png",-.4f,true);Solid(key+"_Collision",terrain,r,6);}
 }
 static BoxCollider2D Solid(string name,Transform parent,Rect r,int layer){var g=new GameObject(name);g.transform.SetParent(parent);g.layer=layer;g.transform.position=P(r.center.x,r.center.y);var c=g.AddComponent<BoxCollider2D>();c.size=r.size/50;c.sharedMaterial=friction;return c;}
 static BoxCollider2D LocalSolid(string name,Transform parent,Rect r,int layer){var g=new GameObject(name);g.transform.SetParent(parent,false);g.layer=layer;g.transform.localPosition=new Vector3(r.center.x/50,-r.center.y/50,0);var c=g.AddComponent<BoxCollider2D>();c.size=r.size/50;c.sharedMaterial=friction;return c;}
 static GameObject Paint(string name,Transform parent,Rect r,Rect crop,string tex,float z,bool interactive,bool local=false){
  var g=new GameObject(name);g.transform.SetParent(parent,false);if(local)g.transform.localPosition=new Vector3(r.x/50,-r.yMax/50,z);else g.transform.position=P(r.x,r.yMax,z);
  var t=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+tex);var m=new Mesh{name=name};float w=r.width/50,h=r.height/50;m.vertices=new[]{Vector3.zero,new Vector3(w,0),new Vector3(0,h),new Vector3(w,h)};m.uv=new[]{new Vector2(crop.x/t.width,1-crop.yMax/t.height),new Vector2(crop.xMax/t.width,1-crop.yMax/t.height),new Vector2(crop.x/t.width,1-crop.y/t.height),new Vector2(crop.xMax/t.width,1-crop.y/t.height)};m.triangles=new[]{0,2,1,1,2,3};m.RecalculateBounds();m.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=Save(m,Geo+name+".asset");var mat=Mat(name,interactive?"LightFeast/InteractablePaint":"LightFeast/PaintedLayer",Color.white);mat.mainTexture=t;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
 }
 static Material Mat(string n,string shader,Color color){var m=new Material(Shader.Find(shader));m.SetColor("_Color",color);return Save(m,Mats+n+".mat");}
 static void Shape(GameObject go,Vector2[] outline,float width,float height,Rect crop,string texture){
  var mesh=go.GetComponent<MeshFilter>().sharedMesh;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+texture);var v=new Vector3[outline.Length+1];var uv=new Vector2[v.Length];var tri=new int[outline.Length*3];
  Vector2 centre=Vector2.zero;foreach(var p in outline)centre+=p;centre/=outline.Length;
  for(int i=0;i<v.Length;i++){var q=i==0?centre:outline[i-1];v[i]=new Vector3(q.x*width/50,(1-q.y)*height/50,0);uv[i]=new Vector2((crop.x+q.x*crop.width)/tex.width,1-(crop.y+q.y*crop.height)/tex.height);}
  for(int i=0;i<outline.Length;i++){tri[i*3]=0;tri[i*3+1]=i+1;tri[i*3+2]=(i+1)%outline.Length+1;}mesh.Clear();mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
 }
 static T Save<T>(T obj,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);return old;}AssetDatabase.CreateAsset(obj,path);return obj;}
 static LineRenderer Line(string name,Vector3[] nodes,float width,Color color,bool glow=false){var l=new GameObject(name).AddComponent<LineRenderer>();l.transform.SetParent(props);l.useWorldSpace=true;l.positionCount=nodes.Length;l.SetPositions(nodes);l.widthMultiplier=width;l.numCapVertices=4;l.numCornerVertices=4;l.sharedMaterial=Mat(name,glow?"LightFeast/SoftLight":"LightFeast/PaintedLayer",color);return l;}
 static LineRenderer Rope(string name,Vector3[] nodes,float width){var l=Line(name,nodes,Mathf.Max(.1f,width),Color.white);l.textureMode=LineTextureMode.Tile;var m=Mat(name,"LightFeast/HempRope",new Color(.78f,.72f,.61f));m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Level03/rope-hemp.png");m.SetFloat("_TileLength",1.1f);l.sharedMaterial=m;var glow=l.gameObject.AddComponent<InteractionRopeGlow>();glow.glowMaterial=Mat("GardenRope_WhiteRim","LightFeast/SoftLight",new Color(.92f,.97f,1,.12f));return l;}
 public static void BuildAll(){BuildScene();BuildPlayer();}
 public static void BuildPlayer(){var dir=Work+"/Builds/Windows-Level06-NOVA";Directory.CreateDirectory(dir);Directory.CreateDirectory(Work+"/Validation/level06");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.StandaloneWindows64,locationPathName=dir+"/LightFeast.exe"});File.WriteAllText(Work+"/Validation/level06/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Sixth build failed");}
 public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/SixthGreenhouse.unity");EditorApplication.ExecuteMenuItem("Window/General/Game");}
}
