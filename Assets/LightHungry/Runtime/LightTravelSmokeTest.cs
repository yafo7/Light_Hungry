using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace LightHungry {
    public sealed class LightTravelSmokeTest:MonoBehaviour {
        List<string> results=new List<string>();PlayerController p;LightTravel travel;string folder;
        void Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-travelQA")<0)return;p=GetComponent<LabSession>().player;p.automationInput=true;travel=p.GetComponent<LightTravel>();folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification"));StartCoroutine(Run());}
        void Check(bool value,string message){string line=(value?"PASS ":"FAIL ")+message;results.Add(line);Debug.Log(line);}
        void Capture(string name){var camera=Camera.main;var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Path.Combine(folder,name),t.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Destroy(t);rt.Release();Destroy(rt);}
        IEnumerator Run(){
            yield return new WaitForSeconds(.7f);var pad=GameObject.Find("Workshop launch").GetComponent<LightLaunchPad>();var back=GameObject.Find("Workshop return launch").GetComponent<LightLaunchPad>();
            var mirror=GameObject.Find("Workshop depth mirror").GetComponent<SpaceMirror>();
            Check(travel.Launch(pad),"Launch from authored start pad");
            yield return new WaitForSeconds(.9f);Check(travel.IsTravelling&&travel.Reflections==1,"Travel crosses spatial mirror instead of teleporting");Capture("travel-crossing.png");
            var before=p.transform.position;LabSession.Instance.SetPaused(true);yield return new WaitForSecondsRealtime(.2f);Check(Vector3.Distance(before,p.transform.position)<.001f,"Pause freezes travelling actor");LabSession.Instance.SetPaused(false);
            yield return new WaitForSeconds(2);Check(travel.LastResult=="Arrived"&&!travel.IsTravelling,"Materialize at side landing");Check(CameraViewController.Active.CurrentView==4&&Mathf.Abs(p.transform.position.z-9)<.1f,"Landing selects side view and depth");Check(p.visual.enabled&&!p.Body3D.isKinematic,"Entity animation and physics restored");Capture("travel-arrival.png");
            p.SetSpatialPosition(back.transform.position+Vector3.up*.03f);yield return new WaitForSeconds(.2f);Check(travel.Launch(back),"Return launch accepted");yield return new WaitForSeconds(2.6f);Check(travel.LastResult=="Arrived"&&CameraViewController.Active.CurrentView==1&&Mathf.Abs(p.transform.position.z)<.05f,"Mirror returns player to original face");
            yield return new WaitForSeconds(.2f);mirror.orientation=1;mirror.Apply();p.SetSpatialPosition(pad.transform.position+Vector3.up*.03f);yield return new WaitForSeconds(.2f);Check(travel.Launch(pad),"Wrong-route launch accepted");yield return new WaitForSeconds(.7f);travel.Cancel();Check(!travel.IsTravelling&&p.visual.enabled&&Mathf.Abs(p.transform.position.z)<.05f,"Failure or cancel returns entity to start");mirror.orientation=0;mirror.Apply();
            yield return new WaitForSeconds(.2f);var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.layer=8;block.transform.position=pad.Origin+Vector3.right*1.5f;Physics.SyncTransforms();travel.Launch(pad);yield return new WaitForSeconds(.5f);Check(travel.LastResult=="Blocked"&&!travel.IsTravelling&&p.visual.enabled,"Solid obstacle cancels travel and restores entity");Destroy(block);yield return new WaitForSeconds(.2f);
            float range=travel.maxDistance;travel.maxDistance=1;travel.Launch(pad);yield return new WaitForSeconds(.4f);Check(travel.LastResult=="Distance limit"&&!travel.IsTravelling,"Unbounded ray has distance recovery");travel.maxDistance=range;
            var box=FindObjectOfType<CarryableProp>();p.SetSpatialPosition(box.transform.position+Vector3.left*1.5f+Vector3.up*.03f);yield return new WaitForSeconds(.3f);var carry=p.GetComponent<PlayerCarry>();Check(carry.PickUp(box),"Pick up portable prop");yield return null;Check(box.Held&&carry.Held==box,"Carried prop tracks player");Check(carry.Drop()&&!box.Held,"Drop restores prop collision");
            var sw=FindObjectOfType<DeviceSwitch>();sw.Interact(p);Check(sw.target.state&&!sw.target.gate.enabled,"Switch opens connected gate");sw.Interact(p);Check(!sw.target.state&&sw.target.gate.enabled,"Switch closes connected gate");
            File.WriteAllLines(Path.Combine(folder,"travel-results.txt"),results);Application.Quit(results.Exists(s=>s.StartsWith("FAIL"))?1:0);
        }
    }
}
