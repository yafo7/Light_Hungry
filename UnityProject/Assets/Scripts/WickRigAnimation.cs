using UnityEngine;
public class WickRigAnimation:MonoBehaviour {
 public Animator animator;PlayerMotor motor;
 void Start(){motor=GetComponentInParent<PlayerMotor>();}
 void Update(){if(!motor||!animator)return;animator.SetFloat("Move",motor.ExternalMotion?0:Mathf.Abs(motor.MoveInput),.12f,Time.deltaTime);animator.SetBool("Grounded",motor.Grounded);}
}
