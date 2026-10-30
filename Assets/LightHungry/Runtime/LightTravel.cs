using UnityEngine;
namespace LightHungry {
    public sealed class LightTravel : MonoBehaviour {
        public float speed=7,maxDistance=80;
        public int reflectionLimit=12;
        public Sprite[] photonFrames;
        public bool IsTravelling {get;private set;}
        public string LastResult {get;private set;}
        public int Reflections {get;private set;}
        PlayerController player;Vector3 origin,position,direction;int originalView;float distance;
        Collider previous;Collider[] contacts;bool[] contactStates;GameObject orb;TrailRenderer trail;
        RigidbodyInterpolation savedInterpolation;
        float activeSpeed;
        static Material trailMaterial;
        float blockedFlash;
        void Awake(){player=GetComponent<PlayerController>();}
        public bool Launch(LightLaunchPad pad){
            if(!pad||!pad.available||IsTravelling||!player.Body3D||!player.Grounded||Vector3.Distance(transform.position,pad.transform.position)>2.5f||CameraViewController.Active.IsRotating||GetComponent<PlayerCarry>().Held)return false;
            return Begin(pad.Origin,pad.Direction,speed);
        }
        public bool LaunchFromLight(Vector3 start,Vector3 heading,float velocity){
            if(IsTravelling||!player.Body3D||!CameraViewController.Active||CameraViewController.Active.IsRotating||heading.sqrMagnitude<.5f)return false;
            return Begin(start,heading.normalized,velocity);
        }
        bool Begin(Vector3 start,Vector3 heading,float velocity){
            origin=player.Body3D.position;originalView=CameraViewController.Active.CurrentView;activeSpeed=velocity;
            player.PrepareViewChange();player.EndRelease();savedInterpolation=player.Body3D.interpolation;player.Body3D.interpolation=RigidbodyInterpolation.None;player.Body3D.isKinematic=true;
            contacts=player.Body3D.GetComponentsInChildren<Collider>();contactStates=new bool[contacts.Length];for(int i=0;i<contacts.Length;i++){contactStates[i]=contacts[i].enabled;contacts[i].enabled=false;}
            player.visual.enabled=false;IsTravelling=true;Reflections=0;distance=0;previous=null;position=start;direction=heading;LastResult="Travelling";
            // A departure receiver must not immediately re-capture the new photon.
            foreach(var landing in FindObjectsOfType<LightLanding>())if(Vector3.Distance(landing.Feet,origin)<1.2f){previous=landing.GetComponent<Collider>();break;}
            orb=new GameObject("Travelling light");var orbSprite=orb.AddComponent<SpriteRenderer>();orbSprite.sprite=photonFrames!=null&&photonFrames.Length>0?photonFrames[0]:player.visual.sprite;orbSprite.sortingOrder=20;orb.AddComponent<ViewFacingSprite>();orb.transform.localScale=Vector3.one*.55f;
            orb.AddComponent<FaceMembership>().faces=RoomFaces.All;
            orb.GetComponent<Renderer>().sharedMaterial=SpatialLabWorld.Active.actorMaterial;
            if(!trailMaterial)trailMaterial=new Material(Shader.Find("Sprites/Default"));
            trail=orb.AddComponent<TrailRenderer>();trail.sharedMaterial=trailMaterial;trail.startWidth=.2f;trail.endWidth=0;trail.time=.35f;trail.startColor=new Color(.85f,.93f,1f,.6f);trail.endColor=new Color(.8f,.9f,1f,0);
            var particles=orb.AddComponent<ParticleSystem>();var main=particles.main;main.startLifetime=.4f;main.startSpeed=.1f;main.startSize=.035f;main.startColor=new Color(.9f,.95f,1f,.4f);main.simulationSpace=ParticleSystemSimulationSpace.World;var emission=particles.emission;emission.rateOverTime=8;var shape=particles.shape;shape.radius=.06f;particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=trailMaterial;
            Move(position);return true;
        }
        void Update(){
            if(!IsTravelling)return;
            if(LabSession.Instance&&LabSession.Instance.paused)return;
            if(orb){blockedFlash=Mathf.Max(0,blockedFlash-Time.deltaTime);orb.transform.localScale=Vector3.one*(blockedFlash>0?1.4f:1.15f);var sprite=orb.GetComponent<SpriteRenderer>();if(photonFrames!=null&&photonFrames.Length>0)sprite.sprite=photonFrames[(int)(Time.time*10)%photonFrames.Length];sprite.color=blockedFlash>0?new Color(1,.65f,.65f):new Color(.7f,.84f,.95f);}
            if(Input.GetKeyDown(KeyCode.Backspace)){Finish(false,null,"Cancelled");return;}
            if(Input.GetKeyDown(KeyCode.Space)&&TryJumpOut())return;
            float step=activeSpeed*Time.deltaTime;
            for(int i=0;i<8&&step>0&&IsTravelling;i++){
                if(!SpaceOptics.Cast(position,direction,step,previous,player,true,out var hit)){Move(position+direction*step);distance+=step;step=0;}
                else {
                    Move(hit.point);distance+=hit.distance;step-=hit.distance;
                    var landing=hit.collider.GetComponentInParent<LightLanding>();
                    if(landing){Finish(landing.CanMaterialize(player),landing,"Landing blocked");break;}
                    if(!SpaceOptics.Reflect(hit.collider,direction,out var next,out int view)){StopAtObstacle();break;}
                    if(++Reflections>reflectionLimit){Finish(false,null,"Reflection limit");break;}
                    bool cross=Mathf.Abs(Vector3.Dot(next,Camera.main.transform.forward))>.5f;
                    direction=next;previous=hit.collider;Move(position+direction*.04f);distance+=.04f;
                    if(view==0&&cross)view=Mathf.Abs(direction.z)>.5f?(direction.z>0?4:3):Mathf.Abs(direction.x)>.5f?(direction.x>0?1:2):0;
                    if(view!=0)CameraViewController.Active.FollowTravelView(view);
                    if(cross&&trail)trail.Clear();
                    if(cross)GetComponent<ScreenBreakEffect>()?.Burst(position);
                }
                if(distance>maxDistance)Finish(false,null,"Distance limit");
            }
        }
        void Move(Vector3 p){position=p;player.SetSpatialPosition(p-Vector3.up*.95f);if(orb)orb.transform.position=p;}
        bool FreeAt(Vector3 feet)=>!Physics.CheckBox(feet+Vector3.up*.72f,new Vector3(.39f,.69f,.34f),Quaternion.identity,player.groundMask,QueryTriggerInteraction.Ignore);
        void StopAtObstacle(){
            Vector3 feet=position-Vector3.up*.95f;
            // Return only along the incoming segment far enough to fit the body beside the obstacle.
            for(int i=0;i<=25;i++){var candidate=feet-direction*(i*.08f);if(FreeAt(candidate)){RestoreAt(candidate,false,"Stopped at obstacle");return;}}
            // A narrow passage can be too small to restore a body. Remain a photon and permit retry/cancel.
            position-=direction*.05f;Move(position);blockedFlash=.18f;LastResult="No room to materialize";
            direction=-direction;previous=null;
        }
        int NearestView(){int view=1;float best=360;for(int i=1;i<=4;i++){float d=Mathf.Abs(Mathf.DeltaAngle(CameraViewController.Active.VisualYaw,CameraViewController.Yaw(i)));if(d<best){best=d;view=i;}}return view;}
        void RestoreAt(Vector3 feet,bool jump,string result){
            IsTravelling=false;player.Body3D.isKinematic=false;player.SetSpatialPosition(feet);player.Body3D.interpolation=savedInterpolation;
            for(int i=0;i<contacts.Length;i++)if(contacts[i])contacts[i].enabled=contactStates[i];
            player.visual.enabled=true;CameraViewController.Active.EndTravelView(NearestView(),true);player.PrepareViewChange();player.ApplyViewPlane(player.ViewRight);player.Body3D.velocity=jump?Vector3.up*player.jumpSpeed:Vector3.zero;
            if(orb)Destroy(orb);GetComponent<LightExposure>()?.Protect();LastResult=result;
        }
        void Finish(bool success,LightLanding landing,string failure){
            Debug.Log("Light travel result="+(success?"Arrived":failure)+" at="+position+" reflections="+Reflections);
            IsTravelling=false;player.Body3D.isKinematic=false;
            player.SetSpatialPosition(success?landing.Feet:origin);
            player.Body3D.interpolation=savedInterpolation;
            for(int i=0;i<contacts.Length;i++)if(contacts[i])contacts[i].enabled=contactStates[i];
            player.visual.enabled=true;CameraViewController.Active.EndTravelView(success?landing.view:originalView);player.ApplyViewPlane(player.ViewRight);
            if(orb)Destroy(orb);LastResult=success?"Arrived":failure;
            GetComponent<LightExposure>()?.Protect();
        }
        public bool TryJumpOut(){
            if(!IsTravelling)return false;Vector3 feet=position-Vector3.up*.95f;
            if(!FreeAt(feet)){LastResult="Exit blocked";blockedFlash=.18f;return false;}
            int view=1;float best=360;for(int i=1;i<=4;i++){float delta=Mathf.Abs(Mathf.DeltaAngle(CameraViewController.Active.VisualYaw,CameraViewController.Yaw(i)));if(delta<best){best=delta;view=i;}}
            RestoreAt(feet,true,"Jumped out");return true;
        }
        public void Cancel(){if(IsTravelling)Finish(false,null,"Cancelled");}
    }
}
