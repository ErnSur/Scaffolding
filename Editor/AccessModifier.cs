using System;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    [Flags]
    public enum AccessModifier
    {
        [InspectorName("public")]
        Public = 0,
        [InspectorName("private")]
        Private = 1,
        [InspectorName("protected")]
        Protected = 2,
        [InspectorName("internal")]
        Internal = 3,
        [InspectorName("protected internal")]
        ProtectedInternal = 4,
        [InspectorName("private protected")]
        PrivateProtected = 5
    }
}