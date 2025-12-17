using UnityEngine;

public class EnemyVoiceCountManager
{
    // brainstorm
    // set up logic that states:
    //    when an enemy wants to play a sound, it must make this class aware
    //      via the game manager
    //    if their voice would not make the count of currently playing voices for 
    //      their enemy type exceed the maximum, they can play their sound
    //      otherwise they cannot play
    //    TODO/stretch goal: implement a "try again" logic for voices not allowed to play
    //    once their sound has ended, inform this class so the number of current voices can be updated
    private int maxOwlVoices;
    private int currentOwlVoices;
    public bool canPlayOwlSound;
    private int maxPhantomVoices = 4;
    public int currentPhantomVoices = 0;
    public bool canPlayPhantomSound = true;
    private int maxSnakeVoices;
    private int currentSnakeVoices;
    public bool canPlaySnakeSound;

    public bool CanPlaySound(string enemyName)
    {
        switch (enemyName)
        {
            case "Phantom":
                if (currentPhantomVoices + 1 <= maxPhantomVoices)
                {
                    Debug.Log("debugging");
                    return true;
                }
                else return false;
            default:
                break;
        }
        return true;
    }

    public void IncreaseVoiceCount(string enemyName)
    {
        switch (enemyName)
        {
            case "Phantom":
                currentPhantomVoices += 1;
                break;
            default:
                break;
        }
    }

    public void DecreaseVoiceCount(string enemyName)
    {
        switch (enemyName)
        {
            case "Phantom":
                currentPhantomVoices -= 1;
                break;
            default:
                break;
        }
    }
}
