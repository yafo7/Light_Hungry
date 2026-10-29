using UnityEngine;

namespace LightHungry {
    /// <summary>Absolute debug views: front, opposite, camera-left, camera-right.</summary>
    [DefaultExecutionOrder(-800)]
    [DisallowMultipleComponent]
    public sealed class CameraViewController : MonoBehaviour {
        public static CameraViewController Active { get; private set; }
        public Camera targetCamera;
        public PlayerController player;
        public Vector3 observationCenter = Vector3.zero;
        [Min(20)] public float orbitDistance = 40;
        [Range(.15f, 1)] public float quarterTurnSeconds = .45f;
        [Range(0, 3)] public float lookDistance = 1.1f;
        public int CurrentView { get; private set; } = 1;
        public bool IsRotating { get; private set; }
        public Vector3 ViewRight => Quaternion.Euler(0, Yaw(CurrentView), 0) * Vector3.right;
        public float VisualYaw => visualYaw;
        public int PendingView => pendingView;
        float visualYaw, startYaw, targetYaw, elapsed, duration, look;
        int destination = 1, pendingView;
        Vector3 heldPosition;
        float previousTimeScale = 1;
        bool travelFollowing,settling;int travelTarget=1;
        public void FollowTravelView(int view){travelFollowing=true;settling=false;travelTarget=view;}
        public void EndTravelView(int view,bool smooth=false){travelFollowing=smooth;settling=smooth;travelTarget=view;CurrentView=view;if(!smooth)visualYaw=Yaw(view);pendingView=0;ApplyCamera();}

        public static float Yaw(int view) {
            switch (view) { case 2: return 180; case 3: return 90; case 4: return -90; default: return 0; }
        }

        void Awake() {
            Active = this;
            if (!targetCamera) targetCamera = GetComponent<Camera>();
            if (!targetCamera) targetCamera = Camera.main;
            if (!player) player = FindObjectOfType<PlayerController>();
            if (targetCamera) {
                targetCamera.orthographic = true;
                targetCamera.nearClipPlane = .1f;
                targetCamera.farClipPlane = Mathf.Max(120, orbitDistance * 3);
                targetCamera.transparencySortMode = TransparencySortMode.Perspective;
            }
            ApplyCamera();
        }

        void Update() {
            if(LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()&&LabSession.Instance.GetComponent<LightRouteReview>().IsPlaying)return;
            if(travelFollowing){if(!(LabSession.Instance&&LabSession.Instance.paused))visualYaw=Mathf.MoveTowardsAngle(visualYaw,Yaw(travelTarget),270*Time.deltaTime);if(settling&&Mathf.Abs(Mathf.DeltaAngle(visualYaw,Yaw(travelTarget)))<.01f){travelFollowing=false;settling=false;}return;}
            int requested = ReadViewKey();
            if (requested != 0) RequestView(requested);
            if (IsRotating) {
                // Gameplay time is paused; the camera uses real elapsed time.
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = t * t * (3 - 2 * t);
                visualYaw = Mathf.LerpUnclamped(startYaw, targetYaw, ease);
                if (t >= 1) CompleteRotation();
            } else if (pendingView != 0 && !(LabSession.Instance && LabSession.Instance.paused)) {
                int next = pendingView;
                pendingView = 0;
                RequestView(next);
            }
            float desiredLook = Input.GetKey(KeyCode.W) ? lookDistance : Input.GetKey(KeyCode.S) ? -lookDistance : 0;
            look = Mathf.Lerp(look, desiredLook, 1 - Mathf.Exp(-4 * Time.unscaledDeltaTime));
        }

        void LateUpdate() { if(!(LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()&&LabSession.Instance.GetComponent<LightRouteReview>().IsPlaying))ApplyCamera(); }

        static int ReadViewKey() {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 4;
            return 0;
        }

        public bool RequestView(int view) {
            if(player&&player.GetComponent<LightTravel>()&&player.GetComponent<LightTravel>().IsTravelling)return false;
            if (view < 1 || view > 4) return false;
            if (IsRotating) { pendingView = view == destination ? 0 : view; return true; }
            if (view == CurrentView) { pendingView = 0; return true; }
            if (LabSession.Instance && LabSession.Instance.paused) return false;
            if (!player || !player.Grounded || !player.Body3D) return false;
            heldPosition = player.Body3D.position;
            player.PrepareViewChange();
            destination = view;
            startYaw = visualYaw;
            float delta = Mathf.DeltaAngle(startYaw, Yaw(view));
            targetYaw = startYaw + delta;
            duration = quarterTurnSeconds * Mathf.Sqrt(Mathf.Abs(delta) / 90);
            elapsed = 0;
            previousTimeScale = Time.timeScale;
            IsRotating = true;
            Time.timeScale = 0;
            return true;
        }

        void CompleteRotation() {
            CurrentView = destination;
            visualYaw = Yaw(CurrentView);
            player.SetSpatialPosition(heldPosition);
            player.ApplyViewPlane(ViewRight);
            Physics.SyncTransforms();
            IsRotating = false;
            Time.timeScale = LabSession.Instance && LabSession.Instance.paused ? 0 : previousTimeScale;
        }

        void ApplyCamera() {
            if (!targetCamera) return;
            var rotation = Quaternion.Euler(0, visualYaw, 0);
            targetCamera.transform.SetPositionAndRotation(observationCenter + Vector3.up * look + rotation * (Vector3.back * orbitDistance), rotation);
        }

        void OnDisable() {
            if (IsRotating) Time.timeScale = LabSession.Instance && LabSession.Instance.paused ? 0 : previousTimeScale;
            IsRotating = false;
            pendingView = 0;
            if (Active == this) Active = null;
        }
    }
}
