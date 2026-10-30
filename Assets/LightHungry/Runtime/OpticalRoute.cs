using UnityEngine;
namespace LightHungry {
    // Logical face ownership is independent of camera visibility and overlapping world coordinates.
    public struct OpticalRoute {
        public int view;
        public bool inTransit;
        public RoomFaces Face=>FaceMembership.ForView(view);
        public RoomFaces DisplayFaces=>inTransit?RoomFaces.All:Face;
        public static OpticalRoute From(Component owner)=>new OpticalRoute{view=Primary(FaceMembership.OpticalOf(owner))};
        public static int Primary(RoomFaces faces){for(int v=1;v<=4;v++)if((faces&FaceMembership.ForView(v))!=0)return v;return 1;}
        public static Vector3 Normal(int view)=>Quaternion.Euler(0,CameraViewController.Yaw(view),0)*Vector3.forward;
        public static bool InPlane(Vector3 direction,int view)=>Mathf.Abs(Vector3.Dot(direction.normalized,Normal(view)))<.01f;
        public static bool IsDepthMirror(Component item){var mirror=item.GetComponentInParent<MirrorSurface>();return (mirror&&mirror.depthRotation)||item.GetComponentInParent<SpaceMirror>();}
        public bool CanHit(Component item,Vector3 incoming){
            if(!item)return false;
            var mirror=item.GetComponentInParent<MirrorSurface>();
            if(mirror&&mirror.face){var link=item.GetComponent<SpatialColliderLink>();if(item!=mirror.face&&(!link||link.original!=mirror.face)&&(!mirror.opticalPivot||!item.transform.IsChildOf(mirror.opticalPivot)))return false;}
            if(inTransit)return IsDepthMirror(item)&&FaceMembership.OpticalOf(item)!=RoomFaces.None;
            if((FaceMembership.OpticalOf(item)&Face)==0)return false;
            return !mirror||mirror.depthRotation||InPlane(incoming,view);
        }
        public bool Reflect(Component item,Vector3 incoming,out Vector3 outgoing,out int enteredView){
            outgoing=incoming;enteredView=0;if(!CanHit(item,incoming))return false;
            var mirror=item.GetComponentInParent<MirrorSurface>();var spatial=item.GetComponentInParent<SpaceMirror>();
            if(!mirror&&!spatial)return false;
            outgoing=Vector3.Reflect(incoming,mirror?mirror.WorldNormal:spatial.Normal).normalized;
            if(!IsDepthMirror(item))return !inTransit&&InPlane(outgoing,view);
            if(!inTransit){if(!InPlane(outgoing,view))inTransit=true;return true;}
            // A receiving depth mirror must actually redirect the ray into its own plane.
            var membership=FaceMembership.OpticalOf(item);int hint=mirror?mirror.destinationView:spatial.travelView;
            int candidate=0;
            if(hint>=1&&hint<=4&&(membership&FaceMembership.ForView(hint))!=0&&InPlane(outgoing,hint))candidate=hint;
            if(candidate==0)for(int v=1;v<=4;v++)if((membership&FaceMembership.ForView(v))!=0&&InPlane(outgoing,v)){candidate=v;break;}
            if(candidate!=0){view=candidate;inTransit=false;enteredView=view;}
            return true;
        }
    }
}
