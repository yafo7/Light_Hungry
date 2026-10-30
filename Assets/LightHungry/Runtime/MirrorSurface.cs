using UnityEngine;
namespace LightHungry {
    public sealed class MirrorSurface:Interactable {
        [Tooltip("Mirror tangent angles in degrees from +X. Reflection uses its perpendicular normal.")]
        public float[] angles={45,135};public int angleIndex;
        public Transform mirrorArt;public Collider2D face;
        public bool continuousRotation;
        public bool depthRotation;
        public float rotationSpeed=45,rotationAngle=45;
        [Tooltip("共享接收镜的优先面；不会代替真实命中与角度判定。0 按所属面自动选择。") ] public int destinationView;
        public Transform opticalPivot;
        public override bool Continuous=>continuousRotation;
        public Vector3 WorldNormal=>continuousRotation?transform.TransformDirection(depthRotation?Quaternion.Euler(0,rotationAngle,0)*Vector3.right:Quaternion.Euler(0,0,rotationAngle)*Vector3.up).normalized:face?face.transform.up.normalized:(Vector3)Normal;
        public float[] sizes={.7f,1f,1.4f};public int sizeIndex=1;
        public Vector2 Normal=>new Vector2(-Mathf.Sin(Angle*Mathf.Deg2Rad),Mathf.Cos(Angle*Mathf.Deg2Rad));
        public float Angle=>angles[Mathf.Clamp(angleIndex,0,angles.Length-1)]+transform.eulerAngles.z;
        public override void Interact(PlayerController player){if(continuousRotation)return;angleIndex=(angleIndex+1)%angles.Length;Apply();}
        public override void Hold(PlayerController player,float deltaTime){rotationAngle=Mathf.Repeat(rotationAngle+rotationSpeed*deltaTime,360);Apply();}
        public void Resize(){sizeIndex=(sizeIndex+1)%sizes.Length;Apply();}
        void OnEnable(){Apply();}
        public void Apply(){
            if(continuousRotation){
                Quaternion rotation=depthRotation?Quaternion.Euler(0,rotationAngle,0)*Quaternion.Euler(0,0,-90):Quaternion.Euler(0,0,rotationAngle);
                if(opticalPivot)opticalPivot.localRotation=rotation;
                if(mirrorArt)mirrorArt.localRotation=depthRotation?Quaternion.Euler(0,rotationAngle,0):Quaternion.Euler(0,0,rotationAngle);
                return;
            }
            if(mirrorArt){mirrorArt.localRotation=Quaternion.Euler(0,0,angles[angleIndex]-45);mirrorArt.localScale=Vector3.one*sizes[sizeIndex];}if(face){face.transform.localRotation=Quaternion.Euler(0,0,angles[angleIndex]);face.transform.localScale=Vector3.one*sizes[sizeIndex];}}
        void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;var center=opticalPivot?opticalPivot.position:transform.position;Gizmos.DrawLine(center,center+WorldNormal*1.5f);}
    }
}
