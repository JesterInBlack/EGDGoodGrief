namespace BehaviorDesigner.Runtime.Tasks
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [System.Obsolete("Shared Task Attribute is deprecated. Use Behavior Designer variables instead.")]
    public class SharedFieldAttribute : System.Attribute
    {
        // Intentionally left blank
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [System.Obsolete("Synchronized Task Attribute is deprecated. Use Behavior Designer variables instead.")]
    public class SynchronizedFieldAttribute : System.Attribute
    {
        // Intentionally left blank
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LinkedTaskAttribute : System.Attribute
    {
        // Intentionally left blank
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InheritedFieldAttribute : System.Attribute
    {
        // Intentionally left blank
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TaskIconAttribute : System.Attribute
    {
        public string IconPath { get { return mIconPath; } }
        public readonly string mIconPath;
        public TaskIconAttribute(string iconPath) { mIconPath = iconPath; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HelpURLAttribute : System.Attribute
    {
        public string URL { get { return mURL; } }
        private readonly string mURL;
        public HelpURLAttribute(string url) { mURL = url; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TaskCategoryAttribute : System.Attribute
    {
        public string Category { get { return mCategory; } }
        public readonly string mCategory;
        public TaskCategoryAttribute(string category) { mCategory = category; }
    }
}