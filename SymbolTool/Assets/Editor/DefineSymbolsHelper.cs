using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AntonMakesGames.Tools
{
    public class DefineSymbolsHelper : EditorWindow
    {
        private const string DATA_PATH = "Assets/";
        const string DATA_NAME_BASE = "DefineData.Asset";
        private string DataName
        {
            get {
                return DATA_NAME_BASE + Application.productName;
            }
        }

        DefineData m_defineData = null;

        [MenuItem("Tools/Symbol Tool")]
        public static void OpenWindow()
        {
             EditorWindow.GetWindow(typeof(DefineSymbolsHelper));
        }

        #region GUI

        private void OnEnable()
        {
            titleContent.text = "Symbol Tool";
        }

        private void Awake()
        {

            if (PlayerPrefs.HasKey(DataName))
            {
                m_defineData = AssetDatabase.LoadAssetAtPath<DefineData>(PlayerPrefs.GetString(DataName));
            }
        }
        
        private bool m_settings = false;
        private string m_additionalDefine;

        public void OnGUI()
        {
//            m_settings = EditorGUILayout.BeginToggleGroup("Settings", m_settings);
            m_settings = EditorGUILayout.Foldout(m_settings, "Settings");
            if (m_settings)
            {
                EditorGUI.indentLevel++;
                ManageDefineData();
                if (m_defineData != null)
                {
                    if (GUILayout.Button("Reset"))
                    {
                        m_defineData.Collection.Clear();
                        Grab();
                    }

                    if (GUILayout.Button("Grab Values"))
                    {
                        Grab();
                    }

                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                    GUILayout.BeginHorizontal();
                    {
                        m_additionalDefine = EditorGUILayout.TextField(m_additionalDefine);
                        if (GUILayout.Button("Add", GUILayout.Width(40)))
                        {
                            bool added = Add(m_additionalDefine);
                            if (added)
                            {
                                m_additionalDefine = "";
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Open Player settings"))
                {
                    EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (m_defineData == null)
            {
                GUILayout.Label("Define data not setted!");
                return;
            }

            if (m_defineData.Collection.Count > 0)
            {
                DisplaySymbols();

                if (GUILayout.Button("Apply"))
                {
                    Set();
                    Save();
                }
            }
        }


        public void OnInspectorUpdate()
        {
            this.Repaint();
        }

        private void DisplaySymbols()
        {
            GUILayout.BeginVertical();
            foreach (var def in m_defineData.Collection)
            {
                def.IsActive = GUILayout.Toggle(def.IsActive, def.Name);
            }
            GUILayout.EndVertical();
        }

        private void ManageDefineData()
        {
            var prev = m_defineData;
            m_defineData = (DefineData)EditorGUILayout.ObjectField("Define Data", m_defineData, typeof(DefineData), false);
            if (prev != m_defineData && m_defineData != null)
            {
                string dataPath = AssetDatabase.GetAssetPath(m_defineData);
                PlayerPrefs.SetString(DataName, dataPath);
                PlayerPrefs.Save();
            }
            if (m_defineData == null)
            {
                if (GUILayout.Button("Create Data"))
                {
                    m_defineData = S_CreateDefineData();
                    Grab();
                }
            }
        }

        #endregion

        #region Core

        private void Save()
        {
            if (m_defineData == null)
            {
                Debug.LogError("No Data to save");
                return;
            }
            EditorUtility.SetDirty(m_defineData);
            AssetDatabase.SaveAssets();
        }

        private void Grab()
        {
            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string current = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup);
            var strArray = current.Split(';');
            foreach (string s in strArray)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    Add(s);
                }
            }
        }

        private bool Add(string s)
        {
            if (!string.IsNullOrEmpty(s) && !m_defineData.Contains(s))
            {
                m_defineData.Collection.Add(new DefineEntity
                {
                    Name = s,
                    IsActive = true,
                });
                return true;
            }
            return false;
        }

        private void Set()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DefineEntity def in m_defineData.Collection)
            {
                if (def.IsActive)
                {
                    sb.Append(def.Name).Append(';');
                }
            }
            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentGroup, sb.ToString());
        }
        
        static DefineData S_CreateDefineData()
        {
            UnityEngine.Debug.Log("Creating a new build data");
            DefineData defData = ScriptableObject.CreateInstance<DefineData>();
            AssetDatabase.CreateAsset(defData, DATA_PATH + DATA_NAME_BASE);
            UnityEngine.Debug.Log("Define Data created");
            return defData;
        }

        #endregion
    }
}
