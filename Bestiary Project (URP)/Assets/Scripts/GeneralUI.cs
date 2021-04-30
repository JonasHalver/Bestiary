using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralUI : MonoBehaviour
{
    public void Glossary()
    {
        GameManager.ChangeState(GameManager.GameState.Glossary);
    }
    public void PauseMenu()
    {
        GameManager.ChangeState(GameManager.GameState.PauseMenu);
    }
    public void Journal()
    {
        GameManager.ChangeState(GameManager.GameState.Bestiary);
    }
}
