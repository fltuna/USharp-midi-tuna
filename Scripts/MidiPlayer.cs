
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Midi;
using VRC.SDKBase;
using VRC.Udon;

public class MidiPlayer : UdonSharpBehaviour
{

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

    public override void MidiControlChange(int channel, int number, int value)
    {
        base.MidiControlChange(channel, number, value);
    }


    private void FindAndPlay()
    {

    }

    private string GetRomanizedScale()
    {
        return "ERROR_TYPE_NONE";
    }

    private void Setup()
    {

    }

    private void Loop()
    {

    }
}
