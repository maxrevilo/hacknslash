using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;
using System;
using System.Collections;

class InfiniteBrawlScene : BattleGameScene
{
    [Serializable]
    public struct EnemyDef
    {
        public string name;
        public int power;
    }

    [SerializeField]
    public string waveTamplate = "Wave {0}";
    [SerializeField]
    public string powerTamplate = "Power {0}";
    [SerializeField]
    public Text enemiesLeftText;
    [SerializeField]
    public CanvasRenderer waveCanvas;
    [SerializeField]
    public Text waveNumber;
    [SerializeField]
    public Text wavePower;
    [SerializeField]
    public EnemyDef[] enemiesDefs;
    [SerializeField]
    public Bounds bounds;
    [SerializeField]
    public int initialEnemyPower = 1;
    [SerializeField]
    public GameObject enemiesContainer;

    private int wave;
    private int prevEnemyPower;
    private int currentEnemyPower;

    private GameObject[] currentEnemies;
    private int enemiesAlive;

    private int enemiesCreated = 0;

    protected override void Awake()
    {
        base.Awake();
        currentEnemies = new GameObject[0];
        Array.Sort<EnemyDef>(enemiesDefs, (a, b) => b.power - a.power);
        if (enemiesLeftText == null) throw new Exception("enemiesLeftText not set");
        if (waveCanvas == null) throw new Exception("waveCanvas not set");
        if (waveNumber == null) throw new Exception("waveNumber not set");
        if (wavePower == null) throw new Exception("wavePower not set");
    }


    protected override void Start()
    {
        base.Start();
        wave = 1;
        currentEnemyPower = initialEnemyPower;
        prevEnemyPower = 0;
        Timing.RunCoroutine(nextWave(3f));
    }

    protected override void Update()
    {
        base.Update();
    }

    protected IEnumerator<float> nextWave(float wait = 5f)
    {
        foreach (GameObject go in currentEnemies)
        {
            go.DestroyAPS();
        }

        int nextEnemyPower = currentEnemyPower + prevEnemyPower;
        prevEnemyPower = currentEnemyPower;
        currentEnemyPower = nextEnemyPower;

        waveCanvas.gameObject.SetActive(true);
        waveNumber.text = String.Format(waveTamplate, wave);
        wavePower.text = String.Format(powerTamplate, currentEnemyPower);

        Debug.LogFormat(waveTamplate, wave, currentEnemyPower);

        yield return Timing.WaitForSeconds(wait);

        waveCanvas.gameObject.SetActive(false);

        wave++;
        
        currentEnemies = SpawnEnemyWave(currentEnemyPower);
        
        enemiesAlive = currentEnemies.Length;
        enemiesLeftText.text = enemiesAlive.ToString();
    }

    protected GameObject[] SpawnEnemyWave(int power)
    {
        ArrayList enemies = new ArrayList(10);
        
        while(power > 0)
        {
            EnemyDef enemyDef = new EnemyDef();
            foreach(EnemyDef ed in enemiesDefs)
            {
                if(ed.power <= power)
                {
                    power -= ed.power;
                    enemyDef = ed;
                    break;
                }
            }
            if (enemyDef.name == null) break;

            Vector3 position = bounds.center + new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * bounds.extents.x,
                0,
                UnityEngine.Random.Range(-1f, 1f) * bounds.extents.z
            );
            Quaternion rotation = Quaternion.LookRotation(mainPlayer.transform.position - position, Vector3.up);

            GameObject enemy = PoolingSystem.Instance.InstantiateAPS(
                enemyDef.name,
                position,
                rotation,
                enemiesContainer != null? enemiesContainer: gameObject
            );
            if(enemy.name[0] != 'A') enemy.name = "A " + enemiesCreated++;
            //Debug.LogFormat("Spawn enemy {0} with power {1} ({2} left)", enemy.name, enemyDef.power, power);
            enemy.BroadcastMessage("ResetComponent", SendMessageOptions.DontRequireReceiver);

            PlayerConstitution enemyConstitution = enemy.GetComponent<PlayerConstitution>();
            enemyConstitution.OnDieEvent += OnEnemyDied;

            enemies.Add(enemy);
        }

        return (GameObject[]) enemies.ToArray(typeof(GameObject));
    }

    protected void OnEnemyDied(PlayerMain playerMain, float lastHit)
    {
        enemiesAlive--;
        enemiesLeftText.text = enemiesAlive.ToString();
        playerMain.GetComponent<PlayerConstitution>().OnDieEvent -= OnEnemyDied;
        if (enemiesAlive == 0)
        {
            Timing.RunCoroutine(nextWave());
        }
    }
}