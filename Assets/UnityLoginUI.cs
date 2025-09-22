

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class UnityLoginUI : MonoBehaviour
{
    [Header("UI References - Email/Password")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI statusText;

    [Header("UI References - Code Login")]
    public TMP_InputField codeInput;
    public Button codeLoginButton;

    [Header("Panels")]
    public GameObject emailLoginPanel;
    public GameObject codeLoginPanel;
    public GameObject secondPanel;

    [Header("API Settings")]
    public string apiBaseUrl = "https://backendtccc-1.onrender.com/api";
    public string nextSceneName = "loopy";

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        codeLoginButton.onClick.AddListener(OnCodeLoginClicked);

        ShowEmailLogin();
    }

    #region Email/Password Login
    public void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "⚠️ Please enter email and password";
            return;
        }

        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post($"{apiBaseUrl}/auth/login", form))
        {
            www.timeout = 10;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + www.downloadHandler.text);
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                statusText.text = "❌ " + GetErrorMessage(www);
                Debug.LogError("Error: " + www.error);
            }
        }
    }
    #endregion

    #region Code Login
    public void OnCodeLoginClicked()
    {

        Debug.Log(" CodeLogin button clicked");
        string code = codeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "⚠️ Please enter the 6-digit code";
            Debug.Log("Please enter the 6-digit code");
            return;
        }

        //if (code.Length != 6 || !System.Text.RegularExpressions.Regex.IsMatch(code, @"^\\d{6}$"))
        //{
        //    statusText.text = "⚠️ Invalid code format";
        //    Debug.Log("Invalid code format");

        //    return;
        //}
        if (code.Length != 6 || !System.Text.RegularExpressions.Regex.IsMatch(code, @"^\d{6}$"))
        {
            statusText.text = "⚠️ Invalid code format";
            Debug.Log("Invalid code format");
            return;
        }


        StartCoroutine(CodeLoginRequest(code));
    }

    private IEnumerator CodeLoginRequest(string code)
    {
        CodeVerifyRequest requestData = new CodeVerifyRequest { code = code };
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/unity/verify-code", "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    CodeVerifyResponse response = JsonUtility.FromJson<CodeVerifyResponse>(request.downloadHandler.text);

                    if (response.success)
                    {
                        statusText.text = "✅ Login successful!";
                        Debug.Log($"Welcome {response.data.name}!");
                        SceneManager.LoadScene(nextSceneName);
                    }
                    else
                    {
                        statusText.text = "❌ " + response.message;
                        Debug.LogError($"Login failed: {response.message}");
                    }
                }
                catch (Exception e)
                {
                    statusText.text = "❌ Failed to parse server response";
                    Debug.LogError("JSON error: " + e.Message);
                }
            }
            else
            {
                statusText.text = "❌ " + GetErrorMessage(request);
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    #endregion

    #region Helpers
    public void ShowEmailLogin()
    {


       

        emailLoginPanel.SetActive(true);
        codeLoginPanel.SetActive(false);
    }

    public void ShowCodeLogin()
    {
        emailLoginPanel.SetActive(false);
        codeLoginPanel.SetActive(true);
    }

    private string GetErrorMessage(UnityWebRequest request)
    {
        if (request.responseCode == 400)
            return "Invalid request. Please check your input.";
        else if (request.responseCode == 401)
            return "Invalid credentials. Please try again.";
        else if (request.responseCode == 404)
            return "Not found. Please try again.";
        else if (request.responseCode == 500)
            return "Server error. Please try again later.";
        else if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            if (request.error == "Request timeout")
                return "Request timeout. Please try again.";
            else
                return "Connection error. Please check your internet connection.";
        }
        else
            return "An error occurred. Please try again.";
    }
    #endregion
}

[System.Serializable]
public class CodeVerifyRequest
{
    public string code;
}

[System.Serializable]
public class CodeVerifyResponse
{
    public bool success;
    public string message;
    public UserData data;
}

[System.Serializable]
public class UserData
{
    public string userId;
    public string studentId;
    public string name;
    public string email;
    public string picture;
}
