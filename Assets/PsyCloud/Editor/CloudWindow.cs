using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PsyCloud
{
    public class CloudWindow : EditorWindow
    {
        static CloudWindow window;
        [MenuItem("Tools/PsyCloud")]
        public static void ShowWindow()
        {
            window = CloudWindow.GetWindow<CloudWindow>();
            window.Show();
        }

        string cookies = "phpdisk_info=U2ZRYlAyVm5UZARiAG4HVAJgBDULWwJkUmkHYQI0U2EEMV5sAGdSagE7B2QPXARrVWRQM14wBWdVbgQ1DzpRZlM2UTFQYFZjVGAEYQBuB2oCYQQ0C2QCMFIzBzECNVNjBDJeZQA3Um4BOgc1D2AEV1U0UGpeMQViVWYEZQ85UWZTY1FrUDE%3D; ylogin=1104264";
        Vector2 scroll;
        GUIDir rootDir;

        private void OnGUI()
        {
            cookies = EditorGUILayout.TextField("Cookies", cookies);

            if (rootDir == null)
            {
                if (GUILayout.Button("Login"))
                {
                    var cloud = new Cloud();
                    Task.Run(async () =>
                    {
                        await cloud.Login(cookies);
                        rootDir = new GUIDir(cloud.root, 0);
                    });
                }
            }
            else
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                DrawDir(rootDir);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDir(GUIDir dir, int intent = 0)
        {
            EditorGUI.indentLevel = intent;
            EditorGUI.BeginChangeCheck();
            dir.foldout = EditorGUILayout.Foldout(dir.foldout, dir.name);
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                if (dir.foldout) dir.Fetch();
                else dir.Clear();
            }

            if (dir.foldout)
            {
                foreach (var _dir in dir.dirs)
                {
                    DrawDir(_dir, intent + 1);
                }

                foreach (var _file in dir.files)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{_file.name} : {_file.size}");
                    if (!_file.isDownloading && !_file.isDownloaded)
                    {
                        if (GUILayout.Button("Download"))
                        {
                            _file.Download();
                        }
                    }
                    else if (_file.isDownloading)
                    {
                        EditorGUILayout.LabelField($"Downloading({_file.downloadProgress})...");
                    }
                    else if (_file.isDownloaded)
                    {
                        EditorGUILayout.LabelField("Downloaded");
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    public class GUIFile
    {
        public CloudFile file;
        public bool isDownloading;
        public float downloadProgress;
        public bool isDownloaded;

        public string name => file.name;
        public string size => file.size;

        public GUIFile(CloudFile file, int depth)
        {
            this.file = file;
        }

        public void Download()
        {
            if (isDownloading)
                return;
            isDownloading = true;
            var downloadExt = ".psydownload";
            var path = Path.Combine(Application.dataPath, name + downloadExt);
            Task.Run(async () =>
            {
                await file.Download(path, (p) => downloadProgress = p);
                if (File.Exists(path))
                {
                    File.Move(path, path.Substring(0, path.Length - downloadExt.Length));
                }
                isDownloading = false;
                isDownloaded = true;
            });
        }
    }

    public class GUIDir
    {
        public CloudDirectory dir;
        public bool foldout;
        public int depth;
        public bool isFetching;

        public string name => dir == null ? "null" : dir.name;

        public GUIDir(CloudDirectory dir, int depth)
        {
            this.dir = dir;
            this.depth = depth;
        }

        public void Fetch()
        {
            if (isFetching)
                return;
            isFetching = true;
            Task.Run(async () =>
            {
                var dirInfos = await dir.GetDirectories();
                dirs.Clear();
                foreach (var info in dirInfos)
                {
                    dirs.Add(new GUIDir(info, depth + 1));
                }
                var fileInfos = await dir.GetFiles();
                files.Clear();
                foreach (var info in fileInfos)
                {
                    files.Add(new GUIFile(info, depth + 1));
                }
                isFetching = false;
            });
        }

        public void Clear()
        {
            isFetching = false;
            dir.ClearCaches();
            dirs.Clear();
            files.Clear();
        }

        public List<GUIDir> dirs = new List<GUIDir>();
        public List<GUIFile> files = new List<GUIFile>();
    }
}
