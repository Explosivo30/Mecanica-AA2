using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager instance { get; private set; }

    private List<WallCollider> walls = new List<WallCollider>();
    public IReadOnlyList<WallCollider> Walls => walls;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void RegisterWall(WallCollider wall)
    {
        if(!walls.Contains(wall))
        {
            walls.Add(wall);
        }
    }

    public void UnregisterWall(WallCollider wall)
    {
        walls.Remove(wall);
    }
}
