using UnityEngine;

namespace LightHungry {
    public sealed class SpaceMirror : Interactable {
        [Tooltip("Normals in the root's local coordinates. E cycles discrete orientations.")]
        public Vector3[] normals = { new Vector3(1,0,-1), new Vector3(1,0,1) };
        public int orientation;
        public bool adjustable = true;
        [Range(0,4), Tooltip("Shared receiving mirror preference only: 0 uses face membership. Actual reflection must enter that plane.")]
        public int travelView;
        public Transform face;
        public Vector3 Normal => transform.TransformDirection(normals != null && normals.Length > 0 ? normals[Mathf.Clamp(orientation,0,normals.Length-1)].normalized : Vector3.forward);
        public override void Interact(PlayerController player) {
            var carry = GetComponent<CarryableProp>();
            if(carry) { player.GetComponent<PlayerCarry>()?.Toggle(carry); return; }
            Cycle();
        }
        public void Cycle() { if(!adjustable || normals == null || normals.Length==0)return; orientation=(orientation+1)%normals.Length; Apply(); }
        void OnEnable(){Apply();}
        void OnValidate(){Apply();}
        public void Apply(){if(face)face.rotation=Quaternion.LookRotation(Normal,Mathf.Abs(Vector3.Dot(Normal,Vector3.up))>.98f?Vector3.forward:Vector3.up);}
        void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;Gizmos.DrawLine(transform.position,transform.position+Normal*1.5f);}
    }
}
