using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public static class AddressableBuildScript
{
    public static void BuildAddressables()
    {
        // Clean existing Addressables content
        AddressableAssetSettings.CleanPlayerContent();
        
        // Build Addressables content
        AddressableAssetSettings.BuildPlayerContent();
    }
}
