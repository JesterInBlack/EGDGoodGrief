using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public interface IBehavior
    {
        string GetOwnerName();
        int GetInstanceID();
        BehaviorSource GetBehaviorSource();
        void SetBehaviorSource(BehaviorSource behaviorSource);
        Object GetObject();
        void ClearUnityObjects();
        int SerializeUnityObject(Object unityObject);
        Object DeserializeUnityObject(int id);
    }
}