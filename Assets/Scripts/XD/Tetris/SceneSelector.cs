using UnityEngine;

namespace XD.TETRIS
{
    public class SceneSelector: MonoBehaviour
    {
        /// <summary> Загрузка сцены по ее имени.</summary>
        /// <param name="sceneName">Имя сцены.</param>
        public void SelectScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
            #if DEBUG_MODE
            Debug.Log("Scene " + sceneName);
            #endif
            
        }
    }
}