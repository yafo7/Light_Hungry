using UnityEngine;
using UnityEngine.Events;
namespace LightHungry {
    public sealed class BeastReceiver:MonoBehaviour {
        public float requiredSeconds=3;public SpriteRenderer visual;public Sprite hungry,feeding,fed;
        public bool isGoal=true;
        public UnityEvent onSatisfied=new UnityEvent(),onCleared=new UnityEvent();
        public bool Lit {get;set;}
        public float Progress {get;private set;}
        public bool Satisfied=>Progress>=requiredSeconds;
        public void Tick(float dt){bool before=Satisfied;if(Lit)Progress=Mathf.Min(requiredSeconds,Progress+Mathf.Max(0,dt));if(visual)visual.sprite=Satisfied?fed:Lit?feeding:hungry;if(!before&&Satisfied)onSatisfied.Invoke();}
        public void Clear(){Progress=0;Lit=false;onCleared.Invoke();}
    }
}
