// AdMobManager.cs
// Google Mobile Ads Unity 10.x ´ëÀÀ
// - ½Ì±ÛÅæ + DontDestroyOnLoad
// - ¹è³Ê(»ó½Ã Ç¥½Ã), Àü¸é/¸®¿öµå(Ç®½ºÅ©¸° ½Ã ¹è³Ê ÀÚµ¿ ¼û±è/º¹¿ø)
// - ¾À ÀüÈ¯/¾Û º¹±Í/ÇØ»óµµ º¯È­ ½Ã ¹è³Ê ÀÚµ¿ º¹±¸
// - Editor/PC¿¡¼­ ÄÄÆÄÀÏ ¿À·ù ¹æÁö¸¦ À§ÇØ ¸ðµç AdMob Å¸ÀÔ ÂüÁ¶¿¡ UNITY_ANDROID °¡µå Àû¿ë
// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
#endif

public sealed class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance { get; private set; }

    [Header("Use Production IDs (¿î¿µ ÀüÈ¯ ½Ã Ã¼Å©)")]
    public bool useProductionIds = false;

    [Header("Ad Unit IDs - TEST (Google °ø½Ä)")]
    public string testBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    public string testRewardedId = "ca-app-pub-3940256099942544/5224354917";

    [Header("Ad Unit IDs - PRODUCTION (ÄÜ¼Ö ¹ß±Þ°ª ÀÔ·Â)")]
    public string bannerId_Prod = "";
    public string interstitialId_Prod = "";
    public string rewardedId_Prod = "";

    [Header("Behavior")]
    [Tooltip("°ÔÀÓ ½ÃÀÛ ½Ã ¹è³Ê ÀÚµ¿ Ç¥½Ã")]
    public bool showBannerOnStart = true;

#if UNITY_ANDROID
    private BannerView banner;
    private InterstitialAd interstitial;
    private RewardedAd rewarded;

    private bool initialized;
    private bool bannerWasVisibleBeforeFullscreen;
    private int lastWidthDp = -1; // Æø(dp) ¹Ù²î¸é Àç»ý¼º Æ®¸®°Å
#else
    // ¿¡µðÅÍ/Å¸ ÇÃ·§Æû¿¡¼­µµ ÇÔ¼ö È£ÃâÀº °¡´ÉÇÏ°Ô ÇÏµÇ, ³»ºÎ´Â NO-OP
    private bool initialized;
#endif

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Lifecycle
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void Awake()
    {
        // ½Ì±ÛÅæ º¸Àå + ¾À À¯Áö
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ¾À º¯°æ, Æ÷Ä¿½º/ÀÏ½ÃÁ¤Áö ÀÌº¥Æ® ±¸µ¶
        SceneManager.sceneLoaded += OnSceneLoaded;     // ¡ç ÀÌ ÀÌº¥Æ®·Î º¯°æ
        SceneManager.activeSceneChanged -= OnActiveSceneChanged; // È¤½Ã ÀÌÀü ÄÚµå ÀÖÀ¸¸é Á¦°Å

#if UNITY_ANDROID
        // (¼±ÅÃ) µ¿ÀÇ/Å¸°ÙÆÃ Á¤Ã¥Àº ÇÊ¿ä½Ã RequestConfiguration ¼Ó¼ºÀ¸·Î ÁöÁ¤
        var cfg = new RequestConfiguration
        {
            // ¿¹: Å×½ºÆ® µð¹ÙÀÌ½º µî·Ï ÇÊ¿ä ½Ã
            // TestDeviceIds = new System.Collections.Generic.List<string> { "TEST_DEVICE_ID" }
        };
        MobileAds.SetRequestConfiguration(cfg);

        MobileAds.Initialize(_ =>
        {
            Debug.Log("[AdMob] Initialize complete");
            initialized = true;

            PreloadAll();

            if (showBannerOnStart)
                ShowBanner();
        });
#else
        Debug.Log("[AdMob] Android¿¡¼­¸¸ µ¿ÀÛ (¿¡µðÅÍ/PC´Â NO-OP)");
#endif
    }
#if UNITY_ANDROID
    private System.Collections.IEnumerator RecreateBannerSoon()
    {
        yield return null;                       // 1ÇÁ·¹ÀÓ ´ë±â
        yield return new WaitForSecondsRealtime(0.05f); // ±â±âº° Å¸ÀÌ¹Ö ÀÌ½´ ´ëºñ »ìÂ¦ ´õ ´ë±â
        RecreateForOrientationChange();          // Destroy ¡æ Show (»õ ¹è³Ê »ý¼º)
    }
#endif
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if UNITY_ANDROID
        // ¾À ·Îµå Á÷ÈÄ ÇÑ ÅÒ ½¬°í "Ç×»ó" Àç»ý¼º (Æø º¯°æ/Æ÷Ä¿½º ÀÌ½´±îÁö ÇÑ¹æ¿¡ ÇØ°á)
        StartCoroutine(RecreateBannerSoon());
#endif
    }
    private void OnApplicationPause(bool pause)
    {
#if UNITY_ANDROID
        if (!pause) StartCoroutine(RecreateBannerSoon());
#endif
    }
    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_ANDROID
        if (hasFocus) StartCoroutine(RecreateBannerSoon());
#endif
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

#if UNITY_ANDROID
        DestroyBanner();

        interstitial?.Destroy();
        rewarded?.Destroy();
        interstitial = null;
        rewarded = null;
#endif
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Banner (»ó½Ã)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void ShowBanner()
    {
#if UNITY_ANDROID
        if (!initialized) { Debug.LogWarning("[AdMob] ¾ÆÁ÷ ÃÊ±âÈ­ Àü"); return; }

        if (banner != null) { banner.Show(); return; }

        string adUnitId = useProductionIds && !string.IsNullOrEmpty(bannerId_Prod) ? bannerId_Prod : testBannerId;

        AdSize size = AdSize.Banner;
        banner = new BannerView(adUnitId, size, AdPosition.Bottom);

        banner.OnBannerAdLoaded += () => Debug.Log("[AdMob] Banner loaded");
        banner.OnBannerAdLoadFailed += (e) => Debug.LogError($"[AdMob] Banner load failed: {e}");
        banner.OnAdPaid += (v) => Debug.Log($"[AdMob] Banner paid {v.Value} micros {v.CurrencyCode}");

        banner.LoadAd(new AdRequest());
#endif
    }

    public void HideBanner()
    {
#if UNITY_ANDROID
        banner?.Hide();
#endif
    }

    public void DestroyBanner()
    {
#if UNITY_ANDROID
        banner?.Destroy();
        banner = null;
#endif
    }

#if UNITY_ANDROID
    private AdSize GetAdaptiveAdSize()
    {
        // px -> dp º¯È¯
        float dpi = Screen.dpi <= 0 ? 160f : Screen.dpi;
        int widthDp = Mathf.Clamp(Mathf.RoundToInt(Screen.width / (dpi / 160f)), 320, 1200);
        lastWidthDp = widthDp;
        return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(widthDp);
    }
#endif

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Interstitial (Àü¸é)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PreloadInterstitial()
    {
#if UNITY_ANDROID
        if (!initialized || interstitial != null) return;

        string adUnitId = useProductionIds && !string.IsNullOrEmpty(interstitialId_Prod) ? interstitialId_Prod : testInterstitialId;

        InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null) { Debug.LogError($"[AdMob] Interstitial load failed: {error}"); return; }

            interstitial = ad;
            RegisterInterstitialEvents(ad);
            Debug.Log("[AdMob] Interstitial loaded");
        });
#endif
    }

#if UNITY_ANDROID
    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            bannerWasVisibleBeforeFullscreen = banner != null;
            HideBanner();
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdMob] Interstitial closed");
            ad.Destroy();
            interstitial = null;
            if (bannerWasVisibleBeforeFullscreen) ShowBanner();
            PreloadInterstitial();
        };
        ad.OnAdFullScreenContentFailed += (err) =>
        {
            Debug.LogError($"[AdMob] Interstitial show failed: {err}");
            ad.Destroy();
            interstitial = null;
            if (bannerWasVisibleBeforeFullscreen) ShowBanner();
            PreloadInterstitial();
        };
    }
#endif

    public bool ShowInterstitial(Action onClosed = null)
    {
#if UNITY_ANDROID
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.OnAdFullScreenContentClosed += () => onClosed?.Invoke();
            interstitial.Show();
            return true;
        }
        Debug.LogWarning("[AdMob] Interstitial not ready");
        PreloadInterstitial();
#endif
        onClosed?.Invoke(); // ºñ¾Èµå·ÎÀÌµå¿¡¼­µµ ÄÝ¹é º¸Àå
        return false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Rewarded (º¸»óÇü)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PreloadRewarded()
    {
#if UNITY_ANDROID
        if (!initialized || rewarded != null) return;

        string adUnitId = useProductionIds && !string.IsNullOrEmpty(rewardedId_Prod) ? rewardedId_Prod : testRewardedId;

        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null) { Debug.LogError($"[AdMob] Rewarded load failed: {error}"); return; }

            rewarded = ad;
            RegisterRewardedEvents(ad);
            Debug.Log("[AdMob] Rewarded loaded");
        });
#endif
    }

#if UNITY_ANDROID
    private void RegisterRewardedEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            bannerWasVisibleBeforeFullscreen = banner != null;
            HideBanner();
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdMob] Rewarded closed");
            ad.Destroy();
            rewarded = null;
            if (bannerWasVisibleBeforeFullscreen) ShowBanner();
            PreloadRewarded();
        };
        ad.OnAdFullScreenContentFailed += (err) =>
        {
            Debug.LogError($"[AdMob] Rewarded show failed: {err}");
            ad.Destroy();
            rewarded = null;
            if (bannerWasVisibleBeforeFullscreen) ShowBanner();
            PreloadRewarded();
        };
    }
#endif

    /// <summary>º¸»óÇü ±¤°í Ç¥½Ã. ¼º°ø(º¸»ó Áö±Þ) ½Ã true ¹ÝÈ¯.</summary>
    public bool ShowRewarded(Action<bool> onFinished = null)
    {
#if UNITY_ANDROID
        if (rewarded != null && rewarded.CanShowAd())
        {
            rewarded.Show(reward =>
            {
                // ÇÊ¿äÇÏ¸é reward.Amount / reward.Type »ç¿ë
                onFinished?.Invoke(true);
            });
            return true;
        }
        Debug.LogWarning("[AdMob] Rewarded not ready");
        PreloadRewarded();
#endif
        onFinished?.Invoke(false);
        return false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Utilities
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PreloadAll()
    {
        PreloadInterstitial();
        PreloadRewarded();
    }

#if UNITY_ANDROID
    private void EnsureBannerVisibleSoon(bool checkRecreate = true)
    {
        StartCoroutine(_EnsureBannerVisibleCo(checkRecreate));
    }

    private IEnumerator _EnsureBannerVisibleCo(bool checkRecreate)
    {
        yield return null; // ¾À ÀüÈ¯ Á÷ÈÄ ÇÑ ÇÁ·¹ÀÓ ´ë±â
        if (!initialized) yield break;

        if (checkRecreate && ShouldRecreateForWidthChange())
        {
            RecreateForOrientationChange();
            yield break;
        }

        if (banner == null) ShowBanner();
        else { banner.Hide(); banner.Show(); } // Åä±Û·Î °¡½Ã¼º È¸º¹
    }

    private bool ShouldRecreateForWidthChange()
    {
        float dpi = Screen.dpi <= 0 ? 160f : Screen.dpi;
        int widthDpNow = Mathf.Clamp(Mathf.RoundToInt(Screen.width / (dpi / 160f)), 320, 1200);
        if (lastWidthDp < 0) { lastWidthDp = widthDpNow; return false; }
        bool changed = widthDpNow != lastWidthDp;
        if (changed) lastWidthDp = widthDpNow;
        return changed;
    }

    public void RecreateForOrientationChange()
    {
        DestroyBanner();
        ShowBanner();
    }
#endif

    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
#if UNITY_ANDROID
        EnsureBannerVisibleSoon(true);
#endif
    }
}
