using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// Only builds the seventh scene; the six earlier authored scenes stay intact.
public static class SeventhLevelBuilder {
 const string Art="Assets/Art/Level07/", OldArt="Assets/Art/Level06/";
 const string Geo="Assets/Art/TrellisReusableGeometry/", Mats="Assets/Materials/TrellisReusable/";
 const string Prefabs="Assets/Prefabs/TrellisReusable/", Garden="Assets/Prefabs/GardenReusable/";
 static string Work=>Path.GetFullPath(Application.dataPath+"/../..");
 static readonly Rect FlankCrop=new Rect(536,537,455,455),ScaleCrop=new Rect(720,690,110,120);
 // Measured alpha bounds in the immutable atlas. Standing lines are below flowers
 // and the decorative far rail, and are offset explicitly in the prefab quads.
 static readonly Rect ShelfCrop=new Rect(38,118,446,270),BridgeCrop=new Rect(523,212,487,94),WindowCrop=new Rect(1141,68,277,362);
 static readonly Rect CartCrop=new Rect(44,662,453,244),LeafCrop=new Rect(637,521,261,481),RollCrop=new Rect(1061,681,438,207);
 static PhysicsMaterial2D friction;
 static Vector3 P(float x,float y,float z=0)=>new Vector3(x/50,(887-y)/50,z);
 static Transform Group(string name,Transform parent=null){var g=new GameObject(name).transform;if(parent)g.SetParent(parent,false);return g;}

 [MenuItem("食光兽/搭建第七关：叶幕走廊")]
 public static void BuildScene(){
  foreach(var d in new[]{Art,Geo,Mats,Prefabs,Work+"/Validation/level07"})Directory.CreateDirectory(d);
  foreach(string n in new[]{"background-front.png","background-back.png","trellis-atlas.png"}){
   File.Copy(Work+"/art/level07-unity-assets-v01-2026-11-05/"+n,Art+n,true);
  }
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(string n in new[]{"background-front.png","background-back.png","trellis-atlas.png"}){
   var importer=(TextureImporter)AssetImporter.GetAtPath(Art+n);
   importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
   importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
  }
  BuildPrefabs();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  friction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");
  var flow=Group("SeventhTrellis_CrossFacePuzzle").gameObject.AddComponent<SeventhTrellisFlow>();
  flow.faceRoots=new GameObject[2];var backgrounds=new Transform[2];var terrain=new Transform[2];var props=new Transform[2];
  for(int face=0;face<2;face++){
   var root=Group(face==0?"FACE_A_Front_LeafCorridor":"FACE_B_Back_Maintenance");flow.faceRoots[face]=root.gameObject;
   backgrounds[face]=Group("01_Background_NoCollision",root);terrain[face]=Group("02_HorizontalPlatforms_Collisions",root);props[face]=Group("03_ReusableProps_Mechanisms",root);
   string file=face==0?"background-front.png":"background-back.png";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+file);
   Paint("Backdrop_"+face,backgrounds[face],new Rect(0,0,1774,887),new Rect(0,0,texture.width,texture.height),Art+file,8,false);
  }
  Bank("Front_Entrance",terrain[0],0,580);Bank("Front_Exit",terrain[0],1380,394);
  Bank("Back_LeftA",terrain[1],0,500);Bank("Back_LeftB",terrain[1],500,500);Bank("Back_Right",terrain[1],1340,434);
  flow.frontSteps=new Transform[3];flow.frontStepColliders=new Collider2D[3];
  var frontRects=new[]{new Rect(600,650,130,8),new Rect(770,590,130,8),new Rect(940,530,130,8)};
  for(int j=0;j<3;j++){
   var r=frontRects[j];flow.frontSteps[j]=Instance("LeafShelf",terrain[0],r.x,r.y,r.width/140,1).transform;
   flow.frontSteps[j].name="Front_UnfoldingShelf_"+j;
   flow.frontStepColliders[j]=Solid("Front_ShelfCollision_"+j,terrain[0],r,6);flow.frontStepColliders[j].enabled=false;
   flow.frontSteps[j].localScale=new Vector3(r.width/140,.04f,1);
  }
  Platform("Front_KDeck",terrain[0],new Rect(1110,470,115,8));
  foreach(var r in new[]{new Rect(480,650,140,8),new Rect(660,590,140,8),new Rect(840,530,140,8),new Rect(1020,470,120,8)})Platform("Back_FixedShelf_"+r.x,terrain[1],r);

  // The permanent FRONT barrier is independent of the roll-up BACK barrier.
  LeafWall("Front_PermanentLeafWall",terrain[0],new Rect(1245,0,135,887),false);
  Solid("Front_PermanentOpaqueWallCollision",terrain[0],new Rect(1245,-80,135,967),6);
  flow.screen=Group("Back_RollingOpaqueScreen",terrain[1]);flow.screen.position=P(1190,0,-.45f);
  LeafWall("Back_Screen",flow.screen,new Rect(0,0,50,710),true);
  flow.screenBlock=Solid("Back_ClosedScreen_Collision",terrain[1],new Rect(1190,0,50,710),6);
  var roller=Instance("RollShade",props[1],1215,35,.65f,.65f);roller.name="Back_Screen_RollerAndMount";roller.transform.position=P(1215,35,-.8f);
  flow.bridge=Instance("RailBridge",terrain[1],1000,710,340/400f,1).transform;flow.bridge.name="Back_HingedRailBridge";
  flow.bridgeCollider=Solid("Back_OpenedBridge_Collision",terrain[1],new Rect(1000,710,340,8),6);flow.bridgeCollider.enabled=false;
  flow.bridge.localScale=new Vector3(340/400f,.04f,1);
  Rail("Back_LeftRail",props[1],250,1000);Rail("Back_RightRail",props[1],1340,1700);

  flow.switches=new[]{Instance("Lever",props[0],1170,450,1.3f,1.3f,true).transform,Instance("Lever",props[1],1080,450,1.3f,1.3f,true).transform};
  flow.switches[0].name="K_FRONT_OpensBackBridgeAndScreen";flow.switches[1].name="K_BACK_OpensFrontShelves";
  // Physical-looking transmission shafts disappear into the opposite-face trellis.
  Cord("FrontK_PassThroughShaft",props[0],new[]{P(1174,450,.35f),P(1220,450,.35f)},.12f);
  for(int j=0;j<3;j++){
   float y=450+j*65;
   var pin=Instance("Pulley",props[1],570,y,.33f,.33f,true);pin.name="Back_PassThrough_ToFrontShelf_"+j;
   Cord("BackK_ToOppositeShelf_"+j,props[1],new[]{P(1080,446-j*3,.25f),P(900,y-20,.25f),P(570,y,.25f)},.075f);
  }
  var coupling=Instance("Pulley",props[1],1210,450,.4f,.4f,true);coupling.name="Back_PassiveCoupling_FromKFront";
  Cord("KFront_Input_ToScreen",props[1],new[]{P(1210,450,.3f),P(1210,85,.3f),P(1215,60,.3f)},.08f);
  Cord("KFront_Input_ToBridge",props[1],new[]{P(1210,450,.3f),P(1170,620,.3f),P(1000,700,.3f)},.08f);

  Window("P_Front_SourceWindow",props[0],285,670);Window("R0_Front_ReturnWindow",props[0],480,670);Window("R1_Front_ReturnWindow",props[0],1455,670);
  Window("Q_Back_ReceiverWindow",props[1],285,670);Window("R0_Back_EmptyDockWindow",props[1],480,670);Window("R1_Back_DockWindow",props[1],1455,670);
  // A small absorbing cap ends the front R0 ray locally. Its back-plane casing
  // is decoration, so it doesn't add an unplanned obstacle before the jump chain.
  Paint("R0_OpaqueLightStop",props[0],new Rect(537,652,13,40),new Rect(173,529,186,468),OldArt+"terrain-atlas.png",-.8f,true);
  DockMark("R0",props[1],480);DockMark("R1",props[1],1455);
  flow.cart=Instance("MirrorCart",props[1],480,670,1,1).transform;flow.cart.name="ONE_ReversibleMirrorCart";
  flow.cartWheels=new Transform[0];
  // A local body region, never a complete creature, carries the independent target.
  // Continue the curved body below the bank rim and beyond the right frame.
  // The foreground bank and root gate conceal the lower crop, so the animal
  // reads as one large living surface rather than a floating rectangular card.
  var flank=Paint("Front_IvoryCreatureBackRegion",props[0],new Rect(1480,120,360,620),FlankCrop,OldArt+"terrain-atlas.png",1,false);
  Shape(flank,new[]{new Vector2(0,1),new Vector2(.015f,.87f),new Vector2(.04f,.71f),new Vector2(.095f,.56f),new Vector2(.18f,.39f),new Vector2(.3f,.25f),new Vector2(.46f,.13f),new Vector2(.64f,.05f),new Vector2(.83f,.012f),new Vector2(1,0),new Vector2(1,1)},360,620,FlankCrop,OldArt+"terrain-atlas.png");
  var scale=Paint("Front_GrayTargetScale",props[0],new Rect(1604,287,92,66),ScaleCrop,OldArt+"terrain-atlas.png",-.5f,false);
  Shape(scale,new[]{new Vector2(.01f,.96f),new Vector2(.06f,.65f),new Vector2(.2f,.36f),new Vector2(.44f,.15f),new Vector2(.74f,.04f),new Vector2(1,0),new Vector2(.96f,.28f),new Vector2(.83f,.56f),new Vector2(.61f,.78f),new Vector2(.32f,.93f)},92,66,ScaleCrop,OldArt+"terrain-atlas.png");flow.scaleSurface=scale.GetComponent<Renderer>();
  flow.scaleGlow=SpiritEffects.Halo("Front_FedScale_Glow",props[0],P(1650,320,-1),new Vector2(.9f,.9f),new Color(1,.8f,.3f,.65f));flow.scaleGlow.enabled=false;
  Instance("RootArch",props[0],1665,590,1.1f,1.1f,true);
  flow.rootGate=Group("Front_ExitRootCurtain",props[0]);flow.rootGate.position=P(1718,590,-1);
  Paint("Front_ExitCurtainPaint",flow.rootGate,new Rect(-22,0,44,120),LeafCrop,Art+"trellis-atlas.png",-.7f,true,true);
  flow.exitBlock=Solid("Front_ExitBlock",terrain[0],new Rect(1708,590,20,120),6);
  Solid("Front_LeftBoundary",terrain[0],new Rect(-25,-80,25,1000),6);Solid("Back_LeftBoundary",terrain[1],new Rect(-25,-80,25,1000),6);Solid("Back_RightBoundary",terrain[1],new Rect(1774,-80,25,1000),6);

  BuildPlayer(flow);
  flow.path=Group("ONE_Source_ConnectedFrontDepthBackDepthFrontPath").gameObject.AddComponent<SeventhFaceLightPath>();flow.path.flow=flow;flow.path.travelSpeed=4.8f;flow.path.chargeRadius=.32f;
  flow.path.frontSourceBeams=BeamPair("Front_Source",props[0]);flow.path.rearBeams=BeamPair("Back_LowRay",props[1]);flow.path.frontReturnBeams=BeamPair("Front_Return",props[0]);flow.path.beams=new LineRenderer[0];
  var cameraObject=Group("Main Camera").gameObject;cameraObject.tag="MainCamera";var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8.87f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.14f,.18f,.15f);cameraObject.transform.position=new Vector3(17.74f,8.87f,-40);cameraObject.AddComponent<AudioListener>();
  flow.cameraRig=cameraObject.AddComponent<SeventhFaceCamera>();flow.cameraRig.view=camera;flow.cameraRig.player=flow.player.transform;flow.cameraRig.flow=flow;flow.cameraRig.playSize=5.5f;flow.cameraRig.canvasWidth=35.48f;flow.cameraRig.height=17.74f;
  flow.faceRoots[1].SetActive(false);flow.RefreshOptics();
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/SeventhTrellis.unity");
  var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path.EndsWith("SeventhTrellis.unity")))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/SeventhTrellis.unity",true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
  AssetDatabase.ExportPackage(new[]{Prefabs.TrimEnd('/'),Garden.TrimEnd('/')},Work+"/art/level07-unity-assets-v01-2026-11-05/TrellisReusable.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
  Debug.Log("SEVENTH_SCENE_READY");
 }

 static void BuildPlayer(SeventhTrellisFlow flow){
  var go=Group("WickSpirit_Player").gameObject;go.transform.position=P(200,709,-1);
  var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var capsule=go.AddComponent<CapsuleCollider2D>();capsule.size=new Vector2(.42f,.94f);capsule.offset=new Vector2(0,.49f);capsule.sharedMaterial=friction;
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab"));visual.transform.SetParent(go.transform,false);
  flow.player=go.AddComponent<PlayerMotor>();flow.player.visual=visual.transform;flow.player.speed=3.2f;flow.player.jumpSpeed=11;flow.player.groundMask=(1<<6)|(1<<10);flow.player.hairEmitter=SpiritEffects.Embers(go.transform).transform;
  flow.ability=go.AddComponent<LightFormAbility>();flow.ability.rearmOnMovement=true;flow.ability.restoreSurfaces=1<<10;
 }
 static void BuildPrefabs(){
  Make("LeafShelf","leafShelf",new Rect(0,-72*140f/446,140,270*140f/446),ShelfCrop);
  Make("RailBridge","railBridge",new Rect(0,-35*400f/487,400,94*400f/487),BridgeCrop);
  Make("MirrorWindow","mirrorWindow",new Rect(-277*30f/362,-196*60f/362,277*60f/362,60),WindowCrop);
  Make("LeafPanel","leafPanel",new Rect(0,0,135,481*135f/261),LeafCrop);
  Make("RollShade","rollShade",new Rect(-85,0,170,207*170f/438),RollCrop);
  var root=Group("MirrorCart").gameObject;var info=root.AddComponent<GardenReusableProp>();info.kind="mirrorCart";
  Paint("MirrorCart_ChassisPaint",root.transform,new Rect(-40,40-244*80f/453,80,244*80f/453),CartCrop,Art+"trellis-atlas.png",-.1f,true,true);
  var mirror=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Garden+"GardenMirror.prefab"));mirror.name="Cart_IndependentReflector";mirror.transform.SetParent(root.transform,false);mirror.transform.localScale=Vector3.one*.28f;var part=mirror.GetComponent<GardenReusableProp>();part.movingPart.localRotation=Quaternion.Euler(0,0,-25);info.movingPart=part.movingPart;
  PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"MirrorCart.prefab");UnityEngine.Object.DestroyImmediate(root);
 }
 static void Make(string name,string kind,Rect r,Rect crop){var root=Group(name).gameObject;root.AddComponent<GardenReusableProp>().kind=kind;Paint(name+"_Paint",root.transform,r,crop,Art+"trellis-atlas.png",0,true,true);PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(root);}
 static GameObject Instance(string name,Transform parent,float x,float y,float sx=1,float sy=1,bool old=false){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>((old?Garden:Prefabs)+name+".prefab"));go.transform.SetParent(parent);go.transform.position=P(x,y,-.3f);go.transform.localScale=new Vector3(sx,sy,1);return go;}
 static void Bank(string name,Transform parent,float x,float width){var bank=Instance("Bank",parent,x,710,width/500,177/300f,true);bank.name=name+"_Paint";Solid(name+"_Collision",parent,new Rect(x,710,width,220),6);}
 static void Platform(string name,Transform parent,Rect r){var go=Instance("LeafShelf",parent,r.x,r.y,r.width/140,1);go.name=name;Solid(name+"_Collision",parent,r,6);}
 static void Window(string name,Transform parent,float x,float y){var go=Instance("MirrorWindow",parent,x,y,1,1);go.name=name;}
 static void DockMark(string name,Transform parent,float x){foreach(float dx in new[]{-36f,36f}){var pin=Instance("Lever",parent,x+dx,702,.42f,.42f,true);pin.name=name+"_CartDockStop_"+dx;}}
 static void Rail(string name,Transform parent,float left,float right){
  // Narrow timber/bronze strips use existing painted material and sit at the
  // bank's standing rim. Collision belongs to the bank, never the decorations.
  Paint(name,parent,new Rect(left,706,right-left,8),new Rect(615,258,280,20),OldArt+"terrain-atlas.png",-.4f,true);
 }
 static void LeafWall(string name,Transform parent,Rect area,bool local){
  float tile=area.width*LeafCrop.height/LeafCrop.width;
  for(float y=0;y<area.height;y+=tile*.90f){float h=Mathf.Min(tile,area.height-y);var crop=LeafCrop;crop.height*=h/tile;Paint(name+"_Tile_"+(int)y,parent,new Rect(area.x,area.y+y,area.width,h),crop,Art+"trellis-atlas.png",-.45f,false,local);}
 }
 static BoxCollider2D Solid(string name,Transform parent,Rect r,int layer){var g=Group(name,parent);g.gameObject.layer=layer;g.position=P(r.center.x,r.center.y);var c=g.gameObject.AddComponent<BoxCollider2D>();c.size=r.size/50;c.sharedMaterial=friction;return c;}
 static GameObject Paint(string name,Transform parent,Rect rect,Rect crop,string texture,float z,bool interactive,bool local=false){
  var g=Group(name,parent).gameObject;if(local)g.transform.localPosition=new Vector3(rect.x/50,-rect.yMax/50,z);else g.transform.position=P(rect.x,rect.yMax,z);
  var t=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);var mesh=new Mesh{name=name};float w=rect.width/50,h=rect.height/50;mesh.vertices=new[]{Vector3.zero,new Vector3(w,0),new Vector3(0,h),new Vector3(w,h)};mesh.uv=new[]{new Vector2(crop.x/t.width,1-crop.yMax/t.height),new Vector2(crop.xMax/t.width,1-crop.yMax/t.height),new Vector2(crop.x/t.width,1-crop.y/t.height),new Vector2(crop.xMax/t.width,1-crop.y/t.height)};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateBounds();mesh.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=Save(mesh,Geo+name+".asset");var mat=Material(name,interactive?"LightFeast/InteractablePaint":"LightFeast/PaintedLayer",Color.white);mat.mainTexture=t;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
 }
 static void Shape(GameObject go,Vector2[] outline,float width,float height,Rect crop,string texture){var mesh=go.GetComponent<MeshFilter>().sharedMesh;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);var v=new Vector3[outline.Length+1];var uv=new Vector2[v.Length];var tri=new int[outline.Length*3];Vector2 centre=Vector2.zero;foreach(var p in outline)centre+=p;centre/=outline.Length;for(int i=0;i<v.Length;i++){var p=i==0?centre:outline[i-1];v[i]=new Vector3(p.x*width/50,(1-p.y)*height/50);uv[i]=new Vector2((crop.x+p.x*crop.width)/tex.width,1-(crop.y+p.y*crop.height)/tex.height);}for(int i=0;i<outline.Length;i++){tri[i*3]=0;tri[i*3+1]=i+1;tri[i*3+2]=(i+1)%outline.Length+1;}mesh.Clear();mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}
 static T Save<T>(T obj,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);return old;}AssetDatabase.CreateAsset(obj,path);return obj;}
 static Material Material(string name,string shader,Color color){var m=new Material(Shader.Find(shader)){name=name};m.SetColor("_Color",color);return Save(m,Mats+name+".mat");}
 static LineRenderer Line(string name,Transform parent,Vector3[] points,float width,Color color,string shader){var l=Group(name,parent).gameObject.AddComponent<LineRenderer>();l.useWorldSpace=true;l.positionCount=points.Length;l.SetPositions(points);l.widthMultiplier=width;l.numCapVertices=5;l.numCornerVertices=4;l.sharedMaterial=Material(name,shader,color);return l;}
 static LineRenderer[] BeamPair(string name,Transform parent)=>new[]{Line(name+"_Halo",parent,new Vector3[2],.20f,new Color(1,.65f,.16f,.28f),"LightFeast/SoftLight"),Line(name+"_Core",parent,new Vector3[2],.065f,new Color(1,.95f,.65f,1),"LightFeast/SoftLight")};
 static void Cord(string name,Transform parent,Vector3[] points,float width){var l=Line(name,parent,points,Mathf.Max(width,.085f),new Color(.68f,.61f,.49f),"LightFeast/HempRope");l.textureMode=LineTextureMode.Tile;l.sharedMaterial.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Level03/rope-hemp.png");l.sharedMaterial.SetFloat("_TileLength",1.1f);var rim=l.gameObject.AddComponent<InteractionRopeGlow>();rim.glowMaterial=Material("TrellisRope_WhiteRim","LightFeast/SoftLight",new Color(.92f,.97f,1,.1f));}
 public static void BuildAll(){BuildScene();BuildPlayer();}
 public static void BuildPlayer(){var dir=Work+"/Builds/Windows-Level07-NOVA";Directory.CreateDirectory(dir);Directory.CreateDirectory(Work+"/Validation/level07");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.StandaloneWindows64,locationPathName=dir+"/LightFeast.exe"});File.WriteAllText(Work+"/Validation/level07/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Seventh build failed");}
 public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/SeventhTrellis.unity");EditorApplication.ExecuteMenuItem("Window/General/Game");}
}
