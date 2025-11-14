using UnityEngine;

public class PlayerUpgradesBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerController player;  // drag or auto-find

    [Header("Upgrade SOs (ids must match save ids)")]
    [SerializeField] private UpgradeSO dashUpgrade;    // id = "dash"
    [SerializeField] private UpgradeSO aoeUpgrade;     // id = "aoe"

    void Awake()
    {
        if (!player) player = GetComponent<PlayerController>();

        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded += Apply;
    }

    void OnDestroy()
    {
        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded -= Apply;
    }

    void Start() => Apply(); // handle scene start

    public void Apply()
    {
        if (PlayerProgress.Instance == null || player == null) return;

        bool hasDash = dashUpgrade && PlayerProgress.Instance.HasUpgrade(dashUpgrade);
        bool hasAoe  = aoeUpgrade  && PlayerProgress.Instance.HasUpgrade(aoeUpgrade);

        player.SetDashEnabled(hasDash);
        player.SetAoeEnabled(hasAoe);
    }
}
