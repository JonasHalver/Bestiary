using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarSpawner : MonoBehaviour
{ 
    public GameObject healthBar;
    public Transform ally, enemy;
    public Animator allyBG, enemyBG;

    private void Start()
    {
        CombatManager.CharactersSpawned += SpawnHealthBars;
        TutorialManager.ShowAlly += MoveAllyBars;
        TutorialManager.ShowEnemy += MoveEnemyBars;
    }   
    

    private void OnDisable()
    {
        CombatManager.CharactersSpawned -= SpawnHealthBars;
        TutorialManager.ShowAlly -= MoveAllyBars;
        TutorialManager.ShowEnemy -= MoveEnemyBars;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnHealthBars(bool isAlly)
    {
        //if (timing != CombatManager.CombatTiming.CombatBegins) return;
        for (int i = 0; i < CombatManager.actors.Count; i++)
        {
            Character character = CombatManager.actors[i];
            if (character.stats.characterType == CharacterStats.CharacterTypes.Adventurer && isAlly)
            {
                GameObject newHealthBar = Instantiate(healthBar, ally);
                newHealthBar.GetComponent<HealthBar>().character = character;
            }
            else if (character.stats.characterType == CharacterStats.CharacterTypes.NPC && !isAlly)
            {
                GameObject newHealthBar = Instantiate(healthBar, enemy);
                newHealthBar.GetComponent<HealthBar>().character = character;
            }
        }
    }

    private void MoveAllyBars()
    {
        allyBG.SetTrigger("MoveIn");
    }

    private void MoveEnemyBars()
    {
         enemyBG.SetTrigger("MoveIn");
    }
}
