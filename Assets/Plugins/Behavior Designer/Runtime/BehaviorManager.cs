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
        private bool showExternalTrees = false;
        private bool dirty = false;
        public bool Dirty { get { return dirty; } set { dirty = value; } }
#endif

        // convenience task used for adding new tasks
        public class TaskAddData
        {
            public bool fromExternalTask = false;
            public ParentTask parentTask = null;
            public int parentIndex = -1;
            public Dictionary<string, object> sharedVariables = null;
            public Dictionary<string, object> inheritedFields = null;

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            public Vector2 nodeOffset = Vector2.zero;
#endif
        }

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

            // load the external behavior tree if the entry task is null and the external behavior tree is not
            if (behavior.GetBehaviorSource().RootTask == null && behavior.externalBehavior != null) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                // an entry task is required in order to view the tree within the editor
                var entryTask = ScriptableObject.CreateInstance("EntryTask") as Task;
                entryTask.ID = 0;
                entryTask.Owner = behavior;
                entryTask.NodeData = new NodeData();
                behavior.GetBehaviorSource().EntryTask = entryTask;
#endif
                var behaviorTreeReferenceTask = ScriptableObject.CreateInstance("BehaviorTreeReference") as BehaviorReference;
                behaviorTreeReferenceTask.ID = 1;
                behaviorTreeReferenceTask.Owner = behavior;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                behaviorTreeReferenceTask.NodeData = new NodeData();
                behaviorTreeReferenceTask.NodeData.Position = new Vector2(0, 100);
#endif
                behaviorTreeReferenceTask.externalBehaviors = new ExternalBehavior[] { behavior.externalBehavior };
                behavior.GetBehaviorSource().RootTask = behaviorTreeReferenceTask;
            }

            if (behavior.GetBehaviorSource().RootTask == null) {
                Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains no root task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                return;
            }

            behaviorTree = new BehaviorTree();
            behaviorTree.taskList = new List<Task>();
            behaviorTree.behavior = behavior;
            behaviorTree.parentIndex = new List<int>();
            behaviorTree.childrenIndex = new List<List<int>>();
            behaviorTree.relativeChildIndex = new List<int>();
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            behaviorTree.originalTaskList = new List<Task>();
            behaviorTree.originalIndex = new List<int>();
#endif
            behaviorTree.parentIndex.Add(-1); // add the first entry for the root task
            behaviorTree.relativeChildIndex.Add(-1);

            // Create new instances of the shared variables if the behavior is coming from a prefab to prevent multiple objects from referencing the same variables
            if (behavior.GetInstanceID() < 0) {
                var behaviorSource = behavior.GetBehaviorSource();
                if (behaviorSource.Variables != null) {
                    for (int i = 0; i < behaviorSource.Variables.Count; ++i) {
                        behaviorSource.Variables[i] = copySharedVariable(behaviorSource.Variables[i]);
                    }
                }
            }

            bool hasExternalBehavior = false;
            int status = addToTaskList(behaviorTree, behavior.GetBehaviorSource().RootTask, ref hasExternalBehavior, new TaskAddData());
            if (status < 0) {
                // something is wrong with the tree. Don't go any further
                behaviorTree = null;
                switch (status) {
                    case -1:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" is invalid. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -2:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" cannot find the referenced external task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -3:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains an invalid task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                    case -4:
                        Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains multiple external behavior trees at the root task or as a child of a parent task which cannot contain so many children (such as a decorator task). This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
                        break;
                }
                return;
            }
            
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // a behavior tree is dirty if new tasks had to be instantiated
            dirty = behaviorTree.taskList[0].GetInstanceID() < 0 || hasExternalBehavior;
            if (dirty) {
                // update the root task with the newly instantiated task
                behavior.GetBehaviorSource().RootTask = behaviorTree.taskList[0];
            }
#endif
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
#if !UNITY_WINRT && !DLL_DEBUG && !DLL_RELEASE
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
#if !UNITY_WINRT && !DLL_DEBUG && !DLL_RELEASE
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

            // start with the first index if it isn't disabled
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            if (!behaviorTree.taskList[0].NodeData.Disabled) {
#endif
                pushTask(behaviorTree, 0, 0);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            }
#endif
        }

        // returns 0 for success
        // returns -1 if a parent doesn't have any children
        // returns -2 if the external task cannot be found
        // returns -3 if the task is null
        // returns -4 if there are multiple external behavior trees and the parent task is null or cannot handle as many behavior trees specified
        public int addToTaskList(BehaviorTree behaviorTree, Task task, ref bool hasExternalBehavior, TaskAddData data)
        {
            if (task == null) {
                return -3;
            }

#pragma warning disable 0618
            if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                task.GetType() == typeof(BehaviorReference) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.BehaviorReference))) {
                BehaviorSource[] behaviorSource = null;
                if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior))) {
                    Debug.LogWarning(string.Format("{0}: The External Behavior Tree task has been deprecated. Use the Behavior Tree Reference task instead.", behaviorTree.behavior.ToString()));
                    BehaviorDesigner.Runtime.Tasks.ExternalBehavior externalBehaviorTask = task as BehaviorDesigner.Runtime.Tasks.ExternalBehavior;
                    if (externalBehaviorTask != null && externalBehaviorTask.externalTask != null) {
                        var behavior = externalBehaviorTask.externalTask.GetComponent<Behavior>();
                        if (behavior != null) {
                            behavior = UnityEngine.Object.Instantiate(behavior) as Behavior;
                            behaviorSource = new BehaviorSource[] { behavior.GetBehaviorSource() };
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
                        ExternalBehavior[] externalBehaviors = null;
                        if ((externalBehaviors = behaviorReference.getExternalBehaviors()) != null) {
                            behaviorSource = new BehaviorSource[externalBehaviors.Length];
                            for (int i = 0; i < externalBehaviors.Length; ++i) {
                                behaviorSource[i] = externalBehaviors[i].BehaviorSource;
                                behaviorSource[i].Owner = externalBehaviors[i];
                            }
                        } else {
                            return -2;
                        }
                    } else {
                        return -2;
                    }
                }
                if (behaviorSource != null) {
                    ParentTask parentTask = data.parentTask;
                    int parentIndex = data.parentIndex;
                    for (int i = 0; i < behaviorSource.Length; ++i) {
                        if ((behaviorSource[i].Owner as Behavior) != null && (behaviorSource[i].Owner as Behavior).UpdateDeprecatedTasks()) {
                            Debug.LogWarning(string.Format("{0}: the data format for this behavior tree has been deprecated. Run the Behavior Designer Update tool or select this GameObject within the inspector to update this behavior tree.", ToString()));
                        }
                        // make sure the behavior source is properly serialized
                        behaviorSource[i].checkForJSONSerialization();
                        var externalRootTask = behaviorSource[i].RootTask;
                        if (externalRootTask != null) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            if (!data.fromExternalTask && i == 0) {
                                if (behaviorTree.behavior.GetInstanceID() < 0) {
                                    behaviorTree.originalTaskList.Add(copyTask(behaviorTree, task, data));
                                } else {
                                    behaviorTree.originalTaskList.Add(task);
                                }
                            }
#endif
                            // bring the external behavior trees variables into the parent behavior tree
                            if (behaviorSource[i].Variables != null) {
                                for (int j = 0; j < behaviorSource[i].Variables.Count; ++j) {
                                    // only set the behavior tree variable if it doesn't already exist in the parent behavior tree
                                    if (behaviorTree.behavior.GetVariable(behaviorSource[i].Variables[j].name) == null) {
                                        // create a new instance of the variable to prevent overwriting the value
                                        var localSharedVariable = copySharedVariable(behaviorSource[i].Variables[j]);
                                        behaviorTree.behavior.SetVariable(localSharedVariable.name, localSharedVariable);
                                        // add it to the shared variables dictionary
                                        if (data.sharedVariables == null) {
                                            data.sharedVariables = new Dictionary<string, object>();
                                        }
                                        if (!data.sharedVariables.ContainsKey(localSharedVariable.name)) {
                                            data.sharedVariables.Add(localSharedVariable.name, localSharedVariable);
                                        }
                                    }
                                }
                            }

                            // find all of the inherited fields
                            var fields = task.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                            for (int j = 0; j < fields.Length; ++j) {
                                if (fields[j].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0) {
                                    if (data.inheritedFields == null) {
                                        data.inheritedFields = new Dictionary<string, object>();
                                    }
                                    if (!data.inheritedFields.ContainsKey(fields[j].Name)) {
                                        if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable))) {
                                            var sharedVariable = fields[j].GetValue(task) as SharedVariable;
                                            if (sharedVariable.IsShared) {
                                                var newSharedVariable = behaviorTree.behavior.GetVariable(sharedVariable.name);
                                                if (newSharedVariable == null && data.sharedVariables != null && data.sharedVariables.ContainsKey(sharedVariable.name)) {
                                                    newSharedVariable = data.sharedVariables[sharedVariable.name] as SharedVariable;
                                                }
                                                data.inheritedFields.Add(fields[j].Name, newSharedVariable);
                                            } else {
                                                data.inheritedFields.Add(fields[j].Name, sharedVariable);
                                            }
                                        } else {
                                            data.inheritedFields.Add(fields[j].Name, fields[j].GetValue(task));
                                        }
                                    }
                                }
                            }
                            if (i > 0) {
                                // If there are multiple external behavior trees then the parent task/index were probably changed
                                data.parentTask = parentTask;
                                data.parentIndex = parentIndex;
                                // Return an error if the parent task is null (root task) or if there are too many external behavior trees
                                if (data.parentTask == null || i >= data.parentTask.MaxChildren()) {
                                    return -4;
                                } else {
                                    // add the external tree
                                    behaviorTree.parentIndex.Add(data.parentIndex);
                                    behaviorTree.relativeChildIndex.Add(data.parentTask.Children.Count);
                                    behaviorTree.childrenIndex[data.parentIndex].Add(behaviorTree.taskList.Count);
                                    data.parentTask.AddChild(externalRootTask, data.parentTask.Children.Count);
                                }
                            }
                            hasExternalBehavior = true;
                            bool fromExternalTask = data.fromExternalTask;
                            data.fromExternalTask = true;
                            int status = 0;
                            if ((status = addToTaskList(behaviorTree, externalRootTask, ref hasExternalBehavior, data)) < 0) {
                                return status;
                            }
                            // reset back to the original value
                            data.fromExternalTask = fromExternalTask;
                        } else {
                            return -2;
                        }
                    }
                } else {
                    return -2;
                }
            } else {
                // If the task is coming from an external behavior or prefab than create a new task. This is done to
                // keep all of the properties local and not override an external behavior or prefab with any property values.
                Task localTask = null;
                if (data.fromExternalTask || behaviorTree.behavior.GetInstanceID() < 0) { // prefabs have an instance id of less than 0
                    localTask = copyTask(behaviorTree, task, data);
                    behaviorTree.taskList.Add(localTask);
                    if (data.fromExternalTask) {
                        if (data.parentTask == null) { // A null parent task means the root task is the behavior tree reference task, so just replace that task
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            localTask.NodeData.Position = behaviorTree.behavior.GetBehaviorSource().RootTask.NodeData.Position;
#endif
                        } else {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            localTask.NodeData.Position = data.parentTask.NodeData.Position + data.nodeOffset;
#endif
                            int relativeChildIndex = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            int parentIndex = behaviorTree.parentIndex[behaviorTree.parentIndex.Count - 1];
                            var origParentType = behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]].GetType();
#pragma warning disable 0618
                            if (origParentType == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || origParentType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                                origParentType == typeof(BehaviorReference) || origParentType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.BehaviorReference))) {
#endif
                                data.parentTask.ReplaceAddChild(localTask, relativeChildIndex);
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                            } else {
                                // if the original parent type is not a task reference then this task index is the task reference task
                                data.parentTask.ReplaceAddChild(behaviorTree.originalTaskList[behaviorTree.originalTaskList.Count - 1], relativeChildIndex);
                            }
#endif
#pragma warning restore 0618
                        }
                    } else {
                        if (data.parentTask != null) {
                            int relativeChildIndex = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
                            data.parentTask.ReplaceAddChild(localTask, relativeChildIndex);
                        }
                    }
                } else {
                    task.ReferenceID = behaviorTree.taskList.Count;
                    behaviorTree.taskList.Add(task);

                    localTask = task;
                }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                if (behaviorTree.originalTaskList.Count == 0) {
                    behaviorTree.originalTaskList.Add(localTask);
                    behaviorTree.originalIndex.Add(0);
                } else {
                    if (!data.fromExternalTask) {
                        behaviorTree.originalTaskList.Add(localTask);
                    }
                    behaviorTree.originalIndex.Add(behaviorTree.originalTaskList.Count - 1);
                }
#endif
                if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                    var parentTask = task as ParentTask;
                    if (parentTask.Children.Count == 0) {
                        return -1; // invalid tree
                    }
                    int status;
                    int parentIndex = behaviorTree.taskList.Count - 1;
                    behaviorTree.childrenIndex.Add(new List<int>());
                    // store the childCount ahead of time in case new external trees are added to the current parent
                    int childCount = parentTask.Children.Count;
                    for (int i = 0; i < childCount; ++i) {
                        behaviorTree.parentIndex.Add(parentIndex);
                        behaviorTree.relativeChildIndex.Add(i);
                        behaviorTree.childrenIndex[parentIndex].Add(behaviorTree.taskList.Count);
                        data.parentTask = localTask as ParentTask;
                        data.parentIndex = parentIndex;
#if UNITY_EDITOR || DLL_RELEASE || DLL_DEBUG
                        data.nodeOffset = parentTask.Children[i].NodeData.Position - parentTask.NodeData.Position;
#endif
                        if ((status = addToTaskList(behaviorTree, parentTask.Children[i], ref hasExternalBehavior, data)) < 0) {
                            return status;
                        }
                    }
                } else {
                    behaviorTree.childrenIndex.Add(null);
                }
            }
            return 0;
        }
        
        private Task copyTask(BehaviorTree behaviorTree, Task task, TaskAddData data)
        {
            var newTask = ScriptableObject.CreateInstance(task.GetType()) as Task;
            var fields = task.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i) {
                // don't copy the children, they will be copied later
                if (task.GetType().IsSubclassOf(typeof(ParentTask)) && fields[i].Name.Equals("children")) {
                    fields[i].SetValue(newTask, null);
                    continue;
                }
                var value = fields[i].GetValue(task);
                if (data.inheritedFields != null && data.inheritedFields.ContainsKey(fields[i].Name) && fields[i].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0) {
                    fields[i].SetValue(newTask, data.inheritedFields[fields[i].Name]);
                } else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)) && value != null) {
                    SharedVariable sharedVariable = value as SharedVariable;
                    var sharedVariableName = (value as SharedVariable).name;
                    if (!sharedVariable.IsShared) {
                        fields[i].SetValue(newTask, copySharedVariable(sharedVariable));
                    } else if (behaviorTree.behavior.GetVariable(sharedVariableName) != null) {
                        fields[i].SetValue(newTask, behaviorTree.behavior.GetVariable(sharedVariableName));
                    } else if (data.sharedVariables != null && data.sharedVariables.ContainsKey(sharedVariableName)) {
                        fields[i].SetValue(newTask, data.sharedVariables[sharedVariableName]);
                    } 
                } else {
                    fields[i].SetValue(newTask, value);
                }
            }
            newTask.ReferenceID = task.ReferenceID = behaviorTree.taskList.Count;
            newTask.ID = task.ID;
            newTask.IsInstant = task.IsInstant;
            newTask.Owner = task.Owner;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            newTask.NodeData = new NodeData();
            newTask.NodeData.copyFrom(task.NodeData, newTask);
#endif
            return newTask;
        }

        private SharedVariable copySharedVariable(SharedVariable variable)
        {
            var newSharedVariable = ScriptableObject.CreateInstance(variable.GetType()) as SharedVariable;
            newSharedVariable.name = variable.name;
            newSharedVariable.SetValue(variable.GetValue());
            return newSharedVariable;
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

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            // If the task is disabled then return immediately with a status of success. Notify the parent task that the child task finished executing so it will move on to the next child
            if (behaviorTree.taskList[taskIndex].NodeData.Disabled) {
                if (behaviorTree.behavior.logTaskChanges) {
                    print(string.Format("{0}: {1}: Skip task {2} (index {3}) at stack index {4} (task disabled)", roundedTime(), behaviorTree.behavior.ToString(), behaviorTree.taskList[taskIndex].GetType(), taskIndex, stackIndex));
                }
                if (behaviorTree.parentIndex[taskIndex] != -1) {
                    var parentTask = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
                    if (!parentTask.CanRunParallelChildren()) {
                        parentTask.OnChildExecuted(TaskStatus.Success);
                    } else {
                        parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], TaskStatus.Success);
                    }
                }
                return TaskStatus.Success;
            }
#endif

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

            if (task is ParentTask) {
                var parentTask = task as ParentTask;
                if (!parentTask.CanRunParallelChildren() || parentTask.OverrideStatus(TaskStatus.Running) != TaskStatus.Running) {
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                    int count = 0;
#endif
                    var childStatus = TaskStatus.Inactive;
                    // nest within a while loop so multiple child tasks can be run within a single update loop (such as conditions)
                    // also, if the parent is a parallel task, start running all of the children
                    int parentStack = stackIndex;
                    int prevChildIndex = -1;
                    while (parentTask.CanExecute() && (childStatus != TaskStatus.Running || parentTask.CanRunParallelChildren())) {
                        List<int> childrenIndexes = behaviorTree.childrenIndex[taskIndex];
                        int childIndex = parentTask.CurrentChildIndex();
                        // bail out if the child index is the same as what it was before runTask was executed
                        if ((childIndex == prevChildIndex && status != TaskStatus.Running) || !isBehaviorEnabled(behaviorTree.behavior)) {
                            status = TaskStatus.Running;
                            break;
                        } 
                        prevChildIndex = childIndex;
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
                    }
#pragma warning restore 0618
                behaviorTree.taskList[taskIndex].NodeData.PushTime = Time.realtimeSinceStartup;
                // reset the execution status of this task and all of its children if it has already been run
                setInactiveExecutionStatus(behaviorTree, taskIndex);
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
        
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        private void setInactiveExecutionStatus(BehaviorTree behaviorTree, int taskIndex)
        {
            if (behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus != TaskStatus.Inactive) {
                behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = TaskStatus.Inactive;
                var type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
#pragma warning disable 0618
                if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                    type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference))) {
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = TaskStatus.Inactive;
                }
#pragma warning restore 0618

                if (behaviorTree.taskList[taskIndex] is ParentTask) {
                    for (int i = 0; i < behaviorTree.childrenIndex[taskIndex].Count; ++i) {
                        setInactiveExecutionStatus(behaviorTree, behaviorTree.childrenIndex[taskIndex][i]);
                    }
                }
            }
        }
#endif

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
            if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
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
                    behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = status;
                }
            }
#pragma warning restore 0618
            behaviorTree.taskList[taskIndex].NodeData.PushTime = -1;
            behaviorTree.taskList[taskIndex].NodeData.PopTime = Time.realtimeSinceStartup;
            behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = status;
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
                    if (behaviorTree.activeStack[i].Count > 0) {
                        if (isParentTask(behaviorTree, taskIndex, behaviorTree.activeStack[i].Peek())) {
                            var childStatus = TaskStatus.Failure;
                            while (i < behaviorTree.activeStack.Count && behaviorTree.activeStack[i].Count > 0) {
                                popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref childStatus, false, notifyOnEmptyStack);
                            }
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

            int stackIndex = -1;
            if (interruptionIndex > -1) {
                // loop through the active tasks. Mark any stack that has interruption index as its parent
                int taskIndex;
                for (int i = 0; i < behaviorTree.activeStack.Count; ++i) {
                    if (behaviorTree.activeStack[i].Count > 0) {
                        taskIndex = behaviorTree.activeStack[i].Peek();
                        while (taskIndex != -1) {
                            if (taskIndex == interruptionIndex) {
                                behaviorTree.interruptionIndex[i] = interruptionIndex;
                                stackIndex = i;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
                                if (behavior.logTaskChanges) {
                                    Debug.Log(string.Format("{0}: {1}: Interrupt task {2} with interrupt index {3} at stack index {4}", roundedTime(), behaviorTree.behavior.ToString(), task.GetType().ToString(), interruptionIndex, stackIndex));
                                }
#endif
                                break;
                            }
                            taskIndex = behaviorTree.parentIndex[taskIndex];
                        }
                    }
                }
            }

            // stop PlayMaker/uScript/DialogueSystem if they are running
            if (treeObjectMap.ContainsKey(behaviorTree)) {
                object obj = treeObjectMap[behaviorTree];
                // remove the mapping because the execution has stopped
                objectTreeMap.Remove(treeObjectMap[behaviorTree]);
                treeObjectMap.Remove(behaviorTree);

                bool extendedMethodInvoked = false;
                var objectType = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker");
                if (objectType != null) {
                    var playMakerMethod = objectType.GetMethod("StopPlayMaker");
                    if (playMakerMethod != null) {
                        // find the playMaker task
                        object playMakerTask = null;
                        if (stackIndex != -1) {
                            int taskIndex = behaviorTree.activeStack[stackIndex].Peek();
                            var playMakerTaskType = Type.GetType("BehaviorDesigner.Runtime.Tasks.Start_PlayMakerFSM");
                            while (taskIndex != -1) {
                                if (behaviorTree.taskList[taskIndex].GetType().Equals(playMakerTaskType)) {
                                    playMakerTask = behaviorTree.taskList[taskIndex];
                                    break;
                                }
                                taskIndex = behaviorTree.parentIndex[taskIndex];
                            }
                        }
                        extendedMethodInvoked = (bool)playMakerMethod.Invoke(null, new object[] { obj, playMakerTask });
                    }
                }
                if (!extendedMethodInvoked) {
                    objectType = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_uScript");
                    if (objectType != null) {
                        var uScriptMethod = objectType.GetMethod("StopuScript");
                        if (uScriptMethod != null) {
                            extendedMethodInvoked = (bool)uScriptMethod.Invoke(null, new object[] { obj });
                        }
                    }
                }
                if (!extendedMethodInvoked) {
                    objectType = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_DialogueSystem");
                    if (objectType != null) {
                        // find the DialogueSystem task
                        object dialogueSystemTask = null;
                        if (stackIndex != -1) {
                            int taskIndex = behaviorTree.activeStack[stackIndex].Peek();
                            var startConversationTaskType = Type.GetType("BehaviorDesigner.Runtime.Tasks.StartConversation");
                            var startSequenceTaskType = Type.GetType("BehaviorDesigner.Runtime.Tasks.StartSequence");
                            while (taskIndex != -1) {
                                var taskType = behaviorTree.taskList[taskIndex].GetType();
                                if (taskType.Equals(startConversationTaskType) || taskType.Equals(startSequenceTaskType)) {
                                    dialogueSystemTask = behaviorTree.taskList[taskIndex];
                                    break;
                                }
                                taskIndex = behaviorTree.parentIndex[taskIndex];
                            }
                        }
                        var dialogueSystemMethod = objectType.GetMethod("StopDialogueSystem");
                        if (dialogueSystemMethod != null) {
                            dialogueSystemMethod.Invoke(null, new object[] { obj, dialogueSystemTask });
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

        // Play Maker / uScript / DialogueSystem support
        public enum ThirdPartyObjectType { PlayMaker, uScript, DialogueSystem }
        public bool mapObjectToTree(object objectKey, Behavior behavior, ThirdPartyObjectType objectType)
        {
            if (objectTreeMap.ContainsKey(objectKey)) {
                string thirdPartyName = "";
                switch (objectType) {
                    case ThirdPartyObjectType.PlayMaker:
                        thirdPartyName = "PlayMaker FSM";
                        break;
                    case ThirdPartyObjectType.uScript:
                        thirdPartyName = "uScript Graph";
                        break;
                    case ThirdPartyObjectType.DialogueSystem:
                        thirdPartyName = "Dialogue System";
                        break;
                }
                Debug.LogError(string.Format("Only one behavior can be mapped to the same instance of the {0}.", thirdPartyName));
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
            // may be null if the topmost task on the stack is disabled
            if (behaviorTree.activeStack[stackIndex].Count == 0)
                return null;
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

        public bool setShouldShowExternalTree(Behavior behavior, bool show)
        {
            if (behaviorTreeMap == null || behaviorTreeMap.Count == 0 || behavior == null || !behaviorTreeMap.ContainsKey(behavior))
                return false;

            showExternalTrees = show;
            bool dirty = false;
            var behaviorTree = behaviorTreeMap[behavior];
            for (int i = 0; i < behaviorTree.taskList.Count; ++i) {
                var type = behaviorTree.originalTaskList[behaviorTree.originalIndex[i]].GetType();
#pragma warning disable 0618
                if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) ||
                    type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference))) {
                    int parentIndex = behaviorTree.parentIndex[i];
                    if (parentIndex != -1) {
                        type = behaviorTree.originalTaskList[behaviorTree.originalIndex[parentIndex]].GetType();
                    }
                    // swap out the root
                    if (parentIndex == -1) {
                        var task = (showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]]);
                        behavior.GetBehaviorSource().RootTask = task;
                        dirty = true;
                    } else if (!type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) &&
                        !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference))) {
                        (behaviorTree.taskList[parentIndex] as ParentTask).Children[behaviorTree.relativeChildIndex[i]] = (showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]]);
                        dirty = true;
                    }
                }
#pragma warning restore 0618
            }

            return dirty;
        }
#endif
    }
}