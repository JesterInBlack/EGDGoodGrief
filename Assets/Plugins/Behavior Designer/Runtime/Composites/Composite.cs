namespace BehaviorDesigner.Runtime.Tasks
{
    // Composite tasks are parent tasks that hold a list of child tasks. For example, one composite task may loop through the child tasks sequentially while another
    // composite task may run all of its child tasks at once. The return status of the composite tasks depends on its children. 
    public abstract class Composite : ParentTask
    {

    }
}