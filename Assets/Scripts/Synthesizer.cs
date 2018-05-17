using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synthesizer : MonoBehaviour {

    #region Classes

    public class Voice
    {
        public int noteNumber;
        public float velocity;

        float frequency;
        float gain;
        float sampleRate;
        double phase;
        double increment;

        System.Random random = new System.Random();

        [SerializeField] WaveType waveType;

        bool isActive;

        public Voice(WaveType wavetype)
        {
            noteNumber = -1;
            velocity = 0;

            isActive = false;

            this.waveType = wavetype;
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
                            data[i + j] = gain * Mathf.Sin((float)phase);
                            break;

                        case WaveType.Square:
                            if (Mathf.Sin((float)phase) >= 0)
                            {
                                data[i + j] = gain;
                            }
                            else
                            {
                                data[i + j] = -gain;
                            }
                            break;

                        case WaveType.Sawtooth:
                            data[i + j] = gain * (Mathf.InverseLerp(Mathf.PI * 2, 0, (float)phase) * 2 - 1);
                            break;

                        case WaveType.Triangle:
                            data[i + j] = gain * (Mathf.PingPong((float)phase, 1f) * 2 - 1);
                            break;

                        case WaveType.Noise:
                            data[i + j] = gain * (((float)random.NextDouble() * 2f) - 1);
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

    #endregion

    #region Fields

    private float sampleRate;
    double phase;
    double increment;
    float sawtoothValue = 0f;

    System.Random random = new System.Random();

    bool isActive = false;
    private int activeNoteNumber = -1;

    [SerializeField] WaveType waveType = WaveType.Sine;

    float frequency = 440f;

    private NoteInput noteInput;

    [SerializeField, Range(0, 10)] float gain = 1f;



    #endregion

    #region Unity Messages

    private void Awake()
    {
        sampleRate = (float)AudioSettings.outputSampleRate;
        noteInput = GetComponent<NoteInput>();
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
        if (!isActive) return;
        Debug.LogFormat("Buffer size: {0} | Channel count: {1}", data.Length / channels, channels);

        for(int i = 0; i < data.Length; i++)
        {
            data[i] = 0f;
        }

        increment = frequency * 2 * Mathf.PI / sampleRate;

        for(int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            if(phase > (Mathf.PI * 2))
            {
                phase -= Mathf.PI * 2;
            }

            for(int j = 0; j < channels; j++)
            {
                switch(waveType)
                {
                    case WaveType.Sine:
                        data[i + j] = gain * Mathf.Sin((float)phase);
                        break;

                    case WaveType.Square:
                        if(Mathf.Sin((float)phase) >= 0)
                        {
                            data[i + j] = gain;
                        }
                        else
                        {
                            data[i + j] = -gain;
                        }
                        break;

                    case WaveType.Sawtooth:
                        data[i + j] = gain * (Mathf.InverseLerp(Mathf.PI * 2, 0, (float)phase) * 2 - 1);
                        break;

                    case WaveType.Triangle:
                        data[i + j] = gain * (Mathf.PingPong((float)phase, 1f) * 2 - 1);
                        break;

                    case WaveType.Noise:
                        data[i + j] = gain * (((float)random.NextDouble() * 2f) -1);
                        break;

                    default:
                        break;
                }
            }
        }

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
        frequency = NoteToFrequency(noteNumber);
        isActive = true;
        activeNoteNumber = noteNumber;
    }

    void Input_OnNoteOff(int noteNumber)
    {
        if (noteNumber == activeNoteNumber)
        {
            isActive = false;
        }
    }

    #endregion

}
