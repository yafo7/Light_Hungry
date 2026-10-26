namespace LightHungry {
    public sealed class SwitchableLight:Interactable {
        public LightEmitter source;
        public override void Interact(PlayerController player){if(source)source.active=!source.active;}
    }
}
