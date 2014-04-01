using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public abstract class ExternalBehavior : ScriptableObject, IBehavior
    {
        [SerializeField]
        private BehaviorSource mBehaviorSource;
        public BehaviorSource BehaviorSource { get { return mBehaviorSource; } set { mBehaviorSource = value; } }
        public BehaviorSource GetBehaviorSource() { return mBehaviorSource; }
        public void SetBehaviorSource(BehaviorSource behaviorSource) { mBehaviorSource = behaviorSource; }
        public Object GetObject() { return this; }
        public string GetOwnerName() { return "External Behavior"; }

        [SerializeField]
        private List<Object> mUnityObjects;

        // Support blackboard variables:
        public SharedVariable GetVariable(string name)
        {
            return mBehaviorSource.GetVariable(name);
        }

        public void SetVariable(string name, SharedVariable item)
        {
            mBehaviorSource.SetVariable(name, item);
        }

        public void ClearUnityObjects() { if (mUnityObjects != null) mUnityObjects.Clear(); }

        public int SerializeUnityObject(Object unityObject) {
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

            var obj = mUnityObjects[id];
            // UnityEngine.Object overrides equals so that obj == null is true when ReferenceEquals(obj, null) returns false.
            // Return the correct null value.
            if (obj == null) {
                return null;
            }

            return obj;
        }
    }
}