(function (scope) {
  'use strict';
  const radians = degrees => degrees * Math.PI / 180;
  const ease = t => t*t*t*(t*(t*6-15)+10);
  class LeverRig {
    constructor(definition) {
      this.definition = definition;
      this.angle = definition.motion.leftDegrees;
      this.target = this.angle;
      this.from = this.angle;
      this.elapsed = 0;
      this.moving = false;
    }
    setTarget(right) {
      this.from = this.angle;
      this.target = right ? this.definition.motion.rightDegrees : this.definition.motion.leftDegrees;
      this.elapsed = 0;
      this.moving = Math.abs(this.target-this.angle) > 1e-8;
    }
    seek(angle) {
      this.angle = Math.max(this.definition.motion.leftDegrees, Math.min(this.definition.motion.rightDegrees, angle));
      this.target = this.angle;
      this.moving = false;
    }
    update(deltaMs) {
      if (!this.moving) return;
      this.elapsed = Math.min(this.definition.motion.durationMs, this.elapsed + deltaMs);
      this.angle = this.from + (this.target-this.from)*ease(this.elapsed/this.definition.motion.durationMs);
      if (this.elapsed >= this.definition.motion.durationMs) { this.angle = this.target; this.moving = false; }
    }
    pose() {
      const [x,y] = this.definition.bones[1].position;
      const length = this.definition.bones[1].length, a = radians(this.angle);
      return { angle:this.angle, pivot:[x,y], grip:[x+length*Math.sin(a),y-length*Math.cos(a)], length, scale:[1,1] };
    }
    draw(ctx, textures, showBones=false) {
      const pose = this.pose();
      ctx.clearRect(0,0,512,512);
      // One rigid attachment, 100% weight on the handle bone. No animated scale.
      ctx.save();
      ctx.translate(...pose.pivot);
      ctx.rotate(radians(this.angle));
      ctx.drawImage(textures.arm, -pose.pivot[0], -pose.pivot[1]);
      ctx.restore();
      ctx.drawImage(textures.base,0,0);
      if (showBones) {
        ctx.save(); ctx.strokeStyle='#71f6de';ctx.fillStyle='#152e34';ctx.lineWidth=3;
        ctx.beginPath();ctx.moveTo(...pose.pivot);ctx.lineTo(...pose.grip);ctx.stroke();
        for(const point of [pose.pivot,pose.grip]){ctx.beginPath();ctx.arc(...point,7,0,Math.PI*2);ctx.fill();ctx.stroke();}
        ctx.restore();
      }
      return pose;
    }
  }
  scope.LeverRig = LeverRig;
  scope.LeverEase = ease;
  if (typeof module !== 'undefined') module.exports = {LeverRig,ease};
})(typeof window !== 'undefined' ? window : globalThis);
