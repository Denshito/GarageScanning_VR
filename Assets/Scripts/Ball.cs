using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Ball : MonoBehaviour
{
    [SerializeField] XRSimpleInteractable interactable;
    [SerializeField] string fixedText = "交互成功：固定文本";
    [Header("Chatbot")]
    [SerializeField] llm chatbot;
    public BallGameController gameController;

    private static int totalEliminations;
    private static float lastEliminateTime = -1f;

    void Reset()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (chatbot == null)
        {
            chatbot = FindFirstObjectByType<llm>();
        }
    }

    void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        if (chatbot == null)
        {
            chatbot = FindFirstObjectByType<llm>();
        }
    }

    void OnEnable()
    {
        if (interactable == null) return;
        interactable.selectEntered.AddListener(OnSelectEntered); // 按下抓取/选择时触发
        // 如果你想“射线指到就打印”，改用 hoverEntered
        // interactable.hoverEntered.AddListener(OnHoverEntered);
    }

    void OnDisable()
    {
        if (interactable == null) return;
        interactable.selectEntered.RemoveListener(OnSelectEntered);
        // interactable.hoverEntered.RemoveListener(OnHoverEntered);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log(fixedText, this);

        if (chatbot != null)
        {
            totalEliminations += 1;
            float now = Time.time;
            float interval = lastEliminateTime < 0f ? now : now - lastEliminateTime;
            lastEliminateTime = now;
            chatbot.OnBallEliminated(totalEliminations, interval);
        }
        else
        {
            Debug.LogWarning("[Chatbot] PrintOnRayInteract 未绑定 llm 组件。", this);
        }

        Destroy(gameObject);
    }

    // void OnHoverEntered(HoverEnterEventArgs args)
    // {
    //     Debug.Log(fixedText, this);
    // }
}