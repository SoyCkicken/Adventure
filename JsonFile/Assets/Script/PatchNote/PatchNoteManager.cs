using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using static SaveManager;

public class PatchNoteViewer : MonoBehaviour
{
    public GameObject patchNotePannelObject; // 패치 노트 패널 오브젝트
    public GameObject textPrefab; // TMP_Text 프리팹
    public Transform contentParent; // ScrollView Content 등 부모 오브젝트
    public TextAsset jsonFile; // Resources에 넣고 연결
    public Button close_Button; // 닫기 버튼
    public SaveManager saveManager; // SaveManager 인스턴스
    public PlayerPrefs playerPrefs;

    public void Open(bool forceShow)
    {
        patchNotePannelObject.SetActive(true);
        this.forceShow = forceShow;
    }
    private bool forceShow = false;

    private void Start()
    {
        var saveData = (saveManager != null) ? saveManager.GetOrCreateSave()
                                             : new SaveManager.SaveData();


        //Debug.Log(PlayerPrefs.GetInt("ShowPatchNoteToggle"));
        //if (PlayerPrefs.GetInt("ShowPatchNoteToggle") == 1)
        //{
        //    if (saveData != null)
        //    {
        //        saveData.showPatchNoteToggle = true; // PlayerPrefs에서 가져온 값이 1이면 토글 활성화
        //    }
        //    Debug.Log("플레이어 토글의 값은 1입니다");
        //}
        //else
        //{
        //    if (saveData != null)
        //    {
        //        saveData.showPatchNoteToggle = false; // PlayerPrefs에서 가져온 값이 0이면 토글 비활성화   
        //        Debug.Log("플레이어 토글의 값은 0입니다");
        //    }
        //    else
        //    {
        //        Debug.Log("세이브 데이터가 없습니다");
        //    }

        //}

        //// 토글이 false면(표시하지 않기로 설정) 패널 비활성화 후 종료
        //if (saveData != null && saveData.showPatchNoteToggle)
        //{

        //    Debug.Log(saveData.showPatchNoteToggle);
        //    if (saveData.lastSeenVersion != Application.version)
        //    {
        //        // 버전이 다르면 무조건 표시
        //        forceShow = true;
        //        Debug.Log("버전이 달라서 여기에 들어와졌습니다");
        //    }
        //   else if(saveData.lastSeenVersion == Application.version && saveData.showPatchNoteToggle)
        //    {
        //        // 버전이 같고 토글이 켜져있으면 패널 비활성화
        //        Debug.Log("버전이 같지만 토글이 활성화 되어 있어서 여기 들어와졌습니다");
        //        //forceShow = true; // 강제로 표시하도록 설정
        //        patchNotePannelObject.SetActive(false);
        //        return;
        //    }
        //    else
        //    {
        //        Debug.Log("버전도 같지만 토글이 비활성화 되어 있거나 예외적인 상황입니다");
        //        // 버전이 같고 토글이 켜져있으면 표시
        //        //forceShow = saveData.showPatchNoteToggle;
        //        patchNotePannelObject.SetActive(true);

        //        patchNoteRead();

        //        close_Button.onClick.AddListener(() =>
        //        {
        //            var data = saveManager.WriteLoadFile();
        //            if (data == null) data = new SaveManager.SaveData();

        //            if (forceShow)
        //            {
        //                // 버전 다를 때는 무조건 새 버전 기록
        //                data.lastSeenVersion = Application.version;
        //            }
        //            else
        //            {
        //                // 버전 같을 때는 토글 상태만 저장
        //                data.showPatchNoteToggle = saveManager.showPatchNoteToggle.isOn;
        //            }
        //            PlayerPrefs.SetInt("ShowPatchNoteToggle", data.showPatchNoteToggle ? 1 : 0);
        //            if (PlayerPrefs.GetInt("ShowPatchNoteToggle") == 1)
        //            {
        //                saveData.showPatchNoteToggle = true; // PlayerPrefs에서 가져온 값이 1이면 토글 활성화
        //                Debug.Log("플레이어 토글의 값은 1입니다");
        //            }
        //            else
        //            {
        //                saveData.showPatchNoteToggle = false; // PlayerPrefs에서 가져온 값이 0이면 토글 비활성화   
        //                Debug.Log("플레이어 토글의 값은 0입니다");
        //            }
        //            saveManager.WriteSaveFile(data);
        //            patchNotePannelObject.SetActive(false);
        //        });
        //    }
        //}
        //else
        //{
        //    patchNotePannelObject.SetActive(true);
        //    //Debug.Log(saveData.showPatchNoteToggle);
        //}
        //close_Button.onClick.AddListener(() =>
        //{
        //    var data = saveManager.WriteLoadFile();
        //    if (data == null) data = new SaveManager.SaveData();

        //    if (forceShow)
        //    {
        //        // 버전 다를 때는 무조건 새 버전 기록
        //        data.lastSeenVersion = Application.version;
        //    }
        //    else
        //    {
        //        // 버전 같을 때는 토글 상태만 저장
        //        data.showPatchNoteToggle = saveManager.showPatchNoteToggle.isOn;
        //    }
        //    PlayerPrefs.SetInt("ShowPatchNoteToggle", data.showPatchNoteToggle ? 1 : 0);
        //    if (PlayerPrefs.GetInt("ShowPatchNoteToggle") == 1)
        //    {
        //        saveData.showPatchNoteToggle = true; // PlayerPrefs에서 가져온 값이 1이면 토글 활성화
        //        Debug.Log("플레이어 토글의 값은 1입니다");
        //    }
        //    else
        //    {
        //        //saveData.showPatchNoteToggle = false; // PlayerPrefs에서 가져온 값이 0이면 토글 비활성화   
        //        Debug.Log("플레이어 토글의 값은 0입니다");
        //    }
        //    saveManager.WriteSaveFile(data);
        //    patchNotePannelObject.SetActive(false);
        //});

        //patchNoteRead();

        // 2) PlayerPrefs → 세이브 반영(기본값 1로 가정: 첫 실행 때 표시)
        bool togglePref = PlayerPrefs.GetInt("ShowPatchNoteToggle", 1) == 1;
        saveData.showPatchNoteToggle = togglePref;

        bool versionChanged = saveData.lastSeenVersion != Application.version;
        bool shouldShow = forceShow || versionChanged || saveData.showPatchNoteToggle;

        patchNotePannelObject.SetActive(shouldShow);

        if (!shouldShow) return;

        // 3) 컨텐츠 채우기
        patchNoteRead();

        // 4) 닫기 버튼 리스너는 단 한 번만
        close_Button.onClick.RemoveAllListeners();
        close_Button.onClick.AddListener(() =>
        {
            var data = saveManager.GetOrCreateSave();

            // 보였다가 닫는 시점엔 “이번 버전은 읽었다”로 기록
            data.lastSeenVersion = Application.version;

            // UI 토글 상태를 저장(있으면)
            if (saveManager.showPatchNoteToggle != null)
                data.showPatchNoteToggle = saveManager.showPatchNoteToggle.isOn;

            PlayerPrefs.SetInt("ShowPatchNoteToggle", data.showPatchNoteToggle ? 1 : 0);

            saveManager.WriteSaveFile(data);
            patchNotePannelObject.SetActive(false);
        });

    }

    void patchNoteRead()
    {
        var data = JsonUtility.FromJson<Patch_Note_List>(jsonFile.text);

        // 버전 내림차순 정렬
        data.Patch_Notes.Sort((a, b) =>
            new System.Version(b.version).CompareTo(new System.Version(a.version)));

        foreach (var note in data.Patch_Notes)
        {
            var headerObj = Instantiate(textPrefab, contentParent);
            headerObj.GetComponent<TMP_Text>().text = $"<b>{note.version} ({note.Date})</b>";

            foreach (var line in note.Lines)
            {
                var lineObj = Instantiate(textPrefab, contentParent);
                lineObj.GetComponent<TMP_Text>().text = $" - {line}";
            }

            var spaceObj = Instantiate(textPrefab, contentParent);
            spaceObj.GetComponent<TMP_Text>().text = "";
        }
    }
}
