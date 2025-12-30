using UnityEngine;

/// <summary>
/// Base weapon class. Attach to weapon model objects.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public string WeaponName = "Weapon";
    public WeaponType Type = WeaponType.Pistol;
    
    [Header("Stats")]
    public float Damage = 20f;
    public float FireRate = 0.2f; // Seconds between shots
    public float Range = 50f;
    public float Spread = 0.02f; // Accuracy spread
    
    [Header("Ammo")]
    public int MagazineSize = 12;
    public int CurrentAmmo = 12;
    public float ReloadTime = 1.5f;
    
    [Header("Recoil")]
    public float RecoilVertical = 1f;
    public float RecoilHorizontal = 0.5f;
    public float RecoilRecoverySpeed = 5f;
    
    [Header("Audio")]
    public AudioClip FireSound;
    public AudioClip ReloadSound;
    public AudioClip EmptySound;
    
    [Header("Visual")]
    public ParticleSystem MuzzleFlashVFX;
    public Transform MuzzlePoint;

    public enum WeaponType
    {
        Pistol,
        SMG,
        Rifle,
        Shotgun,
        Melee
    }
    
    public bool CanFire => CurrentAmmo > 0;
    public bool NeedsReload => CurrentAmmo == 0;
    public bool IsMagazineFull => CurrentAmmo == MagazineSize;
}
