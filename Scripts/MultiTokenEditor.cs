using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenEditor : EditorWindow
    {
        public const float CANVAS_SIZE_WIDTH  = 4000f;
        public const float CANVAS_SIZE_HEIGHT = 4000f;

        public const float BACKGROUND_DIM_SIZE = 50f;

        [SerializeField]
        private Dictionary<string, string> _erc1155Contracts = new Dictionary<string, string>();

        MultiTokenContract selectedContract = null;

        [NonSerialized]
        GUIStyle nodeStyle;

        [NonSerialized] 
        GUIStyle nodeTokenStyle;

        [NonSerialized]
        MultiTokenNode draggingNode = null;

        [NonSerialized]
        Vector2 draggingOffset = Vector2.zero;

        [NonSerialized]
        MultiTokenNode creatingNode = null;

        [NonSerialized]
        MultiTokenNode deletingNode = null;

        [NonSerialized]
        MultiTokenNode linkingParentNode = null;

        Vector2 scrollPosition;

        [NonSerialized]
        bool draggingCanvas = false;

        [NonSerialized]
        Vector2 draggingCanvasOffset;

        [MenuItem("Window/Ethereum/MultiToken Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(MultiTokenEditor), false, "Multitoken Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            MultiTokenContract contractInstance = EditorUtility.InstanceIDToObject(instanceID) as MultiTokenContract;

            if (contractInstance != null)
            {
                ShowEditorWindow();
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;

            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.normal.textColor  = Color.white;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border  = new RectOffset(12, 12, 12, 12);

            nodeTokenStyle = new GUIStyle();
            nodeTokenStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            nodeTokenStyle.normal.textColor = Color.white;
            nodeTokenStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeTokenStyle.border  = new RectOffset(12, 12, 12, 12);
        }

        private void OnSelectionChanged() 
        {
            MultiTokenContract contractInstance = Selection.activeObject as MultiTokenContract;

            if (contractInstance != null)
            {
                selectedContract = contractInstance;

                Repaint();
            }
        }

        private void OnGUI()
        {
            if (selectedContract == null)
            {
                EditorGUILayout.LabelField("No Contract Selected.");
            }
            else
            {
                ProcessEvents();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                Rect canvas = GUILayoutUtility.GetRect(CANVAS_SIZE_WIDTH, CANVAS_SIZE_HEIGHT);
                DrawBackground(canvas);

                foreach (var node in selectedContract.GetAllNodes()) 
                {
                    DrawConnections(node);
                }

                foreach (var node in selectedContract.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if ((creatingNode != null) && (creatingNode is MultiTokenContractNode))
                {
                    selectedContract.CreateNode((MultiTokenContractNode) creatingNode);
                    creatingNode = null;
                }

                if (deletingNode != null)
                {
                    selectedContract.DeleteNode(deletingNode);
                    deletingNode = null;
                }
            }
        }

        private void DrawBackground(Rect canvas)
        {            
            var backgroundTexture = Resources.Load("background") as Texture2D;

            Rect textureCoords =
                new Rect(0, 0, canvas.width / backgroundTexture.width, canvas.height / backgroundTexture.height);

            GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textureCoords);
        }

        private void DrawConnections(MultiTokenNode node)
        {
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);

            foreach (MultiTokenNode childNode in selectedContract.GetAllChildren(node))
            {
                if (childNode != null)
                {
                    Vector3 endPosition = new Vector3(childNode.GetRect().xMin, childNode.GetRect().center.y, 0);

                    Vector3 controlPointOffset = endPosition - startPosition;
                    controlPointOffset.y = 0;
                    controlPointOffset.x *= 0.8f;

                    Handles.DrawBezier(startPosition,
                                       endPosition,
                                       startPosition + controlPointOffset,
                                       endPosition - controlPointOffset,
                                       Color.white,
                                       null,
                                       4f);
                }
            }
        }

        private void ProcessEvents()
        {
            if ((Event.current.type == EventType.MouseDown) && (draggingNode == null))
            {
                Vector2 relativeMousePosition = Event.current.mousePosition + scrollPosition;

                draggingNode = GetNodeAtPoint(relativeMousePosition);
                if (draggingNode != null)
                {
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;

                    Selection.activeObject = draggingNode;
                }
                else
                {
                    draggingCanvas       = true;
                    draggingCanvasOffset = relativeMousePosition;

                    Selection.activeObject = selectedContract;
                }
            }
            else if ((Event.current.type == EventType.MouseDrag) && (draggingNode != null))
            {
                draggingNode.SetRectPosition(Event.current.mousePosition + draggingOffset);

                GUI.changed = true;
            }
            else if ((Event.current.type == EventType.MouseDrag) && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;

                GUI.changed = true;
            }
            else if ((Event.current.type == EventType.MouseUp) && (draggingNode != null))
            {
                draggingNode   = null;
                draggingOffset = Vector2.zero;
            }
            else if ((Event.current.type == EventType.MouseUp) && draggingCanvas)
            {
                draggingCanvas       = false;
                draggingCanvasOffset = Vector2.zero;
            }
        }

        private void DrawLinkButton(MultiTokenNode targetNode)
        {
            if ((linkingParentNode != null) && (linkingParentNode is MultiTokenContractNode))
            {
                var contractNode = (MultiTokenContractNode) linkingParentNode;

                if (contractNode.GetChildren().Contains(targetNode.name))
                {
                    if (GUILayout.Button("unlink"))
                    {
                        contractNode.RemoveChild(targetNode.name);
                        linkingParentNode = null;
                    }
                }
                else if (targetNode.name != contractNode.name)
                {
                    if (GUILayout.Button("child"))
                    {
                        contractNode.AddChild(targetNode.name);
                        linkingParentNode = null;
                    }
                }
                else
                {
                    if (GUILayout.Button("cancel"))
                    {
                        linkingParentNode = null;
                    }
                }
            }
        }

        private void DrawNode(MultiTokenNode node)
        {
            GUIStyle currentStyle =
                (node is MultiTokenMintNode) ? nodeTokenStyle : nodeStyle;

            GUILayout.BeginArea(node.GetRect(), currentStyle);

            /**
             ** NODE CREATION
             **/

            GUILayout.EndArea();            
        }

        private MultiTokenNode GetNodeAtPoint(Vector2 mousePosition)
        {
            var absoluteMousePosition = mousePosition;

            return selectedContract.GetAllNodes().LastOrDefault(x => x.GetRect().Contains(absoluteMousePosition));
        }
    }
}
