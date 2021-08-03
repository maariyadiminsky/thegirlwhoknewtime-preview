/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Doozy.Engine.Soundy;

public class AudioSimple : MonoBehaviour {
    [SerializeField] AudioClip Sound;
    AudioMixerGroup MixerGroup = null;
    [SerializeField] float Volume = 1f;
    [SerializeField] float Pitch = 1f;
    [SerializeField] bool ShouldPlayWhileGamePaused = false;
    [SerializeField] bool ShouldSkipMixerGroup = false;

    SoundyController SoundController;
    static SoundyManager SimpleSoundyManager;
    [SerializeField] SoundyManager TempSoundyManager; // for example in loader

    private void OnEnable() {
        FindSimpleSoundyManager();
        FindWait();

        if (!ShouldSkipMixerGroup) FindMixerGroup();
    }

    void FindMixerGroup() {
        if (MixerGroup == null) {
            MixerGroup = GameMaster.FindAudioWrapper().SFXGeneral;
        }
    }

    void FindSimpleSoundyManager() {
        if (SimpleSoundyManager == null) {
            SimpleSoundyManager = GameMaster.FindSoundyManager();

            if (SimpleSoundyManager == null)
                SimpleSoundyManager = TempSoundyManager;
        }
    }

    static WaitForEndOfFrame Wait;
    void FindWait() {
        if (Wait == null) Wait = GameMaster.FindWait(Wait);
    }

    public static bool PlayingSimpleSound = false;
    public void PlaySound() {
        PlayingSimpleSound = true;


        if (!ShouldSkipMixerGroup) FindMixerGroup();
        FindSimpleSoundyManager();
        FindWait();

        if (GlobalVars.IsGamePaused && !ShouldPlayWhileGamePaused) return;

        if (Sound != null && gameObject.activeSelf) {
            SoundController = SimpleSoundyManager.Play(audioClip: Sound, outputAudioMixerGroup: MixerGroup, volume: Volume, pitch: Pitch, loop: false, spatialBlend: 0f);
            if (ShouldPlayWhileGamePaused) {
                SoundController.AudioSource.ignoreListenerPause = true;
            }

            PlayingSimpleSound = false;
        }
    }
}
