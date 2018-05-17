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

}
