using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class NodeData
    {
        // a reference to the node designer when in the editor
        [SerializeField]
        private object nodeDesigner;
        public object NodeDesigner { get { return nodeDesigner; } set { nodeDesigner = value; } }

        // the position within the graph
        [SerializeField]
        private Vector2 position;
        public Vector2 Position { get { return position; } set { position = value; } }

        [SerializeField]
        private string friendlyName = "";
        public string FriendlyName { get { return friendlyName; } set { friendlyName = value; } }

        [SerializeField]
        private string comment = "";
        public string Comment { get { return comment; } set { comment = value; } }

        // is the current task a breakpoint
        [SerializeField]
        private bool isBreakpoint = false;
        public bool IsBreakpoint { get { return isBreakpoint; } set { isBreakpoint = value; } }

        [SerializeField]
        private Texture icon;
        public Texture Icon { get { return icon; } set { icon = value; } }

        [SerializeField]
        private bool collapsed = false;
        public bool Collapsed { get { return collapsed; } set { collapsed = value; } }

        [SerializeField]
        private bool disabled = false;
        public bool Disabled { get { return disabled; } set { disabled = value; } }

        // keep a separate list of the fields that will serialize properly
        [SerializeField]
        private List<string> watchedFieldNames = null;
        private List<FieldInfo> watchedFields = null;
        public List<FieldInfo> WatchedFields { get { return watchedFields; } }

        // the time that the task was pushed.
        private float pushTime = -1;
        public float PushTime { get { return pushTime; } set { pushTime = value; } }
        // the time that the task was popped.
        private float popTime = -1;
        public float PopTime { get { return popTime; } set { popTime = value; } }

        private TaskStatus executionStatus = TaskStatus.Inactive;
        public TaskStatus ExecutionStatus { get { return executionStatus; } set { executionStatus = value; } }

        public void initWatchedFields(Task task)
        {
            if (watchedFieldNames != null && watchedFieldNames.Count > 0) {
                watchedFields = new List<FieldInfo>();
                for (int i = 0; i < watchedFieldNames.Count; ++i) {
                    var field = task.GetType().GetField(watchedFieldNames[i]);
                    if (field != null) {
                        watchedFields.Add(field);
                    }
                }
            }
        }

        public void copyFrom(NodeData nodeData, Task task)
        {
            nodeDesigner = nodeData.NodeDesigner;
            position = nodeData.Position;
            friendlyName = nodeData.FriendlyName;
            comment = nodeData.Comment;
            isBreakpoint = nodeData.IsBreakpoint;
            collapsed = nodeData.Collapsed;
            disabled = nodeData.Disabled;
            if (nodeData.WatchedFields != null && nodeData.WatchedFields.Count > 0) {
                watchedFields = new List<FieldInfo>();
                watchedFieldNames = new List<string>();
                for (int i = 0; i < nodeData.watchedFields.Count; ++i) {
                    var field = task.GetType().GetField(nodeData.WatchedFields[i].Name);
                    if (field != null) {
                        watchedFields.Add(field);
                        watchedFieldNames.Add(field.Name);
                    }
                }
            }
        }

        public bool containsWatchedField(FieldInfo field)
        {
            return (watchedFields != null && watchedFields.Contains(field));
        }

        public void addWatchedField(FieldInfo field)
        {
            if (watchedFields == null) {
                watchedFields = new List<FieldInfo>();
                watchedFieldNames = new List<string>();
            }

            watchedFields.Add(field);
            watchedFieldNames.Add(field.Name);
        }

        public void removeWatchedField(FieldInfo field)
        {
            if (watchedFields != null) {
                watchedFields.Remove(field);
                watchedFieldNames.Remove(field.Name);
            }
        }

        public Dictionary<string, object> serialize()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("Position", position);
            if (friendlyName.Length > 0) {
                dict.Add("FriendlyName", friendlyName);
            }
            if (comment.Length > 0) {
                dict.Add("Comment", comment);
            }
            if (collapsed) {
                dict.Add("Collapsed", collapsed);
            }
            if (disabled) {
                dict.Add("Disabled", disabled);
            }
            if (watchedFieldNames != null && watchedFieldNames.Count > 0) {
                dict.Add("WatchedFields", watchedFieldNames);
            }
            return dict;
        }

        public void deserialize(Dictionary<string, object> dict, Task task)
        {
            position = StringToVector2((string)dict["Position"]);
            if (dict.ContainsKey("FriendlyName")) {
                friendlyName = (string)dict["FriendlyName"];
            }
            if (dict.ContainsKey("Comment")) {
                comment = (string)dict["Comment"];
            }
            if (dict.ContainsKey("Collapsed")) {
                collapsed = Convert.ToBoolean(dict["Collapsed"]);
            }
            if (dict.ContainsKey("Disabled")) {
                collapsed = Convert.ToBoolean(dict["Disabled"]);
            }
            if (dict.ContainsKey("WatchedFields")) {
                watchedFieldNames = new List<string>();
                watchedFields = new List<FieldInfo>();

                var objectValues = dict["WatchedFields"] as IList;
                for (int i = 0; i < objectValues.Count; ++i) {
                    var field = task.GetType().GetField((string)objectValues[i]);
                    if (field != null) {
                        watchedFieldNames.Add(field.Name);
                        watchedFields.Add(field);
                    }
                }
            }
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            var stringSplit = vector2String.Substring(1, vector2String.Length - 2).Split(',');
            return new Vector3(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]));
        }
    }
}