using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightHungry.EditorTools {
    public static class WatercolorAssets {
        public const string Folder="Assets/LightHungry/Art/Watercolor/";
        public static void Import(){
            AssetDatabase.Refresh();
            Slice("identity",3,new[]{0f,1f});MotionFrameRepair.Repair();
            Slice("interaction",4,new[]{0f,.28f,.53f,.77f,1f});
            Slice("props",4,new[]{0f,.29f,.49f,.73f,1f});
            Slice("terrain",4,new[]{0f,.31f,.68f,1f});Slice("beast",4,new[]{0f,.64f,1f});
            var path=Folder+"background.png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
        }
        static void Slice(string name,int columns,float[] rows){
            string path=Folder+name+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=256;importer.isReadable=true;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Color32[] pixels=texture.GetPixels32();var metas=new SpriteMetaData[columns*(rows.Length-1)];
            for(int row=0;row<rows.Length-1;row++)for(int col=0;col<columns;col++){
                int x0=Mathf.FloorToInt(col*texture.width/(float)columns)+2,x1=Mathf.FloorToInt((col+1)*texture.width/(float)columns)-2;
                int y0=Mathf.FloorToInt((1-rows[row+1])*texture.height)+2,y1=Mathf.FloorToInt((1-rows[row])*texture.height)-2;
                int minX=x1,minY=y1,maxX=x0,maxY=y0;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>110){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                if(maxX<=minX){minX=x0;maxX=x1;minY=y0;maxY=y1;}
                minX=Math.Max(x0,minX-4);maxX=Math.Min(x1,maxX+4);minY=Math.Max(y0,minY-4);maxY=Math.Min(y1,maxY+4);
                metas[row*columns+col]=new SpriteMetaData{name=name+"_"+(row*columns+col).ToString("00"),rect=new Rect(minX,minY,maxX-minX,maxY-minY),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,0),border=Vector4.zero};
            }
            string folder=Folder+"Frames";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(Folder.TrimEnd('/'),"Frames");
            foreach(var meta in metas){var sprite=Sprite.Create(texture,meta.rect,meta.pivot,256,0,SpriteMeshType.FullRect);sprite.name=meta.name;string spritePath=folder+"/"+meta.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);if(existing){EditorUtility.CopySerialized(sprite,existing);UnityEngine.Object.DestroyImmediate(sprite);}else AssetDatabase.CreateAsset(sprite,spritePath);}
            AssetDatabase.SaveAssets();
        }
        public static Sprite[] Frames(string name)=>AssetDatabase.FindAssets("t:Sprite",new[]{Folder+"Frames"}).Select(id=>AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(id))).Where(s=>s&&s.name.StartsWith(name+"_",StringComparison.Ordinal)).OrderBy(s=>s.name,StringComparer.Ordinal).ToArray();
        public static Sprite Frame(string name,int index)=>Frames(name)[index];
    }
}
