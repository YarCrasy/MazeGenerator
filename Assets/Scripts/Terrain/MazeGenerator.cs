using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

public class MazeGenerator : MonoBehaviour
{
    // Maze size is mesured as MAX_MAZE_SIZE * MAX_MAZE_SIZR
    public const int MAX_MAZE_SIZE = 5, MODULE_SIZE = 6;   

    [SerializeField] GameObject moduleObj;
    GameObject mazeParent;

    // save refecences
    readonly GameObject[,] maze = new GameObject[MAX_MAZE_SIZE,MAX_MAZE_SIZE];
    readonly MazeModule[,] modules = new MazeModule[MAX_MAZE_SIZE,MAX_MAZE_SIZE];

    int x, z;   //actual position   used z insted of y for better understanding in game space
    readonly int[] auxX = { -1, 0, 1, 0 }, auxZ = { 0, -1, 0, 1 };  // used for the 4 respective direction
    readonly Stack<MazeModule> pathStack = new();

    private void Awake()
    {
        GenerateModules();
        GeneratePath();

        
        MazeBake(); //IT ONLY CAN BAKE IN UNITY BUILD, IT THROW ERROR IF YOU TRY TO BUILD THE GAME
    }

    void GenerateModules()
    {
        //creats a gameObject where can hide all the modules in inspector
        mazeParent = new GameObject("Maze");

        //Instantiate modules and each one saves their own position
        for (int x = 0; x < MAX_MAZE_SIZE; x++)
        {
            for (int z = 0; z < MAX_MAZE_SIZE; z++)
            {
                Vector3 pos = new(x * MODULE_SIZE, 0, z * MODULE_SIZE); //this pos is for the game space, not for matrix pos
                maze[x, z] = Instantiate(moduleObj, pos, Quaternion.identity);
                maze[x, z].transform.parent = mazeParent.transform;
                modules[x, z] = maze[x, z].GetComponent<MazeModule>();  //get the reference for each script to access to their walls
                modules[x, z].posX = x;
                modules[x, z].posZ = z;
            }
        }
    }

    void GeneratePath()
    {
        //Decide from where start the generation of the path
        //Added the firt Module to the stack
        x = Random.Range(0, MAX_MAZE_SIZE);
        z = Random.Range(0, MAX_MAZE_SIZE); 
        //Debug.Log(x + " " + z);
        pathStack.Push(modules[x, z]);

        //
        for (int limit = 0; limit < 10000 && !AllVisited(); limit++)
        {
            MazeModule mod = pathStack.Peek();
            x = mod.posX;
            z = mod.posZ;
            mod.visited = true;

            WallDirection dir = (WallDirection)Random.Range(0, 4);

            //Debug.Log(dir + ", Next pos: " + (x + auxX[(int)dir]) + " " + (z + auxZ[(int)dir]));

            if (PosInRange(x + auxX[(int)dir], z + auxZ[(int)dir]))
            {
                MazeModule next = modules[x + auxX[(int)dir], z + auxZ[(int)dir]];

                if (!AllDirVisited())
                {
                    if (!next.visited)
                    {
                        if (dir == WallDirection.left)
                        {
                            mod.DeactivateWall(dir);
                            next.DeactivateWall(WallDirection.right);
                        }
                        else if (dir == WallDirection.back)
                        {
                            mod.DeactivateWall(dir);
                            next.DeactivateWall(WallDirection.front);
                        }
                        else if (dir == WallDirection.right)
                        {
                            mod.DeactivateWall(dir);
                            next.DeactivateWall(WallDirection.left);
                        }
                        else if (dir == WallDirection.front)
                        {
                            mod.DeactivateWall(dir);
                            next.DeactivateWall(WallDirection.back);
                        }
                        next.visited = true;
                        pathStack.Push(next);
                    }
                    //else
                    //{
                    //    Debug.Log("no move: DIR VISITED");
                    //}
                }
                //else
                //{
                //    mod = pathStack.Pop();
                //    Debug.Log("POPED TO " + mod.posX + " " + mod.posZ);
                //}
            }
            //else
            //{
            //    Debug.Log("no move: DIR OUT OF BOUNDS");
            //}
        }
    }

    // if there is not object is the stack, it might be all visited
    bool AllVisited()
    {
        if (pathStack.Count == 0) return true;
        else return false;
    }

    //check in each of the 4 directions, if there is any not visited module, it return false
    bool AllDirVisited()
    {
        bool findNotVisited = true;
        for (int i = 0; i < auxX.Length && findNotVisited; i++)
        {
            if (PosInRange(x + auxX[i], z + auxZ[i]))
            {
                if (!modules[x + auxX[i], z + auxZ[i]].visited)
                {
                    findNotVisited = false;
                }
            }
        }
        return findNotVisited;
    }

    //check if the position is in range [0, MAX MAZE SIZE]
    bool PosInRange(int x, int z)
    {
        if ((x >= 0 && x <= MAX_MAZE_SIZE - 1) && (z >= 0 && z <= MAX_MAZE_SIZE - 1)) return true;
        else return false;
    }


    //WORK IN PROGRESS
    void MazeBake()
    {
        NavMeshBuilder.BuildNavMesh();

        StaticOcclusionCulling.Compute();
        StaticOcclusionCulling.RemoveCacheFolder();
    }

}
