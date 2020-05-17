using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using QuickEye.UIToolkit;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Linq;

namespace QuickEye.Scaffolding
{
    [Serializable]
    public class FieldsFromGameObjectListState : ISerializationCallbackReceiver
    {
        public GameObject target;

        public bool includeChildren = true;

        public List<SerializedField> fields = new List<SerializedField>();

        private FieldsFromGameObjectList _list;

        public void Apply(FieldsFromGameObjectList list)
        {
            _list = list;
            list.Target = target;
            list.IncludeChildren = includeChildren;
            list.Fields = fields;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_list == null) return;

            target = _list.Target;
            includeChildren = _list.IncludeChildren;
            fields = _list.Fields;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }

    public class FieldsFromGameObjectList : VisualElement
    {
        private const string _uxmlPath = "QuickEye/Scaffolding/FieldsFromGameObjectList";
        private const string _listItemUxmlPath = _uxmlPath + "-item";

        public event Action<GameObject> TargetChanged;

        [Q("target-field")]
        private ObjectField _targetField;

        [Q("includeChildren-toggle")]
        private Toggle _includeChildrenToggle;

        [Q("fields-container")]
        private ListView _fieldsListView;

        private VisualTreeAsset _listItemPrototype;

        private ScaffoldingSettings _settings;
        private List<SerializedField> _fields;

        public GameObject Target { get; set; }

        public bool IncludeChildren { get; set; } = true;

        public List<SerializedField> Fields
        {
            get => _fields;
            set
            {
                _fields = value;
                if (_fieldsListView != null)
                    _fieldsListView.itemsSource = _fields;
            }
        }

        public FieldsFromGameObjectList()
        {
            _settings = ScaffoldingSettings.GetOrCreateSettings();
            _listItemPrototype = Resources.Load<VisualTreeAsset>(_listItemUxmlPath);

            var tree = Resources.Load<VisualTreeAsset>(_uxmlPath);
            tree.CloneTree(this);
            this.AssignQueryResults(this);

            SetupTargetField();
            SetupListView();
            this.InitField(_includeChildrenToggle, OnIncludeChildrenChange, () => IncludeChildren);
        }

        private void UpdateFieldsData()
        {
            var componenets = Target == null
                ? Array.Empty<Component>()
                : _includeChildrenToggle.value
                ? Target.GetComponentsInChildren<Component>()
                : Target.GetComponents<Component>();

            var newFields = componenets
                .Where(c => !_settings.IgnoredTypes.Contains(c.GetType()))
                .Select(c => new SerializedField(c, true))
                .ToList();

            if (Fields == null)
                Fields = newFields;
            else
            {
                var existingFieldIds = new HashSet<int>(Fields.Select(f => f.Id));
                Fields = newFields.Select(f =>
                existingFieldIds.Contains(f.Id)
                ? Fields.First(ef => ef.Id == f.Id) : f
                ).ToList();
            }
        }

        private void SetupTargetField()
        {
            _targetField.objectType = typeof(GameObject);
            this.InitField(_targetField, OnTargetFieldChange, () => Target);
        }

        private void OnIncludeChildrenChange(ChangeEvent<bool> evt)
        {
            if (evt.target != _includeChildrenToggle) return;

            IncludeChildren = _includeChildrenToggle.value;

            UpdateFieldsData();
        }

        private void OnTargetFieldChange(ChangeEvent<Object> evt)
        {
            if (evt.target != _targetField) return;

            Target = evt.newValue as GameObject;

            UpdateFieldsData();
            TargetChanged?.Invoke(Target);
        }

        private void SetupListView()
        {
            _fieldsListView.makeItem = MakeListItem;
            _fieldsListView.bindItem = BindListItem;

            _fieldsListView.itemsSource = Fields;
        }

        private void BindListItem(VisualElement item, int index)
        {
            var fieldData = _fieldsListView.itemsSource[index] as SerializedField;
            item.userData = fieldData;

            var pingButton = item.Q<Button>("icon");

            var icon = EditorGUIUtility.ObjectContent(fieldData.reference, fieldData.reference.GetType()).image as Texture2D;
            pingButton.style.backgroundImage = icon;

            var nameField = item.Q<TextField>();
            nameField.value = fieldData.name;

            var toggle = item.Q<Toggle>();
            toggle.value = fieldData.enabled;
        }

        private VisualElement MakeListItem()
        {
            var item = _listItemPrototype.CloneTree();

            var pingButton = item.Q<Button>("icon");

            pingButton.clickable = new Clickable(() => EditorGUIUtility.PingObject(item.userData as UnityEngine.Object));

            var nameField = item.Q<TextField>();

            nameField.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != nameField) return;

                var fieldData = item.userData as SerializedField;

                fieldData.name = evt.newValue;
            });

            var toggle = item.Q<Toggle>();
            toggle.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (evt.target != toggle) return;
                if (evt.destinationPanel != this.panel) return;
                var fieldData = item.userData as SerializedField;
                item.Q("to-disable").SetEnabled(fieldData.enabled);
            });

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != toggle) return;

                var fieldData = item.userData as SerializedField;
                fieldData.enabled = evt.newValue;
                item.Q("to-disable").SetEnabled(fieldData.enabled);
            });

            return item;
        }

        public new class UxmlFactory : UxmlFactory<FieldsFromGameObjectList, UxmlTraits> { }
    }
}