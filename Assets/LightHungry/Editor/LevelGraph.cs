using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightHungry.EditorTools {
    // Scene references are the source of truth; the window does not keep a second graph asset.
    public static class LevelGraph {
        public sealed class Edge {
            public MonoBehaviour source,target;public DeviceCircuit circuit;public string slot;
            public string Label=>circuit?(circuit.rule==CircuitRule.All?"同时满足":"任意满足"):slot;
        }
        public static bool IsSource(MonoBehaviour c)=>c is DeviceSwitch||c is DevicePlate||c is PressurePlate||c is RailLever||c is PulleyRope;
        public static bool IsTarget(MonoBehaviour c)=>c is SpaceDevice||c is RailDevice||c is LightEmitter||c is SpaceMirror||c is MirrorSurface;
        public static List<MonoBehaviour> Nodes(){
            var result=new List<MonoBehaviour>();foreach(var c in Object.FindObjectsOfType<MonoBehaviour>())if(c.gameObject.scene==EditorSceneManager.GetActiveScene()&&(IsSource(c)||IsTarget(c)))result.Add(c);
            result.Sort((a,b)=>string.Compare(a.name,b.name,System.StringComparison.Ordinal));return result;
        }
        public static List<Edge> Edges(){
            var edges=new List<Edge>();
            foreach(var c in Nodes()){
                if(c is PulleyRope rope&&rope.load)Add(edges,rope,rope.load.GetComponent<MirrorSurface>(),"吊起");
                if(c is DeviceSwitch s){Add(edges,s,s.target,"开关");Add(edges,s,s.lightTarget,"光源");}
                if(c is DevicePlate p)Add(edges,p,p.target,"踩踏");
                if(c is RailLever l)Add(edges,l,l.target,"切换端点");
                if(c is RailDevice rail){Add(edges,rail.leftPlate,rail,"左移");Add(edges,rail.rightPlate,rail,"右移");}
            }
            foreach(var circuit in Object.FindObjectsOfType<DeviceCircuit>())if(circuit.gameObject.scene==EditorSceneManager.GetActiveScene())foreach(var input in circuit.inputs)if(input!=null&&input.source&&circuit.output)edges.Add(new Edge{source=input.source,target=circuit.output,circuit=circuit});
            return edges;
        }
        static void Add(List<Edge> edges,MonoBehaviour source,MonoBehaviour target,string slot){if(source&&target)edges.Add(new Edge{source=source,target=target,slot=slot});}
        public static void Changed(Object item){EditorUtility.SetDirty(item);PrefabUtility.RecordPrefabInstancePropertyModifications(item);if(item is Component c)EditorSceneManager.MarkSceneDirty(c.gameObject.scene);}
        public static DeviceCircuit Connect(MonoBehaviour source,MonoBehaviour target){
            if(!source||!target||!IsSource(source)||!IsTarget(target)||source.gameObject.scene!=target.gameObject.scene||EditorApplication.isPlaying)return null;
            if(target is MirrorSurface m){if(source is PulleyRope rope){Undo.RecordObject(rope,"Bind pulley load");rope.load=m.transform;Changed(rope);}return null;}
            if(source is PulleyRope)return null;
            if(target is RailDevice rail&&(rail.leftPlate||rail.rightPlate||Object.FindObjectsOfType<RailLever>().AnyLeverFor(rail)))return null;
            Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Connect puzzle devices");
            var circuit=target.GetComponent<DeviceCircuit>();
            if(circuit&&circuit.output&&circuit.output!=target)return null;
            if(!circuit)circuit=Undo.AddComponent<DeviceCircuit>(target.gameObject);
            Undo.RecordObject(circuit,"Connect puzzle devices");circuit.output=target;
            // Convert existing direct links to this output once, avoiding competing writers.
            foreach(var edge in Edges())if(!edge.circuit&&edge.target==target){Append(circuit,edge.source);ClearLegacy(edge);}
            Append(circuit,source);Changed(circuit);Undo.CollapseUndoOperations(group);return circuit;
        }
        static bool AnyLeverFor(this RailLever[] levers,RailDevice target){foreach(var lever in levers)if(lever.target==target)return true;return false;}
        public static bool ConnectDirectional(MonoBehaviour source,MonoBehaviour target,int mode){
            if(EditorApplication.isPlaying||!(target is RailDevice rail)||rail.GetComponent<DeviceCircuit>())return false;
            if((mode==1||mode==2)&&source is PressurePlate plate){Undo.RecordObject(rail,"Connect directional plate");if(mode==1)rail.leftPlate=plate;else rail.rightPlate=plate;Changed(rail);return true;}
            if(mode==3&&source is RailLever lever){Undo.RecordObject(lever,"Connect rail lever");lever.target=rail;Changed(lever);return true;}
            return false;
        }
        static void Append(DeviceCircuit circuit,MonoBehaviour source){if(!circuit.inputs.Exists(i=>i!=null&&i.source==source))circuit.inputs.Add(new CircuitInput{source=source});}
        public static void Disconnect(Edge edge){
            if(EditorApplication.isPlaying)return;
            if(edge.circuit){var circuit=edge.circuit;Undo.RecordObject(circuit,"Disconnect puzzle devices");circuit.inputs.RemoveAll(i=>i.source==edge.source);Changed(circuit);if(circuit.inputs.Count==0)Undo.DestroyObjectImmediate(circuit);}
            else ClearLegacy(edge);
        }
        static void ClearLegacy(Edge edge){
            if(edge.source is PulleyRope rope){Undo.RecordObject(rope,"Disconnect pulley load");rope.load=null;Changed(rope);}
            if(edge.source is DeviceSwitch s){Undo.RecordObject(s,"Replace direct switch connection");if(edge.target==s.target)s.target=null;if(edge.target==s.lightTarget)s.lightTarget=null;Changed(s);}
            if(edge.source is DevicePlate p){Undo.RecordObject(p,"Replace direct plate connection");p.target=null;Changed(p);}
            if(edge.source is RailLever l){Undo.RecordObject(l,"Replace direct lever connection");l.target=null;Changed(l);}
            if(edge.target is RailDevice r&&edge.source is PressurePlate){Undo.RecordObject(r,"Replace direct rail connection");if(r.leftPlate==edge.source)r.leftPlate=null;if(r.rightPlate==edge.source)r.rightPlate=null;Changed(r);}
        }
    }
}
