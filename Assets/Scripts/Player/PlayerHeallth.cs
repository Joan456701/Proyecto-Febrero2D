using UnityEngine;

public class PlayerHeallth : MonoBehaviour
{
    public int maxLives = 5;
    public int lives;
    public bool NoDamage = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lives = maxLives;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage()
    { 
        if (!NoDamage && lives > 0)
        {
            lives = lives - 1;
        }
    }
}
