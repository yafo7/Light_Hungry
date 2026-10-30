using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightHungry {
    [DefaultExecutionOrder(1000)]
    public sealed class LabSession:MonoBehaviour {
        public static LabSession Instance {get;private set;}
        public PlayerController player;public BeastReceiver goal;public BeamSystem beams;
        public bool startWithReflection=true,startWithSwallow=true;
        public bool paused;bool restarting;float look;
        public bool Completed {get;private set;}
        public event System.Action LevelCompleted;
        public bool CompletionNoticeVisible=>GetComponent<CompletionNotice>()&&GetComponent<CompletionNotice>().IsVisible;
        void LateUpdate(){
            if(Completed||restarting)return;
            bool satisfied=false;
            if(goal)satisfied=goal.isGoal&&goal.Satisfied;
            else{
                int targets=0;satisfied=true;
                foreach(var beast in FindObjectsOfType<BeastReceiver>())if(beast.isGoal){targets++;satisfied&=beast.Satisfied;}
                satisfied&=targets>0;
            }
            if(!satisfied)return;
            Completed=true;var notice=GetComponent<CompletionNotice>();if(!notice)notice=gameObject.AddComponent<CompletionNotice>();notice.Show();LevelCompleted?.Invoke();
        }
        void Awake(){Instance=this;Time.timeScale=1;Application.targetFrameRate=60;}
        void Start(){ExposureJourneySmokeTest.ConfigureLegacy(this);if(player){player.reflectUnlocked=startWithReflection;player.swallowUnlocked=startWithSwallow;}gameObject.AddComponent<LabSmokeTest>();gameObject.AddComponent<CameraViewSmokeTest>();gameObject.AddComponent<LightTravelSmokeTest>();gameObject.AddComponent<ExposureJourneySmokeTest>();gameObject.AddComponent<FaceIsolationSmokeTest>();gameObject.AddComponent<WatercolorSmokeTest>();}
        void Update(){
            if(Input.GetKeyDown(KeyCode.R))Restart();
            if(GetComponent<LightRouteReview>()&&GetComponent<LightRouteReview>().IsPlaying)return;
            if(Input.GetKeyDown(KeyCode.Escape))SetPaused(!paused);
            if(Input.GetKeyDown(KeyCode.F1)){player.reflectUnlocked=!player.reflectUnlocked;if(!player.reflectUnlocked&&player.Mode==PlayerController.AbilityMode.Reflect)player.EndRelease();}
            if(Input.GetKeyDown(KeyCode.F2)){player.swallowUnlocked=!player.swallowUnlocked;if(!player.swallowUnlocked)player.EndRelease();}
            if(player&&player.transform.position.y< -10)Restart();
            if(Camera.main&&!CameraViewController.Active){float target=Input.GetKey(KeyCode.W)?1.1f:Input.GetKey(KeyCode.S)?-1.1f:0;look=Mathf.Lerp(look,target,Time.unscaledDeltaTime*4);Camera.main.transform.position=new Vector3(0,look,-10);}
        }
        public void SetPaused(bool value){paused=value;Time.timeScale=paused||(CameraViewController.Active&&CameraViewController.Active.IsRotating)?0:1;}
        public void Restart(){if(restarting)return;restarting=true;Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
    }
}
