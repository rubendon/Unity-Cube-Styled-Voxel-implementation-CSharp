# Unity-Cube-Styled-Voxel-implementation-CSharp
C# script containing methods to read, represent and manipulate Voxel data obtained from xmt scans.
Scans should be in '.mask' format, which contains:
  - a 13-bit header representing the object dimensions.
  - a byte representation of the remaining values.

To use your own input data initiate the 3D 'data' array in 'VoxelData'.
The system uses a texture map 'Atlas.png',
