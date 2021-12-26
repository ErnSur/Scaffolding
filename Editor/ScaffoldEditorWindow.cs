using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using QuickEye.UIToolkit;
using System.IO;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.ShortcutManagement;
#endif

namespace QuickEye.Scaffolding
{
    public class ScaffoldEditorWindow : EditorWindow
    {
        private static Font robotoMonoFont;
#if ASSET_FACTORY
        [CreateAssetEntry]
        private static CreateAssetStrategy CreateEntry()
        {
            return new CreateAssetStrategy<MonoScript>("Script/Script from game object", OpenWindowFromAssetFactory) { FileExtension
 = ".cs" };
        }
#endif
#if UNITY_2019_1_OR_NEWER
        [Shortcut("Tools/Scaffolding...", KeyCode.O, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
#endif
        [MenuItem("Assets/Create/Scaffolding...", false, -20)]
        private static void OpenWindow_Shortcut() => OpenWindow();

        private static ScaffoldEditorWindow OpenWindow()
        {
            var wnd = GetWindow<ScaffoldEditorWindow>(
#if ASSET_FACTORY
                typeof(AssetFactoryWindow)
#endif
            );
            wnd.titleContent = CreateWindowTitle();
            return wnd;
        }

        private static GUIContent CreateWindowTitle()
        {
            var name = "New Script...";
            var icon = EditorGUIUtility.IconContent("Project").image;
            return new GUIContent(name, icon);
        }

        private static void OpenWindowFromAssetFactory(string path)
        {
            var wnd = OpenWindow();
            wnd._fileLocationPanel.Directory = Path.GetDirectoryName(path);
            var target = wnd._fieldListState.target;
            wnd._fileLocationPanel.FileName = target != null ? target.name.Replace(" ", "") : "NewMonoBehaviour.cs";
        }

        #region SerializedData

        [SerializeField]
        private FieldsFromGameObjectListState _fieldListState = new FieldsFromGameObjectListState();

        [SerializeField]
        private bool _previewOn;

        [SerializeField]
        private int _selectedCategory;

        [SerializeField]
        private Vector2 _scrollPositionResult;

        #endregion

        private ScaffoldingSettings _settings;

        #region ViewElements

        [Q("content-page")]
        private VisualElement _contentPage;

        [Q("preview-content")]
        private VisualElement _previewContent;

        [Q("category-list")]
        private ListView _categoryListView;

        [Q("preview-toggle")]
        private VisualElement _previewToggle;

        [Q]
        private FieldsFromGameObjectList _fieldsList;

        [Q]
        private FileLocationPanel _fileLocationPanel;

        [Q("methods-content")]
        private VisualElement _methodsList;

        #endregion

        private void OnEnable()
        {
            SetupView();
        }

        private void SetupView()
        {
            _settings = ScaffoldingSettings.GetOrCreateSettings();
            robotoMonoFont = Resources.Load<Font>("QuickEye/Scaffolding/RobotoMono-Regular");

            var tree = Resources.Load<VisualTreeAsset>("QuickEye/Scaffolding/ScriptScaffolding");
            tree.CloneTree(rootVisualElement);

            rootVisualElement.AssignQueryResults(this);
            RegisterEventHandlers();
            InitCategoryListView();
            InitPreview();
            SetupFieldList();
        }

        private void RegisterEventHandlers()
        {
            _fileLocationPanel.AddClicked += CreateScript;
            _fileLocationPanel.CancelClicked += Close;
        }

        private void SetupFieldList()
        {
            if (!_fieldListState.target)
                _fieldListState.target = Selection.activeGameObject;

            _fieldListState.Apply(_fieldsList);
        }

        private void CreateScript()
        {
            var scriptData = new ScriptContent
            {
                @namespace = _settings.DefaultNamespace,
                typeName = Path.GetFileNameWithoutExtension(_fileLocationPanel.FileName).Replace(" ", ""),
                fields = GetScaffoldingText(false),
                usingNamespaces = _fieldsList.Fields.Select(f => f.reference.GetType().Namespace).Distinct().ToArray()
            };

            var template = _settings.ScriptTemplate.text;
            var path = Path.ChangeExtension(_fileLocationPanel.FullPath, ".cs");
            ScaffoldingUtility.CreateScript(scriptData, path, template);
#if ASSET_FACTORY
            GetWindow<AssetFactoryWindow>().Close();
#endif
            Close();
        }

        private void InitPreview()
        {
            _previewToggle.AddManipulator(new Clickable(() =>
            {
                _previewOn = !_previewOn;
                _previewToggle.ToggleInClassList("preview-toggle-on");

                var menu = rootVisualElement.Q("menu-content");
                menu.ToggleDisplayStyle(!_previewOn);
                _previewContent.ToggleDisplayStyle(_previewOn);
            }));

            var previewLabel = _previewToggle[0];

            previewLabel.transform.position = new Vector3(22, -17, 0);
            previewLabel.transform.rotation = Quaternion.Euler(0, 0, 90);

            var imguiContainer = _previewContent.Q<IMGUIContainer>();
            imguiContainer.onGUIHandler = ResultSection;
        }

        private void InitCategoryListView()
        {
            _categoryListView.makeItem = () =>
            {
                var item = new Label();
                item.AddToClassList("category-item");
                return item;
            };

            _categoryListView.bindItem = (item, index) =>
            {
                var category = ((string name, Action))_categoryListView.itemsSource[index];
                (item as Label).text = category.name;
            };

            _categoryListView.onSelectionChanged += OnSelectionChange;

            void OnSelectionChange(IEnumerable<object> selectedItems)
            {
                var category = ((string, Action action))selectedItems.FirstOrDefault();
                category.action?.Invoke();
            }

            _categoryListView.itemsSource = CreateCategories();
            _categoryListView.selectedIndex = _selectedCategory;
        }

        private (string name, Action action)[] CreateCategories()
        {
            return new (string name, Action action)[]
            {
                ("Fields", () => ToggleContent(_fieldsList)),
                ("Methods", () => ToggleContent(_methodsList))
            };
        }

        private void ToggleContent(VisualElement element)
        {
            if (element == null)
                return;
            element.ToggleDisplayStyle(true);
            foreach (var child in _contentPage.Children())
            {
                if (child != element)
                    child.ToggleDisplayStyle(false);
            }
        }

        private class Category
        {
            public string name;
            public VisualElement content;
        }

        private string GetScaffoldingText(bool richText) =>
            string.Join("\n\n", _fieldsList.Fields.Where(f => f.enabled)
                .Select(f =>
                    ScaffoldingUtility.GetFieldDeclarationLine(AccessModifier.Private, f.reference.GetType().Name,
                        f.name, richText)));

        private void ResultSection()
        {
            using (var s = new EditorGUILayout.ScrollViewScope(_scrollPositionResult))
            {
                GUI.backgroundColor = _settings.BackgroundColor;
                var richText = $"<color=white>{GetScaffoldingText(false)}</color>";
                Debug.Log($"MES: {_TextFieldConsolaStyle.font}");
                GUILayout.TextArea(richText, _TextFieldConsolaStyle);
                GUI.backgroundColor = Color.white;
                _scrollPositionResult = s.scrollPosition;
            }
        }

        private GUIStyle _TextFieldConsolaStyle => new GUIStyle("textfield")
        {
            font = robotoMonoFont,
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            richText = true
        };
    }
}