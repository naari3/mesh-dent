using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshDent))]
public class MeshDentEditor : Editor
{
    private int selectedSphereIndex = -1;

    private enum EditMode { Move, Rotate, Scale }
    private EditMode currentMode = EditMode.Move;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Edit Mode", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentMode == EditMode.Move, "Move", "Button"))
            currentMode = EditMode.Move;
        if (GUILayout.Toggle(currentMode == EditMode.Rotate, "Rotate", "Button"))
            currentMode = EditMode.Rotate;
        if (GUILayout.Toggle(currentMode == EditMode.Scale, "Scale", "Button"))
            currentMode = EditMode.Scale;
        EditorGUILayout.EndHorizontal();

        var meshDent = (MeshDent)target;
        if (meshDent.dentSpheres != null && meshDent.dentSpheres.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Sphere to Edit", EditorStyles.boldLabel);

            string[] options = new string[meshDent.dentSpheres.Length + 1];
            options[0] = "None";
            for (int i = 0; i < meshDent.dentSpheres.Length; i++)
            {
                options[i + 1] = $"Sphere {i}";
            }

            int selected = EditorGUILayout.Popup("Active Sphere", selectedSphereIndex + 1, options);
            selectedSphereIndex = selected - 1;
        }
    }

    private void OnSceneGUI()
    {
        var meshDent = (MeshDent)target;
        if (meshDent.dentSpheres == null) return;

        for (int i = 0; i < meshDent.dentSpheres.Length; i++)
        {
            var sphere = meshDent.dentSpheres[i];
            if (sphere == null) continue;

            Vector3 worldPos = meshDent.transform.TransformPoint(sphere.localPosition);
            Quaternion worldRot = meshDent.transform.rotation * sphere.rotation;

            float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;
            if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
            {
                selectedSphereIndex = i;
                Repaint();
            }

            if (i == selectedSphereIndex)
            {
                EditorGUI.BeginChangeCheck();

                switch (currentMode)
                {
                    case EditMode.Move:
                    {
                        Vector3 newWorldPos = Handles.PositionHandle(worldPos, worldRot);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(meshDent, "Move Dent Sphere");
                            sphere.localPosition = meshDent.transform.InverseTransformPoint(newWorldPos);
                            EditorUtility.SetDirty(meshDent);
                        }
                        break;
                    }

                    case EditMode.Rotate:
                    {
                        Quaternion newWorldRot = Handles.RotationHandle(worldRot, worldPos);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(meshDent, "Rotate Dent Sphere");
                            sphere.rotation = Quaternion.Inverse(meshDent.transform.rotation) * newWorldRot;
                            EditorUtility.SetDirty(meshDent);
                        }
                        break;
                    }

                    case EditMode.Scale:
                    {
                        Vector3 newScale = Handles.ScaleHandle(sphere.scale, worldPos, worldRot, HandleUtility.GetHandleSize(worldPos));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(meshDent, "Scale Dent Sphere");
                            sphere.scale = newScale;
                            EditorUtility.SetDirty(meshDent);
                        }
                        break;
                    }
                }

                Handles.color = sphere.gizmoColor;
                Handles.Label(worldPos + Vector3.up * sphere.radius * Mathf.Max(sphere.scale.x, sphere.scale.y, sphere.scale.z),
                    $"Sphere {i}");
            }
        }
    }
}
