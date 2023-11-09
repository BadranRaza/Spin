using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SpinWheel : MonoBehaviour
{
    public GameObject CoverImg;
    public RectTransform wheelRectTransform;
    public RectTransform needleRectTransform;
    public RectTransform wheelHolderRectTransform;
    public Text resultText;
    public Button spinButton;
    public Button WatchAdButton;
    public float spinDuration = 3f;
    private bool isSpinning = false;
    private DateTime lastSpinTime;
    public Image RewardImage;
    public Sprite CoinImage, GemImage;
    public Text RewardAmountText;
    int segmentCount = 8;

    public bool watchedAdRecently = false;

    private const float CooldownDuration = 86400f; // 24 hours in seconds
    bool ifSpining;
    private void OnEnable()
    {
        // Load the saved game state
        LoadLocalGameState();

        if (!PlayerPrefs.HasKey("WatchAdSpinRecently"))
        {
            PlayerPrefs.SetInt("WatchAdSpinRecently", watchedAdRecently ? 1 : 0);
            watchedAdRecently = PlayerPrefs.GetInt("WatchAdSpinRecently") == 1;
        }
        else
        {
            watchedAdRecently = PlayerPrefs.GetInt("WatchAdSpinRecently") == 1;
        }

        //CoverImg.SetActive(true);
        RewardImage.gameObject.SetActive(false);
        RewardAmountText.gameObject.SetActive(false);
        //lastSpinTime = DateTime.MinValue;
        UpdateUI();
        CoverImg.SetActive(true);
        Invoke(nameof(CoverImageFu), 3f);
    }


    private void CoverImageFu()
    {
        CoverImg.gameObject.SetActive(false);

    }

    private void OnDisable()
    {
        // Save the game state when the script is disabled
        SaveLocalGameState();
    }

    private void Update()
    {
        // Check the time and update the UI in the Update function
        UpdateUI();
    }

    private void SaveLocalGameState()
    {
        string formattedTime = lastSpinTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz");
        PlayerPrefs.SetString("LastSpinTime", formattedTime);
    }

    private void LoadLocalGameState()
    {
        if (PlayerPrefs.HasKey("LastSpinTime"))
        {
            string savedTime = PlayerPrefs.GetString("LastSpinTime");
            lastSpinTime = DateTime.Parse(savedTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            Debug.Log("Loaded LastSpin Time: " + lastSpinTime);
        }
        else
        {
            lastSpinTime = DateTime.MinValue;
            Debug.Log("LastSpin Time not found in PlayerPrefs.");
        }
    }

    private void UpdateUI()
    {
        if (!watchedAdRecently)
        {
            //CoverImg.SetActive(true);
            float timeSinceLastSpin = (float)(DateTime.Now - lastSpinTime).TotalSeconds;

            if (timeSinceLastSpin < CooldownDuration)
            {
                if (!ifSpining)
                {
                    WatchAdButton.gameObject.SetActive(true);
                    spinButton.interactable = false;
                    resultText.text = "Spin will be available in: " + GetTimeRemaining();
                }

                ////CoverImg.SetActive(false);

            }
            else
            {
                if (!ifSpining)
                {
                    WatchAdButton.gameObject.SetActive(false);
                    watchedAdRecently = false;
                    PlayerPrefs.SetInt("WatchAdSpinRecently", watchedAdRecently ? 1 : 0);
                    spinButton.interactable = true;
                    resultText.text = "Click Spin to Play!";
                }

            }
        }
        else
        {
            resultText.text = "Spin will be available in: " + GetTimeRemaining();
            WatchAdButton.gameObject.SetActive(false);
            spinButton.interactable = false;
        }

        ////CoverImg.SetActive(false);
    }

    private string GetTimeRemaining()
    {
        float timeSinceLastSpin = (float)(DateTime.Now - lastSpinTime).TotalSeconds;
        //Debug.Log("timeSinceLastSpin " + timeSinceLastSpin);
        float remainingTime = CooldownDuration - timeSinceLastSpin;

        int hours = (int)(remainingTime / 3600);
        int minutes = (int)((remainingTime % 3600) / 60);
        int seconds = (int)(remainingTime % 60);

        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }

    // Additional Chaipi System for Spin on WatchAd
    public void WatchAdStartSpinning()
    {
        //GameManager.Instance.freeToUseAudioSource.PlayOneShot(GameManager.Instance.WatchAdButtonSpinClip);
        StartCoroutine(SpinCoroutine());
    }

    // Start spinning with a desired result index
    public void StartSpinning()
    {
        if (!isSpinning && DateTime.Now >= lastSpinTime.AddHours(24))
        {
            spinButton.interactable = false;
            WatchAdButton.gameObject.SetActive(false);
            // Save the game state
            lastSpinTime = DateTime.Now;
            SaveLocalGameState();
            StartCoroutine(SpinCoroutine());
        }
    }

    private IEnumerator SpinCoroutine()
    {
        Debug.Log("Starting SpinCoroutine...");

        isSpinning = true;

        float startRotation = wheelRectTransform.eulerAngles.z;
        float randomRotation = UnityEngine.Random.Range(720f, 1080f); // Randomize the rotation
        float endRotation = startRotation + randomRotation;

        float elapsedTime = 0f;

        while (elapsedTime < spinDuration)
        {
            float t = elapsedTime / spinDuration;
            float rotation = Mathf.Lerp(startRotation, endRotation, t);

            // Rotate the wheel visually
            wheelRectTransform.rotation = Quaternion.Euler(0f, 0f, rotation);

            elapsedTime += Time.deltaTime;
            ifSpining = true;
            yield return null;
        }

        // Ensure the wheel ends at the correct rotation
        wheelRectTransform.rotation = Quaternion.Euler(0f, 0f, endRotation);

        Debug.Log("SpinCoroutine Finished Spinning...");

        isSpinning = false;

        // Calculate the result index based on the improved method
        int resultIndex = CalculateResultIndex();

        Debug.Log("Actual Result Index: " + resultIndex);

        string[] rewards = {
            "5 Gems", "5000 Coins", "50 Gems", "1500 Coins", "2 Gems", "1000 Coins",
            "10 Gems", "500 Coins"
        };

        string reward = rewards[resultIndex];
        resultText.text = "You won: " + reward;

        if (resultIndex == 0)
        {
            RewardGems(5);
        }
        if (resultIndex == 1)
        {
            RewardCoins(5000);
        }
        if (resultIndex == 2)
        {
            RewardGems(50);
        }
        if (resultIndex == 3)
        {
            RewardCoins(1500);
        }
        if (resultIndex == 4)
        {
            RewardGems(2);
        }
        if (resultIndex == 5)
        {
            RewardCoins(1000);
        }
        if (resultIndex == 6)
        {
            RewardGems(10);
        }
        if (resultIndex == 7)
        {
            RewardCoins(500);
        }

        ifSpining = false;
    }

    private int CalculateResultIndex()
    {
        // Calculate the normalized rotation angle
        float normalizedRotation = (360f + (wheelRectTransform.eulerAngles.z % 360f)) % 360f;

        // Calculate the angle between segments
        float segmentAngle = 360f / segmentCount;

        // Calculate the angle for the top center segment
        float topCenterAngle = 0f; // Assuming top center is at 0 degrees

        // Calculate the result index as the segment that aligns with the top center within a tolerance
        int resultIndex = Mathf.FloorToInt((normalizedRotation - topCenterAngle + segmentAngle / 2f) / segmentAngle);

        return resultIndex;
    }

    // Reset the cooldown manually
    public void ResetCooldown()
    {
        lastSpinTime = DateTime.MinValue;
        SaveLocalGameState();
        watchedAdRecently = false;
        PlayerPrefs.SetInt("WatchAdSpinRecently", watchedAdRecently ? 1 : 0);
        UpdateUI();
    }

    public void RewardCoins(int amount)
    {
        if (!watchedAdRecently)
        {
            WatchAdButton.gameObject.SetActive(true);
            Debug.Log("LLOO GGG");
        }

        RewardImage.gameObject.SetActive(true);
        RewardAmountText.gameObject.SetActive(true);
        RewardImage.sprite = CoinImage;
        RewardAmountText.text = amount.ToString();
        CoverImg.SetActive(true);
        Invoke(nameof(CoverImageFu), 2f);
    }

    public void RewardGems(int amount)
    {
        if (!watchedAdRecently)
        {
            WatchAdButton.gameObject.SetActive(true);
            Debug.Log("LLOO GGG");
        }

        RewardImage.gameObject.SetActive(true);
        RewardAmountText.gameObject.SetActive(true);
        RewardImage.sprite = GemImage;
        RewardAmountText.text = amount.ToString();
        CoverImg.SetActive(true);
        Invoke(nameof(CoverImageFu), 2f);
    }

    public void WatchAd()
    {
        SpinOnAd();
    }

    public void SpinOnAd()
    {
        watchedAdRecently = true;
        PlayerPrefs.SetInt("WatchAdSpinRecently", watchedAdRecently ? 1 : 0);
        if (watchedAdRecently == true)
        {
            WatchAdStartSpinning();
            WatchAdButton.gameObject.SetActive(false);
            Debug.Log("SpinActivated");
        }
    }
}
