/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

// this works with SPINE ANIMATION SOFTWARE
public class AnimationManager : MonoBehaviour {
    // ****************************** VARS ****************************** //

    // used for updating current character's(characterName-pose) active animations
    static Dictionary<int, AnimationData> AnimationDataDictionary = new Dictionary<int, AnimationData>();
    static CharacterData Character;
    static string ViewName; // holds the Viewname and layer type needed when updating data for layer
    static string LayerType;
    static SkeletonGraphic CurrentSkeletonGraphic;
    static AnimationEventManager AnimationEventManager;
    static bool IsCharacterAnimation = false;

    // multiple tracks for track type, useful for assets with multiple characters
    // or characters with multiple same type animation, ie. main characters
    public static int ATMOSPHERE_TRACK = 0;
    public static int ATMOSPHERE_2_TRACK = 1;
    public static int BODY_TRACK = 2;
    public static int BODY_2_TRACK = 3;
    public static int EYE_TRACK = 4;
    public static int EYE_2_TRACK = 5;
    public static int EMOTION_TRACK = 6;
    public static int EMOTION_2_TRACK = 7;
    public static int MOUTH_TRACK = 8;
    public static int MOUTH_2_TRACK = 9;
    public static int MARK_TRACK = 10;
    public static int MARK_2_TRACK = 12;
    public static int MARK_3_TRACK = 13;

    public enum ANIMATION_TYPE {
        None,
        atmosphere,
        atmosphere_2,
        body,
        body_2,
        eye,
        eye_2,
        emotion,
        emotion_2,
        mouth,
        mouth_2,
        mark,
        mark_2,
        mark_3
    };

    public static string FindAnimationTypeFromEnum(ANIMATION_TYPE Type) {
        switch(Type) {
            case ANIMATION_TYPE.atmosphere:
                return TYPES.ANIMATION.TYPE.ATMOSPHERE;
            case ANIMATION_TYPE.atmosphere_2:
                return TYPES.ANIMATION.TYPE.ATMOSPHERE_2;
            case ANIMATION_TYPE.body:
                return TYPES.ANIMATION.TYPE.BODY;
            case ANIMATION_TYPE.body_2:
                return TYPES.ANIMATION.TYPE.BODY_2;
            case ANIMATION_TYPE.eye:
                return TYPES.ANIMATION.TYPE.EYE;
            case ANIMATION_TYPE.eye_2:
                return TYPES.ANIMATION.TYPE.EYE_2;
            case ANIMATION_TYPE.emotion:
                return TYPES.ANIMATION.TYPE.EMOTION;
            case ANIMATION_TYPE.emotion_2:
                return TYPES.ANIMATION.TYPE.EMOTION_2;
            case ANIMATION_TYPE.mouth:
                return TYPES.ANIMATION.TYPE.MOUTH;
            case ANIMATION_TYPE.mouth_2:
                return TYPES.ANIMATION.TYPE.MOUTH_2;
            case ANIMATION_TYPE.mark:
                return TYPES.ANIMATION.TYPE.MARK;
            case ANIMATION_TYPE.mark_2:
                return TYPES.ANIMATION.TYPE.MARK_2;
            case ANIMATION_TYPE.mark_3:
                return TYPES.ANIMATION.TYPE.MARK_3;
            case ANIMATION_TYPE.None:
            default:
                return null;
        }
    }

    // ***************************** SPECIAL ***************************** //
    // some of these methods might only be used once or twice.
    public static class SPECIFIC_PURPOSE_TYPES {
        public const string CHAT = "Chat";
        public const string STOP_CHAT = "StopChat";
    }

    // ***************************** SET/GET SKINS ***************************** //

    static public void SetOutfit(SkeletonGraphic SkeletonGraphic, string SkinName) {
        if (string.IsNullOrEmpty(SkinName)) return;

        SkeletonGraphic.Skeleton.SetSkin(SkinName);
        SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
        SkeletonGraphic.LateUpdate();
    }

    static public string GetOutfit(SkeletonGraphic SkeletonGraphic) {
        return SkeletonGraphic.Skeleton.Skin.Name;
    }

    // ***************************** SET ALPHA ***************************** //

    static Color Color;
    static public void SetAlpha(SkeletonGraphic SkeletonGraphic, float Alpha) {
        Color = SkeletonGraphic.color;
        SkeletonGraphic.color = new Color(Color.r, Color.g, Color.b, Alpha);
    }

    // ********************** ANIMATION UTILS METHODS *********************** //

    public static int FindAnimationTrackFromType(string Type) {
        switch (Type) {
            case TYPES.ANIMATION.TYPE.ATMOSPHERE:
                return ATMOSPHERE_TRACK;
            case TYPES.ANIMATION.TYPE.BODY:
                return BODY_TRACK;
            case TYPES.ANIMATION.TYPE.EYE:
                return EYE_TRACK;
            case TYPES.ANIMATION.TYPE.EMOTION:
                return EMOTION_TRACK;
            case TYPES.ANIMATION.TYPE.MOUTH:
                return MOUTH_TRACK;
            case TYPES.ANIMATION.TYPE.MARK:
                return MARK_TRACK;
            case TYPES.ANIMATION.TYPE.ATMOSPHERE_2:
                return ATMOSPHERE_2_TRACK;
            case TYPES.ANIMATION.TYPE.BODY_2:
                return BODY_2_TRACK;
            case TYPES.ANIMATION.TYPE.EYE_2:
                return EYE_2_TRACK;
            case TYPES.ANIMATION.TYPE.EMOTION_2:
                return EMOTION_2_TRACK;
            case TYPES.ANIMATION.TYPE.MOUTH_2:
                return MOUTH_2_TRACK;
            case TYPES.ANIMATION.TYPE.MARK_2:
                return MARK_2_TRACK;
            case TYPES.ANIMATION.TYPE.MARK_3:
                return MARK_3_TRACK;
            default:
                return TYPES.NULL_REPLACEMENT.NUM;
        }
    }

    public static void JumpToAnimationStart(string Name, string Type, SkeletonGraphic AnimationSkeletonGraphic, int Track, bool ShouldLoop = true, bool IsChar = false) {
        IS_CHARACTER = IsChar;

        JumpToStartOrEndFrame(Name: Name, Type: Type, AnimationSkeletonGraphic: AnimationSkeletonGraphic, Track: Track, ShouldLoop, LastFrame: false);
    }

    public static void JumpToAnimationEnd(string Name, string Type, SkeletonGraphic AnimationSkeletonGraphic, int Track, bool ShouldLoop = true, bool IsChar = false) {
        IS_CHARACTER = IsChar;

        JumpToStartOrEndFrame(Name: Name, Type: Type, AnimationSkeletonGraphic: AnimationSkeletonGraphic, Track: Track, ShouldLoop, LastFrame: true);
    }

    public static void JumpToAnimationCustomTrackTime(string Name, string Type, SkeletonGraphic AnimationSkeletonGraphic, int Track, bool ShouldLoop = true, float CustomTrackTime = 0f, bool IsChar = false) {
        IS_CHARACTER = IsChar;

        JumpToStartOrEndFrame(Name: Name, Type: Type, AnimationSkeletonGraphic: AnimationSkeletonGraphic, Track: Track, ShouldLoop, IsCustomTrackTime: true, CustomTrackTime: CustomTrackTime);
    }

    static void JumpToStartOrEndFrame(string Name, string Type, SkeletonGraphic AnimationSkeletonGraphic, int Track, bool ShouldLoop = true, bool LastFrame = false, bool IsCustomTrackTime = false, float CustomTrackTime = 0f, bool IsCharacter = false) {
        AnimationDataSetup(Type, Name);

        Spine.TrackEntry animationTrackEntry = AnimationSkeletonGraphic.AnimationState.SetAnimation(
            TRACK,
            NAME,
            ShouldLoop
        );

        if (IsCustomTrackTime) {
            animationTrackEntry.TrackTime = CustomTrackTime;
        } else {
            animationTrackEntry.TrackTime = LastFrame ? animationTrackEntry.AnimationEnd : animationTrackEntry.AnimationStart; //Sets current trackTime to the end of the animation
        }

        // Also, if you want it to snap from whatever the previous animation was on that TrackIndex, you'll need to set the mix time to zero.
        // ie. animationTrackEntry.TimeScale = 0;
        animationTrackEntry.TimeScale = 0;
    }

    // *********************** SET ANIMATION METHODS ************************ //

    // need this because character animations are differently organized
    // because of so many animations
    static string LOCATION_PREFIX;
    static string CHAR_PREFIX = "/";
    static string BG_PREFIX = "-";

    static string EMOTION_STRING;
    static string CHAR_EMOTION_STRING = "emotion";
    static string BG_EMOTION_STRING = "emo";

    static string NAME = "";
    static bool IsPlayingAnimationLoopCount;
    static void SetAnimationBasedOnAnimationType(string AnimationType, string AnimationName, float AnimationSpeed, bool AnimationLoop, int AnimationLoopCount, bool shouldUseMouthFromEmotionTrackInsteadOfMouthTrack = false) {

        AnimationDataSetup(AnimationType, AnimationName, AnimationLoopCount, shouldUseMouthFromEmotionTrackInsteadOfMouthTrack);

        IsPlayingAnimationLoopCount = AnimationLoopCount > 0;

        // stops bug that plays past animations in a queue, especially valuable for talking/mouth animation
        if (IsPlayingAnimationLoopCount) {
            StopSingleAnimation(CurrentSkeletonGraphic.AnimationState, TRACK);
        }

        SetAnimationToTrack(NAME, TRACK, AnimationSpeed, AnimationLoop, AnimationLoopCount);
    }

    static void AnimationDataSetup(string AnimationType, string AnimationName, int AnimationLoopCount = 0, bool shouldUseMouthFromEmotionTrackInsteadOfMouthTrack = false) {
        if (IS_CHARACTER) {
            LOCATION_PREFIX = CHAR_PREFIX;
            EMOTION_STRING = CHAR_EMOTION_STRING;
        } else {
            LOCATION_PREFIX = BG_PREFIX;
            EMOTION_STRING = BG_EMOTION_STRING;
        }

        TRACK = BODY_TRACK;
        NAME = "";
        switch (AnimationType) {
            case TYPES.ANIMATION.TYPE.ATMOSPHERE:
                if (AnimationEventManager != null) AnimationEventManager.atmosphereLoopCount = AnimationLoopCount;
                TRACK = ATMOSPHERE_TRACK;
                NAME = AnimationName == "default" ? "atmosphere" : $"atmosphere-{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.ATMOSPHERE_2:
                if (AnimationEventManager != null) AnimationEventManager.atmosphereLoopCount_2 = AnimationLoopCount;
                TRACK = ATMOSPHERE_2_TRACK;
                NAME = $"atmosphere-{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.BODY:
                if (AnimationEventManager != null) AnimationEventManager.bodyLoopCount = AnimationLoopCount;
                TRACK = BODY_TRACK;
                NAME = $"body{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.BODY_2:
                if (AnimationEventManager != null) AnimationEventManager.bodyLoopCount = AnimationLoopCount;
                TRACK = BODY_2_TRACK;
                NAME = $"body{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.EYE:
                if (AnimationEventManager != null) AnimationEventManager.eyeLoopCount = AnimationLoopCount;
                TRACK = EYE_TRACK;
                NAME = $"eye{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.EYE_2:
                if (AnimationEventManager != null) AnimationEventManager.eyeLoopCount_2 = AnimationLoopCount;
                TRACK = EYE_2_TRACK;
                NAME = $"eye{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.EMOTION:
                if (AnimationEventManager != null) AnimationEventManager.emotionLoopCount = AnimationLoopCount;
                TRACK = EMOTION_TRACK;
                NAME = $"{EMOTION_STRING}{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.EMOTION_2:
                if (AnimationEventManager != null) AnimationEventManager.emotionLoopCount_2 = AnimationLoopCount;
                TRACK = EMOTION_2_TRACK;
                NAME = $"{EMOTION_STRING}{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.MOUTH:
                if (AnimationEventManager != null) {
                    AnimationEventManager.mouthLoopCount = AnimationLoopCount;
                    AnimationEventManager.shouldUseMouthFromEmotionTrack = shouldUseMouthFromEmotionTrackInsteadOfMouthTrack;
                }
                TRACK = MOUTH_TRACK;
                NAME = $"mouth-talking{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.MOUTH_2:

                if (AnimationEventManager != null) {
                    AnimationEventManager.mouthLoopCount_2 = AnimationLoopCount;
                    AnimationEventManager.shouldUseMouthFromEmotionTrack = shouldUseMouthFromEmotionTrackInsteadOfMouthTrack;
                }
                TRACK = MOUTH_2_TRACK;
                NAME = $"mouth-talking{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.MARK:
                if (AnimationEventManager != null) AnimationEventManager.markLoopCount = AnimationLoopCount;
                TRACK = MARK_TRACK;
                NAME = $"mark{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.MARK_2:
                if (AnimationEventManager != null) AnimationEventManager.markLoopCount_2 = AnimationLoopCount;
                TRACK = MARK_2_TRACK;
                NAME = $"mark{LOCATION_PREFIX}{AnimationName}";
                break;
            case TYPES.ANIMATION.TYPE.MARK_3:
                if (AnimationEventManager != null) AnimationEventManager.markLoopCount_3 = AnimationLoopCount;
                TRACK = MARK_3_TRACK;
                NAME = $"mark{LOCATION_PREFIX}{AnimationName}";
                break;
        }
    }

    static void SetAnimationToTrack(string AnimationName, int Track, float AnimationSpeed, bool AnimationLoop, int AnimationLoopCount = 0) {
        Spine.AnimationState CurrentAnimationState = CurrentSkeletonGraphic.AnimationState;

        AnimationData AnimationData = new AnimationData {
            Track = Track,
            Name = AnimationName,
            Speed = AnimationSpeed,
            ShouldLoop = AnimationLoop,
            LoopCount = AnimationLoopCount
        };

        // check if animation on current track already exists
        if (!(CurrentAnimationState.GetCurrent(0) == null)) {
            UpdateSingleAnimation(CurrentAnimationState, AnimationData);
        } else {
            Spine.TrackEntry animationTrackEntry = CurrentAnimationState.SetAnimation(Track, AnimationName, AnimationLoop);
            animationTrackEntry.TimeScale = AnimationSpeed;
        }

        AnimationDataDictionary[Track] = AnimationData;
    }

    static Spine.TrackEntry TrackEntry = null;
    static int TRACK;
    public static void SetAnimationSpeed(CharacterData CharacterToUpdate, SkeletonGraphic CharacterSkeletonGraphicToUpdate, string Type, float Speed) {
        Character = CharacterToUpdate;
        CurrentSkeletonGraphic = CharacterSkeletonGraphicToUpdate;
        //AnimationDataDictionary = Character.currentAnimationsData;

        TrackEntry = null;

        TRACK = FindAnimationTrackFromType(Type);

        if (TRACK != GlobalFn.DefaultIntValue()) {
            TrackEntry = CurrentSkeletonGraphic.AnimationState.GetCurrent(TRACK);
        }

        if (TrackEntry != null) {
            TrackEntry.TimeScale = Speed;
        }
    }

    // ****************** WAYS TO CALL SET ANIMATION(S) ********************* //

    // simple animation setting without worry about other params for Spine
    // text file example usage: setCASimple(Catherine,45Front,eye:moving,body:walking)$
    // ViewLayerData is the View Namea nd layer, used to update that layer's data in View for saving purposes
    // Note: emotion and mark loops are false by default
    static bool IS_CHARACTER = false;
    public static void SetAnimationsSimple(
        SkeletonGraphic CurrentSkeletonGraphicToUpdate, AnimationEventManager CurrentAnimationEventManager,
        CharacterData CharacterToUpdate = null, string ViewNameData = "", string LayerTypeData = "",
        string atmosphere = "", string body = "",  string eye = "", string emotion = "", string mouth = "", string mark = "",
        string atmosphere_2 = "", string body_2 = "", string eye_2 = "", string emotion_2 = "", string mouth_2 = "", string mark_2 = "",
        string mark_3 = "",
        float atmosphereSpeed = 1f, float bodySpeed = 1f, float eyeSpeed = 1f, float emotionSpeed = 1f, float mouthSpeed = 1f, float markSpeed = 1f,
        float atmosphereSpeed_2 = 1f, float bodySpeed_2 = 1f, float eyeSpeed_2 = 1f, float emotionSpeed_2 = 1f, float mouthSpeed_2 = 1f, float markSpeed_2 = 1f,
        float markSpeed_3 = 1f,
        bool atmosphereLoop = true, bool bodyLoop = true, bool eyeLoop = true, bool emotionLoop = false, bool mouthLoop = true, bool markLoop = false,
        bool atmosphereLoop_2 = true, bool bodyLoop_2 = true, bool eyeLoop_2 = true, bool emotionLoop_2 = false, bool mouthLoop_2 = true, bool markLoop_2 = false,
        bool markLoop_3 = false,
        int atmosphereLoopCount = 0, int bodyLoopCount = 0, int eyeLoopCount = 0, int emotionLoopCount = 0, int mouthLoopCount = 0, int markLoopCount = 0,
        int atmosphereLoopCount_2 = 0, int bodyLoopCount_2 = 0, int eyeLoopCount_2 = 0, int emotionLoopCount_2 = 0, int mouthLoopCount_2 = 0, int markLoopCount_2 = 0,
        int markLoopCount_3 = 0,

        bool shouldUseMouthFromEmotionTrackInsteadOfMouthTrack = false
     ) {

        if (CharacterToUpdate != null) {
            IsCharacterAnimation = true;
            Character = CharacterToUpdate;
            ViewName = "";
            LayerType = "";
            IS_CHARACTER = true;
        } else {
            IsCharacterAnimation = false;
            Character = null;
            ViewName = ViewNameData;
            LayerType = LayerTypeData;

            IS_CHARACTER = false;
        }

        CurrentSkeletonGraphic = CurrentSkeletonGraphicToUpdate;
        CurrentAnimationEventManager.CurrentSkeletonGraphic = CurrentSkeletonGraphicToUpdate; // had to set this because it contained incorrect one
        AnimationEventManager = CurrentAnimationEventManager;

        if (!string.Equals(atmosphere, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.ATMOSPHERE, atmosphere, atmosphereSpeed, atmosphereLoop, atmosphereLoopCount);
        }
        if (!string.Equals(atmosphere_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.ATMOSPHERE_2, atmosphere_2, atmosphereSpeed_2, atmosphereLoop_2, atmosphereLoopCount_2);
        }

        if (!string.Equals(body, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.BODY, body, bodySpeed, bodyLoop, bodyLoopCount);
        }
        if (!string.Equals(body_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.BODY_2, body_2, bodySpeed_2, bodyLoop_2, bodyLoopCount_2);
        }

        if (!string.Equals(eye, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.EYE, eye, eyeSpeed, eyeLoop, eyeLoopCount);
        }
        if (!string.Equals(eye_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.EYE_2, eye_2, eyeSpeed_2, eyeLoop_2, eyeLoopCount_2);
        }

        if (!string.Equals(emotion, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.EMOTION, emotion, emotionSpeed, emotionLoop, emotionLoopCount);
        }
        if (!string.Equals(emotion_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.EMOTION_2, emotion_2, emotionSpeed_2, emotionLoop_2, emotionLoopCount_2);
        }

        if (!string.Equals(mouth, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.MOUTH, mouth, mouthSpeed, mouthLoop, mouthLoopCount, shouldUseMouthFromEmotionTrackInsteadOfMouthTrack);
        }
        if (!string.Equals(mouth_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.MOUTH_2, mouth_2, mouthSpeed_2, mouthLoop_2, mouthLoopCount_2, shouldUseMouthFromEmotionTrackInsteadOfMouthTrack);
        }

        if (!string.Equals(mark, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.MARK, mark, markSpeed, markLoop, markLoopCount);
        }
        if (!string.Equals(mark_2, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.MARK_2, mark_2, markSpeed_2, markLoop_2, markLoopCount_2);
        }
        if (!string.Equals(mark_3, "")) {
            SetAnimationBasedOnAnimationType(TYPES.ANIMATION.TYPE.MARK_3, mark_3, markSpeed_3, markLoop_3, markLoopCount_3);
        }
    }

    public static void SetAnimation(CharacterData CharacterToUpdate, SkeletonGraphic CharacterSkeletonGraphicToUpdate, AnimationData animationData) {
        CurrentSkeletonGraphic = CharacterSkeletonGraphicToUpdate;

        UpdateSingleAnimation(CurrentSkeletonGraphic.AnimationState, animationData);

        // update the character animation dictionary
        UpdateSingleAnimationInDictionary(animationData);
    }

    // ******************************* UPDATE ******************************* //

    public static void UpdateSingleAnimation(Spine.AnimationState CurrentAnimationState, AnimationData animationData) {
        CurrentAnimationState.AddEmptyAnimation(animationData.Track, 0.2f, 0);

        Spine.TrackEntry animationTrackEntry = CurrentAnimationState.SetAnimation(animationData.Track, animationData.Name, animationData.ShouldLoop);
        animationTrackEntry.TimeScale = animationData.Speed;
    }

    static void UpdateSingleAnimationInDictionary(AnimationData animationData) {
        AnimationDataDictionary[animationData.Track] = animationData;
    }

    public static void StopSingleAnimation(Spine.AnimationState CurrentAnimationState, int Track, float StopSpeed = 1.5f) {
        if (CurrentAnimationState.GetCurrent(Track) == null) return;

        // mouth animations can't stop properly
        // so i've created a var to be passed in when animations are set
        // if after mouth ends should emotion mouth be used
        // or the mouth animation's mouth
        if (Track == MOUTH_TRACK || Track == MOUTH_2_TRACK) return;

        CurrentAnimationState.AddEmptyAnimation(Track, StopSpeed, 0);
    }

    public static void StopSingleAnimation(Spine.AnimationState CurrentAnimationState, string AnimationType) {
        FindAnimationTrackFromType(AnimationType);

        CurrentAnimationState.AddEmptyAnimation(TRACK, 1.5f, 0);
    }

    public static void ResetToDefault(Spine.AnimationState CurrentAnimationState, SkeletonGraphic CurrentSkeletonGraphic = null) {
        CurrentAnimationState.ClearTracks();
        CurrentSkeletonGraphic.Skeleton.SetToSetupPose();
    }

 // ***************************** DATA CLASSES ***************************** //

    public class AnimationData {
        public int Track;
        public string Name;
        public float Speed;
        public bool ShouldLoop;
        public int LoopCount = 0;
    }
}

