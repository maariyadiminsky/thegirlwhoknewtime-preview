/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class LayerManager : MonoBehaviour {
    public static LayerManager instance;
    static WaitForEndOfFrame Wait;

    public GameObject ImagePrefab;
    public GameObject AnimationPrefab;

    public static bool ShouldWaitUntilLayerStopMoving = false;
    public static bool LayerIsMoving = false;

    public enum LayerTypeEnum {
        Background,
        Midground,
        Foreground,
        Cinematic,
        Background_2,
        Background_3,
        Background_4,
        Midground_2,
        Foreground_2,
        Foreground_3
    }

    void Awake() {
        instance = this;
    }

    void Start() {
        FindWait();
    }

    static void FindWait() {
        if (Wait == null) {
            Wait = GameMaster.FindWait(Wait);
        }
    }

    public class SIZE_AND_ALPHA {
        // used during runtime
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        public float Alpha = 1f;

        // Created so Easy Save 3 can save this. Needs a parameterless constructor.
        public SIZE_AND_ALPHA() { }

        public SIZE_AND_ALPHA(float _ScaleX, float _ScaleY, float _Alpha) {
            ScaleX = _ScaleX;
            ScaleY = _ScaleY;
            Alpha = _Alpha;
        }

        // Setters
        public void UpdateAlpha(float _Alpha) { Alpha = _Alpha; }
        public void UpdateScaleX(float _ScaleX) { ScaleX = _ScaleX; }
        public void UpdateScaleY(float _ScaleY) { ScaleY = _ScaleY; }
        public void UpdateScale(float _ScaleX, float _ScaleY) {
            ScaleX = _ScaleX;
            ScaleY = _ScaleY;
        }

        // Getters
        public Vector2 GetScale() { return new Vector2(ScaleX, ScaleY); }
        public float GetAlpha() { return Alpha; }
    }

    [Serializable]
    public class LAYER_PARTICLE_EFFECT {
        [NonSerialized] public string Name = "";

        public SIZE_AND_ALPHA SizeAndAlpha = new SIZE_AND_ALPHA(_ScaleX: 1f, _ScaleY: 1f, _Alpha: 1f);
        public GameObject ParticleEffectObject = null; // object holding particle effect
        public GameObject ParticleEffect = null; // the actual particle effect
        public ParticleSystemRenderer ParticleSystem = null;
    }

    [Serializable]
    public class LAYER_IMAGE {
        [NonSerialized] public string Name = "";
        public SIZE_AND_ALPHA SizeAndAlpha = new SIZE_AND_ALPHA(_ScaleX: 1.04f, _ScaleY: 1.04f, _Alpha: 1f);

        // For position
        public GameObject ImageObject = null;
        public RectTransform ImageRectTransform;// for moving image transform around
        public Image Image = null;
        [NonSerialized] public Sprite Sprite = null;
    }

    [Serializable]
    public class LAYER_ANIMATION {
        [NonSerialized] public string Name = "";
        [NonSerialized] public string Outfit = "";
        [SerializeField] TYPES.ANIMATION.OUTFIT.Outfit_Enum OutfitEnum;
        public SIZE_AND_ALPHA SizeAndAlpha = new SIZE_AND_ALPHA(_ScaleX: 1.04f, _ScaleY: 1.04f, _Alpha: 1f);

        public GameObject AnimationObject = null; // the empty "Animation" object with the actual animation object(this one has the Spine data)
        public GameObject Animation = null; // the actual animation object(this one has the Spine data)
        public SkeletonGraphic SkeletonGraphic = null;

        // this will not be serialized/not saved
        // todo: remove since not saving anymore?
        [NonSerialized]
        public Dictionary<int, AnimationManager.AnimationData> AnimationDataInView =
            new Dictionary<int, AnimationManager.AnimationData>();

        public void SetOutfitFromEnum() {
            switch (OutfitEnum) {
                case TYPES.ANIMATION.OUTFIT.Outfit_Enum.main_1:
                    Outfit = TYPES.ANIMATION.OUTFIT.MAIN_1_SPINE;
                    break;
                case TYPES.ANIMATION.OUTFIT.Outfit_Enum.main_2:
                    Outfit = TYPES.ANIMATION.OUTFIT.MAIN_2_SPINE;
                    break;
                case TYPES.ANIMATION.OUTFIT.Outfit_Enum.main_3:
                    Outfit = TYPES.ANIMATION.OUTFIT.MAIN_3_SPINE;
                    break;
            }
        }

        public void SetAnimationOutfit() {
            AnimationManager.SetOutfit(SkeletonGraphic, Outfit);
        }
    }

    [Serializable]
    public class LAYER {
        public LayerTypeEnum LayerTypeEnum = LayerTypeEnum.Background;
        [NonSerialized] public string LayerType = "";
        [NonSerialized] public string ParentView = "";

        [NonSerialized] public bool HasSetupAnimationEventManager = false; // does this need to be serialized?
        [NonSerialized] public bool IsShowing = true;

        // Layer Parent
        public GameObject EpisodeLayerPanel;
        public Transform EpisodeLayerPanelTransform;
        public RectTransform EpisodeLayerPanelRectTransform;// for moving layer around
        public SIZE_AND_ALPHA SizeAndAlpha = new SIZE_AND_ALPHA(_ScaleX: 1f, _ScaleY: 1f, _Alpha: 1f);
        public LAYER_IMAGE CurrentLayerImage = null;
        public LAYER_ANIMATION CurrentLayerAnimation = null;
        public LAYER_PARTICLE_EFFECT CurrentLayerParticleEffect = null;

        public void SetLayerTypeFromEnum() {
            LayerType = $"{LayerTypeEnum}";
        }

        int GameLayer;
        public void Setup(string ViewName) {
            SetLayerTypeFromEnum();
            ParentView = ViewName;

            // set correct so post-processing/beautify effects work correctly
            GameLayer = TYPES.GAME_LAYER.FindGameLayerInt(LayerType);

            if (CurrentLayerImage != null) {
                if (CurrentLayerImage.ImageObject == null) return;

                CurrentLayerImage.ImageObject.layer = GameLayer;

                if (CurrentLayerImage.ImageRectTransform == null) {
                    CurrentLayerImage.ImageRectTransform = CurrentLayerImage.ImageObject.GetComponent<RectTransform>();
                }

                if (CurrentLayerImage.Image == null) {
                    CurrentLayerImage.ImageObject.GetComponent<Image>();
                }

                CurrentLayerImage.Sprite = CurrentLayerImage.Image.sprite;
                CurrentLayerImage.Name = CurrentLayerImage.Sprite.name;

            }

            if (CurrentLayerAnimation != null) {
                if (CurrentLayerAnimation.AnimationObject == null) return;

                CurrentLayerAnimation.AnimationObject.layer = GameLayer;

                if (CurrentLayerAnimation.Animation == null) {
                    CurrentLayerAnimation.Animation = CurrentLayerAnimation.AnimationObject.transform.GetChild(0).gameObject;
                }

                CurrentLayerAnimation.Name = CurrentLayerAnimation.Animation.name;
                CurrentLayerAnimation.Animation.layer = GameLayer;

                if (CurrentLayerAnimation.SkeletonGraphic == null) {
                    CurrentLayerAnimation.SkeletonGraphic = CurrentLayerAnimation.Animation.GetComponent<SkeletonGraphic>();
                }
                CurrentLayerAnimation.SetOutfitFromEnum();
                CurrentLayerAnimation.SetAnimationOutfit();
            }

            if (CurrentLayerParticleEffect != null) {
                if (CurrentLayerParticleEffect.ParticleEffectObject == null) return;

                CurrentLayerParticleEffect.ParticleEffectObject.layer = GameLayer;

                if (CurrentLayerParticleEffect.ParticleEffect == null) {
                    CurrentLayerParticleEffect.ParticleEffect = CurrentLayerParticleEffect.ParticleEffectObject.transform.GetChild(0).gameObject;
                }

                CurrentLayerParticleEffect.Name = CurrentLayerParticleEffect.ParticleEffect.name;
                CurrentLayerParticleEffect.ParticleEffect.layer = GameLayer;

                if (CurrentLayerParticleEffect.ParticleSystem == null) {
                    CurrentLayerParticleEffect.ParticleSystem = CurrentLayerParticleEffect.ParticleEffect.GetComponent<ParticleSystemRenderer>();
                }

                CurrentLayerParticleEffect.ParticleSystem.sortingLayerName = LayerType;
            }
        }

        // =====> Image/Layer position

        // either pass in dictionary with multiple values or a single value to update
        public void ChangeEpisodeLayerPanelOrImagePosition(
            float SingleValue = TYPES.NULL_REPLACEMENT.FLOAT,
            string SingleValueType = TYPES.NULL_REPLACEMENT.STRING,
            Dictionary<string, float> MultipleValuesToUpdate = null,
            bool IsForImageOnly = false // either update entire panel or just the image
        ) {

            if (IsForImageOnly && CurrentLayerImage == null) {
                return;
            }

            if (MultipleValuesToUpdate != null && MultipleValuesToUpdate.Count > 0) {
                foreach (KeyValuePair<string, float> ValueDetails in MultipleValuesToUpdate) {
                    ChangeSinglePartOnEpisodeLayerPanelOrImagePosition(ValueDetails.Key, ValueDetails.Value, IsForImageOnly: IsForImageOnly);
                }
            } else if (SingleValue != TYPES.NULL_REPLACEMENT.FLOAT && !string.IsNullOrEmpty(SingleValueType) && SingleValueType != TYPES.NULL_REPLACEMENT.STRING) {
                ChangeSinglePartOnEpisodeLayerPanelOrImagePosition(SingleValueType, SingleValue, IsForImageOnly: IsForImageOnly);
            }

        }

        void ChangeSinglePartOnEpisodeLayerPanelOrImagePosition(string Type, float Amount, bool IsForImageOnly = false) {
            switch (Type) {
                case TYPES.OFFSETS.TOP:
                    if (IsForImageOnly) {
                        CurrentLayerImage.ImageRectTransform.SetTop(Amount);
                    } else {
                        EpisodeLayerPanelRectTransform.SetTop(Amount);
                    }
                    break;
                case TYPES.OFFSETS.BOTTOM:
                    if (IsForImageOnly) {
                        CurrentLayerImage.ImageRectTransform.SetBottom(Amount);
                    } else {
                        EpisodeLayerPanelRectTransform.SetBottom(Amount);
                    }
                    break;
                case TYPES.OFFSETS.LEFT:
                    if (IsForImageOnly) {
                        CurrentLayerImage.ImageRectTransform.SetLeft(Amount);
                    } else {
                        EpisodeLayerPanelRectTransform.SetLeft(Amount);
                    }
                    break;
                case TYPES.OFFSETS.RIGHT:
                    if (IsForImageOnly) {
                        CurrentLayerImage.ImageRectTransform.SetRight(Amount);
                    } else {
                        EpisodeLayerPanelRectTransform.SetRight(Amount);
                    }
                    break;
            }
        }

        public IEnumerator StartMovingImage(RectTransform ObjectRectTransform, Vector2 Position, float MoveSpeed = 100f, float SizeSpeed = 10f, Vector2 IncomingSize = new Vector2(), bool ShouldChangeLayerSize = false, bool ShouldChangeLayerPos = false, bool ShouldWaitUntilStopMoving = false) {
            LayerIsMoving = true;
            ShouldWaitUntilLayerStopMoving = ShouldWaitUntilStopMoving;
            MoveSpeed *= Time.deltaTime;
            SizeSpeed *= Time.deltaTime;

            FindWait();

            if (CurrentLayerImage == null || CurrentLayerImage.ImageObject == null) {
                LayerIsMoving = false;
                LayerManager.ShouldWaitUntilLayerStopMoving = false;
                yield break;
            }

            while (GlobalFn.IsDistanceBetweenTwoVectorsGreaterThanZero(ObjectRectTransform.anchoredPosition, Position)) {
                if (ShouldChangeLayerPos && GlobalFn.IsDistanceBetweenTwoVectorsGreaterThanZero(ObjectRectTransform.anchoredPosition, Position)) {
                    ObjectRectTransform.anchoredPosition = Vector2.MoveTowards(ObjectRectTransform.anchoredPosition, Position, MoveSpeed);
                }

                if (ShouldChangeLayerSize && !GlobalFn.IsTwoVector3Equal(ObjectRectTransform.localScale, IncomingSize)) {
                    ObjectRectTransform.localScale = Vector2.MoveTowards(ObjectRectTransform.localScale, IncomingSize, SizeSpeed);
                }
                yield return Wait;
            }

            if (ShouldChangeLayerPos) CurrentLayerImage.ImageObject.GetComponent<RectTransform>().anchoredPosition = Position;
            if(ShouldChangeLayerSize) CurrentLayerImage.ImageObject.GetComponent<RectTransform>().localScale = IncomingSize;

            LayerIsMoving = false;
            LayerManager.ShouldWaitUntilLayerStopMoving = false;
            yield break;
        }

        // ************************** ANIMATION ************************** //
        public IEnumerator StartMovingAnimation(RectTransform ObjectRectTransform, Vector2 Position, float MoveSpeed = 100f, float SizeSpeed = 10f, Vector2 IncomingSize = new Vector2(), bool ShouldChangeLayerSize = false, bool ShouldChangeLayerPos = false, bool ShouldWaitUntilStopMoving = false) {
            LayerIsMoving = true;
            ShouldWaitUntilLayerStopMoving = ShouldWaitUntilStopMoving;
            MoveSpeed *= Time.deltaTime;
            SizeSpeed *= Time.deltaTime;

            FindWait();

            if (CurrentLayerAnimation == null || CurrentLayerAnimation.Animation == null) {
                LayerIsMoving = false;
                LayerManager.ShouldWaitUntilLayerStopMoving = false;
                yield break;
            }

            while (GlobalFn.IsDistanceBetweenTwoVectorsGreaterThanZero(ObjectRectTransform.anchoredPosition, Position)) {
                if (ShouldChangeLayerPos && GlobalFn.IsDistanceBetweenTwoVectorsGreaterThanZero(ObjectRectTransform.anchoredPosition, Position)) {
                    ObjectRectTransform.anchoredPosition = Vector2.MoveTowards(ObjectRectTransform.anchoredPosition, Position, MoveSpeed);
                }

                if (ShouldChangeLayerSize && !GlobalFn.IsTwoVector3Equal(ObjectRectTransform.localScale, IncomingSize)) {
                    ObjectRectTransform.localScale = Vector2.MoveTowards(ObjectRectTransform.localScale, IncomingSize, SizeSpeed);
                }

                yield return Wait;
            }

            if (ShouldChangeLayerPos) CurrentLayerAnimation.AnimationObject.GetComponent<RectTransform>().anchoredPosition = Position;
            if (ShouldChangeLayerSize) CurrentLayerAnimation.AnimationObject.GetComponent<RectTransform>().localScale = IncomingSize;

            LayerIsMoving = false;
            LayerManager.ShouldWaitUntilLayerStopMoving = false;
            yield break;
        }

        public void ChangeAnimationPosition(Vector2 Position) {
            if (CurrentLayerAnimation == null) {
                return;
            }

            CurrentLayerAnimation.AnimationObject.GetComponent<RectTransform>().anchoredPosition = Position;
        }

        public Vector2 GetAnimationPosition() {
            return CurrentLayerAnimation.AnimationObject.GetComponent<RectTransform>().anchoredPosition;
        }

        public string SetAnimationAssetPath(string Episode, string Value) {
            string SecondValue = Value.Contains("/") ? Value.Split('/')[Value.Split('/').Length - 1] : Value;

            return $"Backgrounds/{Episode}/Animation/{Value}/Prefab/{SecondValue}";
        }

        public void UpdateLayerAnimationData(Dictionary<int, AnimationManager.AnimationData> AnimationData) {
            if (CurrentLayerAnimation != null && CurrentLayerAnimation.AnimationDataInView != null) {
                CurrentLayerAnimation.AnimationDataInView = AnimationData;
            }
        }

        AnimationEventManager AnimationEventManager = AnimationEventManager.instance;
        void SetupEventManager() {
            AnimationEventManager.SubscribeToAnimationEvents(
                CurrentLayerAnimation.SkeletonGraphic, ViewNameData: ParentView, LayerTypeData: LayerType, ShouldStopAtLastFrame: false
            );
            HasSetupAnimationEventManager = true;
        }

        public Dictionary<int, AnimationManager.AnimationData> GetAnimationData() {
            if (CurrentLayerAnimation != null) {
                return CurrentLayerAnimation.AnimationDataInView;
            } else return null;
        }

        public void SetOutfit(string outfit) {
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null || string.IsNullOrEmpty(outfit)) return;

            CurrentLayerAnimation.Outfit = outfit;
            AnimationManager.SetOutfit(CurrentLayerAnimation.SkeletonGraphic, outfit);
        }

        public string GetOutfit() {
            return CurrentLayerAnimation.Outfit;
        }

        // name of animation, type: ie body, body_2 etc.
        int TrackIndex;
        public void JumpToAnimationStart(string AnimationName, string AnimationType, bool ShouldLoop = true) {
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null) return;

            TrackIndex = 0;
            TrackIndex = AnimationManager.FindAnimationTrackFromType(AnimationType);
            if (TrackIndex == TYPES.NULL_REPLACEMENT.NUM) return;

            AnimationManager.JumpToAnimationStart(Name: AnimationName, Type: AnimationType, AnimationSkeletonGraphic: CurrentLayerAnimation.SkeletonGraphic, Track: TrackIndex, ShouldLoop);
        }

        public void JumpToAnimationEnd(string AnimationName, string AnimationType, bool ShouldLoop = true) {
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null) return;

            TrackIndex = 0;
            TrackIndex = AnimationManager.FindAnimationTrackFromType(AnimationType);
            if (TrackIndex == TYPES.NULL_REPLACEMENT.NUM) return;

            AnimationManager.JumpToAnimationEnd(Name: AnimationName, Type: AnimationType, AnimationSkeletonGraphic: CurrentLayerAnimation.SkeletonGraphic, Track: TrackIndex, ShouldLoop);
        }

        // which frame to stop at
        public void JumpToAnimationCustomTrackTime(string AnimationName, string AnimationType, bool ShouldLoop = true, float TrackTime = 0f) {
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null) return;

            TrackIndex = 0;
            TrackIndex = AnimationManager.FindAnimationTrackFromType(AnimationType);
            if (TrackIndex == TYPES.NULL_REPLACEMENT.NUM) return;

            AnimationManager.JumpToAnimationCustomTrackTime(Name: AnimationName, Type: AnimationType, AnimationSkeletonGraphic: CurrentLayerAnimation.SkeletonGraphic, Track: TrackIndex, ShouldLoop, CustomTrackTime: TrackTime);
        }

        // Note: emotion and mark loops are false by default, since most of the time they don't need to loop
        // Note: this is important since loop count animatons won't work properly if event manager is still set up to old animation
        // only useful if a layer has switching animations(especially important for switch layer animations)
        public void SetupAnimationsSimple(
            string atmosphere = "", string body = "", string eye = "", string emotion = "", string mouth = "", string mark = "",
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
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null) return;

            if (!HasSetupAnimationEventManager) {
                SetupEventManager();
            }

            AnimationManager.SetAnimationsSimple(
                CurrentSkeletonGraphicToUpdate: CurrentLayerAnimation.SkeletonGraphic,
                CurrentAnimationEventManager: AnimationEventManager,
                CharacterToUpdate: null,
                ViewNameData: ParentView, LayerTypeData: LayerType,
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
                markLoopCount_3,
                shouldUseMouthFromEmotionTrackInsteadOfMouthTrack
            );
        }

        public void StopAnimation(string AnimationType, float Speed = 0.25f) {
            if (CurrentLayerAnimation == null || CurrentLayerAnimation.SkeletonGraphic == null) return;

            TrackIndex = 0;
            TrackIndex = AnimationManager.FindAnimationTrackFromType(AnimationType);
            if (TrackIndex == TYPES.NULL_REPLACEMENT.NUM) return;

            AnimationManager.StopSingleAnimation(CurrentLayerAnimation.SkeletonGraphic.AnimationState, TrackIndex, Speed);
        }

        // ************************** IMAGE ************************** //

        public string SetImageAssetPath(string Episode, string Value) {
            return $"Backgrounds/{Episode}/Backgrounds/{Value}";
        }

        public void ChangeLayerPanelSize(Vector2 Size) {
            SizeAndAlpha.UpdateScale(_ScaleX: Size.x, _ScaleY: Size.y);
            EpisodeLayerPanelTransform.localScale = Size;
        }

        public void ChangeLayerImageSize(Vector2 Size) {
            if (CurrentLayerImage == null) {
                return;
            }

            CurrentLayerImage.SizeAndAlpha.UpdateScale(_ScaleX: Size.x, _ScaleY: Size.y);
            CurrentLayerImage.ImageObject.transform.localScale = CurrentLayerImage.SizeAndAlpha.GetScale();
        }

        public void ChangeLayerAnimationSize(Vector2 Size) {
            if (CurrentLayerAnimation == null) {
                return;
            }

            CurrentLayerAnimation.SizeAndAlpha.UpdateScale(_ScaleX: Size.x, _ScaleY: Size.y);
            CurrentLayerAnimation.AnimationObject.transform.localScale = CurrentLayerAnimation.SizeAndAlpha.GetScale();
        }

        // if adding data to layer during runtime
        int NewGameLayer;
        public void LayerImageSetup(
            Transform ViewTransform, string LayerTypeData,
            LAYER_IMAGE ImageData = null, Transform LayerTransform = null
         ) {
            ParentView = ViewTransform.name;
            LayerType = GlobalFn.FirstLetterToUpperCaseOrConvertNullToEmptyString(LayerTypeData);
            NewGameLayer = TYPES.GAME_LAYER.FindGameLayerInt(LayerType);
            EpisodeLayerPanelTransform = LayerTransform == null ? ViewTransform.Find($"Episode{LayerType}Panel"): LayerTransform;

            EpisodeLayerPanel = EpisodeLayerPanelTransform.gameObject as GameObject;
            EpisodeLayerPanelRectTransform = EpisodeLayerPanel.GetComponent<RectTransform>();

            // check if transform for what I'm creating already exists, otherwise create it
            Transform ReferenceTransform = EpisodeLayerPanelTransform.Find(TYPES.PREFAB_TYPE.IMAGE);
            GameObject Reference;
            if (ReferenceTransform == null) {
                Reference = Instantiate(instance.ImagePrefab, EpisodeLayerPanelTransform) as GameObject;
            } else Reference = ReferenceTransform.gameObject;

            if (!Reference.activeInHierarchy) Reference.SetActive(true);
            // Make sure to set layer so post-processing/beautify effects work correctly
            Reference.layer = NewGameLayer;

            Image ImageComponent = Reference.GetComponent<Image>();
            ImageData.Image = ImageComponent;
            ImageData.ImageObject = Reference;
            ImageData.ImageRectTransform = ImageData.ImageObject.GetComponent<RectTransform>();

            ImageData.ImageObject.SetActive(false);
            EpisodeLayerPanel.SetActive(false);

            if (ImageData != null) {
                CurrentLayerImage = ImageData;
                UpdateImageAlpha(ImageData.SizeAndAlpha.GetAlpha());
            }
        }

        // if adding data to layer during runtime
        public void LayerAnimationSetup(
            Transform ViewTransform, string LayerTypeData,
            LAYER_ANIMATION AnimationData = null, Transform LayerTransform = null
        ) {
            ParentView = ViewTransform.name;
            LayerType = GlobalFn.FirstLetterToUpperCaseOrConvertNullToEmptyString(LayerTypeData);
            NewGameLayer = TYPES.GAME_LAYER.FindGameLayerInt(LayerType);
            EpisodeLayerPanelTransform = LayerTransform == null ? ViewTransform.Find($"Episode{LayerType}Panel") : LayerTransform;

            EpisodeLayerPanel = EpisodeLayerPanelTransform.gameObject as GameObject;
            EpisodeLayerPanelRectTransform = EpisodeLayerPanel.GetComponent<RectTransform>();

            // check if transform for what I'm creating already exists, otherwise create it
            Transform ReferenceTransform = EpisodeLayerPanelTransform.Find(TYPES.PREFAB_TYPE.ANIMATION);
            GameObject Reference;
            if (ReferenceTransform == null) {
                Reference = Instantiate(instance.AnimationPrefab, EpisodeLayerPanelTransform) as GameObject;
            } else Reference = ReferenceTransform.gameObject;

            if (!Reference.activeInHierarchy) Reference.SetActive(true);

            // Make sure to set layer so post-processing/beautify effects work correctly
            Reference.layer = NewGameLayer; // empty animation object
            AnimationData.Animation.layer = NewGameLayer; // animation

            // Attach instance of animation prefab to animation object
            if (AnimationData.Animation != null & Reference.transform != null) {
                AnimationData.Animation = Instantiate(AnimationData.Animation, Reference.transform); // attach animation to empty animation object
                AnimationData.AnimationObject = Reference; // empty animation object
                AnimationData.SkeletonGraphic = AnimationData.AnimationObject.transform.GetChild(0).GetComponent<SkeletonGraphic>();
            }

            AnimationData.AnimationObject.SetActive(false);
            EpisodeLayerPanel.SetActive(false);

            if (AnimationData != null)
                CurrentLayerAnimation = AnimationData;
        }

                // created for updating the layer in view during runtime, within txt file
        LAYER_ANIMATION LayerAnimationData;
        bool CurrentLayerAnimationDataExists;
        GameObject LayerAnimationGO;
        public IEnumerator UpdateLayerAnimation(
            string AnimationName,
            float Alpha = 1f, Dictionary<string, float> Position = default, Vector2 Size = default,
            bool ShouldUpdateAnimationAlpha = false, bool ShouldUpdateAnimationPosition = false, bool ShouldUpdateAnimationSize = false,
            GameObject AnimationObject = null
        ) {
            CurrentLayerAnimationDataExists = CurrentLayerAnimation != null;
            // make sure current layer image exists, if not create one
            LayerAnimationData = CurrentLayerAnimationDataExists ? CurrentLayerAnimation : new LAYER_ANIMATION();

            // if there is a new layer animation, then the animation event manager
            // needs to update, other loop counts won't work
            HasSetupAnimationEventManager = false;

            // get sprite
            LayerAnimationGO = AnimationObject == null ? ViewManager.GetViewLayerAnimation(ParentView, AnimationName) : AnimationObject;
            while (LayerAnimationGO == null) yield return Wait;

            ViewManager.HideAnimationLoadLaterAssets();

            // if there is currently an animation in this layer, move it back to AssetsToLoadLater pool
            // Note: This will only work(animation moving) if I add the animation object, etc in the LayersInfo of the ViewData
            if (CurrentLayerAnimationDataExists && LayerAnimationData.Animation != null) {
                LayerAnimationData.Animation.transform.SetParent(ViewManager.CurrentViewAssetToLoadLaterObject);
            }

            // set new animation object, skeleton object and animation name
            LayerAnimationData.Animation = LayerAnimationGO;
            LayerAnimationData.SkeletonGraphic = LayerAnimationGO.GetComponent<SkeletonGraphic>();
            LayerAnimationData.Name = AnimationName;
            LayerAnimationGO.transform.SetParent(LayerAnimationData.AnimationObject.transform);

            if (ShouldUpdateAnimationAlpha) { UpdateAnimationAlpha(Alpha); }

            // for animations i dont change animation directly but panel,
            // since panel is changed for position as well
            if (ShouldUpdateAnimationSize) { ChangeLayerPanelSize(Size); }

            if (ShouldUpdateAnimationPosition) { ChangeEpisodeLayerPanelOrImagePosition(MultipleValuesToUpdate: Position, IsForImageOnly:false); }

            yield break;
        }

        // created for updating the layer in view during runtime, within txt file
        LAYER_IMAGE LayerImageData;
        bool CurrentLayerImageDataExists;
        Sprite LayerSprite;
        public IEnumerator UpdateLayerImage(
            string ImageName,
            float Alpha = 1f, Dictionary<string, float> Position = default, Vector2 Size = default,
            bool ShouldUpdateImageAlpha = false, bool ShouldUpdateImagePosition = false, bool ShouldUpdateImageSize = false,
            Sprite ImageSprite = null
        ) {
            CurrentLayerImageDataExists = CurrentLayerImage != null;
            // make sure current layer image exists, if not create one
            LayerImageData = CurrentLayerImageDataExists ? CurrentLayerImage : new LAYER_IMAGE();

            // get sprite
            // either the Image was passed in or the name of the image so I can find it.
            LayerSprite = ImageSprite == null ? ViewManager.GetViewLayerSprite(ParentView, ImageName) : ImageSprite;
            yield return GlobalFn.WaitTry(LayerSprite);

            ViewManager.HideBackgroundLoadLaterAssets();

            // set sprite and image name
            LayerImageData.Image.sprite = LayerSprite;
            LayerImageData.Sprite = LayerSprite;
            LayerImageData.Name = ImageName;

            // if image data doesn't exist yet, set references
            // no need to yield for this so system can do other things, unless needs alpha change too
            if (!CurrentLayerImageDataExists) {
                LayerImageSetup(EpisodeLayerPanelTransform.root, LayerType, ImageData: LayerImageData);

                if (ShouldUpdateImageAlpha || ShouldUpdateImagePosition || ShouldUpdateImageSize) {
                    while (CurrentLayerImage == null || CurrentLayerImage.Image == null) yield return Wait;
                }
            }

            if (ShouldUpdateImageAlpha) { UpdateAnimationAlpha(Alpha); }

            if (ShouldUpdateImageSize) { ChangeLayerImageSize(Size); }

            if (ShouldUpdateImagePosition) { ChangeEpisodeLayerPanelOrImagePosition(MultipleValuesToUpdate: Position, IsForImageOnly:true); }

            yield break;
        }

        public void UpdateImageAlpha(float Alpha = 1f) {
            if (CurrentLayerImage == null && CurrentLayerImage.Image == null) return;

            var color = CurrentLayerImage.Image.color;
            color.a = Alpha;
            CurrentLayerImage.Image.color = color;

            CurrentLayerImage.SizeAndAlpha.UpdateAlpha(Alpha);
        }

        public void UpdateAnimationAlpha(float Alpha = 1f) {
            AnimationManager.SetAlpha(CurrentLayerAnimation.SkeletonGraphic, Alpha);
        }

        public void ToggleLayer(bool toggle) {
            if (EpisodeLayerPanel != null) {
                EpisodeLayerPanel.SetActive(toggle);
                IsShowing = toggle;
            }
        }
    }
}
