using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synthesizer : MonoBehaviour {

    #region Classes

    public class Voice
    {
        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        private int noteNumber;
        private float velocity;

        float frequency;
        float gain;
        float sampleRate;
        double phase;
        double increment;

        System.Random random = new System.Random();

        [SerializeField] WaveType waveType;

        private bool isActive;

        public Voice(WaveType wavetype, float gain)
        {
            noteNumber = -1;
            velocity = 0;

            isActive = false;

            this.waveType = wavetype;
            this.gain = gain;
        }

        public void NoteOn(int noteNumber, float velocity)
        {
            this.noteNumber = noteNumber;
            this.velocity = velocity;
            this.frequency = Synthesizer.NoteToFrequency(noteNumber);

            phase = 0;
            sampleRate = AudioSettings.outputSampleRate;

            isActive = true;
        }

        public void NoteOff(int noteNumber)
        {
            if(noteNumber == this.noteNumber)
            {
                isActive = false;
            }
        }

        public void WriteAudioBuffer(ref float[] data, int channels)
        {
            if (!isActive) return;

            increment = frequency * 2 * Mathf.PI / sampleRate;

            // Write audio buffer

            for (int i = 0; i < data.Length; i += channels)
            {
                phase += increment;
                if (phase > (Mathf.PI * 2))
                {
                    phase -= Mathf.PI * 2;
                }

                for (int j = 0; j < channels; j++)
                {
                    switch (waveType)
                    {
                        case WaveType.Sine:
                            data[i + j] += gain * Mathf.Sin((float)phase);
                            break;

                        case WaveType.Square:
                            if (Mathf.Sin((float)phase) >= 0)
                            {
                                data[i + j] += gain;
                            }
                            else
                            {
                                data[i + j] += -gain;
                            }
                            break;

                        case WaveType.Sawtooth:
                            data[i + j] += gain * (Mathf.InverseLerp(Mathf.PI * 2, 0, (float)phase) * 2 - 1);
                            break;

                        case WaveType.Triangle:
                            data[i + j] += gain * (Mathf.PingPong((float)phase, 1f) * 2 - 1);
                            break;

                        case WaveType.Noise:
                            data[i + j] += gain * (((float)random.NextDouble() * 2f) - 1);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

    }

    #endregion

    public enum WaveType
    {
        Sine,
        Square,
        Sawtooth,
        Triangle,
        Noise
    }

    #region Constants

    const int fixedNoteNumber = 69;
    const float fixedFrequency = 440f;
    const int polyphony = 32;

    #endregion

    #region Fields

    [SerializeField] WaveType waveType = WaveType.Sine;
    [SerializeField, Range(0, 1)] float gain = 1f;
    [SerializeField] int transpose = 0;
    private int octave;

    private Voice[] voicesPool;
    private List<Voice> activeVoices;
    private Stack<Voice> freeVoices;
    private Dictionary<int, Voice> noteDict;
    
    private NoteInput noteInput;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        noteInput = GetComponent<NoteInput>();

        voicesPool = new Voice[polyphony];
        activeVoices = new List<Voice>();
        freeVoices = new Stack<Voice>();
        noteDict = new Dictionary<int, Voice>();

        for(int i = 0; i < voicesPool.Length; i++)
        {
            voicesPool[i] = new Voice(waveType, gain);
            freeVoices.Push(voicesPool[i]);
        }
    }

    private void OnEnable()
    {
        if(noteInput != null)
        {
            noteInput.OnNoteOn -= Input_OnNoteOn;
            noteInput.OnNoteOn += Input_OnNoteOn;

            noteInput.OnNoteOff -= Input_OnNoteOff;
            noteInput.OnNoteOff += Input_OnNoteOff;
        }
    }

    private void OnDisable()
    {
        if(noteInput != null)
        {
            noteInput.OnNoteOn -= Input_OnNoteOn;
            noteInput.OnNoteOff -= Input_OnNoteOff;
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }

        for (int i = activeVoices.Count -1; i < 0; i--)
        {
            activeVoices[i].WriteAudioBuffer(ref data, channels);
            if(!activeVoices[i].IsActive)
            {
                freeVoices.Push(activeVoices[i]);
                activeVoices.RemoveAt(i);
            }
        }

        //TODO ? (Unknown challenge)
    }

    #endregion

# region Static Public Functions
    static public float NoteToFrequency(int noteNumber)
    {
        float twelfthRoot = Mathf.Pow(2f, (1f / 12f));
        return fixedFrequency * Mathf.Pow(twelfthRoot, noteNumber - fixedNoteNumber);
    }
    #endregion

    #region Event Handler

    void Input_OnNoteOn(int noteNumber, float velocity)
    {
        noteNumber += transpose + 12 * octave;

        if(noteDict.ContainsKey(noteNumber))
        {
            return;
        }

        if(freeVoices.Count > 0)
        {
            Voice voice = freeVoices.Pop();
            voice.NoteOn(noteNumber, velocity);
            activeVoices.Add(voice);
            noteDict.Add(noteNumber, voice);
        }
    }

    void Input_OnNoteOff(int noteNumber)
    {
        noteNumber += transpose + 12 * octave;

        if (noteDict.ContainsKey(noteNumber))
        {
            noteDict[noteNumber].NoteOff(noteNumber);
            noteDict.Remove(noteNumber);
        }
        
    }

    #endregion

}
