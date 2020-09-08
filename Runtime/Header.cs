using UnityEngine;

namespace BK.HierarchyHeader
{
    public enum HeaderType
    {
        Default, Dotted, Custom
    }

    public enum HeaderAlignment
    {
        Start, Center, End
    }

    public class Header : MonoBehaviour
    {
        public string title = "Header";

        [HideInInspector] public HeaderType type;
        [HideInInspector] public HeaderAlignment alignment;

        private void OnDrawGizmos()
        {
            //locking the postion
            transform.position = Vector3.zero;
        }
    }
}