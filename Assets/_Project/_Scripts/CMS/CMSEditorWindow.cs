#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;

namespace CMSResources {
    public class CMSEditorWindow : EditorWindow {

        #region Entity Types
        private abstract class CMSEntity {
            public abstract string Id { get; }
            public abstract string TypeLabel { get; }
        }

        private class DataEntity : CMSEntity {
            public IDataEntity Data;
            public string FolderName;
            public override string Id => Data.ID;
            public override string TypeLabel => $"📄 {FolderName}";
        }

        private class AssetEntity : CMSEntity {
            public string IdStr;
            public UnityEngine.Object Asset;
            public override string Id => IdStr;
            public override string TypeLabel => $"🎨 {Asset.GetType().Name}";
        }
        #endregion

        #region Fields
        private List<CMSEntity> _entities;
        private List<CMSEntity> _filteredEntities;
        private int _selectedIndex = -1;
        private Vector2 _scrollList;
        private Vector2 _scrollDetail;
        private string _statusMessage;
        private MessageType _statusType;

        private string _searchQuery;

        // Filter state
        private int _entityTypeFilter; // 0=All, 1=Data, 2=Asset
        private int _dataTypeFilter;   // 0=All, 1+=specific data type
        private int _assetTypeFilter;  // 0=All, 1+=specific asset type index
        private List<string> _dataTypeNames = new();
        private List<string> _assetTypeNames = new();

        // UI State
        private bool _autoScan;
        private bool _autoGenerateIds;

        // Selection state
        private HashSet<string> _selectedIds = new(); // for batch operations
        private Dictionary<string, string> _dataFolders = new(); // ID → folder path

        private static GUIStyle _sectionStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _boxStyle;
        private static bool _stylesInit;

        private static void InitStyles() {
            if (_stylesInit) return;
            _sectionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, margin = new RectOffset(0, 0, 2, 2) };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 24 };
            _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 6, 6), margin = new RectOffset(0, 4, 4, 4) };
            _stylesInit = true;
        }
        #endregion

        #region Menu Item
        [MenuItem("CMS/Editor %#&C")]
        public static void ShowWindow() {
            var window = GetWindow<CMSEditorWindow>("📂CMS-Editor");
            window.minSize = new Vector2(700, 520);
            window.Init();
        }
        #endregion

        #region Init
        private void Init() {
            _entities ??= new List<CMSEntity>();
            _filteredEntities ??= new List<CMSEntity>();
            _assetTypeNames ??= new List<string>();
            _selectedIds ??= new HashSet<string>();
            _dataFolders ??= new Dictionary<string, string>();
        }

        private bool _prefsLoaded;
        private void LoadPrefs() {
            if (_prefsLoaded) 
                return;
            
            _autoScan = EditorPrefs.GetBool("CMS_AutoScan", false);
            _autoGenerateIds = EditorPrefs.GetBool("CMS_AutoGenIds", false);
            _prefsLoaded = true;
        }

        private void OnGUI() {
            InitStyles();                                                                                     
            Init();
            HandleDragAndDrop();
            DrawSplitView();
            DrawFooter(); 
        }
        
        private void OnEnable() {
            LoadPrefs();
            CMSAssetWatcher.OnAssetsChanged += OnAssetsChanged;
        }

        private void OnDisable() {
            CMSAssetWatcher.OnAssetsChanged -= OnAssetsChanged;
        }

        private void OnAssetsChanged() {
            if (_autoScan)
                ScanCMSFolder();
            if (_autoGenerateIds) 
                CMSIdGenerator.Generate();
            
            Repaint();
        }
        #endregion

        private void HandleDragAndDrop() {
            if (Event.current.type != EventType.DragUpdated && Event.current.type != EventType.DragPerform)
                return;

            bool hasValidPaths = DragAndDrop.paths.Length > 0 && 
                                 DragAndDrop.paths[0].StartsWith("Assets/");
            if (!hasValidPaths) 
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Move;

            if (Event.current.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();

                string targetFolder = CMSConstants.LocalCMSPath;
                Directory.CreateDirectory(targetFolder);

                foreach (var path in DragAndDrop.paths) {
                    string fileName = Path.GetFileName(path);
                    string targetPath = Path.Combine(targetFolder, fileName);

                    int counter = 1;
                    while (File.Exists(targetPath) || Directory.Exists(targetPath)) {
                        string nameNoExt = Path.GetFileNameWithoutExtension(path);
                        string ext = Path.GetExtension(path);
                        targetPath = Path.Combine(targetFolder, $"{nameNoExt}_{counter}{ext}");
                        counter++;
                    }

                    FileUtil.MoveFileOrDirectory(path, targetPath);
                }

                AssetDatabase.Refresh();
                ScanCMSFolder();
                SetStatus($"Moved {DragAndDrop.paths.Length} file(s) to CMS.", MessageType.Info);
            }

            Event.current.Use();
        }

        private void DrawSplitView() {
            EditorGUILayout.BeginHorizontal();

            // Left: entity list
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            DrawEntityList();
            EditorGUILayout.EndVertical();

            // Right: detail
            EditorGUILayout.BeginVertical();
            _scrollDetail = EditorGUILayout.BeginScrollView(_scrollDetail);
            DrawDetailPanel();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntityList() {
            EditorGUILayout.Space(1);
            EditorGUILayout.BeginVertical("box");

            // Search
            EditorGUI.BeginChangeCheck();
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
                ApplyFilter();

            // Filters
            EditorGUILayout.BeginHorizontal();
            string[] entityFilters = { "All", "📄 Data", "🎨 Asset" };
            int newEntityFilter = GUILayout.Toolbar(_entityTypeFilter, entityFilters, GUILayout.Height(24));
            if (newEntityFilter != _entityTypeFilter) {
                _entityTypeFilter = newEntityFilter;
                _dataTypeFilter = 0;
                _assetTypeFilter = 0;
                ApplyFilter();
            }
            EditorGUILayout.EndHorizontal();

            if (_entityTypeFilter == 1 && _dataTypeNames != null && _dataTypeNames.Count > 0) {
                EditorGUILayout.BeginHorizontal();
                var dataFilterNames = new string[_dataTypeNames.Count + 1];
                dataFilterNames[0] = "All";
                for (int i = 0; i < _dataTypeNames.Count; i++)
                    dataFilterNames[i + 1] = _dataTypeNames[i];

                int newDataFilter = EditorGUILayout.Popup(_dataTypeFilter, dataFilterNames);
                if (newDataFilter != _dataTypeFilter) {
                    _dataTypeFilter = newDataFilter;
                    ApplyFilter();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (_entityTypeFilter == 2 && _assetTypeNames != null && _assetTypeNames.Count > 0) {
                EditorGUILayout.BeginHorizontal();
                var assetFilterNames = new string[_assetTypeNames.Count + 1];
                assetFilterNames[0] = "All";
                for (int i = 0; i < _assetTypeNames.Count; i++)
                    assetFilterNames[i + 1] = _assetTypeNames[i];

                int newAssetFilter = EditorGUILayout.Popup(_assetTypeFilter, assetFilterNames);
                if (newAssetFilter != _assetTypeFilter) {
                    _assetTypeFilter = newAssetFilter;
                    ApplyFilter();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(2);

            _scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));

            var display = _filteredEntities;

            if (display.Count == 0) {
                EditorGUILayout.HelpBox("No matches.", MessageType.Info);
            }
            else {
                foreach (var entity in display) {
                    GUILayout.BeginHorizontal(GUILayout.Height(40));

                    // Checkbox for batch selection (centered vertically)
                    GUILayout.BeginVertical(GUILayout.Width(18), GUILayout.Height(40));
                    GUILayout.FlexibleSpace();
                    bool isBatchSelected = _selectedIds.Contains(entity.Id);
                    bool newBatchSelection = EditorGUILayout.Toggle(isBatchSelected, GUILayout.Width(16));
                    if (newBatchSelection != isBatchSelected) {
                        if (newBatchSelection) _selectedIds.Add(entity.Id);
                        else _selectedIds.Remove(entity.Id);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    // Main button area (selects entity for viewing)
                    GUI.backgroundColor = (_selectedIndex >= 0 && _entities.IndexOf(entity) == _selectedIndex)
                        ? new Color(0.2f, 0.5f, 1f) : Color.white;
                    string label = $"<b>{entity.Id}</b>\n{entity.TypeLabel}";
                    var boldButtonStyle = new GUIStyle(GUI.skin.button) { richText = true };
                    if (GUILayout.Button(label, boldButtonStyle, GUILayout.ExpandHeight(true)))
                        _selectedIndex = _entities.IndexOf(entity);
                    GUI.backgroundColor = Color.white;

                    // Action buttons column (stacked vertically)
                    GUILayout.BeginVertical(GUILayout.Width(28));
                    // Folder emoji button (pings file)
                    if (GUILayout.Button("📂", EditorStyles.miniButton, GUILayout.Width(28), GUILayout.Height(20))) {
                        _selectedIndex = _entities.IndexOf(entity);
                        PingEntity(entity);
                    }

                    // Pencil emoji button (opens file in editor)
                    if (GUILayout.Button("✏️", EditorStyles.miniButton, GUILayout.Width(28), GUILayout.Height(20))) {
                        _selectedIndex = _entities.IndexOf(entity);
                        if (entity is DataEntity) {
                            string path = GetEntityPath(entity);
                            if (!string.IsNullOrEmpty(path)) {
                                string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
                                if (File.Exists(fullPath)) {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                                        FileName = fullPath,
                                        UseShellExecute = true
                                    });
                                }
                            }
                        } else if (entity is AssetEntity ae) {
                            // Open asset in Unity's default editor (prefab, material, etc.)
                            AssetDatabase.OpenAsset(ae.Asset);
                        }
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.EndHorizontal();
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(.3f));
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailPanel() {
            EditorGUILayout.BeginVertical(_boxStyle);

            EditorGUILayout.LabelField("📋 Details", _sectionStyle);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4);

            if (_selectedIndex < 0 || _selectedIndex >= _entities.Count) {
                EditorGUILayout.HelpBox("Select an entity.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            var entity = _entities[_selectedIndex];
            string entityPath = GetEntityPath(entity);

            EditorGUILayout.LabelField($"🔖 {entity.Id}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {entity.TypeLabel}", EditorStyles.miniLabel);

            // Path label
            if (!string.IsNullOrEmpty(entityPath)) {
                var pathStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
                EditorGUILayout.LabelField($"📁 {entityPath}", pathStyle);
            }

            EditorGUILayout.Space(2);

            if (entity is DataEntity de) {
                EditorGUILayout.Space(2);
                if (GUILayout.Button("✏️ Open in Editor", GUILayout.Height(28))) {
                    string path = GetEntityPath(entity);
                    if (!string.IsNullOrEmpty(path)) {
                        // Convert Unity relative path to absolute filesystem path
                        string fullPath = Path.Combine(Application.dataPath, "..", path);
                        fullPath = Path.GetFullPath(fullPath);
                        if (File.Exists(fullPath)) {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                                FileName = fullPath,
                                UseShellExecute = true
                            });
                        } else {
                            SetStatus($"File not found: {fullPath}", MessageType.Error);
                        }
                    }
                }
                EditorGUILayout.Space(2);
                DrawFieldsEditable(de.Data);
            } else if (entity is AssetEntity ae) {
                EditorGUILayout.Space(2);
                if (GUILayout.Button("✏️ Open in Editor", GUILayout.Height(28))) {
                    AssetDatabase.OpenAsset(ae.Asset);
                }
                EditorGUILayout.Space(2);
                DrawAssetPreview(ae);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAssetPreview(AssetEntity ae) {
            var asset = ae.Asset;
            if (asset == null) return;

            EditorGUILayout.LabelField("Asset Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // Preview by asset type — similar to Inspector preview
            if (asset is GameObject go) {
                // Render a preview of the prefab/GameObject
                Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(200), GUILayout.ExpandWidth(true));
                var previewTex = AssetPreview.GetAssetPreview(go);
                if (previewTex != null) {
                    EditorGUI.DrawTextureTransparent(previewRect, previewTex, ScaleMode.ScaleToFit);
                } else {
                    EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f));
                    EditorGUI.LabelField(previewRect, "Preview not available", EditorStyles.centeredGreyMiniLabel);
                }
            }
            else if (asset is Texture2D tex) {
                Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(200), GUILayout.ExpandWidth(true));
                EditorGUI.DrawTextureTransparent(previewRect, tex, ScaleMode.ScaleToFit);
            }
            else if (asset is Sprite sprite) {
                Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(200), GUILayout.ExpandWidth(true));
                if (sprite.texture != null) {
                    // Draw the sprite with proper aspect ratio by using the sprite's texture rect
                    var t = sprite.texture;
                    Rect spriteRect = sprite.rect;
                    // Calculate UVs to show only the sprite portion
                    var uv = new Rect(
                        spriteRect.x / t.width,
                        spriteRect.y / t.height,
                        spriteRect.width / t.width,
                        spriteRect.height / t.height
                    );
                    GUI.DrawTextureWithTexCoords(previewRect, t, uv, true);
                }
            }
            else if (asset is Material mat) {
                Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(200), GUILayout.ExpandWidth(true));
                // Render a preview sphere with the material
                EditorGUI.DrawPreviewTexture(previewRect, mat.mainTexture != null ? mat.mainTexture : Texture2D.grayTexture, mat, ScaleMode.ScaleToFit);
            }
            else if (asset is AudioClip clip) {
                EditorGUILayout.Space(4);
                EditorGUILayout.ObjectField("Audio Clip", clip, typeof(AudioClip), false);
                EditorGUILayout.Space(2);
                // Show audio properties
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("Length (s)", clip.length);
                EditorGUILayout.IntField("Frequency (Hz)", clip.frequency);
                EditorGUI.EndDisabledGroup();
            }
            else if (asset is AnimationClip animClip) {
                EditorGUILayout.Space(4);
                EditorGUILayout.ObjectField("Animation Clip", animClip, typeof(AnimationClip), false);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("Length (s)", animClip.length);
                EditorGUILayout.FloatField("Frame Rate", animClip.frameRate);
                EditorGUILayout.LabelField("Legacy", animClip.legacy ? "Yes" : "No");
                EditorGUI.EndDisabledGroup();
            }
            else if (asset is TextAsset textAsset) {
                EditorGUILayout.Space(4);
                var textStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true, richText = false };
                float textHeight = Mathf.Min(textStyle.CalcHeight(new GUIContent(textAsset.text), EditorGUIUtility.currentViewWidth - 80), 300);
                EditorGUILayout.SelectableLabel(textAsset.text, textStyle, GUILayout.Height(textHeight));
            }
            else if (asset is Font font) {
                EditorGUILayout.ObjectField("Font", font, typeof(Font), false);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Font Size", font.fontSize);
                EditorGUI.EndDisabledGroup();
            }
            else if (asset is Mesh mesh) {
                EditorGUILayout.Space(4);
                EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Vertex Count", mesh.vertexCount);
                EditorGUILayout.IntField("Triangle Count", mesh.triangles.Length / 3);
                EditorGUI.EndDisabledGroup();
            }
            else if (asset is RuntimeAnimatorController controller) {
                EditorGUILayout.ObjectField("Animator Controller", controller, typeof(RuntimeAnimatorController), false);
            }
            else if (asset is ScriptableObject so) {
                EditorGUILayout.Space(4);
                EditorGUI.BeginDisabledGroup(true);
                var soFields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in soFields) {
                    DrawFieldValue(field.Name, field.GetValue(so), true);
                }
                EditorGUI.EndDisabledGroup();
            }
            else {
                // Fallback: generic object field
                EditorGUILayout.ObjectField("Asset", asset, asset.GetType(), false);
            }
        }

        private void DrawFieldsEditable(IDataEntity item) {
            var type = item.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "ID" && p.CanRead && p.CanWrite)
                .ToArray();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            bool changed = false;

            foreach (var prop in props) {
                var oldVal = prop.GetValue(item);
                var newVal = DrawFieldValue(prop.Name, oldVal, false);
                if (!Equals(oldVal, newVal)) {
                    prop.SetValue(item, newVal);
                    changed = true;
                }
            }

            foreach (var field in fields) {
                var oldVal = field.GetValue(item);
                var newVal = DrawFieldValue(field.Name, oldVal, false);
                if (!Equals(oldVal, newVal)) {
                    field.SetValue(item, newVal);
                    changed = true;
                }
            }

            if (!changed) 
                return;
            
            SaveToJson(item);
            SetStatus($"Saved changes to {item.ID}.json", MessageType.Info);
        }

        private object DrawFieldValue(string label, object value, bool readOnly) {
            if (value == null) {
                EditorGUILayout.LabelField(label, "null");
                return null;
            }

            var t = value.GetType();
            if (t.IsEnum) {
                var newVal = EditorGUILayout.EnumPopup(label, (Enum)value);
                return readOnly ? value : newVal;
            }
            if (value is int i) 
                return EditorGUILayout.IntField(label, i);
            if (value is long l)
                return EditorGUILayout.LongField(label, l);
            if (value is float f) 
                return EditorGUILayout.FloatField(label, f);
            if (value is double d)
                return EditorGUILayout.DoubleField(label, d);
            if (value is bool b)
                return EditorGUILayout.Toggle(label, b);
            if (value is string s)
                return EditorGUILayout.TextField(label, s);
            if (value is Vector2 v2)
                return EditorGUILayout.Vector2Field(label, v2);
            if (value is Vector3 v3) 
                return EditorGUILayout.Vector3Field(label, v3);
            if (value is Vector4 v4)
                return EditorGUILayout.Vector4Field(label, v4);
            if (value is Color color)
                return EditorGUILayout.ColorField(label, color);
            if (value is Color32 color32)
                return EditorGUILayout.ColorField(label, (Color)color32);
            if (value is Rect rect)
                return EditorGUILayout.RectField(label, rect);
            if (value is Bounds bounds)
                return EditorGUILayout.BoundsField(label, bounds);
            if (value is AnimationCurve curve)
                return EditorGUILayout.CurveField(label, curve);
            if (value is Gradient gradient)
                return EditorGUILayout.GradientField(label, gradient);
            if (value is LayerMask layerMask)
                return (LayerMask)EditorGUILayout.LayerField(label, (int)layerMask);

            // Fallback for unsupported types
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, value.ToString());
            EditorGUI.EndDisabledGroup();
            return value;
        }

        #region Footer
        private void DrawFooter() {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginVertical("box", GUILayout.MinHeight(60));
            EditorGUILayout.LabelField("📜 Status", _sectionStyle);
            EditorGUILayout.Space(3);
            if (!string.IsNullOrEmpty(_statusMessage))
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
            else
                EditorGUILayout.LabelField("✅ Ready", EditorStyles.miniLabel);
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(6);

            // Buttons
            float btnHeight = 32f;
            GUIStyle bigButton = new GUIStyle(_buttonStyle) { fixedHeight = btnHeight };

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Scan", bigButton, GUILayout.Height(btnHeight)))
                ScanCMSFolder();
            if (GUILayout.Button("➕ Create", bigButton, GUILayout.Height(btnHeight)))
                ShowCreateConfigMenu();
            if (GUILayout.Button("🔑 Generate", bigButton, GUILayout.Height(btnHeight)))
                CMSIdGenerator.Generate();
            if (GUILayout.Button($"🗑️ Delete ({_selectedIds.Count})", bigButton, GUILayout.Height(btnHeight)))
                DeleteSelected();
            
            GUILayout.FlexibleSpace();

            // Right bottom checkboxes
            EditorGUI.BeginChangeCheck();
            _autoScan = EditorGUILayout.ToggleLeft("🔄 Auto-Scan", _autoScan, GUILayout.Width(110));
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool("CMS_AutoScan", _autoScan);

            EditorGUI.BeginChangeCheck();
            _autoGenerateIds = EditorGUILayout.ToggleLeft("🔑 Auto-IDs", _autoGenerateIds, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool("CMS_AutoGenIds", _autoGenerateIds);

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Actions
        private void ShowCreateConfigMenu() {
            // Get all IDataEntity types
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => typeof(IDataEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (types.Count == 0) {
                SetStatus("No IDataEntity types found.", MessageType.Warning);
                return;
            }

            // Build generic menu using GenericMenu
            var menu = new GenericMenu();
            foreach (var type in types.OrderBy(t => t.Name)) {
                var t = type;
                menu.AddItem(new GUIContent(t.Name), false, () => CreateConfigForType(t));
            }
            menu.ShowAsContext();
        }

        private void CreateConfigForType(Type dataType) {
            string folderPath = Path.Combine(CMSConstants.ConfigsPath, dataType.Name);
            Directory.CreateDirectory(folderPath);

            string fileName = EditorUtility.SaveFilePanel(
                $"Create New {dataType.Name}", folderPath, $"{dataType.Name}.json", "json");
            if (string.IsNullOrEmpty(fileName)) return;

            string id = Path.GetFileNameWithoutExtension(fileName);
            var instance = Activator.CreateInstance(dataType) as IDataEntity;

            // Set ID and default strings
            var idProp = dataType.GetProperty("ID");
            idProp?.SetValue(instance, id);
            Registry.SetDefaultStrings(instance);

            var settings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include
            };
            string json = JsonConvert.SerializeObject(instance, settings);
            File.WriteAllText(fileName, json);
            AssetDatabase.Refresh();

            SetStatus($"Created: {id}.json", MessageType.Info);
            ScanCMSFolder();
            _selectedIndex = _entities.FindIndex(e => e.Id == id);
        }

        private void ScanCMSFolder() {
            Registry.Reload();

            // Reset selection
            _selectedIndex = -1;
            _entityTypeFilter = 0;
            _dataTypeFilter = 0;
            _assetTypeFilter = 0;
            _selectedIds.Clear();

            // Build entity list with folder info
            _entities.Clear();
            _dataFolders.Clear();

            // Load data entities with folder names from categories
            foreach (var cat in Registry.Categories) {
                string folderPath = Path.Combine(CMSConstants.ConfigsPath, cat.FolderName);
                if (!Directory.Exists(folderPath)) continue;
                var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var file in jsonFiles) {
                    try {
                        string json = File.ReadAllText(file);
                        var entry = Newtonsoft.Json.JsonConvert.DeserializeObject(json, cat.DataType) as IDataEntity;
                        if (entry != null) {
                            var id = Path.GetFileNameWithoutExtension(file);
                            _entities.Add(new DataEntity { Data = entry, FolderName = cat.FolderName });
                            _dataFolders[id] = cat.FolderName;
                        }
                    }
                    catch { /* skip */ }
                }
            }

            // Load asset entities
            string cmsAssetPath = Path.Combine(Application.dataPath, "Resources", "CMS");
            if (Directory.Exists(cmsAssetPath)) {
                var allFiles = Directory.GetFiles(cmsAssetPath, "*.*", SearchOption.AllDirectories);
                var skipExts = new[] { ".json", ".meta", ".dll", ".cs", ".md" };
                foreach (var file in allFiles) {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (skipExts.Contains(ext)) continue;
                    string id = Path.GetFileNameWithoutExtension(file);
                    if (_dataFolders.ContainsKey(id)) continue;
                    string resourcePath = ToResourcePath(file);
                    if (string.IsNullOrEmpty(resourcePath)) continue;
                    UnityEngine.Object asset = Resources.Load<UnityEngine.Object>(resourcePath);
                    if (asset != null)
                        _entities.Add(new AssetEntity { IdStr = id, Asset = asset });
                }
            }

            _entities = _entities.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase).ToList();

            _dataTypeNames = _entities.OfType<DataEntity>()
                .Select(e => e.FolderName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            _assetTypeNames = _entities.OfType<AssetEntity>()
                .Select(ae => ae.Asset.GetType().Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            ApplyFilter();
            SetStatus($"Scanned: {_entities.Count} entities ({_dataTypeNames.Count} data types, {_assetTypeNames.Count} asset types)", MessageType.Info);
            Repaint();
        }

        private static string ToResourcePath(string filePath) {
            string normalized = filePath.Replace('\\', '/');
            int idx = normalized.IndexOf("/Resources/");
            if (idx < 0) return null;
            string sub = normalized.Substring(idx + "/Resources/".Length);
            int dotIdx = sub.LastIndexOf('.');
            if (dotIdx > 0) sub = sub.Substring(0, dotIdx);
            return sub;
        }

        private void DeleteSelected() {
            if (_selectedIds.Count == 0) {
                SetStatus("No items selected for deletion.", MessageType.Warning);
                return;
            }

            int count = _selectedIds.Count;
            if (!EditorUtility.DisplayDialog("Batch Delete", 
                $"Delete {count} selected item(s)? This cannot be undone.", "Delete", "Cancel"))
                return;

            int deletedCount = 0;
            var toDelete = _entities.Where(e => _selectedIds.Contains(e.Id)).ToList();
            
            foreach (var entity in toDelete) {
                if (entity is DataEntity de) {
                    string folder = _dataFolders.TryGetValue(de.Id, out var f) ? f : de.FolderName;
                    string folderPath = Path.Combine(CMSConstants.ConfigsPath, folder);
                    string filePath = Path.Combine(folderPath, $"{de.Id}.json");
                    if (File.Exists(filePath)) {
                        File.Delete(filePath);
                        deletedCount++;
                    }
                }
                else if (entity is AssetEntity ae) {
                    string path = AssetDatabase.GetAssetPath(ae.Asset);
                    if (!string.IsNullOrEmpty(path)) {
                        AssetDatabase.DeleteAsset(path);
                        deletedCount++;
                    }
                }
            }

            AssetDatabase.Refresh();
            _selectedIds.Clear();
            ScanCMSFolder();
            SetStatus($"Deleted {deletedCount} item(s).", MessageType.Info);
        }

        private void ApplyFilter() {
            var source = _entities;
            var result = new List<CMSEntity>();

            foreach (var e in source) {
                // Entity type filter
                if (_entityTypeFilter == 1 && e is AssetEntity) continue;
                if (_entityTypeFilter == 2 && e is DataEntity) continue;

                // Data type sub-filter
                if (_entityTypeFilter == 1 && _dataTypeFilter > 0 && e is DataEntity de) {
                    string actualType = _dataTypeNames[_dataTypeFilter - 1];
                    if (de.FolderName != actualType) continue;
                }

                // Asset type sub-filter
                if (_entityTypeFilter == 2 && _assetTypeFilter > 0 && e is AssetEntity ae) {
                    string actualType = _assetTypeNames[_assetTypeFilter - 1];
                    if (ae.Asset.GetType().Name != actualType) continue;
                }

                // Search query
                if (!string.IsNullOrEmpty(_searchQuery)) {
                    string q = _searchQuery.ToLowerInvariant();
                    if (!e.Id.ToLowerInvariant().Contains(q)) continue;
                }

                result.Add(e);
            }

            _filteredEntities = result.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase).ToList();
        }
        #endregion

        #region Helpers
        private void SetStatus(string msg, MessageType type) {
            _statusMessage = msg;
            _statusType = type;
        }

        private string GetEntityPath(CMSEntity entity) {
            if (entity is DataEntity de) {
                string folder = _dataFolders.TryGetValue(de.Id, out var f) ? f : de.FolderName;
                return $"Assets/Resources/CMS/Configs/{folder}/{de.Data.ID}.json";
            }
            if (entity is AssetEntity ae) {
                return AssetDatabase.GetAssetPath(ae.Asset);
            }
            return null;
        }

        private void PingEntity(CMSEntity entity) {
            if (entity is DataEntity de) {
                string folder = _dataFolders.TryGetValue(de.Id, out var f) ? f : de.FolderName;
                string localPath = $"Assets/Resources/CMS/Configs/{folder}/{de.Data.ID}.json";
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(localPath);
                if (obj != null) {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
            else if (entity is AssetEntity ae) {
                EditorGUIUtility.PingObject(ae.Asset);
                Selection.activeObject = ae.Asset;
            }
        }

        private void SaveToJson(IDataEntity item) {
            string folderName = _dataFolders.TryGetValue(item.ID, out var f) ? f : "";
            if (string.IsNullOrEmpty(folderName)) {
                // Fallback: find folder by scanning categories
                foreach (var cat in Registry.Categories) {
                    string fp = Path.Combine(CMSConstants.ConfigsPath, cat.FolderName);
                    if (File.Exists(Path.Combine(fp, $"{item.ID}.json"))) {
                        folderName = cat.FolderName;
                        break;
                    }
                }
            }
            string folderPath = Path.Combine(CMSConstants.ConfigsPath, folderName);
            string filePath = Path.Combine(folderPath, $"{item.ID}.json");
            Registry.SetDefaultStrings(item);
            var settings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(item, settings);
            File.WriteAllText(filePath, json);
        }
        #endregion
    }
}
#endif