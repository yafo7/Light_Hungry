using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class NativeFrameCapture {
 public static void Save(string path,Camera camera=null){
  var source=camera?camera:Camera.main;if(!source)return;
  var go=new GameObject("QA_URP_Snapshot");var c=go.AddComponent<Camera>();c.CopyFrom(source);c.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);c.enabled=false;
  System.Action<ScriptableRenderContext,System.Collections.Generic.List<Camera>> capture=null;
  capture=(context,cameras)=>{
   RenderPipelineManager.beginContextRendering-=capture;
   var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1440,810,24,RenderTextureFormat.ARGB32);c.targetTexture=rt;
   UniversalRenderPipeline.RenderSingleCamera(context,c);context.Submit();RenderTexture.active=rt;
   var t=new Texture2D(1440,810,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1440,810),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());
   Object.Destroy(t);c.targetTexture=null;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(go);
  };
  RenderPipelineManager.beginContextRendering+=capture;
 }
}

