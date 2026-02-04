using UnityEditor;
using UnityEngine;
using System;

public class TowerEditorWindow : EditorWindow
{
    private Tower selectedTower;
    private Vector2 scrollPosition;
    private int selectedPathIndex = 0; // Track selected path

    [MenuItem("Tools/Tower Editor")]
    public static void ShowWindow()
    {
        GetWindow<TowerEditorWindow>("Tower Editor");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        try
        {
            EditorGUILayout.LabelField("Tower Editor", EditorStyles.boldLabel);

            selectedTower = EditorGUILayout.ObjectField("Selected Tower", selectedTower, typeof(Tower), true) as Tower;

            if (selectedTower == null)
            {
                EditorGUILayout.HelpBox("Select a Tower GameObject in the scene.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);

            // Begin change check for undo/redo and saving
            EditorGUI.BeginChangeCheck();

            var towerType = selectedTower.GetType();

            float newRange = EditorGUILayout.FloatField("Range", selectedTower.GetRange());
            float newFireRate = EditorGUILayout.FloatField("Fire Rate", (float)towerType.GetField("baseFireRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedTower));
            float newDamage = EditorGUILayout.FloatField("Damage", (float)towerType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedTower));
            int newCost = EditorGUILayout.IntField("Cost", selectedTower.GetCost());
            float newSpreadAngle = EditorGUILayout.FloatField("Spread Angle", (float)towerType.GetField("spreadAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedTower));
            // Upgrade Paths
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Upgrade Paths", EditorStyles.boldLabel);

            string[] pathNames = new string[selectedTower.upgradePaths.Length];
            for (int i = 0; i < pathNames.Length; i++)
            {
                pathNames[i] = $"Path {i + 1}";
            }
            selectedPathIndex = EditorGUILayout.Popup("Select Path", selectedPathIndex, pathNames);

            if (selectedPathIndex < 0 || selectedPathIndex >= selectedTower.upgradePaths.Length)
                selectedPathIndex = 0;

            int path = selectedPathIndex;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Path {path + 1}", EditorStyles.boldLabel);

            var pathObj = selectedTower.upgradePaths[path];
            if (pathObj != null && pathObj.upgrades != null)
            {
                for (int tier = 0; tier < pathObj.upgrades.Length; tier++)
                {
                    var upgrade = pathObj.upgrades[tier];
                    if (upgrade == null)
                    {
                        pathObj.upgrades[tier] = new TowerUpgrade();
                        upgrade = pathObj.upgrades[tier];
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Tier {tier + 1}", EditorStyles.miniBoldLabel);
                    upgrade.upgradeName = EditorGUILayout.TextField("Name", upgrade.upgradeName);
                    upgrade.description = EditorGUILayout.TextField("Description", upgrade.description);
                    upgrade.cost = EditorGUILayout.IntField("Cost", upgrade.cost);
                    upgrade.icon = (Sprite)EditorGUILayout.ObjectField("Icon", upgrade.icon, typeof(Sprite), false);
                    upgrade.rangeBonus = EditorGUILayout.FloatField("Range Bonus", upgrade.rangeBonus);
                    upgrade.fireRateBonus = EditorGUILayout.FloatField("Fire Rate Bonus", upgrade.fireRateBonus);
                    upgrade.damageBonus = EditorGUILayout.FloatField("Damage Bonus", upgrade.damageBonus);
                    upgrade.projectilesPerShotBonus = EditorGUILayout.IntField("Projectiles Per Shot Bonus", upgrade.projectilesPerShotBonus);
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();

            // End change check and apply changes
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedTower, "Modify Tower Stats");

                // Set private fields via reflection
                towerType.GetField("range", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(selectedTower, newRange);
                towerType.GetField("baseFireRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(selectedTower, newFireRate);
                towerType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(selectedTower, newDamage);
                towerType.GetField("cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(selectedTower, newCost);

                EditorUtility.SetDirty(selectedTower);

                // If editing a prefab asset, record modifications
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.IsPartOfPrefabInstance(selectedTower))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(selectedTower);
                }
#endif
                AssetDatabase.SaveAssets(); // Optional: force save
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }
}