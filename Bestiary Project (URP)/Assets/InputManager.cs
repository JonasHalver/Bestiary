using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static event System.Action OpenPauseMenu;
    public static event System.Action OpenJournal;
    public static event System.Action OpenGlossary;
    public static event System.Action Pause;
    public static event System.Action Commit;
    public static event System.Action Escape;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetButtonDown("OpenPauseMenu")) OpenPauseMenu.Invoke();
        if (Input.GetButtonDown("OpenJournal")) OpenJournal.Invoke();
        if (Input.GetButtonDown("OpenGlossary")) OpenGlossary.Invoke();
        if (Input.GetButtonDown("Pause")) Pause.Invoke();
        if (Input.GetButtonDown("Commit")) Commit.Invoke();
        if (Input.GetButtonDown("Escape")) Escape.Invoke();
    }
}
