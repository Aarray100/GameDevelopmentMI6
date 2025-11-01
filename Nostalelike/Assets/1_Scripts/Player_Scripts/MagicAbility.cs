using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Water,
    Poison,
    Heal
}

[CreateAssetMenu(fileName = "New Magic Ability", menuName = "Combat/Magic Ability")]
public class MagicAbility : ScriptableObject
{
    [Header("Ability Info")]
    public string abilityName = "Fireball";
    public Sprite abilityIcon;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Damage & Type")]
    public DamageType damageType = DamageType.Fire;
    public float baseDamage = 20f;
    public float damageScaling = 1.0f;  // Skaliert mit Magic Power
    
    [Header("Resource Cost")]
    public float manaCost = 10f;
    public float staminaCost = 0f;
    
    [Header("Cooldown")]
    public float cooldown = 2f;
    
    [Header("Animation")]
    public string animationTrigger = "CastFire";  // Name des Animation-Triggers
    
    [Header("Status Effects")]
    public bool appliesStatusEffect = false;
    public StatusEffectType statusEffect;
    public float effectDuration = 5f;
    public float effectTickInterval = 1f;
    public float effectValue = 5f;  // z.B. Damage per Tick oder Slow Amount
    
    [Header("Projectile (Optional)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileRange = 15f;
    
    [Header("Visual Effects")]
    public GameObject castEffectPrefab;
    public GameObject hitEffectPrefab;
    public Color abilityColor = Color.red;
}

public enum StatusEffectType
{
    None,
    Burn,      // Fire: HP Damage over Time
    Poison,    // Poison: HP Damage + Armor Reduction
    Slow,      // Water: Movement Speed Reduction
    Heal       // Heal: HP Regeneration over Time
}
