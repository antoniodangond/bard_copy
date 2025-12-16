public static class UpgradeRuntimeApplier
{
    public static void ApplyAllFromProgress()
    {
        var pp = PlayerProgress.Instance;
        if (pp == null) return;

        Apply("dash", pp.HasUpgradeId("dash"));
        Apply("aoe",  pp.HasUpgradeId("aoe"));
    }

    private static void Apply(string id, bool enabled)
    {
        switch (id)
        {
            case "dash":
                PlayerController.canDash = enabled;
                break;

            case "aoe":
                PlayerController.AbilityGate.AOEUnlocked = enabled;
                break;
        }
    }
}
