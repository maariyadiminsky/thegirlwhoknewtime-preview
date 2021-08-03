/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections.Generic;
using UnityEngine;

public class AchievementsManager : MonoBehaviour {
    public static AchievementsManager instance;

    // these are the achievements that are already unlocked and saved coming from General Save File
    public static Dictionary<string, Achievement_Mystery> CurrentMysteryAchievements = new Dictionary<string, Achievement_Mystery>();
    public static Dictionary<string, Achievement_Badge> CurrentBadgeAchievements = new Dictionary<string, Achievement_Badge>();

    public static int TotalPointsEarned = 0;

    // the count result from this will be in the notification number
    // until player sees notification
    public static int NewAchievementCount = 0;
    // this will be show in menu
    // these are the badges earned, in save data
    public static int TotalAchievementBadgeCount = 0;
    public static int TotalAchievementMysteryCount = 0;
    public static int TotalMysteriesSolvedCount = 0;

    // these are the total badges in general, not earned
    static float TotalPossibleBadges;
    static float TotalPossibleMysteries;
    public static bool HasSeenNewAchievementNotification = false;
    public static AchievementsViewManager AchievementsViewManager;

    static WaitForEndOfFrame Wait;

    static ReferencesForEpisodeUtils _ReferencesForEpisodeUtils;

    void Awake() {
        instance = this;

        FindTotalAchievementTypesCount();
    }

    void OnEnable() {
        FindWait();
    }

    static void FindWait() {
        if (Wait == null) {
            Wait = GameMaster.FindWait(Wait);
        }
    }

    static void FindReferencesForEpisodeUtils() {
        if (_ReferencesForEpisodeUtils == null) {
            GameMaster.FindViewManager().FindReferencesForEpisodeUtils();

            _ReferencesForEpisodeUtils = GameMaster.FindViewManager().ReferencesForEpisodeUtils;
        }
    }

    static void FindTotalAchievementTypesCount() {
        // -1 for both because don't count "None" option
        TotalPossibleBadges = System.Enum.GetValues(typeof(ACHIEVEMENT_CONST.Badge_Type)).Length - 1 ;
        TotalPossibleMysteries = System.Enum.GetValues(typeof(ACHIEVEMENT_CONST.Mystery_Type)).Length - 1;

        TotalPointsPossible = Mathf.Round((TotalPossibleBadges * 10) + (TotalPossibleMysteries * 20));
    }

    public static void FindTotalAchievementsEarnedPercentages() {
        CalculateTotalBadgesUnlocked();
        CalculateTotalMysteriesSolved();
        CalculateTotalMysteriesUnlocked();
    }

    // ========================> SCENE START/END METHODS

    public static void UpdateDataAtSceneStart(SaveManagerUtils.TEMP_ACHIEVEMENTS_DATA AchievementData) {
        // reset
        HasSeenNewAchievementNotification = false;

        // update
        CurrentMysteryAchievements = new Dictionary<string, Achievement_Mystery>(AchievementData.CurrentMysteryAchievements);
        CurrentBadgeAchievements = new Dictionary<string, Achievement_Badge>(AchievementData.CurrentBadgeAchievements);
        NewAchievementCount = AchievementData.NewAchievementCount;
        TotalAchievementBadgeCount = AchievementData.TotalAchievementBadgeCount;
        TotalAchievementMysteryCount = AchievementData.TotalAchievementMysteryCount;
        TotalMysteriesSolvedCount = AchievementData.TotalMysteriesSolvedCount;
        TotalPointsEarned = AchievementData.TotalPointsEarned;
    }

    // gets new achievements and update CurrentAchievements
    // if it already exists, update the existing one
    static Achievement_Mystery Achievement_MysteryResult;
    static Achievement_Badge Achievement_BadgeResult;

    public static int GetPoints(ACHIEVEMENT_CONST.Enum_Type Type, ACHIEVEMENT_CONST.Badge_Type BadgeAchievement = ACHIEVEMENT_CONST.Badge_Type.None, ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None) {
        Achievement_BadgeResult = null;
        Achievement_MysteryResult = null;

        if (Type == ACHIEVEMENT_CONST.Enum_Type.Badge && BadgeAchievement != ACHIEVEMENT_CONST.Badge_Type.None &&
            CurrentBadgeAchievements.TryGetValue($"{BadgeAchievement}", out Achievement_BadgeResult)) {
            return Achievement_BadgeResult.Points;
        } else if (Type == ACHIEVEMENT_CONST.Enum_Type.Mystery && MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None &&
            CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {
            return Achievement_MysteryResult.Points;
        }

        return 0;
    }

    // ========================> SCENE RUNNING METHODS

    public bool IsAchievementSeen(ACHIEVEMENT_CONST.Enum_Type Type, ACHIEVEMENT_CONST.Badge_Type BadgeAchievement = ACHIEVEMENT_CONST.Badge_Type.None, ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None) {
        Achievement_BadgeResult = null;
        Achievement_MysteryResult = null;

        // achievements will show even without scene save
        // this will only cause issues for people who have internet problems or giveup without saving
        // so previously unlocked achievements will be locked again, until they save of course
        if (Type == ACHIEVEMENT_CONST.Enum_Type.Badge && BadgeAchievement != ACHIEVEMENT_CONST.Badge_Type.None) {
            if (CurrentBadgeAchievements.TryGetValue($"{BadgeAchievement}", out Achievement_BadgeResult)) return Achievement_BadgeResult.SawAchievement;
        } else if (Type == ACHIEVEMENT_CONST.Enum_Type.Mystery && MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None) {
            if (CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) return Achievement_MysteryResult.SawAchievement;
        }

        return false;
    }

    public bool IsAchievementUnlocked(ACHIEVEMENT_CONST.Enum_Type Type, ACHIEVEMENT_CONST.Badge_Type BadgeAchievement = ACHIEVEMENT_CONST.Badge_Type.None, ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None) {
        Achievement_BadgeResult = null;
        Achievement_MysteryResult = null;

        if ((Type == ACHIEVEMENT_CONST.Enum_Type.Badge) && (BadgeAchievement != ACHIEVEMENT_CONST.Badge_Type.None) &&
            (CurrentBadgeAchievements.TryGetValue($"{BadgeAchievement}", out Achievement_BadgeResult))) {
            return Achievement_BadgeResult.Unlocked;
        } else if (Type == ACHIEVEMENT_CONST.Enum_Type.Mystery && MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None &&
            CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {

            return Achievement_MysteryResult.Unlocked;
        }

        return false;
    }

    public bool IsMysteryAchievementSolved(ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement) {
        Achievement_MysteryResult = null;

        if (MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None &&
            CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {
            return Achievement_MysteryResult.Solved;
        }

        return false;
    }

    // when a new badge/mystery is unlocked
    public void UnlockBadge(ACHIEVEMENT_CONST.Badge_Type BadgeAchievement = ACHIEVEMENT_CONST.Badge_Type.None, Achievement_Badge Achievement = null) {
        // reset
        Achievement_BadgeResult = null;

        // unlock
        if (BadgeAchievement != ACHIEVEMENT_CONST.Badge_Type.None && CurrentBadgeAchievements.TryGetValue($"{BadgeAchievement}", out Achievement_BadgeResult)) {
            StartUnlockBadge(Achievement_BadgeResult);
        } else if (Achievement != null && CurrentBadgeAchievements.TryGetValue(Achievement.Name, out Achievement_BadgeResult)) {
            StartUnlockBadge(Achievement_BadgeResult);
        } else {
            StartUnlockBadge(Achievement, IsNew: true);
        }
    }

    void StartUnlockBadge(Achievement_Badge Result, bool IsNew = false) {
        if (Result.Unlocked) return;

        Result.Unlocked = true;

        if (IsNew) CurrentBadgeAchievements.Add($"{Result.Name}", Result);

        TotalPointsEarned += 10;
        TotalAchievementBadgeCount += 1;
        NewAchievementCount += 1;

        UpdatePauseButtonNotification();
    }

    public void UnlockMystery(ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None, Achievement_Mystery Achievement = null) {
        // reset
        Achievement_MysteryResult = null;

        // unlock
        if (MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None && CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {
            StartUnlockMystery(Achievement_MysteryResult);
        } else if (Achievement != null && CurrentMysteryAchievements.TryGetValue(Achievement.Name, out Achievement_MysteryResult)) {
            StartUnlockMystery(Achievement_MysteryResult);
        } else {
            StartUnlockMystery(Achievement, IsNew: true);
        }
    }

    void StartUnlockMystery(Achievement_Mystery Result, bool IsNew = false) {
        if (Result.Unlocked) return;

        Result.Unlocked = true;

        if (IsNew) CurrentMysteryAchievements.Add($"{Result.Name}", Result);

        TotalPointsEarned += 10;
        TotalAchievementMysteryCount += 1;
        NewAchievementCount += 1;

        UpdatePauseButtonNotification();
    }

    // when a mystery has been solved
    public void SolveMystery(ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None, Achievement_Mystery Achievement = null) {
        // reset
        Achievement_MysteryResult = null;

        // if mystery achievement already exists update it
        if (MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None && CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {
            StartSolveMystery(Achievement_MysteryResult);
        } else if (Achievement != null && CurrentMysteryAchievements.TryGetValue(Achievement.Name, out Achievement_MysteryResult)) {
            StartSolveMystery(Achievement_MysteryResult);
        }
    }

    void StartSolveMystery(Achievement_Mystery Result) {
        // no need to solve twice
        if (Result.Solved) return;

        Result.Solved = true;
        // player should know of update
        Result.SawAchievement = false;

        TotalPointsEarned += 10;
        NewAchievementCount += 1;
        // Update amount of mysterious solved
        TotalMysteriesSolvedCount += 1;

        UpdatePauseButtonNotification();
    }

    // should be called when clicked on achievement icon
    public static void SawNewAchievement(ACHIEVEMENT_CONST.Enum_Type Type, ACHIEVEMENT_CONST.Badge_Type BadgeAchievement = ACHIEVEMENT_CONST.Badge_Type.None, ACHIEVEMENT_CONST.Mystery_Type MysteryAchievement = ACHIEVEMENT_CONST.Mystery_Type.None) {
        if (Type == ACHIEVEMENT_CONST.Enum_Type.Badge && BadgeAchievement != ACHIEVEMENT_CONST.Badge_Type.None &&
            CurrentBadgeAchievements.TryGetValue($"{BadgeAchievement}", out Achievement_BadgeResult)) {
            // don't set twice, otherwise new achievementcount will distract twice
            if (Achievement_BadgeResult.SawAchievement) return;

            Achievement_BadgeResult.SawAchievement = true;

            NewAchievementCount = NewAchievementCount == 0 ? 0 : NewAchievementCount - 1;
        } else if (Type == ACHIEVEMENT_CONST.Enum_Type.Mystery && MysteryAchievement != ACHIEVEMENT_CONST.Mystery_Type.None &&
            CurrentMysteryAchievements.TryGetValue($"{MysteryAchievement}", out Achievement_MysteryResult)) {
            // don't set twice, otherwise new achievementcount will distract twice
            if (Achievement_MysteryResult.SawAchievement) return;

            Achievement_MysteryResult.SawAchievement = true;

            NewAchievementCount = NewAchievementCount == 0 ? 0 : NewAchievementCount - 1;
        }

        UpdatePauseButtonNotification();
    }

    // ========================> UI

    public static int GetNewAchievementCount() => NewAchievementCount;

    // all of these total methods return a decimal
    static float TotalPointsPossible;
    public static float CalculateTotalCompleted() {
        return TotalPointsEarned == 0 ? TotalPointsEarned : Mathf.Round(TotalPointsEarned / TotalPointsPossible * 100f) / 100f;
    }

    // find out why its not working
    public static float TotalBadgeEarnedPercentage;
    static void CalculateTotalBadgesUnlocked() {
        TotalBadgeEarnedPercentage = GlobalFn.CalculatePercentageCompleted(TotalAchievementBadgeCount, TotalPossibleBadges);
    }

    public static float TotalMysteriesEarnedPercentage;
    static void CalculateTotalMysteriesUnlocked() {
        TotalMysteriesEarnedPercentage = GlobalFn.CalculatePercentageCompleted(TotalAchievementMysteryCount, TotalPossibleMysteries);
    }

    public static float TotalMysteriesSolvedPercentage;
    static void CalculateTotalMysteriesSolved() {
        TotalMysteriesSolvedPercentage = GlobalFn.CalculatePercentageCompleted(TotalMysteriesSolvedCount, TotalPossibleMysteries);
    }

    // this should be called after they clicked the notifications in pause manager
    public static void ClickedNewAchievementsInPauseMenu() {
        HasSeenNewAchievementNotification = true;
    }

    static void UpdatePauseButtonNotification() {
        FindReferencesForEpisodeUtils();

        _ReferencesForEpisodeUtils.CheckIfHasSeenAchievementNotificationCount();
    }

    // ========================> DATA

    // this just tells achivement prefabs in Achievement list
    // if data should show or hide(and show locked instead)
    // if solved is true, depending on type(which will be entered in prefab)
    // it will turn on green and show points in correct color(orange/green) if mystery

    // POINTS SHOULD BE SPECIFICED IN HANDLE CHOICE WHEN ACHIEVEMENT BADGE IS CREATED
    // then they will be used correctly in AchievementItem
    [System.Serializable]
    public class Achievement_Badge {
        public string Name = "";
        public ACHIEVEMENT_CONST.Badge_Type EnumName;
        public int Points = 10;
        public bool Unlocked = false;
        public bool SawAchievement = false;

        public Achievement_Badge() {}

        public Achievement_Badge(ACHIEVEMENT_CONST.Badge_Type Name = ACHIEVEMENT_CONST.Badge_Type.None) {
            this.EnumName = Name;
            this.Name = $"{Name}";
        }
    }

    [System.Serializable]
    public class Achievement_Mystery {
        public string Name = "";
        public ACHIEVEMENT_CONST.Mystery_Type EnumName;
        public int Points = 20;
        public bool Unlocked = false;
        public bool Solved = false;
        public bool SawAchievement = false;

        public Achievement_Mystery() {}

        public Achievement_Mystery(ACHIEVEMENT_CONST.Mystery_Type Name = ACHIEVEMENT_CONST.Mystery_Type.None) {
            this.EnumName = Name;
            this.Name = $"{Name}";
        }
    }

    [System.Serializable]
    public class ACHIEVEMENT_CONST {
        public ACHIEVEMENT_CONST() {}

        public enum Badge_Type { // 10 ... need 4
            None,

            // Prologue
            TwinSouls, 
            SawUnderwaterVision,
            ShamanicConnection,
            RefuseToHelpDeath,
            CompletedPrologue, 
            TimeTraveler,
            GiveUpDeath,
            SillyClown, 
            Pessimist,
            Skeptic,

            EPISODE_01,

            // Episode 1
            BadFriend,
            GoodFriend, 
            NewLove, 
            StrongHeart,
            Independent,
            PerfectlyTrustworthy,
            CatherineLearnedWhatHappensToHerWorld,
            FailedToEarnCatherineTrust, 
            SlappitySlap,
            KissOfConsequence, 
            HeadOverHeart, 
            StrangeHusband, 
            Earthquake,
            Plague,
            OldLove, 
            YouGotKidnapped,

            EPISODE_02,
        }

        // mysteries with to dos need to add a .SolveMystery method in the place when they are solved.
        public enum Mystery_Type { // 5
            None,

            // Prologue
            WhoDidCatherineSlapAtTheWedding,
            WhatHappenedAtSummerCamp,
            WhoIsTheOmniousBeing,
            WhoIsTheShaman,
            WhoIsIzzy, 

            EPISODE_01,

            // Episode 1 => need to unlock and add Solve
            WhyFrankStareAtCatherine,
            WhyDoesCatherineHaveMemoriesOfFrankThatNeverHappened, 
            WhoIsTheIndigenousManAndBabyInHerVisions,
            WhatHappenedToTheBabyAndPeopleInHerVisions,
            WhatHappenedTheNightBeforeTheWedding, 
            WhatHappenedAtDoctorAppointment,
            WhyDoesCropsLookTheSameAsInHerVisions,
            WhoIsTheMaskedMan,
            WhyIsFrankAnxiousWhenWatchingTheMaskedMan,
            WhyAreCatherinesEyesGlowing, 
            WhoWasTheStrangeWomanWithTheCoveredFaceAtTheWedding,
            WhyDidFranksEyeGlow,
            WhyWasBelleHidingInStorageRoom, 
            WhyCantCatherineRememberBelleSmokes,
            WhoWereTheTwoMenAtTheWedding, 
            WhatDoesSurrenderCatherineToRobertsonMean,
            WhereDidBelleGo,
            WhereCouldFrankBe,
            DidVinnyAndCatherineSafelyEscapeTheWedding,

            EPISODE_02,
        }

        public enum Enum_Type {
            Mystery,
            Badge
        };
    }
}
