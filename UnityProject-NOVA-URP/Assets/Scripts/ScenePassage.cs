using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePassage : MonoBehaviour {
 public string destination="ThirdDrain";public bool Activated {get;private set;}
 void OnTriggerEnter2D(Collider2D other){Touch(other);}void OnTriggerStay2D(Collider2D other){Touch(other);}
 void Touch(Collider2D other){if(Activated||!other.GetComponentInParent<PlayerMotor>())return;Activated=true;SceneFade.Load(destination);}
}
public class SceneFade : MonoBehaviour {
 public static bool Busy {get;private set;}float alpha;static SceneFade instance;
 public static void Load(string target){if(Busy)return;if(!Application.CanStreamedLevelBeLoaded(target)){Debug.LogError("Scene not in build: "+target);return;}var go=new GameObject("SceneTransition");instance=go.AddComponent<SceneFade>();DontDestroyOnLoad(go);Busy=true;instance.StartCoroutine(instance.Transition(target));}
 IEnumerator Transition(string target){while(alpha<1){alpha+=Time.unscaledDeltaTime*3.5f;yield return null;}var op=SceneManager.LoadSceneAsync(target);while(!op.isDone)yield return null;yield return null;while(alpha>0){alpha-=Time.unscaledDeltaTime*3;yield return null;}Busy=false;Destroy(gameObject);}
 void OnGUI(){GUI.depth=-1000;var c=GUI.color;GUI.color=new Color(.015f,.022f,.018f,Mathf.Clamp01(alpha));GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=c;}
}
