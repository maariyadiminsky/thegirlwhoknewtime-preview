/* The Girl Who Knew Time™ code and all related assets are Licensed and Trademarked under TrinityMoon Studios™ */
/* You may not use this code for any personal or commercial project. */
/* Copyright © TrinityMoon Studios and Mariya Diminsky */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GlobalFn : MonoBehaviour {
    public static GlobalFn instance;

    void Awake() {
        instance = this;
    }

    // ************************ UTILITY ************************* //

    public static IEnumerator WaitTry<T>(T CheckThis, float TimeToWait = 0.5f) {
        FindWait();

        float _Time = Time.time;
        while (CheckThis == null) {
            yield return Wait;

            if (Time.time >= _Time + TimeToWait)
                yield break;
        }

        yield break;
    }

    static float CurrentTime;
    public static IEnumerator StartWaitTime(float WaitAmount, bool ShouldBeUnScaled = false, bool ShouldCancelOnForTap = false) {
        CurrentTime = 0f;

        while(((ShouldCancelOnForTap && GlobalVars.WaitingForTutorialTap) || !ShouldCancelOnForTap) && CurrentTime < WaitAmount) {

            if (ShouldBeUnScaled)
                CurrentTime += Time.unscaledDeltaTime;
            else
                CurrentTime += Time.deltaTime;

            yield return null;
        }

        yield break;
    }

    static void FindWait() {
        if (Wait == null) {
            Wait = GameMaster.FindWait(Wait);
        }
    }

    static WaitForEndOfFrame Wait = new WaitForEndOfFrame();
    public static IEnumerator WaitTryBool(bool CheckThis, float TimeToWait = 0.5f) {
        FindWait();

        float _Time = Time.time;
        while (CheckThis == false) {
            yield return Wait;

            if (Time.time >= _Time + TimeToWait)
                yield break;
        }

        yield break;
    }

    public void QuitApp() {
        GlobalVars.QuitGame = true;

        Application.Quit();
    }

    static float pow;
    public static float RoundToDecimalPlace(float num, int decimalPlaces) {
        pow = Mathf.Pow(10, decimalPlaces);
        return Mathf.Round((num * pow) / pow);
    }

    public class TYPES {
        public const string ERROR = "ERROR";
        public const string TEST = "TEST";
    }

    public static void ClearConsole() {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        clearMethod.Invoke(null, null);
    }

    public static bool IsDistanceBetweenTwoVectorsGreaterThanZero(Vector2 First, Vector2 Second) {
        return Vector2.Distance(First, Second) > 0;
    }

    public static bool IsTwoVector3Equal(Vector3 First, Vector3 Second) {
        return First.Equals(Second);
    }

    public static bool StringToBool(string s) => s == "true" || s == "True";

    public static float StringToFloat(string stringValue, float defaultValue = 0) =>
        float.TryParse(stringValue, out defaultValue) ? float.Parse(stringValue) : defaultValue;

    public static int StringToInt(string stringValue, int defaultValue = 0) =>
        int.TryParse(stringValue, out defaultValue) ? int.Parse(stringValue) : defaultValue;

    public static Vector2 StringToVector2(string vectorString) {
        if (vectorString == "default") return new Vector2(0, 0);

        string[] vectorArray = vectorString.Split(',');
        return new Vector2(float.Parse(vectorArray[0]), float.Parse(vectorArray[1]));
    }

    public static bool IsBetweenRange(float thisValue, float value1, float value2) {
        return thisValue >= Mathf.Min(value1, value2) && thisValue <= Mathf.Max(value1, value2);
    }

    public static int DefaultIntValue() { return 9999; }

    public static float DefaultFloatValue() { return 9999f; }

    public static string FirstLetterToUpperCaseOrConvertNullToEmptyString(string s) {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] arr = s.ToCharArray();
        arr[0] = char.ToUpper(arr[0]);

        return new string(arr);
    }

    public static float CalculatePercentageCompleted(int TotalCompleted, float TotalPossible) {
        return TotalCompleted == 0 ? TotalCompleted : (Mathf.Round(TotalCompleted / TotalPossible * 100f) / 100f) * 100f;
    }

    static float StartRotation;
    static float EndRotation;
    static float TimeAmount;
    public static IEnumerator Rotate(Transform RotationItem, float Duration = 10f, float AmountToRotate = -360f) {
        StartRotation = RotationItem.eulerAngles.z;
        EndRotation = StartRotation + AmountToRotate;
        TimeAmount = 0f;

        while (TimeAmount < Duration) {
            TimeAmount += Time.deltaTime;
            RotationItem.eulerAngles = new Vector3(RotationItem.eulerAngles.x, RotationItem.eulerAngles.y, Mathf.Lerp(StartRotation, EndRotation, TimeAmount / Duration) % 360.0f);
            yield return null;
        }
    }

    public void SetOrientationToDefault() {
        GameSettings.SetOrientationToDefault();
    }

    // ************************ SETTINGS ************************* //

    void ToggleVibration() {
        GlobalVars.IsVibrationOn = !GlobalVars.IsVibrationOn;
    }

    void ToggleNotification() {
        GlobalVars.IsNotificationOn = !GlobalVars.IsNotificationOn;
    }
}
