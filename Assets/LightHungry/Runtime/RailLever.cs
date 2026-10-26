using UnityEngine;
namespace LightHungry {
    public sealed class RailLever:Interactable {
        public RailDevice target;public SpriteRenderer visual;public Sprite left,right;bool on;
        public override void Interact(PlayerController player){if(target){target.ToggleEnd();on=!on;if(visual)visual.sprite=on?right:left;}}
    }
}
