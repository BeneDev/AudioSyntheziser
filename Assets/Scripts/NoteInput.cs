using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteInput : MonoBehaviour {

    #region Events

    public delegate void NoteOnDelegate(int noteNumber, float velocity);
    public event NoteOnDelegate OnNoteOn;

    public event System.Action<int> OnNoteOff;

    #endregion

    #region Private Fields

    Dictionary<KeyCode, int> virtualKeysDict;

    [SerializeField] int noteStart = 50;

    [SerializeField] int step = 1;

    [SerializeField] float deltaTime = 0.5f;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        virtualKeysDict = new Dictionary<KeyCode, int>();
        virtualKeysDict.Add(KeyCode.A, 51); // C
        virtualKeysDict.Add(KeyCode.W, 52); // C#
        virtualKeysDict.Add(KeyCode.S, 53); // D
        virtualKeysDict.Add(KeyCode.E, 54); // D#
        virtualKeysDict.Add(KeyCode.D, 55); // E
        virtualKeysDict.Add(KeyCode.F, 56); // F
        virtualKeysDict.Add(KeyCode.T, 57); // F#
        virtualKeysDict.Add(KeyCode.G, 58); // G
        virtualKeysDict.Add(KeyCode.Z, 59); // G#
        virtualKeysDict.Add(KeyCode.H, 60); // A
        virtualKeysDict.Add(KeyCode.U, 61); // A#
        virtualKeysDict.Add(KeyCode.J, 62); // B
        virtualKeysDict.Add(KeyCode.K, 63); // C
        virtualKeysDict.Add(KeyCode.I, 64); // C#
        virtualKeysDict.Add(KeyCode.L, 65); // D
        virtualKeysDict.Add(KeyCode.O, 66); // D#
    }

    private void Start()
    {
        //StartCoroutine(Arpeggiator());
    }

    private void Update()
    {
        foreach(KeyCode key in virtualKeysDict.Keys)
        {
            if(Input.GetKeyDown(key))
            {
                if(OnNoteOn != null)
                {
                    OnNoteOn(virtualKeysDict[key], 1f);
                }
            }
            
            if(Input.GetKeyUp(key))
            {
                if(OnNoteOff != null)
                {
                    OnNoteOff(virtualKeysDict[key]);
                }
            }
        }
    }

    #endregion

    // Simple Sequencer
    IEnumerator Arpeggiator()
    {
        int noteNumber = noteStart;

        while(true)
        {
            if (OnNoteOff != null)
            {
                OnNoteOff(noteNumber);
            }

            if (OnNoteOn != null)
            {
                OnNoteOn(noteNumber += step, 1f);
            }

            yield return new WaitForSeconds(deltaTime);
        }
    }

}
