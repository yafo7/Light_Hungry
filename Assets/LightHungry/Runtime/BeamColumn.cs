using UnityEngine;
namespace LightHungry {
 [ExecuteAlways] public sealed class BeamColumn:MonoBehaviour {
  Mesh mesh;MeshRenderer visual;MaterialPropertyBlock properties;
  public void Set(Vector3 start,Vector3 end,float diameter,Color color,Material material,RoomFaces faces){
   if(!visual){var filter=gameObject.AddComponent<MeshFilter>();visual=gameObject.AddComponent<MeshRenderer>();mesh=new Mesh{name="Light column cylinder"};const int sides=32;var vertices=new Vector3[(sides+1)*2];var normals=new Vector3[vertices.Length];var uv=new Vector2[vertices.Length];var triangles=new int[sides*6];
    for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;var n=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);for(int z=0;z<2;z++){int k=i*2+z;vertices[k]=n*.5f+Vector3.forward*z;normals[k]=n;uv[k]=new Vector2((float)i/sides,z);}if(i<sides){int k=i*2,t=i*6;triangles[t]=k;triangles[t+1]=k+1;triangles[t+2]=k+2;triangles[t+3]=k+2;triangles[t+4]=k+1;triangles[t+5]=k+3;}}
    for(int t=0;t<triangles.Length;t+=3){int swap=triangles[t+1];triangles[t+1]=triangles[t+2];triangles[t+2]=swap;}
    mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();filter.sharedMesh=mesh;properties=new MaterialPropertyBlock();gameObject.AddComponent<FaceMembership>();
   }
   Vector3 delta=end-start;transform.SetPositionAndRotation(start,Quaternion.LookRotation(delta.sqrMagnitude>.00001f?delta:Vector3.forward));transform.localScale=new Vector3(diameter,diameter,delta.magnitude);visual.sharedMaterial=material;visual.sortingOrder=18;properties.SetColor("_Tint",color);properties.SetFloat("_Length",delta.magnitude);visual.SetPropertyBlock(properties);GetComponent<FaceMembership>().faces=faces;visual.enabled=delta.magnitude>.001f;
  }
  void OnDestroy(){if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
 }
}
