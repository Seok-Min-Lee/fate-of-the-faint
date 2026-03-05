using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CardMonoSystem cardSystem;
    [SerializeField] private UIMonoSystem uiSystem;
    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private MotionMonoSystem animationSystem;
    [SerializeField] private CameraMonoSystem cameraSystem;
    [SerializeField] private GoldMonoSystem goldSystem;

    [SerializeField] private PlayerView playerPrefab;
    [SerializeField] private EntityBuffViewPool entityBuffPool;
    [SerializeField] private DamageTextPool damageTextPool;

    [Header("Instance")]
    [SerializeField] private EnemySpawnPlan[] normalPlans;
    [SerializeField] private EnemySpawnPlan[] elitePlans;
    public CombatSystem CombatSystem { get; private set; }
    private void Awake()
    {
        if (!PlayManager.Instance.isLoad)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.INIT);
        }

        CombatSystem = new CombatSystem();

        PlayerInstance playerInstance = CreatePlayerInstance(PlayManager.Instance.CurrentData);
        PlayerView playerView = GameObject.Instantiate<PlayerView>(playerPrefab);
        playerView.Init(
            instance: playerInstance,
            combatManager: this,
            position: new Vector3(-0.71f, 0f, -0.71f),
            buffViewPool: entityBuffPool,
            damageTextPool: damageTextPool
        );

        List<EnemyInstance> enemyInstances = CreateEnemyInstances();
        List<EnemyView> enemyViews = ConvertEnemyInstanceToView(enemyInstances, playerInstance);

        List<CardInstance> cardInstance = CreateCardInstances(PlayManager.Instance.CurrentData.Cards.Select(x => x.Origin));

        //
        DamageSystem damageSystem = new DamageSystem(CombatSystem.EventBus);
        BuffSystem buffSystem = new BuffSystem(CombatSystem.EventBus);
        EnergySystem energySystem = new EnergySystem(
            eventBus: CombatSystem.EventBus, 
            max: playerInstance.Energy
        );
        PowerSystem powerSystem = new PowerSystem(CombatSystem.EventBus);
        animationSystem.Init(CombatSystem.EventBus);
        goldSystem.Init(CombatSystem.EventBus);
        cameraSystem.Init(CombatSystem.EventBus);
        uiSystem.Init(
            eventBus: CombatSystem.EventBus,
            actionSystem: CombatSystem.ActionSystem
        );
        relicSystem.Init(
            eventBus: CombatSystem.EventBus,
            onClick: (relic) => uiSystem.OnClickRelic(relic)
        );
        cardSystem.Init(
            eventBus: CombatSystem.EventBus, 
            actionSystem: CombatSystem.ActionSystem,
            powerSystem: powerSystem,
            cardInstances: cardInstance
        );
        CombatSystem.Init(
            damageSystem: damageSystem, 
            buffSystem: buffSystem,
            energySystem: energySystem, 
            powerSystem: powerSystem,
            cardSystem: cardSystem, 
            uiSystem: uiSystem, 
            relicSystem: relicSystem,
            goldSystem: goldSystem,
            animationSystem: animationSystem,
            cameraSystem: cameraSystem,
            player: playerInstance,
            enemies: enemyInstances
        );
    }
    private void OnEnable()
    {
        CombatSystem.OnEnable();
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        CombatSystem.CombatStart();
    }
    private void OnDisable()
    {
        CombatSystem.OnDisable();
    }
    private void Update()
    {
        CombatSystem.UpdateTick();
    }
    private PlayerInstance CreatePlayerInstance(PlayData run)
    {
        if (PlayManager.Instance.Catalog.TryGetPlayerSO(run.PlayerId, out PlayerSO playerSo))
        {
            return new PlayerInstance(
                data: playerSo,
                maxHp: run.MaxHp,
                currentHp: run.CurrentHp
            );
        }
        
        return null;
    }
    private List<EnemyInstance> CreateEnemyInstances()
    {
        EnemySpawnPlan team = normalPlans[UnityEngine.Random.Range(0, normalPlans.Length)];

        List<EnemyInstance> enemies = new List<EnemyInstance>();

        for (int i = 0; i < team.details.Length; i++)
        {
            EnemySpawnPlanDetail detail = team.details[i];

            for (int j = 0; j < detail.count; j++)
            {
                enemies.Add(new EnemyInstance(
                    data: detail.origin,
                    maxHp: detail.origin.maxHpRange.Roll(),
                    goldReward: detail.origin.goldReward.Roll()
                ));
            }
        }

        return enemies;
    }
    private List<EnemyView> ConvertEnemyInstanceToView(IEnumerable<EnemyInstance> instances, EntityInstance target)
    {
        int instanceCount = instances.Count();

        List<Vector3> positions = new List<Vector3>();
        switch (instanceCount)
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

        List<EnemyView> enemies = new List<EnemyView>();

        for (int i = 0; i < instanceCount; i++)
        {
            EnemyInstance _instance = instances.ElementAt(i);
            GameObject go = GameObject.Instantiate(_instance.Data.prefab);

            EnemyView view = go.GetComponent<EnemyView>();
            view.Init(
                instance: _instance,
                combatManager: this,
                position: positions[i],
                buffViewPool: entityBuffPool,
                damageTextPool: damageTextPool
            );

            enemies.Add(view);
        }

        return enemies;
    }
    private List<CardInstance> CreateCardInstances(IEnumerable<CardSO> samples)
    {
        List<CardInstance> instances = new List<CardInstance>();

        foreach (CardSO sample in samples)
        {
            instances.Add(new CardInstance(
                instanceId: sample.Id,
                origin: sample
            ));
        }

        return instances;
    }
    private void Save()
    {
        //CombatResult result = new CombatResult();

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
