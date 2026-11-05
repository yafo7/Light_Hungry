using System.IO;
using UnityEngine;

public static class NativeFrameCapture {
 // Hidden QA windows have a black swapchain; render the actual scene camera on the native GPU.
 public static void Save(string path,Camera camera=null){var c=camera?camera:Camera.main;if(!c)return;var previous=c.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1440,810,24,RenderTextureFormat.ARGB32);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1440,810,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1440,810),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.Destroy(t);c.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
}
