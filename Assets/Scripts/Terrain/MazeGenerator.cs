using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator instance; //there only can be one instance in the scene

    // Maze size is mesured as MAX_MAZE_SIZE * MAX_MAZE_SIZR
    public const int MODULE_SIZE = 6, MAX_PATH_GEN_LOOP = 10000;
    [Range(5, 20)] public int maxMazeSize;
    [SerializeField] GameObject mazeParent;
    [SerializeField] GameObject moduleObj; 
    [SerializeField] GameObject plane;  //a gigant floor for all the maze
    [SerializeField] MazeModule[] mazeAux;  //getting the maze reference from the scene

    // save refecences
    public GameObject[,] maze;
    public MazeModule[,] modules;

    int actualX, actualZ;   //used z insted of y for better understanding in game space
    readonly int[] auxX = { -1, 0, 1, 0 }, auxZ = { 0, -1, 0, 1 };  // used for the 4 respective direction
    readonly Stack<MazeModule> pathStack = new();

    private void Awake()
    {
        instance = this;
        maze = new GameObject[maxMazeSize, maxMazeSize];
        modules = new MazeModule[maxMazeSize, maxMazeSize];
    }

    private void Start()
    {
        if (mazeAux != null)
        {
            SetModules();
        }
        else
        {
            GenerateModules();
        }
        //QuitParent();
        GeneratePath();
    }

    //generate a all modules needed
    void GenerateModules()
    {
        //creats a gameObject where can hide all the modules in inspector
        mazeParent = new GameObject("Maze");

        //Instantiate modules and each one saves their own position
        for (int x = 0; x < maxMazeSize; x++)
        {
            for (int z = 0; z < maxMazeSize; z++)
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

    //Preparing all the modules in scene, destroy no needed, getting the needed
    void SetModules()
    {
        for (int i = 0; i < mazeAux.Length; i++)
        {
            if (mazeAux[i].posX < maxMazeSize && mazeAux[i].posZ < maxMazeSize)
            {
                modules[mazeAux[i].posX, mazeAux[i].posZ] = mazeAux[i];
                maze[mazeAux[i].posX, mazeAux[i].posZ] = modules[mazeAux[i].posX, mazeAux[i].posZ].gameObject;
            }
            else
            {
                Destroy(mazeAux[i].gameObject);
            }
        }
        mazeAux = null;
    }

    //Generate a path that able the player to move through all the maze
    void GeneratePath()
    {
        //Decide from where start the generation of the path
        
        actualX = Random.Range(0, maxMazeSize);
        actualZ = Random.Range(0, maxMazeSize); 
        //Debug.Log(x + " " + z);
        pathStack.Push(modules[actualX, actualZ]);  //Added the firt Module to the stack

        //dont end until all modules get visited
        //for safety reason, limited loop times
        for (int limit = 0; limit < MAX_PATH_GEN_LOOP && !AllVisited(); limit++)
        {
            //takes the last module added to the stack and set it as visited
            MazeModule mod = pathStack.Peek();
            actualX = mod.posX;
            actualZ = mod.posZ;
            mod.visited = true;

            //check if the actual module have all surrounding modules visited
            if (!AllDirVisited())
            {
                //if not, select randomly a direction
                WallDirection dir = (WallDirection)Random.Range(0, 4);
                //Debug.Log(dir + ", Next pos: " + (x + auxX[(int)dir]) + " " + (z + auxZ[(int)dir]));

                //check if there is a module in the direction
                if (PosInRange(actualX + auxX[(int)dir], actualZ + auxZ[(int)dir]))
                {
                    //if it is the case, get the reference
                    MazeModule next = modules[actualX + auxX[(int)dir], actualZ + auxZ[(int)dir]];

                    //and check is visited or not
                    if (!next.visited)
                    {
                        //in each case, break the respective walls and add the next module to the stack, the next loop, next mod become actual mod
                        switch (dir)
                        {
                            case WallDirection.left:
                                //DeactivateWalls(mod, next, dir, WallDirection.right);
                                DestroyWalls(mod, next, dir, WallDirection.right);
                                break;
                            case WallDirection.back:
                                //DeactivateWalls(mod, next, dir, WallDirection.front);
                                DestroyWalls(mod, next, dir, WallDirection.front);
                                break;
                            case WallDirection.right:
                                //DeactivateWalls(mod, next, dir, WallDirection.left);
                                DestroyWalls(mod, next, dir, WallDirection.left);
                                break;
                            case WallDirection.front:
                                //DeactivateWalls(mod, next, dir, WallDirection.back);
                                DestroyWalls(mod, next, dir, WallDirection.back);
                                break;
                            default: break;
                        }
                        pathStack.Push(next);
                    }
                    else
                    {
                        //Debug.Log("no move: DIR VISITED");
                    }
                }
                else
                {
                    //Debug.Log("no move: DIR OUT OF BOUNDS");
                }
            }
            else
            {
                pathStack.Pop();  //delete de last added module
                //Debug.Log("POPED TO " + mod.posX + " " + mod.posZ);
            }

        }
    }

    void DeactivateWalls(MazeModule actual, MazeModule next, WallDirection dir, WallDirection opposite)
    {
        actual.DeactivateWall(dir);
        next.DeactivateWall(opposite);
    }

    void DestroyWalls(MazeModule actual, MazeModule next, WallDirection dir, WallDirection opposite)
    {
        actual.DestroyWall(dir);
        next.DestroyWall(opposite);
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
            if (PosInRange(actualX + auxX[i], actualZ + auxZ[i]))
            {
                if (!modules[actualX + auxX[i], actualZ + auxZ[i]].visited)
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
        if ((x >= 0 && x <= maxMazeSize - 1) && (z >= 0 && z <= maxMazeSize - 1)) return true;
        else return false;
    }

    //get a random position in the maze's bounds
    public int MazeRandomWorldPosition()
    {
        return Random.Range(0, maxMazeSize - 1) * MODULE_SIZE;
    }

}
