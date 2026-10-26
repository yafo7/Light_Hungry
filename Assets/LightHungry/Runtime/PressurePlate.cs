using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public sealed class PressurePlate:MonoBehaviour {
        public SpriteRenderer visual;public Sprite up,down;
        readonly HashSet<Collider2D> occupants=new HashSet<Collider2D>();
        public bool IsPressed=>occupants.Count>0;
        void OnTriggerEnter2D(Collider2D other){if(other.GetComponentInParent<PlayerController>()||other.GetComponentInParent<PushableBox>())occupants.Add(other);}
        void OnTriggerExit2D(Collider2D other){occupants.Remove(other);}
        void Update(){occupants.RemoveWhere(c=>!c||!c.gameObject.activeInHierarchy);if(visual)visual.sprite=IsPressed?down:up;}
        void OnDisable(){occupants.Clear();}
    }
}
