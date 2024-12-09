using System.Collections.Generic;

public class PlayerStateInstrument: IPlayerState
{
    // TODO: improve this
    private Queue<string> lastPlayedNotes = new Queue<string>(new string[MelodyData.MelodyLength]{
        null,
        null,
        null,
        null,
        null,
    });

    public void Update(PlayerController_New playerController)
    {
        // TODO: improve PlayerInputManager
        if (PlayerInputManager.WasToggleInstrumentPressed)
        {
            playerController.ChangeState(PlayerState_New.Default);
            return;
        }

        // TODO: improve PlayerInputManager
        string notePressed = PlayerInputManager.NotePressed;
        if (notePressed is not null)
        {
            // playerAudio.PlayNote(notePressed);
            // Remove 1st note from queue, and add new note to end of queue,
            // so that the new 1st note is now the 4th-last note played
            lastPlayedNotes.Dequeue();
            lastPlayedNotes.Enqueue(notePressed);
            // Check if should play Melody
            // string melodyToPlay = FindMelodyToPlay(lastPlayedNotes);
            // if (melodyToPlay != null)
            // {
            //     // Start coroutine to change state, play song, and then return to default state
            //     StartCoroutine(PlayMelodyAfterDelay(melodyToPlay));
            // }
        }

        // TODO: check for note pressed
    }
}
