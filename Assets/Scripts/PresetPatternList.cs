using System.Collections.Generic;
using UnityEngine;

public enum PatternName
{
    Blanks,
    A_RepeatedSlides,
    B_AlternatingSlides,
    C_VerticalCombination,
    D_MixedCombination,
    E_DoubleSwitch,
    F_CrossPattern,
    G_RapidAlternation,
    H_DoubleTrouble,
    I_VerticalChaos,
    J_Whiplash,
    K_Barrage,
    L_GrandFinale
}

[CreateAssetMenu(fileName = "PresetPatternList", menuName = "Scriptable Objects/PresetPatternList")]
public class PresetPatternList : ScriptableObject
{
    public List<Pattern> Patterns;
}
