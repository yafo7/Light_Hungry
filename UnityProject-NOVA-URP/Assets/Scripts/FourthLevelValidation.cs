using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// End-to-end player-input validation. No Transform, Rigidbody position, WarpTo,
// ResetAt or puzzle-state setters are used to advance the route.
public class FourthLevelValidation : MonoBehaviour {
    [Serializable] class Check { public string name; public bool passed; public string detail; }
    [Serializable] class Sample {
        public float t, pixelX, pixelFeetY, charge, mirrorAngle;
        public bool grounded, light, guideDown, seedSettled, plateHeld, gateOpen;
        public string action;
    }
    [Serializable] class Report {
        public bool completed, inputDriven = true;
        public string unityVersion, graphicsDevice;
        public float seconds;
        public int jumps, deaths, transformations;
        public List<Check> checks = new List<Check>();
        public List<string> errors = new List<string>();
        public List<Sample> trace = new List<Sample>();
    }
    readonly Report report = new Report();
    ReservoirLevelFlow f;
    string output, action = "load fourth level";
    float begin, nextTrace;
    bool failed, finished;
    const float PixelsPerUnit = 80f;
    static Vector3 Px(float x, float y) { return new Vector3(x / PixelsPerUnit, (846f - y) / PixelsPerUnit, 0); }
    float PixelX { get { return f.player.Body.position.x * PixelsPerUnit; } }
    float FeetY { get { return 846f - f.player.Body.position.y * PixelsPerUnit; } }

    void Log(string text, string stack, LogType kind) {
        if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) report.errors.Add(text);
    }
    void OnDestroy() { Application.logMessageReceived -= Log; }
    void Update() {
        if (finished) return;
        if (begin > 0 && Time.time - begin > 180) {
            CheckIt("whole-run watchdog", false, "180 seconds elapsed: " + Position());
            Finish(); StopAllCoroutines(); return;
        }
        if (!f || !f.player || !f.mechanism || Time.time < nextTrace) return;
        nextTrace = Time.time + .1f;
        report.trace.Add(new Sample {
            t = Time.time - begin, pixelX = PixelX, pixelFeetY = FeetY,
            charge = f.ability.Charge, mirrorAngle = f.mechanism.MirrorAngle,
            grounded = f.player.Grounded, light = f.ability.IsLight,
            guideDown = f.mechanism.GuideDown, seedSettled = f.mechanism.SeedSettled,
            plateHeld = f.mechanism.PlateHeld, gateOpen = f.mechanism.GateOpen, action = action
        });
    }

    IEnumerator Start() {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-validationOutput");
        output = i >= 0 && i + 1 < args.Length ? args[i + 1] : Application.persistentDataPath + "/Level04Validation";
        Directory.CreateDirectory(output);
        begin = Time.time;
        report.unityVersion = Application.unityVersion;
        report.graphicsDevice = SystemInfo.graphicsDeviceName;
        Application.logMessageReceived += Log;
        yield return Until(() => SceneManager.GetActiveScene().name == "FourthReservoir", 12, "load FourthReservoir");
        f = FindObjectOfType<ReservoirLevelFlow>();
        if (!CheckIt("fourth-level flow exists", f != null, "ReservoirLevelFlow")) { Finish(); yield break; }
        f.Testing = true;
        yield return InputFor(new ReservoirInput(), .15f);
        CheckIt("native graphics", SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11, SystemInfo.graphicsDeviceType.ToString());
        CheckIt("supplied rigged 3D spirit", f.player.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(m => m.sharedMesh && m.sharedMesh.vertexCount > 10000 && m.bones.Length >= 20), "Existing WickSpirit prefab, not an image stand-in");
        CheckIt("global three-second ability", Mathf.Abs(f.ability.chargeSeconds - 3) < .001f && f.ability.rearmOnMovement, "Shared ability retains three-second charge; fourth level allows movement to rearm charging");
        bool nova = f.player.GetComponentsInChildren<ParticleSystemRenderer>(true).Any(r => r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Nova/Particles/UberUnlit");
        CheckIt("NOVA head flame material", nova, "Particle shader = Nova/Particles/UberUnlit");
        CheckIt("third scene retained in build", Application.CanStreamedLevelBeLoaded("ThirdDrain"), "Earlier level remains available");
        if (failed) { Finish(); yield break; }
        yield return Capture("01-overview.png");
        yield return InputFor(new ReservoirInput { skip = true }, .08f);
        yield return Until(() => f.player.Grounded && f.cameraRig.IntroFinished, 5, "spawn on left bank");

        var m = f.mechanism;
        CheckIt("initial puzzle state", !m.GuideDown && !m.SeedSettled && !m.GateOpen && !m.Awakened, "Leaf up; seed above; empty plate; root aperture shut; scale asleep");
        float g = Mathf.Abs(Physics2D.gravity.y) * f.player.Body.gravityScale;
        float maxRise = f.player.jumpSpeed * f.player.jumpSpeed / (2 * g);
        float maxRun = f.player.speed * 2 * f.player.jumpSpeed / g;
        CheckIt("both water gaps exceed unaided range", (644 - 370) / PixelsPerUnit > maxRun && (1415 - 1088) / PixelsPerUnit > maxRun, "Flat range=" + maxRun + "; gaps=3.425,4.0875");
        CheckIt("first ledge within jump rise", (558 - 417) / PixelsPerUnit < maxRise, "First rise=1.7625; ballistic maximum=" + maxRise);
        CheckIt("no invisible bridge over first gap", Physics2D.OverlapBox(Px(500, 570), new Vector2(1.6f, .25f), 0, f.player.groundMask) == null, "Probe under water gap");
        var aperture = (Vector2)Px(1160, 468);
        CheckIt("root slit fits light core", Physics2D.OverlapCircle(aperture, f.ability.coreRadius, f.ability.obstacles) == null, "Core-radius opening");
        CheckIt("root slit blocks full body", Physics2D.OverlapCapsule(aperture, new Vector2(.42f, .94f), CapsuleDirection2D.Vertical, 0, f.player.groundMask) != null, "Full entity does not fit the aperture");
        if (failed) { Finish(); yield break; }

        // First crossing also verifies the global bidirectional and stationary controls.
        yield return Walk(310);
        yield return InputFor(new ReservoirInput(), 1.8f);
        CheckIt("charge is gradual", !f.ability.IsLight && f.ability.Charge > 1.4f && f.ability.Charge < 3, "Charge=" + f.ability.Charge);
        yield return Until(() => f.ability.IsLight, 2, "complete first three-second charge");
        yield return LightMove(460);
        // Submit release through Flow.Update, then allow its following physics tick
        // to consume it before measuring stationary drift. Do not widen tolerance.
        yield return InputFor(new ReservoirInput(), .08f);
        float stationary = PixelX;
        yield return InputFor(new ReservoirInput(), .3f);
        CheckIt("light stops when input stops", Mathf.Abs(PixelX - stationary) < 2, "Pixel drift=" + Mathf.Abs(PixelX - stationary));
        yield return LightMove(410);
        CheckIt("light can move backwards", f.ability.IsLight && PixelX < stationary - 30, "Manual reverse along the same ray");
        yield return DriveUntil(() => !f.ability.IsLight && PixelX > 640, new ReservoirInput { move = 1 }, 5, "first light crossing to middle island");
        yield return Until(() => f.player.Grounded && f.Checkpoint >= 1, 3, "middle-island safe recovery");
        CheckIt("first crossing retains closed gate", !m.GateOpen && !m.SeedSettled, "No seed or plate weight yet");
        yield return Capture("02-middle-arrival.png");

        // Weight revelation: transform in the plate's right standing area.
        yield return Walk(950);
        yield return InputFor(new ReservoirInput(), .12f);
        CheckIt("entity weight opens leaf shutter", m.PlateHeld && m.GateOpen && !m.SeedSettled, "Entity standing on plate");
        yield return Capture("03-entity-weight-opens.png");
        yield return Until(() => f.ability.IsLight, 4, "transform while weighing plate");
        yield return InputFor(new ReservoirInput(), .12f);
        CheckIt("light form loses plate weight", !m.PlateHeld && !m.GateOpen && f.ability.IsLight, "Downstream opening closes; current island ray remains safe");
        yield return Capture("04-weightless-shutter-closed.png");
        yield return TapE();
        yield return Until(() => !f.ability.IsLight && f.player.Grounded, 2, "safe entity recovery on middle island");
        CheckIt("closed gate did not kill player", f.Deaths == 0 && PixelX > 640 && PixelX < 1088, "No forced fall during rule discovery");

        yield return Walk(1030);
        yield return TapE();
        CheckIt("guide leaf lowers from ground handle", m.GuideDown, "E at reachable fixed handle");
        yield return InputFor(new ReservoirInput(), .4f);
        yield return Capture("05-guide-lowered.png");
        yield return Walk(690);
        yield return Jump(690, 417, "jump to first middle-island ledge");
        // Approach the right edge before ascending. From the ledge centre the
        // capsule reaches the upper shelf's left side at head height and is
        // stopped there, then lands on the third ledge instead of the second.
        yield return Walk(738);
        yield return Jump(800, 345, "jump up-right to second ledge");
        yield return Jump(720, 281, "jump back-left to third ledge");
        yield return Jump(820, 237, "jump right into upper operating structure");
        yield return Jump(990, 237, "jump over seed to its right-side operating space");
        yield return Walk(990);
        yield return Capture("06-upper-seed-operation.png");
        yield return TapE();
        CheckIt("seed requires reachable right-side operation", f.PushingSeed, "E engages seed pushing from the upper shelf");
        yield return DriveUntil(() => m.SeedMoving, new ReservoirInput { move = -1 }, 6, "push seed into connected guide chute");
        yield return Until(() => m.SeedSettled, 5, "seed rolls down guide into plate cup");
        yield return InputFor(new ReservoirInput(), .2f);
        CheckIt("seed holds plate independently", m.SeedSettled && m.PlateHeld && m.GateOpen && m.GuideDown, "Entity remains upstairs, seed maintains pressure");
        bool truncated = !m.crossingPath.isActiveAndEnabled || m.crossingPath.nodes == null || m.crossingPath.nodes.Length == 0 || m.crossingPath.nodes[m.crossingPath.nodes.Length - 1].x < 1100 / PixelsPerUnit;
        CheckIt("lowered transport leaf still blocks light", truncated && !m.Aligned && !m.Awakened, "Weight alone is insufficient; guide must be folded away");
        yield return Capture("07-seed-held-but-guide-blocks.png");

        // Descend by stepping from the upper shelf onto the island; no test teleport.
        yield return Walk(1067);
        yield return Until(() => f.player.Grounded && Mathf.Abs(FeetY - 558) < 12, 4, "return from upper operating shelf to middle ground");
        yield return Walk(850);
        yield return Jump(700, 417, "jump over settled seed cup onto first ledge");
        yield return Walk(651);
        int beforeDeath = f.Deaths;
        action = "deliberate failed unaided water jump";
        f.testInput = new ReservoirInput { move = -1, jump = true };
        yield return null;
        yield return DriveUntil(() => f.Deaths > beforeDeath, new ReservoirInput { move = -1 }, 5, "water failure respawns on middle checkpoint");
        yield return InputFor(new ReservoirInput(), .35f);
        CheckIt("water failure is recoverable", f.Deaths == beforeDeath + 1 && f.Checkpoint == 1 && f.player.Grounded, "Automatic game respawn; no validation warp");
        CheckIt("checkpoint preserves settled seed", m.SeedSettled && m.GuideDown && m.GateOpen, "Puzzle progress survives a platforming mistake");

        yield return Jump(850, 558, "jump over settled seed after checkpoint recovery");
        yield return Walk(1030);
        yield return TapE();
        yield return InputFor(new ReservoirInput(), .25f);
        CheckIt("folding guide restores continuous ray", !m.GuideDown && m.SeedSettled && m.GateOpen && m.crossingPath.enabled && m.crossingPath.nodes.Last().x > 1400 / PixelsPerUnit, "Seed remains in independent cup while leaf rotates away");
        yield return Capture("08-stable-crossing-ray.png");
        yield return Walk(950);
        yield return Until(() => f.ability.IsLight, 4, "second stable crossing charge");
        yield return LightMove(1160);
        CheckIt("light traverses narrow root aperture", f.ability.IsLight && m.GateOpen && m.SeedSettled, "Core inside root opening; seed still weighs plate");
        yield return Capture("09-light-in-root-aperture.png");
        yield return DriveUntil(() => !f.ability.IsLight && PixelX > 1415, new ReservoirInput { move = 1 }, 4, "safe recovery on right bank");
        yield return Until(() => f.player.Grounded && f.Checkpoint >= 2, 3, "right-bank checkpoint");
        CheckIt("crossing alone does not finish level", !f.Completed && !m.Awakened, "Final mirror still must feed the scale");

        yield return Walk(1490);
        yield return TapE();
        CheckIt("right mirror can be adjusted", f.Adjusting, "E at reachable mirror pivot");
        float target = m.TargetMirrorAngle;
        action = "aim terminal mirror at dark feeding scale";
        float deadline = Time.time + 5;
        while (!m.Aligned && Time.time < deadline && !failed) {
            f.testInput = new ReservoirInput { adjust = Mathf.Sign(target - m.MirrorAngle) };
            yield return null;
        }
        f.testInput = new ReservoirInput();
        CheckIt("reflected ray geometrically hits scale", m.Aligned, "Mirror tangent=" + m.MirrorAngle + ", expected vicinity=" + target);
        if (failed) { yield return Capture("FAIL-mirror-alignment.png"); Finish(); yield break; }
        yield return Until(() => m.Awakened, 4, "continuous exposure awakens belly scale");
        CheckIt("feeding uses sustained light", m.Exposure >= 1.95f, "Exposure=" + m.Exposure);
        CheckIt("feeding alone does not exit", !f.Completed, "Player must approach pump-house passage");
        yield return Capture("10-scale-awakened.png");
        yield return TapE();
        yield return DriveUntil(() => f.Completed, new ReservoirInput { move = 1 }, 6, "walk into pump-house entrance");
        yield return Capture("11-fourth-completed.png");
        CheckIt("complete input-driven fourth-level route", f.Completed && !failed, "No validation state setters or teleports; only normal checkpoint respawn");
        report.jumps = f.player.JumpCount;
        report.deaths = f.Deaths;
        report.transformations = f.ability.TransformCount;

        var previous = f;
        f.testInput = new ReservoirInput { restart = true };
        yield return null;
        yield return Until(() => FindObjectOfType<ReservoirLevelFlow>() != previous && !SceneFade.Busy, 10, "R reloads fourth level");
        f = FindObjectOfType<ReservoirLevelFlow>();
        CheckIt("restart resets progress", f && !f.Completed && !f.mechanism.SeedSettled && !f.mechanism.GuideDown && !f.mechanism.Awakened && f.Checkpoint == 0, "Seed returns upstairs, guide up, scale dormant");
        yield return null;
        Finish();
    }

    bool CheckIt(string name, bool ok, string detail) {
        report.checks.Add(new Check { name = name, passed = ok, detail = detail });
        if (!ok) failed = true;
        Debug.Log("L4_CHECK " + name + "=" + ok + " " + detail);
        return ok;
    }
    IEnumerator Until(Func<bool> condition, float timeout, string label) {
        action = label;
        float end = Time.time + timeout;
        while (!condition() && Time.time < end && !failed) yield return null;
        if (!condition() || failed) {
            if (!failed) CheckIt(label, false, "Timeout; " + Position());
            yield return Capture("FAIL-" + report.checks.Count + ".png");
            Finish(); StopAllCoroutines();
        }
    }
    IEnumerator DriveUntil(Func<bool> condition, ReservoirInput input, float timeout, string label) {
        action = label;
        float end = Time.time + timeout;
        while (!condition() && Time.time < end && !failed) { f.testInput = input; yield return null; }
        f.testInput = new ReservoirInput();
        if (!condition() || failed) {
            if (!failed) CheckIt(label, false, "Timeout; " + Position());
            yield return Capture("FAIL-" + report.checks.Count + ".png");
            Finish(); StopAllCoroutines();
        }
    }
    IEnumerator InputFor(ReservoirInput input, float seconds) {
        float end = Time.time + seconds;
        while (Time.time < end && !failed) { if (f) f.testInput = input; yield return null; }
        if (f) f.testInput = new ReservoirInput();
    }
    IEnumerator TapE() {
        f.testInput = new ReservoirInput { interact = true };
        yield return null;
        yield return InputFor(new ReservoirInput(), .1f);
    }
    IEnumerator Walk(float px) {
        action = "walk to " + px;
        float end = Time.time + 10;
        while (Mathf.Abs(PixelX - px) > 4 && Time.time < end && !failed) {
            f.testInput = new ReservoirInput { move = Mathf.Sign(px - PixelX) };
            yield return null;
        }
        f.testInput = new ReservoirInput();
        if (Mathf.Abs(PixelX - px) > 12 || failed) {
            if (!failed) CheckIt(action, false, Position());
            yield return Capture("FAIL-walk-" + px + ".png");
            Finish(); StopAllCoroutines();
        }
        yield return InputFor(new ReservoirInput(), .08f);
    }
    IEnumerator LightMove(float px) {
        action = "light travel to " + px;
        float end = Time.time + 6;
        while (f.ability.IsLight && Mathf.Abs(PixelX - px) > 5 && Time.time < end && !failed) {
            f.testInput = new ReservoirInput { move = Mathf.Sign(px - PixelX) };
            yield return null;
        }
        f.testInput = new ReservoirInput();
        if (!f.ability.IsLight || Mathf.Abs(PixelX - px) > 14 || failed) {
            if (!failed) CheckIt(action, false, Position());
            yield return Capture("FAIL-light-travel.png");
            Finish(); StopAllCoroutines();
        }
    }
    IEnumerator Jump(float px, float top, string label) {
        yield return Until(() => f.player.Grounded, 3, "ground before " + label);
        action = label;
        int previousJumps = f.player.JumpCount, previousDeaths = f.Deaths;
        float end = Time.time + 3;
        f.testInput = new ReservoirInput { move = Mathf.Abs(px - PixelX) > 4 ? Mathf.Sign(px - PixelX) : 0, jump = true };
        yield return null;
        bool airborne = false;
        while (Time.time < end && !failed) {
            f.testInput = new ReservoirInput { move = Mathf.Abs(px - PixelX) > 4 ? Mathf.Sign(px - PixelX) : 0 };
            if (!f.player.Grounded) airborne = true;
            if (airborne && f.player.Grounded && f.player.Body.velocity.y < 1 && f.player.JumpCount > previousJumps) break;
            if (f.Deaths != previousDeaths) break;
            yield return null;
        }
        f.testInput = new ReservoirInput();
        bool landed = airborne && f.player.Grounded && f.Deaths == previousDeaths && Mathf.Abs(PixelX - px) < 25 && Mathf.Abs(FeetY - top) < 15;
        CheckIt(label, landed, Position() + "; expected surface y=" + top);
        if (!landed) { yield return Capture("FAIL-jump-" + report.checks.Count + ".png"); Finish(); StopAllCoroutines(); }
        yield return InputFor(new ReservoirInput(), .08f);
    }
    IEnumerator Capture(string filename) {
        yield return new WaitForEndOfFrame();
        NativeFrameCapture.Save(Path.Combine(output, filename));
        // URP capture is fulfilled on the next beginContextRendering callback.
        yield return null;
        yield return new WaitForEndOfFrame();
    }
    string Position() {
        return f ? "x=" + PixelX.ToString("F1") + ", feetY=" + FeetY.ToString("F1") + ", grounded=" + f.player.Grounded + ", light=" + f.ability.IsLight + ", deaths=" + f.Deaths : "Flow missing";
    }
    void Finish() {
        if (finished) return;
        finished = true;
        if (f) f.testInput = new ReservoirInput();
        report.seconds = Time.time - begin;
        report.completed = !failed && report.errors.Count == 0 && report.checks.Any(c => c.name == "restart resets progress" && c.passed);
        File.WriteAllText(Path.Combine(output, "playthrough.json"), JsonUtility.ToJson(report, true));
        Application.Quit(report.completed ? 0 : 1);
    }
}
