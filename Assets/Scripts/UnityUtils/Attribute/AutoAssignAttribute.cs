using UnityEngine;
using System;

namespace UnityUtils.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoAssignAttribute : PropertyAttribute
    {
        public bool Required { get; }
        public AutoAssignScope Scope { get; }

        public AutoAssignAttribute(bool required = true, AutoAssignScope scope = AutoAssignScope.Self)
        {
            Required = required;
            Scope = scope;
        }
        
        public enum AutoAssignScope { Self, Children, Parent }
    }
}