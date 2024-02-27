using AssetsTools.NET;
using AdvancedCompany.Cosmetics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using UnityEngine;

namespace AdvancedCompany
{
    public class CosmeticsLoader : MonoBehaviour
    {
        public GameObject Shadow;
        public RectTransform LoadingBar;
        public TextMeshProUGUI LoadingText;
        public TextMeshProUGUI Percentage;

        private HashSet<string> AddedFiles = new HashSet<string>();
        private ConcurrentBag<string> ConvertedFiles = new ConcurrentBag<string>();
        private ConcurrentQueue<string> Files = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, bool> Status = new ConcurrentDictionary<string, bool>();
        private const int MAX_THREADS = 8;
        private bool Converting = false;
        private bool Loading = false;
        private AssetBundleCreateRequest[] Loaders;

        public class ThreadWorker
        {
            public CosmeticsLoader Instance;

            public void Run()
            {
                while (Instance.Files.TryDequeue(out var file))
                {
                    try
                    {
                        if (file.EndsWith(".cosmetics"))
                        {
                            var directory = System.IO.Path.GetDirectoryName(file);
                            var fileName = System.IO.Path.GetFileName(file);
                            string md5File = Path.Combine(directory, fileName + ".md5");
                            string cacheFile = Path.Combine(directory, fileName + ".cache");
                            var md5Found = System.IO.File.Exists(md5File);
                            var cacheFound = System.IO.File.Exists(cacheFile);

                            var fileContent = System.IO.File.ReadAllBytes(file);
                            string checksum = "";
                            using (var md5 = MD5.Create())
                            {
                                checksum = BitConverter.ToString(md5.ComputeHash(fileContent)).Replace("-", "").ToLowerInvariant();
                            }
                            if (md5Found && cacheFound)
                            {
                                string existingChecksum = System.IO.File.ReadAllText(md5File).Trim().ToLowerInvariant();
                                if (checksum != existingChecksum)
                                {
                                    md5Found = false;
                                    cacheFound = false;
                                }
                            }
                            if (!md5Found || !cacheFound)
                            {
                                AssetsTools.NET.Extra.AssetsManager n = new AssetsTools.NET.Extra.AssetsManager();
                                var bundle = n.LoadBundleFile(file);
                                var assetFile = n.LoadAssetsFileFromBundle(bundle, 0);
                                foreach (var assetInfo in assetFile.file.AssetInfos)
                                {
                                    var @override = false;
                                    var baseField = n.GetBaseField(assetFile, assetInfo);
                                    if (baseField["m_AssemblyName"].FieldName != "DUMMY" && baseField["m_AssemblyName"].AsString == "MoreCompany")
                                    {
                                        baseField["m_AssemblyName"].AsString = "AdvancedCompany.Unity";
                                        baseField["m_Namespace"].AsString = "AdvancedCompany.Cosmetics";
                                        @override = true;
                                    }
                                    if (@override)
                                        assetInfo.SetNewData(baseField);
                                }

                                bundle.file.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetFile.file);
                                using (var fileOutput = new FileStream(cacheFile + ".uncompressed", FileMode.Create, FileAccess.Write, FileShare.Read))
                                using (var writer = new AssetsFileWriter(fileOutput))
                                {
                                    bundle.file.Write(writer);
                                }
                                n.UnloadAll(true);

                                var newUncompressedBundle = new AssetBundleFile();
                                newUncompressedBundle.Read(new AssetsFileReader(File.OpenRead(cacheFile + ".uncompressed")));
                                using (AssetsFileWriter writer = new AssetsFileWriter(cacheFile))
                                {
                                    newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4);
                                }
                                newUncompressedBundle.Close();
                                System.IO.File.Delete(cacheFile + ".uncompressed");
                                System.IO.File.WriteAllText(md5File, checksum);
                            }

                            Instance.ConvertedFiles.Add(cacheFile);
                        }

                        Instance.Status[file] = true;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                        Instance.Status[file] = true;
                    }
                }
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            if (Application.isEditor)
            {
                AddDirectory("Assets/TestCosmetics");
                StartConverting();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Converting)
            {
                var total = Status.Count;
                var loaded = 0;
                foreach (var kv in Status)
                {
                    if (kv.Value)
                        loaded++;
                }
                var percentage = (float)loaded / (float)total;
                LoadingBar.sizeDelta = new Vector2(355f * percentage, LoadingBar.sizeDelta.y);
                if (loaded == total)
                {
                    StartLoading();
                }
            }
            else if (Loading)
            {
                float totalProgress = (float) Loaders.Length;
                float progress = 0f;
                var done = 0;
                for (var i = 0; i < Loaders.Length; i++)
                {
                    if (Loaders[i].isDone)
                    {
                        done++;
                        progress += 1f;
                        if (Loaders[i].assetBundle != null)
                            LoadCosmetics(Loaders[i].assetBundle);
                    }
                    else
                    {
                        progress += Loaders[i].progress;
                    }
                }
                var percentage = (float) progress / (float)totalProgress;
                LoadingBar.sizeDelta = new Vector2(355f * percentage, LoadingBar.sizeDelta.y);
                if (done == Loaders.Length)
                    Close();
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
            if (Application.isEditor)
            {
                FindObjectOfType<PlayerSettings>().SetCosmeticsProvider(new PlayerSettings.TestCosmeticProvider());
            }
        }

        private void LoadCosmetics(AssetBundle assetBundle)
        {
            var assetNames = assetBundle.GetAllAssetNames();
            for (var i = 0; i < assetNames.Length; i++)
            {
                if (assetNames[i].EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    GameObject val = assetBundle.LoadAsset<GameObject>(assetNames[i]);
                    val.hideFlags = HideFlags.DontUnloadUnusedAsset;

                    CosmeticInstance cosmetic = val.GetComponent<CosmeticInstance>();
                    if (cosmetic != null)
                    {
                        CosmeticDatabase.AddCosmetic(cosmetic);
                    }
                }
            }
        }

        public void AddDirectory(string directory)
        {
            if (System.IO.Directory.Exists(directory))
            {
                var files = System.IO.Directory.GetFiles(directory);
                HashSet<string> foundCosmetics = new HashSet<string>();
                List<string> foundMD5 = new List<string>();
                List<string> foundCaches = new List<string>();
                for (var i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".cosmetics"))
                    {
                        foundCosmetics.Add(files[i]);
                        AddFile(files[i]);
                    }
                    if (files[i].EndsWith(".cosmetics.md5"))
                    {
                        foundMD5.Add(files[i]);
                    }
                    if (files[i].EndsWith(".cosmetics.cache"))
                    {
                        foundCaches.Add(files[i]);
                    }
                }
                foreach (var cache in foundCaches)
                {
                    if (!foundCosmetics.Contains(cache.Replace(".cosmetics.cache", ".cosmetics")))
                        System.IO.File.Delete(cache);
                }
                foreach (var md5 in foundMD5)
                {
                    if (!foundCosmetics.Contains(md5.Replace(".cosmetics.md5", ".cosmetics")))
                        System.IO.File.Delete(md5);
                }
                var directories = System.IO.Directory.GetDirectories(directory);
                for (var i = 0; i < directories.Length; i++)
                    AddDirectory(directories[i]);
            }
        }

        public void AddFile(string file)
        {
            if (!AddedFiles.Contains(file))
            {
                AddedFiles.Add(file);
                Files.Enqueue(file);
                Status.TryAdd(file, false);
            }
        }

        private void StartLoading()
        {
            LoadingText.text = "Loading cosmetics...";
            Converting = false;
            Loading = true;

            var files = ConvertedFiles.ToArray();
            Loaders = new AssetBundleCreateRequest[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                Loaders[i] = AssetBundle.LoadFromFileAsync(files[i]);
            }
        }

        public void StartConverting()
        {
            Converting = true;
            for (var i = 0; i < MAX_THREADS; i++)
            {
                var worker = new ThreadWorker() { Instance = this };
                Thread t = new Thread(worker.Run);
                t.Start();
            }
        }
    }
}
