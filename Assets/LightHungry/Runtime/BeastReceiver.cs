using UnityEngine;
namespace LightHungry {
    public sealed class BeastReceiver:MonoBehaviour {
        public float requiredSeconds=3;public SpriteRenderer visual;public Sprite hungry,feeding,fed;
        public bool isGoal=true;
        public bool Lit {get;set;}
        public float Progress {get;private set;}
        public bool Satisfied=>Progress>=requiredSeconds;
        public void Tick(float dt){if(Lit)Progress=Mathf.Min(requiredSeconds,Progress+dt);if(visual)visual.sprite=Satisfied?fed:Lit?feeding:hungry;}
        public void Clear(){Progress=0;Lit=false;}
    }
}
