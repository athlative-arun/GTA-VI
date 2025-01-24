using UnityEngine;

public class TimeSkip : MonoBehaviour
{
    public float speed;
   
    void Start()
    {
        
    }

    
    void Update()
    {
        Time.timeScale = speed;
    }
}
