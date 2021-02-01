using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace BK.HierarchyHeader.Editor
{
    public static class HeaderSettings
    {
        [UserSetting] public static UserSetting<int> maxLength = new UserSetting<int>(Instance, "general.maxLength", 30);
        [UserSetting] public static UserSetting<int> minPrefixLength = new UserSetting<int>(Instance, "general.minPrefixLength", 10);
        [UserSetting] public static UserSetting<HeaderType> headerType = new UserSetting<HeaderType>(Instance, "general.headerType", HeaderType.Default);
        [UserSetting] public static UserSetting<string> customPrefix = new UserSetting<string>(Instance, "general.customPrefix", null);
        [UserSetting] public static UserSetting<HeaderAlignment> alignment = new UserSetting<HeaderAlignment>(Instance, "general.alignment", HeaderAlignment.Center);


        [UserSettingBlock("General")]
        static void CustomSettingsGUI(string searchContext)
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                maxLength.value = (int)SettingsGUILayout.SettingsSlider("Max Length", maxLength, 10, 60, searchContext);

                headerType.value = (HeaderType)EditorGUILayout.EnumPopup("Header Type", headerType.value);
                SettingsGUILayout.DoResetContextMenuForLastRect(headerType);

                if (headerType.value == HeaderType.Custom)
                {
                    string v = SettingsGUILayout.SettingsTextField("Custom Prefix", customPrefix, searchContext);
                    if (v?.Length <= 1)
                        customPrefix.value = v;
                }

                alignment.value = (HeaderAlignment)EditorGUILayout.EnumPopup("Header Alignment", alignment.value);
                SettingsGUILayout.DoResetContextMenuForLastRect(alignment);

                if (alignment.value == HeaderAlignment.Start || alignment.value == HeaderAlignment.End)
                {
                    minPrefixLength.value = (int)SettingsGUILayout.SettingsSlider("Min Prefix Length", minPrefixLength, 0, 10, searchContext);
                }

                if (scope.changed)
                {
                    Instance.Save();
                    HeaderEditor.UpdateAllHeader();
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Update Headers"))
                {
                    HeaderEditor.UpdateAllHeader();
                }
            }
        }

        internal const string k_PackageName = "com.bennykok.hierarchy-header";
        internal const string k_PreferencesPath = "Project/BK/Hierarchy Header";

        static Settings s_Instance;

        internal static Settings Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new Settings(k_PackageName);

                return s_Instance;
            }
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider(k_PreferencesPath, Instance,
                new[] { typeof(HeaderSettings).Assembly }, SettingsScope.Project);

            return provider;
        }
    }
}