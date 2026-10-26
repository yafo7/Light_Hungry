using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public sealed class LabSmokeTest:MonoBehaviour {
        static readonly List<string> results=new List<string>();static bool afterReset;bool active;
        string directory;
        void Start(){active=Array.IndexOf(Environment.GetCommandLineArgs(),"-labQA")>=0;if(active){directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification"));Directory.CreateDirectory(directory);StartCoroutine(Run());}}
        void Check(bool condition,string description){results.Add((condition?"PASS ":"FAIL ")+description);Debug.Log(results[results.Count-1]);}
        void Teleport(PlayerController p,Vector2 position){p.Body.position=position;p.Body.velocity=Vector2.zero;Physics2D.SyncTransforms();}
        void Capture(string file){
            var camera=Camera.main;var target=new RenderTexture(1280,720,24);var previous=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(directory,file),texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;Destroy(texture);target.Release();Destroy(target);
        }
        IEnumerator Run(){
            var session=GetComponent<LabSession>();var p=session.player;p.automationInput=true;
            yield return new WaitForSeconds(.5f);
            if(afterReset){
                Check(!p.stored&&p.Mode==PlayerController.AbilityMode.Normal,"Reset restores player light state");
                Check(session.goal.Progress==0,"Reset restores hungry beast");
                var rail=FindObjectOfType<RailDevice>();Check(rail.Position01<.02f,"Reset restores rail position");
                yield return new WaitForEndOfFrame();Capture("InteractionLab.png");yield return new WaitForSeconds(.5f);
                File.WriteAllLines(Path.Combine(directory,"playmode-results.txt"),results);
                Application.Quit(results.Exists(s=>s.StartsWith("FAIL"))?1:0);yield break;
            }
            Check(FindObjectsOfType<TextMesh>(true).Length==0,"Playable scene has no world-space text, including inactive objects");
            Check(typeof(LabSession).GetMethod("OnGUI",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)==null,"Session has no runtime instructional HUD renderer");
            Check(session.beams.SegmentsLastFrame>2,"Sources and mirror/lens paths traced");
            Teleport(p,new Vector2(-10,-5.7f));yield return new WaitForSeconds(.3f);float x=p.transform.position.x;
            p.automationHorizontal=1;yield return new WaitForSeconds(.35f);p.automationHorizontal=0;Check(p.transform.position.x>x+.6f,"Physics horizontal movement");
            Teleport(p,new Vector2(-2,-5.7f));yield return new WaitForSeconds(.3f);float y=p.transform.position.y;p.automationJump=true;yield return new WaitForSeconds(.15f);Check(p.transform.position.y>y+.5f,"Grounded jump moves upward");
            p.reflectUnlocked=false;p.EndRelease();p.ToggleReflection();Check(p.Mode==PlayerController.AbilityMode.Normal,"Locked reflection rejects input");
            p.reflectUnlocked=true;p.ToggleReflection();Check(p.Mode==PlayerController.AbilityMode.Reflect,"Unlocked reflection enters state");
            p.ToggleSwallow();Check(p.Mode==PlayerController.AbilityMode.Mouth,"Swallow replaces reflection state");
            Teleport(p,new Vector2(-11,-3.3f));yield return new WaitForSeconds(.12f);Check(p.stored&&p.Mode==PlayerController.AbilityMode.Normal,"Sunlight absorbed through actual ray collision");
            var benchReceiver=GameObject.Find("Bench receiver").GetComponent<BeastReceiver>();var benchLight=FindObjectOfType<SwitchableLight>();benchLight.source.active=false;benchReceiver.Clear();
            Teleport(p,new Vector2(-2,.1f));p.SetAim(Vector2.right);p.ToggleSwallow();Check(p.Mode==PlayerController.AbilityMode.Release&&!p.stored,"Stored light becomes release emitter");
            yield return new WaitForSeconds(.15f);Check(p.ReleaseRemaining>0,"Release remains active");Check(benchReceiver.Lit,"Released player light crosses lenses and reaches receiver with flashlight OFF");p.ToggleSwallow();Check(p.Mode==PlayerController.AbilityMode.Normal,"K cancels release without duplicating charge");benchLight.source.active=true;
            var oldReceiverPosition=benchReceiver.transform.position;benchReceiver.transform.position=new Vector2(-11.39f,-.3f);benchReceiver.Clear();p.ToggleReflection();p.SetAim(Vector2.up);Teleport(p,new Vector2(-11,-3.3f));yield return new WaitForSeconds(.12f);Check(benchReceiver.Lit,"Player reflection redirects sunlight upward into receiver");p.EndRelease();benchReceiver.transform.position=oldReceiverPosition;
            Teleport(p,new Vector2(-13.7f,-5.7f));
            var mirror=GameObject.Find("Rail mirror").GetComponentInChildren<MirrorSurface>();int angle=mirror.angleIndex;mirror.Interact(p);Check(mirror.angleIndex!=angle,"Mirror angle interaction");mirror.Interact(p);int size=mirror.sizeIndex;mirror.Resize();Check(mirror.sizeIndex!=size,"Mirror size interaction");mirror.Resize();mirror.Resize();
            var toggle=FindObjectOfType<SwitchableLight>();toggle.Interact(p);Check(!toggle.source.active,"Flashlight can be switched off");toggle.Interact(p);
            var box=FindObjectOfType<PushableBox>();var body=box.GetComponent<Rigidbody2D>();float bx=box.transform.position.x;
            for(int i=0;i<12;i++){box.Drive(2);yield return new WaitForFixedUpdate();}Check(box.transform.position.x>bx+.12f,"Box moves with push drive and physics");
            var railDevice=FindObjectOfType<RailDevice>();
            Teleport(p,new Vector2(6,-5.7f));yield return new WaitForSeconds(.25f);Check(railDevice.rightPlate.IsPressed,"Player presses pressure plate");float t=railDevice.Position01;yield return new WaitForSeconds(.25f);Check(railDevice.Position01>t,"Pressed button drives rail");
            Teleport(p,new Vector2(-13.7f,-5.7f));yield return new WaitForSeconds(.12f);
            FindObjectOfType<RailLever>().Interact(p);yield return new WaitForSeconds(2.2f);Check(railDevice.Position01>.98f,"Pulley lever moves mirror to rail endpoint");
            yield return new WaitForSeconds(3.3f);Check(session.goal.Satisfied,"Sunlight reflects from rail mirror into beast mouth");
            File.WriteAllLines(Path.Combine(directory,"beam-paths.txt"),session.beams.LastPaths);Debug.Log("Goal progress="+session.goal.Progress+" rail="+railDevice.transform.position+" mirror="+mirror.transform.position+" angle="+mirror.Angle);
            yield return new WaitForEndOfFrame();Capture("FedBeast.png");yield return new WaitForSeconds(.3f);
            afterReset=true;session.Restart();
        }
    }
}
