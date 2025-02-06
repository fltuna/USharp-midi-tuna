
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TUSMPlayer : UdonSharpBehaviour
{

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(LastInputtedMIDIVelocity))]
    private byte _lastInputtedMIDIVelocity = 0;

    public byte LastInputtedMIDIVelocity
    {
        get => _lastInputtedMIDIVelocity;
        set
        {
            _lastInputtedMIDIVelocity = value;

            if(!isScriptInitialized)
                return;

            EmulateMidiNoteOn(ACCEPTABLE_MIDI_CHANNEL, targetMidiNumber, value);
        }
    }

    private bool isScriptInitialized = false;

    [SerializeField, Header("This object playback a specified sound with corresponding midi number.")]
    public sbyte targetMidiNumber = -1;

    [SerializeField, Header("What midi channel is acceptable for input?")]
    public int ACCEPTABLE_MIDI_CHANNEL = 0;

    [SerializeField, Header("Do not touch this")]
    private TUSMPlaybackManager tusmPlaybackManager;

    void Start()
    {
        init();
    }

    public override void Interact()
    {
        EmulateMidiNoteOn(ACCEPTABLE_MIDI_CHANNEL, targetMidiNumber);
    }

    public override void MidiNoteOn(int channel, int number, int velocity)
    {
        if(!isScriptInitialized)
            return;

        EmulateMidiNoteOn(channel, number, velocity);
    }

    public void EmulateMidiNoteOn(int channel, int number, int velocity = 100)
    {
        if(channel != ACCEPTABLE_MIDI_CHANNEL)
            return;

        if(number != targetMidiNumber)
            return;

        var player = Networking.LocalPlayer;
        Networking.SetOwner(player, this.gameObject);

        if(player.IsOwner(this.gameObject))
        {
            PlaySound(ACCEPTABLE_MIDI_CHANNEL, targetMidiNumber, velocity);
        }
    }

    private void PlaySound(int channel, int number, int velocity)
    {

        AudioSource audioSource = GetComponent<AudioSource>();

        RequestSyncMidiInputTiming();
        tusmPlaybackManager.PlaySound(audioSource, number, velocity);
    }

    private void RequestSyncMidiInputTiming()
    {
        _lastInputtedMIDIVelocity = Convert.ToByte(UnityEngine.Random.Range(0, 255));
    }

    private void init()
    {
        if(targetMidiNumber < 0)
        {
            Debug.LogError($"{TUSMConstants.LOG_PREFIX} Invalid MIDI number was specified in editor! Terminating script...");
            return;
        }

        if(tusmPlaybackManager == null)
        {
            Debug.LogError($"{TUSMConstants.LOG_PREFIX} Could not find the TUSMPLayerManager! Terminating script...");
        }

        isScriptInitialized = true;
    }
}
