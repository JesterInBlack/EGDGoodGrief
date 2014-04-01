using UnityEngine;
using System.Collections.Generic;
using System;

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

        // the time that the task was pushed.
        private float pushTime = -1;
        public float PushTime { get { return pushTime; } set { pushTime = value; } }
        // the time that the task was popped.
        private float popTime = -1;
        public float PopTime { get { return popTime; } set { popTime = value; } }

        public void copyFrom(NodeData nodeData)
        {
            nodeDesigner = nodeData.NodeDesigner;
            position = nodeData.Position;
            friendlyName = nodeData.FriendlyName;
            comment = nodeData.Comment;
            isBreakpoint = nodeData.IsBreakpoint;
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
            return dict;
        }

        public void deserialize(Dictionary<string, object> dict)
        {
            position = StringToVector2((string)dict["Position"]);
            if (dict.ContainsKey("FriendlyName")) {
                friendlyName = (string)dict["FriendlyName"];
            }
            if (dict.ContainsKey("Comment")) {
                comment = (string)dict["Comment"];
            }
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            var stringSplit = vector2String.Substring(1, vector2String.Length - 2).Split(',');
            return new Vector3(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]));
        }
    }
}