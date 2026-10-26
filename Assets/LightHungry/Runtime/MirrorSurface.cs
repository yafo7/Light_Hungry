using UnityEngine;
namespace LightHungry {
    public sealed class MirrorSurface:Interactable {
        [Tooltip("Mirror tangent angles in degrees from +X. Reflection uses its perpendicular normal.")]
        public float[] angles={45,135};public int angleIndex;
        public Transform mirrorArt;public Collider2D face;
        public float[] sizes={.7f,1f,1.4f};public int sizeIndex=1;
        public Vector2 Normal=>new Vector2(-Mathf.Sin(Angle*Mathf.Deg2Rad),Mathf.Cos(Angle*Mathf.Deg2Rad));
        public float Angle=>angles[Mathf.Clamp(angleIndex,0,angles.Length-1)]+transform.eulerAngles.z;
        public override void Interact(PlayerController player){angleIndex=(angleIndex+1)%angles.Length;Apply();}
        public void Resize(){sizeIndex=(sizeIndex+1)%sizes.Length;Apply();}
        void OnEnable(){Apply();}
        public void Apply(){if(mirrorArt){mirrorArt.localRotation=Quaternion.Euler(0,0,angles[angleIndex]-45);mirrorArt.localScale=Vector3.one*sizes[sizeIndex];}if(face){face.transform.localRotation=Quaternion.Euler(0,0,angles[angleIndex]);face.transform.localScale=Vector3.one*sizes[sizeIndex];}}
        void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;Gizmos.DrawLine(transform.position,transform.position+(Vector3)Normal);}
    }
}
