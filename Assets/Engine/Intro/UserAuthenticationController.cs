using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using System.Collections;

public class UserAuthenticationController : MonoBehaviour
{
    [Header("General")]
    [SerializeField, Required] private ServerConfig serverProperties;
    [SerializeField, Required] private Animator userActionsPanel;

    [Header("Navigation")]
    [SerializeField, Required] private ToggleGroup navigationGroup;
    [SerializeField, Required] private Toggle loginGroup;
    [SerializeField, Required] private Toggle signupGroup;

    [SerializeField] private Color toggleOnColor;
    [SerializeField] private Color toggleOffColor;

    [Header("Login")]
    [SerializeField, Required] private GameObject lPanel;
    [SerializeField, Required] private Text lResponseCode;
    [SerializeField, Required] private InputField lUsernameInput;
    [SerializeField, Required] private InputField lPasswordInput;
    [SerializeField, Required] private Button lSubmitButton;

    [Header("Signup")]
    [SerializeField, Required] private GameObject sPanel;
    [SerializeField, Required] private Text sResponseCode;
    [SerializeField, Required] private InputField sUsernameInput;
    [SerializeField, Required] private InputField sPasswordInput;
    [SerializeField, Required] private InputField sPasswordConfirmInput;
    [SerializeField, Required] private Button sSubmitButton;

    [Header("Response")]
    [SerializeField] private Color errorColor;
    [SerializeField] private Color successColor;
    private enum ResponseState { Success, Error}

    private Action<string> onLoggedInEvent;
    Regex usernameRegex = new Regex("[a-zA-Z0-9]{12,24}$");
    Regex passwordRegex = new Regex("^[a-zA-Z0-9!#$%^&*]{12,24}$");

    private void Awake()
    {
        //Cleanup by default
        Cleanup();

        //Register listeners
        loginGroup.onValueChanged.AddListener(OnLoginToggleChanged);
        lSubmitButton.onClick.AddListener(OnLoginSubmitButtonClicked);
        sSubmitButton.onClick.AddListener(OnSignupSubmitButtonClicked);
    }

    public void AuthenticateUser(Action<string> onLoggedInEvent)
    {
        this.onLoggedInEvent = onLoggedInEvent;

        //If username and password is saved in cache
        if (PlayerPrefs.HasKey("vrbike_username") && PlayerPrefs.HasKey("vrbike_password"))
        {
            string username = PlayerPrefs.GetString("vrbike_username");
            string password = PlayerPrefs.GetString("vrbike_password");

            //Send POST Request to confirm if this user is correct
            StartCoroutine(ServerUtils.SendHTTPPostRequestEnum(serverProperties.LoginUserAPI,
                (long responseCode) =>
                {
                    //If the user logged in, alert the main controller
                    if (responseCode == 200)
                        onLoggedInEvent(username);
                    else
                        ManualAuthentication();
                }, new KeyValuePair<string, string>("username", username), new KeyValuePair<string, string>("password", password)));
        }
        else
        {
            ManualAuthentication();
        }
    }

    private void ManualAuthentication()
    {
        //Open the panels
        userActionsPanel.gameObject.SetActive(true);
        userActionsPanel.SetInteger("sate", 0);

        navigationGroup.gameObject.SetActive(true);
        loginGroup.isOn = true;
        lPanel.SetActive(true);
    }

    private void Cleanup()
    {
        userActionsPanel.gameObject.SetActive(false);
        lPanel.SetActive(false);
        sPanel.SetActive(false);
    }

    #region UI Events & Utils
    private void OnLoginToggleChanged(bool value)
    {
        lPanel.SetActive(false);
        sPanel.SetActive(false);
        loginGroup.image.color = signupGroup.image.color = toggleOffColor;

        if (value)
        {
            lPanel.SetActive(true);
            loginGroup.image.color = toggleOnColor;
        }
        else
        {
            sPanel.SetActive(true);
            signupGroup.image.color = toggleOnColor;
        }
    }

    private void OnSignupSubmitButtonClicked()
    {
        string username = sUsernameInput.text;
        string password = sPasswordInput.text;
        string passwordConfirm = sPasswordConfirmInput.text;

        //Client side check
        if (!password.Equals(passwordConfirm))
        {
            DisplaySignupMessage("Error: The passwords do not match", ResponseState.Error);
            return;
        }
        if (!usernameRegex.IsMatch(username))
        {
            DisplaySignupMessage("Error: Username not in the right format", ResponseState.Error);
            return;
        }

        if (!passwordRegex.IsMatch(password))
        {
            DisplaySignupMessage("Error: Password not in the right format", ResponseState.Error);
            return;
        }

        //Prepare POST Request
        StartCoroutine(ServerUtils.SendHTTPPostRequestEnum(serverProperties.CreateUserAPI, (long code) => {
            if (code == 200) { 
                DisplaySignupMessage("Account created successfully", ResponseState.Success);
                sUsernameInput.text = sPasswordInput.text = sPasswordConfirmInput.text = "";
            }
            else if (code == 400)
                DisplaySignupMessage("Error: Input in wrong format", ResponseState.Error);
            else if (code == 409)
                DisplaySignupMessage("Error: Account already exists", ResponseState.Error);
            else
                DisplaySignupMessage("Error: Internal Server Error", ResponseState.Error);

        }, new KeyValuePair<string, string>("username", username), new KeyValuePair<string, string>("password", password)));
    }

    private void DisplaySignupMessage(string text, ResponseState state)
    {
        if (state == ResponseState.Success)
            sResponseCode.color = successColor;
        else if (state == ResponseState.Error)
            sResponseCode.color = errorColor;

        sResponseCode.text = text;
    }

    private void OnLoginSubmitButtonClicked()
    {
        string username = lUsernameInput.text;
        string password = lPasswordInput.text;

        //Client side check
        if (!usernameRegex.IsMatch(username))
        {
            DisplayLoginMessage("Error: Username not in the right format", ResponseState.Error);
            return;
        }

        if (!passwordRegex.IsMatch(password))
        {
            DisplayLoginMessage("Error: Password not in the right format", ResponseState.Error);
            return;
        }

        //Prepare POST Request
        StartCoroutine(ServerUtils.SendHTTPPostRequestEnum(serverProperties.LoginUserAPI, (long code) => {
            if (code == 200)
            {
                //Save to cache
                PlayerPrefs.SetString("vrbike_username", username);
                PlayerPrefs.SetString("vrbike_password", password);

                //Logged in, clear everything and get back
                StartCoroutine(GetBackToMainScreen(username));
            }
            else if (code == 400)
                DisplayLoginMessage("Error: Input in wrong format", ResponseState.Error);
            else if (code == 409)
                DisplayLoginMessage("Error: Wrong password given", ResponseState.Error);
            else
                DisplayLoginMessage("Error: Internal Server Error", ResponseState.Error);

        }, new KeyValuePair<string, string>("username", username), new KeyValuePair<string, string>("password", password)));

    }

    private IEnumerator GetBackToMainScreen(string username)
    {
        userActionsPanel.SetInteger("state", 1);
        yield return new WaitForSeconds(1.2f);

        Cleanup();
        onLoggedInEvent(username);
    }

    private void DisplayLoginMessage(string text, ResponseState state)
    {
        if (state == ResponseState.Success)
            lResponseCode.color = successColor;
        else if (state == ResponseState.Error)
            lResponseCode.color = errorColor;

        lResponseCode.text = text;
    }
    #endregion
}
