using System;
using System.Collections;

using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;

namespace Nethereum.Unity.MultiToken
{
    [CustomEditor(typeof(MultiTokenMintNode))]
    [CanEditMultipleObjects]
    public class MultiTokenMintNodeEditor : Editor
    {
        MultiTokenMintNode _mintNode = null;

        Texture _tokenImage = null;

        Vector2 _textScrollPosition = Vector2.zero;

        Vector2 _imageScrollPosition = Vector2.zero;

        void OnEnable()
        {
            _mintNode = target as MultiTokenMintNode;

            if (_mintNode.IsDeployed)
            {
                if (!String.IsNullOrEmpty(_mintNode.TokenImageUri))
                {
                    EditorCoroutineUtility.StartCoroutine(DownloadImage(_mintNode.TokenImageUri), this);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_mintNode.IsDeployed)
            {
                if (!String.IsNullOrEmpty(_mintNode.MetadataJson))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Metadata Json");
                    GUILayout.EndHorizontal();

                    _textScrollPosition = 
                        GUILayout.BeginScrollView(_textScrollPosition, false, true, GUILayout.MaxHeight(200));
                    EditorGUILayout.TextArea(_mintNode.MetadataJson);
                    GUILayout.EndScrollView();
                }

                if (!String.IsNullOrEmpty(_mintNode.TokenImageUri))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Token Image");
                    GUILayout.EndHorizontal();

                    if (_tokenImage != null)
                    {
                        _imageScrollPosition = 
                            GUILayout.BeginScrollView(_imageScrollPosition, false, true);
                        GUILayout.Box(_tokenImage);
                        GUILayout.EndScrollView();
                    }
                }
            }
        }

        IEnumerator DownloadImage(string imageUrl)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
                _tokenImage = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}
