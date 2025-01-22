
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.Midi;
using VRC.SDKBase;
using VRC.Udon;

public class MidiPlayer : UdonSharpBehaviour
{

    private DataDictionary pianos = new DataDictionary();

    private DataDictionary pressingKeys = new DataDictionary();
    private DataDictionary detectedCCs = new DataDictionary();


    // When this
    [
        SerializeField, 
        Header("Use individual samples instead of pitched C5 sound?"),
        Tooltip("When this enabled, the program will use pitched C5 sound instead of individual samples")
    ]
    public bool useIndividualSoundSources = false;

    [SerializeField, Header("Parent of audio sources objects")]
    public GameObject audioSourcesParent;


    [SerializeField, Header("Enable debug mode?")]
    public bool debugMode = false;

    [SerializeField, Header("TextMeshPro object for output the debug information")]
    public TextMeshProUGUI debugLogOutputTarget;

    [SerializeField, Header("What midi channel is acceptable for input?")]
    public int ACCEPTABLE_MIDI_CHANNEL = 0;

    [SerializeField, Header("What CC is acceptable for input?")]
    public int[] ACCEPTABLE_MIDI_CCs = {64};


    void Start()
    {
        Setup();
    }

    void Update()
    {
        Loop();
    }

    public override void MidiNoteOn(int channel, int number, int velocity)
    {
        DebugPress(channel, number, velocity);
    }

    public override void MidiNoteOff(int channel, int number, int velocity)
    {
        DebugRelease(channel, number, velocity);
    }

    public override void MidiControlChange(int channel, int number, int value)
    {
        DebugCCChange(channel, number, value);
    }


    private void FindAndPlay()
    {

    }

    private string GetRomanizedScale(int number)
    {
        string enumName = MidiScalesExtensions.GetName(number);
        
        if(enumName == null) {
            return "ERROR_OUT_OF_SCALES";
        }

        return enumName;
    }

    private void DebugPress(int channel, int number, int velocity)
    {
        if(!ShouldDebug())
            return;

        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        string romanizedScale = GetRomanizedScale(number);
        Debug.Log("MIDI Pressed: " + $"Channel: {channel} | MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");

        if(pressingKeys.ContainsKey($"{channel}-{number}-MIDI"))
            return;

        pressingKeys.Add($"{channel}-{number}-MIDI", $"MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");
    }

    private void DebugRelease(int channel, int number, int velocity)
    {
        if(!ShouldDebug())
            return;

        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        string romanizedScale = GetRomanizedScale(number);
        Debug.Log("MIDI Released: " + $"Channel: {channel} | MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");
        pressingKeys.Remove($"{channel}-{number}-MIDI");
    }

    private void DebugCCChange(int channel, int number, int value)
    {
        if(!ShouldDebug())
            return;

        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        if(!isCCAllowed(number))
            return;

        Debug.Log("MIDI CC Change Detected: " + $"Channel: {channel} | CC: {number} | Value: {value}");
        pressingKeys.SetValue($"{channel}-{number}-CC", $"Channel: {channel} | CC: {number} | Value: {value}");


        if(number == 64)
            CCSustain(value);

    }

    private bool isCCAllowed(int value)
    {
        for(int i = 0; i < ACCEPTABLE_MIDI_CCs.Length; i++)
        {
            if(ACCEPTABLE_MIDI_CCs[i] == value)
                return true;
        }
        return false;
    }

    private void CCSustain(int value)
    {
        if(value == 0)
        {
            CCDisableSustain();
        }
        else if(value == 127)
        {
            CCEnableSustain();
        }
    }

    private void CCEnableSustain()
    {
        Debug.Log("Sustain CC detected and enabling sustain");
    }

    private void CCDisableSustain()
    {
        Debug.Log("Sustain CC detected and disabling sustain");
    }

    private void Setup()
    {

    }

    private void Loop()
    {
        if(!ShouldDebug())
            return;
        
        PrintDebugInfoToWorld();
    }

    private void PrintDebugInfoToWorld()
    {
        var keys = pressingKeys.GetKeys();
        var outputString = "MIDI INPUTS:\n";


        foreach(var key in keys.ToArray())
        {
            if(!pressingKeys.TryGetValue(key, out var reference))
                continue;

            outputString += reference.String + "\n";
        }

        debugLogOutputTarget.text = outputString;
    }

    private bool ShouldDebug()
    {
        return Utilities.IsValid(debugLogOutputTarget) && debugMode;
    }
}
