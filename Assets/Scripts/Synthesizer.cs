using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synthesizer : MonoBehaviour {

    #region Classes

    public class Voice
    {
        public bool IsActive { get { return isActive; } }

        private int noteNumber;
        private float velocity;

        float frequency;
        float gain;
        float sampleRate;
        double phase;
        double increment;

        private System.Random random = new System.Random();

        private WaveType waveType;

        bool isActive;

        public Voice(WaveType waveType, float gain)
        {
            noteNumber = -1;
            velocity = 0;

            isActive = false;

            this.waveType = waveType;
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
            if (noteNumber == this.noteNumber)
            {
                isActive = false;
            }
        }

        public void WriteAudioBuffer(ref float[] data, int channels)
        {
            if (!isActive) return;

            increment = frequency * 2 * Mathf.PI / sampleRate;

            // write audio buffer
            for (int i = 0; i < data.Length; i += channels)
            {
                phase += increment;

                if (phase > (Mathf.PI * 2))
                {
                    phase -= Mathf.PI * 2;
                }

                for (int c = 0; c < channels; c++)
                {
                    switch (waveType)
                    {
                        case WaveType.Sine:
                            data[i + c] += gain * Mathf.Sin((float)phase);
                            break;

                        case WaveType.Square:
                            if (Mathf.Sin((float)phase) >= 0)
                            {
                                data[i + c] += gain;
                            }
                            else
                            {
                                data[i + c] += -gain;
                            }
                            break;

                        case WaveType.Triangle:
                            data[i + c] += gain * (Mathf.PingPong((float)phase, 1f) * 2f - 1f);
                            break;

                        case WaveType.Sawtooth:
                            data[i + c] += gain * (Mathf.InverseLerp(0, Mathf.PI * 2, (float)phase) * 2 - 1f);
                            break;

                        case WaveType.Noise:
                            data[i + c] += gain * (((float)random.NextDouble() * 2f) - 1f);
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
        Triangle,
        Sawtooth,
        Noise
    }

    #region Constants

    const int fixedNoteNumber = 69;
    const float fixedFrequency = 440f;
    const int polyphony = 32;

    #endregion


    #region Private Fields

    private NoteInput input;

    [SerializeField]
    private WaveType waveType = WaveType.Sine;

    [SerializeField, Range(0, 1f)]
    private float gain = 1f;

    [SerializeField]
    private int transpose = 0;
    private int octave = 0;

    private Voice[] voicesPool;
    private List<Voice> activeVoices;
    private Stack<Voice> freeVoices;
    private Dictionary<int, Voice> noteDict;

    #endregion


    #region Unity Messages

    private void Awake()
    {
        input = GetComponent<NoteInput>();

        voicesPool = new Voice[polyphony];
        activeVoices = new List<Voice>();
        freeVoices = new Stack<Voice>();
        noteDict = new Dictionary<int, Voice>();

        for (int i = 0; i < voicesPool.Length; i++)
        {
            voicesPool[i] = new Voice(waveType, gain);
            freeVoices.Push(voicesPool[i]);
        }
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.OnNoteOn -= Input_OnNoteOn;
            input.OnNoteOn += Input_OnNoteOn;

            input.OnNoteOff -= Input_OnNoteOff;
            input.OnNoteOff += Input_OnNoteOff;
        }
    }


    private void OnDisable()
    {
        if (input != null)
        {
            input.OnNoteOn -= Input_OnNoteOn;

            input.OnNoteOff -= Input_OnNoteOff;
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }

        for (int i = activeVoices.Count - 1; i >= 0; i--)
        {
            activeVoices[i].WriteAudioBuffer(ref data, channels);
            if (activeVoices[i].IsActive == false)
            {
                freeVoices.Push(activeVoices[i]);
                activeVoices.RemoveAt(i);
            }
        }

        // TODO: ?
    }

    #endregion


    #region Static Public Functions

    static public float NoteToFrequency(int noteNumber)
    {
        float twelfthRoot = Mathf.Pow(2f, (1f / 12f));
        return fixedFrequency * Mathf.Pow(twelfthRoot, noteNumber - fixedNoteNumber);
    }

    #endregion


    #region Event Handler

    private void Input_OnNoteOn(int noteNumber, float velocity)
    {
        noteNumber += transpose + 12 * octave;

        Debug.LogFormat("NoteOn: {0}", noteNumber);

        if (noteDict.ContainsKey(noteNumber))
        {
            return;
        }

        if (freeVoices.Count > 0)
        {
            Voice voice = freeVoices.Pop();
            voice.NoteOn(noteNumber, velocity);
            activeVoices.Add(voice);
            noteDict.Add(noteNumber, voice);
        }
    }

    private void Input_OnNoteOff(int noteNumber)
    {
        noteNumber += transpose + 12 * octave;

        Debug.LogFormat("NoteOff: {0}", noteNumber);

        if (noteDict.ContainsKey(noteNumber))
        {
            noteDict[noteNumber].NoteOff(noteNumber);
            noteDict.Remove(noteNumber);
        }
    }

    #endregion

}