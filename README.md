# Mesh Dent

NDMF plugin for applying mesh deformation with spherical shapes.

## Overview

Mesh Dent allows you to create dent/indent effects on SkinnedMeshRenderer meshes using configurable spherical volumes. The deformation is applied at build time through NDMF, with real-time preview support.

## Features

- Spherical and ellipsoid deformation shapes
- Multiple deformation modes: Radial, Normal, Surface
- Real-time preview in Unity Editor
- Scene view handles for intuitive editing
- Non-destructive workflow via NDMF

## Requirements

- Unity 2022.3 or later
- [NDMF](https://github.com/bdunderscore/ndmf) >= 1.10.0
- [Avatar Optimizer](https://github.com/anatawa12/AvatarOptimizer) >= 1.8.0

## Installation

### VPM (Recommended)

1. Add the VPM repository to VCC/ALCOM:
   ```
   https://naari3.github.io/mesh-dent/vpm.json
   ```
2. Install "Mesh Dent" from the package list

### Manual Installation

1. Download the latest release zip
2. Extract to your Unity project's `Packages` folder

## Usage

1. Add the `MeshDent` component to a GameObject with a SkinnedMeshRenderer
2. Add dent spheres in the Inspector
3. Adjust position, scale, and rotation using scene handles or Inspector values
4. Configure strength and falloff for desired effect

### Dent Modes

- **Radial**: Pushes vertices toward the sphere center
- **Normal**: Pushes vertices along their normals
- **Surface**: Moves vertices to the sphere surface

## License

MIT License - see [LICENSE](LICENSE) for details.
