using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarSpawner : MonoBehaviour
{ 
    public GameObject healthBar;
    public Transform ally, enemy;

    private void Start()
    {
        CombatManager.RoundPhases += SpawnHealthBars;
    }

    private void OnDisable()
    {
        CombatManager.RoundPhases -= SpawnHealthBars;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnHealthBars(CombatManager.CombatTiming timing)
    {
        if (timing != CombatManager.CombatTiming.CombatBegins) return;
        for (int i = 0; i < CombatManager.actors.Count; i++)
        {
            Character character = CombatManager.actors[i];
            if (character.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
            {
                GameObject newHealthBar = Instantiate(healthBar, ally);
                newHealthBar.GetComponent<HealthBar>().character = character;
            }
            else
            {
                GameObject newHealthBar = Instantiate(healthBar, enemy);
                newHealthBar.GetComponent<HealthBar>().character = character;
            }
        }
    }
}
