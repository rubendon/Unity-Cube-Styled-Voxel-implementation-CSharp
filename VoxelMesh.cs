using System.Collections.Generic;
using UnityEngine;

public class VoxelMesh{

    //Mesh components
    public GameObject go;
    public Mesh mesh;
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvList;
    public Material material;
    public Material transparent;

    //voxels stored
    public List<Vector3> voxelPosition;
    public List<int> numberOfFaces;

    //specific details
    public Type type;
    public int MeshCount;

    //UV Mapping
    public float pixelSize = 6;
    public float tileX = 9; 
    public float tileY = 9;
    float tilePerc;
    float umin;
    float umax;
    float vmin;
    float vmax;

    //Constructor - Initializes variables and creates mesh
    public VoxelMesh(Type toothType, int meshCount)
    {
        type = toothType;

        mesh = new Mesh(); //initializes lists
        vertices = new List<Vector3>();
        triangles = new List<int>();
        voxelPosition = new List<Vector3>();
        numberOfFaces = new List<int>();
        uvList = new List<Vector2>();

        go = GameObject.Find(type.ToString() + " " + MeshCount);
        go = new GameObject(type.ToString() + " " + meshCount); //adds components to game object
        go.AddComponent<MeshRenderer>();

        if (type != (Type)7)
        {
            material = Resources.Load("Materials/Atlas") as Material; //set material
            setUVXY();
        }
        else
        {
            material = Resources.Load("Materials/removedFillerMat") as Material;
        }
        go.isStatic = true;

        go.AddComponent<MeshFilter>();
        go.GetComponent<MeshRenderer>().sharedMaterial = material;

        go.AddComponent<MeshCollider>();
        go.GetComponent<MeshCollider>().convex = true;
        go.GetComponent<MeshCollider>().isTrigger = true;
        go.AddComponent<SaveMeshInEditor>();

        go.tag = "tooth";
        go.transform.parent = GameObject.Find("tooth").transform;

        LoadMaterials(toothType);
    }

    public void setUVXY()
    {
        if ((int)type==0) //dentine
        {
            tileX = 1;
            tileY = 0;
        }
        if ((int)type == 1) //enamel
        {
            tileX = 0;
            tileY = 0;
        }
        if ((int)type == 2) //decay
        {
            tileX = 2;
            tileY = 0;
        }
        if ((int)type == 3) //decay
        {
            tileX = 2;
            tileY = 0;
        }
        if ((int)type == 4) //pulp
        {
            tileX = 5;
            tileY = 0;
        }
        if ((int)type == 5) //plaque
        {
            tileX = 3;
            tileY = 0;
        }
        if ((int)type == 6) //filler
        {
            tileX = 4;
            tileY = 0;
        }

        tilePerc = 1 / pixelSize;
        umin = tilePerc * tileX;
        umax = tilePerc * (tileX + 1);
        vmin = 0;
        vmax = 1;
    }

    public List<Vector2> addUVs(int direction)
    {
        List<Vector2> uvRange = new List<Vector2>();
        if (direction==0) //TOP SIDE
        {
            uvRange.Add(new Vector2(umax, vmin)); //b
            uvRange.Add(new Vector2(umin, vmin)); //a
            uvRange.Add(new Vector2(umin, vmax)); //d
            uvRange.Add(new Vector2(umax, vmax)); //c
        }
        if (direction == 1) //RIGHT SIDE
        {
            uvRange.Add(new Vector2(umin, vmin));
            uvRange.Add(new Vector2(umin, vmax));
            uvRange.Add(new Vector2(umax, vmax));
            uvRange.Add(new Vector2(umax, vmin));
        }
        if (direction == 2) //BOTTOM SIDE
        {
            uvRange.Add(new Vector2(umin, vmax));
            uvRange.Add(new Vector2(umax, vmax));
            uvRange.Add(new Vector2(umax, vmin));
            uvRange.Add(new Vector2(umin, vmin));
        }
        if (direction == 3) //LEFT SIDE
        {
            uvRange.Add(new Vector2(umax, vmax));
            uvRange.Add(new Vector2(umax, vmin));
            uvRange.Add(new Vector2(umin, vmin));
            uvRange.Add(new Vector2(umin, vmax));
        }
        if (direction == 4) //FRONT SIDE
        {
            uvRange.Add(new Vector2(umax, vmin));
            uvRange.Add(new Vector2(umin, vmin));
            uvRange.Add(new Vector2(umin, vmax));
            uvRange.Add(new Vector2(umax, vmax));
        }
        if (direction == 5) //BACK SIDE
        {
            uvRange.Add(new Vector2(umin, vmax));
            uvRange.Add(new Vector2(umax, vmax));
            uvRange.Add(new Vector2(umax, vmin));
            uvRange.Add(new Vector2(umin, vmin));
        }
        return uvRange;
    }


    public void LoadMaterials(Type toothType)
    {
        if ((int)toothType == 0)
        {
            //material = Resources.Load("Materials/DentineMat") as Material;
            transparent = Resources.Load("Materials/TransDentineMat") as Material;
        }
        else if ((int)toothType == 1)
        {
            //material = Resources.Load("Materials/enamelMat") as Material;
            transparent = Resources.Load("Materials/TransEnamelMat") as Material;

        }
        //else if ((int)toothType == 2) material = Resources.Load("Materials/decayMat") as Material;
        //else if ((int)toothType == 3) material = Resources.Load("Materials/decayMat") as Material;
        else if ((int)toothType == 4)
        {
            //material = Resources.Load("Materials/fillerMat") as Material;
            transparent = Resources.Load("Materials/TransPulpMat") as Material;
        }
        else if ((int)toothType == 5)
        {
            //material = Resources.Load("Materials/plaqueMat") as Material;
            transparent = Resources.Load("Materials/TransPlaqueMat") as Material;
        }

        else if ((int)toothType == 6)
        {
            //material = Resources.Load("Materials/fillerMat") as Material;
            transparent = Resources.Load("Materials/TransFillerMat") as Material;
        }
        else if ((int)toothType == 7)
        {
            transparent = Resources.Load("Materials/removedFillerMat") as Material;
        }//previous decay
    }

    //Clear - empties the mesh and all local lists
    public void Clear()
    {
        mesh = new Mesh();
        vertices.Clear();
        triangles.Clear();
        voxelPosition.Clear();
        numberOfFaces.Clear();
        go.GetComponent<MeshFilter>().mesh = mesh;
    }

    //MakeTransparent - makes mesh transparent
    public void MakeTransparent(bool makeTransparent)
    {
        if (makeTransparent == true)
            go.GetComponent<MeshRenderer>().sharedMaterial = transparent;
        else
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    //UpdateMesh - updates the mesh
    public void UpdateMesh()
    {
        mesh.Clear(); //resets mesh and all its variabless
        mesh.vertices = vertices.ToArray(); 
        mesh.triangles = triangles.ToArray();

        if(type!=(Type)7)
            mesh.uv = uvList.ToArray();
        mesh.RecalculateNormals();
        
        go.GetComponent<MeshFilter>().mesh = mesh; //resets the mesh collider
        UnityEngine.Object.Destroy(go.GetComponent<MeshCollider>());
        if(type!=(Type)7)go.AddComponent<MeshCollider>();

        go.transform.parent = GameObject.Find("tooth").transform;
    }

    //checks whether the mesh has space for more voxels
    public bool HasSpace()
    {
         return (voxelPosition.Count < 500) ? true : false; 
    }

    //adds cube to the list
    public void AddCube(Vector3 position, int faceCount)
    {
        voxelPosition.Add(position);
        numberOfFaces.Add(faceCount);
    }

    //adds vertices of face to vertices array, and index to triangles array
    public void AddFace(Vector3[] verticesRange, int direction)
    {
        vertices.AddRange(verticesRange); //adds the range to vertices
        int vCount = vertices.Count;
        triangles.Add(vCount - 4); //adds all triangles to the list
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);

        uvList.AddRange(addUVs(direction));
    }

    //adds face if cube already exists
    public void AddExistingFace(Vector3[] verticesRange, Vector3 facePos, int direction)
    {

        int placeInArray = 0;
        int faceCount = 0;

        for (int i = 0; i < voxelPosition.Count; i++) //loop through all voxel positions in this mesh
        {
            if (voxelPosition[i] == facePos) //when we find this position in the mesh
            {
                numberOfFaces[i] ++; //adds one to the number of faces linked to this voxel
                placeInArray = i;
                break;
            }
            else
            {
                faceCount += numberOfFaces[i];
            }
        }
        int vertPosition = faceCount * 4;
        int triPosition = faceCount * 6;
        vertices.InsertRange(vertPosition, verticesRange); //adds the vertices to the specified position
        triangles.Insert(triPosition, vertPosition); //add indexes to triangle list
        triangles.Insert(triPosition+1, vertPosition+1);
        triangles.Insert(triPosition+2, vertPosition+2);
        triangles.Insert(triPosition+3, vertPosition);
        triangles.Insert(triPosition+4, vertPosition+2);
        triangles.Insert(triPosition+5, vertPosition+3);
        uvList.InsertRange(vertPosition, addUVs(direction));


        for (int i = triPosition+6; i < triangles.Count; i++)
        {
            triangles[i] += 4;
        }
    }
    //removes cube from the mesh, lists and positions.
    public void RemoveCube(Vector3 position)
    {
        int placeInArray = 0;
        int faceCount = 0;
        int facePosition = 0; //used for finding position to delete from in list

        for(int i = 0; i < voxelPosition.Count; i++) //loop through all voxel positions in this mesh
        {
            if (voxelPosition[i] == position) //when we find this position in the mesh
            {
                faceCount = numberOfFaces[i]; //get the number of faces for this cube
                placeInArray = i;
                break;
            }
            else
                facePosition+=numberOfFaces[i]; //else we add to the number of faces
        }
        int vertPosition = facePosition * 4; //position in vertices 
        int triPosition = facePosition * 6; //position in triangles

        for(int i = 0; i < faceCount; i++)
        {
            for(int j = 0; j < 4; j++) //remove all vertices (4 per face)
            {
                vertices.RemoveAt(vertPosition);
                uvList.RemoveAt(vertPosition);
            }
            for(int j = 0; j < 6; j++) //remove all triangle indexes (6 per face)
                triangles.RemoveAt(triPosition);
        }

        //vertices have been removed, so we need to update the index that triangles are pointing to
        int vertCount = faceCount * 4;
        for(int i = triPosition; i < triangles.Count; i++)
            triangles[i] -= vertCount;
        //removes position and faces of voxel from the list classes lists.
        voxelPosition.RemoveAt(placeInArray);
        numberOfFaces.RemoveAt(placeInArray);
    }

    //containsCube - Checks whether list of voxel positions contains the array
    public bool ContainsCube(Vector3 voxel)
    {
        return (voxelPosition.Contains(voxel)) ? true : false;
    }
}

//Type - types of tooth
public enum Type{
    Dentine, //0 
    Enamel, //1 
    DentineDecay, //2 
    EnamelDecay, //3 
    Pulp, //4 
    Plaque, //5 
    Filler, //6 
    PreviousDecay //7 
}

