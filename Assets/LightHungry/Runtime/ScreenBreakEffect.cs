using UnityEngine;
namespace LightHungry {
    public sealed class ScreenBreakEffect:MonoBehaviour {
        Transform[] shards;Vector3 center;float life;
        public void Burst(Vector3 point){
            Clear();center=point;life=.6f;shards=new Transform[12];
            for(int i=0;i<shards.Length;i++){
                var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name="Light tear shard";Destroy(g.GetComponent<Collider>());shards[i]=g.transform;
                g.AddComponent<FaceMembership>().faces=RoomFaces.All;
                g.GetComponent<Renderer>().sharedMaterial=SpatialLabWorld.Active.solidMaterial;
                var block=new MaterialPropertyBlock();block.SetColor("_Color",i%2==0?new Color(.94f,.96f,1f):new Color(.68f,.80f,.86f));g.GetComponent<Renderer>().SetPropertyBlock(block);
            }
        }
        void Update(){if(life<=0||Time.timeScale==0)return;life-=Time.deltaTime;float t=1-life/.6f;
            for(int i=0;i<shards.Length;i++){float a=i*Mathf.PI*2/shards.Length;Vector3 radial=Camera.main.transform.right*Mathf.Cos(a)+Vector3.up*Mathf.Sin(a);shards[i].position=center+radial*(.15f+t*1.3f);shards[i].rotation=Camera.main.transform.rotation*Quaternion.Euler(t*110,i*15, a*Mathf.Rad2Deg);shards[i].localScale=new Vector3(.07f,.3f,1)*(1-t);}
            if(life<=0)Clear();}
        void Clear(){if(shards!=null)foreach(var s in shards)if(s)Destroy(s.gameObject);shards=null;}
        void OnDestroy(){Clear();}
    }
}
