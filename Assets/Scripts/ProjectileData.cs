using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public AnimatorOverrideController animatorOverride;
    public float speed = 10f;
}
