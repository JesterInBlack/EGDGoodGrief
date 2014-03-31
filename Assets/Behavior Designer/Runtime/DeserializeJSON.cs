using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using MiniJSON;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    public class DeserializeJSON : UnityEngine.Object
    {
        private struct TaskField
        {
            public TaskField(Task t, FieldInfo f) { task = t; fieldInfo = f; }
            public Task task;
            public FieldInfo fieldInfo;
        }

        public static bool Deserialize(BehaviorSource behaviorSource)
        {
            bool changed = false;
            var taskIDs = new Dictionary<TaskField, List<int>>();
            var IDtoTask = new Dictionary<int, Task>();

            var dict = Json.Deserialize(behaviorSource.Serialization) as Dictionary<string, object>;
            // deserialize the variables first so the tasks can reference them
            if (dict.ContainsKey("Variables")) {
                var variables = new List<SharedVariable>();
                var variablesList = dict["Variables"] as IList;
                for (int i = 0; i < variablesList.Count; ++i) {
                    var sharedVariable = DeserializeSharedVariable(variablesList[i] as Dictionary<string, object>, behaviorSource);
                    variables.Add(sharedVariable);
                }
                behaviorSource.Variables = variables;
                changed = true;
            }
            if (dict.ContainsKey("EntryTask")) {
                behaviorSource.EntryTask = DeserializeTask(behaviorSource, dict["EntryTask"] as Dictionary<string, object>, ref IDtoTask, ref taskIDs);
                changed = true;
            }

            if (dict.ContainsKey("RootTask")) {
                behaviorSource.RootTask = DeserializeTask(behaviorSource, dict["RootTask"] as Dictionary<string, object>, ref IDtoTask, ref taskIDs);
                changed = true;
            }

            if (dict.ContainsKey("DetachedTasks")) {
                var detachedTasks = new List<Task>();
                foreach (Dictionary<string, object> detachedTaskDict in (dict["DetachedTasks"] as IEnumerable)) {
                    detachedTasks.Add(DeserializeTask(behaviorSource, detachedTaskDict, ref IDtoTask, ref taskIDs));
                }
                behaviorSource.DetachedTasks = detachedTasks;
                changed = true;
            }

            // deserialization is complete besides assigning the correct tasks based off of the id
            if (taskIDs.Count > 0) {
                foreach (TaskField taskField in taskIDs.Keys) {
                    var idList = taskIDs[taskField] as List<int>;
                    if (taskField.fieldInfo.FieldType.IsArray) { // task array
                        var taskList = Activator.CreateInstance(typeof(List<>).MakeGenericType(taskField.fieldInfo.FieldType.GetElementType())) as IList;
                        for (int i = 0; i < idList.Count; ++i) {
                            taskList.Add(IDtoTask[idList[i]]);
                        }
                        var taskArray = Array.CreateInstance(taskField.fieldInfo.FieldType.GetElementType(), taskList.Count);
                        taskList.CopyTo(taskArray, 0);
                        taskField.fieldInfo.SetValue(taskField.task, taskArray);
                    } else { // single task
                        taskField.fieldInfo.SetValue(taskField.task, IDtoTask[idList[0]]);
                    }
                }
            }

            return changed;
        }

        private static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, ref Dictionary<TaskField, List<int>> taskIDs)
        {
            Task task = null;
            try {
                var type = Type.GetType(dict["ObjectType"] as string);
                // if type is null try a different namespace
                if (type == null) {
                    type = Type.GetType(string.Format("{0}, Assembly-CSharp", dict["ObjectType"] as string));
                }

                // Change the type to an unknown type if the type doesn't exist anymore.
                if (type == null) {
                    if (dict.ContainsKey("Children")) {
                        type = typeof(UnknownParentTask);
                    } else {
                        type = typeof(UnknownTask);
                    }
                }
                task = ScriptableObject.CreateInstance(type) as Task;
            }
            catch (Exception /*e*/) { }

            // What happened?
            if (task == null) {
                Debug.Log("Error: task is null of type " + dict["ObjectType"]);
                return null;
            }

            task.hideFlags = HideFlags.HideAndDontSave;
            task.ID = Convert.ToInt32(dict["ID"]);
            IDtoTask.Add(task.ID, task);
            task.Owner = behaviorSource.Owner as Behavior;
#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
            task.NodeData = new NodeData();
            task.NodeData.deserialize(dict["NodeData"] as Dictionary<string, object>);

            // give a little warning if the task is an unknown type
            if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask))) {
                task.NodeData.FriendlyName = string.Format("Unknown {0}", task.NodeData.FriendlyName);
                task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task type renamed or deleted?{0}", (task.NodeData.Comment.Equals("") ? "" : string.Format("\0{0}", task.NodeData.Comment)));
            }
#endif
            var fields = task.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i) {
                if (dict.ContainsKey(fields[i].Name)) {
                    // If the field is type Task then the task ID was stored. Because the task may not exist yet, wait to reference the task until all deserialization has been completed
                    if (fields[i].FieldType.IsArray && (fields[i].FieldType.GetElementType().Equals(typeof(Task)) || fields[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) { // task array
                        var idStringList = dict[fields[i].Name] as IList;
                        var idList = new List<int>();
                        for (int j = 0; j < idStringList.Count; ++j) {
                            idList.Add(Convert.ToInt32(idStringList[j]));
                        }
                        taskIDs.Add(new TaskField(task, fields[i]), idList);
                    } else if (!fields[i].FieldType.IsArray && (fields[i].FieldType.Equals(typeof(Task)) || fields[i].FieldType.IsSubclassOf(typeof(Task)))) {
                        var idList = new List<int>();
                        idList.Add(Convert.ToInt32(dict[fields[i].Name]));
                        taskIDs.Add(new TaskField(task, fields[i]), idList);
                    } else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable))) {
                        fields[i].SetValue(task, DeserializeSharedVariable(dict[fields[i].Name] as Dictionary<string, object>, behaviorSource));
                    } else if (!fields[i].FieldType.IsArray && (fields[i].FieldType.Equals(typeof(UnityEngine.Object)) || fields[i].FieldType.IsSubclassOf(typeof(UnityEngine.Object))) ||
                        fields[i].FieldType.IsArray && (fields[i].FieldType.GetElementType().Equals(typeof(UnityEngine.Object)) || fields[i].FieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))) {
                        if (fields[i].FieldType.IsArray) {
                            var objectList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fields[i].FieldType.GetElementType())) as IList;
                            var objectValues = dict[fields[i].Name] as IList;
                            int id = 0;
                            for (int j = 0; j < objectValues.Count; ++j) {
                                var dictValue = (string)objectValues[j];
                                if (int.TryParse(dictValue, out id)) {
                                    objectList.Add(behaviorSource.Owner.DeserializeUnityObject(id));
                                } else if (dictValue != null && !dictValue.Equals("")) { // pre-version 1.1.1 - remove with version 1.2.
                                    if (dictValue[0].Equals('/') && task.Owner as Component != null) { // local path to the component
                                        var transform = GetTransform(task.Owner.transform, dictValue);
                                        if (fields[i].FieldType.Equals(typeof(GameObject))) {
                                            objectList.Add(transform.gameObject);
                                        } else {
                                            objectList.Add(transform.GetComponent(fields[i].FieldType));
                                        }
                                    } else {
                                        objectList.Add(Resources.Load(GetResourcePath(dictValue), fields[i].FieldType.GetElementType()));
                                    }
                                }
                            }
                            // copy to an array so SetValue will accept the new value
                            var objectArray = Array.CreateInstance(fields[i].FieldType.GetElementType(), objectList.Count);
                            objectList.CopyTo(objectArray, 0);
                            fields[i].SetValue(task, objectArray);
                        } else {
                            var dictValue = dict[fields[i].Name];
                            if (!dictValue.GetType().Equals(typeof(string))) {
                                fields[i].SetValue(task, behaviorSource.Owner.DeserializeUnityObject(Convert.ToInt32(dictValue)));
                            } else if (dictValue != null && !dictValue.Equals("")) { // pre-version 1.1.1 - remove with version 1.2.
                                if (((string)dictValue)[0].Equals('/') && task.Owner as Component != null) { // local path to the component
                                    var transform = GetTransform(task.Owner.transform, (string)dictValue);
                                    if (fields[i].FieldType.Equals(typeof(GameObject))) {
                                        fields[i].SetValue(task, transform.gameObject);
                                    } else {
                                        fields[i].SetValue(task, transform.GetComponent(fields[i].FieldType));
                                    }
                                } else {
                                    fields[i].SetValue(task, Resources.Load(GetResourcePath((string)dictValue), fields[i].FieldType));
                                }
                            }
                        }
                    } else { // the non object types
                        if (fields[i].FieldType.IsArray) {
                            var objectList = Activator.CreateInstance(typeof(List<>).MakeGenericType(fields[i].FieldType.GetElementType())) as IList;
                            var objectValues = dict[fields[i].Name] as IList;
                            if (fields[i].FieldType.GetElementType().Equals(typeof(int)) || fields[i].FieldType.GetElementType().Equals(typeof(float)) || fields[i].FieldType.GetElementType().Equals(typeof(string)) ||
                                fields[i].FieldType.GetElementType().Equals(typeof(bool)) || fields[i].FieldType.GetElementType().Equals(typeof(double))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(Convert.ChangeType(objectValues[j], fields[i].FieldType.GetElementType()));
                                }
                            } else if (fields[i].FieldType.GetElementType().IsSubclassOf(typeof(Enum))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(Enum.Parse(fields[i].FieldType.GetElementType(), (string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Vector2))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToVector2((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Vector3))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToVector3((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Vector4))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToVector4((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Quaternion))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToQuaternion((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Matrix4x4))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToMatrix4x4((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Color))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToColor((string)objectValues[j]));
                                }
                            } else if (fields[i].FieldType.GetElementType().Equals(typeof(Rect))) {
                                for (int j = 0; j < objectValues.Count; ++j) {
                                    objectList.Add(StringToRect((string)objectValues[j]));
                                }
                            } else {
                                Debug.Log("Unable to cast to type " + fields[i].FieldType);
                            }
                            // copy to an array so SetValue will accept the new value
                            var objectArray = Array.CreateInstance(fields[i].FieldType.GetElementType(), objectList.Count);
                            objectList.CopyTo(objectArray, 0);
                            fields[i].SetValue(task, objectArray);
                        } else {
                            if (fields[i].FieldType.IsSubclassOf(typeof(Enum))) {
                                fields[i].SetValue(task, Enum.Parse(fields[i].FieldType, (string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Vector2))) {
                                fields[i].SetValue(task, StringToVector2((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Vector3))) {
                                fields[i].SetValue(task, StringToVector3((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Vector4))) {
                                fields[i].SetValue(task, StringToVector4((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Quaternion))) {
                                fields[i].SetValue(task, StringToQuaternion((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Matrix4x4))) {
                                fields[i].SetValue(task, StringToMatrix4x4((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Color))) {
                                fields[i].SetValue(task, StringToColor((string)dict[fields[i].Name]));
                            } else if (fields[i].FieldType.Equals(typeof(Rect))) {
                                fields[i].SetValue(task, StringToRect((string)dict[fields[i].Name]));
                            } else {
                                try {
                                    fields[i].SetValue(task, Convert.ChangeType(dict[fields[i].Name], fields[i].FieldType));
                                }
                                catch (InvalidCastException /*e*/) { Debug.Log("Unable to cast to type " + fields[i].FieldType); }
                            }
                        }
                    }
                }
            }

            if (task.GetType().IsSubclassOf(typeof(ParentTask)) && dict.ContainsKey("Children")) {
                var parentTask = task as ParentTask;
                if (parentTask != null) {
                    foreach (Dictionary<string, object> childDict in (dict["Children"] as IEnumerable)) {
                        var child = DeserializeTask(behaviorSource, childDict, ref IDtoTask, ref taskIDs);
                        int index = (parentTask.Children == null ? 0 : parentTask.Children.Count);
                        parentTask.AddChild(child, index);
                    }
                }
            }

            return task;
        }

        private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, BehaviorSource behaviorSource)
        {
            SharedVariable sharedVariable = null;
            // the shared variable may be referencing the variable within the behavior
            if (behaviorSource != null && behaviorSource.Variables != null && dict["Name"] != null) {
                sharedVariable = behaviorSource.GetVariable(dict["Name"] as string);
            }

            if (sharedVariable == null) {
                var variableType = Type.GetType(dict["Type"] as string);
                // if type is null try a different namespace
                if (variableType == null) {
                    variableType = Type.GetType(string.Format("{0}, Assembly-CSharp", dict["Type"] as string));
                }
                sharedVariable = ScriptableObject.CreateInstance(variableType) as SharedVariable;
                sharedVariable.name = dict["Name"] as string;
                sharedVariable.IsShared = !sharedVariable.name.Equals("");
                sharedVariable.hideFlags = HideFlags.HideAndDontSave;
                if (dict.ContainsKey("Value")) {
                    switch (sharedVariable.ValueType) {
                        case SharedVariableTypes.Int:
                            sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(int)));
                            break;
                        case SharedVariableTypes.Float:
                            sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(float)));
                            break;
                        case SharedVariableTypes.Bool:
                            sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(bool)));
                            break;
                        case SharedVariableTypes.String:
                            sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(string)));
                            break;
                        case SharedVariableTypes.Vector2:
                            sharedVariable.SetValue(StringToVector2((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.Vector3:
                            sharedVariable.SetValue(StringToVector3((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.Vector4:
                            sharedVariable.SetValue(StringToVector4((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.Quaternion:
                            sharedVariable.SetValue(StringToQuaternion((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.Color:
                            sharedVariable.SetValue(StringToColor((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.Rect:
                            sharedVariable.SetValue(StringToRect((string)dict["Value"]));
                            break;
                        case SharedVariableTypes.GameObject:
                        case SharedVariableTypes.Transform:
                        case SharedVariableTypes.Object:
                            var dictValue = dict["Value"]; 
                            if (!dictValue.GetType().Equals(typeof(string))) {
                                sharedVariable.SetValue(behaviorSource.Owner.DeserializeUnityObject(Convert.ToInt32(dictValue)));
                            } else if (dictValue != null && !dictValue.Equals("")) { // pre-version 1.1.1 - remove with version 1.2.
                                var type = typeof(UnityEngine.Object);
                                if (sharedVariable.ValueType == SharedVariableTypes.GameObject) {
                                    type = typeof(GameObject);
                                } else if (sharedVariable.ValueType == SharedVariableTypes.Transform) {
                                    type = typeof(Transform);
                                }
                                if (((string)dictValue)[0].Equals('/') && behaviorSource.Owner != null && (behaviorSource.Owner.GetObject() as Behavior) != null) { // local path to the component
                                    var transform = GetTransform((behaviorSource.Owner.GetObject() as Behavior).transform, (string)dictValue);
                                    if (type.Equals(typeof(GameObject))) {
                                        sharedVariable.SetValue(transform.gameObject);
                                    } else {
                                        sharedVariable.SetValue(transform.GetComponent(type));
                                    }
                                } else {
                                    sharedVariable.SetValue(Resources.Load(GetResourcePath((string)dictValue), type));
                                }
                            }
                            break;
                    }
                }
            }

            return sharedVariable;
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            var stringSplit = vector2String.Substring(1, vector2String.Length - 2).Split(',');
            return new Vector2(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]));
        }

        private static Vector3 StringToVector3(string vector3String)
        {
            var stringSplit = vector3String.Substring(1, vector3String.Length - 2).Split(',');
            return new Vector3(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]));
        }

        private static Vector4 StringToVector4(string vector4String)
        {
            var stringSplit = vector4String.Substring(1, vector4String.Length - 2).Split(',');
            return new Vector4(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Quaternion StringToQuaternion(string quaternionString)
        {
            var stringSplit = quaternionString.Substring(1, quaternionString.Length - 2).Split(',');
            return new Quaternion(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Matrix4x4 StringToMatrix4x4(string matrixString)
        {
            var stringSplit = matrixString.Split(null);
            var matrix = new Matrix4x4();
            matrix.m00 = float.Parse(stringSplit[0]);
            matrix.m01 = float.Parse(stringSplit[1]);
            matrix.m02 = float.Parse(stringSplit[2]);
            matrix.m03 = float.Parse(stringSplit[3]);
            matrix.m10 = float.Parse(stringSplit[4]);
            matrix.m11 = float.Parse(stringSplit[5]);
            matrix.m12 = float.Parse(stringSplit[6]);
            matrix.m13 = float.Parse(stringSplit[7]);
            matrix.m20 = float.Parse(stringSplit[8]);
            matrix.m21 = float.Parse(stringSplit[9]);
            matrix.m22 = float.Parse(stringSplit[10]);
            matrix.m23 = float.Parse(stringSplit[11]);
            matrix.m30 = float.Parse(stringSplit[12]);
            matrix.m31 = float.Parse(stringSplit[13]);
            matrix.m32 = float.Parse(stringSplit[14]);
            matrix.m33 = float.Parse(stringSplit[15]);
            return matrix;
        }

        private static Color StringToColor(string colorString)
        {
            var stringSplit = colorString.Substring(6, colorString.Length - 7).Split(',');
            return new Color(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));
        }

        private static Rect StringToRect(string rectString)
        {
            var stringSplit = rectString.Substring(1, rectString.Length - 2).Split(',');
            return new Rect(float.Parse(stringSplit[0].Substring(2, stringSplit[0].Length - 2)), //x:0.00
                            float.Parse(stringSplit[1].Substring(3, stringSplit[1].Length - 3)), // y:0.00
                            float.Parse(stringSplit[2].Substring(7, stringSplit[2].Length - 7)), // width:0.00
                            float.Parse(stringSplit[3].Substring(8, stringSplit[3].Length - 8))); // height:0.00
        }

        private static string GetResourcePath(string path)
        {
            if (!path.Contains("/Resources/")) {
                return "";
            }
            int resourcesIndex = path.IndexOf("/Resources/") + 11;
            int extensionLength = Path.GetExtension(path).Length;
            return path.Substring(resourcesIndex, path.Length - resourcesIndex - extensionLength);
        }

        private static Transform GetTransform(Component obj, string path)
        {
            var pathSplit = path.Split('/');
            var objTransform = obj.transform;
            for (int i = 1; i < pathSplit.Length; ++i) {
                for (int j = 0; j < objTransform.childCount; ++j) {
                    if (objTransform.GetChild(j).name.Equals(pathSplit[i])) {
                        objTransform = objTransform.GetChild(j);
                        break;
                    }
                }
            }
            return objTransform;
        }
    }
}