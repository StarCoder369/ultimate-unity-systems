#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ModularInspectorCleanup : EditorWindow
{
    private sealed class ScriptInfo
    {
        public string AssetPath;
        public ModularInspectorSourceRewriter.Result Result;
        public bool Selected = true;
    }

    private readonly List<ScriptInfo> scripts = new();

    private Vector2 scriptScroll;
    private Vector2 previewScroll;
    private int selectedScript = -1;

    [MenuItem("Tools/Modular Inspector/Cleanup")]
    public static void Open()
    {
        GetWindow<ModularInspectorCleanup>("Modular Inspector Cleanup");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (scripts.Count == 0)
        {
            EditorGUILayout.HelpBox("Scan the project to find scripts using Modular Inspector.", MessageType.Info);
            return;
        }

        DrawScriptList();

        if (selectedScript >= 0 && selectedScript < scripts.Count)
        {
            DrawPreview(scripts[selectedScript]);
        }

        DrawBottomBar();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Scan Project", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            ScanProject();
        }

        if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            SetSelection(true);
        }

        if (GUILayout.Button("Select None", EditorStyles.toolbarButton, GUILayout.Width(85)))
        {
            SetSelection(false);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawScriptList()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Found {scripts.Count} scripts.", EditorStyles.boldLabel);

        scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll, GUILayout.Height(180));

        for (int i = 0; i < scripts.Count; i++)
        {
            ScriptInfo script = scripts[i];

            EditorGUILayout.BeginHorizontal();

            script.Selected = EditorGUILayout.Toggle(script.Selected, GUILayout.Width(20));

            if (GUILayout.Button(script.AssetPath, selectedScript == i ? EditorStyles.boldLabel : EditorStyles.label))
            {
                selectedScript = i;
            }

            EditorGUILayout.LabelField($"{script.Result.Changes.Count} changes", GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview(ScriptInfo script)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Changes", EditorStyles.boldLabel);

        previewScroll = EditorGUILayout.BeginScrollView(previewScroll, GUILayout.Height(250));

        foreach (ModularInspectorSourceRewriter.Change change in script.Result.Changes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"{change.AttributeName} — line {change.Line}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Before: " + change.OriginalText);

            if (change.Type == ModularInspectorSourceRewriter.ChangeType.Convert)
            {
                EditorGUILayout.LabelField("After: " + change.ReplacementText);
            }
            else
            {
                EditorGUILayout.LabelField("After: removed");
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawBottomBar()
    {
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUI.enabled = GetSelectedCount() > 0;

        if (GUILayout.Button("Create Backup + Apply", GUILayout.Width(190), GUILayout.Height(30)))
        {
            ApplyChanges();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void ScanProject()
    {
        scripts.Clear();
        selectedScript = -1;

        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string source = File.ReadAllText(file);
            ModularInspectorSourceRewriter.Result result = ModularInspectorSourceRewriter.Rewrite(source, file);

            if (result.Changes.Count == 0)
            {
                continue;
            }

            string assetPath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');

            scripts.Add(new ScriptInfo
            {
                AssetPath = assetPath,
                Result = result
            });
        }

        Repaint();
    }

    private void ApplyChanges()
    {
        List<string> selectedPaths = new();

        foreach (ScriptInfo script in scripts)
        {
            if (script.Selected && script.Result.Changes.Count > 0 && !script.Result.HasErrors)
            {
                selectedPaths.Add(script.AssetPath);
            }
        }

        if (selectedPaths.Count == 0)
        {
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Apply Cleanup", $"This will modify {selectedPaths.Count} script(s).\n\nA backup will be created first.", "Continue", "Cancel");

        if (!confirm)
        {
            return;
        }

        string backupPath;

        try
        {
            backupPath = ModularInspectorBackup.CreateBackup(selectedPaths);
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("Backup Failed", exception.Message, "OK");
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        try
        {
            foreach (ScriptInfo script in scripts)
            {
                if (!script.Selected || script.Result.Changes.Count == 0 || script.Result.HasErrors)
                {
                    continue;
                }

                string path = Path.Combine(projectRoot, script.AssetPath);
                string expected = script.Result.RewrittenText;

                File.WriteAllText(path, expected);

                string actual = File.ReadAllText(path);

                if (actual != expected)
                {
                    throw new IOException($"Unity could not verify the rewritten file:\n{script.AssetPath}");
                }
            }
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("Cleanup Failed", $"{exception.Message}\n\nYour backup is here:\n{backupPath}", "OK");
            return;
        }

        AssetDatabase.Refresh();

        bool openBackup = EditorUtility.DisplayDialog("Cleanup Complete", $"Successfully changed {selectedPaths.Count} script(s).\n\nBackup:\n{backupPath}", "Open Backup", "Close");

        if (openBackup)
        {
            ModularInspectorBackup.OpenBackupFolder(backupPath);
        }

        ScanProject();
    }

    private void SetSelection(bool selected)
    {
        foreach (ScriptInfo script in scripts)
        {
            script.Selected = selected;
        }
    }

    private int GetSelectedCount()
    {
        int count = 0;

        foreach (ScriptInfo script in scripts)
        {
            if (script.Selected && script.Result.Changes.Count > 0)
            {
                count++;
            }
        }

        return count;
    }
}

#endif