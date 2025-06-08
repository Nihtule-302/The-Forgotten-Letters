using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ResourceManagement.ResourceLocations;
using _Project.Scripts.Core.Utilities.Scene_Management;

/// <summary>
/// Manages Addressable Assets for "The Forgotten Letters" game.
/// Handles content downloads, updates, progress tracking, and error handling.
/// </summary>
public class AddressablesManager : MonoBehaviour
{
    #region Singleton Setup
    
    public static AddressablesManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeUI();
    }
    
    #endregion
    
    #region Inspector Variables
    
    [Header("Content Settings")]

    [Tooltip("Check for Addressables catalog/content updates automatically on Start.")]
    [SerializeField] private bool checkForUpdatesOnStart = true;

    [Tooltip("Download all Addressables content automatically on Start.")]
    [SerializeField] private bool downloadAllContentOnStart = false;

    [Tooltip("Names of Addressables groups to download initially (if not downloading all content).")]
    [SerializeField] private string[] initialDownloadGroups;

    // [Tooltip("Maximum number of retry attempts for failed downloads.")]
    // [SerializeField] private int maxRetryAttempts = 3;

    // [Tooltip("Delay (in seconds) between retry attempts.")]
    // [SerializeField] private float retryDelay = 2f;
    
    [Header("UI References")]
    [Tooltip("Panel shown during download progress.")]
    [SerializeField] private GameObject downloadProgressPanel;

    [Tooltip("Slider UI element for download progress.")]
    [SerializeField] private Slider progressBar;

    [Tooltip("Text for displaying download status messages.")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Tooltip("Text for displaying total download size.")]
    [SerializeField] private TextMeshProUGUI downloadSizeText;

    [Tooltip("Text for displaying current download speed.")]
    [SerializeField] private TextMeshProUGUI downloadSpeedText;

    [Tooltip("Button to retry a failed download.")]
    [SerializeField] private Button retryButton;

    [Tooltip("Button to cancel an ongoing download.")]
    [SerializeField] private Button cancelButton;

    [Tooltip("Panel shown when an error occurs.")]
    [SerializeField] private GameObject errorPanel;

    [Tooltip("Text for displaying error messages.")]
    [SerializeField] private TextMeshProUGUI errorText;

    [Tooltip("Button to close the error panel.")]
    [SerializeField] private Button errorCloseButton;

    [Header("scene Transitioner")]
    [SerializeField] SceneTransitioner sceneTransitioner;
    
    #endregion

    #region Private Variables

    private bool _isInitialized = false;
    private bool _isDownloading = false;
    private List<AsyncOperationHandle> _activeDownloads = new List<AsyncOperationHandle>();
    private float _downloadStartTime;
    private long _totalBytesDownloaded;
    private long _previousBytesDownloaded;
    private float _lastProgressUpdateTime;
    private Coroutine _speedUpdateCoroutine;
    
    #endregion
    
    #region Events
    
    // Events for external systems to hook into
    public event Action OnInitializationComplete;
    public event Action<float> OnDownloadProgressUpdated;
    public event Action<string> OnDownloadComplete;
    public event Action<string> OnDownloadFailed;
    
    #endregion
    
    #region Lifecycle Methods
    
    private void Start()
    {
        Initialize();
    }
    
    private void OnDestroy()
    {
        // Clean up any active operations to prevent memory leaks
        foreach (var handle in _activeDownloads)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        
        _activeDownloads.Clear();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize the Addressables system and check for updates if configured
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
            return;
            
        Debug.Log("[AddressablesManager] Initializing...");
        
        // Initialize Addressables
        Addressables.InitializeAsync().Completed += handle => 
        {
            Debug.Log("[AddressablesManager] Initialization complete");
            _isInitialized = true;
            
            // Check for updates if configured to do so
            if (checkForUpdatesOnStart)
            {
                CheckForUpdates();
            }
            
            // Download initial content if configured
            if (downloadAllContentOnStart)
            {
                DownloadAllContent();
            }
            else if (initialDownloadGroups != null && initialDownloadGroups.Length > 0)
            {
                foreach (var group in initialDownloadGroups)
                {
                    DownloadContentGroup(group);
                }
            }
            
            OnInitializationComplete?.Invoke();
        };
    }
    
    /// <summary>
    /// Initialize UI components and set up button listeners
    /// </summary>
    private void InitializeUI()
    {
        if (downloadProgressPanel != null)
            downloadProgressPanel.SetActive(false);
            
        if (errorPanel != null)
            errorPanel.SetActive(false);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryCurrentDownload);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelCurrentDownload);
            
        if (errorCloseButton != null)
            errorCloseButton.onClick.AddListener(() => errorPanel.SetActive(false));
    }
    
    #endregion
    
    #region Content Update Management
    
    /// <summary>
    /// Check for content updates from the CDN
    /// </summary>
    public void CheckForUpdates()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[AddressablesManager] Cannot check for updates before initialization");
            return;
        }
        
        Debug.Log("[AddressablesManager] Checking for content updates...");
        ShowProgressUI("Checking for updates...");
        
        // Check if there's a newer content catalog available
        Addressables.CheckForCatalogUpdates().Completed += catalogUpdateHandle => 
        {
            List<string> catalogsToUpdate = catalogUpdateHandle.Result;
            
            if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
            {
                Debug.Log($"[AddressablesManager] Found {catalogsToUpdate.Count} catalog updates");
                
                // Update catalogs with new content
                Addressables.UpdateCatalogs(catalogsToUpdate).Completed += updateCatalogsHandle => 
                {
                    Debug.Log("[AddressablesManager] Catalogs updated successfully");
                    
                    // Notify user that updates are available
                    ShowProgressUI("Updates available. Ready to download.");
                    
                    Addressables.Release(updateCatalogsHandle);

                    // Now that update is done, release the original catalog update handle
                    Addressables.Release(catalogUpdateHandle);
                };
            }
            else
            {
                Debug.Log("[AddressablesManager] No catalog updates found");
                HideProgressUI();

                // Release the catalog update handle here since no update started
                Addressables.Release(catalogUpdateHandle);
            }
        };
    }

    
    #endregion
    
    #region Download Management
    
    /// <summary>
    /// Download all available content
    /// </summary>
    public void DownloadAllContent()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[AddressablesManager] Cannot download content before initialization");
            return;
        }
        
        Debug.Log("[AddressablesManager] Starting download of all content");
        
        // Get all resource locations
        Addressables.LoadResourceLocationsAsync("").Completed += locationsHandle => 
        {
            IList<IResourceLocation> locations = locationsHandle.Result;
            
            if (locations != null && locations.Count > 0)
            {
                // Calculate download size first
                GetDownloadSize(locations).Completed += sizeHandle => 
                {
                    long downloadSize = sizeHandle.Result;
                    
                    if (downloadSize > 0)
                    {
                        Debug.Log($"[AddressablesManager] Download size: {FormatFileSize(downloadSize)}");
                        ShowProgressUI($"Downloading all content ({FormatFileSize(downloadSize)})...");
                        
                        // Start the actual download
                        StartDownload(locations);
                    }
                    else
                    {
                        Debug.Log("[AddressablesManager] All content already downloaded");
                        ShowCompletionMessage("All content is up to date!");
                    }
                    
                    Addressables.Release(sizeHandle);
                };
            }
            else
            {
                Debug.Log("[AddressablesManager] No content to download");
                HideProgressUI();
            }
            
            Addressables.Release(locationsHandle);
        };
    }
    
    /// <summary>
    /// Download a specific content group
    /// </summary>
    /// <param name="groupName">The name of the content group to download</param>
    public void DownloadContentGroup(string groupName)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[AddressablesManager] Cannot download content before initialization");
            return;
        }
        
        if (string.IsNullOrEmpty(groupName))
        {
            Debug.LogError("[AddressablesManager] Group name cannot be null or empty");
            return;
        }
        
        Debug.Log($"[AddressablesManager] Starting download of group: {groupName}");
        
        // Get resource locations for the specified group
        Addressables.LoadResourceLocationsAsync(groupName).Completed += locationsHandle => 
        {
            IList<IResourceLocation> locations = locationsHandle.Result;
            
            if (locations != null && locations.Count > 0)
            {
                // Calculate download size first
                GetDownloadSize(locations).Completed += sizeHandle => 
                {
                    long downloadSize = sizeHandle.Result;
                    
                    if (downloadSize > 0)
                    {
                        Debug.Log($"[AddressablesManager] Download size for group {groupName}: {FormatFileSize(downloadSize)}");
                        ShowProgressUI($"Downloading {groupName} ({FormatFileSize(downloadSize)})...");
                        
                        // Start the actual download
                        StartDownload(locations, groupName);
                    }
                    else
                    {
                        Debug.Log($"[AddressablesManager] Group {groupName} already downloaded");
                        ShowCompletionMessage($"Group {groupName} is up to date!");
                    }
                    
                    Addressables.Release(sizeHandle);
                };
            }
            else
            {
                Debug.LogWarning($"[AddressablesManager] No content found for group: {groupName}");
                HideProgressUI();
            }
            
            Addressables.Release(locationsHandle);
        };
    }
    
    /// <summary>
    /// Start downloading content for the specified locations
    /// </summary>
    /// <param name="locations">The resource locations to download</param>
    /// <param name="groupName">Optional group name for tracking</param>
    private void StartDownload(IList<IResourceLocation> locations, string groupName = "")
    {
        _isDownloading = true;
        _downloadStartTime = Time.time;
        _totalBytesDownloaded = 0;
        _previousBytesDownloaded = 0;
        _lastProgressUpdateTime = Time.time;
        
        // Start download speed monitoring
        if (_speedUpdateCoroutine != null)
            StopCoroutine(_speedUpdateCoroutine);
            
        _speedUpdateCoroutine = StartCoroutine(UpdateDownloadSpeedCoroutine());
        
        // Start the download operation
        AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(locations, Addressables.MergeMode.Union);
        _activeDownloads.Add(downloadHandle);
        
        // Track progress
        downloadHandle.Completed += handle => 
        {
            _isDownloading = false;
            
            if (_speedUpdateCoroutine != null)
            {
                StopCoroutine(_speedUpdateCoroutine);
                _speedUpdateCoroutine = null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] Download completed successfully: {groupName}");
                ShowCompletionMessage(string.IsNullOrEmpty(groupName)
                    ? "All content downloaded successfully!"
                    : $"Group {groupName} downloaded successfully!");

                OnDownloadComplete?.Invoke(groupName);
                sceneTransitioner.TranstionToScene();
            }
            else
            {
                Debug.LogError($"[AddressablesManager] Download failed: {groupName}");
                HandleDownloadError(handle, groupName);
            }
            
            _activeDownloads.Remove(handle);
            Addressables.Release(handle);
        };
        
        // Set up progress tracking
        StartCoroutine(TrackDownloadProgress(downloadHandle, groupName));
    }
    
    /// <summary>
    /// Calculate the download size for the specified locations
    /// </summary>
    /// <param name="locations">The resource locations to check</param>
    /// <returns>AsyncOperationHandle with the download size in bytes</returns>
    private AsyncOperationHandle<long> GetDownloadSize(IList<IResourceLocation> locations)
    {
        return Addressables.GetDownloadSizeAsync(locations);
    }
    
    /// <summary>
    /// Track download progress and update UI
    /// </summary>
    private IEnumerator TrackDownloadProgress(AsyncOperationHandle handle, string groupName = "")
    {
        while (!handle.IsDone)
        {
            float progress = handle.PercentComplete;
            
            // Update UI
            UpdateProgressUI(progress, string.IsNullOrEmpty(groupName) 
                ? $"Downloading content... {(progress * 100):F0}%" 
                : $"Downloading {groupName}... {(progress * 100):F0}%");
            
            // Notify listeners
            OnDownloadProgressUpdated?.Invoke(progress);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Update download speed information
    /// </summary>
    private IEnumerator UpdateDownloadSpeedCoroutine()
    {
        while (_isDownloading)
        {
            // Update every 0.5 seconds
            yield return new WaitForSeconds(0.5f);
            
            if (_isDownloading && Time.time - _lastProgressUpdateTime >= 0.5f)
            {
                // Calculate download speed
                float deltaTime = Time.time - _lastProgressUpdateTime;
                long deltaBytes = _totalBytesDownloaded - _previousBytesDownloaded;
                
                if (deltaTime > 0)
                {
                    float downloadSpeed = deltaBytes / deltaTime;
                    
                    if (downloadSpeedText != null)
                    {
                        downloadSpeedText.text = $"{FormatFileSize((long)downloadSpeed)}/s";
                    }
                }
                
                _previousBytesDownloaded = _totalBytesDownloaded;
                _lastProgressUpdateTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// Retry the current download operation
    /// </summary>
    private void RetryCurrentDownload()
    {
        if (_activeDownloads.Count > 0)
        {
            Debug.Log("[AddressablesManager] Retrying download...");
            
            // Cancel current downloads
            CancelCurrentDownload();
            
            // Restart the download process
            DownloadAllContent();
        }
    }
    
    /// <summary>
    /// Cancel the current download operation
    /// </summary>
    private void CancelCurrentDownload()
    {
        if (_activeDownloads.Count > 0)
        {
            Debug.Log("[AddressablesManager] Cancelling downloads...");
            
            foreach (var handle in _activeDownloads)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            
            _activeDownloads.Clear();
            _isDownloading = false;
            
            if (_speedUpdateCoroutine != null)
            {
                StopCoroutine(_speedUpdateCoroutine);
                _speedUpdateCoroutine = null;
            }
            
            HideProgressUI();
        }
    }
    
    /// <summary>
    /// Clear the Addressables cache to free up space
    /// </summary>
    public void ClearCache()
    {
        Debug.Log("[AddressablesManager] Clearing Addressables cache...");
        
        Addressables.ClearDependencyCacheAsync("");
        Debug.Log("[AddressablesManager] Cache cleared successfully");
    }

    
    #endregion
    
    #region UI Management
    
    /// <summary>
    /// Show the progress UI with the specified status message
    /// </summary>
    /// <param name="status">Status message to display</param>
    private void ShowProgressUI(string status)
    {
        if (downloadProgressPanel != null)
            downloadProgressPanel.SetActive(true);
            
        if (statusText != null)
            statusText.text = status;
            
        if (progressBar != null)
            progressBar.value = 0;
            
        if (downloadSizeText != null)
            downloadSizeText.text = "";
            
        if (downloadSpeedText != null)
            downloadSpeedText.text = "";
    }
    
    /// <summary>
    /// Update the progress UI with the current progress
    /// </summary>
    /// <param name="progress">Progress value (0-1)</param>
    /// <param name="status">Status message to display</param>
    private void UpdateProgressUI(float progress, string status)
    {
        if (progressBar != null)
            progressBar.value = progress;
            
        if (statusText != null)
            statusText.text = status;
    }
    
    /// <summary>
    /// Show a completion message and hide the progress UI after a delay
    /// </summary>
    /// <param name="message">Completion message to display</param>
    private void ShowCompletionMessage(string message)
    {
        if (statusText != null)
            statusText.text = message;
            
        if (progressBar != null)
            progressBar.value = 1;
            
        // Hide the UI after a delay
        StartCoroutine(HideProgressUIDelayed(2f));
    }
    
    /// <summary>
    /// Hide the progress UI after a delay
    /// </summary>
    /// <param name="delay">Delay in seconds</param>
    private IEnumerator HideProgressUIDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideProgressUI();
    }
    
    /// <summary>
    /// Hide the progress UI
    /// </summary>
    private void HideProgressUI()
    {
        if (downloadProgressPanel != null)
            downloadProgressPanel.SetActive(false);
    }
    
    /// <summary>
    /// Show an error message in the error panel
    /// </summary>
    /// <param name="error">Error message to display</param>
    private void ShowErrorMessage(string error)
    {
        if (errorPanel != null)
            errorPanel.SetActive(true);
            
        if (errorText != null)
            errorText.text = error;
    }
    
    #endregion
    
    #region Error Handling
    
    /// <summary>
    /// Handle download errors and implement retry logic
    /// </summary>
    /// <param name="handle">The failed operation handle</param>
    /// <param name="groupName">The group name for context</param>
    private void HandleDownloadError(AsyncOperationHandle handle, string groupName = "")
    {
        string errorMessage = $"Failed to download content";
        
        if (!string.IsNullOrEmpty(groupName))
            errorMessage += $" for group {groupName}";
            
        if (handle.OperationException != null)
            errorMessage += $": {handle.OperationException.Message}";
            
        Debug.LogError($"[AddressablesManager] {errorMessage}");
        
        // Show error in UI
        ShowErrorMessage(errorMessage);
        
        // Notify listeners
        OnDownloadFailed?.Invoke(groupName);
        
        // Implement fallback strategy
        FallbackToBuiltInContent();
    }
    
    /// <summary>
    /// Implement a fallback strategy when downloads fail
    /// </summary>
    private void FallbackToBuiltInContent()
    {
        Debug.Log("[AddressablesManager] Falling back to built-in content");
        
        // Here you would implement logic to use built-in content instead of downloaded content
        // This could involve loading assets from Resources folder or using simplified versions
        
        // For example:
        // GameManager.Instance.UseBuiltInAssets = true;
    }
    
    /// <summary>
    /// Log detailed error information for debugging
    /// </summary>
    /// <param name="exception">The exception to log</param>
    private void LogErrorDetails(Exception exception)
    {
        if (exception == null)
            return;
            
        Debug.LogError($"[AddressablesManager] Error Details: {exception.Message}");
        Debug.LogError($"[AddressablesManager] Stack Trace: {exception.StackTrace}");
        
        if (exception.InnerException != null)
        {
            Debug.LogError($"[AddressablesManager] Inner Exception: {exception.InnerException.Message}");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Format a file size in bytes to a human-readable string
    /// </summary>
    /// <param name="bytes">Size in bytes</param>
    /// <returns>Formatted string (e.g., "1.5 MB")</returns>
    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;
        
        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }
        
        return $"{size:F2} {suffixes[suffixIndex]}";
    }
    
    #endregion
}
