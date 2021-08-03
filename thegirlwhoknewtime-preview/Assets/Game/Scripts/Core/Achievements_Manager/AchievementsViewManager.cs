/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using TMPro;

public class AchievementsViewManager : MonoBehaviour {
    [SerializeField] GameObject Dim;

    [Header("Rank Data")]
    [SerializeField] TextMeshProUGUI PointsText;
    [SerializeField] TextMeshProUGUI RankText;
    [SerializeField] GameObject RankDataObject;
    [SerializeField] GameObject FinalRank;
    [SerializeField] UIImage AchievementsFillMeter;

    [Header("Unlocked Data")]
    [SerializeField] Image Bronze;
    [SerializeField] Image Silver;
    [SerializeField] Image Gold;
    [SerializeField] TextMeshProUGUI BadgesUnlockedText;
    [SerializeField] Image BadgesUnlockedImage;
    [SerializeField] TextMeshProUGUI MysteriesUnlockedText;
    [SerializeField] Image MysteriesUnlockedImage;
    [SerializeField] TextMeshProUGUI MysteriesSolvedText;
    [SerializeField] Image MysteriesSolvedImage;

    [Header("Badge Achievement Data")]
    [SerializeField] GameObject Achievement_Popup_Badge;
    [SerializeField] Image Achievement_Popup_Badge_Image;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Badge_Title;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Badge_Description;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Badge_Points;

    [Header("Mystery Achievement Data")]
    [SerializeField] GameObject Achievement_Popup_Mystery;
    [SerializeField] Image Achievement_Popup_Mystery_Image;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Mystery_Title;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Mystery_Description;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Mystery_SolvedDescription;
    [SerializeField] TextMeshProUGUI Achievement_Popup_Mystery_Points;

    [SerializeField] GameObject HalfWayThereIcon;
    [SerializeField] GameObject MysteryCompleteIcon;

    public static bool IsAchievementPopupOpen = false;

    WaitForEndOfFrame Wait;

    void Awake() {
        RankDataObject.SetActive(false);
    }

    void OnEnable() {
        FindWait();
        SetDataText();
        SetStats();
        FindPlayerChoice();
    }

    void FindWait() {
        if (Wait == null) {
            Wait = GameMaster.FindWait(Wait);
        }
    }

    void FindPlayerChoice() {
        PLAYER_CHOICE = EpisodeManager.playerChoices;
    }

    static PlayerChoice PLAYER_CHOICE;
    public static string SimpleTextReplace(string Text) {
        return Text.Replace("[VinnyName]", PLAYER_CHOICE._CalledVinnyByPreferredName ?
            TYPES.VINNY_NAMES.NICKNAME :
            TYPES.VINNY_NAMES.ORIGINAL
        );
    }

    void SetDataText() {
        if (Bronze == null || Silver == null || Gold == null) return;

        AchievementsManager.FindTotalAchievementsEarnedPercentages();

        if (BadgesUnlockedText != null && BadgesUnlockedImage != null) {
            BadgesUnlockedText.text = $"{AchievementsManager.TotalBadgeEarnedPercentage}% Badges";
            SetImageForAchievementData(AmountAchieved: AchievementsManager.TotalBadgeEarnedPercentage, ImageToSet: BadgesUnlockedImage);
        }

        if (MysteriesUnlockedText != null && MysteriesUnlockedImage != null) {
            MysteriesUnlockedText.text = $"{AchievementsManager.TotalMysteriesEarnedPercentage}% Mysteries";
            SetImageForAchievementData(AmountAchieved: AchievementsManager.TotalMysteriesEarnedPercentage, ImageToSet: MysteriesUnlockedImage);
        }

        if (MysteriesSolvedText != null && MysteriesSolvedImage != null) {
            MysteriesSolvedText.text = $"{AchievementsManager.TotalMysteriesSolvedPercentage}% Mysteries Solved";
            SetImageForAchievementData(AmountAchieved: AchievementsManager.TotalMysteriesSolvedPercentage, ImageToSet: MysteriesSolvedImage);
        }
    }

    void SetImageForAchievementData(float AmountAchieved, Image ImageToSet) {
        if (AmountAchieved < 50) {
            ImageToSet.sprite = Bronze.sprite;
        } else if (Mathf.Approximately(AmountAchieved, 100)) {
            ImageToSet.sprite = Gold.sprite;
        } else if (AmountAchieved >= 50) {
            ImageToSet.sprite = Silver.sprite;
        }
    }

    static float TotalCompleted;
    void SetStats() {
        // find total achievements completed
        TotalCompleted = AchievementsManager.CalculateTotalCompleted();

        // set points
        PointsText.text = $"{AchievementsManager.TotalPointsEarned}\n<size=50>Points</size>";

        // set percent fill
        AchievementsFillMeter.fillAmount = TotalCompleted;

        // calculate the rank
        if (GlobalFn.IsBetweenRange(AchievementsFillMeter.fillAmount, 0, 0.14f)) {
            RankText.text = TYPES.ACHIEVEMENT_RANK_TYPES.RANK_1;
        } else if (GlobalFn.IsBetweenRange(AchievementsFillMeter.fillAmount, 0.15f, 0.46f)) {
            RankText.text = TYPES.ACHIEVEMENT_RANK_TYPES.RANK_2;
        } else if (GlobalFn.IsBetweenRange(AchievementsFillMeter.fillAmount, 0.47f, 0.77f)) {
            RankText.text = TYPES.ACHIEVEMENT_RANK_TYPES.RANK_3;
        } else if (GlobalFn.IsBetweenRange(AchievementsFillMeter.fillAmount, 0.78f, 0.99f)) {
            RankText.text = TYPES.ACHIEVEMENT_RANK_TYPES.RANK_4;
        }

        if (Mathf.Approximately(AchievementsFillMeter.fillAmount, 1f)) {
            RankDataObject.SetActive(false);
            FinalRank.SetActive(true);
        } else {
            RankDataObject.SetActive(true);
        }
    }

    public static string FindRank() {
        TotalCompleted = AchievementsManager.CalculateTotalCompleted();

        if (GlobalFn.IsBetweenRange(TotalCompleted, 0, 0.14f)) {
            return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_1;
        } else if (GlobalFn.IsBetweenRange(TotalCompleted, 0.15f, 0.46f)) {
            return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_2;
        } else if (GlobalFn.IsBetweenRange(TotalCompleted, 0.47f, 0.77f)) {
            return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_3;
        } else if (GlobalFn.IsBetweenRange(TotalCompleted, 0.78f, 0.99f)) {
            return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_4;
        } else if (Mathf.Approximately(TotalCompleted, 1f)) {
            return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_5;
        }

        return TYPES.ACHIEVEMENT_RANK_TYPES.RANK_1;
    }

    static string SolvedPreText = "<u>SOLVED!</u>\n";
    public IEnumerator OpenAchievementPopup(
        AchievementsManager.ACHIEVEMENT_CONST.Enum_Type Type, Sprite Sprite,
        string Title, string Description, string SolvedDescription,
        int Points, bool IsSolved = false
     ) {
        // Setup Popup
        if (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Badge) {
            Achievement_Popup_Badge_Image.sprite = Sprite;
            Achievement_Popup_Badge_Title.text = Title;
            Achievement_Popup_Badge_Description.text = Description;
            Achievement_Popup_Badge_Points.text = $"{Points}+ Points Earned";

            // wait a frame just to make sure everything got setup in time
            yield return Wait;

            // open achievement popup
            Achievement_Popup_Badge.SetActive(true);
            Dim.SetActive(true);
        } else if (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Mystery) {
            Achievement_Popup_Mystery_Image.sprite = Sprite;
            Achievement_Popup_Mystery_Title.text = Title;
            Achievement_Popup_Mystery_Description.text = Description;

            if (IsSolved) {
                HalfWayThereIcon.SetActive(false);
                MysteryCompleteIcon.SetActive(true);
                Achievement_Popup_Mystery_SolvedDescription.text = $"{SolvedPreText}{SolvedDescription}";
                Achievement_Popup_Mystery_Points.text = $"{Points}/{Points}+ Points Earned";

                Achievement_Popup_Mystery_SolvedDescription.gameObject.SetActive(true);
            } else {
                HalfWayThereIcon.SetActive(true);
                MysteryCompleteIcon.SetActive(false);
                Achievement_Popup_Mystery_Points.text = $"{Points/2}/{Points}+ Points Earned";

                Achievement_Popup_Mystery_SolvedDescription.gameObject.SetActive(false);
            }

            // wait a frame just to make sure everything got setup in time
            yield return Wait;

            // open achievement popup
            Achievement_Popup_Mystery.SetActive(true);
            Dim.SetActive(true);
        } 
        yield break;
    }

    public void CloseAchievementPopup() {
        Achievement_Popup_Mystery.SetActive(false);
        Achievement_Popup_Badge.SetActive(false);
        Dim.SetActive(false);
    }
}
