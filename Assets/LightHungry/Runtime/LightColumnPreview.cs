using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
 [ExecuteAlways,RequireComponent(typeof(LightEmitter))] public sealed class LightColumnPreview:MonoBehaviour {
  readonly List<BeamColumn> previews=new List<BeamColumn>();
  void Update(){
   if(Application.isPlaying){Clear();return;}var source=GetComponent<LightEmitter>();if(!source.active||!source.columnMaterial){Clear();return;}
   Physics.SyncTransforms();var origin=transform.position;var direction=source.WorldDirection;float range=source.maxLength;Component previous=null;var route=OpticalRoute.From(source);int used=0;
   for(int bounce=0;bounce<12&&range>.02f;bounce++){
    bool found=SpaceOptics.CastPreview(origin,direction,range,previous,route,out var hit);var end=found?hit.point:origin+direction*range;
    if(used==previews.Count){var go=new GameObject("Light column preview");go.hideFlags=HideFlags.HideAndDontSave;previews.Add(go.AddComponent<BeamColumn>());}
    var column=previews[used++];column.gameObject.SetActive(true);column.Set(origin,end,source.beamWidth,source.beamColor,source.columnMaterial,route.DisplayFaces);range-=Vector3.Distance(origin,end);
    if(!found||!route.Reflect(hit.collider,direction,out var next,out _))break;previous=hit.collider;direction=next;origin=end+direction*.035f;range-=.035f;
   }
   for(int i=used;i<previews.Count;i++)previews[i].gameObject.SetActive(false);
  }
  void Clear(){foreach(var preview in previews)if(preview){if(Application.isPlaying)Destroy(preview.gameObject);else DestroyImmediate(preview.gameObject);}previews.Clear();}
  void OnDisable(){Clear();}void OnDestroy(){Clear();}
 }
}
