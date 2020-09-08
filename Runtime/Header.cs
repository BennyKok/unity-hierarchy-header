using UnityEngine;

namespace BK.HierarchyHeader
{
    public enum HeaderType
    {
        Default, Dotted
    }

    public class Header : MonoBehaviour
    {
        public string title = "Header";

        [HideInInspector]
        public HeaderType type;

        private void OnValidate()
        {
            UpdateHeader();
        }

        public void UpdateHeader()
        {
            //Update our header title
            var header = GetHeader();
            gameObject.name = header + " " + title.ToUpper() + " " + header;
        }

        private void OnDrawGizmos()
        {
            //locking the postion
            transform.position = Vector3.zero;
        }

        private string GetHeader()
        {
            switch (type)
            {
                case HeaderType.Dotted: return "------------";
            }
            return "━━━━━━━━";
        }
    }
}