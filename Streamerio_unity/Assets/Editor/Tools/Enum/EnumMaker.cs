using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Infra.Tool
{
    public class EnumMaker: EditorWindow
    {
        private string _enumName = "";
        private string _outputFolder = MainFolders.Script.AssetPath;
        private string _nameSpace = "";
        
        private List<string> _parameterNames = new List<string>();
        private ReorderableList _reorderableList;

        private Vector2 _scrollPos;

        private string _newParam = ""; 
        
        [MenuItem("Tools/Enum Maker")]
        public static void Init()
        {
            var window = (EnumMaker)EditorWindow.GetWindow(typeof(EnumMaker));
            window.titleContent = new GUIContent("Enum Maker");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Enum名", EditorStyles.label);
            _enumName = EditorGUILayout.TextField(_enumName);
            
            GUILayout.Label("名前空間", EditorStyles.label);
            _nameSpace = EditorGUILayout.TextField(_nameSpace);

            if (GUILayout.Button("Enumを読み込む"))
            {
                LoadEnum();
            }

            if (_reorderableList != null)
            {
                GUILayout.Label("保存先フォルダ（Assetsからのパス）(ex:Assets/0_Script/Test)", EditorStyles.label);
                _outputFolder = EditorGUILayout.TextField(_outputFolder);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                _reorderableList.DoLayoutList();
                GUILayout.EndScrollView();

                if (GUILayout.Button("保存"))
                {
                    SaveEnum();
                }
            }
        }
        
        /// <summary>
        /// Enumをロード
        /// </summary>
        private void LoadEnum()
        {
            var type = EnumUtil.GetEnumType(_enumName, _nameSpace);
            
            _parameterNames = new List<string>();
            if (type != null)
            {
                var enumPath = ScriptUtil.FindScriptPath(_enumName);
                _outputFolder = string.IsNullOrEmpty(enumPath) ? 
                    _outputFolder : 
                    Path.GetDirectoryName(enumPath);
                
                var names = Enum.GetNames(type);
                _parameterNames.AddRange(names);
            }

            SetupReorderableList();
        }

        /// <summary>
        /// GUIで表示するリスト
        /// </summary>
        private void SetupReorderableList()
        {
            _reorderableList = new ReorderableList(_parameterNames, typeof(string), true, true, true, true);

            _reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Enum パラメータ一覧");
            };

            _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                _parameterNames[index] = EditorGUI.TextField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    _parameterNames[index]
                );
            };

            _reorderableList.onAddCallback = (ReorderableList list) =>
            {
                _parameterNames.Add("");
            };
        }
        
        /// <summary>
        /// Enumファイルを保存する
        /// </summary>
        private void SaveEnum()
        {
            EnumUtil.CreateEnum(_parameterNames.ToArray(), _nameSpace, _enumName, _outputFolder);
            AssetDatabase.Refresh();
        }
    }
}