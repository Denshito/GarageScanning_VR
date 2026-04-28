using UnityEngine;

public class BallGameController : MonoBehaviour
{
    // 小球预制体（你要自己拖进去）
    public GameObject ballPrefab;

    // 生成范围
    public float spawnRange = 5f;

    // 生成间隔
    public float spawnInterval = 1f;

    private float timer;
    private int totalScore;
    private float lastEliminateTime = -1f;

    [Header("Chatbot")]
    public llm chatbot;

    void Start()
    {
        // 一开始先生成几个
        SpawnRandomBall();
    }

    void Update()
    {
        // 定时生成小球
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
            SpawnRandomBall();
        }
    }

    // 随机生成一个彩色小球
    void SpawnRandomBall()
    {
        // 随机位置
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            0,
            Random.Range(-spawnRange, spawnRange)
        );

        // 实例化小球
        GameObject ball = Instantiate(ballPrefab, randomPos, Quaternion.identity);

        Ball ballScript = ball.GetComponent<Ball>();
        if (ballScript != null)
        {
            ballScript.gameController = this;
        }

        // 随机颜色
        Renderer renderer = ball.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(
                Random.value,
                Random.value,
                Random.value
            );
        }
    }

    // 加分方法
    public void AddScore(int value)
    {
        totalScore += value;
        float now = Time.time;
        float interval = lastEliminateTime < 0f ? now : now - lastEliminateTime;
        lastEliminateTime = now;

        Debug.Log("得分：" + totalScore + "（+" + value + "）");

        if (chatbot != null)
        {
            chatbot.OnBallEliminated(totalScore, interval);
        }
    }
}