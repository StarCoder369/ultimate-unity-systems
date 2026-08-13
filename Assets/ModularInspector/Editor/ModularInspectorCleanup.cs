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
        public string Original;
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

        EditorGUILayout.Space(6);

        if (scripts.Count == 0)
        {
            EditorGUILayout.HelpBox("Scan the project to find scripts using Modular Inspector.", MessageType.Info);
            return;
        }

        DrawScriptList();

        EditorGUILayout.Space(6);

        if (selectedScript >= 0 && selectedScript < scripts.Count)
        {
            DrawPreview(scripts[selectedScript]);
        }

        EditorGUILayout.Space(6);

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

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawScriptList()
    {
        int changed = GetChangedCount();

        EditorGUILayout.LabelField($"Found {scripts.Count} scripts. {changed} contain changes.", EditorStyles.boldLabel);

        scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll, GUILayout.Height(200));

        for (int i = 0; i < scripts.Count; i++)
        {
            ScriptInfo script = scripts[i];

            EditorGUILayout.BeginHorizontal();

            script.Selected = EditorGUILayout.Toggle(script.Selected, GUILayout.Width(20));

            GUIStyle style = selectedScript == i ? EditorStyles.boldLabel : EditorStyles.label;

            if (GUILayout.Button(script.AssetPath, style))
            {
                selectedScript = i;
            }

            if (script.Result.HasErrors)
            {
                EditorGUILayout.LabelField("ERROR", GUILayout.Width(50));
            }
            else
            {
                EditorGUILayout.LabelField($"{script.Result.Changes.Count} changes", GUILayout.Width(90));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview(ScriptInfo script)
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        if (script.Result.HasErrors)
        {
            EditorGUILayout.HelpBox(script.Result.ErrorMessage, MessageType.Error);
            return;
        }

        if (script.Result.Changes.Count == 0)
        {
            EditorGUILayout.HelpBox("No changes are required.", MessageType.Info);
            return;
        }

        previewScroll = EditorGUILayout.BeginScrollView(previewScroll, GUILayout.Height(300));

        foreach (ModularInspectorSourceRewriter.Change change in script.Result.Changes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"{change.AttributeName} — line {change.Line}", EditorStyles.boldLabel);

            if (change.Type == ModularInspectorSourceRewriter.ChangeType.Convert)
            {
                EditorGUILayout.LabelField("- " + change.OriginalText, GetRemovedStyle());
                EditorGUILayout.LabelField("+ " + change.ReplacementText, GetAddedStyle());
            }
            else
            {
                EditorGUILayout.LabelField("- " + change.OriginalText, GetRemovedStyle());
                EditorGUILayout.LabelField("Removed", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawBottomBar()
    {
        int count = GetSelectedChangedCount();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField($"{count} scripts selected");

        GUILayout.FlexibleSpace();

        GUI.enabled = count > 0;

        if (GUILayout.Button("Create Backup + Apply", GUILayout.Height(32), GUILayout.Width(190)))
        {
            ApplyChanges();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private GUIStyle GetRemovedStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
        return style;
    }

    private GUIStyle GetAddedStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
        return style;
    }

    private void ScanProject()
    {
        scripts.Clear();
        selectedScript = -1;

        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        foreach (string absolutePath in files)
        {
            string text = File.ReadAllText(absolutePath);

            ModularInspectorSourceRewriter.Result result = ModularInspectorSourceRewriter.Rewrite(text, absolutePath);

            if (result.Changes.Count == 0 && !result.HasErrors)
            {
                continue;
            }

            string assetPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

            scripts.Add(new ScriptInfo
            {
                AssetPath = assetPath,
                Original = text,
                Result = result
            });
        }

        Repaint();
    }

    private void ApplyChanges()
    {
        List<string> paths = new();

        foreach (ScriptInfo script in scripts)
        {
            if (script.Selected && !script.Result.HasErrors && script.Result.Changes.Count > 0)
            {
                paths.Add(script.AssetPath);
            }
        }

        if (paths.Count == 0)
        {
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog("Apply Cleanup", $"This will modify {paths.Count} script files.\n\nA backup will be created first.", "Create Backup + Apply", "Cancel");

        if (!confirmed)
        {
            return;
        }

        string backupPath = ModularInspectorBackup.CreateBackup(paths);
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        try
        {
            foreach (ScriptInfo script in scripts)
            {
                if (!script.Selected || script.Result.HasErrors || script.Result.Changes.Count == 0)
                {
                    continue;
                }

                string absolutePath = Path.Combine(projectRoot, script.AssetPath);
                File.WriteAllText(absolutePath, script.Result.RewrittenText);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog("Cleanup Failed", $"Something went wrong.\n\nYour backup is here:\n{backupPath}", "OK");
            return;
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Cleanup Complete", $"Cleaned {paths.Count} scripts.\n\nBackup:\n{backupPath}", "OK");

        ScanProject();
    }

    private void SetSelection(bool selected)
    {
        foreach (ScriptInfo script in scripts)
        {
            script.Selected = selected;
        }
    }

    private int GetChangedCount()
    {
        int count = 0;

        foreach (ScriptInfo script in scripts)
        {
            if (script.Result.Changes.Count > 0 && !script.Result.HasErrors)
            {
                count++;
            }
        }

        return count;
    }

    private int GetSelectedChangedCount()
    {
        int count = 0;

        foreach (ScriptInfo script in scripts)
        {
            if (script.Selected && script.Result.Changes.Count > 0 && !script.Result.HasErrors)
            {
                count++;
            }
        }

        return count;
    }
}

#endif