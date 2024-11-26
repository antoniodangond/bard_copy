using UnityEngine;
using UnityEngine.Events;

// TODO: performance can possibly be improved by using regular C# events instead of UnityEvents
public static class CustomEvents
{
    // Player events
    public static UnityEvent<PlayerState> OnPlayerStateChange = new UnityEvent<PlayerState>();
    public static UnityEvent<Dialogue> OnDialogueStart = new UnityEvent<Dialogue>();
    public static UnityEvent<Dialogue> OnDialogueEnd = new UnityEvent<Dialogue>();
    public static UnityEvent OnAttackFinished = new UnityEvent();
    // UI events
    public static UnityEvent<bool> OnPause = new UnityEvent<bool>();
    public static UnityEvent<bool> OnUnPause = new UnityEvent<bool>();
    // Combat events
    public static UnityEvent<GameObject> OnEnemyDeath = new UnityEvent<GameObject>();
    public static UnityEvent<GameObject> OnCombatEncounterCleared = new UnityEvent<GameObject>();
}
