using System.Collections;
using UnityEngine;

public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance { get; private set; }

    [Header("Ссылки")]
    [SerializeField] private PlayerItemManager playerItemManager;
    [SerializeField] private GameObject tradePanel;

    [Header("Объекты на руках (по индексу предмета)")]
    [SerializeField] private GameObject[] playerHandObjects;
    [SerializeField] private GameObject[] traderHandObjects;

    private void Awake()
    {
        Instance = this;

        if (playerItemManager == null)
            playerItemManager = FindFirstObjectByType<PlayerItemManager>();
    }

    private void SetHandObjects(int playerIndex, int traderIndex)
    {
        for (int i = 0; i < playerHandObjects.Length; i++)
            if (playerHandObjects[i] != null)
                playerHandObjects[i].SetActive(i == playerIndex);

        for (int i = 0; i < traderHandObjects.Length; i++)
            if (traderHandObjects[i] != null)
                traderHandObjects[i].SetActive(i == traderIndex);
    }

    private void DisableAllHandObjects()
    {
        foreach (var obj in playerHandObjects)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in traderHandObjects)
            if (obj != null) obj.SetActive(false);
    }

    public void PlayTrade()
    {
        Debug.Log("[TradeManager] PlayTrade вызван");

        if (playerItemManager == null)
        {
            Debug.LogError("[TradeManager] playerItemManager == null!");
            return;
        }

        if (tradePanel == null)
        {
            Debug.LogError("[TradeManager] tradePanel == null! Назначь объект в инспекторе.");
            return;
        }

        int currentIndex = playerItemManager.currentItemIndex;
        int nextIndex = currentIndex + 1;

        Debug.Log($"[TradeManager] currentIndex={currentIndex} nextIndex={nextIndex}");

        StartCoroutine(TradeRoutine(currentIndex, nextIndex));
    }

    private IEnumerator TradeRoutine(int playerIndex, int traderIndex)
    {
        Debug.Log("[TradeManager] TradeRoutine начался, включаю панель");

        tradePanel.SetActive(true);

        Debug.Log($"[TradeManager] tradePanel.activeSelf = {tradePanel.activeSelf}");

        SetHandObjects(playerIndex, traderIndex);

        var movers = tradePanel.GetComponentsInChildren<WorldChildMover>(true);
        Debug.Log($"[TradeManager] Найдено movers: {movers.Length}");

        foreach (var mover in movers)
            mover.StartMoving();

        float maxDuration = 0f;
        foreach (var mover in movers)
        {
            float dur = mover.GetDuration();
            if (dur > maxDuration) maxDuration = dur;
        }

        Debug.Log($"[TradeManager] Жду {maxDuration + 0.5f} сек");
        yield return new WaitForSeconds(maxDuration + 0.5f);

        foreach (var mover in movers)
            mover.ResetPosition();

        yield return new WaitForSeconds(maxDuration);

        DisableAllHandObjects();
        tradePanel.SetActive(false);

        playerItemManager.TradeTo(traderIndex);
        Debug.Log("[TradeManager] Трейд завершён");
    }
}