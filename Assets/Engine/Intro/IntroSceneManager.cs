using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField, Required] private Image loadingImage;
    [SerializeField, Required] private GameObject loggedInAsGameObject;
    [SerializeField, Required] private Text loggedInAsPreview;
    [SerializeField, Required] private UserAuthenticationController authenticationController;

    private bool isWaitingForAuthentication;
    private string username;

    // Start is called before the first frame update
    void Start()
    {
        //Cleanup
        loadingImage.gameObject.SetActive(false);
        loggedInAsGameObject.SetActive(false);
        loggedInAsPreview.gameObject.SetActive(false);

        //Try to log the user in
        isWaitingForAuthentication = true;

        //Start authentication process
        StartCoroutine(AuthenticationProcess());
    }
    
    IEnumerator AuthenticationProcess()
    {
        //Start loading animation
        loadingImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        
        //Authenticate
        authenticationController.AuthenticateUser((username) =>
        {
            this.username = username;
            this.isWaitingForAuthentication = false;
        });

        //Wait for response
        while (isWaitingForAuthentication)
            yield return null;

        //Display result
        loggedInAsPreview.text = "Welcome " + username;
        loggedInAsPreview.gameObject.SetActive(true);
        loggedInAsGameObject.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        //Load new scene
        SceneManager.LoadScene("menu");
    }
}
