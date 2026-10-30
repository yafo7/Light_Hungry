using System;
using UnityEngine;

namespace LightHungry {
    [Flags] public enum RoomFaces { None=0, Front=1, Back=2, Left=4, Right=8, All=15 }

    [DisallowMultipleComponent]
    public sealed class FaceMembership : MonoBehaviour {
        public RoomFaces faces=RoomFaces.Front;
        public static RoomFaces ForView(int view)=> (RoomFaces)(1 << (Mathf.Clamp(view,1,4)-1));
        public static RoomFaces Of(Component item) {
            if(!item)return RoomFaces.None;
            // The player and a held prop remain visible while the camera changes sides.
            if(item.GetComponentInParent<PlayerController>())return RoomFaces.All;
            var member=item.GetComponentInParent<FaceMembership>();
            return member?member.faces:RoomFaces.Front;
        }
        public static RoomFaces AfterReflection(Vector3 before,Vector3 after,RoomFaces current,int explicitView=0) {
            if(explicitView>0)return ForView(explicitView);
            if(Mathf.Abs(after.z)>.5f&&Mathf.Abs(before.z)<.5f)return after.z>0?RoomFaces.Right:RoomFaces.Left;
            if(Mathf.Abs(after.x)>.5f&&Mathf.Abs(before.x)<.5f)return RoomFaces.Front;
            return current;
        }
    }
}
