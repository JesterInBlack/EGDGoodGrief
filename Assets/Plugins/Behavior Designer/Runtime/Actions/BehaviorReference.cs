using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    // The Behavior Tree Reference task allows you to run another behavior tree within the current behavior tree.
    // One use for this is that if you have an unit that plays a series of tasks to attack. You may want the unit to attack at different points within
    // the behavior tree, and you want that attack to always be the same. Instead of copying and pasting the same tasks over and over you can just use
    // an external behavior and then the tasks are always guaranteed to be the same. This example is demonstrated in the RTS sample project located at
    // http://www.opsive.com/assets/BehaviorDesigner/samples.php.
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=53")]
    [TaskIcon("BehaviorTreeReferenceIcon.png")]
    public abstract class BehaviorReference : Action
    {
        // External behavior that this task should reference.
        public BehaviorDesigner.Runtime.ExternalBehavior externalBehavior;

        // Returns the external behavior. Can be overridden.
        public virtual BehaviorDesigner.Runtime.ExternalBehavior getExternalBehavior()
        {
            return externalBehavior;
        }

        public override void OnReset()
        {
            // Reset the properties back to their original values
            externalBehavior = null;
        }
    }
}