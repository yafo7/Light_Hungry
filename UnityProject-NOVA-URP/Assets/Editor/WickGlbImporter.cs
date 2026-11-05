using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Narrow, explicit importer for the supplied static, uncompressed glTF 2.0 model.
// Original GLB is retained separately. The optimized GLB has one identity mesh node.
public static class WickGlbImporter {
 [Serializable] public class Root {public View[] bufferViews;public Accessor[] accessors;public GMesh[] meshes;public GImage[] images;public GNode[] nodes;}
 [Serializable] public class View {public int byteOffset,byteLength,byteStride;}
 [Serializable] public class Accessor {public int bufferView,byteOffset,componentType,count;public string type;}
 [Serializable] public class GMesh {public Primitive[] primitives;}
 [Serializable] public class Primitive {public Attr attributes;public int indices;}
 [Serializable] public class Attr {public int POSITION,NORMAL,TEXCOORD_0;}
 [Serializable] public class GImage {public int bufferView;public string mimeType;}
 [Serializable] public class GNode {public float[] matrix,translation,rotation,scale;}
 public static GameObject Import() {
  string dir="Assets/Models/Runtime/";byte[] b=File.ReadAllBytes(dir+"WickSpirit.glb");
  if(BitConverter.ToUInt32(b,0)!=0x46546c67||BitConverter.ToInt32(b,4)!=2)throw new Exception("Expected GLB 2.0");
  int jsonLength=BitConverter.ToInt32(b,12),bin=20+jsonLength+8;
  Root gltf=JsonUtility.FromJson<Root>(System.Text.Encoding.UTF8.GetString(b,20,jsonLength));
  if(gltf.meshes.Length!=1||gltf.meshes[0].primitives.Length!=1)throw new Exception("Importer expects one primitive");
  foreach(var node in gltf.nodes)if(node.matrix!=null||node.translation!=null||node.rotation!=null||node.scale!=null)throw new Exception("Nonidentity GLB node requires explicit transform handling");
  var p=gltf.meshes[0].primitives[0];var pa=gltf.accessors[p.attributes.POSITION];
  var vertices=new Vector3[pa.count];var normals=new Vector3[pa.count];var uv=new Vector2[pa.count];
  for(int i=0;i<pa.count;i++){
   vertices[i]=Read3(gltf,b,bin,p.attributes.POSITION,i);vertices[i].z=-vertices[i].z;
   normals[i]=Read3(gltf,b,bin,p.attributes.NORMAL,i);normals[i].z=-normals[i].z;
   var a=gltf.accessors[p.attributes.TEXCOORD_0];int o=Offset(gltf,bin,p.attributes.TEXCOORD_0,i,8);
   uv[i]=new Vector2(BitConverter.ToSingle(b,o),1-BitConverter.ToSingle(b,o+4));
  }
  var ia=gltf.accessors[p.indices];var triangles=new int[ia.count];int size=ia.componentType==5123?2:4;
  if(ia.componentType!=5123&&ia.componentType!=5125)throw new Exception("Unsupported indices");
  for(int i=0;i<ia.count;i++){int o=Offset(gltf,bin,p.indices,i,size);triangles[i]=size==2?BitConverter.ToUInt16(b,o):(int)BitConverter.ToUInt32(b,o);}
  for(int i=0;i<triangles.Length;i+=3){int t=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=t;}
  Mesh mesh=new Mesh{name="WickSpirit_99446tri",indexFormat=IndexFormat.UInt32};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();
  AssetDatabase.DeleteAsset(dir+"WickSpirit.mesh.asset");AssetDatabase.CreateAsset(mesh,dir+"WickSpirit.mesh.asset");
  var im=gltf.bufferViews[gltf.images[0].bufferView];byte[] jpeg=new byte[im.byteLength];Buffer.BlockCopy(b,bin+im.byteOffset,jpeg,0,jpeg.Length);File.WriteAllBytes(dir+"WickSpirit.basecolor.jpg",jpeg);
  AssetDatabase.ImportAsset(dir+"WickSpirit.basecolor.jpg",ImportAssetOptions.ForceSynchronousImport);
  var ti=(TextureImporter)AssetImporter.GetAtPath(dir+"WickSpirit.basecolor.jpg");ti.maxTextureSize=2048;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.SaveAndReimport();
  Material mat=new Material(Shader.Find("LightFeast/Spirit")){name="WickSpirit_Watercolour"};mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"WickSpirit.basecolor.jpg");
  AssetDatabase.DeleteAsset("Assets/Materials/WickSpirit.mat");AssetDatabase.CreateAsset(mat,"Assets/Materials/WickSpirit.mat");
  GameObject root=new GameObject("WickSpirit_Visual");var visual=new GameObject("OriginalMesh");visual.transform.SetParent(root.transform,false);
  visual.AddComponent<MeshFilter>().sharedMesh=mesh;visual.AddComponent<MeshRenderer>().sharedMaterial=mat;
  float h=mesh.bounds.size.y;visual.transform.localScale=Vector3.one/h;visual.transform.localPosition=new Vector3(-mesh.bounds.center.x/h,-mesh.bounds.min.y/h,-mesh.bounds.center.z/h);
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/WickSpirit.prefab");UnityEngine.Object.DestroyImmediate(root);
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../Validation/model-import.json"),"{\"originalTriangles\":1988934,\"runtimeTriangles\":"+triangles.Length/3+",\"runtimeVertices\":"+vertices.Length+",\"hasSkeleton\":false,\"normalizedHeight\":1}");
  return prefab;
 }
 static int Offset(Root r,int bin,int accessor,int i,int packed){var a=r.accessors[accessor];var v=r.bufferViews[a.bufferView];return bin+v.byteOffset+a.byteOffset+i*(v.byteStride>0?v.byteStride:packed);}
 static Vector3 Read3(Root r,byte[] b,int bin,int accessor,int i){if(r.accessors[accessor].componentType!=5126)throw new Exception("Expected float attributes; run gltfpack -noq");int o=Offset(r,bin,accessor,i,12);return new Vector3(BitConverter.ToSingle(b,o),BitConverter.ToSingle(b,o+4),BitConverter.ToSingle(b,o+8));}
}
