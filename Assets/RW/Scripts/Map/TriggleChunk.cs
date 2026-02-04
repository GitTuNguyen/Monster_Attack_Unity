using UnityEngine;

public class TriggleChunk : MonoBehaviour
{
    public GameObject targetMap;
    MapController mc;
    private void Start()
    {
        mc = FindFirstObjectByType<MapController>();
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            mc.currentChunk = targetMap;
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (mc.currentChunk == targetMap)
            {
                mc.currentChunk = null;
            }
        }
    }
}
