﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

    [CustomEditor(typeof(SwitchManager))]
    [System.Serializable]
    public class SwitchManagerEditor : Editor
    {
        // Variables
        private int currentTab;

        public override void OnInspectorGUI()
        {
            // GUI skin variable
            GUISkin customSkin;

            // Select GUI skin depending on the editor theme
            if (EditorGUIUtility.isProSkin == true)
                customSkin = (GUISkin)Resources.Load("Editor\\Custom Skin Dark");
            else
                customSkin = (GUISkin)Resources.Load("Editor\\Custom Skin Light");

            GUILayout.Space(-70);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Top Header
            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Switch Top Header"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Toolbar content
            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Events");
            toolbarTabs[1] = new GUIContent("Saving");
            toolbarTabs[2] = new GUIContent("Settings");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(60);

            currentTab = GUILayout.Toolbar(currentTab, toolbarTabs, customSkin.FindStyle("Toolbar Indicators"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(50);

            // Draw toolbar tabs as a button
            if (GUILayout.Button(new GUIContent("Events", "Events"), customSkin.FindStyle("Toolbar Items")))
                currentTab = 0;

            if (GUILayout.Button(new GUIContent("Saving", "Saving"), customSkin.FindStyle("Toolbar Saving")))
                currentTab = 1;

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Toolbar Settings")))
                currentTab = 2;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Property variables
            var OnEvents = serializedObject.FindProperty("OnEvents");
            var OffEvents = serializedObject.FindProperty("OffEvents");

            var saveValue = serializedObject.FindProperty("saveValue");
            var switchTag = serializedObject.FindProperty("switchTag");

            var invokeAtStart = serializedObject.FindProperty("invokeAtStart");
            var isOn = serializedObject.FindProperty("isOn");

            // Draw content depending on tab index
            switch (currentTab)
            {
                case 0:
                    GUILayout.Space(20);
                    GUILayout.Label("EVENTS", customSkin.FindStyle("Header"));
                    GUILayout.Space(2);
                    EditorGUILayout.PropertyField(OnEvents, new GUIContent("On Events"), true);
                    EditorGUILayout.PropertyField(OffEvents, new GUIContent("Off Events"), true);

                    GUILayout.Space(4);
                    break;

                case 1:
                    GUILayout.Space(20);
                    GUILayout.Label("SAVING", customSkin.FindStyle("Header"));
                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    saveValue.boolValue = GUILayout.Toggle(saveValue.boolValue, new GUIContent("Save Value"), customSkin.FindStyle("Toggle"));
                    saveValue.boolValue = GUILayout.Toggle(saveValue.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();

                    if (saveValue.boolValue == true)
                    {
                        EditorGUI.indentLevel = 2;
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(new GUIContent("Tag:"), customSkin.FindStyle("Text"), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(switchTag, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        EditorGUI.indentLevel = 0;
                        GUILayout.Space(2);
                        EditorGUILayout.HelpBox("Each switch should has its own unique tag.", MessageType.Info);
                    }

                    GUILayout.Space(4);
                    break;

                case 2:
                    GUILayout.Space(20);
                    GUILayout.Label("SETTINGS", customSkin.FindStyle("Header"));
                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    invokeAtStart.boolValue = GUILayout.Toggle(invokeAtStart.boolValue, new GUIContent("Invoke At Start"), customSkin.FindStyle("Toggle"));
                    invokeAtStart.boolValue = GUILayout.Toggle(invokeAtStart.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    isOn.boolValue = GUILayout.Toggle(isOn.boolValue, new GUIContent("Is On"), customSkin.FindStyle("Toggle"));
                    isOn.boolValue = GUILayout.Toggle(isOn.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();

                    if (saveValue.boolValue == true)
                        EditorGUILayout.HelpBox("Save Value is enabled. This option won't be used if there's a stored value.", MessageType.Info);

                    GUILayout.Space(4);
                    break;
            }

            // Apply the changes
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif