using UnityEngine;
using System;

[AddComponentMenu("MeshDent/Mesh Dent")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
[DisallowMultipleComponent]
public class MeshDent : MonoBehaviour
{
    public enum DentMode
    {
        Radial,
        Normal,
        Surface
    }

    [Serializable]
    public class DentSphere
    {
        public bool enabled = true;
        public Vector3 localPosition;
        public Vector3 scale = Vector3.one;      // 楕円体のスケール（1,1,1で球体）
        public Quaternion rotation = Quaternion.identity;  // 楕円体の回転
        public float radius = 0.1f;              // 基本半径
        public float strength = 1.0f;
        public float falloff = 1.0f;
        public Color gizmoColor = Color.red;
    }

    public DentMode dentMode = DentMode.Surface;
    public DentSphere[] dentSpheres = new DentSphere[0];

    void OnEnable() { }

    void OnDrawGizmosSelected()
    {
        if (dentSpheres == null) return;

        foreach (var sphere in dentSpheres)
        {
            if (sphere == null) continue;

            // 楕円体のGizmo描画
            Vector3 worldPos = transform.TransformPoint(sphere.localPosition);
            Quaternion worldRot = transform.rotation * sphere.rotation;
            Vector3 worldScale = Vector3.Scale(transform.lossyScale, sphere.scale * sphere.radius);

            Matrix4x4 matrix = Matrix4x4.TRS(worldPos, worldRot, worldScale);

            float alpha = sphere.enabled ? 0.3f : 0.1f;
            Color wireColor = sphere.enabled ? sphere.gizmoColor : Color.gray;

            Gizmos.matrix = matrix;
            Gizmos.color = new Color(sphere.gizmoColor.r, sphere.gizmoColor.g, sphere.gizmoColor.b, alpha);
            Gizmos.DrawSphere(Vector3.zero, 1f);
            Gizmos.color = wireColor;
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
