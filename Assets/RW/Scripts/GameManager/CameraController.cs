using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            if (Player.LocalPlayer != null)
            {
                target = Player.LocalPlayer.transform;
            }
            return;
        }

        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );
    }
}
