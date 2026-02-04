using Unity.Collections;
using Unity.Netcode;

public struct SkillOptionInfo : INetworkSerializable
{
    public FixedString64Bytes SkillName;
    public FixedString512Bytes Description;
    public int NextLevel;
    public int IsWeapon;

    public SkillOptionInfo(string skillName, string description, int nextLevel, bool isWeapon)
    {
        SkillName = skillName;
        Description = description;
        NextLevel = nextLevel;
        IsWeapon = isWeapon ? 1 : 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref SkillName);
        serializer.SerializeValue(ref Description);
        serializer.SerializeValue(ref NextLevel);
        serializer.SerializeValue(ref IsWeapon);
    }
}
