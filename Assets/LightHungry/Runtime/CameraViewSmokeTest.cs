using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightHungry {
    public sealed class CameraViewSmokeTest : MonoBehaviour {
        static readonly List<string> results = new List<string>();
        static bool afterReset;
        string directory;
        CameraViewController view;
        PlayerController player;
        LabSession session;
        void Start() {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-viewQA") < 0) return;
            directory = Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification"));
            Directory.CreateDirectory(directory);
            view = CameraViewController.Active;
            session = GetComponent<LabSession>();
            player = session.player;
            player.automationInput = true;
            StartCoroutine(Run());
        }
        void Check(bool condition,string message) {
            results.Add((condition?"PASS ":"FAIL ")+message);
            Debug.Log(results[results.Count-1]);
        }
        void Teleport(Vector3 position) {
            player.SetSpatialPosition(position);
        }
        void Capture(string name) {
            var camera = Camera.main;
            var target = new RenderTexture(1280,720,24);
            var previous = RenderTexture.active;
            camera.targetTexture=target;
            camera.Render();
            RenderTexture.active=target;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(directory,name),texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;
            Destroy(texture);target.Release();Destroy(target);
        }
        IEnumerator Run() {
            yield return new WaitForSeconds(.6f);
            if(afterReset) {
                Check(view.CurrentView==1&&!view.IsRotating,"Reset returns to default camera view");
                Check(Time.timeScale==1&&!session.paused,"Reset restores running simulation");
                Check(Vector3.Distance(player.transform.position,new Vector3(-14.4f,-5.83f,0))<.15f,"Reset restores authored player position");
                File.WriteAllLines(Path.Combine(directory,"camera-view-results.txt"),results);
                Application.Quit(results.Exists(line=>line.StartsWith("FAIL"))?1:0);yield break;
            }
            Check(view&&SpatialLabWorld.Active&&player.Body3D,"Existing scene has spatial camera and physics adapter");
            Check(FindObjectsOfType<TextMesh>(true).Length==0&&FindObjectsOfType<Canvas>(true).Length==0,"Camera feature adds no text or instructional UI");
            Check(GameObject.Find("ConvexLens")&&GameObject.Find("ConcaveLens")&&GameObject.Find("Flashlight"),"User-added scene objects remain present");
            var mirror=GameObject.Find("Rail mirror").GetComponentInChildren<MirrorSurface>();
            var rail=FindObjectOfType<RailDevice>();
            int angle=mirror.angleIndex;mirror.Interact(player);
            int retainedAngle=mirror.angleIndex;
            var source=GameObject.Find("Sunlight").transform;
            var sourcePosition=source.position;
            var mirrorPosition=mirror.transform.position;
            yield return null;
            int[] sequence={2,3,4,1};
            Vector3[] rights={Vector3.left,Vector3.back,Vector3.forward,Vector3.right};
            string[] names={"view-2-opposite.png","view-3-left.png","view-4-right.png","view-1-default.png"};
            for(int i=0;i<sequence.Length;i++) {
                Vector3 before=player.Body3D.position;
                string paths=string.Join("\n",session.beams.LastPaths);
                float railBefore=rail.Position01;
                Check(view.RequestView(sequence[i]),"Accept absolute view "+sequence[i]);
                Check(view.IsRotating&&Time.timeScale==0,"View "+sequence[i]+" pauses simulation during transition");
                yield return new WaitForSecondsRealtime(.2f);
                if(i==0) {yield return new WaitForEndOfFrame();Capture("view-transition.png");}
                Check(Vector3.Distance(before,player.Body3D.position)<.001f,"Transition preserves actor world position "+sequence[i]);
                Check(Mathf.Abs(rail.Position01-railBefore)<.001f,"Transition freezes device movement "+sequence[i]);
                yield return new WaitForSecondsRealtime(.7f);
                Check(!view.IsRotating&&view.CurrentView==sequence[i]&&Time.timeScale==1,"Absolute view settles "+sequence[i]);
                Check(Vector3.Dot(view.ViewRight,rights[i])>.999f,"View-relative movement basis "+sequence[i]);
                Check(Vector3.Distance(before,player.Body3D.position)<.025f,"No actor teleport after view "+sequence[i]);
                Check(source.position==sourcePosition&&mirror.transform.position==mirrorPosition&&mirror.angleIndex==retainedAngle,"World positions and mirror state persist "+sequence[i]);
                Check(string.Join("\n",session.beams.LastPaths)==paths,"Camera alone preserves world beam paths "+sequence[i]);
                Check(Vector3.Dot(player.visual.transform.forward,Camera.main.transform.forward)>.999f,"Actor sprite faces camera "+sequence[i]);
                yield return new WaitForEndOfFrame();Capture(names[i]);
            }
            mirror.angleIndex=angle;mirror.Apply();
            foreach(int face in new[]{1,2,3,4}) {
                view.RequestView(face);yield return new WaitForSecondsRealtime(.8f);
                Teleport(new Vector3(-10,-5.7f,0));yield return new WaitForSeconds(.25f);
                Vector3 before=player.Body3D.position;
                player.automationHorizontal=1;yield return new WaitForSeconds(.3f);player.automationHorizontal=0;
                Vector3 delta=player.Body3D.position-before;
                Check(Vector3.Dot(delta,view.ViewRight)>.65f,"D movement follows screen right in view "+face);
                Check(Mathf.Abs(Vector3.Dot(delta,Camera.main.transform.forward))<.015f,"Movement stays on current depth plane "+face);
                yield return new WaitForSeconds(.2f);
            }
            float height=player.transform.position.y;player.automationJump=true;
            yield return new WaitForSeconds(.15f);
            Check(player.transform.position.y>height+.5f,"Side-view jump remains vertical");
            Check(!view.RequestView(2),"Airborne view change is rejected");
            yield return new WaitForSeconds(1);
            view.RequestView(3);view.RequestView(1);view.RequestView(2);
            yield return new WaitForSecondsRealtime(1.6f);
            Check(view.CurrentView==2&&!view.IsRotating&&Time.timeScale==1,"Rapid requests use last target without stuck time scale");
            session.SetPaused(true);
            Check(!view.RequestView(4),"Paused game rejects new rotation");
            session.SetPaused(false);
            view.RequestView(4);session.SetPaused(true);
            yield return new WaitForSecondsRealtime(.9f);
            Check(!view.IsRotating&&Time.timeScale==0&&session.paused,"Pause during rotation remains paused after camera settles");
            session.SetPaused(false);
            Check(Time.timeScale==1,"Unpause restores gameplay after rotation");
            view.RequestView(3);
            yield return new WaitForSecondsRealtime(.1f);
            afterReset=true;session.Restart();
        }
    }
}
