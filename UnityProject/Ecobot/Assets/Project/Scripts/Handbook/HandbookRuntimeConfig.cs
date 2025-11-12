using UnityEngine;

namespace Handbook
{
    [CreateAssetMenu(fileName = "HandbookRuntimeConfig", menuName = "Project/Handbook/Runtime Config")]
    public class HandbookRuntimeConfig : ScriptableObject
    {
        public string RootFolder;     
        public string Language = "ru";
        public string MediaBasePath = "media";
        public string Version = "1.0.0";
    }
}