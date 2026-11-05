using UnityEngine;
namespace LightHungry {
    // An authored receiving plane, not a portal: ray intersection determines the landing position.
    [ExecuteAlways, RequireComponent(typeof(BoxCollider), typeof(FaceMembership))]
    public sealed class CrossFaceSurface : MonoBehaviour {
        public bool available=true;
        public bool allowMaterialize=true;
        public int View=>OpticalRoute.Primary(FaceMembership.OpticalOf(this));
        public Vector3 Normal=>OpticalRoute.Normal(View);
        GameObject spot;Mesh mesh;Material material;float lastHit=-100;
        public Renderer SpotRenderer=>spot?spot.GetComponent<Renderer>():null;
        public bool CanReceive(OpticalRoute route,Vector3 direction)=>available&&route.inTransit&&route.view!=View&&Mathf.Abs(Vector3.Dot(direction.normalized,Normal))>.05f;
        public void Illuminate(Vector3 point,Vector3 incoming,Color color,float diameter){
            if(!spot){
                spot=new GameObject("Cross-face light spot");spot.hideFlags=Application.isPlaying?HideFlags.None:HideFlags.HideAndDontSave;
                spot.AddComponent<FaceMembership>().faces=FaceMembership.ForView(View);
                mesh=new Mesh{name="Light spot disc"};var vertices=new Vector3[34];var triangles=new int[96];var colors=new Color[34];
                colors[0]=new Color(1,1,1,.75f);
                for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;vertices[i+1]=new Vector3(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f,0);colors[i+1]=new Color(1,1,1,0);if(i<32){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}}
                mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();spot.AddComponent<MeshFilter>().sharedMesh=mesh;
                material=new Material(Shader.Find("Sprites/Default"));spot.AddComponent<MeshRenderer>().sharedMaterial=material;
            }
            spot.SetActive(true);spot.GetComponent<FaceMembership>().faces=FaceMembership.ForView(View);
            spot.transform.SetPositionAndRotation(point-incoming.normalized*.025f,Quaternion.LookRotation(Normal));spot.transform.localScale=Vector3.one*Mathf.Max(.3f,diameter*1.5f);material.color=color;lastHit=Time.realtimeSinceStartup;
        }
        void Update(){if(spot&&Time.realtimeSinceStartup-lastHit>.15f&&Time.timeScale>0)spot.SetActive(false);}
        void OnDisable(){if(spot){if(Application.isPlaying){Destroy(spot);Destroy(mesh);Destroy(material);}else{DestroyImmediate(spot);DestroyImmediate(mesh);DestroyImmediate(material);}}}
        void OnDrawGizmosSelected(){var box=GetComponent<BoxCollider>();Gizmos.color=new Color(.6f,.8f,1,.6f);var old=Gizmos.matrix;Gizmos.matrix=transform.localToWorldMatrix;Gizmos.DrawWireCube(box.center,box.size);Gizmos.matrix=old;}
    }
}
