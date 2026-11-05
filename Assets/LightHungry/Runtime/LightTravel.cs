using UnityEngine;
namespace LightHungry {
    public sealed class LightTravel : MonoBehaviour {
        public float speed=7,maxDistance=400;
        public int reflectionLimit=64;
        public Sprite[] photonFrames;
        public bool IsTravelling {get;private set;}
        public OpticalRoute Route {get;private set;}
        public string LastResult {get;private set;}
        public int Reflections {get;private set;}
        PlayerController player;Vector3 origin,position,direction;int originalView;float distance;
        Collider previous;Collider[] contacts;bool[] contactStates;GameObject orb;TrailRenderer trail;
        RigidbodyInterpolation savedInterpolation;
        float activeSpeed;
        public LightEmitter Source {get;private set;}
        public float TravelStrength {get;private set;}
        public Vector3 Direction=>direction;
        public int Takeovers {get;private set;}
        static Material trailMaterial;
        float blockedFlash;
        void Awake(){player=GetComponent<PlayerController>();}
        public bool Launch(LightLaunchPad pad){
            if(!pad||!pad.available||IsTravelling||!player.Body3D||!player.Grounded||Vector3.Distance(transform.position,pad.transform.position)>2.5f||CameraViewController.Active.IsRotating||GetComponent<PlayerCarry>().Held)return false;
            return Begin(pad.Origin,pad.Direction,speed,OpticalRoute.From(pad));
        }
        public bool LaunchFromLight(Vector3 start,Vector3 heading,float velocity){return LaunchFromLight(start,heading,velocity,new OpticalRoute{view=CameraViewController.Active?CameraViewController.Active.CurrentView:1});}
        public bool LaunchFromLight(Vector3 start,Vector3 heading,float velocity,OpticalRoute route,LightEmitter source=null){
            if(IsTravelling||!player.Body3D||!CameraViewController.Active||CameraViewController.Active.IsRotating||heading.sqrMagnitude<.5f)return false;
            bool launched=Begin(start,heading.normalized,velocity,route);if(launched){Source=source;TravelStrength=source?source.strength:1;}return launched;
        }
        bool Begin(Vector3 start,Vector3 heading,float velocity,OpticalRoute route){
            Route=route;Source=null;TravelStrength=1;Takeovers=0;
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
                bool found=SpaceOptics.Cast(position,direction,step,previous,player,true,Route,out var hit);
                float reachable=found?hit.distance:step;
                if(TryTakeover(position+direction*reachable,out float taken)){distance+=taken;step-=taken;continue;}
                if(!found){Move(position+direction*step);distance+=step;step=0;}
                else {
                    Move(hit.point);distance+=hit.distance;step-=hit.distance;
                    var surface=hit.collider.GetComponentInParent<CrossFaceSurface>();
                    if(surface){ArriveAtSurface(surface,hit.point);break;}
                    var lens=hit.collider.GetComponentInParent<LensSurface>();
                    if(lens){direction=lens.Refract(direction,Route).normalized;previous=hit.collider;Move(position+direction*.04f);distance+=.04f;step=Mathf.Max(0,step-.04f);if(++Reflections>reflectionLimit){Finish(false,null,"Reflection limit");break;}continue;}
                    var landing=hit.collider.GetComponentInParent<LightLanding>();
                    if(landing){Finish(landing.CanMaterialize(player),landing,"Landing blocked");break;}
                    var route=Route;bool wasTransit=route.inTransit;
                    if(!route.Reflect(hit.collider,direction,out var next,out int view)){StopAtObstacle();break;}
                    Route=route;
                    if(++Reflections>reflectionLimit){Finish(false,null,"Reflection limit");break;}
                    bool cross=wasTransit!=Route.inTransit;
                    direction=next;previous=hit.collider;Move(position+direction*.04f);distance+=.04f;
                    // Look ahead to a physical receiving mirror; never infer a face from +X/+Z alone.
                    if(Route.inTransit&&SpaceOptics.Cast(position,direction,maxDistance-distance,previous,player,true,Route,out var receiving)){
                        var targetSurface=receiving.collider.GetComponentInParent<CrossFaceSurface>();if(targetSurface)view=targetSurface.View;else{var arrival=Route;if(arrival.Reflect(receiving.collider,direction,out _,out int arrivingView)&&!arrival.inTransit)view=arrivingView;}
                    }
                    if(view>0)CameraViewController.Active.FollowTravelView(view);
                    if(cross&&trail)trail.Clear();
                    if(cross)GetComponent<ScreenBreakEffect>()?.Burst(position);
                }
                if(distance>maxDistance)Finish(false,null,"Distance limit");
            }
        }
        bool TryTakeover(Vector3 end,out float travelled){
            travelled=0;if(Route.inTransit||!LabSession.Instance||!LabSession.Instance.beams)return false;
            BeamPathSegment selected=default;float first=2;
            foreach(var segment in LabSession.Instance.beams.PathSegments){
                if(!segment.source||!segment.source.isActiveAndEnabled||!segment.source.active||segment.source.strength<=TravelStrength+.001f||segment.route.inTransit||segment.route.view!=Route.view||Vector3.Dot(segment.Direction,direction)>.999f)continue;
                if(segment.IntersectSweep(position,end,out float fraction)&&(fraction<first-.0001f||(Mathf.Abs(fraction-first)<.0001f&&(!selected.source||segment.source.strength>selected.source.strength)))){first=fraction;selected=segment;}
            }
            if(!selected.source)return false;Vector3 entry=Vector3.Lerp(position,end,first);travelled=Vector3.Distance(position,entry);Move(entry);direction=selected.Direction;Route=selected.route;Source=selected.source;TravelStrength=Source.strength;previous=null;Takeovers++;return true;
        }
        void ArriveAtSurface(CrossFaceSurface surface,Vector3 point){
            if(!surface.allowMaterialize){Finish(false,null,"Surface landing disabled");return;}
            var saved=Route;Route=new OpticalRoute{view=surface.View};
            Vector3 feet=point-Vector3.up*.95f;
            if(!FreeAt(feet)){Route=saved;LastResult="Surface landing blocked";blockedFlash=.18f;position-=direction*.05f;direction=-direction;previous=null;return;}
            Move(point);if(trail)trail.Clear();GetComponent<ScreenBreakEffect>()?.Burst(point);RestoreAt(feet,false,"Arrived at face surface");
        }
        void Move(Vector3 p){position=p;player.SetSpatialPosition(p-Vector3.up*.95f);if(orb)orb.transform.position=p;if(IsTravelling&&!Route.inTransit)foreach(var overlap in Physics.OverlapSphere(p,.15f,1<<13,QueryTriggerInteraction.Collide)){var exit=overlap.GetComponentInParent<LevelExit>();if(exit&&exit.allowPhoton)exit.TryEnter(player);}}
        bool FreeAt(Vector3 feet){foreach(var collider in Physics.OverlapBox(feet+Vector3.up*.72f,new Vector3(.39f,.69f,.34f),Quaternion.identity,player.groundMask,QueryTriggerInteraction.Ignore))if((FaceMembership.OpticalOf(collider)&Route.Face)!=0)return false;return true;}
        void StopAtObstacle(){
            Vector3 feet=position-Vector3.up*.95f;
            // Return only along the incoming segment far enough to fit the body beside the obstacle.
            for(int i=0;i<=25;i++){var candidate=feet-direction*(i*.08f);if(FreeAt(candidate)){RestoreAt(candidate,false,"Stopped at obstacle");return;}}
            // A narrow passage can be too small to restore a body. Remain a photon and permit retry/cancel.
            position-=direction*.05f;Move(position);blockedFlash=.18f;LastResult="No room to materialize";
            direction=-direction;previous=null;
        }
        int NearestView(){if(Route.view>0)return Route.view;int view=1;float best=360;for(int i=1;i<=4;i++){float d=Mathf.Abs(Mathf.DeltaAngle(CameraViewController.Active.VisualYaw,CameraViewController.Yaw(i)));if(d<best){best=d;view=i;}}return view;}
        void RestoreAt(Vector3 feet,bool jump,string result){
            IsTravelling=false;player.Body3D.isKinematic=false;player.SetSpatialPosition(feet);player.Body3D.interpolation=savedInterpolation;
            for(int i=0;i<contacts.Length;i++)if(contacts[i])contacts[i].enabled=contactStates[i];
            player.visual.enabled=true;CameraViewController.Active.EndTravelView(NearestView(),true);player.PrepareViewChange();player.ApplyViewPlane(player.ViewRight);player.Body3D.velocity=jump?Vector3.up*player.jumpSpeed:Vector3.zero;FindObjectOfType<FaceCollisionIsolation>()?.Refresh();
            if(orb)Destroy(orb);GetComponent<LightExposure>()?.Protect();LastResult=result;
        }
        void Finish(bool success,LightLanding landing,string failure){
            Debug.Log("Light travel result="+(success?"Arrived":failure)+" at="+position+" reflections="+Reflections);
            IsTravelling=false;player.Body3D.isKinematic=false;
            player.SetSpatialPosition(success?landing.Feet:origin);
            player.Body3D.interpolation=savedInterpolation;
            for(int i=0;i<contacts.Length;i++)if(contacts[i])contacts[i].enabled=contactStates[i];
            player.visual.enabled=true;CameraViewController.Active.EndTravelView(success?landing.view:originalView);player.ApplyViewPlane(player.ViewRight);FindObjectOfType<FaceCollisionIsolation>()?.Refresh();
            if(orb)Destroy(orb);LastResult=success?"Arrived":failure;
            GetComponent<LightExposure>()?.Protect();
        }
        public bool TryJumpOut(){
            if(!IsTravelling)return false;if(Route.inTransit){LastResult="Cannot materialize between faces";blockedFlash=.18f;return false;}Vector3 feet=position-Vector3.up*.95f;
            if(!FreeAt(feet)){LastResult="Exit blocked";blockedFlash=.18f;return false;}
            int view=1;float best=360;for(int i=1;i<=4;i++){float delta=Mathf.Abs(Mathf.DeltaAngle(CameraViewController.Active.VisualYaw,CameraViewController.Yaw(i)));if(delta<best){best=delta;view=i;}}
            RestoreAt(feet,true,"Jumped out");return true;
        }
        public void Cancel(){if(IsTravelling)Finish(false,null,"Cancelled");}
    }
}
