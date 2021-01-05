using Hzexe.Lanzou;
using Hzexe.Lanzou.Model.Lanzou;
using System.Collections.Generic;
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

        LanzouClient client = null;
        string cookies = "";
        GetDirResponse.TextItem root;
        bool isLogin => client != null;
        Vector2 scroll = Vector2.zero;

        private async void OnGUI()
        {
            cookies = EditorGUILayout.TextField("Cookies", cookies);

            if (!isLogin)
            {
                if (GUILayout.Button("Login"))
                {
                    client = new LanzouClient(cookies);
                    root = new GetDirResponse.TextItem() { fol_id = "-1", name = "Root", is_lock = "0", onof = "0", folder_des = "", folderlock = "0" };
                }
            }
            else
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                DrawDir(root);
                EditorGUILayout.EndScrollView();
            }

            if (client != null)
            {
                if (GUILayout.Button("Fetch"))
                {
                    var rep = await client.LsDirAsync("-1");
                    foreach (var dir in rep.text)
                    {
                        Debug.LogError(dir.name + "\n" + dir.fol_id);
                    }
                }
            }
        }

        Dictionary<string, bool> foldoutDict = new Dictionary<string, bool>();
        private bool GetFoldout(string key)
        {
            foldoutDict.TryGetValue(key, out var foldout);
            return foldout;
        }
        
        private void DrawDir(GetDirResponse.TextItem dir)
        {
            foldoutDict[dir.fol_id] = EditorGUILayout.Foldout(GetFoldout(dir.fol_id), dir.name);
            if (!foldoutDict[dir.fol_id])
            { 
            
            }
        }
    }
}