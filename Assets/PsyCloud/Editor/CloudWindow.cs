using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PsyCloud
{
    public class CloudWindow : EditorWindow
    {
        [MenuItem("Tools/PsyCloud")]
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(CloudWindow));
            window.Show();
        }

        string cookies = "phpdisk_info=U2ZRYlAyVm5UZARiAG4HVAJgBDULWwJkUmkHYQI0U2EEMV5sAGdSagE7B2QPXARrVWRQM14wBWdVbgQ1DzpRZlM2UTFQYFZjVGAEYQBuB2oCYQQ0C2QCMFIzBzECNVNjBDJeZQA3Um4BOgc1D2AEV1U0UGpeMQViVWYEZQ85UWZTY1FrUDE%3D; ylogin=1104264";
        Cloud cloud;
        Vector2 scroll;

        private void OnGUI()
        {
            cookies = EditorGUILayout.TextField("Cookies", cookies);

            if (cloud == null)
            {
                if (GUILayout.Button("Login"))
                {
                    cloud = new Cloud();
                    Task.WaitAll(cloud.Login(cookies));
                }
            }
            else
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                DrawDir(cloud.root);
                EditorGUILayout.EndScrollView();
            }
        }

        Dictionary<string, bool> foldoutDict = new Dictionary<string, bool>();
        private bool GetFoldout(string key)
        {
            foldoutDict.TryGetValue(key, out var foldout);
            return foldout;
        }

        private void DrawDir(CloudDirectory dir, int intent = 0)
        {
            EditorGUI.indentLevel = intent;
            EditorGUI.BeginChangeCheck();
            if (dir == null || dir.name == null)
            {
                Debug.LogError("dir is null");
                return;
            }
            foldoutDict[dir.id] = EditorGUILayout.Foldout(GetFoldout(dir.id), dir.name);
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                if (foldoutDict[dir.id])
                {
                    if (!dir.isGettingDirectories)
                        Task.WaitAll(dir.GetDirectories());
                    if (!dir.isGettingFiles)
                        Task.WaitAll(dir.GetFiles());
                }
                else
                {
                    if (!dir.isGettingDirectories && !dir.isGettingFiles)
                        dir.ClearCaches();
                }
            }

            if (foldoutDict[dir.id])
            {
                if (dir.cachedDirectories != null)
                {
                    foreach (var _dir in dir.cachedDirectories)
                    {
                        DrawDir(_dir, intent + 1);
                    }
                }

                if (dir.cachedFiles != null)
                {
                    foreach (var _file in dir.cachedFiles)
                    {
                        EditorGUILayout.LabelField($"{_file.name} : {_file.size}");
                    }
                }
            }
        }
    }
}
