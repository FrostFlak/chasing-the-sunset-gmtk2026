#if UNITY_EDITOR
// Author: VatsalAmbastha.com, https://www.x.com/vatsalAmbastha
// Made for our game SHOCKFACE!: https://discord.gg/9PruHaftWM

// How to use:
// Create Assets/Editor folder (or an Editor folder anywhere in your Unity project) 
// Copy this script and paste it into ToggleInspectorLockShortcut.cs inside the Editor folder
// Use ALT+Q to toggle inspector lock

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class ToggleInspectorLockShortcut {
    const string OverlayName = "inspector-lock-tint-overlay";
    static readonly Color LockedTint = new(0.55f, 0.08f, 0.08f, 0.1f);

    static readonly Type InspectorWindowType =
        typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

    static readonly PropertyInfo IsLockedProperty =
        InspectorWindowType?.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);

    static readonly FieldInfo ParentField =
        typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);

    static EditorWindow _lastFocusedInspector;
    static readonly Dictionary<int, bool> LastLockStates = new();

    static ToggleInspectorLockShortcut() {
        Subscribe();
        AssemblyReloadEvents.beforeAssemblyReload += Unsubscribe;
    }

    static void Subscribe() {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    static void Unsubscribe() {
        EditorApplication.update -= OnEditorUpdate;
    }

    static void OnEditorUpdate() {
        if (InspectorWindowType == null || IsLockedProperty == null)
            return;

        var focused = EditorWindow.focusedWindow;
        if (focused != null && InspectorWindowType.IsInstanceOfType(focused))
            _lastFocusedInspector = focused;

        var inspectors = Resources.FindObjectsOfTypeAll(InspectorWindowType);
        var aliveIds = new HashSet<int>(inspectors.Length);

        foreach (var obj in inspectors) {
            if (obj is not EditorWindow inspector)
                continue;

            int id = inspector.GetInstanceID();
            aliveIds.Add(id);

            bool locked = (bool)IsLockedProperty.GetValue(inspector);
            LastLockStates.TryGetValue(id, out bool wasLocked);

            if (locked != wasLocked)
                inspector.Repaint();

            ApplyTint(inspector, locked);
            LastLockStates[id] = locked;
        }

        var staleIds = new List<int>();
        foreach (var pair in LastLockStates) {
            if (!aliveIds.Contains(pair.Key))
                staleIds.Add(pair.Key);
        }

        foreach (int id in staleIds)
            LastLockStates.Remove(id);

        if (_lastFocusedInspector && !_lastFocusedInspector)
            _lastFocusedInspector = null;
    }
    [MenuItem("Edit/HotKeys/Toggle Lock &q")]
    static void ToggleInspectorLock() {
        var inspector = GetTargetInspectorWindow();
        if (inspector == null || IsLockedProperty == null)
            return;

        ActivateInspectorTab(inspector);
        _lastFocusedInspector = inspector;

        bool locked = (bool)IsLockedProperty.GetValue(inspector);
        IsLockedProperty.SetValue(inspector, !locked);
        inspector.Repaint();
        ApplyTint(inspector, !locked);
        LastLockStates[inspector.GetInstanceID()] = !locked;
    }

    static EditorWindow GetTargetInspectorWindow() {
        var context = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;

        if (context != null) {
            var dockedInspector = GetInspectorInSameDock(context);
            if (dockedInspector != null)
                return dockedInspector;

            if (InspectorWindowType.IsInstanceOfType(context))
                return context;
        }

        if (_lastFocusedInspector)
            return _lastFocusedInspector;

        var inspectors = Resources.FindObjectsOfTypeAll(InspectorWindowType);
        return inspectors.Length > 0 ? inspectors[0] as EditorWindow : null;
    }

    static void ActivateInspectorTab(EditorWindow inspector) {
        if (inspector == null)
            return;

        SelectPaneInDock(GetDockArea(inspector), inspector);
        inspector.Focus();
    }

    static EditorWindow GetInspectorInSameDock(EditorWindow contextWindow) {
        var dockArea = GetDockArea(contextWindow);
        if (dockArea == null)
            return null;

        foreach (var pane in GetPanesInDock(dockArea)) {
            if (pane != null && InspectorWindowType.IsInstanceOfType(pane))
                return pane;
        }

        return null;
    }

    static object GetDockArea(EditorWindow window) {
        if (window == null || ParentField == null)
            return null;

        object current = ParentField.GetValue(window);
        while (current != null) {
            var type = current.GetType();
            if (type.Name == "DockArea")
                return current;

            current = ParentField.GetValue(current);
        }

        return null;
    }

    static IEnumerable<EditorWindow> GetPanesInDock(object dockArea) {
        if (dockArea == null)
            yield break;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var panesField = dockArea.GetType().GetField("m_Panes", flags);
        if (panesField?.GetValue(dockArea) is not System.Collections.IList panes)
            yield break;

        foreach (var pane in panes) {
            if (pane is EditorWindow editorWindow)
                yield return editorWindow;
        }
    }

    static void SelectPaneInDock(object dockArea, EditorWindow pane) {
        if (dockArea == null || pane == null)
            return;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var panes = new List<EditorWindow>();
        foreach (var p in GetPanesInDock(dockArea))
            panes.Add(p);

        int index = panes.IndexOf(pane);
        if (index < 0)
            return;

        var selectedProperty = dockArea.GetType().GetProperty("selected", flags);
        if (selectedProperty != null && selectedProperty.CanWrite)
            selectedProperty.SetValue(dockArea, index);
    }


    static void ApplyTint(EditorWindow inspector, bool locked) {
        var root = inspector.rootVisualElement;
        if (root == null)
            return;

        var overlay = root.Q<VisualElement>(OverlayName);

        if (!locked) {
            overlay?.RemoveFromHierarchy();
            return;
        }

        if (overlay != null)
            return;

        overlay = new VisualElement { name = OverlayName };
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.right = 0;
        overlay.style.top = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = LockedTint;
        overlay.pickingMode = PickingMode.Ignore;
        root.Add(overlay);
        overlay.BringToFront();
    }
}
#endif