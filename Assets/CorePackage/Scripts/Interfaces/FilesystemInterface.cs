using System;
using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FilesystemInterface : Interface
{

    /// <summary>
    /// Initializes the class with the name Filesystem.
    /// For the filesystem is no login needed and it should appear in the data-browser.
    /// The Fileextension-Type-Map will be filled here.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator InitializeInternal()
    {
        Name = "Filesystem";
        OnSetupBrowser = SetupBrowser;
        yield break;
    }

    /// <summary>
    /// This static method is searching for all connected drives on the computer.
    /// </summary>
    /// <returns>The list of all drives as VRResourceData</returns>
    public static List<VRResourceData> GetDrives()
    {
        List<VRResourceData> drives = new List<VRResourceData>();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        VRResourceData parentDir = null;
        for (int i = 0; i < allDrives.Length; i++)
            if (HasAccesToLocalData(allDrives[i].RootDirectory.FullName))
                drives.Add(new VRResourceData(allDrives[i].RootDirectory.FullName, allDrives[i].RootDirectory.FullName, parentDir, "", DateTime.MaxValue, DateTime.MaxValue, VRData.SourceType.Local));
        return drives;
    }


    /// <summary>
    /// This method collects the content of a given directory.
    /// </summary>
    /// <param name="directory">The directory which content is to be returned.</param>
    /// <param name="filteredItems">The set of all wanted file types (collective names).</param>
    /// <param name="searchPattern">A search pattern to search for in the file name (optional).</param>
    /// <returns>The (filtered) content of the given directory as a list of VRResourceData</returns>
    public static List<VRResourceData> LoadDirectoryContent(VRResourceData directory, HashSet<string> filteredItems, string searchPattern)
    {
        if (directory.Type == VRResourceData.DataType.File ||
            filteredItems == null || filteredItems.Count == 0) return null;

        List<VRResourceData> dirContent = new List<VRResourceData>();
        DirectoryInfo actualDir = new DirectoryInfo(directory.Path);
        DirectoryInfo childDir;

        bool includeFolders = filteredItems.Contains(StolperwegeHelper.FOLDER);
        if (includeFolders)
        {
            for (int i = 0; i < actualDir.GetDirectories().Length; i++)
                if (HasAccesToLocalData(actualDir.GetDirectories()[i].FullName) &&
                    (searchPattern == null || searchPattern == "" ||
                    actualDir.GetDirectories()[i].Name.ToLower().Contains(searchPattern)))
                {
                    childDir = actualDir.GetDirectories()[i];
                    dirContent.Add(new VRResourceData(childDir.Name, childDir.FullName, directory, "", DateTime.MinValue, DateTime.MinValue, VRData.SourceType.Local));
                }
        }

        FileInfo[] files = actualDir.GetFiles();
        string fExt;

        if (filteredItems.Count > 1 || !includeFolders)
        {
            foreach (FileInfo file in files)
            {
                if (!HasAccesToLocalData(file.FullName)) continue;
                if (file.Attributes == FileAttributes.Hidden) continue;
                fExt = file.Extension.Contains(".") ? file.Extension.Replace(".", "") : file.Extension;
                if (((StolperwegeHelper.FileExtensionTypeMap.ContainsKey(fExt) && filteredItems.Contains(StolperwegeHelper.FileExtensionTypeMap[fExt])) ||
                    (!StolperwegeHelper.FileExtensionTypeMap.ContainsKey(fExt) && filteredItems.Contains(StolperwegeHelper.OTHER))) &&
                    (searchPattern == null || searchPattern == "" || file.Name.ToLower().Contains(searchPattern)))
                    dirContent.Add(new VRResourceData(file.Name.Replace("." + fExt, ""), file.FullName, directory, fExt, file.Length, "", DateTime.MinValue, DateTime.MinValue, VRData.SourceType.Local));

            }
        }        

        return dirContent;
    }

    /// <summary>
    /// This method checks whether the user has access / permission to the specified path.
    /// </summary>
    /// <param name="path">The specified path</param>
    /// <returns>True if the user has access to the specified path, otherwise false.</returns>
    public static bool HasAccesToLocalData(string path)
    {
        FileSystemSecurity security = null;
        try
        {
            if (Directory.Exists(path))
                security = Directory.GetAccessControl(path);

            if (File.Exists(path))
                security = File.GetAccessControl(path);
        }
        catch (Exception)
        {
            return false;
        }


        if (security == null) return false;
        AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(System.Security.Principal.SecurityIdentifier));
        if (rules == null) return false;
        bool access = true;
        foreach (FileSystemAccessRule rule in rules)
            access &= (rule.AccessControlType == AccessControlType.Allow);
        return access;
    }

    /// <summary>
    /// This method defines how the file browser should be initialized to navigate through the file system with it.
    /// </summary>
    /// <param name="browser">The instance of the DataBrowser, that should be setted up for browsing the file system</param>
    /// <returns></returns>
    public IEnumerator SetupBrowser(DataBrowser browser)
    {
        // Close any panels animated
        if (browser.FilterPanel.IsActive)
        {
            if (browser.SearchPanel.IsActive) StartCoroutine(browser.FilterPanel.Activate(false));
            else yield return StartCoroutine(browser.FilterPanel.Activate(false));
        }
        if (browser.SearchPanel.IsActive) yield return StartCoroutine(browser.SearchPanel.Activate(false));

        // ============================= FILTER PANEL SETUP ============================

        // Set filters
        if (!browser.DataSpaceFilterMap.ContainsKey(Name))
        {
            browser.DataSpaceFilterMap.Add(Name, new Dictionary<string, InteractiveCheckbox.CheckboxStatus> {
                { StolperwegeHelper.FOLDER, InteractiveCheckbox.CheckboxStatus.AllChecked } ,
                { StolperwegeHelper.TEXT, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.PICTURE, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.AUDIO, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.VIDEO, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.OTHER, InteractiveCheckbox.CheckboxStatus.AllChecked } });
        }

        // Define filter update event
        browser.FilterPanel.FilterUpdater = () =>
        {
            for (int i = 0; i < browser.FilterPanel.Checkboxes.Length; i++)
            {
                browser.FilterPanel.Checkboxes[i].gameObject.SetActive((browser.FilterPanel.TypePointer + i) < browser.FilterPanel.TypeList.Count);
                if (browser.FilterPanel.Checkboxes[i].gameObject.activeInHierarchy)
                {
                    browser.FilterPanel.Checkboxes[i].ButtonValue = browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i];
                    browser.FilterPanel.Checkboxes[i].Status = browser.FilterPanel.Types[browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i]];
                    browser.FilterPanel.Openers[i].gameObject.SetActive(false);
                }
            }
        };

        // Set event for changing checkboxes
        browser.FilterPanel.CheckboxUpdater = (type, status) => { browser.FilterPanel.Types[type] = status; };

        // Initialize filter panel
        browser.FilterPanel.Init("File Types", browser.DataSpaceFilterMap[Name]);

        // ============================= DATA PANEL SETUP ============================
        // Root button functionality
        browser.DataPanel.Root.gameObject.SetActive(true);
        browser.DataPanel.Root.OnClick = () =>
        {
            browser.SetActualState(Name, null);
            browser.DataPanel.Init("Local Drives", GetDrives());
            browser.DataPanel.ParentDir.Active = false;
            browser.DataPanel.Root.Active = false;
        };

        // Parent button functionality
        browser.DataPanel.ParentDir.OnClick = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null || actualDir.IsDrive)
            {
                browser.SetActualState(Name, null);
                browser.DataPanel.Init("Local Drives", GetDrives());
                browser.DataPanel.ParentDir.Active = false;
                browser.DataPanel.Root.Active = false;
            }
            else
            {
                browser.SetActualState(Name, actualDir.Parent);
                browser.DataPanel.Init(actualDir.Parent.Path, LoadDirectoryContent(actualDir.Parent, browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                browser.DataPanel.ParentDir.Active = actualDir.Parent.Parent != null;
                browser.DataPanel.Root.Active = actualDir.Parent.Parent != null;

            }
        };

        // Define browser update event
        browser.BrowserUpdater = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;
            browser.DataPanel.Init(actualDir.Path, LoadDirectoryContent(actualDir, browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
            browser.DataPanel.ParentDir.Active = actualDir != null;
            browser.DataPanel.Root.Active = actualDir != null;
        };
        
        // Define datacontainer events
        foreach (DataContainer dc in browser.DataPanel.DataContainers)
        {
            dc.OnClick = () =>
            {
                if (dc.Resource != null)
                {
                    VRResourceData resource = (VRResourceData)dc.Resource;
                    if (resource.Type != VRResourceData.DataType.File)
                    {
                        browser.SetActualState(Name, resource);
                        browser.DataPanel.Init(resource.Path, LoadDirectoryContent(resource, browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                        browser.DataPanel.ParentDir.Active = true;
                        browser.DataPanel.Root.Active = true;
                    }
                }
                
            };
        }

        // ============================= LOADING LAST STATE ============================ 

        StartCoroutine(browser.FilterPanel.Activate(true));
        StartCoroutine(browser.SearchPanel.Activate(true));
        VRResourceData dir = null;
        if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) dir = (VRResourceData)browser.LastBrowserStateMap[Name];
        if (dir == null) browser.DataPanel.Root.OnClick();
        else
        {
            browser.SetActualState(Name, dir.Path);
            browser.DataPanel.Init(dir.Path, LoadDirectoryContent(dir, browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
            browser.DataPanel.ParentDir.Active = dir != null;
            browser.DataPanel.Root.Active = dir != null;
        }
    }

}
