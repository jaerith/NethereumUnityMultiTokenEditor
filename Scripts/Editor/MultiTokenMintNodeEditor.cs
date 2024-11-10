using System;
using System.Collections;

using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

namespace Nethereum.Unity.MultiToken
{
    [CustomEditor(typeof(MultiTokenMintNode))]
    [CanEditMultipleObjects]
    public class MultiTokenMintNodeEditor : Editor
    {
        MultiTokenMintNode _mintNode = null;

        Texture _tokenImage = null;

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
                if (!String.IsNullOrEmpty(_mintNode.TokenImageUri))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    GUILayout.EndHorizontal();

                    if (_tokenImage != null)
                    {
                        GUILayout.Box(_tokenImage);
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
