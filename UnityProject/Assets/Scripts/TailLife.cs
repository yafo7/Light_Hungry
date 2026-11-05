using UnityEngine;

public class TailLife : MonoBehaviour {
 public Renderer scaleGlow; public ParticleSystem awakeningParticles;public MeshFilter tailMesh;
 Mesh mesh;Vector3[] original,work;public bool Awakened {get;private set;}public float Charge {get;private set;}
 void Start(){mesh=tailMesh.mesh;original=mesh.vertices;work=(Vector3[])original.Clone();}
 public void Illuminate(float value){Charge=value;}
 public void Wake(){if(Awakened)return;Awakened=true;awakeningParticles.Play();}
 void Update(){
  float pulse=Awakened?.72f+Mathf.Sin(Time.time*2)*.12f:.1f+Charge*.65f;
  scaleGlow.material.SetColor("_Color",new Color(1,.65f,.16f,pulse));
  if(mesh==null)return;float breath=(Awakened?.045f:.008f)*Mathf.Sin(Time.time*(Awakened?1.9f:1));
  for(int i=0;i<work.Length;i++){work[i]=original[i];float x=original[i].x/22;work[i].y+=breath*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,.74f,x)))*Mathf.Sin(Mathf.PI*Mathf.Clamp01(x/.75f));}mesh.vertices=work;
 }
}
