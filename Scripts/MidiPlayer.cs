
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
        Tooltip("When this disabled, the program will use pitched C5 sound instead of individual samples")
    ]
    public bool useIndividualSoundSources = false;

    [SerializeField, Header("Parent of audio sources objects")]
    public GameObject audioSourcesParent;

    private DataDictionary individualAudioSources = new DataDictionary();


    [SerializeField, Header("Enable debug mode?")]
    public bool debugMode = false;

    [SerializeField, Header("TextMeshPro object for output the debug information")]
    public TextMeshProUGUI debugLogOutputTarget;

    [SerializeField, Header("What midi channel is acceptable for input?")]
    public int ACCEPTABLE_MIDI_CHANNEL = 0;

    [SerializeField, Header("What CC is acceptable for input?")]
    public int[] ACCEPTABLE_MIDI_CCs = {64};


    [UdonSynced(UdonSyncMode.None)]
    // {Channel, number, velocity or value, MIDIType}
    private sbyte[] LAST_INPUTTED_MIDI = {-1, -1, -1, -1};


    private byte globalCurrentSoundPriority = 255;

    private bool isScriptInitialized = false;

    private byte notesPlayingInSameTime = 0;

    void Start()
    {
        Setup();
    }

    void Update()
    {
        Loop();
    }

    public override void OnDeserialization()
    {
        if(!isScriptInitialized)
            return;

        sbyte channel = LAST_INPUTTED_MIDI[0];
        sbyte number = LAST_INPUTTED_MIDI[1];
        sbyte value = LAST_INPUTTED_MIDI[2];
        MIDIType midiType = (MIDIType) LAST_INPUTTED_MIDI[3];



        if(
            channel == -1 ||
            number == -1 ||
            value == -1
        ) {
            ResetLastInputtedMidi();
            Debug.LogError("Failed to obtain synced midi input from deserialization method! Cancelling the playback!");
            return;
        }

        ResetLastInputtedMidi();

        switch(midiType) {
            case MIDIType.PRESS:
                EmulateMidiNoteOn(channel, number, value);
                break;

            case MIDIType.RELEASE:
                EmulateMidiNoteOff(channel, number, value);
                break;

            case MIDIType.CC:
                EmulateMidiNoteOn(channel, number, value);
                break;

            default:
                Debug.LogError("Unknown MIDI type is specified or failed to sync input between master and client!");
                break;
        }
    }

    public override void MidiNoteOn(int channel, int number, int velocity)
    {
        if(!isScriptInitialized)
            return;

        SyncMidiInput(channel, number, velocity, MIDIType.PRESS);
        EmulateMidiNoteOn(channel, number, velocity);
    }

    public override void MidiNoteOff(int channel, int number, int velocity)
    {
        if(!isScriptInitialized)
            return;

        SyncMidiInput(channel, number, velocity, MIDIType.RELEASE);
        EmulateMidiNoteOff(channel, number, velocity);
    }

    public override void MidiControlChange(int channel, int number, int value)
    {
        if(!isScriptInitialized)
            return;

        SyncMidiInput(channel, number, value, MIDIType.CC);
        EmulateMidiControlChange(channel, number, value);
    }

    public void EmulateMidiNoteOn(int channel, int number, int velocity)
    {
        DebugPress(channel, number, velocity);
        FindAndPlay(number, velocity);
        notesPlayingInSameTime++;
    }

    private void EmulateMidiNoteOff(int channel, int number, int velocity)
    {
        DebugRelease(channel, number, velocity);
        notesPlayingInSameTime--;
    }

    private void EmulateMidiControlChange(int channel, int number, int value)
    {
        DebugCCChange(channel, number, value);
    }


    private void FindAndPlay(int number, int velocity)
    {
        string romanizedScale = GetRomanizedScale(number);

        if(!individualAudioSources.TryGetValue(romanizedScale, out var audioSourceComponent)) {
            Debug.LogWarning($"Failed to get AudioSource component of {romanizedScale}. cancelling the playback.");
            return;
        }

        AudioSource audioSource = (AudioSource) audioSourceComponent.Reference;

        Debug.Log($"MIDI Playing: scale: {romanizedScale} | pitch: {audioSource.pitch}");

        audioSource.priority = UpdateSoundPriority();
        // TODO()
        // Use PlayOneShot() instead Play() for non clipped sound.
        // audioSource.PlayOneShot(audioSource.clip);
        audioSource.Play();
    }

    private byte UpdateSoundPriority()
    {
        if(globalCurrentSoundPriority == byte.MaxValue){
            globalCurrentSoundPriority = 0;
        }
        else {
            globalCurrentSoundPriority++;
        }
        pressingKeys.SetValue("CurrentPriority", $"Global Current Sound Priority: {globalCurrentSoundPriority} | Notes playing in same time: {notesPlayingInSameTime}");
        return globalCurrentSoundPriority;
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

    private void SyncMidiInput(int channel, int number, int value, MIDIType midiType)
    {
        LAST_INPUTTED_MIDI[0] = (sbyte) channel;
        LAST_INPUTTED_MIDI[1] = (sbyte) number;
        LAST_INPUTTED_MIDI[2] = (sbyte) value;
        LAST_INPUTTED_MIDI[3] = (sbyte) MIDIType.PRESS;
        RequestSerialization();
        ResetLastInputtedMidi();
    }

    private void ResetLastInputtedMidi()
    {
        LAST_INPUTTED_MIDI[0] = -1;
        LAST_INPUTTED_MIDI[1] = -1;
        LAST_INPUTTED_MIDI[2] = -1;
        LAST_INPUTTED_MIDI[3] = -1;
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
        if(useIndividualSoundSources) {
            foreach(var child in audioSourcesParent.GetComponentsInChildren<AudioSource>()) {
                individualAudioSources.Add($"{child.gameObject.name}", child);
            }
        }
        else {
            GameObject instantiateTarget = null;

            foreach(var child in audioSourcesParent.GetComponentsInChildren<AudioSource>()) {
                if(child.gameObject.name.Equals("C5", StringComparison.OrdinalIgnoreCase)) {
                    instantiateTarget = child.gameObject;
                    break;
                }
            }

            if(!Utilities.IsValid(instantiateTarget)) {
                Debug.LogError("Failed to find C5 AudioSource component! Stopping script!");
                return;
            }

            for(int i = 0; i < (int)MidiScales.B8; i++) {
                string romanizedScale = GetRomanizedScale(i);
                GameObject instantiatedGameObject = Instantiate(instantiateTarget);
                instantiatedGameObject.name = romanizedScale;

                AudioSource instantiatedAudioSource = instantiatedGameObject.GetComponent<AudioSource>();

                float playBackPitch = MidiPitchConverter.MidiNoteToPitch(i);
                instantiatedAudioSource.pitch = playBackPitch;
                individualAudioSources.Add($"{instantiatedAudioSource.gameObject.name}", instantiatedAudioSource);
            }
            
        }
        isScriptInitialized = true;
        Debug.Log("Script has been initialized successfully!");
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

public enum MIDIType
{
    PRESS = 0,
    RELEASE,
    CC
}
