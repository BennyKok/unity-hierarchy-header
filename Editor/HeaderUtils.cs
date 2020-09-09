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

            //Update the header
            HeaderEditor.UpdateHeader(header.GetComponent<Header>());

            //Register undo
            Undo.RegisterCreatedObjectUndo(header, "Create Header");

            //Select the created header
            Selection.activeGameObject = header;
        }
    }
}