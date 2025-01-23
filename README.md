[English] [[日本語]](./README_JA.md)

# tuna's USharp MIDI

- [tuna's USharp MIDI](#tunas-usharp-midi)
  - [Setup](#setup)
    - [Sounds Folder Structure](#sounds-folder-structure)
  - [Settings](#settings)
    - [Use individual Sound Sources](#use-individual-sound-sources)
    - [Debug Mode](#debug-mode)
    - [Debug Log Output Target](#debug-log-output-target)
    - [ACCEPTABLE MIDI CHANNEL](#acceptable-midi-channel)
    - [ACCEPTABLE MIDI CCS](#acceptable-midi-ccs)
  - [Problem in VRChat](#problem-in-vrchat)


## Setup


1. Import TextMeshProUGUI
2. Import `tuna's midi piano.unitypackage`
3. Audio Setup
    - If you planned to use individual samples per note
        1. Setup audio files and folders (see [Sounds Folder Structure](#sounds-folder-structure))
        2. Open: Tools -> tuna -> Midi -> Audio Setup Tool
        3. Set `Audio Files Folder Path` to root of sounds folder structure (e.g. Assets/Sounds/5/ = Assets/Sounds/)
        4. Set `Parent Object` to object named AudioSources in prefab
        5. Press "Setup Audio Objects"
        6. Check `Use Individual Sound Sources`
        7. Audio setup is done
    - If you planned to use pitched sound with one sample
        1. Setup audio files and folders (see [Sounds Folder Structure](#sounds-folder-structure))
        2. Put C5 sound file to `5/C5`
        3. Uncheck `Use Individual Sound Sources`
        4. Done
4. Click root object of prefab and Edit Script settings (see [Settings](#settings))
5. Done


### Sounds Folder Structure

1. Sound files are requires to naming with Music Scale. (e.g. C1, CS2, D3, DS2) Also Scale is FL Studio base, so C5 is a MIDI 60 (center C).
2. Folders are requires to naming with octave number (e.g. 2, 3, 4)

Example structure:

```
- root
    - Editor
    - Scripts
    - Sounds
        - 2
            - C2
            - CS2
            - D2
            - DS2
            - E2
            - FS2
            - G2
            - GS2
            - A2
            - AS2
            - B
        - 3
            - C3
            - CS3
            - ...
```



## Settings

### Use individual Sound Sources

When this disabled, the program will use pitched C5 sound instead of individual samples.

### Debug Mode

Select debug mode.

- WORLD_TEXT: Display current MIDI input in World Text
- CONSOLE: Everything will be logged to console, (This log is will saved in VRChat folder)


### Debug Log Output Target

If you want to use WORLD_TEXT debug mode, You need to attach Text Mesh Pro UGUI component in here.

### ACCEPTABLE MIDI CHANNEL

What midi channel is acceptable for input?

### ACCEPTABLE MIDI CCS

MIDI CC is currently not supported.



## Problem in VRChat

We cannot play around 30+ sounds in same time, when we try to play about 30 sounds are correctly played, but sounds failed to playback audio after over sounds

so I've limiting the sounds can be played on same time to avoid this problem.

