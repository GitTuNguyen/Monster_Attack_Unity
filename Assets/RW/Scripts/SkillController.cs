using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public string skillName;
    public SkillType skillType;
    public Sprite sprite;
    public GameObject prefab;
    public int level;
    public int maxLevel;
    public Player player;
    protected void Awake()
    {
        player = GetComponentInParent<Player>();
        if (player == null)
            player = Player.LocalPlayer ?? FindFirstObjectByType<Player>();
    }

    protected virtual void Start()
    {
        level = 1;
    }

    public void SetOwner(Player owner)
    {
        player = owner;
    }

    public virtual void SetStats(int level)
    {

    }

    public virtual string getDescriptionNextLevel()
    {
        return "";
    }

    public bool isCanUplevel()
    {
        return level < maxLevel;
    }
    public void Upgrade()
    {
        level++;
        SetStats(level);
    }
}
