using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueEditorWindow : EditorWindow
{
    ObjectField treeField;
    DialogueTree currentTree;
    DialogueGraphView graphView;

    [MenuItem("Window/Dialogue Editor")]
    public static void Open()
    {
        DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>();
        wnd.titleContent = new UnityEngine.GUIContent("Dialogue Editor");
    }

    private void OnEnable()
    {
        if (currentTree != null && graphView != null)
            graphView.Load(currentTree);
    }

    public void CreateGUI()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Column;
        container.style.flexGrow = 1;
        rootVisualElement.Add(container);
        

        // Create toolbar
        var toolbar = new Toolbar();
        var createNodeButton = new Button(() =>
        {
            graphView.CreateNode("New Speaker");
        })
        {
            text = "Create Node"
        };

         // GraphView
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };
        graphView.style.flexGrow = 1;

        treeField = new ObjectField("Dialogue Tree")
        {
            objectType = typeof(DialogueTree),
            allowSceneObjects = false
        };
        treeField.RegisterValueChangedCallback(evt =>
        {
            currentTree = evt.newValue as DialogueTree;
            graphView.Load(currentTree);
        });

        if (currentTree == null)
        {
            string defaultPath = "Assets/DefaultDialogueTree.asset";
            currentTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(defaultPath);

            if (currentTree == null)
            {
                currentTree = CreateInstance<DialogueTree>();
                AssetDatabase.CreateAsset(currentTree, defaultPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[INIT] Created default DialogueTree at: " + defaultPath);
            }

            treeField.SetValueWithoutNotify(currentTree);
            graphView.Load(currentTree);
        }


        var saveButton = new Button(() =>
        {
            Debug.Log("[SAVE] Save button clicked");

            if (graphView == null)
            {
                Debug.LogWarning("[SAVE] GraphView is null!");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Dialogue Tree",
                currentTree != null ? currentTree.name : "NewDialogueTree",
                "asset",
                "Choose location to save DialogueTree");

            if (!string.IsNullOrEmpty(path))
            {
                if (currentTree == null)
                {
                    currentTree = CreateInstance<DialogueTree>();
                    AssetDatabase.CreateAsset(currentTree, path);
                    Debug.Log("[SAVE] Created new DialogueTree at: " + path);
                }
                else
                {
                    string currentPath = AssetDatabase.GetAssetPath(currentTree);
                    if (currentPath != path)
                    {
                        // Move and rename the existing asset to new path
                        string moveResult = AssetDatabase.MoveAsset(currentPath, path);
                        if (!string.IsNullOrEmpty(moveResult))
                        {
                            Debug.LogError("[SAVE] Failed to move asset: " + moveResult);
                            return;
                        }
                        Debug.Log("[SAVE] Moved asset to: " + path);
                    }
                }

                graphView.Save(currentTree);
                EditorUtility.SetDirty(currentTree);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                currentTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(path);
                treeField.SetValueWithoutNotify(currentTree);
                Debug.Log("[SAVE] Saved DialogueTree to: " + path);
            }
        }) { text = "Save" };


        var newButton = new Button(() =>
        {
            string path = EditorUtility.SaveFilePanelInProject(
            "Save Dialogue Tree", 
            "New DialogueTree", 
            "asset", 
            "Enter a file name to save the dialogue tree");

            if (!string.IsNullOrEmpty(path))
            {
                currentTree = CreateInstance<DialogueTree>();
                AssetDatabase.CreateAsset(currentTree, path);
                AssetDatabase.SaveAssets();
                treeField.SetValueWithoutNotify(currentTree);
                graphView.Load(currentTree); // Explicitly load the new tree


                UnityEngine.Debug.Log("Created new DialogueTree at: " + path);
            }

        }) { text = "New" };

        toolbar.Add(createNodeButton);
        toolbar.Add(treeField);
        toolbar.Add(saveButton);
        toolbar.Add(newButton);
        container.Add(toolbar);

       
        container.Add(graphView);
    }
}
