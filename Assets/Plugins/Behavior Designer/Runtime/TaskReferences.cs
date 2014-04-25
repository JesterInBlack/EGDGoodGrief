using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class TaskReferences : MonoBehaviour
    {
        // Check the references from the inspector with a behavior
        public static void CheckReferences(BehaviorSource behaviorSource)
        {
            if (behaviorSource.RootTask != null) {
                checkReferences(behaviorSource, behaviorSource.RootTask);
            }

            if (behaviorSource.DetachedTasks != null) {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; ++i) {
                    checkReferences(behaviorSource, behaviorSource.DetachedTasks[i]);
                }
            }
        }

        private static void checkReferences(BehaviorSource behaviorSource, Task task)
        {
            var fieldInfo = task.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fieldInfo.Length; ++i) {
                if (!fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.Equals(typeof(Task)) || fieldInfo[i].FieldType.IsSubclassOf(typeof(Task)))) {
                    // find the new task
                    var referencedTask = fieldInfo[i].GetValue(task) as Task;
                    if (referencedTask != null && !referencedTask.Owner.Equals(behaviorSource.Owner)) {
                        var newTask = findReferencedTask(behaviorSource, referencedTask);
                        if (newTask != null) {
                            fieldInfo[i].SetValue(task, newTask);
                        }
                    }
                } else if (fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.GetElementType().Equals(typeof(Task)) || fieldInfo[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) {
                    var referencedTasks = fieldInfo[i].GetValue(task) as Task[];
                    if (referencedTasks != null && i < referencedTasks.Length) {
                        if (referencedTasks[i].Owner == null || referencedTasks[i].Owner.Equals(behaviorSource.Owner)) {
                            continue;
                        }
                        var referencedTasksList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldInfo[i].FieldType.GetElementType())) as IList;
                        for (int j = 0; j < referencedTasks.Length; ++j) {
                            var newTask = findReferencedTask(behaviorSource, referencedTasks[j]);
                            if (newTask != null) {
                                referencedTasksList.Add(newTask);
                            }
                        }
                        // copy to an array so SetValue will accept the new value
                        var referencedTasksArray = Array.CreateInstance(fieldInfo[i].FieldType.GetElementType(), referencedTasksList.Count);
                        referencedTasksList.CopyTo(referencedTasksArray, 0);
                        fieldInfo[i].SetValue(task, referencedTasksArray);
                    }
                }
            }

            if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                var parentTask = task as ParentTask;
                if (parentTask.Children != null) {
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        checkReferences(behaviorSource, parentTask.Children[i]);
                    }
                }
            }
        }

        private static Task findReferencedTask(BehaviorSource behaviorSource, Task referencedTask)
        {
            int referencedTaskID = referencedTask.ID;
            Task task;
            if (behaviorSource.RootTask != null && (task = findReferencedTask(behaviorSource.RootTask, referencedTaskID)) != null) {
                return task;
            }

            if (behaviorSource.DetachedTasks != null) {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; ++i) {
                    if ((task = findReferencedTask(behaviorSource.DetachedTasks[i], referencedTaskID)) != null) {
                        return task;
                    }
                }
            }
            return null;
        }

        private static Task findReferencedTask(Task task, int referencedTaskID)
        {
            if (task.ID == referencedTaskID) {
                return task;
            }

            if (task.GetType().IsSubclassOf(typeof(ParentTask))) {
                var parentTask = task as ParentTask;
                if (parentTask.Children != null) {
                    Task childTask;
                    for (int i = 0; i < parentTask.Children.Count; ++i) {
                        if ((childTask = findReferencedTask(parentTask.Children[i], referencedTaskID)) != null) {
                            return childTask;
                        }
                    }
                }
            }
            return null;
        }

        // Check the references from the Behavior Manager with a task list
        public static void CheckReferences(Behavior behavior, List<Task> taskList)
        {
            for (int i = 0; i < taskList.Count; ++i) {
                checkReferences(behavior, taskList[i], taskList);
            }
        }

        private static void checkReferences(Behavior behavior, Task task, List<Task> taskList)
        {
            var fieldInfo = task.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fieldInfo.Length; ++i) {
                if (!fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.Equals(typeof(Task)) || fieldInfo[i].FieldType.IsSubclassOf(typeof(Task)))) {
                    // find the new task
                    var referencedTask = fieldInfo[i].GetValue(task) as Task;
                    if (referencedTask != null && !referencedTask.Owner.Equals(behavior)) {
                        var newTask = findReferencedTask(referencedTask, taskList);
                        if (newTask != null) {
                            fieldInfo[i].SetValue(task, newTask);
                        }
                    }
                } else if (fieldInfo[i].FieldType.IsArray && (fieldInfo[i].FieldType.GetElementType().Equals(typeof(Task)) || fieldInfo[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) {
                    var referencedTasks = fieldInfo[i].GetValue(task) as Task[];
                    if (referencedTasks != null && i < referencedTasks.Length) {
                        if (referencedTasks[i].Owner != null && referencedTasks[i].Owner.Equals(behavior)) {
                            continue;
                        }
                        var referencedTasksList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldInfo[i].FieldType.GetElementType())) as IList;
                        for (int j = 0; j < referencedTasks.Length; ++j) {
                            var newTask = findReferencedTask(referencedTasks[j], taskList);
                            if (newTask != null) {
                                referencedTasksList.Add(newTask);
                            }
                        }
                        // copy to an array so SetValue will accept the new value
                        var referencedTasksArray = Array.CreateInstance(fieldInfo[i].FieldType.GetElementType(), referencedTasksList.Count);
                        referencedTasksList.CopyTo(referencedTasksArray, 0);
                        fieldInfo[i].SetValue(task, referencedTasksArray);
                    }
                }
            }
        }

        private static Task findReferencedTask(Task referencedTask, List<Task> taskList)
        {
            int referencedTaskID = referencedTask.ReferenceID;
            for (int i = 0; i < taskList.Count; ++i) {
                if (taskList[i].ReferenceID == referencedTaskID) {
                    return taskList[i];
                }
            }

            return null;
        }
    }
}