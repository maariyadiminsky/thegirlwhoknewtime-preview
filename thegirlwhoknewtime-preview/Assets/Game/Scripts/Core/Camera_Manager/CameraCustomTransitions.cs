/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using BeautifyEffect;

public class CameraCustomTransitions : MonoBehaviour {
    public static CameraCustomTransitions instance;

    static Beautify _Beautify;

    GameObject FirstView;
    GameObject NextView;
    ViewData FirstViewData;
    ViewData NextViewData;

    [SerializeField] float ChangeStrength = 0.006f;
    float TimeToWaitBetweenAlphas = 0.06f;

    FadeEffect FadeEffect;
    CameraManager CameraManager;

    static Camera _Camera;
    static Camera _EffectsCamera;
    static GameObject _CameraContainer;
    static AudioManager _AudioManager;

    static WaitForEndOfFrame Wait;

    void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    IEnumerator Start() {
        FadeEffect = GameMaster.FindFadeEffect();
        CameraManager = GameMaster.FindCameraManager();
        FindAudioManager();

        Wait = GameMaster.FindWait(Wait);
        while (CameraManager == null) yield return Wait;

        if (CameraManager.PostProcessEffects_EffectCamera == null) yield break;

        _Camera = CameraManager._Camera;
        _EffectsCamera = CameraManager._EffectsCamera;
        _CameraContainer = CameraManager._CameraContainer;

        _Beautify = CameraManager.FindCameraBeautify(TYPES.CAMERA_TYPE.MAIN);

        yield break;
    }

    Camera FindMainCamera() {
        return _Camera == null ? CameraManager._Camera : _Camera;
    }

    Camera FindEffectCamera() {
        return _EffectsCamera == null ? CameraManager._EffectsCamera : _EffectsCamera;
    }

    GameObject FindCameraContainer() {
        return _CameraContainer = _CameraContainer == null ? CameraManager._CameraContainer : _CameraContainer;
    }

    void FindAudioManager() {
        if (_AudioManager == null) {
            _AudioManager = GameLoad._AudioManager == null ? new AudioManager() : GameLoad._AudioManager;
        }
    }

    // ====> Other things that need to happen during transition

    // camera size and position changes during transition
    bool HasTransionedHalfway = false;
    Coroutine StartedMidChange = null;
    IEnumerator StartMidChange(
        // general
        string FirstViewName, string NextViewName, string TransitionType,
        // image layer
        string LayerTypeToChange = TYPES.LAYER.BACKGROUND, bool ShouldChangeImageLayer = false, bool ShouldChangeAnimationLayer = false,
        string ImageForLayerName = "", float LayerImageAlpha = 1f, Dictionary<string, float> LayerImagePosition = default, Vector2 LayerImageSize = default,
        bool ShouldUpdateLayerImageAlpha = false, bool ShouldUpdateLayerImagePosition = false, bool ShouldUpdateLayerImageSize = false, bool ShouldApplyEffectsAfterTransition = false,
        // animation layer
        string AnimationName = "", float AnimationAlpha = 1f, Dictionary<string, float> AnimationPosition = default, Vector2 AnimationSize = default,
        bool ShouldUpdateLayerAnimationAlpha = false, bool ShouldUpdateLayerAnimationPosition = false, bool ShouldUpdateLayerAnimationSize = false,
        // sound
        string SoundName = "",
        float Volume = 1f, float Pitch = 1f,
        bool SoundShouldLoop = false, bool SoundShouldFadeIn = true, bool SoundShouldFadeOut = true, float SoundDelay = 0f
    ) {
        // =====> early setup

        NextViewName = string.IsNullOrEmpty(NextViewName) ? ViewManager.GetCurrentActiveView().name : NextViewName;

        LayerManager.LAYER _Layer = null;
        if (ShouldChangeImageLayer || ShouldChangeAnimationLayer) {
            _Layer = ViewManager.GetLayerDataForView(LayerTypeToChange, NextViewName);
        }

        // =====> reset effects related

        while (!HasTransionedHalfway) yield return Wait;

        // =====> sound related

        if (!string.IsNullOrEmpty(SoundName)) {
            if (SoundName.Contains(TYPES.SOUND.GROUP_MUSIC) && !SoundName.Contains(TYPES.SOUND.GROUP_MUSIC_SFX)) {
                // needs to be seperate because can only have one at a time
                _AudioManager.PlayMusic(
                    SoundName: SoundName,
                    volume: Volume, pitch: Pitch,
                    shouldLoop: SoundShouldLoop, shouldFadeInNewMusic: SoundShouldFadeIn, shouldFadeOutCurrentMusic: SoundShouldFadeOut,
                    Delay: SoundDelay
                );
            } else {
                // can have multiple playing at a time
                // even Music SFX plays here
                _AudioManager.PlaySound(
                    SoundName: SoundName,
                    volume: Volume, pitch: Pitch,
                    shouldLoop: SoundShouldLoop, shouldFadeIn: SoundShouldFadeIn,
                    Delay: SoundDelay
                );
            }
        }

        // =====> layer related
        // change image layer
        if (ShouldChangeImageLayer) {
            if (!string.IsNullOrEmpty(ImageForLayerName) && TYPES.LAYER.IsExistingViewLayer(LayerTypeToChange) && _Layer != null) {
                StartCoroutine(_Layer.UpdateLayerImage(ImageForLayerName,
                        Alpha: LayerImageAlpha, Position: LayerImagePosition, Size: LayerImageSize,
                        ShouldUpdateImageAlpha: ShouldUpdateLayerImageAlpha, ShouldUpdateImagePosition: ShouldUpdateLayerImagePosition, ShouldUpdateImageSize: ShouldUpdateLayerImageSize
                    )
                );
            }
        }

        // change animation layer
        if (ShouldChangeAnimationLayer) {
            if (!string.IsNullOrEmpty(AnimationName) && TYPES.LAYER.IsExistingViewLayer(LayerTypeToChange) && _Layer != null) {
                StartCoroutine(_Layer.UpdateLayerAnimation(AnimationName,
                        Alpha: AnimationAlpha, Position: AnimationPosition, Size: AnimationSize,
                        ShouldUpdateAnimationAlpha: ShouldUpdateLayerAnimationAlpha, ShouldUpdateAnimationPosition: ShouldUpdateLayerAnimationPosition, ShouldUpdateAnimationSize: ShouldUpdateLayerAnimationSize
                    )
                );
            }
        }

        HasTransionedHalfway = false;

        if (TransitionType == TYPES.CAMERA_TRANSITION_TYPE.DEFAULT)
            IsTransitioning = false;

        yield break;
    }

    // ====> Start Transition

    public static bool IsTransitioning = false;
    Coroutine Transitioning = null;
    public void Transition(
        GameObject FirstView, GameObject NextView,
        ViewData FirstViewData, ViewData NextViewData,
        string TransitionType = TYPES.CAMERA_TRANSITION_TYPE.DEFAULT, string SpeedType = TYPES.CAMERA_TRANSITION_TYPE.SPEED.MEDIUM,
        bool SkipToNextView = false, // if true, this will skip the unblurring/unsaturation and jump right to the next view
        // layer
        string LayerTypeToChange = TYPES.LAYER.BACKGROUND, bool ShouldChangeImageLayer = false, bool ShouldChangeAnimationLayer = false,
        // image layer
        string ImageForLayerName = "", float ImageLayerAlpha = 1f, Dictionary<string, float> ImageLayerPosition = default, Vector2 ImageLayerSize = default,
        bool ShouldUpdateLayerImageAlpha = false, bool ShouldUpdateLayerImagePosition = false, bool ShouldUpdateLayerImageSize = false, bool ShouldApplyEffectsAfterTransition = false,
        // animation layer
        string AnimationName = "", float AnimationAlpha = 1f, Dictionary<string, float> AnimationPosition = default, Vector2 AnimationSize = default,
        bool ShouldUpdateLayerAnimationAlpha = false, bool ShouldUpdateLayerAnimationPosition = false, bool ShouldUpdateLayerAnimationSize = false,
        // panel layer
        string SoundName = "",
        float Volume = 1f, float Pitch = 1f,
        bool SoundShouldLoop = false, bool SoundShouldFadeIn = true, bool SoundShouldFadeOut = true, float SoundDelay = 0f,
        bool ShouldReset = false
    ) {
        FindAudioManager();
        if (FirstView == null || NextView == null) return;
        this.FirstView = FirstView;
        this.NextView = NextView;
        this.FirstViewData = FirstViewData;
        this.NextViewData = NextViewData;

        ViewData.ShouldReset = ShouldReset;

        if (!TYPES.CAMERA_TRANSITION_TYPE.IsCameraTransitionType(TransitionType)) {
            TransitionType = TYPES.CAMERA_TRANSITION_TYPE.DEFAULT;
        }

        // reset camera pos and zoom
        if (StartedMidChange != null) StopCoroutine(StartedMidChange);

        StartedMidChange = StartCoroutine(StartMidChange(
            FirstViewName: FirstView.name, NextViewName: NextView.name, TransitionType: TransitionType,
            // layer
            LayerTypeToChange: LayerTypeToChange, ShouldChangeImageLayer: ShouldChangeImageLayer, ShouldChangeAnimationLayer: ShouldChangeAnimationLayer, ShouldApplyEffectsAfterTransition: ShouldApplyEffectsAfterTransition,
            // image layer
            ImageForLayerName: ImageForLayerName, LayerImageAlpha: ImageLayerAlpha, LayerImagePosition: ImageLayerPosition, LayerImageSize: ImageLayerSize,
            ShouldUpdateLayerImageAlpha: ShouldUpdateLayerImageAlpha, ShouldUpdateLayerImagePosition: ShouldUpdateLayerImagePosition, ShouldUpdateLayerImageSize: ShouldUpdateLayerImageSize,
            // animation layer
            AnimationName: AnimationName, AnimationAlpha: AnimationAlpha, AnimationPosition: AnimationPosition, AnimationSize: AnimationSize,
            ShouldUpdateLayerAnimationAlpha: ShouldUpdateLayerAnimationAlpha, ShouldUpdateLayerAnimationPosition: ShouldUpdateLayerAnimationPosition, ShouldUpdateLayerAnimationSize: ShouldUpdateLayerAnimationSize,
            // sound
            SoundName: SoundName,
            Volume: Volume, Pitch: Pitch,
            SoundShouldLoop: SoundShouldLoop, SoundShouldFadeIn: SoundShouldFadeIn, SoundShouldFadeOut: SoundShouldFadeOut, SoundDelay: SoundDelay
        ));

        // reset transition
        if (Transitioning != null) StopCoroutine(Transitioning);

        SpeedType = GlobalFn.FirstLetterToUpperCaseOrConvertNullToEmptyString(SpeedType);

        // choose transition
        switch (TransitionType) {
            case TYPES.CAMERA_TRANSITION_TYPE.FADE:
                Transitioning = StartCoroutine(DefaultFadeTransition(FirstView, NextView, SpeedType, TransitionType));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.DISSOLVE:
                Transitioning = StartCoroutine(Dissolve(FirstView, NextView, SpeedType, TransitionType));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.BLUR:
                Transitioning = StartCoroutine(BlurTransition(SpeedType, TransitionType, SkipToNextView: SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION:
                Transitioning = StartCoroutine(BlurTransition(SpeedType, TransitionType, WithSaturation: true, SkipToNextView: SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.BLOOM:
                Transitioning = StartCoroutine(BloomTransition(SpeedType, SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2:
                Transitioning = StartCoroutine(Saturation_2(SpeedType, SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_3:
                //Transitioning = StartCoroutine(BlurTransition(FirstView, NextView, SpeedType, TransitionType, WithSaturation_3: true, SkipToNextView: SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.SATURATION: 
                Transitioning = StartCoroutine(Saturation(SpeedType, TransitionType, SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.WARP:
                Transitioning = StartCoroutine(WarpTransition(SpeedType, TransitionType, SkipToNextView: SkipToNextView));
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.WARP_BLUR:
                Transitioning = StartCoroutine(WarpTransition(SpeedType, TransitionType, WithBlur: true, SkipToNextView: SkipToNextView));
                break;
            default: // simple show and hide with no animation
                IsTransitioning = true;

                SwitchView();
                break;
        }
    }

    void SwitchView() {
        FirstView.SetActive(false);
        HasTransionedHalfway = true;

        if (ViewData.ShouldReset)
            StartCoroutine(StartResetWait());
        else NextView.SetActive(true);
    }

    IEnumerator StartResetWait() {
        // needs to be active for reset to run
        NextView.SetActive(true);
        while (ViewData.IsResetting || ViewData.ShouldReset) yield return Wait;

        yield break;
    }

    // =====> Transition Type
    LensDistortion _LensDistortion = null;
    LensDistortion LensDistortionExample = null;
    PostProcessProfile EffectCameraPostProcessProfile;
    float CurrentBlur;
    float MaxLensDistortionIntensity;
    float MinLensDistortionIntensity;
    float CurrentLensDistortionIntensity;
    float ChangeStrengthWithBlurForWarp;
    IEnumerator WarpTransition(
        string SpeedType, string TransitionType, bool WithBlur = false,
        bool SkipToNextView = false // if true, this will skip the unblurring/unsaturation and jump right to the next view
    ) {
        IsTransitioning = true;

        // reset
        this.FirstViewData.IsFirstViewInWarpEffect = false;
        this.FirstViewData.IsSecondViewInWarpEffect = false;
        this.NextViewData.IsFirstViewInWarpEffect = false;
        this.NextViewData.IsSecondViewInWarpEffect = false;

        // add
        this.FirstViewData.IsFirstViewInWarpEffect = true;
        this.NextViewData.IsSecondViewInWarpEffect = true;

        SetupSpeedType(SpeedType, TransitionType);

        if (EffectCameraPostProcessProfile == null)
            EffectCameraPostProcessProfile = CameraManager.PostProcessEffects_EffectCamera.profile;

        // create the effect(called "setting" in PP) if it doesn't exist on current MainCamera PP
        bool HadLensDistortion = false;
        if (EffectCameraPostProcessProfile.TryGetSettings(out LensDistortionExample)) {
            HadLensDistortion = true;
            _LensDistortion = LensDistortionExample;
        } else {
            _LensDistortion = ScriptableObject.CreateInstance<LensDistortion>();
            EffectCameraPostProcessProfile.AddSettings(_LensDistortion);
        }

        // make sure it exits
        if (_LensDistortion == null) {
            IsTransitioning = false;
            yield break;
        }

        // extra setup if Blur enabled
        _Camera = FindMainCamera();
        if (WithBlur) {
            if (_Camera == null) {
                IsTransitioning = false;
                yield break;
            }

            if (_Beautify == null) {
                IsTransitioning = false;
                yield break;
            }
        }

        DepthOfFieldBeforeEffect = false;
        BokehBeforeEffect = false;
        DepthOfFieldApertureBeforeEffect = 2.8f;
        DepthOfFieldFocalLengthBeforeEffect = 0f;
        MaxBlur = 0f;
        MinBlur = 0f; // so camera goes back to the blur set before this effect
        CurrentBlur = 0f;

        if (WithBlur) {
            DepthOfFieldBeforeEffect = _Beautify.depthOfField;
            BokehBeforeEffect = _Beautify.depthOfFieldBokeh;
            DepthOfFieldApertureBeforeEffect = _Beautify.depthOfFieldAperture;
            DepthOfFieldFocalLengthBeforeEffect = _Beautify.depthOfFieldFocalLength;

            _Beautify.depthOfField = true;
            _Beautify.depthOfFieldBokeh = false;
            _Beautify.depthOfFieldAperture = 0.4f; // just the right amount for intense blur, 2.8 is ok too, but this is better
            _Beautify.depthOfFieldFocalLength = 0f;

            MaxBlur = 0.5f;
            MinBlur = DepthOfFieldFocalLengthBeforeEffect; // so camera goes back to the blur set before this effect
            CurrentBlur = MinBlur;
        }

        // transition
        if(EffectCameraPostProcessProfile.TryGetSettings(out _LensDistortion)) {
            _LensDistortion.enabled.Override(true);

            MaxLensDistortionIntensity = -100f;
            MinLensDistortionIntensity = _LensDistortion.intensity;
            CurrentLensDistortionIntensity = MinLensDistortionIntensity;

            ChangeStrengthWithBlurForWarp = Mathf.Round(ChangeStrength * 100);

            while (CurrentLensDistortionIntensity > MaxLensDistortionIntensity || (WithBlur && CurrentBlur < MaxBlur)) {
                //// Debug.Log($"In Warp... Before CurrentLensDistortionIntensity{CurrentLensDistortionIntensity}, current:{CurrentBlur}, Max:{MaxBlur}, CurrentBlur < MaxBlur: {CurrentBlur < MaxBlur}");
                if (CurrentLensDistortionIntensity > MaxLensDistortionIntensity) {
                    CurrentLensDistortionIntensity -= WithBlur ? ChangeStrengthWithBlurForWarp : ChangeStrength;
                    _LensDistortion.intensity.Override(Mathf.MoveTowards(CurrentLensDistortionIntensity, MaxLensDistortionIntensity, TimeToWaitBetweenAlphas));
                }

                if (WithBlur && CurrentBlur < MaxBlur) {
                    CurrentBlur += ChangeStrength;
                    _Beautify.depthOfFieldFocalLength = Mathf.MoveTowards(CurrentBlur, MaxBlur, TimeToWaitBetweenAlphas);
                }

                yield return Wait;
            }

            SwitchView();

            if (SkipToNextView) {
                _LensDistortion.intensity.Override(MinLensDistortionIntensity);
                if (WithBlur)
                    _Beautify.depthOfFieldFocalLength = MinBlur;
            } else {
                while (CurrentLensDistortionIntensity < MinLensDistortionIntensity || (WithBlur && CurrentBlur > MinBlur)) {
                    if (CurrentLensDistortionIntensity < MinLensDistortionIntensity) {
                        CurrentLensDistortionIntensity += WithBlur ? ChangeStrengthWithBlurForWarp : ChangeStrength;
                        _LensDistortion.intensity.Override(Mathf.MoveTowards(CurrentLensDistortionIntensity, MinLensDistortionIntensity, TimeToWaitBetweenAlphas));
                    }

                    if (WithBlur && CurrentBlur > MinBlur) {
                        CurrentBlur -= ChangeStrength;
                        _Beautify.depthOfFieldFocalLength = Mathf.MoveTowards(CurrentBlur, MinBlur, TimeToWaitBetweenAlphas);
                    }

                    yield return Wait;
                }
            }

            if (!HadLensDistortion) {
                _LensDistortion.enabled.Override(false); // in case for whatever reason remove below fails
                EffectCameraPostProcessProfile.RemoveSettings(_LensDistortion.GetType());

            }
        }

        // Make sure to set blur back to the way it was before effect
        if (WithBlur) {
            _Beautify.depthOfField = DepthOfFieldBeforeEffect;
            _Beautify.depthOfFieldBokeh = BokehBeforeEffect;
            _Beautify.depthOfFieldAperture = DepthOfFieldApertureBeforeEffect;
            _Beautify.depthOfFieldFocalLength = DepthOfFieldFocalLengthBeforeEffect;
        }

        IsTransitioning = false;
        yield break;
    }

    IEnumerator Saturation(
        string SpeedType, string TransitionType,
        bool SkipToNextView = false
    ){

        if (CameraManager == null) yield break;

        _Camera = FindMainCamera();
        if (_Camera == null) yield break;

        IsTransitioning = true;

        SetupSpeedType(SpeedType, TransitionType);

        // Make sure everything is active and enabled
        _Camera.gameObject.SetActive(true);
        _Beautify.enabled = true;

        // simply uses bloom
        // data before transition
        bool BloomBeforeEffect = _Beautify.bloom;
        float BloomIntensityBeforeEffect = _Beautify.bloomIntensity;
        float BloomThresholdBeforeEffect = _Beautify.bloomThreshold;
        float BloomDepthBeforeEffect = _Beautify.bloomDepthAtten;
        // set up bloom
        _Beautify.bloomIntensity = 10f;
        _Beautify.bloomDepthAtten = 0f;
        _Beautify.bloom = true;

        float CurrentBloomThreshold = BloomThresholdBeforeEffect;
        float MaxBloomThreshold = 1.2f;
        float MinBloomThreshold = 0f;

        // saturate
        while(CurrentBloomThreshold >= MinBloomThreshold) {
            CurrentBloomThreshold -= ChangeStrength;
            _Beautify.bloomThreshold = Mathf.MoveTowards(CurrentBloomThreshold, MinBloomThreshold, TimeToWaitBetweenAlphas);

            yield return Wait;
        }

        // Switch views here
        SwitchView();

        // unsaturate
        if (SkipToNextView) {
            _Beautify.bloomIntensity = BloomIntensityBeforeEffect;
            _Beautify.bloomThreshold = BloomThresholdBeforeEffect;
            _Beautify.bloomDepthAtten = BloomDepthBeforeEffect;
            _Beautify.bloom = BloomBeforeEffect;
        } else {
            while (CurrentBloomThreshold <= MaxBloomThreshold) {
                CurrentBloomThreshold += ChangeStrength;
                _Beautify.bloomThreshold = Mathf.MoveTowards(CurrentBloomThreshold, MaxBloomThreshold, TimeToWaitBetweenAlphas);

                yield return Wait;
            }

            _Beautify.bloomIntensity = BloomIntensityBeforeEffect;
            _Beautify.bloomThreshold = BloomThresholdBeforeEffect;
            _Beautify.bloomDepthAtten = BloomDepthBeforeEffect;
            _Beautify.bloom = BloomBeforeEffect;
        }

        IsTransitioning = false;

        yield break;
    }

    float MinContrastBefore;
    float MinBrightnessBefore;
    float CurrentContrastAndBrightness;
    float MaxConstrastAndBrightness;
    IEnumerator Saturation_2(string SpeedType, bool SkipToNextView = false) {
        IsTransitioning = true;

        _Camera = FindMainCamera();

        SetupSpeedType(SpeedType, TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2);

        // assuming here constrast and brightness are the same(they are in Default_color_profile)
        MinContrastBefore = _Beautify.contrast; // default 1 or 1.05
        MinBrightnessBefore = _Beautify.brightness; // default 1 or 1.05
        CurrentContrastAndBrightness = MinContrastBefore;
        MaxConstrastAndBrightness = 1.396f;

        while (CurrentContrastAndBrightness < MaxConstrastAndBrightness) {
            CurrentContrastAndBrightness += ChangeStrength;
            _Beautify.contrast = Mathf.MoveTowards(CurrentContrastAndBrightness, MaxConstrastAndBrightness, TimeToWaitBetweenAlphas);
            _Beautify.brightness = Mathf.MoveTowards(CurrentContrastAndBrightness, MaxConstrastAndBrightness, TimeToWaitBetweenAlphas);

            yield return Wait;
        }

        // Switch views here
        SwitchView();

        if (SkipToNextView) {
            _Beautify.contrast = MinContrastBefore;
            _Beautify.brightness = MinContrastBefore;
        } else {
            while (CurrentContrastAndBrightness > MinContrastBefore) {
                CurrentContrastAndBrightness -= ChangeStrength;
                _Beautify.contrast = Mathf.MoveTowards(CurrentContrastAndBrightness, MinContrastBefore, TimeToWaitBetweenAlphas);
                _Beautify.brightness = Mathf.MoveTowards(CurrentContrastAndBrightness, MinContrastBefore, TimeToWaitBetweenAlphas);

                yield return Wait;
            }
        }

        _Beautify.contrast = MinContrastBefore;
        _Beautify.brightness = MinBrightnessBefore;

        IsTransitioning = false;
        yield break;
    }

    bool BloomBeforeEffect;
    float BloomIntensityBeforeEffect;
    float MinBloomIntensity;
    float MaxBloomIntensity;
    float CurrentBloomIntensity;
    IEnumerator BloomTransition(string SpeedType, bool SkipToNextView = false) {
        _Camera = FindMainCamera();

        IsTransitioning = true;

        SetupSpeedType(SpeedType, TYPES.CAMERA_TRANSITION_TYPE.BLOOM);

        BloomBeforeEffect = _Beautify.bloom;
        BloomIntensityBeforeEffect = _Beautify.bloomIntensity;
        CurrentBloomIntensity = BloomIntensityBeforeEffect;
        MinBloomIntensity = 0;
        MaxBloomIntensity = 0.5f;

        _Beautify.bloom = true;

        while (CurrentBloomIntensity < MaxBloomIntensity) {
            CurrentBloomIntensity += ChangeStrength;
            _Beautify.bloomIntensity = CurrentBloomIntensity;

            yield return Wait;
        }

        // Switch views here
        SwitchView();

        if (!SkipToNextView) {
            while (CurrentBloomIntensity > MinBloomIntensity) {
                CurrentBloomIntensity -= ChangeStrength;
                _Beautify.bloomIntensity = CurrentBloomIntensity;

                yield return Wait;
            }
        }

        _Beautify.bloom = BloomBeforeEffect;
        _Beautify.bloomIntensity = BloomIntensityBeforeEffect;

        IsTransitioning = false;
        yield break;
    }

    bool DepthOfFieldBeforeEffect;
    bool BokehBeforeEffect;
    float DepthOfFieldApertureBeforeEffect;
    float DepthOfFieldFocalLengthBeforeEffect;
    float MaxBlur;
    float MinBlur;

    bool AFEffectBefore = false;
    bool AFVerticalbefore = false;
    float AFMinThreshold = 1f; // 1 makes the brightness invisible
    float AFMaxThreshold = 1f;
    float CurrentThreshold = 1f;
    IEnumerator BlurTransition(
        string SpeedType, string TransitionType,
        bool WithSaturation = false, bool WithSaturation_3 = false,
        bool SkipToNextView = false // if true, this will skip the unblurring/unsaturation and jump right to the next view
    ) {
        _Camera = FindMainCamera();

        IsTransitioning = true;

        SetupSpeedType(SpeedType, TransitionType);

        // Make sure everything is active and enabled
        _Camera.gameObject.SetActive(true);
        _Beautify.enabled = true;

        // Setup
        DepthOfFieldBeforeEffect = _Beautify.depthOfField;
        BokehBeforeEffect = _Beautify.depthOfFieldBokeh;
        DepthOfFieldApertureBeforeEffect = _Beautify.depthOfFieldAperture;
        DepthOfFieldFocalLengthBeforeEffect = _Beautify.depthOfFieldFocalLength;
        _Beautify.depthOfFieldBokeh = false;
        _Beautify.depthOfField = true;
        _Beautify.depthOfFieldAperture = 3.9f; // just the right amount for intense blur, 2.8 is ok too, but this is better
        MaxBlur = WithSaturation_3 ? 0.396f : 0.5f;
        MinBlur = DepthOfFieldFocalLengthBeforeEffect;

        TimeToWaitBetweenAlphas *= Time.deltaTime;

        // =======> Setup if WithSaturation
        // AF stands for Anamorphic Flares
        // NOTE: this assumes AF intensity stays at 10 and AF spread at 2
        AFEffectBefore = false;
        AFVerticalbefore = false;
        AFMinThreshold = 1f; // 1 makes the brightness invisible
        AFMaxThreshold = 1f;
        CurrentThreshold = 1f;

        if (WithSaturation) {
            AFMinThreshold = _Beautify.anamorphicFlaresThreshold;
            CurrentThreshold = AFMinThreshold;
            // store what they were before this effect to reset at the end
            AFEffectBefore = _Beautify.anamorphicFlares;
            AFVerticalbefore = _Beautify.anamorphicFlaresVertical;
            AFMinThreshold = _Beautify.anamorphicFlaresThreshold;

            // turn on flares and make vertical, looks more past-memoryish
            _Beautify.anamorphicFlares = true;
            _Beautify.anamorphicFlaresVertical = false;

            // target
            AFMaxThreshold = 0f; // make brightness visible
        }

        // =======> start transition, blur with optional saturation
        while ((_Beautify.depthOfFieldFocalLength < MaxBlur) ||
            (WithSaturation && CurrentThreshold > AFMaxThreshold)
         ) {
            if (_Beautify.depthOfFieldFocalLength < MaxBlur) {
                _Beautify.depthOfFieldFocalLength += ChangeStrength;
                _Beautify.depthOfFieldFocalLength = Mathf.MoveTowards(_Beautify.depthOfFieldFocalLength, MaxBlur, TimeToWaitBetweenAlphas);
            }

            if (WithSaturation && CurrentThreshold > AFMaxThreshold) {
                CurrentThreshold -= ChangeStrength;
                _Beautify.anamorphicFlaresThreshold = Mathf.MoveTowards(CurrentThreshold, AFMaxThreshold, TimeToWaitBetweenAlphas);
            }
            yield return Wait;
        }

        // Switch views here
        SwitchView();

        // Unblur/UnSaturate
        if (SkipToNextView) {
            _Beautify.depthOfFieldFocalLength = MinBlur;

            if (WithSaturation)
                _Beautify.anamorphicFlaresThreshold = AFMinThreshold;
        } else {
            while ((_Beautify.depthOfFieldFocalLength > MinBlur) ||
                (WithSaturation && CurrentThreshold < AFMinThreshold)
            ) {
                if (_Beautify.depthOfFieldFocalLength > MinBlur) {
                    _Beautify.depthOfFieldFocalLength -= ChangeStrength;
                    _Beautify.depthOfFieldFocalLength = Mathf.MoveTowards(_Beautify.depthOfFieldFocalLength, MinBlur, TimeToWaitBetweenAlphas);
                }

                if (WithSaturation && CurrentThreshold < AFMinThreshold) {
                    CurrentThreshold += ChangeStrength;
                    _Beautify.anamorphicFlaresThreshold = Mathf.MoveTowards(CurrentThreshold, AFMinThreshold, TimeToWaitBetweenAlphas);
                }

                yield return Wait;
            }
        }

        _Beautify.depthOfField = DepthOfFieldBeforeEffect;
        _Beautify.depthOfFieldBokeh = BokehBeforeEffect;
        _Beautify.depthOfFieldAperture = DepthOfFieldApertureBeforeEffect;
        _Beautify.depthOfFieldFocalLength = DepthOfFieldFocalLengthBeforeEffect;

        if (WithSaturation) {
            _Beautify.anamorphicFlares = AFEffectBefore;
            _Beautify.anamorphicFlaresVertical = AFVerticalbefore;
            _Beautify.anamorphicFlaresThreshold = AFMinThreshold;
        }

        IsTransitioning = false;
        yield break;
    }

    Coroutine FadingOut = null;
    Coroutine FadingIn = null;

    // Transition Type: When one fades out completely, the other fades in
    CanvasGroup FirstViewCanvasGroup;
    CanvasGroup NextViewCanvasGroup;
    IEnumerator Dissolve(GameObject FirstView, GameObject NextView, string SpeedType, string TransitionType) {
        IsTransitioning = true;

        SetupSpeedType(SpeedType, TransitionType);

        if (FadingOut != null) StopCoroutine(FadingOut);
        if (FadingIn != null) StopCoroutine(FadingIn);

        FirstViewCanvasGroup = FirstView.GetComponent<CanvasGroup>();
        NextViewCanvasGroup = NextView.GetComponent<CanvasGroup>();
 
        FadingOut = StartCoroutine(FadeEffect.StartFadeOutCG(FirstViewCanvasGroup, _TimeToWaitBetweenAlphas: TimeToWaitBetweenAlphas, _AlphaChangeStrength: ChangeStrength));

        while (FirstViewCanvasGroup.alpha > 0.8f) yield return Wait;

        NextView.SetActive(true);

        HasTransionedHalfway = true;

        FadingIn = StartCoroutine(FadeEffect.StartFadeInCG(NextViewCanvasGroup, _TimeToWaitBetweenAlphas: TimeToWaitBetweenAlphas, _AlphaChangeStrength: ChangeStrength));

        while (FadeEffect.IsFadingIn || FadeEffect.IsFadingOut) yield return Wait;

        FirstView.SetActive(false);

        IsTransitioning = false;
        yield break;
    }

    IEnumerator DefaultFadeTransition(GameObject FirstView, GameObject NextView, string SpeedType, string TransitionType) {
        IsTransitioning = true;

        SetupSpeedType(SpeedType, TransitionType);


        if (FadingOut != null) StopCoroutine(FadingOut);
        if (FadingIn != null) StopCoroutine(FadingIn);

        FadingOut = StartCoroutine(FadeEffect.StartFadeOutCG(FirstView.GetComponent<CanvasGroup>(), _TimeToWaitBetweenAlphas: TimeToWaitBetweenAlphas, _AlphaChangeStrength: ChangeStrength));

        while (FadeEffect.IsFadingOut) yield return Wait;

        NextView.SetActive(true);

        // set this to true so coroutine can end,
        // but I shouldn't combine this transition with a camera and zoom change.
        // It doesn't look good. unless both views have the same position and zoom.
        HasTransionedHalfway = true;

        FadingIn = StartCoroutine(FadeEffect.StartFadeInCG(NextView.GetComponent<CanvasGroup>(), _TimeToWaitBetweenAlphas: TimeToWaitBetweenAlphas, _AlphaChangeStrength: ChangeStrength));

        while (FadeEffect.IsFadingIn || FadeEffect.IsFadingOut) yield return Wait;

        FirstView.SetActive(false);

        IsTransitioning = false;
        yield break;
    }


    // ====> Misc/Setup

    void SetupSpeedType(string SpeedType = TYPES.CAMERA_TRANSITION_TYPE.SPEED.SLOW, string TransitionType = TYPES.CAMERA_TRANSITION_TYPE.FADE) {
        switch(SpeedType) {
            case TYPES.CAMERA_TRANSITION_TYPE.SPEED.FASTER:
                switch(TransitionType) {
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2:
                        ChangeStrength = 0.1f;
                        break;
                }
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.SPEED.FAST:
                switch(TransitionType) {
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP:
                        ChangeStrength = 10f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.SATURATION:
                        ChangeStrength = 0.1f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP_BLUR:
                        ChangeStrength = 0.06f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_3:
                        ChangeStrength = 0.1f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2:
                        ChangeStrength = 0.04f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.FADE:
                    case TYPES.CAMERA_TRANSITION_TYPE.DISSOLVE:
                        ChangeStrength = 0.04f;
                        break;

                }
                TimeToWaitBetweenAlphas = 0.06f;
                break;
            case TYPES.CAMERA_TRANSITION_TYPE.SPEED.MEDIUM:
                switch (TransitionType) {
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP:
                        ChangeStrength = 4f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.SATURATION:
                        ChangeStrength = 0.06f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP_BLUR:
                        ChangeStrength = 0.04f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_3:
                        ChangeStrength = 0.05f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2:
                        ChangeStrength = 0.02f;
                        //ChangeStrength = 0.005f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.FADE:
                    case TYPES.CAMERA_TRANSITION_TYPE.DISSOLVE:
                        ChangeStrength = 0.02f;
                        break;

                }
                TimeToWaitBetweenAlphas = 0.06f;
                break;
            default: // Slow
                switch (TransitionType) {
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP:
                        ChangeStrength = 2f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.SATURATION:
                        ChangeStrength = 0.04f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.WARP_BLUR:
                        ChangeStrength = 0.02f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_3:
                        ChangeStrength = 0.03f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION:
                    case TYPES.CAMERA_TRANSITION_TYPE.BLUR_SATURATION_2:
                        ChangeStrength = 0.003f;
                        break;
                    case TYPES.CAMERA_TRANSITION_TYPE.FADE:
                    case TYPES.CAMERA_TRANSITION_TYPE.DISSOLVE:
                        ChangeStrength = 0.006f;
                        break;

                }
                TimeToWaitBetweenAlphas = 0.06f;
                break;
        }
    }
}
