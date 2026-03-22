using System.Text.RegularExpressions;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Hierarchy_Headers
{
    public static class HierarchyHeaderStyler
    {
        private static readonly Regex HeaderRegex = new("^--- (.+) ---$", RegexOptions.Compiled);
        private static StyleSheet _cachedStyleSheet;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            CheckAndPromptNewHierarchy();

            HierarchyWindow.BindView += OnBindView;
            HierarchyWindow.BindViewItem += OnBindViewItem;
        }

        private static void OnBindView(HierarchyView view)
        {
            LoadAndApplyStyleSheet(view);
        }

        private static void OnBindViewItem(HierarchyViewItem viewItem)
        {
            ResetViewItemStyle(viewItem);

            Match match = HeaderRegex.Match(viewItem.Name.text);
            if (match.Success)
            {
                ApplyHeaderStyle(viewItem, match);
            }
        }

        /// <summary>
        /// 스타일시트를 로드하고 뷰에 적용합니다.
        /// </summary>
        private static void LoadAndApplyStyleSheet(HierarchyView view)
        {
            if (_cachedStyleSheet == null)
            {
                string styleGuid = EditorGUIUtility.isProSkin
                    ? "8ce66cc3823444f0831431287ac6ac13" // Dark
                    : "37ff8bd0fd6a4936bf373bd8ee899b20"; // Light

                string stylePath = AssetDatabase.GUIDToAssetPath(styleGuid);
                _cachedStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
            }

            if (_cachedStyleSheet != null && !view.styleSheets.Contains(_cachedStyleSheet))
            {
                view.styleSheets.Add(_cachedStyleSheet);
            }
        }

        /// <summary>
        /// New Hierarchy 사용 여부를 확인하고, 사용하지 않는 경우 전환을 권장합니다.
        /// </summary>
        private static void CheckAndPromptNewHierarchy()
        {
            bool isNewHierarchyEnabled = HierarchyPreferencesUtility.GetUseNewHierarchy();

            if (!isNewHierarchyEnabled)
            {
                bool shouldSwitch = EditorUtility.DisplayDialog(
                    "설정 필요",
                    "Hierarchy Headers 기능을 사용하려면 New Hierarchy로 전환해야 합니다.",
                    "전환",
                    "취소"
                );

                if (shouldSwitch)
                {
                    bool isSuccess = HierarchyPreferencesUtility.SetUseNewHierarchy(true);
                    
                    if (isSuccess) 
                        Debug.Log("New Hierarchy로 전환되었습니다.");
                }
            }
        }

        /// <summary>
        /// ViewItem의 이전 스타일을 초기화합니다.
        /// </summary>
        private static void ResetViewItemStyle(HierarchyViewItem viewItem)
        {
            viewItem.RowContainer.RemoveFromClassList("Category");
        }

        /// <summary>
        /// 헤더 스타일을 적용하고 텍스트를 정리합니다.
        /// </summary>
        private static void ApplyHeaderStyle(HierarchyViewItem viewItem, Match match)
        {
            // 스타일 적용
            viewItem.RowContainer.AddToClassList("Category");
            viewItem.RowContainer.RemoveFromClassList("unity-collection-view__item--selected");

            // 텍스트 정리 (--- 제거)
            viewItem.Name.text = match.Groups[1].Value.Trim();
        }
    }
}