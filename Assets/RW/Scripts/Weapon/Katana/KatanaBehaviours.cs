public class KatanaBehaviours : WeaponBehaviour
{
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<KatanaController>();
        base.Start();
        Destroy(gameObject, weaponController.timeToDestroy);
    }

}
