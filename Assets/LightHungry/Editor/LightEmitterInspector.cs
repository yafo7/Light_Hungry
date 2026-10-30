using UnityEditor;
using UnityEngine;
namespace LightHungry.EditorTools {
 [CustomEditor(typeof(LightEmitter))] public sealed class LightEmitterInspector:Editor {
  public override void OnInspectorGUI(){serializedObject.Update();EditorGUILayout.PropertyField(serializedObject.FindProperty("active"),new GUIContent("发光"));EditorGUILayout.PropertyField(serializedObject.FindProperty("beamWidth"),new GUIContent("光柱直径（米）"));EditorGUILayout.PropertyField(serializedObject.FindProperty("beamColor"),new GUIContent("颜色／透明度"));EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLength"),new GUIContent("最大长度"));
   var transformDirection=serializedObject.FindProperty("directionFromTransform");EditorGUILayout.PropertyField(transformDirection,new GUIContent("随物体旋转"));
   if(transformDirection.boolValue)EditorGUILayout.HelpBox("物体本地红色 X 轴为发光方向。使用旋转工具调整；Z 轴控制平面方向，Y 轴控制深度方向。",MessageType.None);
   else{var spatial=serializedObject.FindProperty("useSpatialDirection");EditorGUILayout.PropertyField(spatial,new GUIContent("世界空间方向"));EditorGUILayout.PropertyField(serializedObject.FindProperty(spatial.boolValue?"spatialDirection":"direction"),new GUIContent("发射方向"));}
   EditorGUILayout.PropertyField(serializedObject.FindProperty("columnMaterial"),new GUIContent("光柱材质"));serializedObject.ApplyModifiedProperties();
  }
  void OnSceneGUI(){var source=(LightEmitter)target;Handles.color=source.beamColor;float length=HandleUtility.GetHandleSize(source.transform.position);Handles.ArrowHandleCap(0,source.transform.position,Quaternion.LookRotation(source.WorldDirection),length,EventType.Repaint);}
 }
}
