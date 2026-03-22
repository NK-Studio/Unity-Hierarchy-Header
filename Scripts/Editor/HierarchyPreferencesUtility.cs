using System;
using System.Reflection;
using UnityEngine;

namespace Editor.Hierarchy_Headers
{
    /// <summary>
    /// Unity 6000.5 이상에서 새로운 Hierarchy 윈도우 사용 설정을 제어하는 유틸리티
    /// </summary>
    public static class HierarchyPreferencesUtility
    {
        /// <summary>
        /// 새로운 Hierarchy 윈도우 사용 여부를 설정합니다.
        /// Unity 6000.5 이상에서만 동작합니다.
        /// </summary>
        /// <param name="value">true면 새 Hierarchy 사용, false면 기존 Hierarchy 사용</param>
        /// <returns>설정 성공 여부</returns>
        public static bool SetUseNewHierarchy(bool value)
        {
#if UNITY_6000_5_OR_NEWER
            try
            {
                // 1. HierarchyPreferences 타입 찾기
                var hierarchyPrefsType =
                    typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HierarchyPreferences");

                if (hierarchyPrefsType == null)
                {
                    Debug.LogError("Could not find HierarchyPreferences type");
                    return false;
                }

                // 2. UseNewHierarchy 정적 필드 찾기
                var useNewHierarchyField = hierarchyPrefsType.GetField("UseNewHierarchy",
                    BindingFlags.Public | BindingFlags.Static);

                if (useNewHierarchyField == null)
                {
                    Debug.LogError("Could not find UseNewHierarchy field");
                    return false;
                }

                // 3. SavedBool 객체 가져오기
                var savedBoolObj = useNewHierarchyField.GetValue(null);

                if (savedBoolObj == null)
                {
                    Debug.LogError("SavedBool object is null");
                    return false;
                }
                
                // 4. SavedBool의 타입 정보 가져오기
                var savedBoolType = savedBoolObj.GetType();
                
                // 5. value 프로퍼티 찾아서 값 설정
                var valueProperty = savedBoolType.GetProperty("value",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                if (valueProperty != null && valueProperty.CanWrite)
                {
                    valueProperty.SetValue(savedBoolObj, value);
                    return true;
                }

                Debug.LogError("Could not find writable value property on SavedBool");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting UseNewHierarchy via reflection: {e}");
                return false;
            }
#else
            Debug.LogError("SetUseNewHierarchy is only supported in Unity 6000.5 or newer");
            return false;
#endif
        }

        /// <summary>
        /// 현재 새로운 Hierarchy 윈도우 사용 여부를 가져옵니다.
        /// </summary>
        /// <returns>새 Hierarchy 사용 여부</returns>
        public static bool GetUseNewHierarchy()
        {
#if UNITY_6000_5_OR_NEWER
            try
            {
                var hierarchyPrefsType =
                    typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HierarchyPreferences");

                if (hierarchyPrefsType == null)
                    return false;

                var useNewHierarchyField = hierarchyPrefsType.GetField("UseNewHierarchy",
                    BindingFlags.Public | BindingFlags.Static);

                if (useNewHierarchyField == null)
                    return false;

                var savedBoolObj = useNewHierarchyField.GetValue(null);
                if (savedBoolObj == null)
                    return false;

                var savedBoolType = savedBoolObj.GetType();
                
                // SavedBool의 암시적 bool 변환 연산자 사용
                var implicitOp = savedBoolType.GetMethod("op_Implicit",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { savedBoolType },
                    null);

                if (implicitOp != null)
                {
                    return (bool)implicitOp.Invoke(null, new[] { savedBoolObj });
                }

                // 대안: value 프로퍼티 직접 읽기
                var valueProperty = savedBoolType.GetProperty("value",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                if (valueProperty != null && valueProperty.CanRead)
                {
                    return (bool)valueProperty.GetValue(savedBoolObj);
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting UseNewHierarchy: {e}");
                return false;
            }
#else
            return false;
#endif
        }
    }
}