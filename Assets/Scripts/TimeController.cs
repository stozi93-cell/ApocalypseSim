using UnityEngine;

public class TimeController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("PAUSE");
            Time.timeScale = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("SPEED 1x");
            Time.timeScale = 1f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("SPEED 4x");
            Time.timeScale = 4f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("SPEED 8x");
            Time.timeScale = 8f;
        }
    }
}
