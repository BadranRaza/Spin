# 🎡 Autech Spin Wheel

![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)
![Version](https://img.shields.io/badge/version-1.0.0-brightgreen.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

A powerful and production-ready **Spin Wheel System** for Unity. Features advanced reward probability, large number currency handling, and a daily cooldown system.

## ✨ Core Features

-   **Dynamic Reward Configuration**: Easily set up rewards with different types (Coins, Gems), amounts, rarities, and images from the Inspector.
-   **Weighted Probability Engine**: Control the exact probability of each reward appearing, making some items rarer than others.
-   **Large Number Support**: The currency system uses `double` to handle extremely large numbers (up to Tredecillion and beyond).
-   **Advanced Number Formatting**: Automatically formats large currency values with standard suffixes (K, M, B, T, etc.) for a clean UI.
-   **Daily Cooldown System**: Provides a free daily spin with a persistent timer that works across game sessions.
-   **Customizable Animation**: Fine-tune the spin duration and animation curve for the perfect feel.
-   **Audio Feedback**: Separate sound effects for the spin, standard rewards, and rare rewards.
-   **Clean Public API**: Simple, well-documented public methods (`Spin()`, `ResetCooldown()`) for easy UI integration.
-   **Persistent Currency**: Automatically saves and loads player currency using `PlayerPrefs`.

## 🎮 How to Use the Spin Wheel

### Configuration
1.  **Add Prefab**: Drag the `SpinWheelPanel` prefab into your scene.
2.  **Assign UI**: Connect your `TextMeshPro` text elements for coins and gems to the `coinsText` and `gemsText` fields in the `SpinWheel` component.
3.  **Configure Rewards**: In the `SpinWheel` component, expand the `Rewards` array and configure each of the 8 segments:
    -   **Name**: The name of the reward (e.g., "1000 Coins").
    -   **Amount**: The currency amount to award. Can be a very large number.
    -   **Type**: `Coins` or `Gems`.
    -   **Weight**: The probability weight. Higher values are more common.
    -   **Rarity**: `Common`, `Uncommon`, `Rare`, etc. Rewards marked as `Rare` or higher will play the `rareRewardSound`.
4.  **Set Images & Sounds**: Assign sprites for the coin and gem icons, and audio clips for the spin/reward sounds.

### UI Integration
Connect your UI buttons to the public methods in the `SpinWheel.cs` script:
-   **Spin Button**: Connect to `SpinWheel.Spin()`.
-   **Test/Reset Button**: Connect to `SpinWheel.ResetCooldown()`.

The `SpinWheel` script will automatically handle enabling/disabling the spin button based on cooldown status.

### Example: Weighted Probability
To make "5000 Coins" twice as likely as "50 Gems":
-   **5000 Coins**: Set `Weight` to `20`.
-   **50 Gems**: Set `Weight` to `10`.

The system automatically calculates the percentage chance based on the total weight of all rewards.

### Currency Formatting
The currency display automatically handles large numbers.
-   `1500` -> `1.5K`
-   `2500000` -> `2.5M`
-   `1000000000000` -> `1.0T`

## 🚀 Installation Guide
1.  **Download the latest release** from the releases page.
2.  In Unity, go to `Assets > Import Package > Custom Package...` and select the downloaded `.unitypackage` file.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**⭐ Star this repo if it helped you!** 

Made with ❤️ by [Autech](https://github.com/HaseebDev)