using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameInstance : MonoBehaviour
{
    public static MyGameInstance instance;
    

    public Difficulty difficulty;
    public bool hasCleared = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    };
}
