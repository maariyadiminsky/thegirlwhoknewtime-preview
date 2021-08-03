/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItem : MonoBehaviour {
    [SerializeField] AchievementsManager.ACHIEVEMENT_CONST.Enum_Type Type;
    [SerializeField] GameObject NewAchievementIcon;
    [SerializeField] TextMeshProUGUI NewAchievementIconText;
    [SerializeField] AudioSimple AudioSimple; 

    // only applicable for Mystery Achievements
    [SerializeField] GameObject NewAchievementGreenIcon;

    [Header("Badge Type Only")]
    [SerializeField] AchievementsManager.ACHIEVEMENT_CONST.Badge_Type BadgeName = AchievementsManager.ACHIEVEMENT_CONST.Badge_Type.None;

    [Header("Mystery Type Only")]
    [SerializeField] AchievementsManager.ACHIEVEMENT_CONST.Mystery_Type MysteryName = AchievementsManager.ACHIEVEMENT_CONST.Mystery_Type.None;

    [Header("Data")]
    [SerializeField] Image Image;
    [SerializeField] string Title; // Optional
    [SerializeField] string Description;
    [SerializeField] string SolvedDescription;
    int Points;

    [Header("Containers")]
    [SerializeField] GameObject LockedContainer;
    [SerializeField] GameObject UnlockedContainer;

    AchievementsManager _AchievementsManager;

    bool IsSolved = false;
    bool IsUnlocked = false;
    bool IsSeen = false;

    WaitForEndOfFrame Wait;

    void Awake() {
        FindWait();
        FindAchievementsManager();
    }

    void OnEnable() {
        UnlockTry();
        CheckIfSolvedTry();
        CheckIfSeenTry();
        FindTitle();

        Points = AchievementsManager.GetPoints(Type, BadgeName, MysteryName);
    }

    void FindWait() {
        if (Wait == null) {
            Wait = GameMaster.FindWait(Wait);
        }
    }

    void FindAchievementsManager() {
        if (_AchievementsManager == null) {
            _AchievementsManager = GameMaster.FindAchievementsManager();
        }
    }

    void FindTitle() {
        // note: title is either the enum name or if want more detailed enter it, but it's not required
        Title = string.IsNullOrEmpty(Title) ? FindNameFromEnumName() : Title;
    }

    bool IsTypeMystery = false;
    string FindNameFromEnumName() {
        IsTypeMystery = Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Mystery;
        return IsTypeMystery ? $"{MysteryName}?" : $"{BadgeName}";
    }

    void UnlockTry() {
        if ((Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Badge && _AchievementsManager.IsAchievementUnlocked(BadgeAchievement: BadgeName, Type: Type)) ||
            (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Mystery && _AchievementsManager.IsAchievementUnlocked(MysteryAchievement: MysteryName, Type: Type))) {
            LockedContainer.SetActive(false);
            UnlockedContainer.SetActive(true);

            IsUnlocked = true;
        }  else {
            LockedContainer.SetActive(true);
            UnlockedContainer.SetActive(false);

            IsUnlocked = false;
        }
    }

    void CheckIfSolvedTry() {
        if (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Mystery) {
            IsSolved = _AchievementsManager.IsMysteryAchievementSolved(MysteryName);
        }
    }

    void CheckIfSeenTry() {
        // no need to check other logic since this shouldn't show if locked
        if (!IsUnlocked) {
            NewAchievementIcon.SetActive(false);
            return;
        }

        UpdateIsSeen();

        UpdateNotificationIcon();
    }

    void UpdateIsSeen() {
        IsSeen = _AchievementsManager.IsAchievementSeen(Type, BadgeName, MysteryName);
    }

    public void OpenAchievementPopup() {
        // check if achievement is unlocked first
        if (IsUnlocked) {
            AudioSimple.PlaySound();
            StartCoroutine(StartOpenAchievementPopup());
        }
    }

    void UpdateNotificationIcon(bool OpenedPopup = false) {
        if (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Badge) {
            NewAchievementIcon.SetActive(OpenedPopup ? false : !IsSeen);
        } else if (Type == AchievementsManager.ACHIEVEMENT_CONST.Enum_Type.Mystery) {
            if (OpenedPopup) {
                NewAchievementIcon.SetActive(false);
                NewAchievementGreenIcon.SetActive(!IsSolved);
            } else if (!OpenedPopup) {
                NewAchievementIcon.SetActive(!IsSeen);
                NewAchievementGreenIcon.SetActive(IsSeen && !IsSolved);
            }
        }
    }

    bool OpeningPopup = false;
    IEnumerator StartOpenAchievementPopup() {
        if (OpeningPopup) yield break;

        OpeningPopup = true;

        while (AchievementsManager.AchievementsViewManager == null) yield return Wait;

        // set that achievement has been seen
        AchievementsManager.SawNewAchievement(Type, BadgeName, MysteryName);
        UpdateNotificationIcon(OpenedPopup: true);

        // check if any text needs to be replaced
        Title = AchievementsViewManager.SimpleTextReplace(Title);
        Description = AchievementsViewManager.SimpleTextReplace(Description);
        SolvedDescription = AchievementsViewManager.SimpleTextReplace(SolvedDescription);

        // close the "new achievement" icon
        yield return AchievementsManager.AchievementsViewManager.OpenAchievementPopup(Type, Image.sprite, Title, Description, SolvedDescription, Points, IsSolved: IsSolved);

        OpeningPopup = false;

        yield break;
    }
}
