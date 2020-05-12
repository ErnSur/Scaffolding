using System;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    [Serializable]
    public class SerializedField
    {
        public bool enabled;
        public Component reference;
        public string name;
        
        [SerializeField]
        private int _id;

        //this should be serializedObject path, ex: GameObjectA/Cube/MeshRenderer
        public int Id => _id;

        public SerializedField(Component reference, bool enabled)
        {
            this.reference = reference;
            this.enabled = enabled;
            name = ScaffoldingUtility.CreateFieldName(reference);
            _id = name.GetHashCode();
        }
    }
}