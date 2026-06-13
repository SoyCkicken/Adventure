using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using SRDebugger;

public class RemoteTester : MonoBehaviour
{
    [Header("Debug category buttons")]
    public Button mainStoryButton;
    public Button randomStoryButton;
    public Button battleButton;
    public Button weaponTestButton;
    public Button reMoveButton;
    public Button armorTestButton;
    public Button srDebuggerToggleButton;

    [Header("SRDebugger")]
    [SerializeField] private KeyCode srDebuggerToggleKey = KeyCode.F12;
    [SerializeField] private bool enableSRDebuggerHotkey = true;

    [Header("Option button prefab and target parent")]
    public GameObject buttonPrefab;
    public Transform rightPanelParent;

    [Header("Runtime managers")]
    public StoryDisplayManager storyDisplayManager;
    public EventDisplay eventDisplay;
    public GameFlowManager gameFlowManager;
    public InventoryManager inventoryManager;
    public JsonManager jsonManager;

    private List<string> mainStories = new List<string> { "MainScene_1_1", "MainScene_1_2", "MainScene_1_3", "MainScene_1_4", "MainScene_1_5", "MainScene_2_1" };
    private List<string> randomStories = new List<string> { "EventScene_1", "EventScene_2", "EventScene_3", "EventScene_4" };
    private List<string> enemyIDs = new List<string>();
    private List<string> WeaponID = new List<string>();
    private List<string> ArmorID = new List<string>();

    private void Start()
    {
        jsonManager = JsonManager.Instance ?? FindObjectOfType<JsonManager>(true);
        EnsureRuntimeReferences();

        if (jsonManager == null)
        {
            Debug.LogWarning("[RemoteTester] JsonManager is missing. Debug buttons were not initialized.");
            return;
        }

        foreach (var weapon in jsonManager.GetWeaponMasters("Weapon_Master"))
            WeaponID.Add(weapon.Weapon_ID);

        foreach (var armor in jsonManager.GetArmorMasters("Armor_Master"))
            ArmorID.Add(armor.Armor_ID);

        foreach (var monster in jsonManager.GetMonMasters("Mon_Master"))
            enemyIDs.Add(monster.Mon_ID);

        mainStoryButton?.onClick.AddListener(() => ShowOptions(mainStories, OnMainStorySelected));
        randomStoryButton?.onClick.AddListener(() => ShowOptions(randomStories, OnRandomStorySelected));
        battleButton?.onClick.AddListener(() => ShowOptions(enemyIDs, OnBattleSelected));
        weaponTestButton?.onClick.AddListener(() => ShowOptions(WeaponID, WeaponAddInventory));
        armorTestButton?.onClick.AddListener(() => ShowOptions(ArmorID, ArmorAddInventory));
        reMoveButton?.onClick.AddListener(RemoveAllInventory);
        srDebuggerToggleButton?.onClick.AddListener(ToggleSRDebuggerPanel);
    }

    private void Update()
    {
        if (enableSRDebuggerHotkey && Input.GetKeyDown(srDebuggerToggleKey))
            ToggleSRDebuggerPanel();
    }

    public void ToggleSRDebuggerPanel()
    {
        try
        {
            if (!SRDebug.IsInitialized)
            {
                SRDebug.Init();
                SRDebug.Instance.ShowDebugPanel(false);
                Debug.Log("[RemoteTester] SRDebugger initialized and opened.");
                return;
            }

            if (SRDebug.Instance.IsDebugPanelVisible)
            {
                SRDebug.Instance.HideDebugPanel();
                Debug.Log("[RemoteTester] SRDebugger hidden.");
            }
            else
            {
                SRDebug.Instance.ShowDebugPanel(false);
                Debug.Log("[RemoteTester] SRDebugger opened.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[RemoteTester] SRDebugger toggle failed: {ex.Message}");
        }
    }

    private void ShowOptions(List<string> options, System.Action<string> onClickAction)
    {
        if (rightPanelParent == null || buttonPrefab == null)
        {
            Debug.LogWarning("[RemoteTester] Option panel references are missing.");
            return;
        }

        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("[RemoteTester] No debug options to show.");
            return;
        }

        foreach (Transform child in rightPanelParent)
            Destroy(child.gameObject);

        foreach (var option in options)
        {
            GameObject btnObj = Instantiate(buttonPrefab, rightPanelParent);
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = option;

            Button button = btnObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => onClickAction(option));
        }
    }

    private void EnsureRuntimeReferences()
    {
        storyDisplayManager = storyDisplayManager ?? FindObjectOfType<StoryDisplayManager>(true);
        eventDisplay = eventDisplay ?? FindObjectOfType<EventDisplay>(true);
        gameFlowManager = gameFlowManager ?? FindObjectOfType<GameFlowManager>(true);
        inventoryManager = inventoryManager ?? FindObjectOfType<InventoryManager>(true);
    }

    private void ClearRuntimeSequences()
    {
        EnsureRuntimeReferences();
        storyDisplayManager?.StopMainStory();
        eventDisplay?.StopRandomEvent();
        storyDisplayManager?.storyList?.Clear();
        eventDisplay?.groupEvents?.Clear();
    }

    private void OnMainStorySelected(string groupID)
    {
        string[] parts = groupID.Replace("MainScene_", "").Split('_');

        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int chapter) ||
            !int.TryParse(parts[1], out int eventIndex))
        {
            Debug.LogWarning($"[RemoteTester] Invalid main story id: {groupID}");
            return;
        }

        Debug.Log($"[RemoteTester] Load main story: chapter={chapter}, event={eventIndex}");
        ClearRuntimeSequences();

        var manager = storyDisplayManager ?? FindObjectOfType<StoryDisplayManager>(true);
        if (manager == null)
        {
            Debug.LogWarning("[RemoteTester] StoryDisplayManager is missing.");
            return;
        }

        manager.LoadMainStory(chapter, eventIndex);
    }

    private void OnRandomStorySelected(string groupID)
    {
        if (!int.TryParse(groupID.Replace("EventScene_", ""), out int id))
        {
            Debug.LogWarning($"[RemoteTester] Invalid random event id: {groupID}");
            return;
        }

        Debug.Log($"[RemoteTester] Load random event: id={id}");
        ClearRuntimeSequences();

        var manager = eventDisplay ?? FindObjectOfType<EventDisplay>(true);
        if (manager == null)
        {
            Debug.LogWarning("[RemoteTester] EventDisplay is missing.");
            return;
        }

        manager.LoadEventStory(id);
    }

    private void OnBattleSelected(string enemyID)
    {
        Debug.Log($"[RemoteTester] Force battle: {enemyID}");
        ClearRuntimeSequences();

        var manager = gameFlowManager ?? FindObjectOfType<GameFlowManager>(true);
        if (manager == null)
        {
            Debug.LogWarning("[RemoteTester] GameFlowManager is missing.");
            return;
        }

        manager.ForceBattleWithMonster(enemyID);
    }

    private void WeaponAddInventory(string weaponID)
    {
        Debug.Log($"[RemoteTester] Add weapon to inventory: {weaponID}");
        AddInventoryItem(weaponID);
    }

    private void ArmorAddInventory(string armorID)
    {
        Debug.Log($"[RemoteTester] Add armor to inventory: {armorID}");
        AddInventoryItem(armorID);
    }

    private void AddInventoryItem(string itemID)
    {
        var itemData = ItemDataFactory.FromCode(jsonManager, itemID);
        if (itemData == null)
        {
            Debug.LogError($"[RemoteTester] Could not create ItemData for {itemID}.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("[RemoteTester] InventoryManager is missing.");
            return;
        }

        inventoryManager.AddItemToInventory(itemData);
    }

    private void RemoveAllInventory()
    {
        Debug.Log("[RemoteTester] Remove-all inventory action is not implemented yet.");
        // inventoryManager.ClearAllItems();
    }
}
#endif