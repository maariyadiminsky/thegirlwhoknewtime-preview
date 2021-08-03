/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

// this works with SPINE ANIMATION SOFTWARE
// manually used for testing or for animations that don't necessarily depend
// on character or bg assets
public class AnimationSimple : MonoBehaviour {
    [SerializeField] List<AnimationSimpleData> Animations = new List<AnimationSimpleData>();

    void Start() {
        foreach(AnimationSimpleData AnimationData in Animations) {
            PlayAnimation(
                AnimationData.SkeletonAnimation,
                AnimationData.Name,
                AnimationData.Track,
                AnimationData.ShouldLoop,
                AnimationData.Speed
            );
        }
    }

    void PlayAnimation(SkeletonGraphic SkeletonAnimation, string Name, int Track = 0, bool ShouldLoop = true, float Speed = 1f) {
        Spine.TrackEntry animationTrackEntry = SkeletonAnimation.AnimationState.SetAnimation(
            Track,
            Name,
            ShouldLoop
        );

        animationTrackEntry.TimeScale = Speed;
    }

    [System.Serializable]
    public class AnimationSimpleData {
        public SkeletonGraphic SkeletonAnimation;
        public string Name;
        public int Track = 0;
        public bool ShouldLoop = true;
        public float Speed = 1f;
    }
}
