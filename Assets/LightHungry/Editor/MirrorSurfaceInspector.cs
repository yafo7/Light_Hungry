using UnityEditor;
using UnityEngine;
namespace LightHungry.EditorTools {
 [CustomEditor(typeof(MirrorSurface))] public sealed class MirrorSurfaceInspector:Editor {
  bool advanced;
  public override void OnInspectorGUI(){
   serializedObject.Update();var mirror=(MirrorSurface)target;EditorGUILayout.LabelField(mirror.depthRotation?"DepthMirror · 深度镜":"PlaneMirror · 平面镜",EditorStyles.boldLabel);
   EditorGUILayout.PropertyField(serializedObject.FindProperty("continuousRotation"),new GUIContent("按住 E 连续旋转"));
   if(mirror.continuousRotation){EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAngle"),new GUIContent("镜面角度"));EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"),new GUIContent("旋转速度"));}
   else{EditorGUILayout.PropertyField(serializedObject.FindProperty("angles"),new GUIContent("离散角度"),true);EditorGUILayout.PropertyField(serializedObject.FindProperty("angleIndex"),new GUIContent("当前角度序号"));}
   EditorGUILayout.HelpBox(mirror.depthRotation?"跨面需要两面深度镜：第一面射向空间，第二面将光反射回其所属平面。镜子位置和真实角度决定路径。":"仅反射已经进入本面平面的光，不能截获或转换跨面光。",MessageType.None);
   advanced=EditorGUILayout.Foldout(advanced,"高级与镜面引用",true);if(advanced)foreach(string field in new[]{"depthRotation","destinationView","mirrorArt","opticalPivot","face","sizes","sizeIndex"})EditorGUILayout.PropertyField(serializedObject.FindProperty(field),true);
   if(serializedObject.ApplyModifiedProperties()){mirror.Apply();LevelGraph.Changed(mirror);SceneView.RepaintAll();}
  }
 }
}
