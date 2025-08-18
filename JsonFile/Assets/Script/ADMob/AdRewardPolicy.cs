using System;
using UnityEngine;

/// 보상형 광고 노출 정책: 일일/세션/한판 캡 + 쿨타임
/// - PlayerPrefs로 일일/마지막 노출 시각을 영속 저장
/// - 세션/한판 카운트는 메모리 기반(앱 재시작 시 초기화)
public sealed class AdRewardPolicy : MonoBehaviour
{
    public static AdRewardPolicy Instance { get; private set; }

    [Header("Caps")]
    [Tooltip("하루 최대 보상형 광고 시청 허용 개수")]
    public int dailyCap = 12;
    [Tooltip("앱 실행(세션)당 최대 허용 개수")]
    public int sessionCap = 5;
    [Tooltip("한 판(런) 당 최대 허용 개수")]
    public int runCap = 3;

    [Header("Pacing")]
    [Tooltip("광고 간 최소 간격(초)")]
    public int cooldownSeconds = 120;

    [Header("Onboarding")]
    [Tooltip("앱 실행 후 최소 n초 지나야 첫 광고 허용(초)")]
    public int minSecondsAfterAppOpen = 60;

    int sessionCount;
    int runCount;
    DateTime appOpenUtc;

    // PlayerPrefs keys
    const string KEY_LAST_SHOWN_UNIX = "ad.reward.lastShownUnix";
    const string KEY_DAILY_DATE = "ad.reward.dailyDate";
    const string KEY_DAILY_COUNT = "ad.reward.dailyCount";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        appOpenUtc = DateTime.UtcNow;
        RollDailyIfNeeded();
    }

    // ───────── public API ─────────
    public void ResetRun() => runCount = 0;        // 한 판 시작 시 호출
    public void ResetSession() => sessionCount = 0; // 원하면 세션 리셋

    public bool CanShow(out string reason, out TimeSpan wait)
    {
        RollDailyIfNeeded();

        // 1) 온보딩 보호
        var sinceOpen = DateTime.UtcNow - appOpenUtc;
        if (sinceOpen.TotalSeconds < minSecondsAfterAppOpen)
        {
            reason = $"게임 시작 {minSecondsAfterAppOpen - (int)sinceOpen.TotalSeconds}초 후에 이용 가능합니다.";
            wait = TimeSpan.FromSeconds(minSecondsAfterAppOpen) - sinceOpen;
            return false;
        }

        // 2) 쿨타임
        var last = GetLastShownUtc();
        var cd = TimeSpan.FromSeconds(cooldownSeconds);
        if (DateTime.UtcNow - last < cd)
        {
            wait = cd - (DateTime.UtcNow - last);
            reason = $"다음 광고까지 {Format(wait)} 대기해 주세요.";
            return false;
        }

        // 3) 캡
        int daily = PlayerPrefs.GetInt(KEY_DAILY_COUNT, 0);
        if (dailyCap > 0 && daily >= dailyCap) { reason = "오늘은 더 이상 광고를 볼 수 없습니다."; wait = TimeSpan.Zero; return false; }
        if (sessionCap > 0 && sessionCount >= sessionCap) { reason = "이번 세션 한도에 도달했습니다."; wait = TimeSpan.Zero; return false; }
        if (runCap > 0 && runCount >= runCap) { reason = "이번 판에서 더 이상 시청할 수 없습니다."; wait = TimeSpan.Zero; return false; }

        reason = null; wait = TimeSpan.Zero; return true;
    }

    public void RecordShown()
    {
        // 성공적으로 '시청 완료'했을 때 호출
        sessionCount++;
        runCount++;
        PlayerPrefs.SetInt(KEY_DAILY_COUNT, PlayerPrefs.GetInt(KEY_DAILY_COUNT, 0) + 1);
        PlayerPrefs.SetString(KEY_LAST_SHOWN_UNIX, DateTimeToUnix(DateTime.UtcNow).ToString());
        PlayerPrefs.Save();
    }

    // ───────── helpers ─────────
    void RollDailyIfNeeded()
    {
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        string saved = PlayerPrefs.GetString(KEY_DAILY_DATE, "");
        if (saved != today)
        {
            PlayerPrefs.SetString(KEY_DAILY_DATE, today);
            PlayerPrefs.SetInt(KEY_DAILY_COUNT, 0);
            PlayerPrefs.Save();
        }
    }

    DateTime GetLastShownUtc()
    {
        var s = PlayerPrefs.GetString(KEY_LAST_SHOWN_UNIX, "0");
        if (long.TryParse(s, out long unix) && unix > 0) return UnixToDateTime(unix);
        return DateTime.MinValue;
    }

    static long DateTimeToUnix(DateTime dt) => (long)(dt - DateTime.UnixEpoch).TotalSeconds;
    static DateTime UnixToDateTime(long unix) => DateTime.UnixEpoch.AddSeconds(unix);
    static string Format(TimeSpan t) => t.TotalMinutes >= 1 ? $"{(int)t.TotalMinutes:D2}:{t.Seconds:D2}" : $"{t.Seconds}초";
}
