using UnityEngine;

public static class Utils
{
    public static bool HasTargetLayer(LayerMask targetLayer, GameObject gameObject)
    {
        // Use bit shifts to determine if gameObject layer matches specified LayerMask
        // https://discussions.unity.com/t/checking-if-a-layer-is-in-a-layer-mask/860331
        return (targetLayer.value & (1 << gameObject.layer)) != 0;
    }
}
