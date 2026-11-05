using UnityEngine;

// Fixed axle; only the wheel turns as the connected lifting rope travels.
public class DrainPulley : MonoBehaviour {
 public DrainMechanism mechanism;
 public float direction=1;
 void LateUpdate(){if(mechanism)transform.localRotation=Quaternion.Euler(0,0,direction*mechanism.height*210/14*Mathf.Rad2Deg);}
}
