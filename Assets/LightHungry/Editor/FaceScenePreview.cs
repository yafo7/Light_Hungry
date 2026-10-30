using UnityEditor;
using UnityEngine;

namespace LightHungry.EditorTools {
    [InitializeOnLoad]
    public static class FaceScenePreview {
        public static bool enabled;
        public static int view=1;
        static FaceVisibility.RenderScope scope;
        static Camera scopedCamera;
        static FaceScenePreview(){Camera.onPreCull+=Before;Camera.onPostRender+=After;AssemblyReloadEvents.beforeAssemblyReload+=Reset;}
        static void Before(Camera camera){
            if(!enabled||EditorApplication.isPlaying||camera.cameraType!=CameraType.SceneView)return;
            Reset();scopedCamera=camera;scope=new FaceVisibility.RenderScope(FaceMembership.ForView(view));
        }
        static void After(Camera camera){if(camera==scopedCamera)Reset();}
        public static void Reset(){scope?.Dispose();scope=null;scopedCamera=null;}
    }
}
