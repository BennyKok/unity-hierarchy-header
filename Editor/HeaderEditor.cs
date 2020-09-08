using UnityEditor;
using UnityEngine;

namespace BK.HierarchyHeader.Editor
{
    public static class HeaderUtils
    {
        [MenuItem("GameObject/Group Selected %g")]
        private static void GroupSelected()
        {
            if (!Selection.activeTransform) return;
            var go = new GameObject(Selection.activeTransform.name + " Group");
            go.transform.SetSiblingIndex(Selection.activeTransform.GetSiblingIndex());

            go.transform.position = FindCenterPoint(Selection.transforms);

            go.transform.SetParent(Selection.activeTransform.parent);

            Undo.RegisterCreatedObjectUndo(go, "Group Selected");

            foreach (var transform in Selection.transforms)
            {
                Undo.SetTransformParent(transform, go.transform, "Group Selected");
            }

            Selection.activeGameObject = go;
        }

        private static Vector3 FindCenterPoint(Transform[] objects)
        {
            if (objects.Length == 0)
                return Vector3.zero;

            if (objects.Length == 1)
            {
                if (objects[0].TryGetComponent<Renderer>(out var ren))
                    return ren.bounds.center;
                else
                    return objects[0].transform.position;
            }

            var bounds = new Bounds(objects[0].transform.position, Vector3.zero);
            foreach (var transform in objects)
            {
                if (transform.TryGetComponent<Renderer>(out var ren) && ren.GetType() != typeof(ParticleSystemRenderer))
                    bounds.Encapsulate(ren.bounds);
                else
                    bounds.Encapsulate(transform.position);
            }
            return bounds.center;
        }

        [MenuItem("GameObject/Create Header", false, 0)]
        private static void CreateHeader()
        {
            var header = new GameObject();

            //Mark as EditorOnly, so it will not included in final build
            header.tag = "EditorOnly";
            header.AddComponent<Header>();

            //Hide the transform
            header.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;

            //Register undo
            Undo.RegisterCreatedObjectUndo(header, "Create Header");

            //Select the created header
            Selection.activeGameObject = header;
        }

        public static void UpdateAllHeader()
        {
            var targetType = HeaderSettings.Instance.type;
            var targetAlignment = HeaderSettings.Instance.alignment;
            var allHeader = GameObject.FindObjectsOfType<Header>();
            foreach (var header in allHeader)
            {
                header.type = targetType;
                header.alignment = targetAlignment;

                HeaderEditor.UpdateHeader(header);
            }
        }
    }

    [CustomEditor(typeof(Header))]
    public class HeaderEditor : UnityEditor.Editor
    {
        public static string GetSimpleTitle(char prefix, Header header)
        {
            var maxCharLength = HeaderSettings.Instance.maxLength;
            var charLength = maxCharLength - header.title.Length;

            var leftSize = 0;
            var rightSize = 0;
            switch (header.alignment)
            {
                case HeaderAlignment.Start:
                    leftSize = HeaderSettings.Instance.minPrefixLength;
                    rightSize = charLength - leftSize;
                    break;
                case HeaderAlignment.End:
                    rightSize = HeaderSettings.Instance.minPrefixLength;
                    leftSize = charLength - rightSize;
                    break;
                case HeaderAlignment.Center:
                    leftSize = charLength / 2;
                    rightSize = charLength / 2;
                    break;
            }

            string left = leftSize > 0 ? new string(prefix, leftSize) : "";
            string right = rightSize > 0 ? new string(prefix, rightSize) : "";

            return left + " " + header.title.ToUpper() + " " + right;
        }

        public static string GetFormattedTitle(Header header)
        {
            switch (header.type)
            {
                case HeaderType.Dotted:
                    return GetSimpleTitle('-', header);
            }
            return GetSimpleTitle('━', header);
        }

        public static void UpdateHeader(Header header)
        {
            header.name = GetFormattedTitle(header);
            EditorUtility.SetDirty(header);
        }

        private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;

        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;

        public void OnUndoRedo()
        {
            UpdateHeader(target as Header);
        }

        public override void OnInspectorGUI()
        {
            var settings = HeaderSettings.GetOrCreateSettings();
            var typeProperty = serializedObject.FindProperty("type");

            var header = target as Header;

            serializedObject.Update();

            var titleProperty = serializedObject.FindProperty("title");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(titleProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateHeader(header);
            }

            //Sync current header with settings
            if ((HeaderType)typeProperty.enumValueIndex != settings.type)
            {
                typeProperty.enumValueIndex = (int)settings.type;

                UpdateHeader(header);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Options"))
            {
                SettingsService.OpenProjectSettings("Project/BK/Hierarchy Header");
            }
            if (GUILayout.Button("Refresh"))
            {
                UpdateHeader(header);
                HeaderUtils.UpdateAllHeader();
            }
            if (GUILayout.Button("Create Empty"))
            {
                var o = new GameObject("Empty");
                o.transform.SetSiblingIndex((target as Header).transform.GetSiblingIndex() + 1);
                Undo.RegisterCreatedObjectUndo(o, "Create Empty");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}