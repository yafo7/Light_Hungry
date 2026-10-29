using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace LightHungry {
    public sealed class ExposureJourneySmokeTest:MonoBehaviour {
        readonly List<string> results=new List<string>();LabSession session;PlayerController p;LightExposure exposure;LightTravel travel;string folder;
        public static void ConfigureLegacy(LabSession session){
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"-labQA")<0&&Array.IndexOf(args,"-viewQA")<0&&Array.IndexOf(args,"-travelQA")<0)return;
            session.player.GetComponent<LightExposure>().automatic=false;var review=session.GetComponent<LightRouteReview>();if(review)review.automatic=false;
            var old=GameObject.Find("Hungry beast");if(old)session.goal=old.GetComponent<BeastReceiver>();session.startWithSwallow=true;session.startWithReflection=true;
            foreach(var pad in FindObjectsOfType<LightLaunchPad>())pad.available=true;
        }
        void Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-exposureQA")<0)return;session=GetComponent<LabSession>();p=session.player;p.automationInput=true;exposure=p.GetComponent<LightExposure>();travel=p.GetComponent<LightTravel>();folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification"));StartCoroutine(Run());}
        void Check(bool condition,string text){var line=(condition?"PASS ":"FAIL ")+text;results.Add(line);Debug.Log(line);}
        IEnumerator UntilTravel(){float deadline=Time.realtimeSinceStartup+7;while(!travel.IsTravelling&&Time.realtimeSinceStartup<deadline)yield return null;}
        void Capture(string name){var c=Camera.main;var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());c.targetTexture=null;RenderTexture.active=old;Destroy(image);rt.Release();Destroy(rt);}
        IEnumerator Run(){
            yield return new WaitForSeconds(1.3f);yield return new WaitForEndOfFrame();Check(exposure.Progress>.8f&&exposure.Progress<3&&!travel.IsTravelling,"Physical illumination charges continuously without instant transformation");Check(p.visual.color.a<.95f,"Exposure progressively dissolves actor visual");Capture("exposure-charging.png");
            p.SetSpatialPosition(new Vector3(-14.4f,-5.8f,-1));yield return new WaitForSeconds(.2f);Check(exposure.Progress==0,"Leaving beam resets charge");
            p.SetSpatialPosition(new Vector3(-14.4f,-5.8f,0));yield return UntilTravel();Check(travel.IsTravelling,"Three seconds of illumination launches actual travelling light");
            var source=GameObject.Find("Exposure start light").GetComponent<LightEmitter>();source.active=false;
            while(travel.IsTravelling&&p.transform.position.x< -13.6f)yield return null;
            Check(!travel.TryJumpOut()&&travel.IsTravelling,"Narrow occupied space rejects exit without teleporting");
            float exitDeadline=Time.realtimeSinceStartup+5;while(travel.IsTravelling&&p.transform.position.z<7.2f&&Time.realtimeSinceStartup<exitDeadline)yield return null;
            Check(travel.TryJumpOut(),"Space exit can materialize at a clear current position");float y=p.transform.position.y;yield return new WaitForSeconds(.12f);Check(!travel.IsTravelling&&p.visual.enabled&&p.transform.position.y>y+.4f,"Jump-out restores entity and produces vertical jump");Check(exposure.Progress==0,"Jump-out resets charge and activates protection");
            yield return new WaitForSeconds(1.1f);p.SetSpatialPosition(new Vector3(-14.4f,-5.8f,0));source.active=true;yield return UntilTravel();Check(travel.IsTravelling,"Exposure can launch a later trip");
            source.active=false;yield return new WaitForSeconds(4);Check(travel.LastResult=="Arrived"&&CameraViewController.Active.CurrentView==4,"Independent photon continues after source extinguishes and reaches side landing");Capture("exposure-side-arrival.png");
            var review=session.GetComponent<LightRouteReview>();review.duration=2;
            Check(!session.goal.Satisfied&&!review.Played,"Original lab light does not prematurely complete journey goal");
            FindObjectOfType<DeviceSwitch>().Interact(p);GameObject.Find("Goal steering mirror").GetComponent<SpaceMirror>().Cycle();
            float deadline=Time.realtimeSinceStartup+5;while(!review.IsPlaying&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(session.goal.Satisfied&&review.IsPlaying,"Feeding selected goal starts automatic completion review");Check(review.RoutePointCount>=3,"Review snapshots successful multi-segment world light path");
            yield return new WaitForSecondsRealtime(1.4f);Check(review.PlaybackProgress>0&&Time.timeScale==0,"Three-dimensional path playback advances while gameplay is frozen");Capture("completed-route-review.png");
            yield return new WaitForSecondsRealtime(.9f);Check(!review.IsPlaying&&review.Played&&Time.timeScale==1,"Review restores normal camera and simulation after playback");
            File.WriteAllLines(Path.Combine(folder,"exposure-results.txt"),results);Application.Quit(results.Exists(s=>s.StartsWith("FAIL"))?1:0);
        }
    }
}
