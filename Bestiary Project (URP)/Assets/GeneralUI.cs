using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralUI : MonoBehaviour
{
    public void Glossary()
    {
        GameManager.instance.OpenGlossary();
    }
    public void PauseMenu()
    {
        GameManager.instance.Pause();
    }
    public void Journal()
    {
        GameManager.instance.OpenJournal();
    }
}
