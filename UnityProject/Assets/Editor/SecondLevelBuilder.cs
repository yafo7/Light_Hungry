using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SecondLevelBuilder {
 const string Root="Assets/Art/Level02/",Geo="Assets/Art/Level02Geometry/",Mats="Assets/Materials/Level02/";
 const float P=63,Y0=571,W=1774,H=887;static Transform back,terrain,items,lights;static string work=>Environment.GetEnvironmentVariable("LIGHTFEAST_WORKSPACE")??Path.GetFullPath(Application.dataPath+"/../..");
 public static Vector3 Pixel(float x,float y,float z=0){return new Vector3(x/P,(Y0-y)/P,z);}
 [MenuItem("食光兽/搭建第二关与第三关入口")]
 public static void BuildScenes(){
  if(EditorApplication.isPlaying)throw new Exception("Exit play mode before building scenes.");
  EditorSceneManager.SaveOpenScenes();Directory.CreateDirectory(Root);Directory.CreateDirectory(Geo);Directory.CreateDirectory(Mats);Directory.CreateDirectory(work+"/Validation/level02");
  string backup=work+"/Validation/level02/FirstLight-before-level02.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/FirstLight.unity",backup);
  foreach(var file in new[]{"00-background.png","10-ground.png","20-leaf-dew.png","30-rubble.png","40-light.png"})File.Copy(work+"/art/level02-dew-layers-v08-2026-10-30/Layers/"+file,Root+file,true);
  File.Copy(work+"/art/level02-loose-rubble-v10-2026-10-30/30-rubble.png",Root+"30-rubble.png",true);
  File.Copy(work+"/art/level02-loose-rubble-v10-2026-10-30/layer-manifest.json",Root+"source-layer-manifest.json",true);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var file in new[]{"00-background.png","10-ground.png","20-leaf-dew.png","30-rubble.png","40-light.png"}){var t=(TextureImporter)AssetImporter.GetAtPath(Root+file);t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.SaveAndReimport();}
  MakeSecond();MakeThird();WireFirst();
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/FirstLight.unity",true),new EditorBuildSettingsScene("Assets/Scenes/SecondLight.unity",true),new EditorBuildSettingsScene("Assets/Scenes/ThirdDrain.unity",true)};
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/SecondLight.unity");var sv=SceneView.lastActiveSceneView;if(sv){sv.in2DMode=true;sv.rotation=Quaternion.identity;sv.pivot=new Vector3(14,2,0);sv.size=9;sv.Repaint();}EditorApplication.ExecuteMenuItem("Window/General/Game");Debug.Log("SECOND_LIGHT_SCENES_READY");
 }
 static void NewScene(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);RenderSettings.ambientLight=new Color(.55f,.56f,.45f);RenderSettings.fog=false;back=new GameObject("01_Background_NoCollision").transform;terrain=new GameObject("02_WalkableTerrain").transform;items=new GameObject("03_LeafAndRubble").transform;lights=new GameObject("04_RefractionAndShadow").transform;}
 static void MakeSecond(){NewScene();
  Card("PotInterior_Background",back,"00-background.png",1,1,0,0,8);
  Card("MossGround_AndLanding",terrain,"10-ground.png",1,1.1f,0,-23,3);
  Card("Leaf_WithResidualDew",items,"20-leaf-dew.png",.41f,.41f,341,265,1.5f);
  Card("Rubble_WithOpenSlit",items,"30-rubble.png",.61f,.61f,546,252.1f,.5f);
  Polygon("Ground_Collision",terrain,new[]{new Vector2(0,540),new Vector2(260,544),new Vector2(550,555),new Vector2(800,566),new Vector2(1038,573),new Vector2(1090,602),new Vector2(1130,647),new Vector2(1200,696),new Vector2(1280,727),new Vector2(1480,727),new Vector2(1520,755),new Vector2(1580,802),new Vector2(1640,849),new Vector2(1774,900),new Vector2(1774,1000),new Vector2(0,1000)});
  var upper=new[]{new Vector2(900,510),new Vector2(980,450),new Vector2(1040,415),new Vector2(1140,390),new Vector2(1250,410),new Vector2(1400,430),new Vector2(1530,460),new Vector2(1620,550),new Vector2(1700,628),new Vector2(1660,659),new Vector2(1460,678),new Vector2(1290,650),new Vector2(1240,697.56f),new Vector2(950,547.05f)};
  var lower=new[]{new Vector2(950,559.05f),new Vector2(1250,714.75f),new Vector2(1340,779),new Vector2(1240,806),new Vector2(1080,815),new Vector2(890,780),new Vector2(800,720),new Vector2(850,660),new Vector2(900,616)};
  Polygon("Rubble_UpperCollider",items,upper.Select(v=>v*.61f+new Vector2(546,252.1f)).ToArray());Polygon("Rubble_LowerCollider",items,lower.Select(v=>v*.61f+new Vector2(546,252.1f)).ToArray());
  // The buried mouth joins the pot rim and brick arch; a low rubble heap closes the gap below.
  Polygon("PotShell_Right_AndBuriedDrainRoof",terrain,new[]{new Vector2(910,0),new Vector2(1774,0),new Vector2(1774,850),new Vector2(1650,747),new Vector2(1500,655),new Vector2(1500,455),new Vector2(1410,342),new Vector2(1290,223),new Vector2(1150,145),new Vector2(910,108)});
  Polygon("PotShell_Left",terrain,new[]{new Vector2(0,0),new Vector2(841,0),new Vector2(841,108),new Vector2(600,160),new Vector2(420,245),new Vector2(250,430),new Vector2(140,550),new Vector2(0,550)});
  Box("LeftLimit",terrain,new Vector2(-.2f,3),new Vector2(.4f,25));
  var landing=new GameObject("SafeReform_Landing").transform;landing.SetParent(lights);landing.position=Pixel(1395,724.8f);
  var path=lights.gameObject.AddComponent<LightPath>();path.nodes=new[]{Pixel(877,61),Pixel(789,401),Pixel(790,415.336f),Pixel(1370,716.356f)};path.safeEnd=landing;path.beams=new[]{Beam("Ray_WarmHalo",.15f,new Color(1,.63f,.18f,.3f)),Beam("Ray_BrightCore",.026f,new Color(1,.92f,.58f,.95f))};
  // Dust is visual only; the narrow ray above is the effective path.
  var dust=new GameObject("DustInSunlight");dust.transform.SetParent(lights);dust.transform.position=Pixel(850,260,-1.8f);var ps=dust.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.startLifetime=4;main.startSpeed=.025f;main.startSize=.025f;main.startColor=new Color(1,.78f,.36f,.2f);main.maxParticles=55;var em=ps.emission;em.rateOverTime=8;var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Box;sh.scale=new Vector3(1.3f,5,.05f);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Mat("Dust","LightFeast/SoftLight",new Color(1,.8f,.4f,.2f));ps.Play();
  var player=Player(Pixel(320,540,-1));var ability=player.GetComponent<LightFormAbility>();
  var camera=CameraAt("Main Camera",new Vector3(W/P/2,2,-40),8);var rig=camera.gameObject.AddComponent<PotCamera>();rig.player=player.transform;rig.view=camera;
  Shadow("DarkPassage_CoversDownstairs",Pixel(1670,780,-3),new Vector2(560/P,440/P));
  var gate=new GameObject("Shadow_ContactToThirdDrain");gate.transform.SetParent(lights);gate.transform.position=Pixel(1630,780);var c=gate.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=new Vector2(240/P,180/P);var passage=gate.AddComponent<ScenePassage>();passage.destination="ThirdDrain";
  var flow=new GameObject("05_Level02Flow").AddComponent<PotLevelFlow>();flow.player=player;flow.ability=ability;flow.cameraRig=rig;flow.exit=passage;
  Save("SecondLight");
 }
 static void MakeThird(){NewScene();
  if(File.Exists("Assets/Scenes/ThirdDrain.unity")&&File.ReadAllText("Assets/Scenes/ThirdDrain.unity").Contains("ThirdDrain_FullLevel"))return;
  // Only the arrival room is authored here, not the later two-mirror puzzle.
  Quad("Drain_BackWall",back,new Vector3(-1,-3,8),26,15,"00-background.png",new Rect(.71f,.015f,.285f,.33f),new Color(.42f,.55f,.52f,1));
  for(int i=0;i<3;i++)Quad("Drain_Ledge_"+i,terrain,new Vector3(i*8,-3,2),8,3,"10-ground.png",new Rect(.715f,.01f,.115f,.22f),new Color(.45f,.57f,.51f,1));
  Box("ArrivalFloor",terrain,new Vector2(12,-1),new Vector2(26,2));Box("LeftWall",terrain,new Vector2(-.4f,3),new Vector2(.8f,8));Box("ArrivalRoomEnd",terrain,new Vector2(24,3),new Vector2(1,8));
  var player=Player(new Vector3(3.2f,.05f,-1));CameraAt("Main Camera",new Vector3(11.5f,3,-40),7);Shadow("ArrivalShadow",new Vector3(.3f,1.9f,-3),new Vector2(4,6));
  var room=new GameObject("ThirdDrain_ArrivalOnly").AddComponent<DrainArrivalFlow>();room.player=player;room.ability=player.GetComponent<LightFormAbility>();Save("ThirdDrain");
 }
 static void WireFirst(){EditorSceneManager.OpenScene("Assets/Scenes/FirstLight.unity");var flow=UnityEngine.Object.FindObjectOfType<FirstLightFlow>();if(!flow)throw new Exception("Missing FirstLightFlow");var ability=flow.player.GetComponent<LightFormAbility>();if(!ability)ability=flow.player.gameObject.AddComponent<LightFormAbility>();flow.ability=ability;var path=flow.puzzle.GetComponent<LightPath>();if(!path)path=flow.puzzle.gameObject.AddComponent<LightPath>();path.mirrorSource=flow.puzzle;path.beams=new LineRenderer[0];path.safeEnd=null;Save("FirstLight");}
 static PlayerMotor Player(Vector3 position){var g=new GameObject("WickSpirit_Player");g.transform.position=position;var rb=g.AddComponent<Rigidbody2D>();rb.gravityScale=3;rb.freezeRotation=true;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var c=g.AddComponent<CapsuleCollider2D>();c.size=new Vector2(.42f,.94f);c.offset=new Vector2(0,.49f);c.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Spirit.physicsMaterial2D");var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab");if(!prefab)throw new Exception("Missing supplied character prefab");var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab);visual.transform.SetParent(g.transform,false);visual.transform.localRotation=Quaternion.Euler(0,22,0);var m=g.AddComponent<PlayerMotor>();m.visual=visual.transform;m.groundMask=1<<6;m.speed=3.2f;
  // Serialized particles are created with a saved material instead of transient runtime materials.
  var ps=SpiritEffects.Embers(g.transform);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Mat("SpiritEmbers","LightFeast/SoftLight",new Color(1,.8f,.4f,.7f));m.hairEmitter=ps.transform;g.AddComponent<LightFormAbility>();return m;
 }
 static Camera CameraAt(string name,Vector3 p,float size){var g=new GameObject(name);g.tag="MainCamera";g.transform.position=p;var c=g.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=size;c.backgroundColor=new Color(.065f,.055f,.035f);c.clearFlags=CameraClearFlags.SolidColor;c.nearClipPlane=.1f;c.farClipPlane=100;g.AddComponent<AudioListener>();return c;}
 static void Card(string name,Transform parent,string file,float sx,float sy,float tx,float ty,float z){Quad(name,parent,Pixel(tx,ty+H*sy,z),W*sx/P,H*sy/P,file,new Rect(0,0,1,1),Color.white);}
 static GameObject Quad(string name,Transform parent,Vector3 pos,float width,float height,string file,Rect uv,Color tint){var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=pos;var mesh=new Mesh{name=name};mesh.vertices=new[]{Vector3.zero,new Vector3(width,0),new Vector3(0,height),new Vector3(width,height)};mesh.uv=new[]{new Vector2(uv.x,uv.y),new Vector2(uv.xMax,uv.y),new Vector2(uv.x,uv.yMax),new Vector2(uv.xMax,uv.yMax)};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateBounds();mesh=Asset(mesh,Geo+name+".asset");g.AddComponent<MeshFilter>().sharedMesh=mesh;var mat=Mat(name,"LightFeast/PaintedLayer",tint);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+file);g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;}
 static void Polygon(string name,Transform parent,Vector2[] points){var g=new GameObject(name);g.transform.SetParent(parent,false);g.layer=6;g.AddComponent<PolygonCollider2D>().points=points.Select(p=>(Vector2)Pixel(p.x,p.y)).ToArray();}
 static void Capstone(string name,Vector2[] points,Color tint,float z){var g=new GameObject(name);g.transform.SetParent(items,false);g.layer=6;var mesh=new Mesh{name=name};var center=points.Aggregate(Vector2.zero,(a,b)=>a+b)/points.Length;float minX=points.Min(p=>p.x),maxX=points.Max(p=>p.x),minY=points.Min(p=>p.y),maxY=points.Max(p=>p.y);var vertices=new Vector3[points.Length+1];var uv=new Vector2[vertices.Length];var tri=new int[points.Length*3];for(int i=0;i<vertices.Length;i++){var p=i==points.Length?center:points[i];vertices[i]=Pixel(p.x,p.y,z);float u=(p.x-minX)/(maxX-minX),v=(p.y-minY)/(maxY-minY);uv[i]=new Vector2((1320+u*160)/1774,1-(440+v*110)/887);}for(int i=0;i<points.Length;i++){tri[i*3]=points.Length;tri[i*3+1]=i;tri[i*3+2]=(i+1)%points.Length;}mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Asset(mesh,Geo+name+".asset");g.AddComponent<MeshFilter>().sharedMesh=mesh;var mat=Mat(name,"LightFeast/PaintedLayer",tint);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"30-rubble.png");mat.SetFloat("_Cutoff",0);g.AddComponent<MeshRenderer>().sharedMaterial=mat;g.AddComponent<PolygonCollider2D>().points=points.Select(p=>(Vector2)Pixel(p.x,p.y)).ToArray();var edge=new GameObject(name+"_InkEdge");edge.transform.SetParent(items);var l=edge.AddComponent<LineRenderer>();l.loop=true;l.positionCount=points.Length;l.SetPositions(points.Select(p=>Pixel(p.x,p.y,z-.03f)).ToArray());l.widthMultiplier=.026f;l.sharedMaterial=Mat(name+"_Ink","Unlit/Color",new Color(.16f,.13f,.09f,1));}
 static void Box(string name,Transform parent,Vector2 center,Vector2 size){var g=new GameObject(name);g.transform.SetParent(parent,false);g.layer=6;g.transform.position=center;g.AddComponent<BoxCollider2D>().size=size;}
 static LineRenderer Beam(string name,float width,Color color){var g=new GameObject(name);g.transform.SetParent(lights);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=true;l.widthMultiplier=width;l.numCapVertices=5;l.numCornerVertices=3;l.sharedMaterial=Mat(name,"LightFeast/SoftLight",color);return l;}
 static void Shadow(string name,Vector3 center,Vector2 size){var g=new GameObject(name);g.transform.SetParent(lights);g.transform.position=center;g.transform.localScale=new Vector3(size.x,size.y,1);var mesh=Asset(SpiritEffects.Quad(),Geo+name+".asset");g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=Mat(name,"LightFeast/PassageShadow",new Color(.012f,.019f,.016f,1));}
 static Material Mat(string name,string shader,Color color){var m=new Material(Shader.Find(shader)){name=name};m.SetColor("_Color",color);Asset(m,Mats+name+".mat");return AssetDatabase.LoadAssetAtPath<Material>(Mats+name+".mat");}
 static T Asset<T>(T o,string path) where T:UnityEngine.Object {var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(o,old);UnityEngine.Object.DestroyImmediate(o);return old;}AssetDatabase.CreateAsset(o,path);return o;}
 static void Save(string name){EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/"+name+".unity");}
 [MenuItem("食光兽/构建第二关 Windows 试玩")]
 public static void BuildAll(){BuildScenes();var target=work+"/Builds/Windows-Level02";Directory.CreateDirectory(target);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),locationPathName=target+"/LightFeast.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText(work+"/Validation/level02/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Level02 build failed");Debug.Log("SECOND_LIGHT_BUILD_SUCCEEDED");}
}
