using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

namespace Nethereum.Unity.MultiToken
{
    [System.Serializable]
    public class MultiTokenMetadata
    {
        public string metadataUri = string.Empty;

        public string name;

        public string description;

        public string image;

        public string properties;

        public Dictionary<string, string> tokenPropertiesMap = new Dictionary<string, string>();

        protected MultiTokenMetadata()
        { }

        static public MultiTokenMetadata RetrieveTokenMetadata(string tokenMetadataUri)
        {        
            var jsonMetadata = new MultiTokenMetadata();

            var httpClient = new HttpClient();

            try
            {
                var jsonMetadataPayload = httpClient.GetStringAsync(tokenMetadataUri).Result;

                jsonMetadata = JsonUtility.FromJson<MultiTokenMetadata>(jsonMetadataPayload);

                jsonMetadata.metadataUri = tokenMetadataUri;
            }
            catch (Exception ex)
            {
                Debug.Log("ERROR!  Could not retrieve/parse the metadata payload.");
            }

            return jsonMetadata;
        }

    }
}
