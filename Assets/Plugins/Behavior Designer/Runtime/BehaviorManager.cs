using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class BehaviorManager : MonoBehaviour
    {
        static public BehaviorManager instance;

        public enum UpdateIntervalType { EveryFrame, SpecifySeconds, Manual }
        private UpdateIntervalType updateInterval = UpdateIntervalType.EveryFrame;
        public UpdateIntervalType UpdateInterval { get { return updateInterval; } set { updateInterval = value; } }
        private float updateIntervalSeconds = 0;
        public float UpdateIntervalSeconds { get { return updateIntervalSeconds; } set { updateIntervalSeconds = value; } }
        private WaitForSeconds updateWait = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public delegate void TaskBreakpointHandler();
        public event TaskBreakpointHandler onTaskBreakpoint;
#endif

        // one behavior tree for each client
        public class BehaviorTree
        {
            public List<Task> taskList;
            public List<int> parentIndex;
            public List<List<int>> childrenIndex;
            // the relative child index is the index relative to the parent. For example, the first child has a relative child index of 0
            public List<int> relativeChildIndex;
            public List<Stack<int>> activeStack;
            public List<TaskStatus> nonInstantTaskStatus;
            public List<int> interruptionIndex;
            public Behavior behavior;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            public List<Task> originalTaskList;
            public List<int> originalIndex;
#endif
        }
        private List<BehaviorTree> behaviorTrees = new List<BehaviorTree>();
        private Dictionary<Behavior, BehaviorTree> pausedBehaviorTrees = new Dictionary<Behavior, BehaviorTree>();
        private Dictionary<Behavior, BehaviorTree> behaviorTreeMap = new Dictionary<Behavior, BehaviorTree>();
        // PlayMaker / uScript support
        private Dictionary<object, BehaviorTree> objectTreeMap = new Dictionary<object, BehaviorTree>();
        private Dictionary<BehaviorTree, object> treeObjectMap = new Dictionary<BehaviorTree, object>();

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        private bool atBreakpoint = false;
        public bool AtBreakpoint { get { return atBreakpoint; } set { atBreakpoint = value; } }
#endif

        public void Awake()
        {
            instance = this;

            if (updateInterval != UpdateIntervalType.EveryFrame) {
                startCoroutineUpdate();
                enabled = false;
            }
        }

        public void startCoroutineUpdate()
        {
            if (updateInterval == UpdateIntervalType.SpecifySeconds) {
                updateWait = new WaitForSeconds(updateIntervalSeconds);
                StartCoroutine("coroutineUpdate");
            }
        }

        public void stopCoroutineUpdate()
        {
            StopCoroutine("coroutineUpdate");
        }

        public void enableBehavior(Behavior behavior)
        {
            BehaviorTree behaviorTree;
            if (isBehaviorEnabled(behavior)) {
                // unpause
                if (pausedBehaviorTrees.ContainsKey(behavior)) {
                    behaviorTree = pausedBehaviorTrees[behavior];
                    behaviorTrees.Add(behaviorTree);
                    pausedBehaviorTrees.Remove(behavior);

                    for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                        behaviorTree.taskList[i].OnPause(false);
                    }
                }
                return;
            }
            if (behavior.GetBehaviorSource().RootTask == null) {
                Debug.LogError(string.Format("The behavior \"{0}\" on Game Object \"{1}\" contains no root task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                return;
            }

            behaviorTree = new BehaviorTree();
            behaviorTree.taskList = new List<Task>();
            behaviorTree.behavior = behavior;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            behaviorTree.originalTaskList = new List<Task>();
            behaviorTree.originalIndex = new List<int>();
#endif
            behaviorTree.parentIndex = new List<int>();
            behaviorTree.childrenIndex = new List<List<int>>();
            behaviorTree.relativeChildIndex = new List<int>();
            behaviorTree.parentIndex.Add(-1); // add the first entry for the root task
            behaviorTree.relativeChildIndex.Add(-1);
            bool hasExternalBehavior = false;
            int status = addToTaskList(behaviorTree, behavior.GetBehaviorSource().RootTask, ref hasExternalBehavior, false, null, null);
            if (status < 0) {
                // something is wrong with the tree. Don't go any further
                behaviorTree = null;
                switch (status) {
                    case -1:
                        Debug.LogError(string.Format("The behavior \"{0}\" on Game Object \"{1}\" is invalid. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -2:
                        Debug.LogError(string.Format("The behavior \"{0}\" on Game Object \"{1}\" cannot find the referenced external task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                }
                return;
            }

            if (hasExternalBehavior) {
                // Make sure all of the tasks are pointing to other tasks within the same tree
                TaskReferences.CheckReferences(behaviorTree.behavior, behaviorTree.taskList);
            }

            behaviorTree.activeStack = new List<Stack<int>>();
            behaviorTree.interruptionIndex = new List<int>();
            behaviorTree.nonInstantTaskStatus = new List<TaskStatus>();
            // add the first entry
            behaviorTree.activeStack.Add(new Stack<int>());
            behaviorTree.interruptionIndex.Add(-1);
            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behaviorTree.behavior.logTaskChanges) {
                for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                    Debug.Log(string.Format("{0}: Task {1} (index {2})", roundedTime(), behaviorTree.taskList[i].GetType(), i));
                }
            }
#endif

            // set the MonoBehavior components
            Animation clientAnimation = behavior.animation;
            AudioSource clientAudio = behavior.audio;
            Camera clientCamera = behavior.camera;
            Collider clientCollider = behavior.collider;
#if UNITY_4_3
            Collider2D clientCollider2D = behavior.collider2D;
#endif
            ConstantForce clientConstantForce = behavior.constantForce;
            GameObject clientGameObject = behavior.gameObject;
            GUIText clientGUIText = behavior.guiText;
            GUITexture clientGUITexture = behavior.guiTexture;
            HingeJoint clientHingeJoint = behavior.hingeJoint;
            Light clientLight = behavior.light;
#if !UNITY_WINRT
            NetworkView clientNetworkView = behavior.networkView;
#endif
            ParticleEmitter clientParticleEmitter = behavior.particleEmitter;
            ParticleSystem clientParticleSystem = behavior.particleSystem;
            Renderer clientRenderer = behavior.renderer;
            Rigidbody clientRigidbody = behavior.rigidbody;
#if UNITY_4_3
            Rigidbody2D clientRigidbody2D = behavior.rigidbody2D;
#endif
            Transform clientTransform = behavior.transform;

            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                // Assign the mono behavior components
                behaviorTree.taskList[i].Animation = clientAnimation;
                behaviorTree.taskList[i].Audio = clientAudio;
                behaviorTree.taskList[i].Camera = clientCamera;
                behaviorTree.taskList[i].Collider = clientCollider;
#if UNITY_4_3
                behaviorTree.taskList[i].Collider2D = clientCollider2D;
#endif
                behaviorTree.taskList[i].ConstantForce = clientConstantForce;
                behaviorTree.taskList[i].GameObject = clientGameObject;
                behaviorTree.taskList[i].GUIText = clientGUIText;
                behaviorTree.taskList[i].GUITexture = clientGUITexture;
                behaviorTree.taskList[i].HingeJoint = clientHingeJoint;
                behaviorTree.taskList[i].Light = clientLight;
#if !UNITY_WINRT
                behaviorTree.taskList[i].NetworkView = clientNetworkView;
#endif
                behaviorTree.taskList[i].ParticleEmitter = clientParticleEmitter;
                behaviorTree.taskList[i].ParticleSystem = clientParticleSystem;
                behaviorTree.taskList[i].Renderer = clientRenderer;
                behaviorTree.taskList[i].Rigidbody = clientRigidbody;
#if UNITY_4_3
                behaviorTree.taskList[i].Rigidbody2D = clientRigidbody2D;
#endif
                behaviorTree.taskList[i].Transform = clientTransform;
                behaviorTree.taskList[i].Owner = behaviorTree.behavior;

                behaviorTree.taskList[i].OnAwake();
            }

            // the behavior tree is ready to go
            behaviorTrees.Add(behaviorTree);
            behaviorTreeMap.Add(behavior, behaviorTree);

            // start with the first index
            pushTask(behaviorTree, 0, 0);
        }

        // returns 0 for success
        // returns -1 if a parent doesn't have any children
        // returns -2 if the external task cannot be found
        public int addToTaskList(BehaviorTree behaviorTree, Task task, ref bool hasExternalBehavior, bool fromExternalTask, Dictionary<string, object> sharedVariables, Dictionary<string, object> inheritedFields)
        {
#pragma warning disable 0618
            if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                task.GetType() == typeof(BehaviorReference) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.BehaviorReference))) {
                BehaviorSource behaviorSource = null;
                if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior))) {
                    Debug.LogWarning(string.Format("{0}: The External Behavior Tree task has been deprecated. Use the Behavior Tree Reference task instead.", behaviorTree.behavior.ToString()));
                    BehaviorDesigner.Runtime.Tasks.ExternalBehavior externalBehaviorTask = task as BehaviorDesigner.Runtime.Tasks.ExternalBehavior;
                    if (externalBehaviorTask != null && externalBehaviorTask.externalTask != null) {
                        var behavior = externalBehaviorTask.externalTask.GetComponent<Behavior>();
                        if (behavior != null) {
                            behavior = UnityEngine.Object.Instantiate(behavior) as Behavior;
                            behaviorSource = behavior.GetBehaviorSource();
                        } else {
                            return -2;
                        }
                    } else {
                        return -2;
                    }
#pragma warning restore 0618
                } else {
                    var behaviorReference = task as BehaviorReference;
                    if (behaviorReference != null) {
                        if (behaviorReference.getExternalBehavior() != null) {
                            behaviorSource = behaviorReference.getExternalBehavior().BehaviorSource;
                        } else {
                            return -2;
                        }
                    } else {
                        return -2;
                    }
                }
                if (behaviorSource != null) {
                    if ((behaviorSource.Owner as Behavior) != null && (behaviorSource.Owner as Behavior).updateDeprecatedTasks()) {
                        Debug.LogWarning(string.Format("{0}: the data format for this behavior tree has been deprecated. Run the Behavior Designer Update tool or select this game object within the inspector to update this behavior tree.", ToString()));
                    }
                    // make sure the behavior source is properly serialized
                    behaviorSource.checkForJSONSerialization();
                    var externalRootTask = behaviorSource.RootTask;
                    if (externalRootTask != null) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        if (!fromExternalTask) {
                            behaviorTree.originalTaskList.Add(task);
                        }
#endif
                        // bring the external behavior trees variables into the parent behavior tree
                        if (behaviorSource.Variables != null) {
                            for (int i = 0; i < behaviorSource.Variables.Count; ++i) {
                                // only set the behavior tree variable if it doesn't already exist in the parent behavior tree
                                if (behaviorTree.behavior.GetVariable(behaviorSource.Variables[i].name) == null) {
                                    // create a new instance of the variable to prevent overwriting the value
                                    var localSharedVariable = ScriptableObject.CreateInstance(behaviorSource.Variables[i].GetType()) as SharedVariable;
                                    localSharedVariable.name = behaviorSource.Variables[i].name;
                                    localSharedVariable.SetValue(behaviorSource.Variables[i].GetValue());
                                    behaviorTree.behavior.SetVariable(localSharedVariable.name, localSharedVariable);
                                    // add it to the shared variables dictionary
                                    if (sharedVariables == null) {
                                        sharedVariables = new Dictionary<string, object>();
                                    }
                                    if (!sharedVariables.ContainsKey(localSharedVariable.name)) {
                                        sharedVariables.Add(localSharedVariable.name, localSharedVariable);
                                    }
                                }
                            }
                        }

                        // find all of the inherited fields
                        var fields = task.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                        for (int i = 0; i < fields.Length; ++i) {
                            if (fields[i].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0) {
                                if (inheritedFields == null) {
                                    inheritedFields = new Dictionary<string, object>();
                                }
                                if (!inheritedFields.ContainsKey(fields[i].Name)) {
                                    inheritedFields.Add(fields[i].Name, fields[i].GetValue(task));
                                }
                            }
                        }
                        hasExternalBehavior = true;
                        int status = 0;
                        if ((status = addToTaskList(behaviorTree, externalRootTask, ref hasExternalBehavior, true, sharedVariables, inheritedFields)) < 0) {
                            return status;
                        }
                    } else {
                        return -2;
                    }
                } else {
                    return -2;
                }
            } else {
                // If the task is coming from an external behavior than create a new scriptable object. This is done to
                // keep all of the properties local and not override a prefab with any property values.
                if (fromExternalTask) {
                    var localTask = ScriptableObject.CreateInstance(task.GetType()) as Task;
                    var fields = task.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; ++i) {
                        var value = fields[i].GetValue(task);
                        if (inheritedFields != null && inheritedFields.ContainsKey(fields[i].Name) && fields[i].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0) {
                            fields[i].SetValue(localTask, inheritedFields[fields[i].Name]);
                        } else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)) && sharedVariables != null && value != null && sharedVariables.ContainsKey((value as SharedVariable).name)) {
                            fields[i].SetValue(localTask, sharedVariables[(value as SharedVariable).name]);
                        } else {
                            fields[i].SetValue(localTask, value);
                        }
                    }
                    localTask.ReferenceID = task.ReferenceID = behaviorTree.taskList.Count;
                    localTask.ID = task.ID;
                    localTask.Owner = task.Owner;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    localTask.NodeData = new NodeData();
                    if (fromExternalTask) {
                        localTask.NodeData.NodeDesigner = null;
                    } else {
                        localTask.NodeData.copyFrom(task.NodeData);
                    }
#endif
                    behaviorTree.taskList.Add(localTask);
                    task = localTask;
                } else {
                    task.ReferenceID = behaviorTree.taskList.Count;
                    behaviorTree.taskList.Add(task);
                }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.originalTaskList.Count == 0) {
                    behaviorTree.originalTaskList.Add(task);
                    behaviorTree.originalIndex.Add(0);
                } else {
                    if (!fromExternalTask) {
                        behaviorTree.originalTaskList.Add(task);
                    }
                    behaviorTree.originalIndex.Add(behaviorTree.originalTaskList.Count - 1);
                }
#endif
                if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                    var localParentTask = task as ParentTask;
                    if (localParentTask.Children.Count == 0) {
                        return -1; // invalid tree
                    }
                    int status;
                    int parentIndex = behaviorTree.taskList.Count - 1;
                    behaviorTree.childrenIndex.Add(new List<int>());
                    for (int i = 0; i < localParentTask.Children.Count; ++i) {
                        behaviorTree.parentIndex.Add(parentIndex);
                        behaviorTree.relativeChildIndex.Add(i);
                        behaviorTree.childrenIndex[parentIndex].Add(behaviorTree.taskList.Count);
                        if ((status = addToTaskList(behaviorTree, localParentTask.Children[i], ref hasExternalBehavior, fromExternalTask, sharedVariables, inheritedFields)) < 0) {
                            return status;
                        }
                    }
                } else {
                    behaviorTree.childrenIndex.Add(null);
                }
            }
            return 0;
        }

        public void disableBehavior(Behavior behavior)
        {
            disableBehavior(behavior, false);
        }

        public void disableBehavior(Behavior behavior, bool paused)
        {
            if (!isBehaviorEnabled(behavior)) {
                return;
            }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behavior.logTaskChanges) {
                Debug.Log(string.Format("{0}: {1} {2}", roundedTime(), (paused ? "Pausing" : "Disabling"), behavior.ToString()));
            }
#endif

            var behaviorTree = behaviorTreeMap[behavior];
            if (paused) {
                if (!pausedBehaviorTrees.ContainsKey(behavior)) {
                    pausedBehaviorTrees.Add(behavior, behaviorTree);

                    for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                        behaviorTree.taskList[i].OnPause(true);
                    }
                }
            } else {
                // pop all of the tasks so they receive the end callback
                var status = TaskStatus.Success;
                for (int i = behaviorTree.activeStack.Count - 1; i > -1; --i) {
                    while (behaviorTree.activeStack[i].Count > 0) {
                        int stackCount = behaviorTree.activeStack[i].Count;
                        popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref status, true, false);
                        if (stackCount == 1) {
                            break;
                        }
                    }
                }
                behaviorTreeMap.Remove(behavior);
            }
            behaviorTrees.Remove(behaviorTree);
        }

        public void restartBehavior(Behavior behavior)
        {
            if (!isBehaviorEnabled(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            // pop all of the tasks so they receive the end callback
            var status = TaskStatus.Success;
            for (int i = behaviorTree.activeStack.Count - 1; i > -1; --i) {
                while (behaviorTree.activeStack[i].Count > 0) {
                    int stackCount = behaviorTree.activeStack[i].Count;
                    popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref status, true, false);
                    if (stackCount == 1) {
                        break;
                    }
                }
            }

            // start things again
            restart(behaviorTree);
        }

        public bool isBehaviorEnabled(Behavior behavior)
        {
            return behaviorTreeMap != null && behavior != null && behaviorTreeMap.ContainsKey(behavior);
        }

        public void Update()
        {
            tick();
        }

        private IEnumerator coroutineUpdate()
        {
            while (true) {
                tick();
                yield return updateWait;
            }
        }

        public void tick()
        {
            for (int i = 0; i < behaviorTrees.Count; ++i) {
                var behaviorTree = behaviorTrees[i];
                for (int j = behaviorTree.activeStack.Count - 1; j > -1; --j) {
                    // ensure there are no interruptions within the hierarchy
                    var status = TaskStatus.Inactive;
                    int interruptedTask;
                    if (j < behaviorTree.interruptionIndex.Count && (interruptedTask = behaviorTree.interruptionIndex[j]) != -1) {
                        behaviorTree.interruptionIndex[j] = -1;
                        while (behaviorTree.activeStack[j].Peek() != interruptedTask) {
                            int stackCount = behaviorTree.activeStack[j].Count;
                            popTask(behaviorTree, behaviorTree.activeStack[j].Peek(), j, ref status, true);
                            if (stackCount == 1) {
                                break;
                            }
                        }
                        // pop the interrupt task. Performing a check to be sure the interrupted task is at the top of the stack because the interrupted task
                        // may be in a different stack and the active stack has completely been removed
                        if (j < behaviorTree.activeStack.Count && behaviorTree.taskList[interruptedTask] == behaviorTree.taskList[behaviorTree.activeStack[j].Peek()]) {
                            status = (behaviorTree.taskList[interruptedTask] as ParentTask).OverrideStatus();
                            popTask(behaviorTree, interruptedTask, j, ref status, true);
                        }
                    }
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    int count = 0;
#endif
                    int startIndex = -1;
                    int taskIndex;
                    while (status != TaskStatus.Running && j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0) {
                        taskIndex = behaviorTree.activeStack[j].Peek();
                        // bail out if the index is the same as what it was before runTask was executed or the behavior is no longer enabled
                        if ((j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0 && startIndex == behaviorTree.activeStack[j].Peek()) || !isBehaviorEnabled(behaviorTree.behavior)) {
                            break;
                        } else {
                            startIndex = taskIndex;
                        }
                        status = runTask(behaviorTree, taskIndex, j, status);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        // While in the editor make sure we aren't in an infinite loop
                        if (++count > behaviorTree.taskList.Count) {
                            Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
                            disableBehavior(behaviorTree.behavior);
                            break;
                        }
#endif
                    }
                }
            }
        }

        private TaskStatus runTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, TaskStatus previousStatus)
        {
            var task = behaviorTree.taskList[taskIndex];
            if (task == null)
                return previousStatus;

            var status = previousStatus;
            // If the task is non instant and the task has already completed executing then pop the task
            if (!task.IsInstant && (behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Failure || behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Success)) {
                status = behaviorTree.nonInstantTaskStatus[stackIndex];
                popTask(behaviorTree, taskIndex, stackIndex, ref status, true);
                return status;
            }
            pushTask(behaviorTree, taskIndex, stackIndex);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (atBreakpoint) {
                return TaskStatus.Running;
            }
#endif

            if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                var parentTask = task as ParentTask;
                if (!parentTask.CanRunParallelChildren() || parentTask.OverrideStatus(TaskStatus.Running) != TaskStatus.Running) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    int count = 0;
#endif
                    var childStatus = TaskStatus.Inactive;
                    // nest within a while loop so multiple child tasks can be run within a single update loop (such as conditions)
                    // also, if the parent is a parallel task, start running all of the children
                    int parentStack = stackIndex;
                    int startChildIndex = -1;
                    while (parentTask.CanExecute() && (childStatus != TaskStatus.Running || parentTask.CanRunParallelChildren())) {
                        List<int> childrenIndexes = behaviorTree.childrenIndex[taskIndex];
                        int childIndex = parentTask.CurrentChildIndex();
                        // bail out if the child index is the same as what it was before runTask was executed
                        if ((childIndex == startChildIndex && status != TaskStatus.Running) || !isBehaviorEnabled(behaviorTree.behavior)) {
                            status = TaskStatus.Running;
                            break;
                        } else if (startChildIndex == -1) { // remember the first child index
                            startChildIndex = childIndex;
                        }
                        if (parentTask.CanRunParallelChildren()) {
                            // need to create a new stack level
                            behaviorTree.activeStack.Add(new Stack<int>());
                            behaviorTree.interruptionIndex.Add(-1);
                            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
                            stackIndex = behaviorTree.activeStack.Count - 1;
                            parentTask.OnChildRunning(childIndex);
                        } else {
                            parentTask.OnChildRunning();
                        }
                        status = childStatus = runTask(behaviorTree, childrenIndexes[childIndex], stackIndex, status);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                        // While in the editor make sure we aren't in an infinite loop
                        if (++count > behaviorTree.taskList.Count) {
                            Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
                            disableBehavior(behaviorTree.behavior);
                            break;
                        }
#endif
                    }
                    stackIndex = parentStack;
                }
                // let the parent task override the children status. The last child task could fail immediately and we don't want that to represent the entire task
                status = parentTask.OverrideStatus(status);
            } else {
                status = task.OnUpdate();
            }

            if (status != TaskStatus.Running) {
                // pop the task immediately if the task is instant. If the task is not instant then wait for the next update
                if (task.IsInstant) {
                    popTask(behaviorTree, taskIndex, stackIndex, ref status, true);
                } else {
                    behaviorTree.nonInstantTaskStatus[stackIndex] = status;
                }
            }

            return status;
        }

        private void pushTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex)
        {
            if (!isBehaviorEnabled(behaviorTree.behavior)) {
                return;
            }

            if (behaviorTree.activeStack[stackIndex].Count == 0 || behaviorTree.activeStack[stackIndex].Peek() != taskIndex) {
                behaviorTree.activeStack[stackIndex].Push(taskIndex);
                behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Running;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                // update the referenced task if this task is an external task
                var type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
#pragma warning disable 0618
                if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                    type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference))) {
                    // this task needs to be the first external task
                    int parentIndex = behaviorTree.parentIndex[taskIndex];
                    if (parentIndex != -1) {
                        type = behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]].GetType();
                    }
                    if (parentIndex == -1 || (!type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) &&
                        !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference)))) {
                            behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = Time.realtimeSinceStartup;
                    }
                } else {
                    behaviorTree.taskList[taskIndex].NodeData.PushTime = Time.realtimeSinceStartup;
                }
                if (behaviorTree.taskList[taskIndex].NodeData.IsBreakpoint) {
                    atBreakpoint = true;
                    // let behavior designer know
                    if (onTaskBreakpoint != null) {
                        onTaskBreakpoint();
                    }
                }

                if (behaviorTree.behavior.logTaskChanges) {
                    print(string.Format("{0}: {1}: Push task {2} (index {3}) at stack index {4}", roundedTime(), behaviorTree.behavior.ToString(), behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex));
                }
#endif
                behaviorTree.taskList[taskIndex].OnStart();
            }
        }

        private void popTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren)
        {
            popTask(behaviorTree, taskIndex, stackIndex, ref status, popChildren, true);
        }

        private void popTask(BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren, bool notifyOnEmptyStack)
        {
            if (!isBehaviorEnabled(behaviorTree.behavior)) {
                return;
            }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (taskIndex != behaviorTree.activeStack[stackIndex].Peek()) {
                print("error: popping " + taskIndex + " but " + behaviorTree.activeStack[stackIndex].Peek() + " is on top");
            }
#endif

            behaviorTree.activeStack[stackIndex].Pop();
            behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Inactive;
            behaviorTree.taskList[taskIndex].OnEnd();

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // update the referenced task if this task is an external task
            var type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
#pragma warning disable 0618
            if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||  type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || 
                type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference))) {
                // this task needs to be the first external task
                int parentIndex = behaviorTree.parentIndex[taskIndex];
                if (parentIndex != -1) {
                    type = behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]].GetType();
                }
                if (parentIndex == -1 || !type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && 
                    !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference))) {
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = -1;
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PopTime = Time.realtimeSinceStartup;
                }
            } else {
                behaviorTree.taskList[taskIndex].NodeData.PushTime = -1;
                behaviorTree.taskList[taskIndex].NodeData.PopTime = Time.realtimeSinceStartup;
            }
            if (behaviorTree.behavior.logTaskChanges) {
                print(string.Format("{0}: {1}: Pop task {2} (index {3}) at stack index {4} with status {5}", roundedTime(), behaviorTree.behavior.ToString(), behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex, status));
            }
#endif

            // let the parent know
            if (behaviorTree.parentIndex[taskIndex] != -1) {
                var parentTask = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
                if (!parentTask.CanRunParallelChildren()) {
                    parentTask.OnChildExecuted(status);
                    status = parentTask.Decorate(status);
                } else {
                    parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], status);
                }
            }

            // pop any task whose base parent is equal to the base parent of the current task being popped
            if (popChildren) {
                for (int i = behaviorTree.activeStack.Count - 1; i > stackIndex; --i) {
                    if (isParentTask(behaviorTree, taskIndex, behaviorTree.activeStack[i].Peek())) {
                        var childStatus = TaskStatus.Failure;
                        while (i < behaviorTree.activeStack.Count && behaviorTree.activeStack[i].Count > 0) {
                            popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref childStatus, false, notifyOnEmptyStack);
                        }
                    }
                }
            }

            // If there are no more items in the stack, restart the tree (in the case of the root task) or remove the stack created by the parallel task
            if (behaviorTree.activeStack[stackIndex].Count == 0) {
                if (stackIndex == 0) {
                    // restart the tree
                    if (notifyOnEmptyStack) {
                        if (behaviorTree.behavior.restartWhenComplete) {
                            restart(behaviorTree);
                        } else {
                            disableBehavior(behaviorTree.behavior);
                        }
                    }
                    status = TaskStatus.Inactive;
                } else {
                    // don't remove the stack from the very first index
                    removeStack(behaviorTree, stackIndex);

                    // set the status to running to prevent the loop from running again within Update
                    status = TaskStatus.Running;
                }
            }
        }

        private void restart(BehaviorTree behaviorTree)
        {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (behaviorTree.behavior.logTaskChanges) {
                Debug.Log(string.Format("{0}: Restarting {1}", roundedTime(), behaviorTree.behavior.ToString()));
            }
#endif

            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                behaviorTree.taskList[i].OnBehaviorRestart();
            }
            pushTask(behaviorTree, 0, 0);
        }

        // returns if possibleParent is a parent of possibleChild
        private bool isParentTask(BehaviorTree behaviorTree, int possibleParent, int possibleChild)
        {
            int parentIndex = 0;
            int childIndex = possibleChild;
            while (childIndex != -1) {
                parentIndex = behaviorTree.parentIndex[childIndex];
                if (parentIndex == possibleParent) {
                    return true;
                }
                childIndex = parentIndex;
            }
            return false;
        }

        // a task has been interrupted. Store the interrupted index so the update loop knows to stop executing tasks with a parent task equal to the interrupted task
        public void interrupt(Behavior behavior, Task task)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            // determine the index of the task that is causing the interruption
            int interruptionIndex = -1;
            var behaviorTree = behaviorTreeMap[behavior];
            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                if (behaviorTree.taskList[i].Equals(task)) {
                    interruptionIndex = i;
                    break;
                }
            }

            if (interruptionIndex > -1) {
                // loop through the active tasks. Mark any stack that has interruption index as its parent
                int taskIndex;
                for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                    taskIndex = behaviorTree.activeStack[i].Peek();
                    while (taskIndex != -1) {
                        if (taskIndex == interruptionIndex) {
                            behaviorTree.interruptionIndex[i] = interruptionIndex;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            if (behavior.logTaskChanges) {
                                Debug.Log(string.Format("{0}: {1}: Interrupt task {2} with interrupt index {3} at stack index {4}", roundedTime(), behaviorTree.behavior.ToString(), task.GetType().ToString(), interruptionIndex, i));
                            }
#endif
                            break;
                        }
                        taskIndex = behaviorTree.parentIndex[taskIndex];
                    }
                }
            }

            // stop PlayMaker/uScript if they are running
            if (treeObjectMap.ContainsKey(behaviorTree)) {
                object[] objectValueArray = new object[1] { treeObjectMap[behaviorTree] };
                // remove the mapping because the execution has stopped
                objectTreeMap.Remove(treeObjectMap[behaviorTree]);
                treeObjectMap.Remove(behaviorTree);

                bool playMakerInvoked = false;
                var objectType = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker");
                if (objectType != null) {
                    var playMakerMethod = objectType.GetMethod("StopPlayMaker");
                    if (playMakerMethod != null) {
                        playMakerInvoked = (bool)playMakerMethod.Invoke(null, objectValueArray);
                    }
                }
                if (!playMakerInvoked) {
                    objectType = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_uScript");
                    if (objectType != null) {
                        var uScriptMethod = objectType.GetMethod("StopuScript");
                        if (uScriptMethod != null) {
                            uScriptMethod.Invoke(null, objectValueArray);
                        }
                    }
                }
            }
        }

        // remove the stack at stackIndex
        private void removeStack(BehaviorTree behaviorTree, int stackIndex)
        {
            behaviorTree.activeStack.RemoveAt(stackIndex);
            behaviorTree.interruptionIndex.RemoveAt(stackIndex);
            behaviorTree.nonInstantTaskStatus.RemoveAt(stackIndex);
        }

        // Forward the collision/trigger callback to the active task
        public void BehaviorOnCollisionEnter(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnCollisionEnter(collision);
            }
        }

        public void BehaviorOnCollisionExit(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnCollisionExit(collision);
            }
        }

        public void BehaviorOnCollisionStay(Collision collision, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnCollisionStay(collision);
            }
        }

        public void BehaviorOnTriggerEnter(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnTriggerEnter(other);
            }
        }

        public void BehaviorOnTriggerExit(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnTriggerExit(other);
            }
        }

        public void BehaviorOnTriggerStay(Collider other, Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            int taskIndex;
            for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                taskIndex = behaviorTree.activeStack[i].Peek();
                behaviorTree.taskList[taskIndex].OnTriggerStay(other);
            }
        }

        // Play Maker / uScript support
        public bool mapObjectToTree(object objectKey, Behavior behavior, bool fromPlayMaker)
        {
            if (objectTreeMap.ContainsKey(objectKey)) {
                Debug.LogError(string.Format("Only one behavior can be mapped to the same instance of the {0}.", (fromPlayMaker ? "PlayMaker FSM" : "uScript")));
                return false;
            }
            objectTreeMap.Add(objectKey, behaviorTreeMap[behavior]);
            treeObjectMap.Add(behaviorTreeMap[behavior], objectKey);
            return true;
        }

        public BehaviorTree treeForObject(object objectKey)
        {
            if (!objectTreeMap.ContainsKey(objectKey)) {
                return null;
            }
            var behaviorTree = objectTreeMap[objectKey];
            objectTreeMap.Remove(objectKey);
            treeObjectMap.Remove(behaviorTree);
            return behaviorTree;
        }

        public int stackCount(BehaviorTree behaviorTree)
        {
            return behaviorTree.activeStack.Count;
        }

        public Task taskWithTreeAndStackIndex(BehaviorTree behaviorTree, int stackIndex)
        {
            return behaviorTree.taskList[behaviorTree.activeStack[stackIndex].Peek()];
        }

        private decimal roundedTime()
        {
            return Math.Round((decimal)Time.time, 5, MidpointRounding.AwayFromZero);
        }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        public List<Task> getTaskList(Behavior behavior)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior)) {
                return null;
            }

            var behaviorTree = behaviorTreeMap[behavior];
            return behaviorTree.taskList;
        }
#endif
    }
}