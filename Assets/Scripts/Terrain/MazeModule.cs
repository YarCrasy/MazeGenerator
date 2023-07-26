using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallDirection { left, back, right, front }

public class MazeModule : MonoBehaviour
{
    [SerializeField] GameObject[] walls = new GameObject[4];

    public int posX, posZ;

    public bool visited = false;

    private void Awake()
    {
        posX = (int)transform.position.x / MazeGenerator.MODULE_SIZE;
        posZ = (int)transform.position.z / MazeGenerator.MODULE_SIZE;
    }


    public void DeactivateWall(WallDirection dir)
    {
        walls[(int)dir].SetActive(false);
    }

    public void DestroyWall(WallDirection dir)
    {
        Destroy(walls[(int)dir]);
    }

    public bool IsWallActive(WallDirection dir)
    {
        return walls[(int)dir].activeSelf;
    }

}
