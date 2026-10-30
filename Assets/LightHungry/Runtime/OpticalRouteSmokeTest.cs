using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LightHungry {
 public sealed class OpticalRouteSmokeTest:MonoBehaviour {
  readonly List<string> checks=new List<string>();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-routeQA"))new GameObject("Optical route QA").AddComponent<OpticalRouteSmokeTest>().StartCoroutine("Run");}
  void Check(bool ok,string text){var line=(ok?"PASS ":"FAIL ")+text;checks.Add(line);Debug.Log(line);}
  GameObject Item(string name,Vector3 position,int face,int layer){var g=new GameObject(name);g.layer=layer;g.transform.position=position;g.AddComponent<FaceMembership>().faces=FaceMembership.ForView(face);return g;}
  MirrorSurface Mirror(string name,Vector3 position,int face,bool depth,float angle,Vector3 size){var g=Item(name,position,face,11);g.transform.rotation=Quaternion.Euler(0,CameraViewController.Yaw(face),0);var m=g.AddComponent<MirrorSurface>();m.continuousRotation=true;m.depthRotation=depth;m.rotationAngle=angle;g.AddComponent<BoxCollider>().size=size;m.Apply();return m;}
  BeastReceiver Beast(string name,Vector3 position,int face){var g=Item(name,position,face,12);var b=g.AddComponent<BeastReceiver>();b.isGoal=false;g.AddComponent<SphereCollider>().radius=.45f;return b;}
  IEnumerator Run(){
   yield return new WaitForSeconds(.3f);var session=LabSession.Instance;session.GetComponent<LightRouteReview>().automatic=false;var p=session.player;p.automationInput=true;p.GetComponent<LightExposure>().automatic=false;
   foreach(var light in FindObjectsOfType<LightEmitter>())light.active=false;
   var source=Item("Route source",new Vector3(200,40,0),1,13).AddComponent<LightEmitter>();source.beamWidth=1.2f;source.maxLength=30;
   var ghost=Mirror("Opposite overlapping plane mirror",new Vector3(204,40,0),2,false,45,new Vector3(.12f,2,.5f));
   var front=Beast("Front receiver",new Vector3(210,40,0),1);var shadow=Beast("Opposite wide-beam receiver",new Vector3(202,40,.3f),2);
   var wall=Item("Opposite overlapping wall",new Vector3(206,40,0),2,8);wall.AddComponent<BoxCollider>().size=new Vector3(.2f,2,1);
   yield return new WaitForSeconds(.1f);Check(front.Progress>0,"Opposite plane mirror and wall do not interrupt front light");Check(shadow.Progress==0,"Wide column cannot feed opposite-face beast");Check(!session.beams.LastPaths.Any(s=>s.Contains(ghost.name)||s.Contains(wall.name)),"Hidden opposite colliders excluded from ray hits");
   var route=OpticalRoute.From(source);Physics.SyncTransforms();Check(SpaceOptics.CastPreview(source.transform.position,Vector3.right,30,null,route,out var preview)&&preview.collider.GetComponent<BeastReceiver>()==front,"Editor preview uses same face isolation");
   var blocking=Item("Front blocking wall",new Vector3(208,40,0),1,8);blocking.AddComponent<BoxCollider>().size=new Vector3(.2f,2,1);front.Clear();yield return new WaitForSeconds(.1f);Check(front.Progress==0,"Same-face wall still blocks light");Destroy(blocking);yield return null;
   var travel=p.GetComponent<LightTravel>();CameraViewController.Active.EndTravelView(1);travel.LaunchFromLight(new Vector3(200,40,0),Vector3.right,20);yield return new WaitForSeconds(.35f);Check(travel.IsTravelling&&travel.Reflections==0&&travel.Route.view==1&&p.transform.position.x>206,"Photon ignores overlapping opposite-face mirror and wall");travel.Cancel();
   var departure=Mirror("Departure depth mirror",new Vector3(204,40,0),1,true,45,new Vector3(.12f,2,.12f));departure.destinationView=4;
   var intermediate=Mirror("Opposite plane mirror in transit",new Vector3(203.94f,40,3),2,false,45,new Vector3(1,2,.2f));
   var transitBeast=Beast("Opposite beast before receiving mirror",new Vector3(203.94f,40,4.5f),2);
   var receiving=Mirror("Receiving depth mirror",new Vector3(203.94f,40,6),2,true,45,new Vector3(.8f,2,.8f));receiving.destinationView=4;
   var planar=Mirror("Opposite final plane mirror",new Vector3(208,40,5.6f),2,false,-45,new Vector3(.12f,2,.6f));var target=Beast("Opposite final receiver",new Vector3(207.94f,44,5.6f),2);
   yield return new WaitForSeconds(.15f);Check(target.Progress>0,"Two depth mirrors restore opposite plane and allow final plane reflection");Check(transitBeast.Progress==0,"In-transit light cannot feed beast before receiving depth mirror");Check(!session.beams.LastPaths.Any(s=>s.Contains(intermediate.name)),"Plane mirror cannot intercept a cross-face segment");
   route=OpticalRoute.From(source);Check(route.Reflect(departure,Vector3.right,out var depthDirection,out _)&&route.inTransit&&route.view==1,"Departure angle starts transit without forcing numeric destination");Check(!route.CanHit(intermediate,depthDirection),"Transit rejects ordinary plane mirror");Check(route.Reflect(receiving,depthDirection,out var flatDirection,out int entered)&&!route.inTransit&&entered==2&&Mathf.Abs(flatDirection.z)<.01f,"Receiving angle and face, not hint, determine actual entry");
   Check(route.Reflect(planar,flatDirection,out var upward,out _)&&Vector3.Dot(upward,Vector3.up)>.99f&&route.view==2,"Plane mirror remains within destination plane");
   float progress=target.Progress;CameraViewController.Active.EndTravelView(3);yield return new WaitForSeconds(.1f);Check(target.Progress>progress,"Camera turn does not change source optical face or offscreen feeding");CameraViewController.Active.EndTravelView(1);
   receiving.gameObject.SetActive(false);target.Clear();transitBeast.Clear();yield return new WaitForSeconds(.12f);Check(target.Progress==0&&transitBeast.Progress==0,"Removing receiving depth mirror prevents destination interactions");
   travel.LaunchFromLight(new Vector3(200,40,0),Vector3.right,20);yield return new WaitForSeconds(.6f);Check(travel.IsTravelling&&travel.Reflections==1&&travel.Route.inTransit&&travel.Route.view==1,"Photon remains in transit without receiving depth mirror");travel.Cancel();
   receiving.gameObject.SetActive(true);receiving.rotationAngle=0;receiving.Apply();target.Clear();yield return new WaitForSeconds(.1f);Check(target.Progress==0,"Wrong receiving angle does not activate destination plane");
   receiving.rotationAngle=45;receiving.Apply();CameraViewController.Active.EndTravelView(1);travel.LaunchFromLight(new Vector3(200,40,0),Vector3.right,20);yield return new WaitForSeconds(.48f);Check(travel.IsTravelling&&travel.Reflections>=2&&!travel.Route.inTransit&&travel.Route.view==2,"Photon follows same two-mirror transition into opposite face");travel.Cancel();
   var reverse=new OpticalRoute{view=2};Check(reverse.Reflect(receiving,Vector3.left,out var back,out _)&&reverse.inTransit&&reverse.Reflect(departure,back,out var original,out int returned)&&returned==1&&Vector3.Dot(original,Vector3.left)>.99f,"Depth mirror pair also supports reverse travel");
   departure.rotationAngle=0;departure.Apply();var planarOnly=new OpticalRoute{view=1};planarOnly.Reflect(departure,Vector3.right,out _,out _);Check(!planarOnly.inTransit&&planarOnly.view==1,"Depth mirror rotated to planar output does not switch faces");
   for(int face=1;face<=4;face++){bool isolated=true;var checkRoute=new OpticalRoute{view=face};var direction=Quaternion.Euler(0,CameraViewController.Yaw(face),0)*Vector3.right;for(int owner=1;owner<=4;owner++){ghost.GetComponent<FaceMembership>().faces=FaceMembership.ForView(owner);isolated&=checkRoute.CanHit(ghost,direction)==(face==owner);}Check(isolated,"Face "+face+" admits only its own ordinary mirror");}
   var interactionWall=Item("Opposite interaction wall",new Vector3(203.4f,40,0),2,8);interactionWall.AddComponent<BoxCollider>().size=new Vector3(.1f,2,.8f);CameraViewController.Active.EndTravelView(1);p.SetSpatialPosition(new Vector3(203,39.3f,0));yield return new WaitForSeconds(.08f);Check(p.Nearest&&(FaceMembership.OpticalOf(p.Nearest)&RoomFaces.Front)!=0,"E interaction ignores opposite-face targets and occluders");
   Directory.CreateDirectory("Verification");File.WriteAllLines(Path.GetFullPath("Verification/optical-route-qa.txt"),checks);Application.Quit(checks.Any(s=>s.StartsWith("FAIL"))?1:0);
  }
 }
}
