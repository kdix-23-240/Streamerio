using Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common.Editor
{
    public abstract class AutoSetDataScriptableObjectEditor<TKey, TValue, TFile, TSO> : UnityEditor.Editor
        where TKey : Enum
        where TFile : UnityEngine.Object
        where TSO : AutoSetDataScriptableObject<TKey, TValue>
    {
        private TSO _target;

        private void OnEnable()
        {
            _target = target as TSO;
        }

        public override void OnInspectorGUI()
        {
            var setDataButton = GUILayout.Button("辞書にデータを登録(重複は上書き)");
            var resetButton = GUILayout.Button("リセット");
            
            base.OnInspectorGUI();
            
            if (setDataButton)
            {
                SetData();
            }

            if (resetButton)
            {
                Reset();
            }
        }

        /// <summary>
        /// 辞書にデータを登録(重複はスキップ)
        /// </summary>
        private void SetData()
        {
            string[] filePaths = GetPaths();
            
            LoadFiles(filePaths, out var enumParams, out var files);

            CreateEnumFile(enumParams);
            
            CreateDictionary(files, enumParams);
            Save();
            
            Debug.Log("辞書に登録完了");
        }

        /// <summary>
        /// 辞書をリセット
        /// </summary>
        private void Reset()
        {
            _target.ResetDictionary();
            Save();
            
            Debug.Log("Reset Data");
        }
        
        /// <summary>
        /// 取得するファイルのパスをすべて取得
        /// </summary>
        /// <returns></returns>
        private string[] GetPaths()
        {
            List<string> paths = new List<string>();

            // 対象の拡張子ごとに検索
            foreach (var extension in _target.FileExtentions)
            {
                // 対象フォルダ内の対象の拡張子を持つ全ファイルのパスを取得
                var allPaths = Directory.GetFiles(_target.FolderPath, extension.ToPattern(), SearchOption.AllDirectories);
                
                // 検索から外すフォルダ名をパスに含んだら除外する
                foreach (var path in allPaths)
                {
                    if(ContainsFolder(path, _target.IgnoreFolderName))
                    {
                        continue;
                    }
                    
                    paths.Add(path);
                }
            }
            
            return paths.ToArray();
        }
        
        /// <summary>
        /// 対象のフォルダをパスに含むか調べる
        /// </summary>
        /// <param name="path">調査パス</param>
        /// <param name="folderNames">調べるフォルダ名一覧</param>
        /// <returns>対象のフォルダをパスに含むか</returns>
        private bool ContainsFolder(string path, params string[] folderNames)
        {
            // パスを"/"や"\"で切り分け
            var directories = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // パスにフォルダ名を含む場合はtrue
            foreach (var folderName in folderNames)
            {
                if (directories.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// パスからEnumパラメターとファイルを取得
        /// </summary>
        /// <param name="paths">取得したいファイルパス</param>
        /// <param name="enumParams">enumパラメターを格納する用</param>
        /// <param name="files">ロードしたファイルを格納する用</param>
        private void LoadFiles(string[] paths, out string[] enumParams, out TFile[] files)
        {
            int fileCount = paths.Length;
            
            enumParams = new string [fileCount];
            files = new TFile[fileCount];

            for (int i = 0; i < fileCount; i++)
            {
                string path = paths[i];
                
                // ファイル名取得
                var fileName = Path.GetFileNameWithoutExtension(path);
                // ファイル名に/や空白があれば取り除く
                enumParams[i] = fileName.Replace(" ", string.Empty).Replace("/", string.Empty);
                
                // ファイル取得
                files[i] = AssetDatabase.LoadAssetAtPath<TFile>(path);
            }
        }
        
        /// <summary>
        /// Enum作成
        /// </summary>
        /// <param name="enumParams"></param>
        private void CreateEnumFile(string[] enumParams)
        {
            EnumUtil.CreateEnum(enumParams, _target.EnumNameSpace, _target.EnumFileName, _target.EnumPath);
        }

        /// <summary>
        /// 辞書を作成
        /// </summary>
        /// <param name="files">取得したファイル</param>
        /// <param name="enumParams">enumパラメター</param>
        private void CreateDictionary(TFile[] files, string[] enumParams)
        {
            int length = files.Length;
            var dictionary = new SerializeDictionary<TKey, TValue>();
            
            for(int i=0; i<length; i++)
            {
                var key = (TKey)Enum.Parse(typeof(TKey), enumParams[i]);
                var value = CreateValue(files[i]);
                
                dictionary[key] = value;
            }
            
            _target.SetDictionary(dictionary);
        }
        
        /// <summary>
        /// 辞書に登録する値を生成
        /// </summary>
        /// <param name="file">元になるファイル</param>
        /// <returns></returns>
        protected abstract TValue CreateValue(TFile file);

        /// <summary>
        /// スクリプタブルオブジェクトを保存
        /// </summary>
        private void Save()
        {
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Repaint();
            Debug.Log("Save Data");
        }
    }
}