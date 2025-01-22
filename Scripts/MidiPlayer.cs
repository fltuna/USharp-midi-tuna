
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Midi;
using VRC.SDKBase;
using VRC.Udon;

public class MidiPlayer : UdonSharpBehaviour
{

    [SerializeField] private VRCMidiPlayer player;

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

        base.MidiNoteOn(channel, number, velocity);
    }

    public override void MidiNoteOff(int channel, int number, int velocity)
    {
        base.MidiNoteOff(channel, number, velocity);
    }



    private void Setup()
    {

    }

    private void Loop()
    {

    }
}
