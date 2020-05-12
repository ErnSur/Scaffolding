using UnityEditor;
using UnityEngine.UIElements;

namespace QuickEye.Scaffolding
{
    public class ScaffoldingSettingsProvider : SettingsProvider
    {
        private Editor _editor;

        public ScaffoldingSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            //m_CustomSettings = ScaffoldingSettings.GetSerializedSettings();
            _editor = Editor.CreateEditor(ScaffoldingSettings.GetOrCreateSettings());
        }

        public override void OnGUI(string searchContext)
        {
            _editor.OnInspectorGUI();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new ScaffoldingSettingsProvider("Preferences/QuickEye/Scaffolding", SettingsScope.User);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromSerializedObject(ScaffoldingSettings.GetSerializedSettings());
            return provider;
        }
    }
}