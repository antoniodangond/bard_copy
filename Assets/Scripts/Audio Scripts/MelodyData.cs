using System.Collections.Generic;

public class MelodyData
{
    public const string NoteB = "NoteB";
    public const string NoteC = "NoteC";
    public const string NoteD = "NoteD";
    public const string NoteE = "NoteE";
    public const int MelodyLength = 5;
    public const string Melody1 = "Melody1";
    public const string Melody2 = "Melody2";
    // TODO: make melodies customizable in the editor
    private static string[] melody1Inputs = new string[MelodyLength]{
        NoteC,
        NoteB,
        NoteC,
        NoteD,
        NoteE,
    };
    private static string[] melody2Inputs = new string[MelodyLength]{
        NoteC,
        NoteC,
        NoteC,
        NoteC,
        NoteC,
    };
    public static Dictionary<string, string[]> MelodyInputs = new Dictionary<string, string[]>(){
        { Melody1, melody1Inputs }, { Melody2, melody2Inputs }
    };
}
