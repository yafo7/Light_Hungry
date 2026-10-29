using UnityEngine;
namespace LightHungry {
    [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour {
        public enum AbilityMode { Normal, Reflect, Mouth, Release }
        [Header("Movement")] public float speed=5.5f,jumpSpeed=12f,acceleration=45f;
        public LayerMask groundMask;
        [Header("Unlocks")] public bool reflectUnlocked=true,swallowUnlocked=true;
        [Header("Light")] public float releaseDuration=4f; public bool stored;
        public AbilityMode Mode { get; private set; }
        public Vector2 Aim {get;private set;}=Vector2.right;
        public bool Grounded {get;private set;}
        public SpriteRenderer visual;
        public Sprite[] idle,walk,jump,push,swallow,reflect,charged,release;
        [HideInInspector] public bool automationInput; [HideInInspector] public float automationHorizontal;
        [HideInInspector] public bool automationJump;
        public Interactable Nearest {get;private set;}
        public Rigidbody2D Body {get;private set;}
        public Rigidbody Body3D {get;private set;}
        public Vector3 ViewRight=>CameraViewController.Active?CameraViewController.Active.ViewRight:Vector3.right;
        public Vector3 WorldAim=>ViewRight*Aim.x+Vector3.up*Aim.y;
        public Vector2 MotionVelocity=>Body3D?new Vector2(Vector3.Dot(Body3D.velocity,ViewRight),Body3D.velocity.y):Body.velocity;
        public float ReleaseRemaining=>Mathf.Max(0,releaseEnd-Time.time);
        float horizontal,coyote,buffer,releaseEnd; int facing=1; PushableBox grabbed;
        LightEmitter emitter; LineRenderer arrow;
        public Collider2D OwnCollider {get;private set;}
        void Awake(){
            Body=GetComponent<Rigidbody2D>();Body3D=SpatialLabWorld.Active?GetComponentInParent<Rigidbody>():null;OwnCollider=GetComponent<Collider2D>();
            if(Body3D)ApplyViewPlane(ViewRight);
            var mouth=new GameObject("Released light");mouth.transform.SetParent(transform,false);
            emitter=mouth.AddComponent<LightEmitter>();emitter.owner=OwnCollider;emitter.active=false;
            var aim=new GameObject("Aim indicator");aim.transform.SetParent(transform,false);arrow=aim.AddComponent<LineRenderer>();
            BeamSystem.ConfigureLine(arrow,.035f,new Color(.5f,.85f,1f));arrow.positionCount=2;
        }
        void Update(){
            if(LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()&&LabSession.Instance.GetComponent<LightRouteReview>().IsPlaying)return;
            if(GetComponent<LightTravel>()&&GetComponent<LightTravel>().IsTravelling)return;
            Grounded=Body3D?Physics.CheckBox(transform.position+Vector3.up*.035f,new Vector3(.325f,.06f,.26f),Quaternion.identity,groundMask,QueryTriggerInteraction.Ignore):Physics2D.OverlapBox((Vector2)transform.position+Vector2.up*.035f,new Vector2(.65f,.12f),0,groundMask);
            if((CameraViewController.Active&&CameraViewController.Active.IsRotating)||(LabSession.Instance&&LabSession.Instance.paused)){horizontal=0;buffer=0;return;}
            if(Grounded)coyote=.10f;else coyote-=Time.deltaTime;
            horizontal=automationInput?automationHorizontal:Input.GetAxisRaw("Horizontal");
            if(automationInput?automationJump:Input.GetKeyDown(KeyCode.Space)){buffer=.10f;automationJump=false;}else buffer-=Time.deltaTime;
            if(Mode!=AbilityMode.Release){
                if(Mathf.Abs(horizontal)>.1f)facing=horizontal>0?1:-1;
                if(!automationInput)Aim=Input.GetKey(KeyCode.W)?Vector2.up:Input.GetKey(KeyCode.S)?Vector2.down:new Vector2(facing,0);
                if(!automationInput&&Input.GetKeyDown(KeyCode.J))ToggleReflection();
            }
            if(!automationInput&&Input.GetKeyDown(KeyCode.K))ToggleSwallow();
            if(Mode==AbilityMode.Release&&Time.time>=releaseEnd)EndRelease();
            emitter.active=Mode==AbilityMode.Release;emitter.direction=Aim;
            emitter.transform.position=Body3D?transform.position+Vector3.up*.95f+WorldAim*.65f:(Vector3)((Vector2)transform.position+Vector2.up*.95f+Aim*.65f);
            Nearest=FindNearest();
            if(!automationInput){
                if(Input.GetKeyDown(KeyCode.E)&&GetComponent<PlayerCarry>()&&GetComponent<PlayerCarry>().Held){GetComponent<PlayerCarry>().Drop();return;}
                if(Input.GetKeyDown(KeyCode.Q)&&Nearest is MirrorSurface mirror)mirror.Resize();
                if(Input.GetKeyDown(KeyCode.E)&&Nearest){if(Nearest is PushableBox box)grabbed=box;else Nearest.Interact(this);}
                if(!Input.GetKey(KeyCode.E))grabbed=null;
            }
            if(grabbed&&Vector2.Distance(transform.position,grabbed.transform.position)>2.2f)grabbed=null;
            if(grabbed&&Mode!=AbilityMode.Release)grabbed.Drive(horizontal*speed*.65f);
            if(visual){visual.flipX=facing<0; Animate();}
            arrow.enabled=Mode==AbilityMode.Reflect||Mode==AbilityMode.Release||Mode==AbilityMode.Mouth;
            var from=transform.position+Vector3.up*1.25f;arrow.SetPosition(0,from);arrow.SetPosition(1,from+(Body3D?WorldAim:(Vector3)Aim)*.6f);
        }
        void FixedUpdate(){
            if(GetComponent<LightTravel>()&&GetComponent<LightTravel>().IsTravelling)return;
            bool immobile=Mode==AbilityMode.Release;
            if(Body3D){
                var velocity=MotionVelocity;
                float vx=Mathf.MoveTowards(velocity.x,immobile?0:horizontal*speed*(grabbed ? .65f : 1f),acceleration*Time.fixedDeltaTime);
                float vy=velocity.y;
                if(!immobile&&!grabbed&&buffer>0&&coyote>0){vy=jumpSpeed;buffer=0;coyote=0;}
                if(!automationInput&&!Input.GetKey(KeyCode.Space)&&vy>0)vy-=22*Time.fixedDeltaTime;
                Body3D.velocity=ViewRight*vx+Vector3.up*vy;
                Body3D.AddForce(Physics.gravity*(Body.gravityScale-1),ForceMode.Acceleration);
                return;
            }
            Body.velocity=new Vector2(Mathf.MoveTowards(Body.velocity.x,immobile?0:horizontal*speed*(grabbed ? .65f : 1f),acceleration*Time.fixedDeltaTime),Body.velocity.y);
            if(!immobile&&!grabbed&&buffer>0&&coyote>0){Body.velocity=new Vector2(Body.velocity.x,jumpSpeed);buffer=0;coyote=0;}
            if(!automationInput&&!Input.GetKey(KeyCode.Space)&&Body.velocity.y>0)Body.velocity+=Vector2.down*22*Time.fixedDeltaTime;
        }
        public void PrepareViewChange(){horizontal=0;buffer=0;coyote=0;grabbed=null;if(Body3D){Body3D.velocity=Vector3.zero;Body3D.angularVelocity=Vector3.zero;}}
        public void SetSpatialPosition(Vector3 position){
            if(!Body3D)return;
            Body3D.transform.position=position;
            Body3D.position=position;
            if(!Body3D.isKinematic)Body3D.velocity=Vector3.zero;
            transform.localPosition=Vector3.zero;
            Physics.SyncTransforms();
        }
        public void ApplyViewPlane(Vector3 right){
            if(!Body3D)return;
            Body3D.velocity=Vector3.zero;
            Body3D.constraints=RigidbodyConstraints.FreezeRotation|(Mathf.Abs(right.x)>.5f?RigidbodyConstraints.FreezePositionZ:RigidbodyConstraints.FreezePositionX);
        }
        public void SetAim(Vector2 direction){if(Mode!=AbilityMode.Release)Aim=direction.normalized;}
        public void ToggleReflection(){if(!reflectUnlocked||Mode==AbilityMode.Release)return;Mode=Mode==AbilityMode.Reflect?AbilityMode.Normal:AbilityMode.Reflect;}
        public void ToggleSwallow(){
            if(!swallowUnlocked)return;
            if(Mode==AbilityMode.Release){EndRelease();return;}
            if(stored){stored=false;Mode=AbilityMode.Release;releaseEnd=Time.time+releaseDuration;grabbed=null;}
            else Mode=Mode==AbilityMode.Mouth?AbilityMode.Normal:AbilityMode.Mouth;
        }
        public bool Absorb(){if(Mode!=AbilityMode.Mouth||stored||!swallowUnlocked)return false;stored=true;Mode=AbilityMode.Normal;return true;}
        public void EndRelease(){Mode=AbilityMode.Normal;if(emitter)emitter.active=false;}
        Interactable FindNearest(){
            Interactable result=null;float best=1.8f;
            if(Body3D){
                Vector3 origin=transform.position+Vector3.up*.6f;
                foreach(var c in Physics.OverlapSphere(origin,best,~0,QueryTriggerInteraction.Collide)){
                    var target=c.GetComponentInParent<Interactable>();if(!target)continue;
                    Vector3 point=c.ClosestPoint(origin);float distance=Vector3.Distance(origin,point);
                    if(distance>=best)continue;
                    if(distance>.01f&&Physics.Raycast(origin,(point-origin)/distance,out var hit,distance,groundMask,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<Interactable>()!=target)continue;
                    result=target;best=distance;
                }
                return result;
            }
            foreach(var c in Physics2D.OverlapCircleAll((Vector2)transform.position+Vector2.up*.6f,1.8f)){
                var target=c.GetComponentInParent<Interactable>();if(!target)continue;
                float d=Vector2.Distance((Vector2)transform.position+Vector2.up*.6f,c.ClosestPoint((Vector2)transform.position+Vector2.up*.6f));
                if(d<best){result=target;best=d;}
            }return result;
        }
        void Animate(){
            Sprite[] frames=idle;float fps=5;int index=-1;
            if(Mode==AbilityMode.Reflect){frames=reflect;index=1;}
            else if(Mode==AbilityMode.Mouth){frames=swallow;index=1;}
            else if(Mode==AbilityMode.Release){frames=release;index=0;}
            else if(!Grounded){frames=jump;index=MotionVelocity.y>2?2:MotionVelocity.y< -2?4:3;}
            else if(Mathf.Abs(MotionVelocity.x)>.15f){frames=grabbed?push:walk;fps=grabbed?8:10;}
            else if(stored){frames=charged;fps=4;}
            if(frames!=null&&frames.Length>0)visual.sprite=frames[index<0?(int)(Time.time*fps)%frames.Length:Mathf.Min(index,frames.Length-1)];
            visual.color=stored?new Color(1f,.96f,.8f):Color.white;
        }
        void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;Gizmos.DrawWireCube(transform.position+Vector3.up*.72f,new Vector3(.8f,1.4f,0));}
    }
}
