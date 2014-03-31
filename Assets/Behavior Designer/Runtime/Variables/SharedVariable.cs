using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    public enum SharedVariableTypes { Int, Float, Bool, String, Vector2, Vector3, Vector4, Quaternion, Color, Rect, GameObject, Transform, Object }

    public abstract class SharedVariable : ScriptableObject
    {
        public bool IsShared { get { return mIsShared; } set { mIsShared = value; } }
        [SerializeField]
        protected bool mIsShared = false;

        public SharedVariableTypes ValueType { get { return mValueType; } }
        [SerializeField]
        protected SharedVariableTypes mValueType;

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }
}