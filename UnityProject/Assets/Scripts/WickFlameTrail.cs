using UnityEngine;
[DefaultExecutionOrder(100)]
public class WickFlameTrail:MonoBehaviour {
 PlayerMotor motor;ParticleSystem flames;float emission;Vector3 lastPosition;Material material;
 public void Initialize(PlayerMotor owner){
  motor=owner;var go=new GameObject("Head_FlameWisps");go.transform.SetParent(transform,false);flames=go.AddComponent<ParticleSystem>();flames.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=flames.main;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=64;main.startLifetime=.7f;main.startSpeed=0;main.startSize3D=true;main.startSizeX=.1f;main.startSizeY=.22f;main.startSizeZ=.1f;main.startColor=new Color(1,1,1,.6f);main.playOnAwake=false;
  var emit=flames.emission;emit.enabled=false;var shape=flames.shape;shape.enabled=false;
  var size=flames.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.2f,1),new Keyframe(1,0)));
  var color=flames.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(1,.84f,.52f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.65f,.18f),new GradientAlphaKey(0,1)});color.color=gradient;
  var render=flames.GetComponent<ParticleSystemRenderer>();material=new Material(Resources.Load<Material>("SpiritFlame"));material.SetColor("_Color",Color.white);render.sharedMaterial=material;render.renderMode=ParticleSystemRenderMode.Billboard;flames.Play();lastPosition=motor.transform.position;
 }
 void LateUpdate(){
  if(!motor||!flames)return;if(Vector3.Distance(lastPosition,motor.transform.position)>1.2f)flames.Clear();lastPosition=motor.transform.position;
  if(motor.ExternalMotion||!motor.visual.gameObject.activeInHierarchy){flames.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);emission=0;return;}
  if(!flames.isPlaying)flames.Play();float facing=motor.Facing;
  transform.position=motor.visual.TransformPoint(new Vector3(-.25f,.83f,0));
  emission+=Time.deltaTime*(30+Mathf.Abs(motor.MoveInput)*8);
  while(emission>=1){emission--;var p=new ParticleSystem.EmitParams();p.position=transform.position+new Vector3(Random.Range(-.055f,.055f),Random.Range(-.04f,.075f),.12f);p.velocity=new Vector3(-facing*Random.Range(.14f,.36f),Random.Range(.20f,.44f),0);p.startLifetime=Random.Range(.5f,.85f);p.startSize3D=new Vector3(Random.Range(.12f,.20f),Random.Range(.20f,.34f),.1f);p.startColor=new Color(1,1,1,1);flames.Emit(p,1);}
 }
 void OnDestroy(){if(material)Destroy(material);}
}
