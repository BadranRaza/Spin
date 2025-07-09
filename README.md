# 🎡 Autech Spin Wheel

![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)
![Version](https://img.shields.io/badge/version-1.1.0-brightgreen.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

A powerful and production-ready **Spin Wheel System** for Unity. Features weighted reward probability, a daily cooldown system, watch-ad spins, and an event-driven reward system for easy integration with your game's economy.

## ✨ Core Features

-   **Dynamic Reward Configuration**: Easily set up 8 distinct rewards from the Inspector, including their name, amount, type (e.g., Coins, Gems), rarity, and probability weight.
-   **Weighted Probability Engine**: Precisely control the chance of landing on each reward. Higher weights mean higher probability.
-   **Event-Driven Rewards**: Fires a C# event (`OnRewardWon`) when a reward is won, allowing you to cleanly integrate it with any currency or inventory system without modifying the wheel's code.
-   **Daily Cooldown & Ad Spins**: Includes a built-in free daily spin with a persistent 24-hour cooldown timer. Also supports a "watch ad" button to grant an extra spin.
-   **Customizable Animation & Audio**: Fine-tune the spin duration and animation curve. Assign custom sounds for the spin, standard rewards, and rare rewards to enhance player feedback.
-   **Clean Public API**: Simple, well-documented public methods (`SpinFree()`, `SpinOnAd()`, `ResetCooldown()`) for easy integration with your UI buttons.
-   **Self-Contained State**: Automatically saves its own state (cooldown timer and ad watch status) using `PlayerPrefs`, ensuring persistence across game sessions.
-   **Reward Display**: Shows a pop-up panel to celebrate the player's winnings, complete with a reward icon and amount.

## 🎮 How to Use

This guide will walk you through setting up the Spin Wheel in your project.

### 1. Initial Setup
1.  Find the `SpinWheelPanel` prefab located in `Assets/Spin Wheel/Prefabs/`.
2.  Drag and drop it into your scene hierarchy. It should be a child of a Canvas.

### 2. Inspector Configuration

Select the `SpinWheelPanel` object in your scene and look at the `Spin Wheel` component in the Inspector. You will need to configure the following fields:

-   **UI References**: Assign the required UI components. Most are already pre-assigned in the prefab.
    -   `Wheel Rect Transform`: The `RectTransform` of the spinning wheel image.
    -   `Result Text`: A `TextMeshProUGUI` element to display cooldown timers and win messages.
    -   `Spin Button`: The button for the free daily spin.
    -   `Watch Ad Button`: The button to spin after watching an ad.
    -   `Close Button`: The button to close the spin wheel panel.
-   **Reward Display Panel**: Configure the pop-up that appears after a spin.
    -   `Reward Display Panel`: The parent `GameObject` of the panel.
    -   `Reward Amount Text`: A `TextMeshProUGUI` to show the amount won.
    -   `Reward Image`: An `Image` component to show the icon of the reward (coin or gem).
-   **Spinning Settings**:
    -   `Spin Duration`: How long the wheel spins in seconds (e.g., `3.0`).
-   **Rewards**:
    -   `Coin Image`, `Gem Image`: Sprites for your two currency types.
    -   `Rewards`: An array of 8 `RewardData` elements, corresponding to the 8 segments on the wheel.
-   **Probability Settings**:
    -   `Probability Multiplier`: A global multiplier to adjust the weight distribution. Keep at `1` for standard behavior.
-   **Audio**:
    -   `Audio Source`: The `AudioSource` used to play sounds.
    -   `Spin Sound`: Played when the wheel starts spinning.
    -   `Reward Sound`: Played for common rewards.
    -   `Rare Reward Sound`: A special sound for rewards marked as `Rare` or higher.
-   **Animation**:
    -   `Spin Curve`: An `AnimationCurve` to control the easing of the spin animation (e.g., EaseInOut).

### 3. Configuring the Rewards Array

The `Rewards` array has 8 elements, representing the 8 slices of the wheel. The order is important:
-   **Element 0**: Top segment
-   **Element 1**: Top-right segment
-   **Element 2**: Right segment
-   ...and so on, clockwise.

For each element, you must set:
-   **Name**: The display name of the reward (e.g., "1,000 Coins").
-   **Amount**: The currency amount to award. `double` is used to support large numbers.
-   **Type**: `Coins` (regular currency) or `Gems` (premium currency).
-   **Weight**: The probability weight. Higher values are more common relative to other weights.
-   **Rarity**: `Common`, `Uncommon`, `Rare`, etc. Rewards marked as `Rare` or higher will play the `rareRewardSound`.

### 4. Integrating with UI Buttons

Connect your UI `Button` components' `OnClick()` events to the public methods in the `SpinWheel.cs` script:
-   **Free Spin Button**: Connect to `SpinWheel.SpinFree()`.
-   **Watch Ad Button**: Connect to `SpinWheel.SpinOnAd()`. *Note: You need to implement your ad provider's logic to call this method on ad completion.*
-   **Test/Reset Button (Optional)**: If you have a debug menu, connect a button to `SpinWheel.ResetCooldown()` to reset the 24-hour timer.

The script automatically manages the interactability of these buttons based on the cooldown status.

### 5. Receiving Rewards via Events

The `SpinWheel` does **not** manage player currency directly. Instead, it fires a static C# event, `OnRewardWon`, when a spin is complete. You must create another script (e.g., a `CurrencyManager`) to listen for this event and award the currency to the player.

Here is an example of how to subscribe to the event:

```csharp
// In your CurrencyManager.cs or a similar game management script

using UnityEngine;

public class CurrencyManager : MonoBehaviour 
{
    private void OnEnable()
    {
        // Subscribe to the event when this object is enabled
        SpinWheel.OnRewardWon += HandleReward;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SpinWheel.OnRewardWon -= HandleReward;
    }

    // This method will be called by the SpinWheel when a reward is won
    private void HandleReward(double amount, RewardType type)
    {
        if (type == RewardType.Coins)
        {
            // Add 'amount' to the player's coin balance
            Debug.Log($"Player won {amount} coins!");
            // yourCoinVariable += amount;
        }
        else if (type == RewardType.Gems)
        {
            // Add 'amount' to the player's gem balance
            Debug.Log($"Player won {amount} gems!");
            // yourGemVariable += amount;
        }

        // Remember to save the player's new currency balance
        // SavePlayerData();
    }
}
```

### Example: Weighted Probability
To make "5000 Coins" twice as likely to be won as "50 Gems":
-   **5000 Coins Reward**: Set `Weight` to `20`.
-   **50 Gems Reward**: Set `Weight` to `10`.

The system automatically calculates the percentage chance based on the total weight of all 8 rewards.

## 🚀 Installation Guide
1.  **Download the latest release** from the releases page.
2.  In Unity, go to `Assets > Import Package > Custom Package...` and select the downloaded `.unitypackage` file.

## 📄 License
This project is licensed under the MIT License.

---

**⭐ Star this repo if it helped you!** 

Made with ❤️ by [Autech](https://github.com/BadranRaza) 