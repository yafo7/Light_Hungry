using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LightHungry {
 public sealed class WatercolorSmokeTest:MonoBehaviour {
  readonly List<string> results=new List<string>();PlayerController p;LightExposure exposure;LightTravel travel;string folder;
  void Start(){if(!Environment.GetCommandLineArgs().Contains("-watercolorQA"))return;folder=Path.GetFullPath("Verification");Directory.CreateDirectory(folder);p=GetComponent<LabSession>().player;exposure=p.GetComponent<LightExposure>();travel=p.GetComponent<LightTravel>();p.automationInput=true;GetComponent<LightRouteReview>().automatic=false;StartCoroutine(Run());}
  void Check(bool value,string label){string line=(value?"PASS ":"FAIL ")+label;results.Add(line);Debug.Log(line);}
  void Capture(string name){var c=Camera.main;var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());c.targetTexture=null;RenderTexture.active=old;Destroy(image);rt.Release();Destroy(rt);}
  IEnumerator Run(){
   yield return new WaitForSeconds(.6f);Check(p.Grounded,"Player settles on workshop floor");Check(exposure.secondsRequired==2&&exposure.travelSpeed==p.speed*2,"Two second exposure and twice walking speed");Check(p.GetComponent<PlayerPresentation>().walk.Length==8&&travel.photonFrames.Length==4,"Animation frame references");Capture("watercolor-front");
   var sources=FindObjectsOfType<LightEmitter>();var sourceStates=sources.Select(s=>s.active).ToArray();foreach(var s in sources)s.active=false;
   var chargeObject=new GameObject("QA exposure source");var charge=chargeObject.AddComponent<LightEmitter>();charge.useSpatialDirection=true;charge.spatialDirection=Vector3.forward;charge.transform.position=new Vector3(-13,-4.2f,-2);p.SetSpatialPosition(new Vector3(-13,-4.95f,0));
   yield return new WaitForSeconds(1);Check(exposure.Progress>.7f&&exposure.Progress<1.4f&&!travel.IsTravelling,"Actual ray charges without launching before two seconds");
   yield return new WaitForSeconds(1.15f);Check(travel.IsTravelling,"Actual continuous illumination launches after two seconds");travel.Cancel();Destroy(chargeObject);exposure.automatic=false;for(int i=0;i<sources.Length;i++)if(sources[i])sources[i].active=sourceStates[i];
   var mirror=GameObject.Find("Push and rotate mirror").GetComponent<MirrorSurface>();float angle=mirror.rotationAngle;mirror.Hold(p,8);Check(Mathf.Abs(Mathf.DeltaAngle(angle,mirror.rotationAngle))<.01f,"Planar mirror full 360 rotation");
   var depth=GameObject.Find("Cross face mirror").GetComponent<MirrorSurface>();Check(Vector3.Dot(Vector3.Reflect(Vector3.right,depth.WorldNormal),Vector3.forward)>.99f,"45 degree depth mirror sends light forward in world depth");
   p.SetSpatialPosition(new Vector3(6,-1.95f,0));travel.LaunchFromLight(new Vector3(6,-1,0),Vector3.right,11);yield return new WaitForSeconds(.45f);Check(travel.IsTravelling&&travel.Reflections>0&&p.transform.position.z>1,"Photon crosses depth mirror into adjacent face");var orb=GameObject.Find("Travelling light");Check(orb&&Vector3.Distance(orb.transform.position,p.transform.position+Vector3.up*.95f)<.05f,"Photon artwork stays attached to travel position during camera rotation");Capture("watercolor-crossing");travel.Cancel();CameraViewController.Active.EndTravelView(1);
   var rope=FindObjectOfType<PulleyRope>();p.SetSpatialPosition(new Vector3(-1,-4.95f,0));float start=rope.Progress;for(int i=0;i<30;i++){rope.Hold(p,.02f);yield return new WaitForSeconds(.02f);}Check(rope.Progress>start+.4f,"Held rope hoists load");float lifted=rope.Progress;rope.Release(p);yield return new WaitForSeconds(.4f);Check(rope.Progress<lifted-.2f,"Released rope drops load");
   var timed=GameObject.Find("Timed flashlight").GetComponent<DeviceSwitch>();timed.activeSeconds=.2f;timed.Interact(p);Check(timed.lightTarget.active,"E switch activates flashlight");yield return new WaitForSeconds(.3f);Check(!timed.lightTarget.active,"Timed flashlight switches off");
   var box=FindObjectsOfType<PushableBox>().First(b=>b.name=="PushableBox");p.SetSpatialPosition(new Vector3(box.transform.position.x-1,-4.95f,0));float bx=box.transform.position.x;p.automationHorizontal=1;yield return new WaitForSeconds(.55f);p.automationHorizontal=0;Check(box.transform.position.x>bx+.15f,"AD directly pushes box without E");
   var lift=FindObjectOfType<AutomaticLift>();p.SetSpatialPosition(new Vector3(2,-4.94f,0));yield return new WaitForSeconds(.8f);Check(lift.Occupied&&lift.platform.localPosition.y>.3f,"Occupied elevator raises platform and player");
   exposure.automatic=false;p.SetSpatialPosition(new Vector3(-12,-4.95f,0));CameraViewController.Active.EndTravelView(1);
   // Dedicated isolated ray validates collision rules independently of the puzzle arrangement.
   var wall=new GameObject("QA stop wall");wall.layer=8;wall.transform.position=new Vector3(-8,1,0);wall.AddComponent<BoxCollider>().size=new Vector3(.4f,3,.8f);
   var glass=new GameObject("QA transmitting glass");glass.layer=8;glass.transform.position=new Vector3(-10,1,0);glass.AddComponent<BoxCollider>().size=new Vector3(.2f,3,.8f);glass.AddComponent<OpticalPassThrough>();Physics.SyncTransforms();
   travel.LaunchFromLight(new Vector3(-12,1,0),Vector3.right,11);yield return new WaitForSeconds(.6f);Check(!travel.IsTravelling&&travel.LastResult=="Stopped at obstacle"&&p.transform.position.x> -9.2f,"Photon transmits glass and materializes beside opaque wall");Destroy(wall);Destroy(glass);
   p.SetSpatialPosition(new Vector3(-12,1,0));travel.LaunchFromLight(new Vector3(-12,2,0),Vector3.right,11);yield return null;Check(travel.TryJumpOut()&&!travel.IsTravelling,"Space exit restores body mid-route");
   foreach(int view in new[]{1,2,3,4}){CameraViewController.Active.EndTravelView(view);yield return new WaitForEndOfFrame();var renderers=FindObjectsOfType<Renderer>();var states=renderers.Select(r=>r.enabled).ToArray();Capture("watercolor-face-"+view);Check(renderers.Select((r,i)=>r.enabled==states[i]&&!r.forceRenderingOff).All(x=>x),"Face "+view+" render scope restores all renderer states");}
   Check(!FindObjectsOfType<Canvas>().Any()&&!FindObjectsOfType<TextMesh>().Any(),"No in-game instructional UI");
   File.WriteAllLines(Path.Combine(folder,"watercolor-qa.txt"),results);Application.Quit(results.Any(x=>x.StartsWith("FAIL"))?1:0);
  }
 }
}
