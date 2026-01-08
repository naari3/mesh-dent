using System;
using Anatawa12.AvatarOptimizer.Processors.SkinnedMeshes;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using UnityEngine;

[assembly: ExportsPlugin(typeof(MeshDentPlugin))]

public class MeshDentPlugin : Plugin<MeshDentPlugin>
{
    public override string QualifiedName => "net.naari3.mesh-dent";
    public override string DisplayName => "Mesh Dent";

    protected override void Configure()
    {
        InPhase(BuildPhase.Transforming)
            .Run(MeshDentPass.Instance)
            .PreviewingWith(new MeshDentPreview());
    }
}

public class MeshDentPass : Pass<MeshDentPass>
{
    public override string DisplayName => "Apply Mesh Dent";

    protected override void Execute(BuildContext context)
    {
        foreach (var meshDent in context.AvatarRootObject.GetComponentsInChildren<MeshDent>(true))
        {
            var smr = meshDent.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) continue;

            var originalMesh = smr.sharedMesh;
            if (originalMesh == null) continue;
            if (meshDent.dentSpheres == null || meshDent.dentSpheres.Length == 0) continue;

            using (var meshInfo2 = new MeshInfo2(smr))
            {
                var actualPositions = new Vector3[meshInfo2.Vertices.Count];
                var worldToLocal = smr.transform.worldToLocalMatrix;

                for (int i = 0; i < meshInfo2.Vertices.Count; i++)
                {
                    actualPositions[i] = meshInfo2.Vertices[i].ComputeActualPosition(
                        meshInfo2,
                        t => t.localToWorldMatrix,
                        worldToLocal
                    );
                }

                var newMesh = UnityEngine.Object.Instantiate(originalMesh);
                newMesh.name = originalMesh.name + "_Dented";

                MeshDentProcessor.ApplyDent(newMesh, originalMesh, actualPositions, meshDent);

                smr.sharedMesh = newMesh;
            }

            UnityEngine.Object.DestroyImmediate(meshDent);
        }
    }
}

public static class MeshDentProcessor
{
    public static void Process(MeshDent meshDent)
    {
        var skinnedMeshRenderer = meshDent.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null) return;

        var originalMesh = skinnedMeshRenderer.sharedMesh;
        if (originalMesh == null) return;

        if (meshDent.dentSpheres == null || meshDent.dentSpheres.Length == 0) return;

        var newMesh = UnityEngine.Object.Instantiate(originalMesh);
        newMesh.name = originalMesh.name + "_Dented";

        ApplyDent(newMesh, originalMesh, originalMesh.vertices, meshDent);

        skinnedMeshRenderer.sharedMesh = newMesh;
    }

    public static void ApplyDent(Mesh targetMesh, Mesh originalMesh, Vector3[] actualPositions, MeshDent meshDent)
    {
        var vertices = originalMesh.vertices;
        var normals = originalMesh.normals;
        var newVerts = (Vector3[])vertices.Clone();

        foreach (var sphere in meshDent.dentSpheres)
        {
            if (sphere == null || !sphere.enabled) continue;

            Quaternion invRotation = Quaternion.Inverse(sphere.rotation);
            Vector3 invScale = new Vector3(
                1f / (sphere.scale.x * sphere.radius),
                1f / (sphere.scale.y * sphere.radius),
                1f / (sphere.scale.z * sphere.radius)
            );

            for (int i = 0; i < newVerts.Length; i++)
            {
                Vector3 actualPos = actualPositions[i];
                Vector3 localPos = invRotation * (actualPos - sphere.localPosition);
                Vector3 normalizedPos = Vector3.Scale(localPos, invScale);
                float dist = normalizedPos.magnitude;

                if (dist < 1f && dist > 0.0001f)
                {
                    Vector3 delta = CalculateDentDelta(actualPos, sphere, dist, normalizedPos, meshDent.dentMode, normals[i]);
                    newVerts[i] = vertices[i] + delta;
                }
            }
        }

        targetMesh.vertices = newVerts;
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

    private static Vector3 CalculateDentDelta(
        Vector3 actualPos,
        MeshDent.DentSphere sphere,
        float dist,
        Vector3 normalizedPos,
        MeshDent.DentMode mode,
        Vector3 normal)
    {
        float falloffFactor = Mathf.Pow(1 - dist, sphere.falloff);

        switch (mode)
        {
            case MeshDent.DentMode.Radial:
            {
                float blend = (1 - dist) * sphere.strength * falloffFactor;
                Vector3 surfaceLocal = normalizedPos.normalized;
                Vector3 surfaceWorld = sphere.rotation * new Vector3(
                    surfaceLocal.x * sphere.scale.x * sphere.radius,
                    surfaceLocal.y * sphere.scale.y * sphere.radius,
                    surfaceLocal.z * sphere.scale.z * sphere.radius
                ) + sphere.localPosition;
                return (surfaceWorld - actualPos) * blend;
            }
            case MeshDent.DentMode.Normal:
            {
                float pushAmount = (1 - dist) * sphere.strength * falloffFactor * sphere.radius;
                return -normal * pushAmount;
            }
            case MeshDent.DentMode.Surface:
            {
                float blend = sphere.strength * falloffFactor;
                Vector3 surfaceLocal = normalizedPos.normalized;
                Vector3 surfaceWorld = sphere.rotation * new Vector3(
                    surfaceLocal.x * sphere.scale.x * sphere.radius,
                    surfaceLocal.y * sphere.scale.y * sphere.radius,
                    surfaceLocal.z * sphere.scale.z * sphere.radius
                ) + sphere.localPosition;
                return (surfaceWorld - actualPos) * blend;
            }
        }
        return Vector3.zero;
    }
}
