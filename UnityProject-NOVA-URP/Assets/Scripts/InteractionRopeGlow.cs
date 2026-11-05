using UnityEngine;
// Mirrors dynamic rope vertices; the outline cannot lag behind the lifting mechanism.
public class InteractionRopeGlow:MonoBehaviour {
 public Material glowMaterial;LineRenderer source,glow;
 void Awake(){source=GetComponent<LineRenderer>();var g=new GameObject("SoftWhiteRopeOutline");g.transform.SetParent(transform,false);glow=g.AddComponent<LineRenderer>();glow.sharedMaterial=glowMaterial;glow.useWorldSpace=true;glow.widthMultiplier=.30f;glow.numCornerVertices=5;glow.numCapVertices=4;}
 void LateUpdate(){glow.positionCount=source.positionCount;for(int i=0;i<source.positionCount;i++){var p=source.GetPosition(i);p.z+=.025f;glow.SetPosition(i,p);}glow.enabled=source.enabled;}
}
