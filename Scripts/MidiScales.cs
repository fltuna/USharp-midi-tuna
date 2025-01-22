public enum MidiScales
{
    C0 = 0,
    CS0,
    D0,
    DS0,
    E0,
    F0,
    FS0,
    G0,
    GS0,
    A0,
    AS0,
    B0,
    C1,
    CS1,
    D1,
    DS1,
    E1,
    F1,
    FS1,
    G1,
    GS1,
    A1,
    AS1,
    B1,
    C2,
    CS2,
    D2,
    DS2,
    E2,
    F2,
    FS2,
    G2,
    GS2,
    A2,
    AS2,
    B2,
    C3,
    CS3,
    D3,
    DS3,
    E3,
    F3,
    FS3,
    G3,
    GS3,
    A3,
    AS3,
    B3,
    C4,
    CS4,
    D4,
    DS4,
    E4,
    F4,
    FS4,
    G4,
    GS4,
    A4,
    AS4,
    B4,
    C5,
    CS5,
    D5,
    DS5,
    E5,
    F5,
    FS5,
    G5,
    GS5,
    A5,
    AS5,
    B5,
    C6,
    CS6,
    D6,
    DS6,
    E6,
    F6,
    FS6,
    G6,
    GS6,
    A6,
    AS6,
    B6,
    C7,
    CS7,
    D7,
    DS7,
    E7,
    F7,
    FS7,
    G7,
    GS7,
    A7,
    AS7,
    B7,
    C8
}
public static class MidiScalesExtensions
{
    public static string GetName(int value)
    {
        switch (value)
        {
            case (int)MidiScales.C0: return "C0";
            case (int)MidiScales.CS0: return "C#0";
            case (int)MidiScales.D0: return "D0";
            case (int)MidiScales.DS0: return "D#0";
            case (int)MidiScales.E0: return "E0";
            case (int)MidiScales.F0: return "F0";
            case (int)MidiScales.FS0: return "F#0";
            case (int)MidiScales.G0: return "G0";
            case (int)MidiScales.GS0: return "G#0";
            case (int)MidiScales.A0: return "A0";
            case (int)MidiScales.AS0: return "A#0";
            case (int)MidiScales.B0: return "B0";
            case (int)MidiScales.C1: return "C1";
            case (int)MidiScales.CS1: return "C#1";
            case (int)MidiScales.D1: return "D1";
            case (int)MidiScales.DS1: return "D#1";
            case (int)MidiScales.E1: return "E1";
            case (int)MidiScales.F1: return "F1";
            case (int)MidiScales.FS1: return "F#1";
            case (int)MidiScales.G1: return "G1";
            case (int)MidiScales.GS1: return "G#1";
            case (int)MidiScales.A1: return "A1";
            case (int)MidiScales.AS1: return "A#1";
            case (int)MidiScales.B1: return "B1";
            case (int)MidiScales.C2: return "C2";
            case (int)MidiScales.CS2: return "C#2";
            case (int)MidiScales.D2: return "D2";
            case (int)MidiScales.DS2: return "D#2";
            case (int)MidiScales.E2: return "E2";
            case (int)MidiScales.F2: return "F2";
            case (int)MidiScales.FS2: return "F#2";
            case (int)MidiScales.G2: return "G2";
            case (int)MidiScales.GS2: return "G#2";
            case (int)MidiScales.A2: return "A2";
            case (int)MidiScales.AS2: return "A#2";
            case (int)MidiScales.B2: return "B2";
            case (int)MidiScales.C3: return "C3";
            case (int)MidiScales.CS3: return "C#3";
            case (int)MidiScales.D3: return "D3";
            case (int)MidiScales.DS3: return "D#3";
            case (int)MidiScales.E3: return "E3";
            case (int)MidiScales.F3: return "F3";
            case (int)MidiScales.FS3: return "F#3";
            case (int)MidiScales.G3: return "G3";
            case (int)MidiScales.GS3: return "G#3";
            case (int)MidiScales.A3: return "A3";
            case (int)MidiScales.AS3: return "A#3";
            case (int)MidiScales.B3: return "B3";
            case (int)MidiScales.C4: return "C4";
            case (int)MidiScales.CS4: return "C#4";
            case (int)MidiScales.D4: return "D4";
            case (int)MidiScales.DS4: return "D#4";
            case (int)MidiScales.E4: return "E4";
            case (int)MidiScales.F4: return "F4";
            case (int)MidiScales.FS4: return "F#4";
            case (int)MidiScales.G4: return "G4";
            case (int)MidiScales.GS4: return "G#4";
            case (int)MidiScales.A4: return "A4";
            case (int)MidiScales.AS4: return "A#4";
            case (int)MidiScales.B4: return "B4";
            case (int)MidiScales.C5: return "C5";
            case (int)MidiScales.CS5: return "C#5";
            case (int)MidiScales.D5: return "D5";
            case (int)MidiScales.DS5: return "D#5";
            case (int)MidiScales.E5: return "E5";
            case (int)MidiScales.F5: return "F5";
            case (int)MidiScales.FS5: return "F#5";
            case (int)MidiScales.G5: return "G5";
            case (int)MidiScales.GS5: return "G#5";
            case (int)MidiScales.A5: return "A5";
            case (int)MidiScales.AS5: return "A#5";
            case (int)MidiScales.B5: return "B5";
            case (int)MidiScales.C6: return "C6";
            case (int)MidiScales.CS6: return "C#6";
            case (int)MidiScales.D6: return "D6";
            case (int)MidiScales.DS6: return "D#6";
            case (int)MidiScales.E6: return "E6";
            case (int)MidiScales.F6: return "F6";
            case (int)MidiScales.FS6: return "F#6";
            case (int)MidiScales.G6: return "G6";
            case (int)MidiScales.GS6: return "G#6";
            case (int)MidiScales.A6: return "A6";
            case (int)MidiScales.AS6: return "A#6";
            case (int)MidiScales.B6: return "B6";
            case (int)MidiScales.C7: return "C7";
            case (int)MidiScales.CS7: return "C#7";
            case (int)MidiScales.D7: return "D7";
            case (int)MidiScales.DS7: return "D#7";
            case (int)MidiScales.E7: return "E7";
            case (int)MidiScales.F7: return "F7";
            case (int)MidiScales.FS7: return "F#7";
            case (int)MidiScales.G7: return "G7";
            case (int)MidiScales.GS7: return "G#7";
            case (int)MidiScales.A7: return "A7";
            case (int)MidiScales.AS7: return "A#7";
            case (int)MidiScales.B7: return "B7";
            case (int)MidiScales.C8: return "C8";
            default: return null;
        }
    }
}