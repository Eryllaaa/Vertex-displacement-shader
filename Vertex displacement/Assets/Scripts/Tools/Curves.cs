using UnityEngine;

// Author : Brian
public static class Curves {

    // Consts
    public enum EnumAnimations { LINEAR, EASE_OUT_EXPO, EASE_OUT_BACK, EASE_IN_EXPO, EASE_IN_OUT_CUBIC, EASE_OUT_SINE, EASE_IN_CUBIC, EASE_OUT_CUBIC, EASE_OUT_CIRC, EASE_OUT_ELASTIC, EASE_IN_ELASTIC, EASE_IN_OUT_SINE }
    
    // Variables
    public static Curve GetCurve(EnumAnimations pIndex) => GetCurve((int)pIndex);
    public static Curve GetCurve(int pIndex) => _AnimationsArray[pIndex];
    private static Curve[] _AnimationsArray = new Curve[12] {
        Linear, EaseOutExpo, EaseOutBack, EaseInExpo, EaseInOutCubic, EaseOutSine, EaseInCubic, EaseOutCubic, EaseOutCirc, EaseOutElastic, EaseInElastic, EaseInOutSine
    };

    // Delegates
    public delegate float Curve(float pRatio);

    // Functions
    public static float Linear(float pRatio) {
        return pRatio;
    }
    public static float EaseOutExpo(float pRatio) {
        return pRatio == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * pRatio);
    }
    public static float EaseOutBack(float pRatio) {
        const float LC1 = 1.70158f;
        const float LC3 = LC1 + 1f;
        return 1f + LC3 * Mathf.Pow(pRatio - 1f, 3f) + LC1 * Mathf.Pow(pRatio -1f, 2f);
    }
    public static float EaseInExpo(float pRatio) {
        return pRatio == 0f ? 0f : Mathf.Pow(2f, 10f * pRatio - 10f);
    }
    public static float EaseInOutCubic(float pRatio) {
        return pRatio < .5f ? 4f * Mathf.Pow(pRatio, 3f) : 1f - Mathf.Pow(-2f * pRatio + 2f, 3f) * .5f;
    }
    public static float EaseOutSine(float pRatio) {
        return Mathf.Sin(pRatio * Mathf.PI * .5f);
    }
    public static float EaseInCubic(float pRatio) {
        return Mathf.Pow(pRatio, 3f);
    }
    public static float EaseOutCubic(float pRatio) {
        return 1f - Mathf.Pow(1f - pRatio, 3f);
    }
    public static float EaseOutCirc(float pRatio) {
        return Mathf.Sqrt(1f - Mathf.Pow(pRatio - 1f, 2f));
    }
    public static float EaseOutElastic(float pRatio) {
        const float LC4 = 2f * Mathf.PI / 3f;
        return pRatio == 0f ? 0f : pRatio == 1f ? 1f : 1f + Mathf.Pow(2f, -10f * pRatio) * Mathf.Sin((pRatio * 10f - .75f) * LC4);
    }
    public static float EaseInElastic(float pRatio) {
        const float NEG_LC4 = -2f * Mathf.PI / 3f;
        return pRatio == 0f ? 0f : pRatio == 1f ? 1f : Mathf.Pow(2f, 10f * pRatio - 10f) * Mathf.Sin((pRatio * -10f + 10.75f) * NEG_LC4) * NEG_LC4;
    }
    public static float EaseInOutSine(float pRatio) {
        return Mathf.Cos(Mathf.PI * pRatio) * -.5f + .5f;
    }
}