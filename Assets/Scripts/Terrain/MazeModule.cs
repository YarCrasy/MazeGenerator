using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallDirection { left, back, right, front }

public class MazeModule : MonoBehaviour
{
    [SerializeField] GameObject[] walls = new GameObject[4];

    public int posX, posZ;

    public bool visited = false;

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
