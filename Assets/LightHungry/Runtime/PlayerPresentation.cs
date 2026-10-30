using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(650)]
    public sealed class PlayerPresentation:MonoBehaviour {
        public Sprite[] idle,walk,jump,push,pull,dissolve;
        public Sprite lookUp,lookDown,interact;
        public float height=1.55f,opacity=.8f;
        PlayerController player;
        void Awake(){player=GetComponent<PlayerController>();}
        void LateUpdate(){
            if(!player.visual||!player.visual.enabled)return;
            Sprite sprite=null;var exposure=GetComponent<LightExposure>();
            if(exposure&&exposure.Progress>.25f)sprite=At(dissolve,Mathf.Clamp((int)(exposure.Progress/exposure.secondsRequired*dissolve.Length),0,dissolve.Length-1));
            else if(player.Holding)sprite=player.Holding is PulleyRope?Loop(pull,7):interact;
            else if(Time.time-player.LastInteractionTime<.3f)sprite=interact;
            else if(!player.Grounded)sprite=At(jump,player.MotionVelocity.y>3?1:player.MotionVelocity.y< -3?3:2);
            else if(player.IsPushing)sprite=Loop(push,8);
            else if(Mathf.Abs(player.MotionVelocity.x)>.15f)sprite=Loop(walk,10);
            else if(Input.GetKey(KeyCode.W))sprite=lookUp;
            else if(Input.GetKey(KeyCode.S))sprite=lookDown;
            else sprite=Loop(idle,4);
            if(sprite){player.visual.sprite=sprite;var reference=At(idle,0);player.visual.transform.localScale=Vector3.one*(height/Mathf.Max(.01f,reference?reference.bounds.size.y:sprite.bounds.size.y));}
            var color=player.visual.color;color.a=opacity;player.visual.color=color;
        }
        static Sprite At(Sprite[] a,int i)=>a!=null&&a.Length>0?a[Mathf.Clamp(i,0,a.Length-1)]:null;
        static Sprite Loop(Sprite[] a,float fps)=>a!=null&&a.Length>0?a[(int)(Time.time*fps)%a.Length]:null;
    }
}
