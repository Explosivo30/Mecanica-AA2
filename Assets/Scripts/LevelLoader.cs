using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private Vector3 goalCenter;
    private Vector3 goalSize;

    private PhysicsBody ball;

    public int maxStickContactsPerLevel;
    private int stickContacts = 0;
    private bool levelCompleted = false;

    void Start()
    {
        goalCenter = transform.position;
        goalSize = transform.localScale;
        ball = FindObjectOfType<PhysicsBody>();    
    }

    void FixedUpdate()
    {
        if (levelCompleted || ball == null)
            return;

        if (IsInsideGoal(ball.transform.position))
        {
            if(stickContacts <= maxStickContactsPerLevel)
            {
                levelCompleted = true;
                LoadNextLevel();
            }
            else
            {
                RestartLevel();
            }
        }
    }

    public void RegisterStickContact()
    {
        stickContacts++;
    }

    bool IsInsideGoal(Vector3 pos)
    {
        Vector3 min = goalCenter - goalSize;
        Vector3 max = goalCenter + goalSize;

        return (pos.x >= min.x && pos.x <= max.x &&
                pos.y >= min.y && pos.y <= max.y &&
                pos.z >= min.z && pos.z <= max.z);
    }

    void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if(currentIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentIndex + 1);
        }
        else{
            Debug.Log("Volviendo al primer nivel");
            SceneManager.LoadScene(0);
        }
    }

    void RestartLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(goalCenter, goalSize);
    }
}
