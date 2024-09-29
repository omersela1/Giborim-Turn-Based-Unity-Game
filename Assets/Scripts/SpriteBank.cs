using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBank : MonoBehaviour
{
    private Dictionary<string, Sprite> spriteBank;
    public Sprite knight, pirate, bandit, goblin, troll, yeti;

    private static SpriteBank instance;

    public static SpriteBank Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("SpriteBank").GetComponent<SpriteBank>();
            }

            return instance;
        }
    }
    void Awake()
    {
        spriteBank = new Dictionary<string, Sprite>();
        spriteBank.Add("Knight", knight);
        spriteBank.Add("Pirate", pirate);
        spriteBank.Add("Bandit", bandit);
        spriteBank.Add("Goblin", goblin);
        spriteBank.Add("Troll", troll);
        spriteBank.Add("Yeti", yeti);

        Debug.Log("All sprites have been added to dictionary.");
    }

    public Sprite GetSpriteByTypeName(string name)
    {
        return spriteBank[name];
    }

    public Sprite GetRandomSprite()
    {
        int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(CharacterTypes)).Length);
        CharacterTypes randomType = (CharacterTypes)randomIndex;
        return GetSpriteByTypeName(randomType.ToString());
    }
    
    
}
