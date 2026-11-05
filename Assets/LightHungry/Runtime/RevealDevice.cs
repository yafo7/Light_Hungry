using UnityEngine;
namespace LightHungry {
    // Keep the logical object alive; only its authored presentation and collision are revealed.
    public sealed class RevealDevice : MonoBehaviour {
        public Transform target;
        public bool state;
        Renderer[] renderers;Collider[] solids;Collider2D[] flatSolids;bool[] r,c,c2;
        void Awake(){Cache();Apply();}
        void Cache(){if(renderers!=null)return;var root=target?target:transform;renderers=root.GetComponentsInChildren<Renderer>(true);solids=root.GetComponentsInChildren<Collider>(true);flatSolids=root.GetComponentsInChildren<Collider2D>(true);r=System.Array.ConvertAll(renderers,x=>x.enabled);c=System.Array.ConvertAll(solids,x=>x.enabled);c2=System.Array.ConvertAll(flatSolids,x=>x.enabled);}
        public void SetState(bool value){Cache();state=value;Apply();}
        void Apply(){for(int i=0;i<renderers.Length;i++)if(renderers[i])renderers[i].enabled=state&&r[i];for(int i=0;i<solids.Length;i++)if(solids[i])solids[i].enabled=state&&c[i];for(int i=0;i<flatSolids.Length;i++)if(flatSolids[i])flatSolids[i].enabled=state&&c2[i];}
    }
}
