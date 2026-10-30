using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
 public sealed class NewLevelNameWindow:EditorWindow {
  string levelName="level1",error="";
  public static void Open(){var w=CreateInstance<NewLevelNameWindow>();w.titleContent=new GUIContent("新建关卡 Scene");w.minSize=w.maxSize=new Vector2(360,155);w.ShowUtility();}
  public static string ScenePath(string name){
   name=(name??"").Trim();if(string.IsNullOrEmpty(name)||name=="."||name==".."||name.IndexOfAny(Path.GetInvalidFileNameChars())>=0||name.EndsWith(".")||name.EndsWith(" "))throw new ArgumentException("请输入有效场景名，不要包含路径或扩展名。");
   if(name.EndsWith(".unity",StringComparison.OrdinalIgnoreCase))name=name.Substring(0,name.Length-6);
   string path=LevelKit.Root+"Scenes/"+name+".unity";if(File.Exists(path)||AssetDatabase.LoadAssetAtPath<SceneAsset>(path))throw new ArgumentException("同名 Scene 已存在，请换一个名字。");return path;
  }
  void OnGUI(){EditorGUILayout.LabelField("名称",EditorStyles.boldLabel);GUI.SetNextControlName("SceneName");levelName=EditorGUILayout.TextField(levelName);EditorGUILayout.LabelField("保存到 Assets/LightHungry/Scenes",EditorStyles.miniLabel);if(!string.IsNullOrEmpty(error))EditorGUILayout.HelpBox(error,MessageType.Error);
   using(new EditorGUILayout.HorizontalScope()){if(GUILayout.Button("取消"))Close();if(GUILayout.Button("创建")||(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Return)){try{string path=ScenePath(levelName);if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;WatercolorWorkshop.CreateEmpty(path);Close();}catch(Exception e){error=e.Message;}}}
   if(Event.current.type==EventType.Repaint&&GUI.GetNameOfFocusedControl()=="")EditorGUI.FocusTextInControl("SceneName");
  }
 }
}
