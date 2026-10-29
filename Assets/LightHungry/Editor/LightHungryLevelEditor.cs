using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
    public sealed class LightHungryLevelEditor:EditorWindow {
        int palette,view=1;float depth,grid=.5f;bool brush,preview=true;Vector3 position;Vector2 scroll;GameObject source,target;Editor inspector;string report="";
        readonly List<Vector3> path=new List<Vector3>();
        [MenuItem("Light Hungry/关卡编辑器")]
        public static void Open(){GetWindow<LightHungryLevelEditor>("食光 · 关卡编辑器");}
        void OnEnable(){SceneView.duringSceneGui+=SceneGUI;Selection.selectionChanged+=Repaint;}
        void OnDisable(){SceneView.duringSceneGui-=SceneGUI;Selection.selectionChanged-=Repaint;if(inspector)DestroyImmediate(inspector);}
        void OnGUI(){
            if(EditorApplication.isPlaying){EditorGUILayout.HelpBox("退出 Play 后编辑并保存关卡。",MessageType.Info);return;}
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("场景",EditorStyles.boldLabel);
            if(GUILayout.Button("新建关卡（保留当前场景）")){
                if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){
                    string p=EditorUtility.SaveFilePanelInProject("新建关卡","LightLevel","unity","选择新关卡位置");
                    if(!string.IsNullOrEmpty(p)){if(System.IO.File.Exists(p))p=AssetDatabase.GenerateUniqueAssetPath(p);LevelKit.EnsureAssets();LevelKit.NewLevel(p);}
                }
            }
            if(GUILayout.Button("保存当前场景"))EditorSceneManager.SaveScene(SceneManager());
            if(GUILayout.Button("生成缺少的道具预制体"))LevelKit.EnsureAssets();
            view=GUILayout.Toolbar(view-1,new[]{"1 默认","2 对面","3 左面","4 右面"})+1;
            if(GUILayout.Button("对齐 Scene 视图")){var s=SceneView.lastActiveSceneView;if(s)s.LookAt(Vector3.zero,Quaternion.Euler(0,CameraViewController.Yaw(view),0),18,true);}
            depth=EditorGUILayout.FloatField("编辑平面深度",depth);grid=Mathf.Max(.1f,EditorGUILayout.FloatField("吸附网格",grid));
            palette=EditorGUILayout.Popup("物件",palette,LevelKit.Items);position=EditorGUILayout.Vector3Field("世界位置",position);
            if(GUILayout.Button("放置物件"))Place(position);
            brush=EditorGUILayout.Toggle("Scene 点击放置（Esc 退出）",brush);preview=EditorGUILayout.Toggle("显示化光路径",preview);
            EditorGUILayout.Space();EditorGUILayout.LabelField("机关连线",EditorStyles.boldLabel);
            source=(GameObject)EditorGUILayout.ObjectField("按钮／开关",source,typeof(GameObject),true);target=(GameObject)EditorGUILayout.ObjectField("目标机关",target,typeof(GameObject),true);
            if(GUILayout.Button("连接")){
                var d=target?target.GetComponent<SpaceDevice>():null;
                if(source&&d){Undo.RecordObject(source.GetComponent<Component>(),"Connect device");var s=source.GetComponent<DeviceSwitch>();var p=source.GetComponent<DevicePlate>();if(s){Undo.RecordObject(s,"Connect switch");s.target=d;EditorUtility.SetDirty(s);}if(p){Undo.RecordObject(p,"Connect plate");p.target=d;EditorUtility.SetDirty(p);}EditorSceneManager.MarkSceneDirty(SceneManager());report=s||p?"机关已连接":"来源必须是开关或按钮";}else report="请选择开关／按钮和有效机关";
            }
            if(GUILayout.Button("检查当前关卡"))report=ValidateLevel();
            if(!string.IsNullOrEmpty(report))EditorGUILayout.HelpBox(report,MessageType.Info);
            EditorGUILayout.Space();EditorGUILayout.LabelField("选中物件参数",EditorStyles.boldLabel);
            if(Selection.activeGameObject){Editor.CreateCachedEditor(Selection.activeGameObject,null,ref inspector);inspector.OnInspectorGUI();foreach(var component in Selection.activeGameObject.GetComponents<MonoBehaviour>()){if(!component)continue;var ed=Editor.CreateEditor(component);ed.OnInspectorGUI();DestroyImmediate(ed);}}
            EditorGUILayout.EndScrollView();
        }
        static UnityEngine.SceneManagement.Scene SceneManager()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        void Place(Vector3 p){var g=LevelKit.Put(LevelKit.Items[palette],p);Undo.RegisterCreatedObjectUndo(g,"Place light puzzle prop");Selection.activeGameObject=g;EditorSceneManager.MarkSceneDirty(g.scene);}
        void SceneGUI(SceneView scene){
            if(EditorApplication.isPlaying)return;
            if(preview){Physics.SyncTransforms();foreach(var pad in FindObjectsOfType<LightLaunchPad>()){var landing=SpaceOptics.Preview(pad,path,out _);Handles.color=landing?Color.cyan:new Color(1,.5f,.2f);Handles.DrawAAPolyLine(3,path.ToArray());}}
            if(!brush)return;var e=Event.current;if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){brush=false;e.Use();Repaint();return;}
            Vector3 normal=Quaternion.Euler(0,CameraViewController.Yaw(view),0)*Vector3.forward;
            var plane=new Plane(normal,normal*depth);var ray=HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if(plane.Raycast(ray,out float hit)){Vector3 p=ray.GetPoint(hit);p=new Vector3(Mathf.Round(p.x/grid)*grid,Mathf.Round(p.y/grid)*grid,Mathf.Round(p.z/grid)*grid);Handles.color=Color.yellow;Handles.DrawWireCube(p,Vector3.one*.5f);if(e.type==EventType.MouseDown&&e.button==0&&!e.alt){Place(p);e.Use();}}
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));scene.Repaint();
        }
        public static string ValidateLevel(){
            var issues=new List<string>();if(FindObjectsOfType<PlayerController>().Length!=1)issues.Add("关卡需要且仅需要一个 Player");if(!FindObjectOfType<CameraViewController>())issues.Add("缺少四视角摄像机");if(!FindObjectOfType<SpatialLabWorld>())issues.Add("缺少 Spatial Lab");if(!FindObjectOfType<BeastReceiver>())issues.Add("缺少野兽／接光目标");
            foreach(var s in FindObjectsOfType<DeviceSwitch>())if(!s.target&&!s.lightTarget)issues.Add(s.name+" 未连接目标");
            foreach(var p in FindObjectsOfType<DevicePlate>())if(!p.target)issues.Add(p.name+" 未连接目标");
            var points=new List<Vector3>();Physics.SyncTransforms();foreach(var p in FindObjectsOfType<LightLaunchPad>())if(!SpaceOptics.Preview(p,points,out string problem))issues.Add(p.name+"："+problem);
            foreach(var l in FindObjectsOfType<LightLanding>())if(!l.spawn)issues.Add(l.name+" 缺少实体落点");
            return issues.Count==0?"配置检查通过（仍需 Play 验证路线与手感）":string.Join("\n",issues);
        }
    }
}
