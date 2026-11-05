using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// URP draws one forward pass per material. Schedule the three original rim passes explicitly.
public class SpiritOutlineFeature : ScriptableRendererFeature {
 class OutlinePass : ScriptableRenderPass {
  public override void Execute(ScriptableRenderContext context, ref RenderingData data) {
   var cmd=CommandBufferPool.Get("Watercolour spirit rims");
   foreach(var skin in Object.FindObjectsOfType<SkinnedMeshRenderer>()) {
    if(!skin.enabled||!skin.gameObject.activeInHierarchy)continue;
    var mat=skin.sharedMaterial;
    if(!mat||mat.shader.name!="LightFeast/Spirit")continue;
    cmd.DrawRenderer(skin,mat,0,4);
    for(int pass=1;pass<=3;pass++)cmd.DrawRenderer(skin,mat,0,pass);
   }
   context.ExecuteCommandBuffer(cmd);CommandBufferPool.Release(cmd);
  }
 }
 OutlinePass pass;
 public override void Create(){pass=new OutlinePass{renderPassEvent=RenderPassEvent.BeforeRenderingTransparents};}
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){renderer.EnqueuePass(pass);}
}
