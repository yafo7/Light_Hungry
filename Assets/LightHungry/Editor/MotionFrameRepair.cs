using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightHungry.EditorTools {
 public static class MotionFrameRepair {
  sealed class Part {public List<int> pixels=new List<int>();public int x0=int.MaxValue,y0=int.MaxValue,x1,y1;}
  public static void Repair(){
   const string recipe=WatercolorAssets.Folder+"motion.lhframes";if(!System.IO.File.Exists(recipe))System.IO.File.WriteAllText(recipe,"Connected character silhouettes from motion.png");AssetDatabase.ImportAsset(recipe,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
   foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(recipe).OfType<Sprite>()){string path=WatercolorAssets.Folder+"Frames/"+sprite.name+".asset";var previous=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(previous)EditorUtility.CopySerialized(sprite,previous);else AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(sprite),path);}
   AssetDatabase.SaveAssets();Debug.Log("MOTION_FRAMES_REPAIRED 16 connected silhouettes, original GUIDs retained");
  }
  public static Sprite[] Generate(){
   var output=new List<Sprite>();
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(WatercolorAssets.Folder+"motion.png");var colors=texture.GetPixels32();int w=texture.width,h=texture.height;var seen=new bool[colors.Length];var parts=new List<Part>();var queue=new Queue<int>();
   for(int seed=0;seed<colors.Length;seed++){
    if(seen[seed]||colors[seed].a<110)continue;var part=new Part();queue.Enqueue(seed);seen[seed]=true;
    while(queue.Count>0){int i=queue.Dequeue(),x=i%w,y=i/w;part.pixels.Add(i);part.x0=Math.Min(x,part.x0);part.y0=Math.Min(y,part.y0);part.x1=Math.Max(x,part.x1);part.y1=Math.Max(y,part.y1);
     foreach(int n in new[]{x>0?i-1:-1,x<w-1?i+1:-1,y>0?i-w:-1,y<h-1?i+w:-1})if(n>=0&&!seen[n]&&colors[n].a>=110){seen[n]=true;queue.Enqueue(n);}
    }
    if(part.pixels.Count>3000)parts.Add(part);
   }
   if(parts.Count!=16)throw new Exception("Motion sheet must contain 16 separate character silhouettes, found "+parts.Count);
   var ordered=parts.OrderByDescending(p=>(p.y0+p.y1)*.5f).ToArray();
   for(int row=0;row<4;row++){
    var group=ordered.Skip(row*4).Take(4).OrderBy(p=>p.x0).ToArray();
    for(int col=0;col<4;col++){
     var p=group[col];int x0=Math.Max(0,p.x0-3),y0=Math.Max(0,p.y0-3),x1=Math.Min(w,p.x1+4),y1=Math.Min(h,p.y1+4);var rect=new Rect(x0,y0,x1-x0,y1-y0);
     // Align at the feet, rather than at the asymmetric flame-tip bounding-box center.
     var feet=p.pixels.Where(i=>i/w<p.y0+18).ToArray();float anchor=feet.Length>0?(float)feet.Average(i=>i%w):(p.x0+p.x1)*.5f;
     var pivot=new Vector2((anchor-x0)/rect.width,(p.y0-y0)/rect.height);
     // Extract the selected silhouette into an imported texture. This also excludes neighboring
     // poses that overlap this rectangle vertically, and keeps transparent anti-aliased edges.
     var scan=p.pixels.GroupBy(i=>i/w).ToDictionary(g=>g.Key,g=>new[]{g.Min(i=>i%w),g.Max(i=>i%w)});
     int tw=x1-x0,th=y1-y0;var isolated=new Color32[tw*th];
     for(int y=y0;y<y1;y++){
      var spans=new List<int[]>();for(int dy=-2;dy<=2;dy++)if(scan.TryGetValue(y+dy,out var span))spans.Add(span);if(spans.Count==0)continue;
      int left=Math.Max(x0,spans.Min(a=>a[0])-2),right=Math.Min(x1,spans.Max(a=>a[1])+3);
      for(int x=left;x<right;x++)isolated[(y-y0)*tw+x-x0]=colors[y*w+x];
     }
     var frameTexture=new Texture2D(tw,th,TextureFormat.RGBA32,false);frameTexture.SetPixels32(isolated);frameTexture.Apply();frameTexture.filterMode=FilterMode.Bilinear;frameTexture.wrapMode=TextureWrapMode.Clamp;
     string name="motion_"+(row*4+col).ToString("00");frameTexture.name=name+"_texture";var sprite=Sprite.Create(frameTexture,new Rect(0,0,tw,th),pivot,256,0,SpriteMeshType.FullRect);sprite.name=name;output.Add(sprite);
    }
   }
   return output.ToArray();
  }
 }
}
