using UnityEngine;
using System.Collections;

namespace BehaviorDesigner.Runtime
{
    // ScriptableObjects do not have coroutines like monobehaviours do. Therefore we must add the functionality ourselves by using the parent behavior component which is a monobehaviour.
    public class TaskCoroutine
    {
        private IEnumerator mCoroutine;
        private Behavior mParent;
        private string mCoroutineName;
        private bool mStop = false;
        public void Stop() { mStop = true; }

        public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
        {
            mParent = parent;
            mCoroutine = coroutine;
            mCoroutineName = coroutineName;
            parent.StartCoroutine(RunCoroutine());
        }

        public IEnumerator RunCoroutine()
        {
            yield return null;
            while (!mStop) {
                if (mCoroutine != null && mCoroutine.MoveNext()) {
                    yield return mCoroutine.Current;
                } else {
                    break;
                }
            }
            mParent.TaskCoroutineEnded(this, mCoroutineName);
        }
    }
}