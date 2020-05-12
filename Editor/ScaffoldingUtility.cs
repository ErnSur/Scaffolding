using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    public class ScriptContent
    {
        public string[] usingNamespaces;
        public string @namespace;
        public string typeName;
        public string fields;
        public string methods;
    }

    public static class ScaffoldingUtility
    {
        private static ScaffoldingSettings _settings;
        private static ScaffoldingRichTextFormatter _formatProvider;

        static ScaffoldingUtility()
        {
            _settings = ScaffoldingSettings.GetOrCreateSettings();
            _formatProvider = new ScaffoldingRichTextFormatter(_settings);
        }

        public static UnityEngine.Object CreateScript(ScriptContent data, string path, string template)
        {
            var scriptContent = GenerateScriptTextContent(data, template);
            var fullPath = Path.GetFullPath(path);
            File.WriteAllText(fullPath, scriptContent, new System.Text.UTF8Encoding());

            // Import the asset
            AssetDatabase.ImportAsset(path);

            return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }

        public static string GenerateScriptTextContent(ScriptContent data, string template)
        {
            var usingsEndIndex = template.IndexOf("#USINGSEND#");

            foreach (var namescp in data.usingNamespaces)
            {
                if (string.IsNullOrEmpty(namescp)) continue;
                if (template.LastIndexOf(namescp, usingsEndIndex) == -1)
                {
                    var newLine = $"\nusing {namescp};";
                    template = template.Insert(usingsEndIndex, newLine);
                    usingsEndIndex += newLine.Length;
                }
            }
            template = template.Replace("#USINGSEND#", string.Empty);

            var @namespace = data.@namespace;

            template = template.Replace("#NAMESPACE#", @namespace);

            template = template.Replace("#SCRIPTNAME#", data.typeName);

            var fieldsIndex = template.IndexOf("#FIELDS#");
            var endOfLineIndex = template.LastIndexOf('\n', fieldsIndex);
            var indent = new string(' ', fieldsIndex - endOfLineIndex - 1);
            var indentedFields = data.fields.Replace("\n", Environment.NewLine + indent);
            template = template.Replace("#FIELDS#", indentedFields);


            template = template.Replace("#METHODS#", data.methods);

            template = template.Replace("#NOTRIM#", "");

            return template;
        }

        public static string GetFieldDeclarationLine(AccessModifier accessModifier, string typeName, string fieldName, bool richText)
        {
            fieldName = FormatFieldName(accessModifier, fieldName);

            IFormatProvider format = richText ? _formatProvider : null;

            return string.Format(format,
                "{0:b}{1:t}{2:b}\n{3:m} {4:t} {5:i}",
                "[", "SerializeField", "]",
                accessModifier.ToString().ToLower(), typeName, fieldName + ";");
        }

        private static string FormatFieldName(AccessModifier modifier, string name)
        {
            var prefix = _settings.Prefixes[modifier];
            var style = _settings.Styles[modifier];
            switch (style)
            {
                case CaseStyle.LowerCamelCase: return prefix + ToLowerCamelCase(name);
                case CaseStyle.UpperCamelCase: return prefix + name;
            }
            return string.Empty;
        }

        private static string ToLowerCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        public static string CreateFieldName(UnityEngine.Object reference)
        {
            var typeName = reference.GetType().Name;
            var gameobjectName = reference.name;
            string result;

            //switch (reference)
            //{
            //    case TMPro.TextMeshProUGUI _:
            //    case TMPro.TMP_Text _:
            //        typeName = "Text";
            //        break;
            //}

            switch (gameobjectName)
            {
                case "Text (TMP)":
                    gameobjectName = "Text";
                    break;
            }

            if (gameobjectName.ToLower().Contains(typeName.ToLower()))
            {
                result = gameobjectName;
            }
            else if (typeName.ToLower().Contains(gameobjectName.ToLower()))
            {
                result = typeName;
            }
            else
            {
                result = reference.name + typeName;
            }

            return Regex.Replace(result, @"\s+|(\(|\))", "");
        }
    }
#if PACKAGE_TMPRO
    public abstract class FieldNameFormatter : ScriptableObject
    {
        public abstract string Format(UnityEngine.Object reference, string name);

        public class Context
        {
            UnityEngine.Object reference;
            public string gameObjectName;
            public string typeName;
        }
    }

    public class TextMeshProFieldNameFormatter
    {
        public string Format(UnityEngine.Object reference, string name)
        {
            string typeName = null;
            switch (reference)
            {
                case TMPro.TextMeshProUGUI _:
                case TMPro.TMP_Text _:
                    typeName = "Text";
                    break;
            }
            return null;
        }

        public bool TryFormat(object reference, ref List<string> words)
        {
            if (reference is TMPro.TMP_Text txt)
                return false;

            //words.Add()

            return true;
        }
    }
#endif
}