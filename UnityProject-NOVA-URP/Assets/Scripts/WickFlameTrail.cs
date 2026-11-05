using UnityEngine;
// Official NOVA 2.0.0 sample fire adapted to the spirit's head.
[DefaultExecutionOrder(100)]
public class WickFlameTrail:MonoBehaviour {
 Renderer tongue;Material tongueMaterial;int diagnostics;PlayerMotor motor;ParticleSystem[] flames;Vector3 lastPosition;GameObject effect;
 public void Initialize(PlayerMotor owner){
  motor=owner;effect=Instantiate(Resources.Load<GameObject>("NovaHeadFlame"),transform);effect.name="NOVA_HeadFlame_OfficialSample";
  effect.transform.localScale=Vector3.one*.16f;
  flames=effect.GetComponentsInChildren<ParticleSystem>();
  foreach(var ps in flames){var main=ps.main;main.simulationSpace=ps.name=="ps_sparks"?ParticleSystemSimulationSpace.World:ParticleSystemSimulationSpace.Local;ps.transform.localRotation=Quaternion.identity;ps.GetComponent<ParticleSystemRenderer>().renderMode=ParticleSystemRenderMode.Billboard;main.scalingMode=ParticleSystemScalingMode.Hierarchy;ps.Play();}
  var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.24f,0,0),new Vector3(.24f,0,0),new Vector3(-.24f,.65f,0),new Vector3(.24f,.65f,0)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateNormals();
  var panel=new GameObject("NOVA_OrientedFlameSequence");panel.transform.SetParent(transform,false);panel.AddComponent<MeshFilter>().sharedMesh=mesh;tongue=panel.AddComponent<MeshRenderer>();
  var source=System.Array.Find(flames,p=>p.name=="ps_fire_main").GetComponent<ParticleSystemRenderer>().sharedMaterial;tongueMaterial=new Material(source);tongueMaterial.SetFloat("_BaseMapProgressCoord",0);tongueMaterial.SetFloat("_BaseMapRotation",0);tongueMaterial.DisableKeyword("_BASE_MAP_ROTATION_ENABLED");tongueMaterial.SetFloat("_AlphaTransitionProgressCoord",0);tongueMaterial.SetColor("_TintColor",new Color(1.1f,.88f,.5f,1));tongue.sharedMaterial=tongueMaterial;
  foreach(var ps in flames)if(ps.name=="ps_fire_main"||ps.name=="ps_fire_back")ps.GetComponent<ParticleSystemRenderer>().enabled=false;
  lastPosition=motor.transform.position;
  Debug.Log("NOVA_HEAD_FLAME shader="+System.Array.Find(effect.GetComponentsInChildren<ParticleSystemRenderer>(),r=>r.enabled&&r.sharedMaterial&&r.sharedMaterial.shader.name.StartsWith("Nova/")).sharedMaterial.shader.name+" source=NovaShader/2.0.0/pfb_eff_fire");
 }
 void LateUpdate(){
  if(!motor||flames==null)return;
  bool hidden=motor.ExternalMotion||!motor.visual.gameObject.activeInHierarchy;
  tongue.enabled=!hidden;
  if(hidden){foreach(var ps in flames)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);return;}
  if(Vector3.Distance(lastPosition,motor.transform.position)>1.2f)foreach(var ps in flames)ps.Clear();lastPosition=motor.transform.position;
  transform.position=motor.visual.TransformPoint(new Vector3(-.33f,.87f,0))+new Vector3(0,0,.14f);
  effect.transform.rotation=Quaternion.identity;diagnostics++;tongue.transform.rotation=Quaternion.Euler(0,0,motor.Facing*62);tongue.transform.position=transform.position-tongue.transform.up*.22f;tongueMaterial.SetFloat("_BaseMapProgress",Mathf.Repeat(Time.time*.85f,1));
  foreach(var ps in flames){
   var render=ps.GetComponent<ParticleSystemRenderer>();if(render.enabled&&render.sharedMaterial.shader.name.StartsWith("Nova/")){render.sharedMaterial.SetFloat("_BaseMapRotation",0);render.alignment=ParticleSystemRenderSpace.Local;ps.transform.localRotation=Quaternion.Euler(0,0,motor.Facing*62);}
   var main=ps.main;main.startRotation3D=false;main.startRotation=0;var spin=ps.rotationOverLifetime;spin.enabled=false;main.startSpeed=0;
   var shape=ps.shape;shape.enabled=false;
   var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;velocity.x=0;velocity.y=.3f;velocity.z=0;
   var force=ps.forceOverLifetime;force.enabled=false;
   if(!ps.isPlaying)ps.Play();
  }
 }
 void OnDestroy(){if(tongueMaterial)Destroy(tongueMaterial);if(tongue){var mesh=tongue.GetComponent<MeshFilter>().sharedMesh;if(mesh)Destroy(mesh);}}
}




