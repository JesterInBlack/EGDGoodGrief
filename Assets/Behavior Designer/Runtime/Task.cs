using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public enum TaskStatus
    {
        Inactive,
        Failure,
        Success,
        Running
    }

    public abstract class Task : ScriptableObject
    {
        // OnAwake is called once before the task is executed. Think of it as a constructor
        public virtual void OnAwake() { }

        // OnStart is called immediately before execution. It is used to setup any variables that need to be reset from the previous run
        public virtual void OnStart() { }

        // OnUpdate runs the actual task
        public virtual TaskStatus OnUpdate() { return TaskStatus.Success; }

        // OnEnd is called after execution on a success or failure.
        public virtual void OnEnd() { }

        // OnPause is called when the behavior is paused and resumed
        public virtual void OnPause(bool paused) { }

        // The priority select will need to know this tasks priority of running
        public virtual float GetPriority() { return 0; }

        // OnBehaviorRestart is called after a complete behavior execution and the behavior is going to restart
        public virtual void OnBehaviorRestart() { }

        // OnReset is called by the inspector to reset the public properties
        public virtual void OnReset() { }

        // Same as Editor.OnSceneGUI except this is executed on a runtime class
        public virtual void OnSceneGUI() { }

        // Support coroutines within the task
        protected void StartCoroutine(string methodName) { Owner.startTaskCoroutine(this, methodName); }
        protected void StartCoroutine(System.Collections.IEnumerator routine) { Owner.StartCoroutine(routine); }
        protected void StartCoroutine(string methodName, object value) { Owner.startTaskCoroutine(this, methodName, value); }
        protected void StopCoroutine(string methodName) { Owner.stopTaskCoroutine(methodName); }
        protected void StopAllCoroutines() { Owner.stopAllTaskCoroutines(); }

        // MonoBehaviour components:
        public Animation Animation { set { animation = value; } }
        protected Animation animation;
        public AudioSource Audio { set { audio = value; } }
        protected AudioSource audio;
        public Camera Camera { set { camera = value; } }
        protected Camera camera;
        public Collider Collider { set { collider = value; } }
        protected Collider collider;
#if UNITY_4_3
        public Collider2D Collider2D { set { collider2D = value; } }
        protected Collider2D collider2D;
#endif
        public ConstantForce ConstantForce { set { constantForce = value; } }
        protected ConstantForce constantForce;
        public GameObject GameObject { set { gameObject = value; } }
        protected GameObject gameObject;
        public GUIText GUIText { set { guiText = value; } }
        protected GUIText guiText;
        public GUITexture GUITexture { set { guiTexture = value; } }
        protected GUITexture guiTexture;
        public HingeJoint HingeJoint { set { hingeJoint = value; } }
        protected HingeJoint hingeJoint;
        public Light Light { set { light = value; } }
        protected Light light;
        public NetworkView NetworkView { set { networkView = value; } }
        protected NetworkView networkView;
        public ParticleEmitter ParticleEmitter { set { particleEmitter = value; } }
        protected ParticleEmitter particleEmitter;
        public ParticleSystem ParticleSystem { set { particleSystem = value; } }
        protected ParticleSystem particleSystem;
        public Renderer Renderer { set { renderer = value; } }
        protected Renderer renderer;
        public Rigidbody Rigidbody { set { rigidbody = value; } }
        protected Rigidbody rigidbody;
#if UNITY_4_3
        public Rigidbody2D Rigidbody2D { set { rigidbody2D = value; } }
        protected Rigidbody2D rigidbody2D;
#endif
        public Transform Transform { set { transform = value; } }
        protected Transform transform;

        // NodeData contains properties used by the editor
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        [SerializeField]
        private NodeData nodeData = null;
        public NodeData NodeData { get { return nodeData; } set { nodeData = value; } }
#endif

        // Keep a reference to the behavior that owns this task
        [SerializeField]
        private Behavior owner = null;
        public Behavior Owner { get { return owner; } set { owner = value; } }

        // The unique id of the task
        [SerializeField]
        private int id = -1;
        public int ID { get { return id; } set { id = value; } }

        [SerializeField]
        private bool instant = true;
        public bool IsInstant { get { return instant; } set { instant = value; } }

        private int referenceID = -1;
        public int ReferenceID { get { return referenceID; } set { referenceID = value; } }
    }
}