using UnityEngine;
namespace LightHungry {
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RailDevice:MonoBehaviour {
        public float travel=3,speed=1.7f;public PressurePlate leftPlate,rightPlate;
        public float Position01=>Mathf.Clamp01((transform.position.x-start.x)/travel);
        Vector3 start;Rigidbody2D body;Rigidbody spatial;bool seekEnd;float goal;Vector2 step;
        void Awake(){start=transform.position;body=GetComponent<Rigidbody2D>();spatial=SpatialLabWorld.Active?GetComponentInParent<Rigidbody>():null;}
        public void ToggleEnd(){goal=Position01<.5f?1:0;seekEnd=true;}
        void FixedUpdate(){
            int direction=(rightPlate&&rightPlate.IsPressed?1:0)-(leftPlate&&leftPlate.IsPressed?1:0);
            if((leftPlate&&leftPlate.IsPressed)||(rightPlate&&rightPlate.IsPressed))seekEnd=false;
            float current=spatial?spatial.position.x:body.position.x;
            float target=current;
            if(direction!=0)target=Mathf.Clamp(target+direction*speed*Time.fixedDeltaTime,start.x,start.x+travel);
            else if(seekEnd)target=Mathf.MoveTowards(target,start.x+goal*travel,speed*Time.fixedDeltaTime);
            step=new Vector2(target-current,0);if(spatial)spatial.MovePosition(spatial.position+(Vector3)step);else body.MovePosition(body.position+step);
        }
        public void CarrySpatialCollision(Collision collision){
            if(collision.rigidbody&&collision.transform.position.y>transform.position.y+.4f)collision.rigidbody.position+=(Vector3)step;
        }
        void OnCollisionStay2D(Collision2D collision){
            if(collision.rigidbody&&collision.transform.position.y>transform.position.y+.4f)collision.rigidbody.position+=step;
        }
        void OnDrawGizmosSelected(){Gizmos.color=Color.yellow;Gizmos.DrawLine(transform.position,transform.position+Vector3.right*travel);}
    }
}
