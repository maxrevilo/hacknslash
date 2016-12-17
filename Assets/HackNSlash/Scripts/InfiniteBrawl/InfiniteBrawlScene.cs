using System;
using UnityEngine;
using System.Collections.Generic;
using MovementEffects;

class InfiniteBrawlScene : BattleGameScene
{

    public string waveTamplate = "Wave {0}";
    public string enemyNameOnPool = "Enemy Hammer";
    public Bounds bounds;
    public int initialEnemyPower = 1;
    public GameObject enemiesContainer;

    private int wave;
    private int prevEnemyPower;
    private int currentEnemyPower;

    private GameObject[] currentEnemies;
    private int enemiesAlive;

    bool firstUpdate = true;

    protected override void Awake()
    {
        base.Awake();
        currentEnemies = new GameObject[0];
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

        Debug.LogFormat(waveTamplate, wave);

        yield return Timing.WaitForSeconds(wait);

        wave++;
        int nextEnemyPower = currentEnemyPower + prevEnemyPower;
        prevEnemyPower = currentEnemyPower;
        currentEnemyPower = nextEnemyPower;
        currentEnemies = SpawnEnemyWave(currentEnemyPower);
        enemiesAlive = currentEnemies.Length;
    }

    protected GameObject[] SpawnEnemyWave(int count)
    {
        GameObject[] result = new GameObject[count];
        UnityEngine.Random rnd = new UnityEngine.Random();

        for(int i = 0; i < count; i++)
        {
            Vector3 position = bounds.center + new Vector3(UnityEngine.Random.Range(-1f, 1f) * bounds.extents.x, 0, UnityEngine.Random.Range(-1f, 1f) * bounds.extents.z);

            Quaternion rotation = Quaternion.LookRotation(mainPlayer.transform.position - position, Vector3.up);

            /*Debug.LogFormat("{0} on {1} facing {2} under {3}",
                enemyNameOnPool,
                position,
                rotation,
                enemiesContainer != null ? enemiesContainer : gameObject
            );*/

            GameObject enemy = PoolingSystem.Instance.InstantiateAPS(
                enemyNameOnPool,
                position,
                rotation,
                enemiesContainer != null? enemiesContainer: gameObject
            );
            if(enemy.name[0] != 'A') enemy.name = "A " + i;
           // Timing.RunCoroutine(ResetEnemy(enemy), Segment.FixedUpdat
            enemy.BroadcastMessage("ResetComponent", SendMessageOptions.DontRequireReceiver);

            PlayerConstitution enemyConstitution = enemy.GetComponent<PlayerConstitution>();
            enemyConstitution.OnDieEvent += OnEnemyDied;

            result[i] = enemy;
        }

        return result;
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