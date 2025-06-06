using System;

namespace DigDig2.Debug
{
    public enum DebugMenuToggleable
    {
        toggleable,
        non_toggleable
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DebugAttribute : Attribute
    {
        public DebugMenuToggleable IsToggelable { get; }

        public DebugAttribute(DebugMenuToggleable isToggelable = DebugMenuToggleable.toggleable)
        {
            IsToggelable = isToggelable;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DebugSerializedAttribute : Attribute
    {
    }
}
