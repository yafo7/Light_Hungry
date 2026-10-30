using UnityEditor;
using UnityEngine;

namespace LightHungry.EditorTools {
    [CustomEditor(typeof(DeviceCircuit))]
    public sealed class DeviceCircuitInspector:Editor {
        public override void OnInspectorGUI(){
            serializedObject.Update();EditorGUILayout.LabelField("机关组合逻辑",EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("output"),new GUIContent("控制目标"));
            var rule=serializedObject.FindProperty("rule");rule.enumValueIndex=EditorGUILayout.Popup("触发条件",rule.enumValueIndex,new[]{"任意一个输入满足","所有输入同时满足"});
            EditorGUILayout.PropertyField(serializedObject.FindProperty("invertOutput"),new GUIContent("反转输出"));
            var inputs=serializedObject.FindProperty("inputs");
            for(int i=0;i<inputs.arraySize;i++){var input=inputs.GetArrayElementAtIndex(i);EditorGUILayout.PropertyField(input.FindPropertyRelative("source"),new GUIContent("输入 "+(i+1)));EditorGUILayout.PropertyField(input.FindPropertyRelative("invert"),new GUIContent("输入 "+(i+1)+" 取反"));}
            if(serializedObject.ApplyModifiedProperties())LevelGraph.Changed(target);
        }
    }
    [CustomEditor(typeof(FaceMembership))]
    public sealed class FaceMembershipInspector:Editor {
        public override void OnInspectorGUI(){serializedObject.Update();var faces=serializedObject.FindProperty("faces");faces.intValue=EditorGUILayout.MaskField("所属面",faces.intValue,new[]{"1 默认面","2 对面","3 左面","4 右面"});EditorGUILayout.HelpBox("同时控制显示与光学交互归属。普通物件选择一个面；勾选多个面表示明确共享。",MessageType.None);if(serializedObject.ApplyModifiedProperties())LevelGraph.Changed(target);}
    }
}
