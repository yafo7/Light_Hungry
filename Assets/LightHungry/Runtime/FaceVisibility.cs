using System.Collections.Generic;
using UnityEngine;

namespace LightHungry {
    // This filters rendering only. Colliders, scripts and off-screen optics keep running.
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class FaceVisibility : MonoBehaviour {
        public int editingView=1;
        public static int NearestView(float yaw) {
            int result=1;float distance=361;
            for(int view=1;view<=4;view++){float d=Mathf.Abs(Mathf.DeltaAngle(yaw,CameraViewController.Yaw(view)));if(d<distance){distance=d;result=view;}}
            return result;
        }
        public int DisplayedView=>Application.isPlaying&&CameraViewController.Active?NearestView(CameraViewController.Active.VisualYaw):editingView;
        RenderScope scope;
        void OnPreCull(){
            scope?.Dispose();scope=null;
            if(Application.isPlaying&&LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()?.IsPlaying==true)return;
            scope=new RenderScope(FaceMembership.ForView(DisplayedView));
        }
        void OnPostRender(){scope?.Dispose();scope=null;}
        void OnDisable(){scope?.Dispose();scope=null;}
        public sealed class RenderScope:System.IDisposable {
            readonly List<Renderer> renderers=new List<Renderer>();readonly List<bool> previous=new List<bool>();
            public RenderScope(RoomFaces visible) {
                foreach(var renderer in Object.FindObjectsOfType<Renderer>()) {
                    if((FaceMembership.Of(renderer)&visible)!=0)continue;
                    renderers.Add(renderer);previous.Add(renderer.forceRenderingOff);renderer.forceRenderingOff=true;
                }
            }
            public void Dispose(){for(int i=0;i<renderers.Count;i++)if(renderers[i])renderers[i].forceRenderingOff=previous[i];renderers.Clear();previous.Clear();}
        }
    }
}
