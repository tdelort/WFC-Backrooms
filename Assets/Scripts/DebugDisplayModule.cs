using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public class DebugDisplayModule : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text modelName;
        [SerializeField] TMPro.TMP_Text rotation;
        [SerializeField] TMPro.TMP_Text xFlipped;
        [SerializeField] TMPro.TMP_Text north;
        [SerializeField] TMPro.TMP_Text east;
        [SerializeField] TMPro.TMP_Text south;
        [SerializeField] TMPro.TMP_Text west;

        [SerializeField] MeshFilter meshFilter;
        [SerializeField] Transform meshTransform;

        public void Init(Prototype prototype)
        {
            modelName.text = prototype.mesh_name;
            rotation.text = "rotation : " + prototype.mesh_rotation.ToString();
            xFlipped.text = "xFlipped : " + prototype.mesh_is_xflipped.ToString();
            north.text = prototype.north;
            east.text = prototype.east;
            south.text = prototype.south;
            west.text = prototype.west;

            Mesh mesh = Resources.Load<Mesh>("Models/" + prototype.mesh_name);
            meshFilter.mesh = mesh;

            if (prototype.mesh_is_xflipped)
                meshTransform.localScale = Vector3.Scale(new Vector3(-1, 1, 1), meshTransform.localScale);

            meshTransform.Rotate(Vector3.up * 90 * prototype.mesh_rotation);   
        }
    }
}
