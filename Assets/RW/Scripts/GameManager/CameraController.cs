using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;    

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            Debug.Log("camera Move");
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        }        
    }
}
