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
        public static RoomFaces OpticalOf(Component item){
            if(!item)return RoomFaces.None;var player=item.GetComponentInParent<PlayerController>();
            if(player){var travel=player.GetComponent<LightTravel>();return travel&&travel.IsTravelling?travel.Route.Face:ForView(CameraViewController.Active?CameraViewController.Active.CurrentView:1);}
            var member=item.GetComponentInParent<FaceMembership>();return member?member.faces:RoomFaces.Front;
        }
    }
}
