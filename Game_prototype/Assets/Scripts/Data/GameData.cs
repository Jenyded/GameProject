using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
    public class GameData : ScriptableObject
    {
        public AssetReference metaSceneReference;
        public AssetReference battleSceneReference;
        public AssetReference windowsDataReference;
        public CharacterData characterData;
    }
}