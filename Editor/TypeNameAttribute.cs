using System;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class TypeNameAttribute : PropertyAttribute
    {

    }
}