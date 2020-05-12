using System;
using UnityEditor;
using UnityEngine;

namespace QuickEye.Scaffolding
{
    [CustomPropertyDrawer(typeof(TypeNameAttribute))]
    public class TypePropertyDrawer : PropertyDrawer
    {
        private const float _statusRectWidth = 1;

        private Color _validTypeColor = new Color(0.306f, 0.729f, 0.337f);
        private Color _invalidTypeColor = new Color(0.988f, 0.243f, 0.212f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TryGetDraggedValue(position, out var type))
            {
                property.stringValue = type.AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            }

            CalculateLayout(position, out var textFieldRect, out var statusRect);

            EditorGUI.PropertyField(textFieldRect, property);

            DrawValidTypeStatus(statusRect, IsPropertyValueValid(property));
        }

        private static void CalculateLayout(Rect totalSpace, out Rect textFieldRect, out Rect statusRect)
        {
            textFieldRect = new Rect
            {
                x = totalSpace.x,
                y = totalSpace.y,
                height = totalSpace.height,
                width = totalSpace.width
            };

            statusRect = new Rect
            {
                x = totalSpace.x + EditorGUIUtility.labelWidth + 5,
                y = totalSpace.yMax - 1,
                height = _statusRectWidth,
                width = totalSpace.width - EditorGUIUtility.labelWidth - 7
            };
        }

        private void DrawValidTypeStatus(Rect rect, bool isValid)
        {
            EditorGUI.DrawRect(rect, isValid ? _validTypeColor : _invalidTypeColor);
        }

        private bool TryExtractType(UnityEngine.Object reference, out Type type)
        {
            type = reference is MonoScript script ? script.GetClass() : reference.GetType();
            return type != null;
        }

        private bool TryGetDraggedValue(Rect dropArea, out Type type)
        {
            type = null;
            Event currentEvent = Event.current;
            EventType currentEventType = currentEvent.type;

            if (!dropArea.Contains(currentEvent.mousePosition))
                return false;

            switch (currentEventType)
            {
                case EventType.DragUpdated:
                    if (TryExtractType(DragAndDrop.objectReferences[0], out _))
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    currentEvent.Use();
                    break;

                case EventType.Repaint:
                    if (DragAndDrop.visualMode == DragAndDropVisualMode.None ||
                        DragAndDrop.visualMode == DragAndDropVisualMode.Rejected)
                        break;

                    EditorGUI.DrawRect(dropArea, Color.grey);
                    break;

                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    TryExtractType(DragAndDrop.objectReferences[0], out type);
                    currentEvent.Use();
                    return true;
            }
            return false;
        }

        private bool IsPropertyValueValid(SerializedProperty property) =>
            Type.GetType(property.stringValue) != null;
    }
}