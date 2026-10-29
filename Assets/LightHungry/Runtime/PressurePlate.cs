using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public sealed class PressurePlate:MonoBehaviour {
        public SpriteRenderer visual;public Sprite up,down;
        readonly HashSet<Collider2D> occupants=new HashSet<Collider2D>();
        readonly HashSet<Collider> spatialOccupants=new HashSet<Collider>();
        public bool IsPressed=>occupants.Count>0||spatialOccupants.Count>0;
        void OnTriggerEnter2D(Collider2D other){if(other.GetComponentInParent<PlayerController>()||other.GetComponentInParent<PushableBox>())occupants.Add(other);}
        void OnTriggerExit2D(Collider2D other){occupants.Remove(other);}
        public void OnTriggerEnter(Collider other){if(other.GetComponentInParent<PlayerController>()||other.GetComponentInParent<PushableBox>())spatialOccupants.Add(other);}
        public void OnTriggerExit(Collider other){spatialOccupants.Remove(other);}
        void Update(){occupants.RemoveWhere(c=>!c||!c.gameObject.activeInHierarchy);spatialOccupants.RemoveWhere(c=>!c||!c.gameObject.activeInHierarchy);if(visual)visual.sprite=IsPressed?down:up;}
        void OnDisable(){occupants.Clear();spatialOccupants.Clear();}
    }
}
