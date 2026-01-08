using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using UnityEngine;

public class MeshDentPreview : IRenderFilter
{
    public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
    {
        var targets = new List<RenderGroup>();

        foreach (var meshDent in context.GetComponentsByType<MeshDent>())
        {
            context.Observe(meshDent);

            var smr = meshDent.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                targets.Add(RenderGroup.For(smr));
            }
        }

        return targets.ToImmutableList();
    }

    public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
    {
        return Task.FromResult<IRenderFilterNode>(new MeshDentPreviewNode());
    }
}

public class MeshDentPreviewNode : IRenderFilterNode
{
    public RenderAspects WhatChanged => RenderAspects.Mesh;

    public void OnFrame(Renderer original, Renderer proxy)
    {
        if (original == null || proxy == null) return;

        var meshDent = original.GetComponent<MeshDent>();
        if (meshDent == null || !meshDent.enabled) return;

        var originalSmr = original as SkinnedMeshRenderer;
        var proxySmr = proxy as SkinnedMeshRenderer;
        if (originalSmr == null || proxySmr == null) return;

        var originalMesh = originalSmr.sharedMesh;
        if (originalMesh == null) return;

        if (meshDent.dentSpheres == null || meshDent.dentSpheres.Length == 0) return;

        var bakedMesh = new Mesh();
        originalSmr.BakeMesh(bakedMesh);
        var actualPositions = bakedMesh.vertices;

        var previewMesh = Object.Instantiate(originalMesh);
        MeshDentProcessor.ApplyDent(previewMesh, originalMesh, actualPositions, meshDent);
        proxySmr.sharedMesh = previewMesh;

        Object.DestroyImmediate(bakedMesh);
    }

    public void Dispose() { }
}
