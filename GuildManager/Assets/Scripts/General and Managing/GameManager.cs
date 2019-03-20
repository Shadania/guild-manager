using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Manages a few things, holds resources that have to be publicly available
public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null && !_applicationIsQuiting)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject newObject = new GameObject("Singleton_GameManager");
                    _instance = newObject.AddComponent<GameManager>();
                }
            }
    
            return _instance;
        }
    }
    private static bool _applicationIsQuiting = false;
    
    
    
    private void OnApplicationQuit()
    {
        _applicationIsQuiting = true;
    }
    #endregion Singleton


    
    public GameObject PlayerAvatar;
    public GameObject PlayerGuildDesk;

    // Same as above
    public NameGenerator NameGen;

    // Textures for armor & swords
    public Texture ArmorLeather;
    public Texture ArmorIron;
    public Texture ArmorSteel;
    public Texture ArmorMithril;

    public Texture SwordWood;
    public Texture SwordIron;
    public Texture SwordSteel;
    public Texture SwordMithril;

    // Materials for armor & swords (wood doubles as leather)
    public Material MaterialWood;
    public Material MaterialIron;
    public Material MaterialSteel;
    public Material MaterialMithril;

    // The color your guildies and enemies turn when they get hurt
    public Material MaterialHurt;

    // Some prefabs that had to be publicly available
    public GameObject LootCratePrefab;
    public GameObject EnemyPrefab;

    // SFX
    public AudioClip HurtSoundEffect;
    public AudioClip GoldSoundEffect;
}
