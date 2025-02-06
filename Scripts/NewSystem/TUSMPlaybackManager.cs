using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TUSMPlaybackManager : UdonSharpBehaviour
{
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


    //
    // these value is used to avoid filling up the SoundSource's priority by a this script.
    //
    // total sum of globalSoundPriorityBaseOffset, RESERVED_SLOTS_BASS and MAX_VOICES is -
    // - Should not be exceeds a 255. otherwise script will stop working.
    private const byte globalSoundPriorityBaseOffset = 160;
    private const byte globalSoundPriorityMax = RESERVED_SLOTS_BASS + MAX_VOICES;
    private byte globalCurrentSoundPriority = globalSoundPriorityBaseOffset;


    public void PlaySound(AudioSource audioSource, int midiNumber, int velocity)
    {

        GameObject cloned = Instantiate(audioSource.gameObject);
        AudioSource clonedAudioSource = cloned.GetComponent<AudioSource>();

        clonedAudioSource.priority = UpdateAndGetSoundPriority();
        clonedAudioSource.volume = VelocityToVolume((byte)velocity);

        if(midiNumber < BASS_MIDI_CUTOFF) {
            AudioSource currentVoice = bassVoices[bassVoiceIndex];
            if(currentVoice != null) {
                Destroy(currentVoice.gameObject);
            }
            bassVoices[bassVoiceIndex] = clonedAudioSource;
            bassVoiceIndex = (bassVoiceIndex + 1) % RESERVED_SLOTS_BASS;
        } else {
            AudioSource currentVoice = otherVoices[otherVoicesIndex];
            if(currentVoice != null) {
                Destroy(currentVoice.gameObject);
            }
            otherVoices[otherVoicesIndex] = clonedAudioSource;
            otherVoicesIndex = (otherVoicesIndex + 1) % (MAX_VOICES-RESERVED_SLOTS_BASS);
        }

        clonedAudioSource.Play();
    }


    private const float DEFAULT_VOLUME_VELOCITY = 100f;

    private float VelocityToVolume(byte velocity)
    {
        return velocity / DEFAULT_VOLUME_VELOCITY;
    }

    private byte UpdateAndGetSoundPriority()
    {
        globalCurrentSoundPriority++;
        if(globalCurrentSoundPriority >= globalSoundPriorityMax + globalSoundPriorityBaseOffset) {
            globalCurrentSoundPriority = globalSoundPriorityBaseOffset;
        }
        return globalCurrentSoundPriority;
    }
}
