using UnityEditor.AssetImporters;
namespace LightHungry.EditorTools {
 [ScriptedImporter(1,"lhframes")] public sealed class MotionFrameImporter:ScriptedImporter {
  public override void OnImportAsset(AssetImportContext ctx){ctx.DependsOnSourceAsset(WatercolorAssets.Folder+"motion.png");var sprites=MotionFrameRepair.Generate();foreach(var sprite in sprites){ctx.AddObjectToAsset(sprite.name+"_texture",sprite.texture);ctx.AddObjectToAsset(sprite.name,sprite);}var info=new UnityEngine.TextAsset("16 isolated animation frames");ctx.AddObjectToAsset("sheet",info);ctx.SetMainObject(info);}
 }
}
