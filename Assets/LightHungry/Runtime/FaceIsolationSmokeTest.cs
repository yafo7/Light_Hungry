using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightHungry {
    public sealed class FaceIsolationSmokeTest:MonoBehaviour {
        readonly List<string> results=new List<string>();string folder;
        void Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-faceQA")<0)return;folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification"));StartCoroutine(Run());}
        void Check(bool value,string message){results.Add((value?"PASS ":"FAIL ")+message);Debug.Log(results[results.Count-1]);}
        Texture2D Render(Camera camera,int width,int height){var rt=new RenderTexture(width,height,24);var previous=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();camera.targetTexture=target;RenderTexture.active=previous;rt.Release();Destroy(rt);return texture;}
        IEnumerator Run(){
            yield return null;var session=GetComponent<LabSession>();session.player.automationInput=true;session.player.GetComponent<LightExposure>().automatic=false;session.GetComponent<LightRouteReview>().automatic=false;
            var view=CameraViewController.Active;session.SetPaused(true);var playerPosition=session.player.transform.position;
            var fixture=new GameObject("Face QA fixture");var cameraObject=new GameObject("Face QA camera");cameraObject.transform.SetParent(fixture.transform);cameraObject.transform.position=new Vector3(100,0,-10);var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=2;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;cameraObject.AddComponent<FaceVisibility>();
            var colors=new[]{Color.red,Color.green,Color.blue,Color.yellow};var renderers=new List<Renderer>();var materials=new List<Material>();
            for(int i=0;i<4;i++){var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.name="Face marker "+(i+1);marker.transform.SetParent(fixture.transform);marker.transform.position=new Vector3(100,0,0);marker.GetComponent<Collider>().enabled=false;marker.AddComponent<FaceMembership>().faces=FaceMembership.ForView(i+1);var material=new Material(Shader.Find("Sprites/Default"));material.color=colors[i];materials.Add(material);marker.GetComponent<Renderer>().sharedMaterial=material;renderers.Add(marker.GetComponent<Renderer>());}
            for(int face=1;face<=4;face++){
                view.EndTravelView(face);yield return new WaitForEndOfFrame();var image=Render(camera,64,64);Color center=image.GetPixel(32,32);Check(Vector3.Distance(new Vector3(center.r,center.g,center.b),new Vector3(colors[face-1].r,colors[face-1].g,colors[face-1].b))<.1f,"Actual rendered pixels isolate face "+face+" even when all four objects overlap");Destroy(image);
                Check(renderers.TrueForAll(r=>!r.forceRenderingOff&&r.gameObject.activeInHierarchy),"Face "+face+" restores rendering flags and keeps objects alive");
                var screenshot=Render(Camera.main,1280,720);File.WriteAllBytes(Path.Combine(folder,"isolated-face-"+face+".png"),screenshot.EncodeToPNG());Destroy(screenshot);
            }
            Check(Vector3.Distance(playerPosition,session.player.transform.position)<.001f,"Rendering four faces never moves the player");
            Check(FaceVisibility.NearestView(44)==1&&FaceVisibility.NearestView(46)==3&&FaceVisibility.NearestView(-46)==4,"Rotation selects one nearest face without showing four faces together");
            Destroy(fixture);foreach(var material in materials)Destroy(material);view.EndTravelView(1);session.SetPaused(false);
            var gateObject=GameObject.CreatePrimitive(PrimitiveType.Cube);gateObject.name="QA circuit output";gateObject.transform.position=new Vector3(100,20,0);gateObject.AddComponent<FaceMembership>().faces=RoomFaces.Right;var gate=gateObject.AddComponent<SpaceDevice>();gate.gate=gateObject.GetComponent<Collider>();gate.movingPart=gateObject.transform;
            var a=new GameObject("QA switch A").AddComponent<DeviceSwitch>();var b=new GameObject("QA switch B").AddComponent<DeviceSwitch>();var circuit=gateObject.AddComponent<DeviceCircuit>();circuit.output=gate;circuit.inputs.Add(new CircuitInput{source=a});circuit.inputs.Add(new CircuitInput{source=b});
            yield return new WaitForEndOfFrame();Check(!gate.state&&gate.gate.enabled,"Circuit starts closed with both inputs inactive");a.Interact(session.player);yield return new WaitForEndOfFrame();Check(gate.state&&!gate.gate.enabled,"Any input opens target through actual interaction");
            circuit.rule=CircuitRule.All;yield return new WaitForEndOfFrame();Check(!gate.state&&gate.gate.enabled,"All inputs keeps target closed until both are active");b.Interact(session.player);yield return new WaitForEndOfFrame();Check(gate.state&&!gate.gate.enabled,"Both inputs open target while its face is hidden");
            b.Interact(session.player);yield return new WaitForEndOfFrame();Check(!gate.state,"Releasing one input closes All-rule target");Destroy(gateObject);Destroy(a.gameObject);Destroy(b.gameObject);
            var selectedGoal=session.goal;selectedGoal.requiredSeconds=100;var steering=GameObject.Find("Goal steering mirror").GetComponent<SpaceMirror>();steering.Cycle();GameObject.Find("DeviceSwitch").GetComponent<DeviceSwitch>().Interact(session.player);yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
            Check(FaceMembership.Of(selectedGoal)==RoomFaces.Right&&selectedGoal.Lit&&session.beams.ReceivedPaths.ContainsKey(selectedGoal),"Hidden side optics still feed the actual receiver from the front view");
            bool tagged=true;foreach(var line in session.beams.GetComponentsInChildren<LineRenderer>())if(line.enabled&&line.GetComponent<FaceMembership>()==null)tagged=false;Check(tagged,"World beam segments have independent face membership");
            File.WriteAllLines(Path.Combine(folder,"face-results.txt"),results);Application.Quit(results.Exists(s=>s.StartsWith("FAIL"))?1:0);
        }
    }
}
