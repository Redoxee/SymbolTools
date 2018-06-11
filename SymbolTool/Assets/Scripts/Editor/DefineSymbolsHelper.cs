using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace AntonMakesGames.Tools
{
    public class DefineSymbolsHelper : EditorWindow
    {
        private const string DATA_PATH = "Assets/";
        const string DATA_NAME_BASE = "defineData.Asset";
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
            m_currentGroupTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        }
        
        private bool m_settings = false;
        private string m_additionalDefine;

        private BuildTargetGroup m_currentGroupTarget;

        public void OnGUI()
        {
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
                            bool added = Add(m_currentGroupTarget, m_additionalDefine);
                            if (added)
                            {
                                m_additionalDefine = "";
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (m_defineData == null)
            {
                GUILayout.Label("Define data not set!");
                return;
            }

            BuildTargetGroup prev = m_currentGroupTarget;
            m_currentGroupTarget = (BuildTargetGroup)EditorGUILayout.EnumPopup(m_currentGroupTarget);
            if (prev != m_currentGroupTarget)
            {
                m_defineData.LastTargetSelected = m_currentGroupTarget;
                EditorUtility.SetDirty(m_defineData);
            }


            if (m_defineData.Collection.Count > 0)
            {
                DisplaySymbols();

                GUILayout.Space(10f);

                string label = "Apply";
                bool warning = EditorUserBuildSettings.selectedBuildTargetGroup != m_currentGroupTarget;
                if (warning)
                {
                    label = "Apply (*)";
                }
                if (GUILayout.Button(label))
                {
                    Set(m_currentGroupTarget);
                    Save();
                }
                if (warning)
                {
                    GUILayout.Label("Selected Target is not the same as the current build target");
                }


                GUILayout.Space(20f);
            }

            if (GUILayout.Button("Open Player settings"))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
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
                var isActive = def.IsActiveOnTarget(m_currentGroupTarget);
                var newValue = GUILayout.Toggle(isActive, def.Name);
                if (isActive != newValue)
                {
                    def.SetValueOnTarget(m_currentGroupTarget, newValue);
                }
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
                    m_defineData.LastTargetSelected = EditorUserBuildSettings.selectedBuildTargetGroup;
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
            foreach (BuildTargetGroup t in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string current = PlayerSettings.GetScriptingDefineSymbolsForGroup(t);
                var strArray = current.Split(';');
                foreach (string s in strArray)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        Add(t, s);
                    }
                }
            }
        }

        private bool Add(BuildTargetGroup t,string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                DefineEntity entity = m_defineData.Get(s);
                if (entity == null)
                {
                    entity = new DefineEntity();
                    entity.Name = s;
                    m_defineData.Add(entity);
                }
                entity.SetValueOnTarget(t, true);
                EditorUtility.SetDirty(m_defineData);
                return true;
            }
            return false;
        }

        private void Set(BuildTargetGroup t)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DefineEntity def in m_defineData.Collection)
            {
                if (def.IsActiveOnTarget(t))
                {
                    sb.Append(def.Name).Append(';');
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(t, sb.ToString());
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
