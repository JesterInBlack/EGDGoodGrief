using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class BehaviorSource
    {
        public string behaviorName = "Behavior";
        public string behaviorDescription = "";

        [SerializeField]
        private int behaviorID = -1;
        public int BehaviorID { get { return behaviorID; } set { behaviorID = value; } }

        [SerializeField]
        private Task mEntryTask = null;
        public Task EntryTask { get { return mEntryTask; } set { mEntryTask = value; } }
        [SerializeField] 
        private Task mRootTask = null;
        public Task RootTask { get { return mRootTask; } set { mRootTask = value; } }
        [SerializeField]
        private List<Task> mDetachedTasks = null;
        public List<Task> DetachedTasks { get { return mDetachedTasks; } set { mDetachedTasks = value; } }
        [SerializeField]
        private string mSerialization;
        public string Serialization { get { return mSerialization; } set { mSerialization = value; } }
        [SerializeField]
        private List<SharedVariable> mVariables;
        public List<SharedVariable> Variables { get { return mVariables; } set { mVariables = value; updateVariablesIndex(); } }
        private Dictionary<string, int> mSharedVariableIndex;

        [SerializeField]
        private IBehavior mOwner;
        public IBehavior Owner { get { return mOwner; } set { mOwner = value; } }

        public BehaviorSource(IBehavior owner)
        {
            mOwner = owner;
        }

        public void save(Task entryTask, Task rootTask, List<Task> detachedTasks)
        {
            mEntryTask = entryTask;
            mRootTask = rootTask;
            mDetachedTasks = detachedTasks;
        }

        public void load(out Task entryTask, out Task rootTask, out List<Task> detachedTasks)
        {
            entryTask = mEntryTask;
            rootTask = mRootTask;
            detachedTasks = mDetachedTasks;
        }

        public void checkForJSONSerialization()
        {
            if (mSerialization != null && !mSerialization.Equals("") && mEntryTask == null) {
                DeserializeJSON.Deserialize(this);
            }
        }

        public SharedVariable GetVariable(string name)
        {
            if (mVariables != null && (mSharedVariableIndex == null || (mSharedVariableIndex.Count != mVariables.Count))) {
                mSharedVariableIndex = new Dictionary<string, int>(Variables.Count);
                for (int i = 0; i < mVariables.Count; ++i) {
                    if (mVariables[i] == null) {
                        return null;
                    }
                    mSharedVariableIndex.Add(mVariables[i].name, i);
                }
            }
            if (mSharedVariableIndex.ContainsKey(name)) {
                return mVariables[mSharedVariableIndex[name]];
            }
            return null;
        }

        public void SetVariable(string name, SharedVariable item)
        {
            if (mVariables == null) {
                mVariables = new List<SharedVariable>();
            }

            if (mSharedVariableIndex != null && mSharedVariableIndex.ContainsKey(name)) {
                mVariables[mSharedVariableIndex[name]] = item;
            } else {
                mVariables.Add(item);
                updateVariablesIndex();
            }
        }

        private void updateVariablesIndex()
        {
            if (mSharedVariableIndex == null) {
                mSharedVariableIndex = new Dictionary<string, int>(mVariables.Count);
            } else {
                mSharedVariableIndex.Clear();
            }
            for (int i = 0; i < mVariables.Count; ++i) {
                if (mVariables[i] == null)
                    continue;
                mSharedVariableIndex.Add(mVariables[i].name, i);
            }
        }

        public override string ToString()
        {
            if (mOwner == null) {
                return behaviorName;
            } else {
                return string.Format("{0} - {1}", Owner.GetOwnerName(), behaviorName);
            }
        }
    }
}