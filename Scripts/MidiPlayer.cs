
using System;
using System.Collections;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.Midi;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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
    public DebugType debugMode = DebugType.NONE;

    [SerializeField, Header("TextMeshPro object for output the debug information")]
    public TextMeshProUGUI debugLogOutputTarget;

    [SerializeField, Header("What midi channel is acceptable for input?")]
    public int ACCEPTABLE_MIDI_CHANNEL = 0;

    [SerializeField, Header("What CC is acceptable for input?")]
    public int[] ACCEPTABLE_MIDI_CCs = {64};


    [UdonSynced(UdonSyncMode.None)]
    // {Channel, number, velocity or value, MIDIType}
    private sbyte[] LAST_INPUTTED_MIDI = new sbyte[4];

    //
    // these value is used to avoid filling up the SoundSource's priority by a this script.
    //
    // total sum of globalSoundPriorityBaseOffset, RESERVED_SLOTS_BASS and MAX_VOICES is -
    // - Should not be exceeds a 255. otherwise script will stop working.
    private const byte globalSoundPriorityBaseOffset = 160;
    private const byte globalSoundPriorityMax = RESERVED_SLOTS_BASS + MAX_VOICES;
    private byte globalCurrentSoundPriority = globalSoundPriorityBaseOffset;

    private bool isScriptInitialized = false;

    private byte notesPlayingInSameTime = 0;

    private AudioSource[] bassVoices = new AudioSource[RESERVED_SLOTS_BASS];
    private AudioSource[] otherVoices = new AudioSource[MAX_VOICES-RESERVED_SLOTS_BASS];
    private int bassVoiceIndex = 0;
    private int otherVoicesIndex = 0;

    //
    // How many bass sound should be reserved?
    // VRChat sound playback limit is around 30~40, and need to stop oldest sound.
    // But stopping basses frequently is are make sound ugly, so reserve some slots to don't stop the basses.
    //
    private const int RESERVED_SLOTS_BASS = 5;

    //
    // How many voices allowed to play in same time?
    // VRChat sound playback limit is around 30~40, and need to stop oldest sound.
    //
    private const int MAX_VOICES = 30;

    //
    // MIDI number lower than this value is recognized as BASS in runtime.
    //
    public const int BASS_MIDI_CUTOFF = 48;

    private string LOG_PREFIX = $"[Tuna's U# Midi v{TMIDI_VERSION}]";

    private const string TMIDI_VERSION = "0.0.1";


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
            Debug.LogError($"{LOG_PREFIX} Failed to obtain synced midi input from deserialization method! Cancelling the playback!");
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
                EmulateMidiControlChange(channel, number, value);
                break;

            default:
                Debug.LogError($"{LOG_PREFIX} Unknown MIDI type is specified or failed to sync input between master and client!");
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
        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        DebugPress(channel, number, velocity);
        FindAndPlay(number, velocity);
    }

    private void EmulateMidiNoteOff(int channel, int number, int velocity)
    {
        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        DebugRelease(channel, number, velocity);
        // FindAndStop(number);
    }

    private void EmulateMidiControlChange(int channel, int number, int value)
    {
        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        if(!isCCAllowed(number))
            return;

        if(number == 64)
            CCSustain(value);

        DebugCCChange(channel, number, value);
    }


    private void FindAndPlay(int number, int velocity)
    {
        string romanizedScale = GetRomanizedScale(number);

        AudioSource audioSource = GetAudioSourceFromRomanizedScale(romanizedScale);

        if(audioSource == null) {
            if(useIndividualSoundSources) {
                Debug.LogWarning($"{LOG_PREFIX} Failed to get AudioSource component of {romanizedScale}. cancelling the playback.");
            }
            return;
        }

        if(ShouldDebug() && debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} MIDI Playing: scale: {romanizedScale} | pitch: {audioSource.pitch}");
        }


        audioSource.priority = UpdateSoundPriority();

        GameObject cloned = Instantiate(audioSource.gameObject);
        AudioSource clonedAudioSource = cloned.GetComponent<AudioSource>();
        clonedAudioSource.volume = VelocityToVolume((byte)velocity);
        cloned.transform.SetParent(audioSourcesParent.transform);

        

        if(number < BASS_MIDI_CUTOFF) {
            AudioSource currentVoice = bassVoices[bassVoiceIndex];
            if(currentVoice != null) {
                Destroy(currentVoice);
            }
            bassVoices[bassVoiceIndex] = clonedAudioSource;
            bassVoiceIndex = (bassVoiceIndex + 1) % RESERVED_SLOTS_BASS;
        } else {
            AudioSource currentVoice = otherVoices[otherVoicesIndex];
            if(currentVoice != null) {
                Destroy(currentVoice);
            }
            otherVoices[otherVoicesIndex] = clonedAudioSource;
            otherVoicesIndex = (otherVoicesIndex + 1) % (MAX_VOICES-RESERVED_SLOTS_BASS);
        }

        clonedAudioSource.Play();
    }

    private AudioSource GetAudioSourceFromRomanizedScale(string romanizedScale)
    {
        if(!individualAudioSources.TryGetValue(romanizedScale, out var audioSourceComponent)) 
            return null;

        return (AudioSource) audioSourceComponent.Reference;
    }

    private byte UpdateSoundPriority()
    {
        globalCurrentSoundPriority++;
        if(globalCurrentSoundPriority >= globalSoundPriorityMax + globalSoundPriorityBaseOffset) {
            globalCurrentSoundPriority = globalSoundPriorityBaseOffset;
        }
        pressingKeys.SetValue("CurrentPriority", $"Global Current Sound Priority: {globalCurrentSoundPriority}");
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

        string romanizedScale = GetRomanizedScale(number);

        if(debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} MIDI Pressed: " + $"Channel: {channel} | MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");
        }


        if(pressingKeys.ContainsKey($"{channel}-{number}-MIDI"))
            return;

        notesPlayingInSameTime++;
        pressingKeys.Add($"{channel}-{number}-MIDI", $"MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");
    }

    private void DebugRelease(int channel, int number, int velocity)
    {
        if(!ShouldDebug())
            return;


        string romanizedScale = GetRomanizedScale(number);

        if(debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} MIDI Released: " + $"Channel: {channel} | MIDI: {number} | Romanized: {romanizedScale} | Velocity: {velocity}");
        }

        if(!pressingKeys.ContainsKey($"{channel}-{number}-MIDI"))
            return;

        notesPlayingInSameTime--;
        pressingKeys.Remove($"{channel}-{number}-MIDI");
    }

    private void DebugCCChange(int channel, int number, int value)
    {
        if(!ShouldDebug())
            return;

        if(debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} MIDI CC Change Detected: " + $"Channel: {channel} | CC: {number} | Value: {value}");
        }

        pressingKeys.SetValue($"{channel}-{number}-CC", $"Channel: {channel} | CC: {number} | Value: {value}");
    }

    private void SyncMidiInput(int channel, int number, int value, MIDIType midiType)
    {
        LAST_INPUTTED_MIDI[0] = Convert.ToSByte(channel);
        LAST_INPUTTED_MIDI[1] = Convert.ToSByte(number);
        LAST_INPUTTED_MIDI[2] = Convert.ToSByte(value);
        LAST_INPUTTED_MIDI[3] = Convert.ToSByte(midiType);

        if(debugMode.HasDebugType(DebugType.CONSOLE))
            Debug.Log($"{LOG_PREFIX} LAST INPUTTED MIDI: {LAST_INPUTTED_MIDI[0]} {LAST_INPUTTED_MIDI[1]} {LAST_INPUTTED_MIDI[2]} {LAST_INPUTTED_MIDI[3]} ");

        RequestSerialization();
        ResetLastInputtedMidi();
    }

    private void ResetLastInputtedMidi()
    {
        LAST_INPUTTED_MIDI[0] = Convert.ToSByte(-1);
        LAST_INPUTTED_MIDI[1] = Convert.ToSByte(-1);
        LAST_INPUTTED_MIDI[2] = Convert.ToSByte(-1);
        LAST_INPUTTED_MIDI[3] = Convert.ToSByte(-1);
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
        if(debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} Sustain CC detected and enabling sustain");
        }
    }

    private void CCDisableSustain()
    {
        if(debugMode.HasDebugType(DebugType.CONSOLE)) {
            Debug.Log($"{LOG_PREFIX} Sustain CC detected and disabling sustain");
        }
    }

    private void Setup()
    {
        Debug.Log($"{LOG_PREFIX} Initializing script...");
        ResetLastInputtedMidi();
        if(useIndividualSoundSources) {
            Debug.Log($"{LOG_PREFIX} We are using individual sound samples!");
            foreach(var child in audioSourcesParent.GetComponentsInChildren<AudioSource>()) {
                individualAudioSources.Add($"{child.gameObject.name}", child);
            }
        }
        else {
            Debug.Log($"{LOG_PREFIX} We are using pitched sound samples!");
            GameObject instantiateTarget = null;

            foreach(var child in audioSourcesParent.GetComponentsInChildren<AudioSource>()) {
                if(child.gameObject.name.Equals("C5", StringComparison.OrdinalIgnoreCase)) {
                    instantiateTarget = child.gameObject;
                    break;
                }
            }

            if(!Utilities.IsValid(instantiateTarget)) {
                Debug.LogError($"{LOG_PREFIX} Failed to find C5 AudioSource component! Stopping script!");
                return;
            }

            for(int i = MidiScalesExtensions.MIN_RANGE; i < MidiScalesExtensions.MAX_RANGE; i++) {
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
        Debug.Log($"{LOG_PREFIX} Script has been initialized successfully!");
    }

    private void Loop()
    {
        if(!ShouldDebug())
            return;
        
        PrintDebugInfoToWorld();
    }

    private void PrintDebugInfoToWorld()
    {       
        if(!ShouldDebug())
            return;

        if(!debugMode.HasDebugType(DebugType.WORLD_TEXT)) {
            debugLogOutputTarget.text = "";
            return;
        }


        var keys = pressingKeys.GetKeys();
        pressingKeys.TryGetValue("CurrentPriority", out var currentPriorityString);

        var outputString = "MIDI INPUTS:\n";
        outputString += currentPriorityString + "\n";
        outputString += $"Current MIDI inputs: {notesPlayingInSameTime}" + "\n";


        foreach(var key in keys.ToArray())
        {
            if(key.String.Equals("CurrentPriority", StringComparison.OrdinalIgnoreCase))
                continue;

            if(!pressingKeys.TryGetValue(key, out var reference))
                continue;

            outputString += reference.String + "\n";
        }

        debugLogOutputTarget.text = outputString;
    }

    private bool ShouldDebug()
    {
        return Utilities.IsValid(debugLogOutputTarget) && debugMode != DebugType.NONE;
    }

    private const float DEFAULT_VOLUME_VELOCITY = 100f;

    private static float VelocityToVolume(byte velocity)
    {
        return velocity / DEFAULT_VOLUME_VELOCITY;
    }
}

public enum MIDIType
{
    PRESS = 0,
    RELEASE,
    CC
}

[Flags]
public enum DebugType
{
    NONE = 0,
    CONSOLE = 1,
    WORLD_TEXT = 2,
}

public static class DebugTypeExtension
{
    public static bool HasDebugType(this DebugType debugMode, DebugType check)
    {
        return ((int)debugMode & (int)check) == (int)check;
    }
}