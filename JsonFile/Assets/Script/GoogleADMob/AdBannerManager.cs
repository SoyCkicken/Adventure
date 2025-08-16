// AdBannerManager.cs
// Google Mobile Ads Unity 10.x 대응 버전
// - RequestConfiguration.Builder() 사용하지 않음
// - AdRequest.Builder() 대신 new AdRequest() 사용
// - Editor/PC에서 컴파일 오류 없도록 UNITY_ANDROID 가드

using UnityEngine;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using System.Collections.Generic;
#endif

public class AdBannerManager : MonoBehaviour
{
    [Header("Ad Unit IDs")]
    [SerializeField] private string testBannerId = "ca-app-pub-3940256099942544/6300978111";
    [SerializeField] private string productionBannerId = "";
    [SerializeField] private bool showOnStart = true;

#if UNITY_ANDROID
    private BannerView bannerView;
#endif
    private bool initialized;

    private void Awake()
    {
#if UNITY_ANDROID
        // (선택) 테스트 디바이스 등록 등 설정은 RequestConfiguration 속성으로 직접 지정
        var cfg = new RequestConfiguration
        {
            // 예) 테스트 디바이스 등록: (필요하면 주석 해제)
            // TestDeviceIds = new List<string> { "TEST_DEVICE_ID" },

            // 예) 아동 보호 설정/퍼스널라이제이션 등도 속성으로 지정 가능
            // TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(cfg);

        MobileAds.Initialize(_ =>
        {
            Debug.Log("[AdMob] Initialize complete.");
            initialized = true;
            if (showOnStart) ShowBanner();
        });
#else
        Debug.Log("[AdMob] Android에서만 동작.");
#endif
    }

    private void OnDestroy()
    {
        DestroyBanner();
    }

    public void ShowBanner(bool useProductionId = false)
    {
#if UNITY_ANDROID
        if (!initialized)
        {
            Debug.LogWarning("[AdMob] 아직 초기화 전.");
            return;
        }

        if (bannerView != null)
        {
            LoadBanner();
            return;
        }

        string adUnitId = useProductionId && !string.IsNullOrEmpty(productionBannerId)
            ? productionBannerId
            : testBannerId;

        // 화면 폭에 맞춘 Anchored Adaptive 사이즈
        AdSize adSize = GetAdaptiveAdSizeForScreenWidth();

        // 하단 고정
        bannerView = new BannerView(adUnitId, adSize, AdPosition.Bottom);

        // 이벤트
        bannerView.OnBannerAdLoaded += () => Debug.Log("[AdMob] Banner loaded.");
        bannerView.OnBannerAdLoadFailed += (err) => Debug.LogError($"[AdMob] Banner load failed: {err}");
        bannerView.OnAdPaid += (adValue) => Debug.Log($"[AdMob] Paid: {adValue.Value} micros {adValue.CurrencyCode}");

        LoadBanner();
#endif
    }

    public void HideBanner()
    {
#if UNITY_ANDROID
        bannerView?.Hide();
#endif
    }

    public void ShowBannerIfCreated()
    {
#if UNITY_ANDROID
        bannerView?.Show();
#endif
    }

    public void DestroyBanner()
    {
#if UNITY_ANDROID
        bannerView?.Destroy();
        bannerView = null;
#endif
    }

    public void ReloadBanner()
    {
#if UNITY_ANDROID
        LoadBanner();
#endif
    }

#if UNITY_ANDROID
    private void LoadBanner()
    {
        if (bannerView == null) return;

        // 10.x에선 기본 생성자 사용 가능
        var request = new AdRequest();
        bannerView.LoadAd(request);
    }

    // Anchored Adaptive 사이즈 계산
    private AdSize GetAdaptiveAdSizeForScreenWidth()
    {
        float dpi = Screen.dpi <= 0 ? 160f : Screen.dpi;
        int widthDp = Mathf.Clamp(Mathf.RoundToInt(Screen.width / (dpi / 160f)), 320, 1200);
        return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(widthDp);
    }

    public void RecreateForOrientationChange(bool useProductionId = false)
    {
        DestroyBanner();
        ShowBanner(useProductionId);
    }
#endif
}
