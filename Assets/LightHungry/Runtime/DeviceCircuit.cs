using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHungry {
    public enum CircuitRule { Any, All }
    [Serializable] public sealed class CircuitInput { public MonoBehaviour source;public bool invert; }
    [DefaultExecutionOrder(100), DisallowMultipleComponent]
    public sealed class DeviceCircuit : MonoBehaviour {
        public MonoBehaviour output;
        public CircuitRule rule;
        public bool invertOutput;
        public List<CircuitInput> inputs=new List<CircuitInput>();
        public bool State {get;private set;}
        bool initialized;
        public static bool Read(MonoBehaviour source){
            if(!source||!source.isActiveAndEnabled)return false;
            if(source is DeviceSwitch s)return s.state;
            if(source is DevicePlate p)return p.IsPressed;
            if(source is PressurePlate old)return old.IsPressed;
            if(source is RailLever lever)return lever.IsOn;
            return false;
        }
        public bool Evaluate(){
            if(inputs.Count==0)return false;
            bool value=rule==CircuitRule.All;
            foreach(var input in inputs){bool next=input!=null&&input.source&&(Read(input.source)^input.invert);if(rule==CircuitRule.Any)value|=next;else value&=next;}
            return value^invertOutput;
        }
        void LateUpdate(){if(inputs.Count==0||!output)return;bool value=Evaluate();if(!initialized||value!=State){initialized=true;State=value;Apply(value);}}
        void Apply(bool value){
            if(output is SpaceDevice device)device.SetState(value);
            else if(output is RailDevice rail)rail.Drive(value);
            else if(output is LightEmitter light)light.active=value;
            else if(output is SpaceMirror mirror){mirror.orientation=value?1:0;mirror.Apply();}
        }
        void OnEnable(){initialized=false;}
    }
}
