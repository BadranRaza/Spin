using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

[System.Serializable]
public class RewardData
{
    public string name;
    public double amount;
    public RewardType type;
    public int weight = 10; // Probability weight (higher = more common)
    public RewardRarity rarity = RewardRarity.Common;
}

public enum RewardType
{
    Coins,
    Gems
}

public enum RewardRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

public class SpinWheel : MonoBehaviour
{
    [Header("Example Currency")]
    public TextMeshProUGUI coinsText;
    public double coinsAmount = 0;
    public TextMeshProUGUI gemsText;
    public double gemsAmount = 0;

    [Header("UI References")]
    public RectTransform wheelRectTransform;
    public TextMeshProUGUI resultText;
    public Button spinButton;
    public Button watchAdButton;

    [Header("Reward Display Panel")]
    public GameObject rewardDisplayPanel;
    public TextMeshProUGUI rewardAmountText;
    public Image rewardImage;

    [Header("Spinning Settings")]
    public float spinDuration = 3f;

    [Header("Rewards")]
    public Sprite coinImage, gemImage;
    public RewardData[] rewards = new RewardData[8]; // Always 8 segments, order: top, top-right, right, bottom-right, bottom, bottom-left, left, top-left

    [Header("Probability Settings")]
    [Range(0.1f, 2f)]
    public float probabilityMultiplier = 1f; // Adjust overall probability distribution

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spinSound;
    public AudioClip rewardSound;
    public AudioClip rareRewardSound; // Special sound for rare rewards

    [Header("Animation")]
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Private variables
    private bool isSpinning = false;
    private bool isSpinningInProgress = false;
    private DateTime lastSpinTime;
    private bool watchedAdRecently = false;

    // Probability system
    private float[] segmentProbabilities;
    private float totalWeight;
    private int selectedSegmentIndex = -1;

    // Constants
    private const float COOLDOWN_DURATION = 86400f; // 24 hours in seconds
    private const int SEGMENT_COUNT = 8;

    // Events
    public static event Action<double, RewardType> OnRewardWon;

    #region Public Methods

    /// <summary>
    /// Resets the spin cooldown for testing purposes.
    /// </summary>
    public void ResetCooldown()
    {
        lastSpinTime = DateTime.MinValue;
        watchedAdRecently = false;
        SaveLocalGameState();
        UpdateUI();
    }

    /// <summary>
    /// Starts a normal spin.
    /// </summary>
    public void SpinFree()
    {
        if (CanStartSpin())
        {
            StartSpinProcess(false);
        }
    }

    /// <summary>
    /// Starts a spin after watching an ad.
    /// </summary>
    public void SpinOnAd()
    {
        watchedAdRecently = true;
        PlayerPrefs.SetInt("WatchAdSpinRecently", 1);
        PlayerPrefs.Save();
        if (CanStartSpin())
        {
            StartSpinProcess(true);
        }
    }

    #endregion

    #region Private Methods

    private void Awake()
    {
        ValidateComponents();
        CalculateProbabilities();

        // Subscribe to reward events
        OnRewardWon += HandleRewardWon;
    }

    private void OnEnable()
    {
        LoadLocalGameState();
        UpdateUI();
        UpdateCurrencyUI();
        HideRewardDisplay();
    }

    private void OnDisable()
    {
        SaveLocalGameState();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        OnRewardWon -= HandleRewardWon;
    }

    private void Update()
    {
        // Only update UI when not spinning to avoid performance issues
        if (!isSpinning && !isSpinningInProgress)
        {
            UpdateUI();
        }
    }

    private void CalculateProbabilities()
    {
        // Calculate total weight
        totalWeight = 0f;
        for (int i = 0; i < rewards.Length; i++)
        {
            if (rewards[i] != null)
            {
                totalWeight += rewards[i].weight * probabilityMultiplier;
            }
        }

        // Calculate individual probabilities
        segmentProbabilities = new float[SEGMENT_COUNT];
        for (int i = 0; i < rewards.Length; i++)
        {
            if (rewards[i] != null)
            {
                segmentProbabilities[i] = (rewards[i].weight * probabilityMultiplier) / totalWeight;
            }
        }
    }

    private int SelectSegmentByProbability()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProbability = 0f;

        for (int i = 0; i < segmentProbabilities.Length; i++)
        {
            cumulativeProbability += segmentProbabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        // Fallback to last segment
        return SEGMENT_COUNT - 1;
    }

    private void ValidateComponents()
    {
        if (wheelRectTransform == null)
            Debug.LogError("SpinWheel: wheelRectTransform is not assigned!");
        if (resultText == null)
            Debug.LogError("SpinWheel: resultText is not assigned!");
        if (spinButton == null)
            Debug.LogError("SpinWheel: spinButton is not assigned!");
        if (watchAdButton == null)
            Debug.LogError("SpinWheel: watchAdButton is not assigned!");
    }

    private void SaveLocalGameState()
    {
        try
        {
            string formattedTime = lastSpinTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz");
            PlayerPrefs.SetString("LastSpinTime", formattedTime);
            PlayerPrefs.SetInt("WatchAdSpinRecently", watchedAdRecently ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game state: {e.Message}");
        }
    }

    private void LoadLocalGameState()
    {
        try
        {
            if (PlayerPrefs.HasKey("LastSpinTime"))
            {
                string savedTime = PlayerPrefs.GetString("LastSpinTime");
                lastSpinTime = DateTime.Parse(savedTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            else
            {
                lastSpinTime = DateTime.MinValue;
            }

            watchedAdRecently = PlayerPrefs.GetInt("WatchAdSpinRecently", 0) == 1;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game state: {e.Message}");
            lastSpinTime = DateTime.MinValue;
            watchedAdRecently = false;
        }
    }

    private void UpdateUI()
    {
        if (resultText == null || spinButton == null || watchAdButton == null) return;

        float timeSinceLastSpin = (float)(DateTime.Now - lastSpinTime).TotalSeconds;
        bool canSpin = timeSinceLastSpin >= COOLDOWN_DURATION && !watchedAdRecently;

        spinButton.interactable = canSpin && !isSpinning;
        watchAdButton.gameObject.SetActive(!canSpin && !watchedAdRecently && !isSpinning);

        if (canSpin)
        {
            resultText.text = "Click Spin to Play!";
        }
        else
        {
            resultText.text = $"Next free spin: {GetTimeRemaining()}";
        }
    }

    private string GetTimeRemaining()
    {
        float timeSinceLastSpin = (float)(DateTime.Now - lastSpinTime).TotalSeconds;
        float remainingTime = COOLDOWN_DURATION - timeSinceLastSpin;

        if (remainingTime <= 0) return "00:00:00";

        int hours = (int)(remainingTime / 3600);
        int minutes = (int)((remainingTime % 3600) / 60);
        int seconds = (int)(remainingTime % 60);

        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }

    private bool CanStartSpin()
    {
        if (isSpinning || isSpinningInProgress) return false;

        float timeSinceLastSpin = (float)(DateTime.Now - lastSpinTime).TotalSeconds;
        bool canSpinFree = timeSinceLastSpin >= COOLDOWN_DURATION;
        bool canSpinWithAd = watchedAdRecently;

        return canSpinFree || canSpinWithAd;
    }

    private void StartSpinProcess(bool isAdSpin)
    {
        isSpinning = true;
        isSpinningInProgress = true;

        // Select the target segment based on probability
        selectedSegmentIndex = SelectSegmentByProbability();

        // Update state
        if (!isAdSpin)
        {
            lastSpinTime = DateTime.Now;
            SaveLocalGameState();
        }

        // Play sound
        PlaySound(spinSound);

        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        if (wheelRectTransform == null) yield break;
        float startRotation = wheelRectTransform.eulerAngles.z;
        int fullTurns = 5;
        float segmentAngle = 360f / rewards.Length; // 45°

        float targetAngle = -selectedSegmentIndex * segmentAngle;

        float endRotation = (fullTurns * 360f) + targetAngle;

        float elapsedTime = 0f;
        while (elapsedTime < spinDuration)
        {
            float t = elapsedTime / spinDuration;
            float curveValue = spinCurve.Evaluate(t);
            float rotation = Mathf.Lerp(startRotation, endRotation, curveValue);
            wheelRectTransform.rotation = Quaternion.Euler(0f, 0f, rotation);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        wheelRectTransform.rotation = Quaternion.Euler(0f, 0f, endRotation);

        float finalZ = wheelRectTransform.eulerAngles.z;
        float normalizedRotation = (360f + (finalZ % 360f)) % 360f;

        float adjustedRotation = normalizedRotation > 180f ? normalizedRotation - 360f : normalizedRotation;
        int segmentAtTop = Mathf.RoundToInt(-adjustedRotation / segmentAngle) % rewards.Length;
        if (segmentAtTop < 0) segmentAtTop += rewards.Length;

        AwardReward(selectedSegmentIndex);
        isSpinning = false;
        isSpinningInProgress = false;
    }

    private void AwardReward(int resultIndex)
    {
        if (resultIndex < 0 || resultIndex >= rewards.Length || rewards[resultIndex] == null) return;

        RewardData reward = rewards[resultIndex];
        resultText.text = $"You won: {reward.name}";

        if (reward.rarity >= RewardRarity.Rare)
        {
            PlaySound(rareRewardSound);
        }
        else
        {
            PlaySound(rewardSound);
        }

        ShowRewardDisplay(reward);

        OnRewardWon?.Invoke(reward.amount, reward.type);

        if (!watchedAdRecently)
        {
            watchAdButton.gameObject.SetActive(true);
        }
    }

    private void ShowRewardDisplay(RewardData reward)
    {
        if (rewardDisplayPanel != null)
        {
            rewardDisplayPanel.SetActive(true);
        }
        if (rewardImage != null)
        {
            rewardImage.sprite = reward.type == RewardType.Coins ? coinImage : gemImage;
        }

        if (rewardAmountText != null)
        {
            rewardAmountText.text = reward.amount.ToString();
        }
    }

    private void HideRewardDisplay()
    {
        if (rewardDisplayPanel != null)
        {
            rewardDisplayPanel.SetActive(false);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleRewardWon(double amount, RewardType type)
    {
        switch (type)
        {
            case RewardType.Coins:
                coinsAmount += amount;
                break;
            case RewardType.Gems:
                gemsAmount += amount;
                break;
        }

        UpdateCurrencyUI();
        Debug.Log($"[Currency] Awarded {amount} {type}. Total Coins: {coinsAmount}, Total Gems: {gemsAmount}");
    }

    private void UpdateCurrencyUI()
    {
        if (coinsText != null)
        {
            coinsText.text = FormatCurrency(coinsAmount);
        }

        if (gemsText != null)
        {
            gemsText.text = FormatCurrency(gemsAmount);
        }
    }

    private string FormatCurrency(double amount)
    {
        if (amount < 1000)
            return amount.ToString("F0");

        if (amount >= 1e42) return (amount / 1e42).ToString("F1") + "Td";
        if (amount >= 1e39) return (amount / 1e39).ToString("F1") + "Dd";
        if (amount >= 1e36) return (amount / 1e36).ToString("F1") + "Ud";
        if (amount >= 1e33) return (amount / 1e33).ToString("F1") + "D";
        if (amount >= 1e30) return (amount / 1e30).ToString("F1") + "N";
        if (amount >= 1e27) return (amount / 1e27).ToString("F1") + "O";
        if (amount >= 1e24) return (amount / 1e24).ToString("F1") + "Sp";
        if (amount >= 1e21) return (amount / 1e21).ToString("F1") + "Sx";
        if (amount >= 1e18) return (amount / 1e18).ToString("F1") + "Qi";
        if (amount >= 1e15) return (amount / 1e15).ToString("F1") + "Qa";
        if (amount >= 1e12) return (amount / 1e12).ToString("F1") + "T";
        if (amount >= 1e9) return (amount / 1e9).ToString("F1") + "B";
        if (amount >= 1e6) return (amount / 1e6).ToString("F1") + "M";
        if (amount >= 1e3) return (amount / 1e3).ToString("F1") + "K";

        return amount.ToString("F0");
    }

    #endregion
}
