using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace LightHungry.EditorTools {
    public sealed class LightHungryLevelEditor:EditorWindow {
        [Serializable] sealed class NodePosition { public string id;public Vector2 point; }
        [SerializeField] List<NodePosition> layout=new List<NodePosition>();
        [SerializeField] int view=1,category,connectionMode;
        [SerializeField] float depth,grid=.5f;
        [SerializeField] bool isolate=true;
        string search="",report="";bool brush;
        Vector3 placement;Vector2 paletteScroll,objectScroll,graphScroll,inspectorScroll,pageScroll;
        bool showPalette=true,showObjects=false,showGraph=false,showDetails=true;
        readonly List<GameObject> prefabs=new List<GameObject>();
        readonly Dictionary<int,Rect> rects=new Dictionary<int,Rect>();
        readonly Dictionary<int,Editor> editors=new Dictionary<int,Editor>();
        GameObject selectedPrefab,dragPrefab;MonoBehaviour wireSource,dragNode,selectedSource,selectedTarget;
        LevelGraph.Edge selectedEdge;Vector2 nodeOffset;
        static readonly string[] Categories={"全部道具","地形","光学","机关","角色与其他"};
        static readonly string[] Views={"1 默认面","2 对面","3 左面","4 右面"};
        const float CardWidth=184,CardHeight=72;

        [MenuItem("Light Hungry/关卡编辑器")]
        public static void Open(){GetWindow<LightHungryLevelEditor>("食光 · 关卡编辑器");}
        void OnEnable(){minSize=new Vector2(320,440);SceneView.duringSceneGui+=SceneGUI;Selection.selectionChanged+=Repaint;EditorApplication.projectChanged+=ReloadPalette;Undo.undoRedoPerformed+=Repaint;ReloadPalette();}
        void OnDisable(){SceneView.duringSceneGui-=SceneGUI;Selection.selectionChanged-=Repaint;EditorApplication.projectChanged-=ReloadPalette;Undo.undoRedoPerformed-=Repaint;FaceScenePreview.enabled=false;FaceScenePreview.Reset();foreach(var editor in editors.Values)if(editor)DestroyImmediate(editor);editors.Clear();SceneView.RepaintAll();}
        void OnInspectorUpdate(){Repaint();}
        public void ReloadPalette(){prefabs.Clear();bool curated=AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Root+"Prefabs/Background.prefab");foreach(string id in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/LightHungry/Prefabs"})){string path=AssetDatabase.GUIDToAssetPath(id);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab&&!prefab.GetComponent<BeamSystem>()&&!prefab.GetComponent<LabSession>()&&(!curated||(WatercolorWorkshop.Palette.Contains(prefab.name)&&path==LevelKit.Root+"Prefabs/"+prefab.name+".prefab")))prefabs.Add(prefab);}prefabs.Sort((a,b)=>string.Compare(a.name,b.name,StringComparison.Ordinal));Repaint();}
        void OnGUI(){
            if(EditorApplication.isPlaying){EditorGUILayout.HelpBox("退出 Play 后修改关卡配置。",MessageType.Info);return;}
            if(selectedEdge!=null&&(!selectedEdge.source||!selectedEdge.target))selectedEdge=null;
            DrawToolbar();
            pageScroll=EditorGUILayout.BeginScrollView(pageScroll);
            showPalette=EditorGUILayout.Foldout(showPalette,"道具库",true);if(showPalette){using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox,GUILayout.Height(310)))DrawPalette();}
            showObjects=EditorGUILayout.Foldout(showObjects,"场景物件",true);if(showObjects)DrawObjectList();
            showGraph=EditorGUILayout.Foldout(showGraph,"机关关系",true);if(showGraph){if(GUILayout.Button("整理关系图")){layout.Clear();graphScroll=Vector2.zero;}DrawGraph(GUILayoutUtility.GetRect(100,320,GUILayout.ExpandWidth(true)));}
            showDetails=EditorGUILayout.Foldout(showDetails,"放置与选中参数",true);if(showDetails)DrawDetails();
            EditorGUILayout.EndScrollView();
        }
        void DrawToolbar(){
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)){
                if(GUILayout.Button("新建关卡",EditorStyles.toolbarButton))NewLevelNameWindow.Open();
                if(GUILayout.Button("保存",EditorStyles.toolbarButton))EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                if(GUILayout.Button("检查",EditorStyles.toolbarButton))report=ValidateLevel();
                if(GUILayout.Button("刷新",EditorStyles.toolbarButton))ReloadPalette();
            }
            EditorGUILayout.LabelField(EditorSceneManager.GetActiveScene().name,EditorStyles.boldLabel);
            int next=EditorGUILayout.Popup("观察面",view-1,Views)+1;if(next!=view){view=next;AlignScene();}
            using(new EditorGUILayout.HorizontalScope()){
                isolate=GUILayout.Toggle(isolate,"只看当前面");if(GUILayout.Button("对齐 Scene"))AlignScene();
            }
            depth=EditorGUILayout.FloatField("放置深度",depth);grid=Mathf.Max(.1f,EditorGUILayout.FloatField("吸附网格",grid));
            FaceScenePreview.view=view;FaceScenePreview.enabled=isolate;
        }
        void AlignScene(){var scene=SceneView.lastActiveSceneView;if(scene)scene.LookAt(Vector3.zero,Quaternion.Euler(0,CameraViewController.Yaw(view),0),18,true);SceneView.RepaintAll();}
        static int Category(GameObject p){if(p.GetComponentInChildren<SpaceMirror>()||p.GetComponentInChildren<MirrorSurface>()||p.GetComponentInChildren<LightEmitter>()||p.GetComponentInChildren<LensSurface>())return 2;if(p.GetComponent<SpaceDevice>()||p.GetComponent<RailDevice>()||p.GetComponent<RailLever>()||p.GetComponent<DevicePlate>()||p.GetComponent<DeviceSwitch>()||p.GetComponent<PressurePlate>()||p.GetComponent<CarryableProp>()||p.GetComponent<PushableBox>())return 3;if(p.layer==8||p.GetComponent<SpaceHazard>())return 1;return 4;}
        void DrawPalette(){
            GUILayout.Label("道具与地形库",EditorStyles.boldLabel);search=EditorGUILayout.TextField(search,EditorStyles.toolbarSearchField);category=EditorGUILayout.Popup(category,Categories);GUILayout.Label("复用项目 prefab · 拖入 Scene",EditorStyles.miniLabel);
            paletteScroll=EditorGUILayout.BeginScrollView(paletteScroll);
            var visible=prefabs.Where(p=>p&&(category==0||Category(p)==category)&&(string.IsNullOrEmpty(search)||p.name.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0)).ToList();
            int columns=Mathf.Max(1,Mathf.FloorToInt((position.width-42)/108));
            for(int i=0;i<visible.Count;i+=columns)using(new EditorGUILayout.HorizontalScope())for(int col=0;col<columns&&i+col<visible.Count;col++){
                var prefab=visible[i+col];Rect rect=GUILayoutUtility.GetRect(103,110,GUILayout.Width(103));GUI.Box(rect,GUIContent.none,prefab==selectedPrefab?"selectionRect":"box");var texture=AssetPreview.GetAssetPreview(prefab);if(!texture)texture=AssetPreview.GetMiniThumbnail(prefab);if(texture)GUI.DrawTexture(new Rect(rect.x+12,rect.y+5,78,72),texture,ScaleMode.ScaleToFit);
                GUI.Label(new Rect(rect.x+3,rect.y+79,97,28),new GUIContent(prefab.name,"拖到 Scene 放置；双击按右侧坐标放置"),new GUIStyle(EditorStyles.miniLabel){wordWrap=true});
                int control=GUIUtility.GetControlID(prefab.GetInstanceID(),FocusType.Passive,rect);var e=Event.current;
                if(rect.Contains(e.mousePosition)&&e.type==EventType.MouseDown&&e.button==0){selectedPrefab=prefab;dragPrefab=prefab;GUIUtility.hotControl=control;if(e.clickCount==2){GUIUtility.hotControl=0;dragPrefab=null;PlacePrefab(prefab,placement,view);}e.Use();Repaint();}
                if(e.type==EventType.MouseDrag&&GUIUtility.hotControl==control&&dragPrefab){DragAndDrop.PrepareStartDrag();DragAndDrop.objectReferences=new Object[]{dragPrefab};GUIUtility.hotControl=0;DragAndDrop.StartDrag(dragPrefab.name);dragPrefab=null;e.Use();}
                if(e.type==EventType.MouseUp&&GUIUtility.hotControl==control){GUIUtility.hotControl=0;dragPrefab=null;e.Use();}
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawObjectList(){
            GUILayout.Label("当前关卡物件",EditorStyles.boldLabel);objectScroll=EditorGUILayout.BeginScrollView(objectScroll,GUILayout.Height(140));
            foreach(var member in Object.FindObjectsOfType<FaceMembership>().Where(m=>m.gameObject.scene==EditorSceneManager.GetActiveScene()).OrderBy(m=>m.name))using(new EditorGUILayout.HorizontalScope()){
                if(GUILayout.Button(member.name,EditorStyles.miniButton,GUILayout.MinWidth(50)))Selection.activeGameObject=member.gameObject;
                GUILayout.Label(member.faces.ToString(),EditorStyles.miniLabel,GUILayout.Width(65));if(GUILayout.Button("定位",EditorStyles.miniButton,GUILayout.Width(42)))Focus(member.gameObject);
            }
            EditorGUILayout.EndScrollView();
        }
        static void Focus(GameObject go){Selection.activeGameObject=go;EditorGUIUtility.PingObject(go);if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.FrameSelected();}
        public static GameObject PlacePrefab(GameObject prefab,Vector3 point,int face){
            if(!prefab||!PrefabUtility.IsPartOfPrefabAsset(prefab))throw new ArgumentException("请选择项目中的 prefab");
            Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Place level object");var placed=(GameObject)PrefabUtility.InstantiatePrefab(prefab,EditorSceneManager.GetActiveScene());Undo.RegisterCreatedObjectUndo(placed,"Place level object");placed.transform.position=point;
            if(!placed.GetComponent<PlayerController>()&&!placed.GetComponent<BeastReceiver>()&&!placed.GetComponent<AbilityPickup>())placed.transform.rotation=Quaternion.Euler(0,CameraViewController.Yaw(face),0)*placed.transform.rotation;
            var light=placed.GetComponent<LightEmitter>();if(light){Undo.RecordObject(light,"Orient placed light");light.useSpatialDirection=true;light.spatialDirection=Quaternion.Euler(0,CameraViewController.Yaw(face),0)*(Vector3)light.direction;LevelGraph.Changed(light);}
            var landing=placed.GetComponent<LightLanding>();if(landing){Undo.RecordObject(landing,"Set landing face");landing.view=face;LevelGraph.Changed(landing);}
            var member=placed.GetComponent<FaceMembership>();if(!member)member=Undo.AddComponent<FaceMembership>(placed);Undo.RecordObject(member,"Set object face");member.faces=placed.GetComponent<PlayerController>()?RoomFaces.All:FaceMembership.ForView(face);LevelGraph.Changed(member);LevelGraph.Changed(placed.transform);if(prefab.name=="Background")RoomBackgrounds.Assign(placed,face);Undo.CollapseUndoOperations(group);Selection.activeGameObject=placed;return placed;
        }
        Vector2 NodePoint(MonoBehaviour node,int index){string id=GlobalObjectId.GetGlobalObjectIdSlow(node).ToString();var entry=layout.Find(p=>p.id==id);if(entry==null){Vector2 point=new Vector2(LevelGraph.IsSource(node)?30:460,30+index*96);while(layout.Exists(p=>Mathf.Abs(p.point.x-point.x)<CardWidth&&Mathf.Abs(p.point.y-point.y)<CardHeight+6))point.y+=96;entry=new NodePosition{id=id,point=point};layout.Add(entry);}return entry.point;}
        void MoveNode(MonoBehaviour node,Vector2 point){string id=GlobalObjectId.GetGlobalObjectIdSlow(node).ToString();var entry=layout.Find(p=>p.id==id);if(entry!=null)entry.point=new Vector2(Mathf.Max(0,point.x),Mathf.Max(0,point.y));}
        static Vector2 In(Rect r)=>new Vector2(r.x,r.center.y);
        static Vector2 Out(Rect r)=>new Vector2(r.xMax,r.center.y);
        void DrawGraph(Rect area){
            GUI.Box(area,GUIContent.none);GUI.Label(new Rect(area.x+8,area.y+3,area.width-16,22),"机关关系 · 场景真实引用",EditorStyles.boldLabel);
            var nodes=LevelGraph.Nodes();int sources=0,targets=0;rects.Clear();foreach(var node in nodes){var p=NodePoint(node,LevelGraph.IsSource(node)?sources++:targets++);rects[node.GetInstanceID()]=new Rect(p.x,p.y,CardWidth,CardHeight);}
            float height=Mathf.Max(800,rects.Count>0?rects.Values.Max(r=>r.yMax)+100:800);graphScroll=GUI.BeginScrollView(new Rect(area.x+2,area.y+27,area.width-4,area.height-29),graphScroll,new Rect(0,0,1500,height));
            var e=Event.current;Handles.BeginGUI();
            foreach(var edge in LevelGraph.Edges()){
                if(!rects.TryGetValue(edge.source.GetInstanceID(),out var a)||!rects.TryGetValue(edge.target.GetInstanceID(),out var b))continue;var start=Out(a);var end=In(b);bool selected=selectedEdge!=null&&selectedEdge.source==edge.source&&selectedEdge.target==edge.target;
                Handles.DrawBezier(start,end,start+Vector2.right*100,end-Vector2.right*100,selected?Color.yellow:new Color(.35f,.8f,.8f),null,selected?4:2);GUI.Label(new Rect((start.x+end.x)/2-34,(start.y+end.y)/2-19,90,20),edge.Label,EditorStyles.miniLabel);
                if(e.type==EventType.MouseDown&&e.button==0&&BezierDistance(e.mousePosition,start,end)<8){selectedEdge=edge;Selection.activeGameObject=edge.target.gameObject;Repaint();}
            }
            if(wireSource&&rects.TryGetValue(wireSource.GetInstanceID(),out var origin))Handles.DrawBezier(Out(origin),e.mousePosition,Out(origin)+Vector2.right*80,e.mousePosition-Vector2.right*80,Color.yellow,null,3);Handles.EndGUI();
            foreach(var node in nodes){Rect rect=rects[node.GetInstanceID()];GUI.Box(rect,GUIContent.none,Selection.activeGameObject==node.gameObject?"selectionRect":"box");GUI.Label(new Rect(rect.x+10,rect.y+7,rect.width-20,20),node.name,EditorStyles.boldLabel);GUI.Label(new Rect(rect.x+10,rect.y+29,rect.width-20,18),node.GetType().Name,EditorStyles.miniLabel);GUI.Label(new Rect(rect.x+10,rect.y+48,rect.width-20,18),FaceMembership.Of(node).ToString(),EditorStyles.miniLabel);
                if(LevelGraph.IsSource(node))GUI.Label(new Rect(rect.xMax-7,rect.center.y-10,18,20),"●");if(LevelGraph.IsTarget(node))GUI.Label(new Rect(rect.x-7,rect.center.y-10,18,20),"●");
                if(e.type==EventType.MouseDown&&e.button==0){if(LevelGraph.IsSource(node)&&Vector2.Distance(e.mousePosition,Out(rect))<14){wireSource=node;e.Use();}else if(rect.Contains(e.mousePosition)){Selection.activeGameObject=node.gameObject;selectedEdge=null;dragNode=node;nodeOffset=e.mousePosition-rect.position;if(e.clickCount==2)Focus(node.gameObject);e.Use();}}
                if(e.type==EventType.MouseUp&&wireSource&&LevelGraph.IsTarget(node)&&Vector2.Distance(e.mousePosition,In(rect))<18){Connect(wireSource,node);wireSource=null;e.Use();Repaint();}
            }
            if(e.type==EventType.MouseDrag&&dragNode){MoveNode(dragNode,e.mousePosition-nodeOffset);e.Use();Repaint();}if(e.type==EventType.MouseUp){wireSource=null;dragNode=null;}
            if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Delete&&selectedEdge!=null){LevelGraph.Disconnect(selectedEdge);selectedEdge=null;e.Use();}GUI.EndScrollView();
        }
        static float BezierDistance(Vector2 point,Vector2 a,Vector2 b){float best=float.PositiveInfinity;Vector2 previous=a;for(int i=1;i<=24;i++){float t=i/24f,u=1-t;Vector2 next=u*u*u*a+3*u*u*t*(a+Vector2.right*100)+3*u*t*t*(b-Vector2.right*100)+t*t*t*b;best=Mathf.Min(best,HandleUtility.DistancePointLine(point,previous,next));previous=next;}return best;}
        void DrawDetails(){
            GUILayout.Label("放置与参数",EditorStyles.boldLabel);GUILayout.Label(selectedPrefab?selectedPrefab.name:"选择左侧道具卡片",EditorStyles.miniLabel);placement=EditorGUILayout.Vector3Field("放置坐标",placement);
            using(new EditorGUI.DisabledScope(!selectedPrefab)){if(GUILayout.Button("按坐标放置"))PlacePrefab(selectedPrefab,placement,view);brush=EditorGUILayout.Toggle("连续点击放置",brush);}
            GUILayout.Space(10);GUILayout.Label("连接操作",EditorStyles.boldLabel);selectedSource=(MonoBehaviour)EditorGUILayout.ObjectField("输入按钮／开关",selectedSource,typeof(MonoBehaviour),true);selectedTarget=(MonoBehaviour)EditorGUILayout.ObjectField("输出机关",selectedTarget,typeof(MonoBehaviour),true);
            connectionMode=EditorGUILayout.Popup("连接方式",connectionMode,new[]{"组合逻辑（任意／同时）","滑轮左移按钮","滑轮右移按钮","滑轮端点开关"});
            var selected=Selection.activeGameObject;if(selected){var components=selected.GetComponents<MonoBehaviour>();using(new EditorGUILayout.HorizontalScope()){if(GUILayout.Button("选作输入"))selectedSource=components.FirstOrDefault(LevelGraph.IsSource);if(GUILayout.Button("选作输出"))selectedTarget=components.FirstOrDefault(LevelGraph.IsTarget);}}
            if(GUILayout.Button("连接输入与输出"))Connect(selectedSource,selectedTarget);
            if(selectedEdge!=null&&GUILayout.Button("删除所选连线")){LevelGraph.Disconnect(selectedEdge);selectedEdge=null;}
            if(!string.IsNullOrEmpty(report))EditorGUILayout.HelpBox(report,MessageType.Info);
            if(selected){GUILayout.Space(10);GUILayout.Label("选中物件："+selected.name,EditorStyles.boldLabel);string renamed=EditorGUILayout.TextField("名称",selected.name);if(renamed!=selected.name){Undo.RecordObject(selected,"Rename level object");selected.name=renamed;LevelGraph.Changed(selected);}if(GUILayout.Button("定位到 Scene"))Focus(selected);if(!selected.GetComponentInParent<FaceMembership>()&&GUILayout.Button("归入当前面")){var m=Undo.AddComponent<FaceMembership>(selected);m.faces=FaceMembership.ForView(view);LevelGraph.Changed(m);}DrawInspector(selected.transform);foreach(var component in selected.GetComponents<MonoBehaviour>())if(component)DrawInspector(component);}
        }
        void Connect(MonoBehaviour source,MonoBehaviour target){bool connected;if(source is PulleyRope rope&&target is MirrorSurface mirror){Undo.RecordObject(rope,"Bind hoisted mirror");rope.load=mirror.transform;LevelGraph.Changed(rope);connected=true;}else connected=connectionMode==0?LevelGraph.Connect(source,target)!=null:LevelGraph.ConnectDirectional(source,target,connectionMode);if(connected){Selection.activeGameObject=target.gameObject;selectedEdge=null;}report=connected?"连接已写入场景。选择目标可设置组合逻辑。":"连接无效：镜子可连接滑轮绳，其他输入可连接光源或机关。";}
        void DrawInspector(Object item){if(!editors.TryGetValue(item.GetInstanceID(),out var editor)||!editor){editor=Editor.CreateEditor(item);editors[item.GetInstanceID()]=editor;}bool wide=EditorGUIUtility.wideMode;float label=EditorGUIUtility.labelWidth;EditorGUIUtility.wideMode=false;EditorGUIUtility.labelWidth=95;try{EditorGUI.BeginChangeCheck();editor.OnInspectorGUI();if(EditorGUI.EndChangeCheck())LevelGraph.Changed(item);}finally{EditorGUIUtility.wideMode=wide;EditorGUIUtility.labelWidth=label;}}
        void SceneGUI(SceneView scene){
            if(EditorApplication.isPlaying)return;var e=Event.current;GameObject dragged=DragAndDrop.objectReferences.OfType<GameObject>().FirstOrDefault(p=>PrefabUtility.IsPartOfPrefabAsset(p));bool dropping=dragged&&(e.type==EventType.DragUpdated||e.type==EventType.DragPerform);if(!brush&&!dropping)return;
            if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){brush=false;e.Use();Repaint();return;}
            Vector3 normal=Quaternion.Euler(0,CameraViewController.Yaw(view),0)*Vector3.forward;var plane=new Plane(normal,normal*depth);var ray=HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if(plane.Raycast(ray,out float hit)){Vector3 point=ray.GetPoint(hit);point=new Vector3(Mathf.Round(point.x/grid)*grid,Mathf.Round(point.y/grid)*grid,Mathf.Round(point.z/grid)*grid);Handles.color=Color.yellow;Handles.DrawWireCube(point,Vector3.one*.6f);
                if(dropping){DragAndDrop.visualMode=DragAndDropVisualMode.Copy;if(e.type==EventType.DragPerform){DragAndDrop.AcceptDrag();PlacePrefab(dragged,point,view);}e.Use();}else if(e.type==EventType.MouseDown&&e.button==0&&!e.alt&&selectedPrefab){PlacePrefab(selectedPrefab,point,view);e.Use();}
            }
            if(brush)HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));scene.Repaint();
        }
        public static string ValidateLevel(){
            var issues=new List<string>();if(Object.FindObjectsOfType<PlayerController>().Length!=1)issues.Add("关卡需要且仅需要一个 Player");if(!Object.FindObjectOfType<CameraViewController>())issues.Add("缺少四视角摄像机");if(!Object.FindObjectOfType<FaceVisibility>())issues.Add("缺少逐面显示控制");if(!Object.FindObjectOfType<SpatialLabWorld>())issues.Add("缺少 Spatial Lab");if(!Object.FindObjectOfType<BeastReceiver>())issues.Add("缺少野兽／接光目标");
            var edges=LevelGraph.Edges();foreach(var node in LevelGraph.Nodes())if(LevelGraph.IsSource(node)&&!edges.Exists(e=>e.source==node))issues.Add(node.name+" 未连接目标");
            foreach(var circuit in Object.FindObjectsOfType<DeviceCircuit>()){if(!circuit.output||!LevelGraph.IsTarget(circuit.output))issues.Add(circuit.name+" 输出无效");if(circuit.inputs.Count==0||circuit.inputs.Exists(i=>i==null||!i.source||!LevelGraph.IsSource(i.source)))issues.Add(circuit.name+" 输入缺失或无效");}
            foreach(var member in Object.FindObjectsOfType<FaceMembership>())if(member.faces==RoomFaces.None)issues.Add(member.name+" 未选择可见面");
            foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())if(root.GetComponentInChildren<Renderer>()&&!root.GetComponent<FaceMembership>()&&!root.GetComponent<SpatialLabWorld>()&&!root.GetComponent<BeamSystem>())issues.Add(root.name+" 未设置面归属（默认仅在默认面显示）");
            var points=new List<Vector3>();Physics.SyncTransforms();foreach(var pad in Object.FindObjectsOfType<LightLaunchPad>())if(pad.available&&!SpaceOptics.Preview(pad,points,out string problem))issues.Add(pad.name+"：光路 "+problem);foreach(var landing in Object.FindObjectsOfType<LightLanding>())if(!landing.spawn)issues.Add(landing.name+" 缺少实体落点");
            return issues.Count==0?"配置检查通过（仍需 Play 验证路线与手感）":string.Join("\n",issues);
        }
    }
}
