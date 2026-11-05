using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
namespace LightHungry {
    [RequireComponent(typeof(BoxCollider),typeof(FaceMembership))]
    public sealed class LevelExit : MonoBehaviour {
        public BeastReceiver[] requiredBeasts=new BeastReceiver[0];
        public bool allowPhoton;
        [Tooltip("留空仅完成本关；填写已加入 Build Settings 的 Scene 名称才切关。")]
        public string nextScene;
        public UnityEvent onEntered=new UnityEvent();
        public bool Entered {get;private set;}
        public bool Ready{get{foreach(var beast in requiredBeasts)if(!beast||!beast.Satisfied)return false;return true;}}
        public bool TryEnter(PlayerController player){
            if(!player||Entered||!Ready||!LabSession.Instance)return false;
            var travel=player.GetComponent<LightTravel>();if(travel&&travel.IsTravelling&&(!allowPhoton||travel.Route.inTransit))return false;
            if(Vector3.Distance(GetComponent<BoxCollider>().ClosestPoint(player.transform.position+Vector3.up*.75f),player.transform.position+Vector3.up*.75f)>.45f)return false;
            if((FaceMembership.OpticalOf(player)&FaceMembership.OpticalOf(this))==0)return false;
            Entered=true;onEntered.Invoke();LabSession.Instance.CompleteLevel();
            if(!string.IsNullOrWhiteSpace(nextScene)){if(Application.CanStreamedLevelBeLoaded(nextScene))SceneManager.LoadScene(nextScene);else Debug.LogError("出口下一关未加入构建列表："+nextScene,this);}
            return true;
        }
        void OnTriggerEnter(Collider other){TryEnter(other.GetComponentInParent<PlayerController>());}
        void OnTriggerStay(Collider other){TryEnter(other.GetComponentInParent<PlayerController>());}
        void OnDrawGizmosSelected(){var box=GetComponent<BoxCollider>();var old=Gizmos.matrix;Gizmos.matrix=transform.localToWorldMatrix;Gizmos.color=Color.green;Gizmos.DrawWireCube(box.center,box.size);Gizmos.matrix=old;}
    }
}
