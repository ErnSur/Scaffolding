using System;
using System.Globalization;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    public class ScaffoldingRichTextFormatter : IFormatProvider, ICustomFormatter
    {
        private ScaffoldingSettings _settings;

        public ScaffoldingRichTextFormatter(ScaffoldingSettings settings)
        {
            _settings = settings;
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) return string.Empty;

            var text = arg.ToString();

            switch (format)
            {
                case "b":
                    return Tint(text, _settings.BracketsColor);
                case "t":
                    return Tint(text, _settings.TypeColor);
                case "m":
                    return Tint(text, _settings.AccessModifierColor);
                case "i":
                    return Tint(text, _settings.IdentifierColor);
                default:
                    if (arg is IFormattable)
                        return ((IFormattable)arg).ToString(format, CultureInfo.CurrentCulture);
                    break;
            }
            return arg.ToString();
        }

        private static string Tint(string content, Color color)
        {
            return $"{Tag($"color=#{ColorUtility.ToHtmlStringRGB(color)}", content)}";
        }

        private static string Tag(string tag, string content)
        {
            var tagValueIndex = tag.IndexOf('=');
            var substringLength = tagValueIndex != -1 ? tagValueIndex : tag.Length;
            return $"<{tag}>{content}</{tag.Substring(0, substringLength)}>";
        }
    }
}