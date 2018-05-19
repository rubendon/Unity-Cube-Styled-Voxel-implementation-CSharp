using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class VoxelData{

    public short[,,] data;
    public int plaqueCount;
    public int decayCount;
    public int maxDecay;
    public int maxPlaque;

    int X;
    int Y;
    int Z;

    public int GetX()
    {
        return X;
    }
    public int GetY()
    {
        return Y;
    }
    public int GetZ()
    {
        return Z;
    }

    public int Width
    {
        //gets how many voxels there are
        get { return data.GetLength(0); }
    }

    public int Depth
    {
        //gets how many variables there are per voxel
        get { return data.GetLength(1); }
    }

    public int Height
    {
        get { return data.GetLength(2);  }
    }

    public int GetCell(int x, int y, int z)
    {
        //gets cell
        return data[x, y, z];
    }

    public int GetNeighbour(int x, int y, int z, Direction dir)
    {
        DataCoordinate offsetToCheck = offsets[(int)dir];
        DataCoordinate neighbourCoord = new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);
        if(neighbourCoord.x<0 || neighbourCoord.x>=Width || neighbourCoord.y<0 || neighbourCoord.y>=Depth || neighbourCoord.z<0 || neighbourCoord.z >= Height)
        {
            return 0;
        }
        else
        {
            return GetCell(neighbourCoord.x, neighbourCoord.y, neighbourCoord.z);
        }
    }

    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    DataCoordinate[] offsets =
    {
        new DataCoordinate( 0, 0, 1),
        new DataCoordinate( 1, 0, 0),
        new DataCoordinate( 0, 0,-1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate( 0, 1, 0),
        new DataCoordinate( 0, -1, 0)
    };


    public void ReadHexData(string fileName)
    {
        //Read the byte file into a byte array
        byte[] file = File.ReadAllBytes(fileName);
        //Convert header to integer values
        X = BitConverter.ToInt16(file, 9);
        Y = BitConverter.ToInt16(file, 5);
        Z = BitConverter.ToInt16(file, 1);
        //Create an integer array to store the binary data
        data = new short[X, Y, Z];
        //Loop through data and store data accordingly
        int xPos = 0;
        int yPos = 0;
        int zPos = 0;
        for(int i = 13; i < file.Length; i++)
        {
            short type = file[i];
            data[xPos, yPos, zPos] = type;
            xPos++;
            if (xPos == X)
            {
                yPos++;
                xPos = 0;
            }
            if (yPos == Y)
            {
                zPos++;
                yPos = 0;
            }
        }
    }

    public Vector3 GetLocalGroove(Vector3 pos)
    {
        Vector3 lowestBlock = pos;
        Vector3 lowestFiller = new Vector3(-1, -1, -1);
        int min = ((int)pos.z - 5 <0)? (int)-pos.z:-5;

        for(int i = 0; i > min; i--)
        {
            if(GetCell((int)pos.x, (int)pos.y, (int)pos.z + i) == 8)
                lowestFiller = new Vector3(pos.x, pos.y, pos.z + i);
            else if (GetCell((int)pos.x, (int)pos.y, (int)pos.z + i) != 0)
            {
                lowestBlock = new Vector3(pos.x, pos.y, pos.z + i + 1);
                break;
            }
        }

        if ((int)lowestFiller.x != -1)
            return lowestFiller;
        else
            return lowestBlock;
    }

    int pCount = 5;
    int dCount = 5;

    public VoxelData()
    {
        ReadHexData("Assets/fw9003_4.mask");
        int gameMode = PlayerPrefs.GetInt("gameMode");
        dCount = 5;
        pCount = 5;

        if (gameMode == 1)
        {
            ReadHexData("Assets/fw9003_8.mask");
             SetCounts(gameMode);
             AddPlaque(10);
             AddDecay(7);
        }
        else if (gameMode == 2) ReadHexData("Assets/fw9003_4.mask");
        else if (gameMode == 3) ReadHexData("Assets/wandi1_d.mask");



    }

    void SetCounts(int gameMode) {
        int dCount = 0;
        int pCount = 0;
        for(int x = 0; x < X; x++)
        {
            for(int y = 0; y < Y; y++)
            {
                for(int z= 0; z < Z; z++)
                {
                    if (data[x, y, z] == 4)
                    {
                        dCount++;
                    }
                    if (data[x, y, z] == 5)
                    {
                        pCount++;
                    }
                }
            }
        }
        maxDecay = 1000 * gameMode;
        maxPlaque = 200 * gameMode;

        decayCount = dCount;
        plaqueCount = pCount;
    }

    public void AddPlaque(int radius)
    {
        Vector3 center = Vector3.zero;
        System.Random random = new System.Random();

        int w = random.Next(3, radius);
        int h = random.Next(3, radius);
        int d = random.Next(3, radius);

        int x = random.Next(w, X - w-1);
        int y = random.Next(h, Y - h-1);
        int z = Z-10- radius;


        for (int i = -w; i < w; i++)
        {
            for (int j = -h; j < h; j++)
            {
                for (int k = -d; k < d; k++)
                {
                    if (plaqueCount == maxPlaque)
                    {
                        return;
                    }
                    Vector3 position = new Vector3(i, j, k);
                    float distance = Vector3.Distance(position, center);
                    if (distance < radius)
                    {
                        if (data[i + x, j + y, k + z] != 0 && data[i + x, j + y, k + z] != 4 && data[i + x, j + y, k + z] != 6)
                            {
                            for(int dir = 0; dir < 6; dir++)
                            {
                                if (GetNeighbour(i + x, j + y, k + z, (Direction)(dir)) == 0){
                                    DataCoordinate off = offsets[(int)dir];
                                    data[i + x + off.x, j + y + off.y, k + z + off.z] = 6;
                                    plaqueCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
        pCount--;
        if(pCount!=0)
            AddPlaque(radius);
    }

    public void AddDecay(int radius)
    {
        Vector3 center = Vector3.zero;
        System.Random random = new System.Random();

        int x = random.Next(radius, X - radius-1);
        int y = random.Next(radius, Y - radius-1);
        int z = Z - 10 - radius;

        for (int i = -radius; i < radius; i++)
        {
            for(int j = -radius; j < radius; j++)
            {
                for(int k = -radius; k < radius; k++)
                {
                    if(decayCount == maxDecay)
                        return;
                    Vector3 position = new Vector3(i, j, k);
                    float distance = Vector3.Distance(position, center);
                    if (distance < radius)
                    {
                        if (data[i + x, j + y, k + z]==1)
                        {
                            data[i + x, j + y, k + z] = 3;
                            decayCount++;
                        }
                        else if (data[i + x, j + y, k + z] == 2)
                        {
                            data[i + x, j + y, k + z] = 4;
                            decayCount++;
                        }
                    }
                }
            }
        }
        dCount--;
        if (dCount != 0)
            AddDecay(radius);
    }

    public void SetVoxel(int x, int y, int z, int val)
    {
        data[x, y, z] = (short)val;
    }

    public Database Database
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
        }
    }
}
public enum Direction
{
    North,
    East,
    South,
    West,
    Up,
    Down
}
