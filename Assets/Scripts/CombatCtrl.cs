using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine;

public class CombatCtrl : MonoBehaviour
{
    [SerializeField] private CombatManager combatSystem;
    [SerializeField] private Player player;

    [SerializeField] private EnemySpawnPlan[] enemyTeams;
    
    private void Start()
    {
        combatSystem.CombatStart();

        EnemySpawnPlan team = enemyTeams[UnityEngine.Random.Range(0, enemyTeams.Length)];

        List<Vector3> positions = new List<Vector3>();
        switch (team.Count)
        {
            case 1:
                positions.Add(new Vector3(0, 0, 6.43f));
                break;
            case 2:
                positions.Add(new Vector3(-1.75f, 0, 6.43f));
                positions.Add(new Vector3(1.75f, 0, 6.43f));
                break;
            case 3:
                positions.Add(new Vector3(-2.5f, 0, 6.43f));
                positions.Add(new Vector3(0, 0, 6.43f));
                positions.Add(new Vector3(2.5f, 0, 6.43f));
                break;
        }

        int num = 0;
        int enemyId = 0;
        for (int i = 0; i < team.details.Length; i++)
        {
            EnemySpawnPlanDetail detail = team.details[i];

            for (int j = 0; j < detail.count; j++)
            {
                EnemyInstance enemyInstance = new EnemyInstance(
                    id: enemyId++, 
                    data: detail.origin, 
                    maxHp: detail.origin.maxHpRange.max
                );

                GameObject go = GameObject.Instantiate(detail.origin.prefab);

                EnemyView view = go.GetComponent<EnemyView>();
                view.Init(
                    instance: enemyInstance, 
                    player: player,
                    position: positions[num++],
                    combat: combatSystem
                );
            }
        }

    }
}
[Serializable]
public struct EnemySpawnPlan
{
    public EnemySpawnPlanDetail[] details;
    public int Count => details.Sum(x => x.count);
}
[Serializable]
public struct EnemySpawnPlanDetail
{
    public EnemySO origin;
    public int count;
}
