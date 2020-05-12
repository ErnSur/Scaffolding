using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    public class ScaffoldingSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string _editorPrefsKey = "quickeye.scaffolding";
        private static ScaffoldingSettings _instance;
        private static bool _serializationStarted;

        public static ScaffoldingSettings GetOrCreateSettings()
        {
            if (_instance)
                return _instance;

            _instance = CreateInstance<ScaffoldingSettings>();
            if (EditorPrefs.HasKey(_editorPrefsKey))
            {
                var json = EditorPrefs.GetString(_editorPrefsKey);
                JsonUtility.FromJsonOverwrite(json, _instance);
            }
            return _instance;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        [Header("Code preview theme")]
        [SerializeField, ColorUsage(false)]
        private Color _backgroundColor = new Color(0.118f, 0.118f, 0.118f);

        [SerializeField, ColorUsage(false)]
        private Color _accessModifierColor = new Color(0.411f, 0.411f, 0.411f);

        [SerializeField, ColorUsage(false)]
        private Color _bracketsColor = new Color(0.604f, 0.792f, 0.165f);

        [SerializeField, ColorUsage(false)]
        private Color _typeColor = new Color(0.306f, 0.788f, 0.69f);

        [SerializeField, ColorUsage(false)]
        private Color _identifierColor = new Color(0.863f, 0.863f, 0.843f);

        [Header("Code style")]
        [SerializeField]
        private FieldPrefixes _prefixes = new FieldPrefixes { @private = "_" };
        [SerializeField]
        private FieldStyles _styles = new FieldStyles { @private = CaseStyle.LowerCamelCase };

        [Header("Other")]
        [SerializeField]
        private string _defaultNamespace = "DefaultNamespace";

        [SerializeField]
        private TextAsset _scriptTemplate;

        [SerializeField, TypeName, Tooltip("You can drag a component or script here.")]
        private List<string> _ignoredTypes = new List<string>
        {
            "UnityEngine.Transform, UnityEngine.CoreModule",
            "UnityEngine.RectTransform, UnityEngine.CoreModule",
            "UnityEngine.MeshFilter, UnityEngine.CoreModule",
            "UnityEngine.CanvasRenderer, UnityEngine.UIModule"
        };

        public Color AccessModifierColor => _accessModifierColor;
        public Color BracketsColor => _bracketsColor;
        public Color TypeColor => _typeColor;
        public Color IdentifierColor => _identifierColor;
        public Color BackgroundColor => _backgroundColor;

        public FieldPrefixes Prefixes => _prefixes;
        public FieldStyles Styles => _styles;

        public TextAsset ScriptTemplate => _scriptTemplate;

        public IReadOnlyCollection<Type> IgnoredTypes { get; private set; }

        public string DefaultNamespace => _defaultNamespace;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            IgnoredTypes = new HashSet<Type>(_ignoredTypes.Select(n => Type.GetType(n)));
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_serializationStarted)
            {
                _serializationStarted = true;

                var json = JsonUtility.ToJson(this, true);
                EditorPrefs.SetString(_editorPrefsKey, json);

                _serializationStarted = false;
            }
        }
    }
}