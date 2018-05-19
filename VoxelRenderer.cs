using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VoxelRenderer
{

    GameObject tooth;

    int[] meshCount;
    List<VoxelMesh> meshList;

    Hashtable table;
    Boolean hashTest = true;

    Material material;

    VoxelData data;

    public int X;
    public int Y;
    public int Z;

    public float scale = 1;

    float adjScale;

    public VoxelRenderer()
    {
        if (hashTest) table = new Hashtable();
        data = new VoxelData();

        X = data.GetX();
        Y = data.GetY();
        Z = data.GetZ();

        tooth = new GameObject("tooth");
        meshList = new List<VoxelMesh>();
        meshCount = new int[8];

        adjScale = scale * 0.5f;

        GenerateVoxelMesh();
        UpdateMesh();
    }

    //Cleare - Loops through all meshes in the list and clears all of their inside data.
    void Clear()
    {
        for (int i = 0; i < meshList.Count; i++)
        {
            meshList[i].Clear();
        }
    }

    //addMesh - for creating a new Voxel mesh object, given the type.
    //New Voxel Meshes are added to the meshList.
    void AddMesh(Type type)
    {
        meshList.Add(new VoxelMesh(type, meshCount[(int)type])
        {
            vertices = new List<Vector3>(),
            triangles = new List<int>()
        });
        meshCount[(int)type]++;
    }

    //getFreeMesh - checks if there is any mesh in meshList of Type type with free space, if not it creates a new mesh.
    int GetFreeMesh(int type)
    {
        for (int i = 0; i < meshList.Count; i++)
        {
            if (meshList[i].type == (Type)type && meshList[i].HasSpace()) //if there exists a mesh which is same type and has space, add to this mesh.
            {
                return i;
            }

        }
        //if there is no mesh to add to, create a new mesh given the type of the voxel.
        AddMesh((Type)type);
        return meshList.Count - 1;
    }

    //GenerateVoxelMesh - for adding all voxel data into meshes of same type, and storing these meshes in meshList.
    void GenerateVoxelMesh()
    {
        int count = 0;
        //loop through z, y and x axis of data
        for (int z = 0; z < data.Height; z++)
        {
            for (int y = 0; y < data.Depth; y++)
            {

                for (int x = 0; x < data.Width; x++)
                {
                    int type = data.GetCell(x, y, z) - 1;
                    //ignore Previous Decay and Empty Space
                    if (
                        type != -1 && type != 7
                        )
                    {
                        count++;
                        int meshNum = GetFreeMesh(type);
                        MakeCube(adjScale, new Vector3((float)x * scale, (float)y * scale, (float)z * scale), x, y, z, data, type, meshNum); //now we make the cube.
                    }
                }
            }
        }
        Debug.Log(count);
    }


    //MakeCube - Draws cubes
    void MakeCube(float cubeScale, Vector3 cubePos, int x, int y, int z, VoxelData data, int type, int meshNum)
    {
        int count = 0; //counts number of faces added to mesh.
        List<int> directions = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            if ((data.GetNeighbour(x, y, z, (Direction)i)!=type+1))
            {
                count += 1;
                MakeFace((Direction)i, cubeScale, cubePos, type, meshNum); //makes the face and adds to count.
                directions.Add(i);
            }
        }

        if (count != 0) //if a face has been added, add the position of the voxel and number of faces.
        {
            meshList[meshNum].AddCube(new Vector3(x, y, z), count);
            int hn = GetHashNum(new Vector3(x, y, z));
            if (hashTest)
            {
                table.Add(hn, meshNum);
                if (!table.ContainsKey(hn))
                {
                    table.Add(hn,meshNum);
                }
                else
                {
                    table[hn] = meshNum;
                }
            }
        }
    }

    //MakeFace - Draws faces 
    void MakeFace(Direction dir, float faceScale, Vector3 facePos, int type, int meshNum)
    {
        meshList[meshNum].AddFace(CubeMeshData.faceVertices(dir, faceScale, facePos), (int)dir); //adds face in voxelmesh object.
    }


    //MakeFace(2) - Draws face for newly revealed voxels
    int MakeFace(Direction dir, float faceScale, Vector3 facePos, int type)
    {
        if (hashTest)
        {
            if (table.ContainsKey(GetHashNum(facePos)))
            {
                int mn = (int)table[GetHashNum(facePos)];
                meshList[mn].AddExistingFace(CubeMeshData.faceVertices(dir, faceScale, facePos), facePos, (int)dir);
                return mn;
            }
        }
        else
        {
            for (int i = 0; i < meshList.Count; i++)
            {
                if (meshList[i].ContainsCube(facePos)) //voxel is already drawn somewhere
                {
                    meshList[i].AddExistingFace(CubeMeshData.faceVertices(dir, faceScale, facePos), facePos, (int)dir);
                    return i;
                }
            }
        }
        // voxel hasnt been drawn yet
        int meshNum = GetFreeMesh(type);
        meshList[meshNum].AddFace(CubeMeshData.faceVertices(dir, faceScale, facePos), (int)dir); //makes the face in voxelmesh object.
        meshList[meshNum].AddCube(facePos, 1); //adds the mesh to the list of voxels in the mesh
        if (hashTest) table.Add(GetHashNum(facePos), meshNum);
        return meshNum;
    }

    int GetHashNum(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;
        return (x + (y * X) + (z * X * Y));
    }


    //updateMesh - Loops through all meshes in the list and updates them.
    void UpdateMesh()
    {
        for (int i = 0; i < meshList.Count; i++)
        {
            meshList[i].UpdateMesh();
        }
    }

    public void XRayMode(bool set)
    {
        for (int i = 0; i < meshList.Count; i++)
        {
            if (meshList[i].type != (Type)3 && meshList[i].type != (Type)2)
            {
                meshList[i].MakeTransparent(set); //make others transparent
            }
        }
    }

    public int AddVoxel(RaycastHit hit)
    {
        int x = (int)(Math.Round(hit.point.x + (hit.normal.x / 2 * scale)) / scale); //gets position we want to add to, adds the normal since we want the cube infront of the one clicked.
        int y = (int)(Math.Round(hit.point.y + (hit.normal.y / 2 * scale)) / scale);
        int z = (int)(Math.Round(hit.point.z + (hit.normal.z / 2 * scale)) / scale);

        if (x < X && y < Y && z < Z && x >= 0 && y >= 0 && z>=0)
        {

            Vector3 groove = data.GetLocalGroove(new Vector3(x,y,z));
            x = (int)groove.x;
            y = (int)groove.y;
            z = (int)groove.z;

            if (data.GetCell(x, y, z) == 8) //remove green cube
            {
                Vector3 remove = new Vector3(x, y, z);
                int hn = GetHashNum(remove);
                int mn = (int)table[hn];
                meshList[mn].RemoveCube(remove);
                meshList[mn].UpdateMesh();
                table.Remove(hn);
            }
            int type = 6;
            int meshNum = GetFreeMesh(type);
            MakeCube(adjScale, new Vector3(x * scale, y * scale, z * scale), x, y, z, data, type, meshNum); //creates a cube given the xyz position
            meshList[meshNum].UpdateMesh(); //updates the mesh that has been added to.
            if (data.GetCell(x,y,z) == 8) //if this is where decay was
            {
                data.SetVoxel(x, y, z, 7);
                return 50;
            }
            else
            {
                data.SetVoxel(x, y, z, 7);
                return -10;
            }
        }
        else
        {
            return 0;
        }
    }

    public int RemoveVoxel(RaycastHit hit, int operation)
    {
        List<int> toUpdate = new List<int>(); //new list of meshes which are edited
        //gets the position of the cube that is clicked
        int x = (int)Math.Round((hit.point.x/scale) - (hit.normal.x / 2)); 
        int y = (int)Math.Round((hit.point.y/scale) - (hit.normal.y / 2));
        int z = (int)Math.Round((hit.point.z/scale) - (hit.normal.z / 2));

        //removing the old faces
        Vector3 voxel = new Vector3(x, y, z);
        int removed = data.GetCell(x, y, z)-1;

        if (operation == 0 || (operation == 1 && removed == 5))
        {

            if (hashTest)
            {
                if (table.ContainsKey(GetHashNum(voxel)))
                {
                    int num = (int)table[GetHashNum(voxel)];
                    meshList[num].RemoveCube(voxel);
                    data.SetVoxel(x, y, z, 0);
                    toUpdate.Add(num);
                    table.Remove(GetHashNum(voxel));

                    if(removed==2 || removed == 3) //decay
                    {
                        int type = 7;
                        int meshNum = GetFreeMesh(type);
                        MakeCube(adjScale, new Vector3(x * scale, y * scale, z * scale), x, y, z, data, type, meshNum);
                        meshList[meshNum].UpdateMesh();
                        toUpdate.Add(meshNum);
                        table[GetHashNum(voxel)] = meshNum;
                    }
                }
                else
                {
                    //UpdateMesh();
                    for (int i = 0; i < meshList.Count; i++)
                    {
                        if (meshList[i].voxelPosition.Contains(voxel))
                        {
                            meshList[i].UpdateMesh();
                        }
                    }

                    // Debug.Log(x + " " + y + " " + z);
                    return 0;
                }
            }
            else
            {
                for (int i = 0; i < meshList.Count; i++) //check which mesh it belongs to
                {
                    if (meshList[i].ContainsCube(voxel))
                    {
                        meshList[i].RemoveCube(voxel); //remove cube from the mesh
                        toUpdate.Add(i);
                        break;
                    }
                }
            }

            //drawing the new faces
            for (int i = 0; i < 6; i++)
            {
                int type = data.GetNeighbour(x, y, z, (Direction)i) - 1; //gets type of neighbour in direction
                if (type != -1 && type != 7) //if not -1, reveal the face behind it.
                {
                    toUpdate.Add(MakeFace(GetDirection(i), adjScale, GetVector(new Vector3(x, y, z) * scale, i), type));
                }
            }
            toUpdate = toUpdate.Distinct().ToList();
            for (int i = 0; i < toUpdate.Count; i++)

            {
                meshList[toUpdate[i]].UpdateMesh();
            }

            if (operation == 0)
            {
                //scoring element
                if (removed == 2 || removed == 3)
                {
                    data.SetVoxel(x, y, z, 8);
                    return 50; //if decay
                }
                else if (removed == 4)
                {
                    return -1000;
                }
                else if (removed ==5)
                {
                    return -10;
                }
                else if (removed == 6)
                {
                    return 0; //if filler
                }
                else return -10; //if healthy tooth
            }
            else
            {
                return 50;
            }
        }
        return 0;
    }

    public int RemovePlaque(RaycastHit hit)
    {
        List<int> toUpdate = new List<int>(); //new list of meshes which are edited
                                              //gets the position of the cube that is clicked
        int x = (int)Math.Round(hit.point.x - (hit.normal.x / 2));
        int y = (int)Math.Round(hit.point.y - (hit.normal.y / 2));
        int z = (int)Math.Round(hit.point.z - (hit.normal.z / 2));

        int removed = data.GetCell(x, y, z);

        if (removed == 6) //plaque
        {
            //removing the old faces
            Vector3 voxel = new Vector3(x, y, z);
            for (int i = 0; i < meshList.Count; i++) //check which mesh it belongs to
            {
                if (meshList[i].ContainsCube(voxel))
                {
                    meshList[i].RemoveCube(voxel); //remove cube from the mesh
                    toUpdate.Add(i);
                    break;

                }
            }
            data.SetVoxel(x, y, z, 1);
            //drawing new cube
            int meshNum = GetFreeMesh(1);
            MakeCube(adjScale, new Vector3(x, y, z)*scale, x, y, z, data, 0, meshNum);
            toUpdate.Add(meshNum);
            toUpdate = toUpdate.Distinct().ToList();
            for (int i = 0; i < toUpdate.Count; i++)
            {
                meshList[toUpdate[i]].UpdateMesh();
            }
            return 100;
        }
        else
        {
            return 0;
        }
    }

    //getDirection - takes direction being checked, and returns direction to be drawn.
    Direction GetDirection(int prev)
    {
        if (prev == 0) return (Direction)2;
        else if (prev == 1) return (Direction)3;
        else if (prev == 2) return (Direction)0;
        else if (prev == 3) return (Direction)1;
        else if (prev == 4) return (Direction)5;
        else return (Direction)4;
    }
    //getVector - takes the direction and the vector and returns cube to be drawn
    Vector3 GetVector(Vector3 v, int dir)
    {
        if (dir == 0) return new Vector3(v.x, v.y, v.z + 1);
        else if (dir == 1) return new Vector3(v.x + 1, v.y, v.z);
        else if (dir == 2) return new Vector3(v.x, v.y, v.z - 1);
        else if (dir == 3) return new Vector3(v.x - 1, v.y, v.z);
        else if (dir == 4) return new Vector3(v.x, v.y + 1, v.z);
        else return new Vector3(v.x, v.y - 1, v.z);

    }
}