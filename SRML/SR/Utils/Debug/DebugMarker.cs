using SRML.SR.Utils.BaseObjects;
using SRML.SR.Templates;
using UnityEngine;

namespace SRML.SR.Utils.Debug
{
    public class DebugMarker : MonoBehaviour
    {
        private MeshRenderer mesh;
        public MarkerType type;
        public float scaleMult;

        private bool hover = false;
        private string objName = string.Empty;

        private System.DateTime lastHover;
        GameObject areaMarker;

        private static GameObject hoverObj;

        public static GameObject HoverObj { get => hoverObj; }

        public void Start()
        {
            if (mesh == null)
                mesh = GetComponent<MeshRenderer>();

            if (type == MarkerType.SpawnPoint)
                objName = GetComponentInParent<SpawnResource>().name.Replace("(Clone)", "") + "." + transform.parent.name;
            else if (type == MarkerType.Plot)
                objName = GetComponentInParent<LandPlot>().name.Replace("(Clone)", "");
            else if (type == MarkerType.DroneNode)
                objName = GetComponentInParent<PathingNetwork>().transform.parent.name.Replace("(Clone)", "") + "." + transform.parent.name;
            else
                objName = transform.parent.name;

            transform.localScale = Vector3.one;

            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = Vector3.zero;
            col.size = Vector3.one;

            // Make Area Marker
            if (BaseObjects.BaseObjects.markerAreaMaterials.ContainsKey(type))
            {
                areaMarker = new GameObject("AreaMarker");
                areaMarker.transform.parent = transform;
                areaMarker.transform.localPosition = Vector3.zero;

                areaMarker.AddComponent<MeshFilter>().sharedMesh = BaseObjects.BaseObjects.cubeMesh;
                areaMarker.AddComponent<MeshRenderer>().sharedMaterial = BaseObjects.BaseObjects.markerAreaMaterials[type];

                areaMarker.transform.localScale = Vector3.one * scaleMult;
            }

            areaMarker?.SetActive(false);
            mesh.enabled = false;
        }

        public void Update()
        {
            if (DebugCommand.DebugMode != mesh.enabled)
            {
                mesh.enabled = DebugCommand.DebugMode;
                areaMarker?.SetActive(DebugCommand.DebugMode);
            }

            if (hover && (System.DateTime.Now - lastHover).TotalSeconds > Time.deltaTime)
            {
                hover = false;
                hoverObj = null;
            }
        }

        private void RemoveHover()
        {
            hover = false;
        }

        public void SetHover()
        {
            if (hoverObj != null && hoverObj != gameObject)
                hoverObj.GetComponent<DebugMarker>().RemoveHover();

            hover = true;
            lastHover = System.DateTime.Now;
            hoverObj = gameObject;
        }

        public void OnGUI()
        {
            if (hover && DebugCommand.DebugMode)
            {
                TextAnchor defAnc = GUI.skin.label.alignment;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUI.Label(new Rect(0, 10, Screen.width, 30), $"<size=16><b>MARKER INFO</b></size>");
                GUI.Label(new Rect(0, 35, Screen.width, 30), $"<size=16><b>Object:</b> {objName}</size>");
                GUI.Label(new Rect(0, 55, Screen.width, 30), $"<size=16><b>Type:</b> {type}</size>");

                GUI.skin.label.alignment = defAnc;
            }
        }
    }
}
