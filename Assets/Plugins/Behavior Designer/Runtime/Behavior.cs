using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public abstract class Behavior : MonoBehaviour, IBehavior
    {
        public bool startWhenEnabled = true;
        public bool pauseWhenDisabled = false;
        public bool restartWhenComplete = false;
        public bool logTaskChanges = false;
        public int group = 0;
        // reference to an external behavior tree, useful if creating a behavior tree from script
        public ExternalBehavior externalBehavior;

        [SerializeField]
        private BehaviorSource mBehaviorSource;
        public BehaviorSource GetBehaviorSource() { return mBehaviorSource; }
        public void SetBehaviorSource(BehaviorSource behaviorSource) { mBehaviorSource = behaviorSource; }
        public Object GetObject() { return this; }
        public string GetOwnerName() { return gameObject.name; }

        // Remove these variables in a future version:
        [SerializeField]
        private Task entryTask;
        [SerializeField]
        private Task rootTask;
        [SerializeField]
        private List<Task> detachedTasks;
        [SerializeField]
        private List<SharedVariable> variables;
        [SerializeField]
        private string serialization;
        [SerializeField]
        private string behaviorName;
        [SerializeField]
        private string behaviorDescription;

        private bool isPaused = false;

        [SerializeField]
        private List<Object> mUnityObjects;
        public List<Object> UnityObjects { get { return mUnityObjects; } set { mUnityObjects = value; } }
        public void ClearUnityObjects() { if (mUnityObjects != null) mUnityObjects.Clear();}
        public int SerializeUnityObject(Object unityObject)
        {
            if (mUnityObjects == null) {
                mUnityObjects = new List<Object>();
            }

            mUnityObjects.Add(unityObject);
            return mUnityObjects.Count - 1;
        }

        public Object DeserializeUnityObject(int id)
        {
            if (id < 0 || id >= mUnityObjects.Count) {
                return null;
            }

            return mUnityObjects[id];
        }
        
        // coroutines
        private Dictionary<string, List<TaskCoroutine>> activeTaskCoroutines = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public bool showBehaviorDesignerGizmo = true;
#endif

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public void OnDrawGizmosSelected()
        {
            if (showBehaviorDesignerGizmo) {
                Gizmos.DrawIcon(transform.position, "Behavior Designer Scene Icon.png");
            }
        }
#endif

        public Behavior()
        {
            mBehaviorSource = new BehaviorSource(this);
        }
    
        public void Awake()
        {
            if (UpdateDeprecatedTasks()) {
                Debug.LogWarning(string.Format("{0}: the data format for this behavior tree has been deprecated. Run the Behavior Designer Update tool or select this game object within the inspector to update this behavior tree.", ToString()));
            }
        }

        public bool HasDeprecatedTasks()
        {
            return (entryTask != null || (serialization != null && !serialization.Equals("")));
        }

        public bool UpdateDeprecatedTasks()
        {
            if (mBehaviorSource == null) {
                mBehaviorSource = new BehaviorSource(this);
            }
            bool changed = false;
            // move any tasks to the new behavior source:
            if (entryTask != null) {
                mBehaviorSource.EntryTask = entryTask;
                entryTask = null;
                changed = true;
            }
            if (rootTask != null) {
                mBehaviorSource.RootTask = rootTask;
                rootTask = null;
                changed = true;
            }
            if (detachedTasks != null && detachedTasks.Count > 0) {
                mBehaviorSource.DetachedTasks = detachedTasks;
                detachedTasks = null;
                changed = true;
            }
            if (variables != null && variables.Count > 0) {
                mBehaviorSource.Variables = variables;
                variables = null;
                changed = true;
            }
            if (serialization != null && !serialization.Equals("")) {
                mBehaviorSource.Serialization = serialization;
                serialization = null;
                changed = true;
            }
            if (behaviorName != null && !behaviorName.Equals("")) {
                mBehaviorSource.behaviorName = behaviorName;
                behaviorName = "";
            }
            if (behaviorDescription != null && !behaviorDescription.Equals("")) {
                mBehaviorSource.behaviorDescription = behaviorDescription;
                behaviorDescription = "";
            }
            mBehaviorSource.Owner = this;
            return changed;
        }

        public void Start()
        {
            if (startWhenEnabled) {
                EnableBehavior();
            }
        }

        [System.Obsolete("Behavior.enableBehavior has been deprectead. Use Behavior.EnableBehavior.")]
        public void enableBehavior()
        {
            EnableBehavior();
        }

        public void EnableBehavior()
        {
            mBehaviorSource.checkForJSONSerialization();
            if (mBehaviorSource.RootTask != null || externalBehavior != null) {
                // create the behavior manager if it doesn't already exist
                CreateBehaviorManager();
                BehaviorManager.instance.enableBehavior(this);
            }
        }

        [System.Obsolete("Behavior.disableBehavior has been deprectead. Use Behavior.DisableBehavior.")]
        public void disableBehavior()
        {
            DisableBehavior();
        }

        public void DisableBehavior()
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.disableBehavior(this, pauseWhenDisabled);
                isPaused = pauseWhenDisabled;
            }
        }

        public void DisableBehavior(bool pause)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.disableBehavior(this, pause);
                isPaused = pause;
            }
        }

        public void OnEnable()
        {
            if (BehaviorManager.instance != null && isPaused) {
                BehaviorManager.instance.enableBehavior(this);
                isPaused = false;
            }
        }

        public void OnDisable()
        {
            DisableBehavior();
        }
        
        // Support blackboard variables:
        public SharedVariable GetVariable(string name)
        {
            return mBehaviorSource.GetVariable(name);
        }

        public void SetVariable(string name, SharedVariable item)
        {
            mBehaviorSource.SetVariable(name, item);
        }

        // Support collisions/triggers:
        public void OnCollisionEnter(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionEnter(collision, this);
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionExit(collision, this);
            }
        }

        public void OnCollisionStay(Collision collision)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnCollisionStay(collision, this);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerEnter(other, this);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerExit(other, this);
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (BehaviorManager.instance != null) {
                BehaviorManager.instance.BehaviorOnTriggerStay(other, this);
            }
        }

        // ScriptableObjects don't normally support coroutines. Add that support here.
        public void StartTaskCoroutine(Task task, string methodName)
        {
            if (activeTaskCoroutines == null) {
                activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
            }

            var method = task.GetType().GetMethod(methodName);
            var taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[] { }), methodName);
            if (activeTaskCoroutines.ContainsKey(methodName)) {
                var taskCoroutines = activeTaskCoroutines[methodName];
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines[methodName] = taskCoroutines;
            } else {
                var taskCoroutines = new List<TaskCoroutine>();
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines.Add(methodName, taskCoroutines);
            }
        }

        public void StartTaskCoroutine(Task task, string methodName, object value)
        {
            if (activeTaskCoroutines == null) {
                activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
            }
            var method = task.GetType().GetMethod(methodName);
            var taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[] { value }), methodName);
            if (activeTaskCoroutines.ContainsKey(methodName)) {
                var taskCoroutines = activeTaskCoroutines[methodName];
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines[methodName] = taskCoroutines;
            } else {
                var taskCoroutines = new List<TaskCoroutine>();
                taskCoroutines.Add(taskCoroutine);
                activeTaskCoroutines.Add(methodName, taskCoroutines);
            }
        }

        public void StopTaskCoroutine(string methodName)
        {
            if (!activeTaskCoroutines.ContainsKey(methodName)) {
                return;
            }

            var taskCoroutines = activeTaskCoroutines[methodName];
            for (int i = 0; i < taskCoroutines.Count; ++i) {
                taskCoroutines[i].Stop();
            }
        }

        public void StopAllTaskCoroutines()
        {
            StopAllCoroutines();

            foreach (var entry in activeTaskCoroutines) {
                var taskCoroutines = entry.Value;
                for (int i = 0; i < taskCoroutines.Count; ++i) {
                    taskCoroutines[i].Stop();
                }
            }
        }

        public void TaskCoroutineEnded(TaskCoroutine taskCoroutine, string coroutineName)
        {
            if (activeTaskCoroutines.ContainsKey(coroutineName)) {
                var taskCoroutines = activeTaskCoroutines[coroutineName];
                if (taskCoroutines.Count == 1) {
                    activeTaskCoroutines.Remove(coroutineName);
                } else {
                    taskCoroutines.Remove(taskCoroutine);
                    activeTaskCoroutines[coroutineName] = taskCoroutines;
                }
            }
        }

        public override string ToString()
        {
            return mBehaviorSource.ToString();
        }

        public static BehaviorManager CreateBehaviorManager()
        {
            if (BehaviorManager.instance == null) {
                var behaviorManager = new GameObject();
                //behaviorManager.hideFlags = HideFlags.HideAndDontSave;
                behaviorManager.name = "Behavior Manager";
                return behaviorManager.AddComponent<BehaviorManager>();
            }
            return null;
        }
    }
}