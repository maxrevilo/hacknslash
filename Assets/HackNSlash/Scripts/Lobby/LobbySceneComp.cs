using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Lobby
{
    public class LobbySceneComp : BaseComponent
    {
        public void GoToAdventureMode()
        {
            SceneManager.LoadScene("LabScape");
        }
        public void GoToWaveMode()
        {
            SceneManager.LoadScene("InfiniteBrawl");
        }
    }
}

