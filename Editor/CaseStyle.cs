using UnityEngine;

namespace QuickEye.Scaffolding
{
    public enum CaseStyle
    {
        UpperCamelCase = 0,
        [InspectorName("lowerCamelCase")]
        LowerCamelCase = 1
    }
}