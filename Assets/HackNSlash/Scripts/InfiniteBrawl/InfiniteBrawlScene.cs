using UnityEngine;
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

    public string waveTamplate = "Wave {0} (Pw:{1})";
    public EnemyDef[] enemiesDefs;
    public Bounds bounds;
    public int initialEnemyPower = 1;
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

        Debug.LogFormat(waveTamplate, wave, currentEnemyPower);

        yield return Timing.WaitForSeconds(wait);

        wave++;
        
        currentEnemies = SpawnEnemyWave(currentEnemyPower);
        
        enemiesAlive = currentEnemies.Length;
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
        Debug.LogFormat("Enemies Left {0}", enemiesAlive);
        playerMain.GetComponent<PlayerConstitution>().OnDieEvent -= OnEnemyDied;
        if (enemiesAlive == 0)
        {
            Timing.RunCoroutine(nextWave());
        }
    }
}