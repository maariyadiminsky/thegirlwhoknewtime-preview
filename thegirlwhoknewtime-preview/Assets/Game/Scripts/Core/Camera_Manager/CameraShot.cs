/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CameraShot {
    public static CameraManager _CameraManager;

    public string Name = "";

    [Header("*************Other View To Open/Close At Same Time as This View")]
    public OtherViewDataClass OtherViewData;

    [System.Serializable]
    // useful for another shot that needs to only have one open
    public class OtherViewDataClass {
        // other views to open
        [Header("Views to Open/Close")]
        public GameObject OtherViewToOpen;
        public RectTransform CurrentRect;
        public Vector2 PositionToChangeToOnOpen; // this is view pos

        // make sure to close the open view in the next show
        // if no longer necessary
        public GameObject OtherViewToClose;

        [Header("Layer Or Objects to Open/Close")]
        // when have multiple objects to open for layer
        // Layer, Gameobjects etc.
        public List<GameObject> ObjectsToOpen;
        public List<GameObject> ObjectsToClose;

        // NOTE: if I set any of these changes
        // it will still be reflected in the next shot,
        // so it must be updated again if necessary
        [Header("Changes for Layers In View")]
        public List<LayerPosAndSize> LayerChanges;
    }

    [Serializable]
    public class LayerPosAndSize {
        public LayerManager.LayerTypeEnum LayerTypeEnum = LayerManager.LayerTypeEnum.Background;

        [Header("Layer Image Changes")]
        public Image ImageObject;
        public bool ChangeLayerImage = false;
        public float ImageAlpha = 1f;
        public bool ChangeLayerImageAlpha = false; // need to include this because float is set to 0 by Unity

        [Header("Layer Animation Changes")]
        public SetAnimationAmount ChangeAnimationLayerAmount;
        public GameObject AnimationObject;
        public bool ChangeLayerAnimation = false;
        [NonSerialized] public bool AlreadyChangedAnimationLayer = false;
        public float AnimationAlpha = 1f;
        public bool ChangeLayerAnimationAlpha = false;

        [Header("Pos and Size Changes")]
        public RectTransform LayerTransform;
        public Vector2 LayerPos;
        public Vector2 LayerSize;
        public bool ChangeLayerPos = false;
        public bool ChangeLayerSize = false;

        public bool ShouldMoveSmooth = false; // this will move the image pos smoothly
        public float MoveSpeed = 100f;
        public float SizeSpeed = 10f;
        public bool ShouldWaitUntilStopMoving = false;
    }

    LayerManager.LAYER _Layer;
    public void LayerChangesSetup() {
        if (GameLoad.isLoadingGame) return;

        FindCameraManager();

        if (OtherViewData.ObjectsToOpen != null && OtherViewData.ObjectsToOpen.Count > 0) {
            foreach (GameObject Object in OtherViewData.ObjectsToOpen) Object.SetActive(true);
        }

        if (OtherViewData.ObjectsToClose != null && OtherViewData.ObjectsToClose.Count > 0) {
            foreach (GameObject Object in OtherViewData.ObjectsToClose) Object.SetActive(false);
        }

        if (OtherViewData.LayerChanges != null && OtherViewData.LayerChanges.Count > 0) {
            foreach(LayerPosAndSize LayerData in OtherViewData.LayerChanges) {
                _Layer = null;

                if (LayerData.ShouldMoveSmooth) {
                    _Layer = ViewManager.GetLayerDataForView($"{LayerData.LayerTypeEnum}", ViewData.CurrentlyInThisView.name);
                    _CameraManager.StartCoroutine(_Layer.StartMovingImage(
                        ObjectRectTransform: LayerData.LayerTransform, Position: LayerData.LayerPos,
                        IncomingSize: LayerData.LayerSize, SizeSpeed: LayerData.SizeSpeed,
                        MoveSpeed: LayerData.MoveSpeed, ShouldChangeLayerSize: LayerData.ChangeLayerSize, ShouldChangeLayerPos: LayerData.ChangeLayerPos,
                        ShouldWaitUntilStopMoving: LayerData.ShouldWaitUntilStopMoving));
                } else if (!LayerData.ChangeLayerAnimation && LayerData.ChangeLayerPos) LayerData.LayerTransform.anchoredPosition = LayerData.LayerPos;

                if (!LayerData.ChangeLayerAnimation && !LayerData.ShouldMoveSmooth && LayerData.ChangeLayerSize) LayerData.LayerTransform.localScale = LayerData.LayerSize;

                if (LayerData.ChangeLayerImage || LayerData.ChangeLayerAnimation) {
                    _Layer = _Layer == null ? ViewManager.GetLayerDataForView($"{LayerData.LayerTypeEnum}", ViewData.CurrentlyInThisView.name) : _Layer;

                    if (LayerData.ChangeLayerImage) {
                        _CameraManager.StartCoroutine(_Layer.UpdateLayerImage(ImageName: LayerData.ImageObject.name, ImageSprite: LayerData.ImageObject.sprite, Alpha: LayerData.ImageAlpha, ShouldUpdateImageAlpha: LayerData.ChangeLayerImageAlpha));
                    }

                    if (LayerData.ChangeLayerAnimation &&
                        (LayerData.ChangeAnimationLayerAmount == SetAnimationAmount.EveryTime || (!LayerData.AlreadyChangedAnimationLayer && LayerData.ChangeAnimationLayerAmount == SetAnimationAmount.Once))) {
                        LayerData.AlreadyChangedAnimationLayer = true;
                        _CameraManager.StartCoroutine(_Layer.UpdateLayerAnimation(AnimationName: LayerData.AnimationObject.name, AnimationObject: LayerData.AnimationObject, Alpha: LayerData.AnimationAlpha, ShouldUpdateAnimationAlpha: LayerData.ChangeLayerAnimationAlpha));

                        // had lots of bugs so had to add here
                        if(LayerData.ChangeLayerPos) LayerData.LayerTransform.anchoredPosition = LayerData.LayerPos;
                        if (!LayerData.ShouldMoveSmooth && LayerData.ChangeLayerSize) LayerData.LayerTransform.localScale = LayerData.LayerSize;
                    }
                }
            }
        }

        if (OtherViewData.OtherViewToOpen != null) {
            ViewData.TempDisableMethodsForView(OtherViewData.OtherViewToOpen.name);
            // since on view open anchor positions reset to 0, I will move them manually using this
            // note: and manually have to make sure view is not full size but middle anchor option
            if (OtherViewData.CurrentRect != null && OtherViewData.PositionToChangeToOnOpen != null) {
                OtherViewData.CurrentRect.anchoredPosition = OtherViewData.PositionToChangeToOnOpen;
            }

            OtherViewData.OtherViewToOpen.SetActive(true);
        }

        if (OtherViewData.OtherViewToClose != null) {
            OtherViewData.OtherViewToClose.SetActive(false);
        }
    }

    // some effects will be rendered for entire view, some for only a certain shot
    // like the screen being black can happen for one shot type but not another
    [Header("*************General Effects Called on Shot Start")]
    public List<CameraManager.GeneralEffectEnum> ShotEffects = new List<CameraManager.GeneralEffectEnum>();
    public bool ShouldVignetteBeCompleteClearAtEnd = false;

    // effects that could be enabled at the start of view
    // but I may not want to use this effect for all shots
    // add to ViewEffectsForShot when I want to add more effects only to shot/or remove them
    [Header("*************View Effects For This Shot")]
    public ViewEffectsForShot ViewEffectsForEachShot;

    [Serializable]
    public class ViewEffectsForShot {
        [Header("Effects From View To Turn Off")]
        public bool BlurOff = false; // turns of all blur effects
        public bool BlurDazedEffectOff = false; // turns off only blur dazed effect
        public bool ShakeEffectOff = false;

        [Header("Beautify Effect For Shot Only")]
        // if true, then View will use the Beautify effects from shot, not the view
        public bool UseShotBeautifyMainCameraEffect = false;
        public bool UseShotBeautifyEffectsCameraEffect = false;
        public CameraManager.BeautifyEffectEnums MainCameraBeautifyEffect = CameraManager.BeautifyEffectEnums.Default_Color_Profile;
        public CameraManager.BeautifyEffectEnums EffectsCameraBeautifyEffect = CameraManager.BeautifyEffectEnums.Default_Profile;

        [Header("PP Effect For Shot Only")]
        // if true, then View will use the PP effect from shot, not the view
        public bool UseShotPPEffect = false;
        public CameraManager.PostProcessEffectEnums PostProcessEffect;

        [Header("Camera Shake Effect For Shot Only")]
        public bool UseShotCameraShakeEffect = false;
        public CameraManager.ShakeEffectEnum CameraShakeEffect;

        [Header("Object Shake Effect For Shot Only")]
        public bool UseShotObjectShakeEffect = false;
        public CameraManager.ShakeEffectEnum ObjectShakeEffect;
        public ShakeManager ObjectShakeManager;
        public MilkShake.Shaker ObjectShaker;
        public GameObject ShakeObject;

        [Header("Blur Effect For Shot Only")]
        // if true, then View will use the Blur effect from shot, not the view
        public bool UseShotBlurEffect = false;

        // these are mostly blur effects that use 2 cameras and might be different for each shot
        // or used only once
        [SerializeField] public CameraManager.BlurEffectEnum BlurEffect = CameraManager.BlurEffectEnum.None;
        [SerializeField] public LayerMask LayersToExcludeEffectsCamera = 0; // 1 << 0
        [SerializeField] public LayerMask LayersToExcludeMainCamera = 0; // 1 << 0
        [SerializeField] public CameraManager.BlurEffectEnum BlurAmount = CameraManager.BlurEffectEnum.BlurMid;
        [SerializeField] public float TimeBetweenSwitch = 2f;
        [SerializeField] public float UnBlurSpeed = 1f;
        [SerializeField] public float CustomBlurAmount = 0f;
    }

    [Header("*************Character Data")]
    // Add characters added from view here for easy access later
    public List<CharacterInfoForShot> CharactersRef = new List<CharacterInfoForShot>();

    [Header("*************View Animation Data")]
    public List<ViewAnimationInfo> ViewAnimationsForShot = new List<ViewAnimationInfo>();

    [Serializable]
    public class ViewAnimationInfo {
        public LayerManager.LayerTypeEnum LayerType;

        [Header("Movement And Pos/Size Change Data")]
        public RectTransform AnimationTransform;
        public Vector2 AnimationPosition; // for walking and simply changing
        public Vector2 AnimationSize;
        public float MoveSpeed = 100; // for walking
        public float SizeSpeed = 10;
        public bool ChangePos = false; // for walking
        public bool ChangeSize = false;
        public bool ChangeInstantly = false;
        public bool WaitUntilStopMoving = false;

        [Header("Animation Data On Shot Start")]
        public SetAnimationAmount SetAnimationAmount;
        public List<AnimationInfo> AnimationData = new List<AnimationInfo>();

        [NonSerialized]
        public bool AnimationAlreadySetOnce = false;
    }

    static LayerManager.LAYER LayerResult;
    public void SetupViewAnimations() {
        FindCameraManager();

        if (ViewAnimationsForShot.Count != 0) {
            foreach (ViewAnimationInfo ViewAnimationInfo in ViewAnimationsForShot) {
                if (ViewAnimationInfo.SetAnimationAmount == SetAnimationAmount.EveryTime ||
                    !ViewAnimationInfo.AnimationAlreadySetOnce && (ViewAnimationInfo.SetAnimationAmount == SetAnimationAmount.Once)) {
                    ViewAnimationInfo.AnimationAlreadySetOnce = true;
                    
                    LayerResult = null;
                    LayerResult = ViewData.CurrentlyInThisView.GetLayer($"{ViewAnimationInfo.LayerType}");
                    if (LayerResult != null) {
                        SetAnimations(Layer: LayerResult, Animations: ViewAnimationInfo.AnimationData);

                        if (ViewAnimationInfo.ChangeInstantly) {
                            if (ViewAnimationInfo.ChangePos) ViewAnimationInfo.AnimationTransform.anchoredPosition = ViewAnimationInfo.AnimationPosition;
                            if (ViewAnimationInfo.ChangeSize) ViewAnimationInfo.AnimationTransform.localScale = ViewAnimationInfo.AnimationSize;
                        } else if (!ViewAnimationInfo.ChangeInstantly && ViewAnimationInfo.AnimationTransform != null) { // walk
                            _CameraManager.StartCoroutine(LayerResult.StartMovingAnimation(ObjectRectTransform: ViewAnimationInfo.AnimationTransform, Position: ViewAnimationInfo.AnimationPosition, MoveSpeed: ViewAnimationInfo.MoveSpeed,
                                SizeSpeed: ViewAnimationInfo.SizeSpeed, IncomingSize: ViewAnimationInfo.AnimationSize,
                                ShouldChangeLayerSize: ViewAnimationInfo.ChangeSize, ShouldChangeLayerPos: ViewAnimationInfo.ChangePos,
                                ShouldWaitUntilStopMoving: ViewAnimationInfo.WaitUntilStopMoving
                                ));
                        }
                    }
                }
            }
        }
    }

    [Header("*************Wait Amount")]
    // if true, game won't be able to call the SwitchView action
    public bool WaitUntilComplete = false;
    // if true, game won't be able to call any actions
    public bool WaitUntilCompleteForAllActions = false;

    // if both above are false, then this has the option of running
    // after this amount of time, game can switch view again
    public float WaitUntilTime = 0f;
    // no actions can play until this time is complete
    public float WaitUntilTimeAllActions = 0f;

    [Header("*************What To Ignore")]
    // all camera data will be ignored
    // and all last view's camera data result will be continued in this view
    public bool DontChangeCameraDataResultFromLastView = false;
    // means don't want to change zoom
    public bool DontChangeZoomDataFromLastView = false;
    // means don't want to change pos
    public bool DontChangePosDataFromLastView = false;
    // means don't want to change rotation
    public bool DontChangeRotationDataFromLastView = false;

    public bool ResetCameraData = false;
    // only meaning they reset zoom and nothing else
    public bool ResetZoomDataOnly = false;
    // only meaning they reset pos and nothing else
    public bool ResetPosDataOnly = false;
    // only meaning they reset rotation and nothing else
    public bool ResetRotationDataOnly = true;

    [Header("*************PositionAndZoom Preference")]
    // if both pos and zoom are happening at the same time
    // can choose, otherwise, default will be used
    public CameraManager.PositionZoomType PositionZoomType = CameraManager.PositionZoomType.Default;

    [Header("*************Position Data")]
    public Vector2 Pos = DefaultPos;

    public bool PosSmooth = false;
    public float PosSmoothSpeed = 1f;
    // means start pos from last view
    public Vector2 PosStartFrom = DefaultPos;
    public bool StartPosFromLastView = true;

    [Header("*************Zoom Data")]
    public float Zoom = DefaultZoom;

    public bool ZoomSmooth = false;
    public float ZoomSmoothSpeed = 1f;
    // will zoom until at target zoom or until hit time
    // whichever faster, pos doesn't have this option
    public float ZoomSmoothDuration = 9999f;
    // means start zoom from custom point
    public float ZoomStartFrom = DefaultZoom;
    // means start zoom from last view's zoom point, not custom point
    // can be useful for same view transitions with different layers
    public bool StartZoomFromLastView = true;

    [Header("*************Rotation Data")]
    public Vector3 Rotation = DefaultRotation;
    public bool RotationSmooth = false;
    public float RotationSmoothSpeed = 1f;
    public Vector3 RotationStartFrom = DefaultRotation;
    public bool StartRotationFromLastView = false;

    static readonly Vector2 DefaultPos = new Vector2(0, 0);
    static readonly float DefaultZoom = 5.1f;
    static readonly Vector3 DefaultRotation = new Vector3(0, 0, 0);

    [Header("*************Customized Data")]
    // will get the current rotate and zoom data above
    public bool RotateAndZoomSmooth = false;
    public float RotateAndZoomSmoothDuration = 0f;

    public enum SetAnimationAmount {
        Once,
        EveryTime
    }

    [Serializable]
    public class CharacterInfoForShot {
        public GameObject CharacterObject;
        public CharacterData CharacterData;

        [Header("Hide?")]
        public bool ShouldHideCharacter = false;

        [Header("Character Movement/Size On Shot Start")]
        public MovementOrSizeChange CharacterMovementOrSizeChange;

        [Header("Animation Data On Shot Start")]
        public SetAnimationAmount SetAnimationAmount;
        public List<AnimationInfo> AnimationData = new List<AnimationInfo>();

        [NonSerialized] public bool AnimationAlreadySetOnce = false;
        [NonSerialized] public bool AnimationLoopAlreadySetOnce = false;
    }

    [Serializable]
    public class MovementOrSizeChange {
        public Vector2 MoveTo = new Vector2();
        public bool ShouldMoveInstantly = false; // will move both size and pos instantly
        public float CharacterWalkSpeed = 1f;
        public float PosChangeSpeed = 100f;
        public float SizeChangeSpeed = 1f;
        public Vector2 Size = new Vector2();
        public bool ShouldSizeInstantly = false; // this is for if i have only pos and no size but want to move it instantly

        public bool ShouldChangePos = false;
        public bool ShouldChangeSize = false;
        public bool Is45Back = false;
        // in some cases, if I want to use two animations at the same time
        // some have less glitches/bugs when the other body_2 animation is on top of
        // the walking body animation
        public bool UseBodyAnimationType = false;

        // when changing both pos and size
        // need to choose when coroutine stops
        // default it's when pos gets to it's pos
        // but I can also do it when size gets to it's pos
        public bool ShouldWaitforSizeInsteadOfPos = false;

        // NOTE: might be useful in the future as off
        // but have to deal with the situation of coroutine continuously going...
        // so until i need it, I just have it as true to always wait
        public bool ShouldWaitUntilCharacterStopMoving = true;

        // sometimes walk animation doesn't work with another animation
        // even if walk animation was used on a different body track
        // better to just not play it in this case
        public bool ShouldNotPlayWalkAnimation = false;
    }

    [Serializable]
    public class AnimationInfo {
        public AnimationManager.ANIMATION_TYPE AnimationType;
        public string Name = "";
        public bool ShouldLoop = false;
        public float Speed = 1f;
        public int LoopCount = 0;
        public JumpToAnimation JumpToAnimation;

        // if there are currently animations on character
        // and want some not to appear in different shot
        public bool ShouldStopAnimation = false;
        public float StopSpeed = 0.25f;
    }

    [Serializable]
    public class JumpToAnimation {
        public bool Start = false;
        public bool End = false;
        public bool Custom = true;
        public float CustomTrackTime;
    }

    // easy access from dictionary
    // chaarcters in shot
    public Dictionary<string, CharacterData> Characters = new Dictionary<string, CharacterData>();

    // example Catherine-45Front-Main
    CharacterData CharacterDataResult;
    public CharacterData GetCharacter(string Character) {
        if(Characters.TryGetValue(Character, out CharacterDataResult)) {
            return CharacterDataResult;
        }

        return null;
    }

    bool CharactersAlreadySetup = false;
    [NonSerialized] public bool HasCharacters = true;
    public void SetupCharacterData() {
        // add characters from ref to dictionary for easy access
        if (!CharactersAlreadySetup) {
            if (CharactersRef.Count > 0) {
                HasCharacters = true;

                if (Characters != null) Characters.Clear();
                if (Characters == null) {
                    Characters = new Dictionary<string, CharacterData>();
                }

                foreach (CharacterInfoForShot CharacterInfo in CharactersRef) {
                    if (CharacterInfo.CharacterData == null) continue;
                    Characters.Add(CharacterInfo.CharacterData.FindFullGOName(), CharacterInfo.CharacterData);
                }
            } else {
                HasCharacters = false;
            }

            CharactersAlreadySetup = true;
        }

        if (!GameLoad.isLoadingGame && HasCharacters) {
            ShowCharacters();
        }
    }

    void ShowCharacters() {
        foreach (CharacterInfoForShot CharacterInfoForShot in CharactersRef) {
            CharacterInfoForShot.CharacterObject.SetActive(true);

            if (GameLoad.isLoadingGame) continue;

            // if has animations
            if (CharacterInfoForShot.AnimationData.Count != 0) {
                if (CharacterInfoForShot.SetAnimationAmount == SetAnimationAmount.EveryTime ||
                    !CharacterInfoForShot.AnimationAlreadySetOnce && (CharacterInfoForShot.SetAnimationAmount == SetAnimationAmount.Once)) {
                    CharacterInfoForShot.AnimationAlreadySetOnce = true;
                    SetAnimations(Character: CharacterInfoForShot.CharacterData, Animations: CharacterInfoForShot.AnimationData);
                }
            }

            // see if there is change of movement or size request
            if (CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldChangePos && CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldChangeSize) {
                CharacterInfoForShot.CharacterData.Move(
                    Position: CharacterInfoForShot.CharacterMovementOrSizeChange.MoveTo,
                    CharacterWalkSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.CharacterWalkSpeed,
                    MoveSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.PosChangeSpeed,
                    SizeSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.SizeChangeSpeed,
                    ShouldMoveInstantly: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldMoveInstantly,
                    Size: CharacterInfoForShot.CharacterMovementOrSizeChange.Size,
                    Is45Back: CharacterInfoForShot.CharacterMovementOrSizeChange.Is45Back,
                    UseBodyNotBody_2: CharacterInfoForShot.CharacterMovementOrSizeChange.UseBodyAnimationType,
                    WaitSizeInstead: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldWaitforSizeInsteadOfPos,
                    ShouldWaitUntilStop: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldWaitUntilCharacterStopMoving,
                    ShouldNotPlayWalkAnimation: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldNotPlayWalkAnimation
                 );
            } else if (CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldChangePos) {
                CharacterInfoForShot.CharacterData.Move(
                    Position: CharacterInfoForShot.CharacterMovementOrSizeChange.MoveTo,
                    CharacterWalkSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.CharacterWalkSpeed,
                    MoveSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.PosChangeSpeed,
                    Size: CharacterInfoForShot.CharacterData.FindSize(),
                    ShouldMoveInstantly: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldMoveInstantly,
                    UseBodyNotBody_2: CharacterInfoForShot.CharacterMovementOrSizeChange.UseBodyAnimationType,
                    Is45Back: CharacterInfoForShot.CharacterMovementOrSizeChange.Is45Back,
                    ShouldWaitUntilStop: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldWaitUntilCharacterStopMoving,
                    ShouldNotPlayWalkAnimation: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldNotPlayWalkAnimation
                 );
            } else if (CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldChangeSize) {
                if (CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldMoveInstantly) {
                    CharacterInfoForShot.CharacterData.SetCharacterSize(CharacterInfoForShot.CharacterMovementOrSizeChange.Size);
                } else {
                    FindCameraManager();

                    _CameraManager.StartCoroutine(CharacterInfoForShot.CharacterData.SetCharacterSizesSmooth(
                        IncomingSize: CharacterInfoForShot.CharacterMovementOrSizeChange.Size,
                        Speed: CharacterInfoForShot.CharacterMovementOrSizeChange.SizeChangeSpeed,
                        CharacterWalkSpeed: CharacterInfoForShot.CharacterMovementOrSizeChange.CharacterWalkSpeed,
                        ShouldWaitUntilStop: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldWaitUntilCharacterStopMoving,
                        UseBodyNotBody_2: CharacterInfoForShot.CharacterMovementOrSizeChange.UseBodyAnimationType,
                        Is45Back: CharacterInfoForShot.CharacterMovementOrSizeChange.Is45Back,
                        ShouldNotPlayWalkAnimation: CharacterInfoForShot.CharacterMovementOrSizeChange.ShouldNotPlayWalkAnimation
                    ));
                }
            }

            if (CharacterInfoForShot.ShouldHideCharacter) {
                CharacterInfoForShot.CharacterObject.SetActive(false);
            }
        }
    }

    bool HasAnimation = false;
    string AnimationTypeTemp;
    bool HasCharacter = false;
    bool HasLayer = false;
    public void SetAnimations(List<AnimationInfo> Animations, CharacterData Character = null, LayerManager.LAYER Layer = null) {
        string atmosphere = ""; string body = ""; string eye = ""; string emotion = ""; string mouth = ""; string mark = "";
        string atmosphere_2 = ""; string body_2 = ""; string eye_2 = ""; string emotion_2 = ""; string mouth_2 = ""; string mark_2 = "";
        string mark_3 = "";
        float atmosphereSpeed = 1f; float bodySpeed = 1f; float eyeSpeed = 1f; float emotionSpeed = 1f; float mouthSpeed = 1f; float markSpeed = 1f;
        float atmosphereSpeed_2 = 1f; float bodySpeed_2 = 1f; float eyeSpeed_2 = 1f; float emotionSpeed_2 = 1f; float mouthSpeed_2 = 1f; float markSpeed_2 = 1f;
        float markSpeed_3 = 1f;
        bool atmosphereLoop = true; bool bodyLoop = true; bool eyeLoop = true; bool emotionLoop = false; bool mouthLoop = true; bool markLoop = false;
        bool atmosphereLoop_2 = true; bool bodyLoop_2 = true; bool eyeLoop_2 = true; bool emotionLoop_2 = false; bool mouthLoop_2 = true; bool markLoop_2 = false;
        bool markLoop_3 = false;
        int atmosphereLoopCount = 0; int bodyLoopCount = 0; int eyeLoopCount = 0; int emotionLoopCount = 0; int mouthLoopCount = 0; int markLoopCount = 0;
        int atmosphereLoopCount_2 = 0; int bodyLoopCount_2 = 0; int eyeLoopCount_2 = 0; int emotionLoopCount_2 = 0; int mouthLoopCount_2 = 0; int markLoopCount_2 = 0;
        int markLoopCount_3 = 0;

        HasAnimation = false;
        HasCharacter = Character != null;
        HasLayer = Layer != null;

        foreach (AnimationInfo AnimationInfo in Animations) {
            AnimationTypeTemp = AnimationManager.FindAnimationTypeFromEnum(AnimationInfo.AnimationType);

            if (AnimationInfo.ShouldStopAnimation) {
                if (HasCharacter) Character.StopAnimation(AnimationType: AnimationTypeTemp, StopSpeed: AnimationInfo.StopSpeed);
                if (HasLayer) Layer.StopAnimation(AnimationType: AnimationTypeTemp, Speed: AnimationInfo.StopSpeed);
                continue;
            } else if (AnimationInfo.JumpToAnimation.Start) {
                if (HasCharacter) Character.JumpToAnimationStart(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop);
                if (HasLayer) Layer.JumpToAnimationStart(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop);
                continue;
            } else if (AnimationInfo.JumpToAnimation.End) {
                if (HasCharacter) Character.JumpToAnimationEnd(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop);
                if (HasLayer) Layer.JumpToAnimationEnd(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop);
                continue;
            } else if (AnimationInfo.JumpToAnimation.Custom) {
                if (HasCharacter) Character.JumpToAnimationCustomTrackTime(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop, TrackTime: AnimationInfo.JumpToAnimation.CustomTrackTime);
                if (HasLayer) Layer.JumpToAnimationCustomTrackTime(AnimationName: AnimationInfo.Name, AnimationType: AnimationTypeTemp, ShouldLoop: AnimationInfo.ShouldLoop, TrackTime: AnimationInfo.JumpToAnimation.CustomTrackTime);
                continue;
            }

            switch (AnimationInfo.AnimationType) {
                case AnimationManager.ANIMATION_TYPE.atmosphere:
                    HasAnimation = true;

                    atmosphere = AnimationInfo.Name;
                    atmosphereSpeed = AnimationInfo.Speed;
                    atmosphereLoop = AnimationInfo.ShouldLoop;
                    atmosphereLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.atmosphere_2:
                    HasAnimation = true;

                    atmosphere_2 = AnimationInfo.Name;
                    atmosphereSpeed_2 = AnimationInfo.Speed;
                    atmosphereLoop_2 = AnimationInfo.ShouldLoop;
                    atmosphereLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.body:
                    HasAnimation = true;

                    body = AnimationInfo.Name;
                    bodySpeed = AnimationInfo.Speed;
                    bodyLoop = AnimationInfo.ShouldLoop;
                    bodyLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.body_2:
                    HasAnimation = true;

                    body_2 = AnimationInfo.Name;
                    bodySpeed_2 = AnimationInfo.Speed;
                    bodyLoop_2 = AnimationInfo.ShouldLoop;
                    bodyLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.eye:
                    HasAnimation = true;

                    eye = AnimationInfo.Name;
                    eyeSpeed = AnimationInfo.Speed;
                    eyeLoop = AnimationInfo.ShouldLoop;
                    eyeLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.eye_2:
                    HasAnimation = true;

                    eye_2 = AnimationInfo.Name;
                    eyeSpeed_2 = AnimationInfo.Speed;
                    eyeLoop_2 = AnimationInfo.ShouldLoop;
                    eyeLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.emotion:
                    HasAnimation = true;

                    emotion = AnimationInfo.Name;
                    emotionSpeed = AnimationInfo.Speed;
                    emotionLoop = AnimationInfo.ShouldLoop;
                    emotionLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.emotion_2:
                    HasAnimation = true;

                    emotion_2 = AnimationInfo.Name;
                    emotionSpeed_2 = AnimationInfo.Speed;
                    emotionLoop_2 = AnimationInfo.ShouldLoop;
                    emotionLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.mouth:
                    HasAnimation = true;

                    mouth = AnimationInfo.Name;
                    mouthSpeed = AnimationInfo.Speed;
                    mouthLoop = AnimationInfo.ShouldLoop;
                    mouthLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.mouth_2:
                    HasAnimation = true;

                    mouth_2 = AnimationInfo.Name;
                    mouthSpeed_2 = AnimationInfo.Speed;
                    mouthLoop_2 = AnimationInfo.ShouldLoop;
                    mouthLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.mark:
                    HasAnimation = true;

                    mark = AnimationInfo.Name;
                    markSpeed = AnimationInfo.Speed;
                    markLoop = AnimationInfo.ShouldLoop;
                    markLoopCount = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.mark_2:
                    HasAnimation = true;

                    mark_2 = AnimationInfo.Name;
                    markSpeed_2 = AnimationInfo.Speed;
                    markLoop_2 = AnimationInfo.ShouldLoop;
                    markLoopCount_2 = AnimationInfo.LoopCount;
                    break;
                case AnimationManager.ANIMATION_TYPE.mark_3:
                    HasAnimation = true;

                    mark_3 = AnimationInfo.Name;
                    markSpeed_3 = AnimationInfo.Speed;
                    markLoop_3 = AnimationInfo.ShouldLoop;
                    markLoopCount_3 = AnimationInfo.LoopCount;
                    break;
            }
        }

        if (HasAnimation) {
            if (Character != null) {
                Character.SetupAnimationsSimple(
                    atmosphere, body, eye, emotion, mouth, mark,
                    atmosphere_2, body_2, eye_2, emotion_2, mouth_2, mark_2,
                    mark_3,
                    atmosphereSpeed, bodySpeed, eyeSpeed, emotionSpeed, mouthSpeed, markSpeed,
                    atmosphereSpeed_2, bodySpeed_2, eyeSpeed_2, emotionSpeed_2, mouthSpeed_2, markSpeed_2,
                    markSpeed_3,
                    atmosphereLoop, bodyLoop, eyeLoop, emotionLoop, mouthLoop, markLoop,
                    atmosphereLoop_2, bodyLoop_2, eyeLoop_2, emotionLoop_2, mouthLoop_2, markLoop_2,
                    markLoop_3,
                    atmosphereLoopCount, bodyLoopCount, eyeLoopCount, emotionLoopCount, mouthLoopCount, markLoopCount,
                    atmosphereLoopCount_2, bodyLoopCount_2, eyeLoopCount_2, emotionLoopCount_2, mouthLoopCount_2, markLoopCount_2,
                    markLoopCount_3
                );
            } else if (Layer != null) {
                Layer.SetupAnimationsSimple(
                    atmosphere, body, eye, emotion, mouth, mark,
                    atmosphere_2, body_2, eye_2, emotion_2, mouth_2, mark_2,
                    mark_3,
                    atmosphereSpeed, bodySpeed, eyeSpeed, emotionSpeed, mouthSpeed, markSpeed,
                    atmosphereSpeed_2, bodySpeed_2, eyeSpeed_2, emotionSpeed_2, mouthSpeed_2, markSpeed_2,
                    markSpeed_3,
                    atmosphereLoop, bodyLoop, eyeLoop, emotionLoop, mouthLoop, markLoop,
                    atmosphereLoop_2, bodyLoop_2, eyeLoop_2, emotionLoop_2, mouthLoop_2, markLoop_2,
                    markLoop_3,
                    atmosphereLoopCount, bodyLoopCount, eyeLoopCount, emotionLoopCount, mouthLoopCount, markLoopCount,
                    atmosphereLoopCount_2, bodyLoopCount_2, eyeLoopCount_2, emotionLoopCount_2, mouthLoopCount_2, markLoopCount_2,
                    markLoopCount_3
                );
            }
        }
    }

    public CameraShot(string Name, bool ShouldResetCameraData = false) {
        this.Name = Name;
        this.ResetCameraData = ShouldResetCameraData;
    }

    void FindCameraManager() {
        if (_CameraManager == null) {
            _CameraManager = GameMaster.FindCameraManager();
        }
    }

    void SetStartFromZoom() {
        if (!StartZoomFromLastView) {
            CameraManager.StopZoom = true;

            _CameraManager._Camera.orthographicSize = ZoomStartFrom;
            _CameraManager._EffectsCamera.orthographicSize = ZoomStartFrom;
        }
    }

    void SetStartFromPos() {
        if (!StartPosFromLastView) {
            CameraManager.StopMoving = true;

            _CameraManager._CameraContainer.transform.position = PosStartFrom;
        }
    }

    void SetStartFromRotation() {
        if (!StartRotationFromLastView) {
            CameraManager.StopRotation = true;

            _CameraManager._CameraContainer.transform.rotation = Quaternion.Euler(RotationStartFrom);
        }
    }

    void SetShotEffects() {
        foreach(CameraManager.GeneralEffectEnum CameraShotEffect in ShotEffects) {
            switch(CameraShotEffect) {
                case CameraManager.GeneralEffectEnum.Vignette_Default:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.DEFAULT);
                    break;
                case CameraManager.GeneralEffectEnum.Vignette_Clear:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.CLEAR);
                    break;
                case CameraManager.GeneralEffectEnum.Vignette_Focus_Dark:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.FOCUS_DARK);
                    break;
                case CameraManager.GeneralEffectEnum.Vignette_Complete_Dark:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.COMPLETE_DARK);
                    break;
                case CameraManager.GeneralEffectEnum.Vignette_Blink_Dazed:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.BLINK_DAZED, ShouldBeCompleteClearAtEnd: ShouldVignetteBeCompleteClearAtEnd);
                    break;
                case CameraManager.GeneralEffectEnum.Vignette_Blink_Clear:
                    _CameraManager.Vignette(Type: TYPES.CAMERA_EFFECTS.VIGNETTE_TYPE.BLINK_CLEAR);
                    break;
            }
        }
    }

    public void Setup() {
        // player can still press next for text but view wont switch until zoom/move complete I believe
        GlobalVars.WaitUntilCameraShotComplete = WaitUntilComplete;
        // means player won't be able to go next for text
        GlobalVars.WaitUntilCameraShotCompleteForAllActions = WaitUntilCompleteForAllActions;

        // wait time until view can change this is seperate from zoom/move wait time
        // so if they have an amount it will be zoom time + this time until view can change
        // works best when the two above options are turned off if want to wait for certain time only
        GlobalVars.WaitUntilCameraShotTime = WaitUntilTime;
        // stops all actions until time passed
        // means player won't be able to go next for text
        GlobalVars.WaitUntilCameraShotTimeAllActions = WaitUntilTimeAllActions;
        FindCameraManager();

        // note: this needs to be first so disableallmethods method
        // is enabled before the rest of methods are called here
        LayerChangesSetup();

        SetupCharacterData();

        SetupViewAnimations();

        SetShotEffects();

        // don't change anything
        if (DontChangeCameraDataResultFromLastView) {
            return;
        }
        // reset everything and return
        else if (ResetCameraData) {
            _CameraManager.ResetCameraDataToDefault();
            return;
        }
        // these are useful if I want to reset only one but keep everything else the same
        else if (ResetZoomDataOnly) {
            _CameraManager.ResetCameraZoomToDefault();
            return;
        } else if (ResetPosDataOnly){
            _CameraManager.ResetCameraPosToDefault();
            return;
        } else if  (ResetRotationDataOnly) {
            _CameraManager.ResetCameraRotateToDefault();
            return;
        }

        // call custom data change first
        if (RotateAndZoomSmooth) {
            // note camera shake doesn't with this one so just manually add it in txt file
            // ie CAMERA_SHAKE(TYPE: Shake_Camera_Idle)$

            SetStartFromPos();
            SetStartFromZoom();
            SetStartFromRotation();

            _CameraManager.SetPosition(position: Pos, shouldIgnoreBoundries: true, IsUpdatingWithSmoothMovement: true);

            _CameraManager.RotateAndZoomSmooth(Rotation: Rotation, ZoomAmount: Zoom, ZoomSpeed: ZoomSmoothSpeed, RotationSpeed: RotationSmoothSpeed, Duration: RotateAndZoomSmoothDuration);

            return;
        }
        // if pos, zoom and rotation should be changed
        else if (!DontChangeZoomDataFromLastView && !DontChangePosDataFromLastView && !DontChangeRotationDataFromLastView) {
            SetStartFromRotation();

            if (RotationSmooth) {
                _CameraManager.SetRotateSmooth(Rotation: Rotation, Speed: RotationSmoothSpeed);
            } else {
                _CameraManager.Rotate(Rotation: Rotation);
            }

            ChangePosAndZoom();

            return;
        }
        // if both pos and zoom should be changed but not rotation
        else if (!DontChangeZoomDataFromLastView && !DontChangePosDataFromLastView) {
            ChangePosAndZoom();

            return;
        }

        // if only rotation should be changed
        if (!DontChangeRotationDataFromLastView) {
            SetStartFromRotation();

            if (RotationSmooth) {
                _CameraManager.SetRotateSmooth(Rotation: Rotation, Speed: RotationSmoothSpeed);
            } else {
                _CameraManager.Rotate(Rotation: Rotation);
            }
        }

        // if only zoom should be changed
        if (!DontChangeZoomDataFromLastView) {
            SetStartFromZoom();

            if (ZoomSmooth) {
                _CameraManager.SetZoomSmooth(ZoomAmount: Zoom, ZoomSpeed: ZoomSmoothSpeed, ZoomTime: ZoomSmoothDuration);
            } else {
                _CameraManager.SetZoom(zoomAmount: Zoom); 
            }
        }

        // if only pos should be changed
        if (!DontChangePosDataFromLastView) {
            SetStartFromPos();

            if (PosSmooth) {
                _CameraManager.SetPositionSmooth(position: Pos, duration: PosSmoothSpeed, shouldIgnoreBoundries: true);
            } else {
                _CameraManager.SetPosition(position: Pos, shouldIgnoreBoundries: true);
            }
        }
    }

    void ChangePosAndZoom() {
        SetStartFromZoom();
        SetStartFromPos();

        // if they should both be smooth
        if (ZoomSmooth && PosSmooth) {
            _CameraManager.SetPositionAndZoomSmooth(
                position: Pos, zoomAmount: Zoom,
                zoomSpeed: ZoomSmoothSpeed, ZoomTime: ZoomSmoothDuration, moveDuration: PosSmoothSpeed,
                moveType: PositionZoomType,
                shouldIgnoreBoundries: true
            );
        }
        // if they should both just be set
        else if (!ZoomSmooth && !PosSmooth) {
            _CameraManager.SetPositionAndZoom(position: Pos, zoomAmount: Zoom, shouldIgnoreBoundries: true);
        }
        // otherwise set the noncoroutine one first
        else if (ZoomSmooth && !PosSmooth) {
            _CameraManager.SetPosition(position: Pos, shouldIgnoreBoundries: true, IsUpdatingWithSmoothMovement: true);

            // since IsUpdatingWithSmoothMovement is false here (since the other movement isn't smooth),
            // it will update the global wait vars when zoom is complete
            _CameraManager.SetZoomSmooth(ZoomAmount: Zoom, ZoomSpeed: ZoomSmoothSpeed, ZoomTime: ZoomSmoothDuration);
        } else if (!ZoomSmooth && PosSmooth) {
            _CameraManager.SetZoom(zoomAmount: Zoom, IsUpdatingWithSmoothMovement: true);

            // since IsUpdatingWithSmoothMovement is false here (since the other movement isn't smooth),
            // it will update the global wait vars when movement is complete
            _CameraManager.SetPositionSmooth(position: Pos, duration: PosSmoothSpeed, shouldIgnoreBoundries: true);
        }
    }
}
