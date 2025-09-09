using UnityEngine;

public class ScytheToggle : MonoBehaviour
{
    public GameObject scythe;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            scythe.SetActive(!scythe.activeSelf);
        }
    }
}