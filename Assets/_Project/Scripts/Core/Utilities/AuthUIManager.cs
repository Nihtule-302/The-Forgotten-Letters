using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TheForgottenLetters
{
    public class AuthUIManager : MonoBehaviour
    {
        [Header("Root UI Panels")]
        public GameObject authRoot;
        public GameObject loginPanel;
        public GameObject registerPanel;
        public GameObject registerStepBasicInfoPanel;
        public GameObject registerStepPasswordPanel;

        [Header("Login Inputs")]
        public TMP_InputField loginEmailInput;
        public TMP_InputField loginPasswordInput;

        [Header("Registration Step Basic Info Inputs")]
        public TMP_InputField regFullNameInput;
        public TMP_InputField regUsernameInput;
        public TMP_InputField regEmailInput;

        [Header("Registration Step Password Inputs")]
        public TMP_InputField regPasswordInput;
        public TMP_InputField regConfirmPasswordInput;

        [Header("Status Panel")]
        public GameObject statusPanel;
        public TMP_Text statusMessageText;

        void Awake()
        {
            ShowAuthRoot();
        }

        public void BeginLoginFlow()
        {
            SwitchToLoginPanel();
        }

        public void BeginRegistrationFlow()
        {
            SwitchToRegisterPanel();
        }

        public void ShowAuthRoot()
        {
            ClearLoginInputs();
            ClearRegistrationInputs();
            authRoot.SetActive(true);
            HideRegisterPanel();
            SwitchToLoginPanel();
        }

        public void HideAuthRoot()
        {
            authRoot.SetActive(false);
            ClearLoginInputs();
            ClearRegistrationInputs();
        }

        public void SwitchToLoginPanel()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            ClearLoginInputs();
        }

        public void HideLoginPanel()
        {
            loginPanel.SetActive(false);
            ClearLoginInputs();
        }

        public void SwitchToRegisterPanel()
        {
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
            ShowRegisterStepBasicInfo();
        }

        public void HideRegisterPanel()
        {
            registerPanel.SetActive(false);
            registerStepBasicInfoPanel.SetActive(false);
            registerStepPasswordPanel.SetActive(false);
            ClearRegistrationInputs();
        }

        public void ShowRegisterStepBasicInfo()
        {
            registerStepBasicInfoPanel.SetActive(true);
            registerStepPasswordPanel.SetActive(false);
        }

        public void ShowRegisterStepCredentials()
        {
            registerStepBasicInfoPanel.SetActive(false);
            registerStepPasswordPanel.SetActive(true);
        }

        public void ShowStatusMessage(string message)
        {
            DisplayStatusMessage(message).Forget();
        }

        private async UniTask DisplayStatusMessage(string message)
        {
            statusPanel.SetActive(true);
            statusMessageText.text = message;
            Debug.Log($"Status: {message}");
            await UniTask.Delay(2000);
            statusPanel.SetActive(false);
        }

        private void ClearLoginInputs()
        {
            loginEmailInput.text = string.Empty;
            loginPasswordInput.text = string.Empty;
        }

        private void ClearRegistrationInputs()
        {
            regFullNameInput.text = string.Empty;
            regUsernameInput.text = string.Empty;
            regEmailInput.text = string.Empty;
            regPasswordInput.text = string.Empty;
            regConfirmPasswordInput.text = string.Empty;
        }

    }
}
