using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatCtrl : MonoBehaviour
{
    [SerializeField] private CardMonoSystem cardSystem;
    [SerializeField] private UIMonoSystem uiSystem;
    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private MotionMonoSystem motionSystem;
    [SerializeField] private CameraMonoSystem cameraSystem;
    [SerializeField] private HpMonoSystem hpSystem;
    [SerializeField] private GoldMonoSystem goldSystem;

    [SerializeField] private PlayerView playerPrefab;
    [SerializeField] private EntityBuffViewPool entityBuffPool;
    [SerializeField] private DamageTextPool damageTextPool;

    [SerializeField] private RectTransform curtainRect;

    public CombatSystem CombatSystem { get; private set; }
    private PlayerView playerView;
    private void Awake()
    {
        if (!RunManager.Instance.isLoad)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.INIT);
        }

        CombatSystem = new CombatSystem();

        PlayerInstance playerInstance = CreatePlayerInstance(RunManager.Instance.CurrentData);
        playerView = GameObject.Instantiate<PlayerView>(playerPrefab);
        playerView.Init(
            eventBus: CombatSystem.EventBus,
            instance: playerInstance,
            position: new Vector3(-0.71f, 0f, -10f),
            buffViewPool: entityBuffPool,
            damageTextPool: damageTextPool
        );

        List<EnemyInstance> enemyInstances = CreateEnemyInstances();
        List<EnemyView> enemyViews = ConvertEnemyInstanceToView(enemyInstances, playerInstance);

        //
        DamageSystem damageSystem = new DamageSystem(CombatSystem.EventBus);
        BuffSystem buffSystem = new BuffSystem(CombatSystem.EventBus);
        PowerSystem powerSystem = new PowerSystem(CombatSystem.EventBus);
        RecordSystem recordSystem = new RecordSystem(CombatSystem.EventBus);
        EnergySystem energySystem = new EnergySystem(eventBus: CombatSystem.EventBus);

        motionSystem.Init(CombatSystem.EventBus);
        hpSystem.Init(CombatSystem.EventBus);
        goldSystem.Init(CombatSystem.EventBus);
        cameraSystem.Init(CombatSystem.EventBus);
        relicSystem.Init(CombatSystem.EventBus);
        uiSystem.Init(
            eventBus: CombatSystem.EventBus,
            actionSystem: CombatSystem.ActionSystem
        );
        cardSystem.Init(
            eventBus: CombatSystem.EventBus, 
            actionSystem: CombatSystem.ActionSystem,
            powerSystem: powerSystem
        );
        CombatSystem.Init(
            damageSystem: damageSystem, 
            buffSystem: buffSystem,
            energySystem: energySystem, 
            powerSystem: powerSystem,
            cardSystem: cardSystem, 
            uiSystem: uiSystem, 
            relicSystem: relicSystem,
            hpSystem: hpSystem,
            goldSystem: goldSystem,
            motionSystem: motionSystem,
            cameraSystem: cameraSystem,
            recordSystem: recordSystem,
            player: playerInstance,
            enemies: enemyInstances
        );

    }
    private void Start()
    {
        SoundKey bgm = RunManager.Instance.MapGraph.LatestNode.Type switch
        {
            MapNodeType.Combat => SoundKey.CombatBGM,
            MapNodeType.Elite => SoundKey.EliteBGM,
            MapNodeType.Boss => SoundKey.BossBGM,
            _ => SoundKey.NormalBGM
        };
        AudioManager.Instance.PlayBGM(bgm);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(playerView.Move(new Vector3(-0.71f, 0f, -0.71f)));
        sequence.AppendCallback(() => CombatSystem.CombatStart());
    }
    private void OnEnable()
    {
        CombatSystem.OnEnable();
    }
    private void OnDisable()
    {
        CombatSystem.OnDisable();
    }
    private void Update()
    {
        CombatSystem.UpdateTick();
    }
    private PlayerInstance CreatePlayerInstance(RunData run)
    {
        if (RunManager.Instance.Catalog.TryGetPlayerSO(run.PlayerId, out PlayerSO playerSo))
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
        List<EnemyInstance> enemies = new List<EnemyInstance>();

        EnemySpawnPlanSO plan = RunManager.Instance.GetEnemyPlan();

        foreach (EnemySO enemy in plan.Enemies)
        {
            enemies.Add(new EnemyInstance(
                data: enemy,
                maxHp: enemy.maxHpRange.Roll(),
                goldReward: enemy.goldReward.Roll()
            ));
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
                eventBus: CombatSystem.EventBus,
                position: positions[i],
                buffViewPool: entityBuffPool,
                damageTextPool: damageTextPool
            );

            enemies.Add(view);
        }

        return enemies;
    }
}