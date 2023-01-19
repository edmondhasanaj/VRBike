using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class MainMenuController : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("bike_physics_test");
    }
}
