using UnityEngine;

public class PlayerItemManager : MonoBehaviour
{
    public static PlayerItemManager Instance { get; private set; }

    [System.Serializable]
    public class ItemEntry
    {
        public ItemData data;
        public GameObject sceneObject;
    }

    [Header("Предметы")]
    [SerializeField] private ItemEntry[] items;
    public int currentItemIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshSceneItems();
    }

    public void RefreshSceneItems()
    {
        for (int i = 0; i < items.Length; i++)
            if (items[i].sceneObject != null)
                items[i].sceneObject.SetActive(i == currentItemIndex);
    }

    public ItemData GetCurrentItem()
    {
        if (currentItemIndex < 0 || currentItemIndex >= items.Length) return null;
        return items[currentItemIndex].data;
    }

    public void TradeTo(int newItemIndex)
    {
        if (newItemIndex < 0 || newItemIndex >= items.Length) return;
        currentItemIndex = newItemIndex;
        RefreshSceneItems();
    }
}