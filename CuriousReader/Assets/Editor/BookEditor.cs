using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Audio;
using System.IO;
using System;
using System.Text.RegularExpressions;
using Unity.VectorGraphics;
using Elendow.SpritedowAnimator;
using CuriousReader.Performance;
using CuriousReader.BookBuilder;
using System.Text;

/// <summary>
/// Used for parsing the page metadata file
/// </summary>
struct PageDirective
{
    public int StartPage;
    public int EndPage;
    public int RepeatCount;

    public PageDirective(int i_startPage, int i_endPage, int i_repeatCount)
    {
        StartPage = i_startPage;
        EndPage = i_endPage;
        RepeatCount = i_repeatCount;
    }
}

public class BookEditor : EditorWindow
{
    StoryBookJson m_rcStoryBook;

    public bool m_bMicrophoneHot = false;
    public bool m_bWasMicrophoneHotLastFrame = false;
    public AudioClip m_rcRecordingClip;
    float m_fRecordingStart;

    string m_strAssetBundleName = "differentplaces";

    public string m_strBookPath;
    public string m_loadedBookNameWithoutExtension;
    string m_strAssetPath;
    string m_strCommonPath;
    string m_strBookRoot;
    string m_strAnimPath;
    string m_strImagePath;

    Rect[] m_rcAudioRects;

    bool[] m_bShowPage;

    AudioClip m_rcTempAudioClip;

    AudioClip[] m_rcPageAudio;

    public string[] m_rastrAnimationNames;

    Vector2 m_vScrollPosition;

    GUIStyle m_boldFoldoutStyle;
    bool    m_foldoutPathsText      = false;
    bool    m_foldoutPageData       = false;
    bool    m_foldoutPageText       = false;
    bool    m_foldoutPageAudio      = false;
    bool    m_foldoutTimestamps     = false;
    bool    m_foldoutGameObjects    = false;
    bool    m_foldoutTriggers       = false;

    HashSet<string> m_assetsMissingForPage = new HashSet<string>();

    int     m_activePageID = 0;

    // Used when we need to replace current book data with a new one, settings this true when attempting to load the
    // new book will actually load the new data from the path provided
    public bool m_needToLoadBookContent;
    public FileSystemWatcher m_bookFileLoadWatcher = null;
    
    bool m_savedRecentlyFromThisEditor = false;

    private void OnGUI()
    {

        m_boldFoldoutStyle = EditorStyles.foldout;

        if (m_bookFileLoadWatcher == null && !string.IsNullOrEmpty(m_strBookPath))
            addFileWatcherForReloadingAtPath(m_strBookPath);

        if (m_rcStoryBook == null || m_needToLoadBookContent)
        {
            if (!string.IsNullOrEmpty(m_strBookPath))
            {
                string strBook = File.ReadAllText(m_strBookPath);
                m_rcStoryBook = JsonUtility.FromJson<StoryBookJson>(strBook);

                if (m_rcStoryBook != null || m_needToLoadBookContent)
                {
                    m_strBookRoot = m_strBookPath;
                    m_strCommonPath = m_strBookPath.Replace(System.IO.Path.GetFileName(m_strBookRoot), "");
#if UNITY_EDITOR_OSX
                    m_strCommonPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(m_strCommonPath).FullName).FullName).FullName).FullName + "/Common";
#endif

#if UNITY_EDITOR_WIN
                    m_strCommonPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(m_strCommonPath).FullName).FullName).FullName).FullName + "\\Common";
#endif
                    string strAssetPath = Application.dataPath.Replace("/Assets", "");
                    m_strAssetPath = strAssetPath;
#if UNITY_EDITOR_WIN
                    m_strAnimPath = m_strCommonPath + "\\Animations";
                    m_strImagePath = m_strCommonPath + "\\Images";
#endif

#if UNITY_EDITOR_OSX
                    m_strAnimPath = m_strCommonPath + "/Animations";
                    m_strImagePath = m_strCommonPath + "/Images";
#endif
                    m_strAnimPath = m_strAnimPath.Replace(strAssetPath, "").TrimStart('/');
                    m_strImagePath = m_strImagePath.Replace(strAssetPath, "").TrimStart('/');

                    // Allocate each of the audio tracks for each of the pages
                    // NOTE: We might need to do this for sub-audio?  Maybe?
                    m_rcAudioRects = new Rect[m_rcStoryBook.pages.Length];
                    m_rcPageAudio = new AudioClip[m_rcStoryBook.pages.Length];

                    m_needToLoadBookContent = false;
                    m_loadedBookNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(m_strBookPath);

                    initializeNewSetOfMissingAssets(); 
                    this.ShowNotification(new GUIContent("Loading: " + m_loadedBookNameWithoutExtension + "!"));
                    // addFileWatcherForReloadingAtPath(m_strBookPath);
                }

                m_rastrAnimationNames = GetAnimationNames();

            }
            m_needToLoadBookContent = false;
            return;
        }

        #region Title & Current Page 

        // Editor Title
        GUILayout.Space(8);

        GUIStyle bookEditorLabelStyle = new GUIStyle();
        bookEditorLabelStyle.alignment = TextAnchor.UpperCenter;
        bookEditorLabelStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("Book Editor (" + m_loadedBookNameWithoutExtension + ")", bookEditorLabelStyle);

        GUILayout.Space(4);
        
        // Edit Page
        m_vScrollPosition = GUILayout.BeginScrollView(m_vScrollPosition);
        
        string[] pageTexts = new string[m_rcStoryBook.pages.Length];
        for (int i = 0; i < m_rcStoryBook.pages.Length; i++) {
            if (m_rcStoryBook.pages[i].texts != null && m_rcStoryBook.pages[i].texts.Length != 0)
                pageTexts[i] = "Page " + i.ToString() + " " + m_rcStoryBook.pages[i].texts[0].text;
            else
                pageTexts[i] = "Page " + i.ToString();
        }

        GUIStyle currentPageDropdownStyle = EditorStyles.popup;
        currentPageDropdownStyle.fontStyle = FontStyle.Bold;
        currentPageDropdownStyle.fontSize = 10;
        currentPageDropdownStyle.fixedHeight = 20;
        
        int currentPageID = m_activePageID;
        m_activePageID = EditorGUILayout.Popup("Current Page", m_activePageID, pageTexts, currentPageDropdownStyle, GUILayout.ExpandWidth(false), GUILayout.Width(580));

        if (currentPageID != m_activePageID)
        {
            initializeNewSetOfMissingAssets();
        }

        currentPageDropdownStyle.fontStyle = FontStyle.Normal;
        currentPageDropdownStyle.fontSize = 9;
        currentPageDropdownStyle.fixedHeight = 16;

        GUILayout.Space(10);

        EditPage(m_rcStoryBook.pages[m_activePageID], m_activePageID);

        GUILayout.EndScrollView();

        #endregion

        #region Story Info & Parameters

        GUILayout.FlexibleSpace();

        GUILayout.Space(4);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutPathsText = EditorGUILayout.Foldout(m_foldoutPathsText, "Story Info & Parameters", m_boldFoldoutStyle);

        // Setting this back to normal, else we see bold font on all foldouts
        SetBoldFoldoutStyleToNormal();

        if (m_foldoutPathsText) {
            GUILayout.Label("Path Values");

            if (!string.IsNullOrEmpty(m_strBookPath)) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Story Book Path", m_strBookPath.Replace("/","\\"));
                EditorGUI.EndDisabledGroup();
            }

            if (!string.IsNullOrEmpty(m_strCommonPath)) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Common Path", m_strCommonPath.Replace("/", "\\"));
                EditorGUI.EndDisabledGroup();
            }

            if (!string.IsNullOrEmpty(m_strAnimPath)) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Animations Path", m_strAnimPath.Replace("/", "\\"));
                EditorGUI.EndDisabledGroup();
            }

            if (!string.IsNullOrEmpty(m_strAssetPath)) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Assets Path", m_strAssetPath.Replace("/", "\\"));
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.Space(10);
        
            GUILayout.Label("Story Parameters");

            m_rcStoryBook.id = EditorGUILayout.IntField("id", m_rcStoryBook.id, EditorStyles.numberField);
            m_rcStoryBook.language = EditorGUILayout.TextField("Language", m_rcStoryBook.language, EditorStyles.textField);
            m_rcStoryBook.fontFamily = EditorGUILayout.TextField("Font Family", m_rcStoryBook.fontFamily, EditorStyles.textField);
            m_rcStoryBook.fontColor = EditorGUILayout.TextField("Font Family", m_rcStoryBook.fontColor, EditorStyles.textField);
            m_rcStoryBook.textFontSize = EditorGUILayout.IntField("Font Size", m_rcStoryBook.textFontSize, EditorStyles.numberField);
            m_rcStoryBook.textStartPositionX = EditorGUILayout.FloatField("Text Start X", m_rcStoryBook.textStartPositionX, EditorStyles.numberField);
            m_rcStoryBook.textStartPositionY = EditorGUILayout.FloatField("Text Start Y", m_rcStoryBook.textStartPositionY, EditorStyles.numberField);
        }

        EditorGUILayout.EndVertical();

        #endregion

        #region Story Automation With New Hierarchy
        
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Metadata And Animations", GUILayout.Height(24))) 
        {
            DirectoryInfo commonDirectory = new DirectoryInfo(m_strCommonPath);

            Dictionary<string, string> storyPathsLookup = CreateSubDirectoryPathsLookup(commonDirectory);


            if (storyPathsLookup.Count == 0 || !storyPathsLookup.ContainsKey("Objects"))
            {
                Debug.LogError("Story path: " + commonDirectory.FullName + " doesn't contain Common directory.");
                return;
            }

            DirectoryInfo storyObjectsDirectoryInfo = new DirectoryInfo(storyPathsLookup["Objects"]);

            Dictionary<string, string> storyObjectPathsLookup = CreateSubDirectoryPathsLookup(storyObjectsDirectoryInfo);

            if (storyObjectPathsLookup.Count == 0)
            {
                Debug.LogError("Story Objects Directory is Empty!");
                return;
            }

            Dictionary<string, List<string>> sceneObjectNamesSet = new Dictionary<string, List<string>>();

            string[] allowedFileFormats = new string[] { ".svg", ".png" };

            foreach (KeyValuePair<string, string> sceneObjectPath in storyObjectPathsLookup) 
            {
                DirectoryInfo sceneObjectPathInfo = new DirectoryInfo(sceneObjectPath.Value);
                FileInfo[] objectFiles = sceneObjectPathInfo.GetFiles();
                foreach (FileInfo objectFileName in objectFiles) 
                {
                    if (Array.IndexOf(allowedFileFormats, objectFileName.Extension.ToLower()) < 0)
                        continue;
                    string[] objectFileNamePieces = System.IO.Path.GetFileNameWithoutExtension(objectFileName.FullName).Split('-');
                    if (objectFileNamePieces.Length == 1 || objectFileNamePieces.Length == 2)
                    {
                        if (!sceneObjectNamesSet.ContainsKey(objectFileNamePieces[0]))
                            sceneObjectNamesSet.Add(objectFileNamePieces[0], null);
                    } else if (objectFileNamePieces.Length == 3) 
                    {
                        string objectAndAnimationName = string.Format(
                            "{0}-{1}", objectFileNamePieces[0], objectFileNamePieces[1]);

                        if (!sceneObjectNamesSet.ContainsKey(objectAndAnimationName))
                            sceneObjectNamesSet.Add(objectAndAnimationName, new List<string>());
                        sceneObjectNamesSet[objectAndAnimationName].Add(
                            string.Format("{0}-{1}", objectFileNamePieces[2], objectFileName.Extension.ToLower()));
                    } else
                    {
                        Debug.LogError("Unable to add file because of incorrect name " + objectFileName.FullName);
                    }
                }
            }

            if (sceneObjectNamesSet.Count == 0)
            {
                Debug.LogError("0 objects found in the story at path: " + storyPathsLookup["Objects"]);
                return;
            }

            StringBuilder objectNameFileBuilder = new StringBuilder();

            objectNameFileBuilder.Append(string.Format("# Scene Objects Count: {0}\n\n", sceneObjectNamesSet.Count));

            string pageMetaDataPath = System.IO.Path.Combine(Directory.GetParent(m_strBookPath).FullName, "PageMetaData.txt");

            bool confirmOverwrite = true;

            if (File.Exists(pageMetaDataPath)) 
            {
                confirmOverwrite = EditorUtility.DisplayDialog(
                    "Confirm Overwrite", "Are you sure you want to overwrite PageMetaData.txt?", "Yes", 
                    "Don't Overwrite");
            }

            if (!confirmOverwrite)
                return;

            bool generateSyntaxHelp = EditorUtility.DisplayDialog(
                    "Help Generation", "Generate help & example?",
                    "Yes",
                    "No");

            if (generateSyntaxHelp)
            {
                objectNameFileBuilder.Append("# !!! Page index starts at 1 and the end page is inclusive !!!");
                objectNameFileBuilder.Append("\n# !!! Key characters (>, &, x, :) can repeat with spaces in between but no other characters !!!");
                objectNameFileBuilder.Append("\n# Syntax Example:");
                objectNameFileBuilder.Append("\n#   ObjectName-animation_name > 1:10 & 12x3 & 17 & 20:25x3");
                objectNameFileBuilder.Append("\n# Description:");
                objectNameFileBuilder.Append("\n#   >       => Separates object name from page instructions");
                objectNameFileBuilder.Append("\n#   &       => Connects the instructions");
                objectNameFileBuilder.Append("\n#   1:10    => Put object on pages 1 through 10 (inclusive)");
                objectNameFileBuilder.Append("\n#   12x3    => Put object on page 12, 3 times");
                objectNameFileBuilder.Append("\n#   17      => Put object on page 17");
                objectNameFileBuilder.Append("\n#   20:25x3 => Put object on pages 20 through 25, 3 times on each\n\n");
            }

            foreach (KeyValuePair<string, List<string>> sceneObject in sceneObjectNamesSet) 
            {
                objectNameFileBuilder.Append(string.Format("{0} >\n", sceneObject.Key));
                if (sceneObject.Key.Contains("-"))
                {
                    string[] objectNameAndAnimation = sceneObject.Key.Split('-');
                    // Get local path, replace everything before Assets with Assets
                    string objectDirectory = ReplaceRegexAndTrim(storyObjectPathsLookup[objectNameAndAnimation[0]], @"(.*?)Assets", "Assets");
                    List<string> frameFullPaths = new List<string>();
                    foreach (string frame in sceneObject.Value)
                    {
                        string[] frameSplit = frame.Split('-');
                        string frameNumber = frameSplit[0];
                        string frameExtension = frameSplit[1];
                        string framePath = System.IO.Path.Combine(objectDirectory, 
                                string.Format("{0}-{1}{2}", sceneObject.Key, frameNumber, frameExtension));
                        frameFullPaths.Add(framePath);
                    }
                    CreateNewAnimation(sceneObject.Key, objectDirectory, frameFullPaths.ToArray());
                }
            }

            File.WriteAllText(pageMetaDataPath, objectNameFileBuilder.ToString());

            this.ShowNotification(new GUIContent("Page Meta Data & Animations Generated!"));

            Debug.Log("Page Meta Data Saved. Path: " + pageMetaDataPath);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        }

        // Note: Uncommenting these lines displays a button that removes all objects on all pages when clicked
        // if (GUILayout.Button("Remove All Objects From Pages", GUILayout.Height(24)))
        // {
        //     foreach (PageClass page in m_rcStoryBook.pages)
        //     {
        //         page.gameObjects = new GameObjectClass[0];
        //     }
        //     this.ShowNotification(new GUIContent("Removed all GameObjects on all Pages!"));
        // }

        if (GUILayout.Button("Import Objects on Pages", GUILayout.Height(24)))
        {
            bool confirmedToProceed = EditorUtility.DisplayDialog(
                "Confirm Import", "This action adds GameObjects on defined pages in the metadata file regardless if they are already added. Proceed?", 
                "Yes",
                "No");
            if (!confirmedToProceed) {
                Debug.Log("Stopped Page Objects Import Operation!");
                return;
            }

            DirectoryInfo commonDirectoryInfo = new DirectoryInfo(m_strCommonPath);

            Dictionary<string, string> storyPathsLookup = CreateSubDirectoryPathsLookup(commonDirectoryInfo);

            if (storyPathsLookup.Count == 0 || !storyPathsLookup.ContainsKey("Objects"))
            {
                Debug.LogError("Story path: " + commonDirectoryInfo.FullName + " doesn't contain Common directory.");
                return;
            }

            DirectoryInfo storyObjectsDirectoryInfo = new DirectoryInfo(storyPathsLookup["Objects"]);

            string pageMetaDataPath = System.IO.Path.Combine(Directory.GetParent(m_strBookPath).FullName, "PageMetaData.txt");

            if (!File.Exists(pageMetaDataPath))
            {
                Debug.LogError("PageMetaData.txt file doesn't exist at path: " + pageMetaDataPath);
                return;
            }

            FileInfo metaDataFileInfo = new FileInfo(pageMetaDataPath);

            string[] metaDataLines = File.ReadAllLines(pageMetaDataPath);

            if (metaDataLines.Length == 0)
            {
                Debug.LogError("PageMetaData.txt file is empty. Stopping...");
                return;
            }

            Dictionary<string, List<PageDirective>> parsedValues = new Dictionary<string, List<PageDirective>>();

            for (int lineIndex = 0; lineIndex < metaDataLines.Length; lineIndex++) 
            {
                int oneBasedLineIndex = lineIndex + 1;
                string trimmedLine = metaDataLines[lineIndex].Trim();


                // Checking if the line is a comment, then skip it
                if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                    continue;

                Debug.Log("---------- Line: " + oneBasedLineIndex + " | " + trimmedLine + " ----------");
                
                // Replace "    " with " "
                trimmedLine = ReplaceRegexAndTrim(trimmedLine, @"\s+", " ");
                // Replace ">> > >> >>" with ">"
                trimmedLine = ReplaceRegexAndTrim(trimmedLine, @"([>\s]>[>\s]*)", ">");

                string[] objectNameAndDirectives = trimmedLine.Split('>');

                if (objectNameAndDirectives.Length != 2)
                {
                    Debug.LogError(
                        string.Format("Meta data syntax error: split on '>' doesn't yield 2 values. Line: {0} | {1}", 
                        oneBasedLineIndex, trimmedLine));
                    continue;
                }

                string objectName = objectNameAndDirectives[0].Trim();
                string directives = objectNameAndDirectives[1].Trim();

                if (string.IsNullOrEmpty(objectName)) 
                {
                    Debug.LogError(string.Format("Object name is empty. Line: {0} | {1}",
                        oneBasedLineIndex, trimmedLine));
                    continue;
                }

                if (string.IsNullOrEmpty(directives))
                {
                    Debug.LogError(string.Format("Object directives is empty. Line: {0} | {1}", 
                        oneBasedLineIndex, trimmedLine));
                    continue;
                }

                // Replace "&&& & & &&" with "&"
                directives = ReplaceRegexAndTrim(directives, @"([&\s]&[&\s]*)", "&");

                string[] directiveTerms = directives.Split('&');

                for (int termIndex = 0; termIndex < directiveTerms.Length; termIndex++)
                {
                    string trimmedTerm = ReplaceRegexAndTrim(directiveTerms[termIndex], @"\s+", " ").ToLower();

                    if (string.IsNullOrEmpty(trimmedTerm))
                    {
                        Debug.LogError(string.Format("Term {0} is empty. Line: {1} | {2}",
                            termIndex + 1, oneBasedLineIndex, trimmedLine));
                        continue;
                    }

                    // Replace "xxXx x" with "x"
                    trimmedTerm = ReplaceRegexAndTrim(trimmedTerm, @"([x\s]x[x\s]*)", "x");

                    string pageInstruction = null;
                    string termRepeatCount = null;

                    // Multiplication directive is present
                    if (trimmedTerm.Contains("x"))
                    {
                        string[] multiplicands = trimmedTerm.Split('x');
                        if (multiplicands.Length != 2)
                        {
                            Debug.LogError(
                                string.Format("Term {0} multiplication should have 2 sides. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        if (string.IsNullOrEmpty(multiplicands[0]))
                        {
                            Debug.LogError(
                                string.Format("Term {0} page instructions is empty. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        if (string.IsNullOrEmpty(multiplicands[1]))
                        {
                            Debug.LogError(
                                string.Format("Term {0} right side of multiplication is empty. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        pageInstruction = multiplicands[0].Trim();
                        termRepeatCount = multiplicands[1].Trim();
                    }
                    else
                    {
                        pageInstruction = trimmedTerm;
                    }
                    
                    // Replace ":::: :: :::" with ":"
                    pageInstruction = ReplaceRegexAndTrim(pageInstruction, @"([:\s]:[:\s]*)", ":"); 

                    string termPage = null;
                    string termEndPage = null;

                    if (pageInstruction.Contains(":"))
                    {
                        string[] pageRange = pageInstruction.Split(':');
                        if (pageRange.Length != 2)
                        {
                            Debug.LogError(string.Format("Term {0} page range should have 2 pages. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        if (string.IsNullOrEmpty(pageRange[0]))
                        {
                            Debug.LogError(
                                string.Format("Term {0} start page is empty. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        if (string.IsNullOrEmpty(pageRange[1]))
                        {
                            Debug.LogError(
                                string.Format("Term {0} end page is empty. Line: {1} | {2}",
                                    termIndex + 1, oneBasedLineIndex, trimmedLine));
                            continue;
                        }

                        termPage = pageRange[0].Trim();
                        termEndPage = pageRange[1].Trim();
                    } 
                    else
                    {
                        termPage = pageInstruction;
                    }

                    // We have so far, trying to parse
                    Debug.Log("Page: " + termPage + " EndPage: " + termEndPage + " By: " + termRepeatCount);
                    
                    PageDirective directive = new PageDirective();

                    int termPageNumber = -1;
                    int termEndPageNumber = -1;
                    int termRepeatCountNumber = -1;

                    bool termPageParseSuccess;
                    try {
                        termPageNumber = Int32.Parse(termPage);
                        termPageParseSuccess = true;
                        directive.StartPage = termPageNumber;
                    } catch (FormatException) {
                        Debug.LogWarning(string.Format("Term {0} page number is not an integer. Line {1} | {2}", 
                            termIndex + 1, oneBasedLineIndex, trimmedLine));
                        termPageParseSuccess = false;
                    }

                    if (!termPageParseSuccess)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(termEndPage))
                    {
                        directive.EndPage = -1;
                    }
                    else
                    {
                        bool termEndPageParseSuccess;
                        try {
                            termEndPageNumber = Int32.Parse(termEndPage);
                            termEndPageParseSuccess = true;
                            directive.EndPage = termEndPageNumber;
                        } catch (FormatException) {
                            Debug.LogWarning(string.Format("Term {0} end page number is not an integer. Line {1} | {2}",
                                termIndex + 1, oneBasedLineIndex, trimmedLine));
                            termEndPageParseSuccess = false;
                        }

                        if (!termEndPageParseSuccess)
                        {
                            continue;
                        }
                    }


                    if (string.IsNullOrEmpty(termRepeatCount))
                    {
                        directive.RepeatCount = -1;
                    }
                    else
                    {
                        bool termRepeatCounterParseSuccess;
                        try {
                            termRepeatCountNumber = Int32.Parse(termRepeatCount);
                            termRepeatCounterParseSuccess = true;
                            directive.RepeatCount = termRepeatCountNumber;
                        } catch (FormatException) {
                            Debug.LogWarning(string.Format("Term {0} end page number is not an integer. Line {1} | {2}",
                                termIndex + 1, oneBasedLineIndex, trimmedLine));
                            termRepeatCounterParseSuccess = false;
                        }

                        if (!termRepeatCounterParseSuccess)
                        {
                            continue;
                        }
                    }

                    // Initialize parsed values page directives for this object
                    if (!parsedValues.ContainsKey(objectName))
                    {
                        parsedValues.Add(objectName, new List<PageDirective>());
                    }
                    else if (parsedValues.ContainsKey(objectName) && parsedValues[objectName] == null) 
                    {
                        parsedValues[objectName] = new List<PageDirective>();
                    }

                    parsedValues[objectName].Add(directive);

                }
            }

            if (parsedValues.Count == 0)
            {
                Debug.LogError("Couldn't find any page directives in PageMetaData.txt");
                return;
            }

            // Start iterating on the directives generating objects on the given pages

            Debug.Log(string.Format("{0} lines parsed successfully! Adding Objects...", parsedValues.Count));

            foreach (KeyValuePair<string, List<PageDirective>> parsedLine in parsedValues)
            {
                string objectAndAnimationName = parsedLine.Key;
                for (int directiveIndex = 0; directiveIndex < parsedLine.Value.Count; directiveIndex++) 
                {
                    int startPage         = parsedLine.Value[directiveIndex].StartPage;
                    int endPage           = parsedLine.Value[directiveIndex].EndPage;
                    int repeatCount       = parsedLine.Value[directiveIndex].RepeatCount;

                    if (startPage < 1 || startPage > m_rcStoryBook.pages.Length)
                    {
                        Debug.LogError($"Start page should be between [1, {m_rcStoryBook.pages.Length}].");
                        continue;
                    }

                    if (endPage < startPage || endPage > m_rcStoryBook.pages.Length)
                        endPage = startPage;

                    for (int oneBasedPageIndex = startPage; oneBasedPageIndex <= endPage; oneBasedPageIndex++)
                    {
                        int pageIndex = oneBasedPageIndex - 1;

                        if (repeatCount < 1)
                            repeatCount = 1;

                        for (int repeat = 0; repeat < repeatCount; repeat++)
                        {
                            GameObjectClass newSceneObject = AddNewGameObjectOnPage(pageIndex);
                            newSceneObject.imageName = objectAndAnimationName;
                            
                            // Add the object animation asset name if it exists where it should already be
                            if (objectAndAnimationName.Contains("-"))
                            {
                                string objectName = objectAndAnimationName.Split('-')[0];
                                newSceneObject.imageName = objectAndAnimationName + "-1";
                                string fullAnimationAssetPath = System.IO.Path.Combine(storyPathsLookup["Objects"], 
                                    objectName);
                                fullAnimationAssetPath = System.IO.Path.Combine(fullAnimationAssetPath, 
                                    objectAndAnimationName + ".asset");
                                if (File.Exists(fullAnimationAssetPath))
                                {
                                    newSceneObject.Animations = new string[1] { objectAndAnimationName };
                                }
                            }
                        }
                    }
                }
            }
            
            // Note: Here we would set the m_rcImageNames tp a new list of images but we no longer need that since we have object field for images

            Debug.Log("Populating objects using the metadata file is finished...");
            this.ShowNotification(new GUIContent("Finished Adding Objects on Pages."));

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        GUILayout.EndHorizontal();
        
        #endregion
        
        #region First Buttons Row at the Bottom (Add Text, Timestamps, Add GameObject, Add Trigger)

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Page Text", GUILayout.Height(24)))
        {
            PageClass currentPage = m_rcStoryBook.pages[m_activePageID];
            TextClass[] rcNewArray = new TextClass[currentPage.texts.Length + 1];
            Array.Copy(currentPage.texts, rcNewArray, currentPage.texts.Length);
            rcNewArray[currentPage.texts.Length] = new TextClass();
            rcNewArray[currentPage.texts.Length].id = currentPage.texts.Length;
            rcNewArray[currentPage.texts.Length].text = "";

            currentPage.texts = rcNewArray;

            this.ShowNotification(new GUIContent("New Text Object Added to Page " + m_activePageID));
        }

        if (GUILayout.Button("Generate Timestamps", GUILayout.Height(24)))
        {
            PageClass currentPage = m_rcStoryBook.pages[m_activePageID];
            if (!string.IsNullOrEmpty(currentPage.audioFile))
            {
                List<string> rcWords = GetTextWords(currentPage.texts);

                TimeStampClass[] rcNewArray = new TimeStampClass[rcWords.Count];

                for (int i = 0; i < rcWords.Count; i++)
                {
                    rcNewArray[i] = new TimeStampClass();
                    rcNewArray[i].start = i * Convert.ToInt32(m_rcPageAudio[m_activePageID].length * 1000) / rcWords.Count;
                    rcNewArray[i].end = rcNewArray[i].start + Convert.ToInt32(m_rcPageAudio[m_activePageID].length * 1000) / rcWords.Count;
                    rcNewArray[i].audio = "";
                    rcNewArray[i].starWord = "No";
                    rcNewArray[i].wordIdx = i;
                }

                currentPage.timestamps = rcNewArray;

                this.ShowNotification(new GUIContent("Timestamps Generated for Page " + m_activePageID));
            }
        }

        if (GUILayout.Button("Add Page GameObject", GUILayout.Height(24)))
        {
            PageClass currentPage = m_rcStoryBook.pages[m_activePageID];
            GameObjectClass[] rcNewArray = new GameObjectClass[currentPage.gameObjects.Length + 1];
            Array.Copy(currentPage.gameObjects, rcNewArray, currentPage.gameObjects.Length);
            rcNewArray[currentPage.gameObjects.Length] = new GameObjectClass();
            rcNewArray[currentPage.gameObjects.Length].anim = new Anim[0];
            rcNewArray[currentPage.gameObjects.Length].Animations = new string[0];
            rcNewArray[currentPage.gameObjects.Length].AnimationsID = new int[0];
            rcNewArray[currentPage.gameObjects.Length].draggable = false;
            rcNewArray[currentPage.gameObjects.Length].id = currentPage.gameObjects.Length;
            currentPage.gameObjects = rcNewArray;

            this.ShowNotification(new GUIContent("New Scene Game Object Added to Page " + m_activePageID));
        }

        if (GUILayout.Button("Add Page Trigger", GUILayout.Height(24)))
        {
            PageClass currentPage = m_rcStoryBook.pages[m_activePageID];
            if (currentPage.gameObjects == null || currentPage.gameObjects.Length == 0) {
                this.ShowNotification(new GUIContent("Error: Current page doesn't contain any Game Objects! Please add one."));
                return;
            }
            TriggerClass[] rcNewArray = new TriggerClass[currentPage.triggers.Length + 1];
            Array.Copy(currentPage.triggers, rcNewArray, currentPage.triggers.Length);
            TriggerClass newTrigger = new TriggerClass();
            newTrigger.invokers = new PerformanceInvoker[0];
            newTrigger.prompts = new PromptType[1] { PromptType.Click };
            newTrigger.type = TriggerType.Animation;
            rcNewArray[currentPage.triggers.Length] = newTrigger;
            currentPage.triggers = rcNewArray;

            this.ShowNotification(new GUIContent("New Scene Trigger Added to Page " + m_activePageID));
        }

        GUILayout.EndHorizontal();

        #endregion

        #region Second Buttons Row At the Bottom (Add Page, Previous, Next, Delete, Add Images, Save JSON)

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Page", GUILayout.Height(24)))
        {
            PageClass[] rcNewArray = new PageClass[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcStoryBook.pages, rcNewArray, m_rcStoryBook.pages.Length);
            int newIndex = m_rcStoryBook.pages.Length;
            rcNewArray[newIndex] = new PageClass();
            rcNewArray[newIndex].gameObjects = new GameObjectClass[0];
            rcNewArray[newIndex].triggers = new TriggerClass[0];
            rcNewArray[newIndex].timestamps = new TimeStampClass[0];
            rcNewArray[newIndex].texts = new TextClass[0];
            rcNewArray[newIndex].audio = new AudioClass();
            rcNewArray[newIndex].pageNumber = m_rcStoryBook.pages.Length;
            rcNewArray[newIndex].script = "GSManager";

            Rect[] NewAudioRects = new Rect[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcAudioRects, NewAudioRects, newIndex);

            AudioClip[] NewAudioClips = new AudioClip[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcPageAudio, NewAudioClips, newIndex);

            m_rcPageAudio = NewAudioClips;
            m_rcAudioRects = NewAudioRects;
            m_rcStoryBook.pages = rcNewArray;

            this.ShowNotification(new GUIContent("New Page Added at Index " + newIndex));
        }

        EditorStyles.popup.fontSize = 12;
        EditorStyles.popup.fontStyle = FontStyle.Bold;

        EditorGUI.BeginDisabledGroup(m_activePageID == 0);
        string previousButtonLabel = m_activePageID == 0 ? 
            "Previous Page" : 
            string.Format("Previous Page({0})", Mathf.Clamp(m_activePageID - 1, 0, m_rcStoryBook.pages.Length - 2));
        if (GUILayout.Button(previousButtonLabel, GUILayout.Height(24))) {
            if (m_activePageID > 0)
            {
                m_activePageID--;
                initializeNewSetOfMissingAssets();
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(m_activePageID == m_rcStoryBook.pages.Length - 1);
        string nextButtonLabel = m_activePageID == m_rcStoryBook.pages.Length - 1 ? 
            "Next Page" : 
            string.Format("Next Page({0})", Mathf.Clamp(m_activePageID + 1, 1, m_rcStoryBook.pages.Length - 1));
        if (GUILayout.Button(nextButtonLabel, GUILayout.Height(24))) {
            if (m_activePageID < m_rcStoryBook.pages.Length -1)
            {
                m_activePageID++;
                initializeNewSetOfMissingAssets();
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(m_rcStoryBook.pages.Length == 1);
        if (GUILayout.Button("Delete Page", GUILayout.Height(24))) {
            bool confirmedDeletion = EditorUtility.DisplayDialog(
                "Confirm Deletion", "Are you sure you want to delete page(" + m_activePageID + ")?", 
                "Trash it", 
                "Thanks for saving me");
            if (confirmedDeletion) {
                int tempIndex = m_activePageID;
                m_activePageID = 0;
                m_rcStoryBook.pages = m_rcStoryBook.pages.RemoveAt(tempIndex);
                this.ShowNotification(new GUIContent("Page " + tempIndex + " Deleted!"));
            }
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Add Images To AssetBundle", GUILayout.Height(24)))
        {
#if UNITY_EDITOR_WIN
            Debug.Log("m_strAssetPath: " + m_strAssetPath);
            Debug.Log("m_strImagePath: " + m_strImagePath);
            Debug.Log(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            Debug.Log(m_strImagePath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strImagePath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
#endif
#if UNITY_EDITOR_OSX            
            Debug.Log(m_strAnimPath.Replace(m_strAssetPath, "").Replace("/Assets/", "Assets/"));
            AddImagesInPath(m_strAnimPath.Replace(m_strAssetPath, "").Replace("/Assets/", "Assets"));
            Debug.Log(m_strImagePath.Replace(m_strAssetPath, "").Replace("/Assets/", "Assets/"));
            AddImagesInPath(m_strImagePath.Replace(m_strAssetPath, "").Replace("/Assets/", "Assets/"));
#endif
        }

        // if (GUILayout.Button("Create Animations", GUILayout.Height(24)))
        // {
        //     Debug.Log(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
        //     ConstructAnimationsInPath(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
        // }

        if (GUILayout.Button("Save JSON", GUILayout.Height(24)))
        {
            Debug.Log(m_strBookPath + " file to be saved.");

            StreamWriter rcWriter = new StreamWriter(m_strBookPath, false);
            rcWriter.Write(JsonUtility.ToJson(m_rcStoryBook, true));
            rcWriter.Close();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            this.ShowNotification(new GUIContent("Changes Saved!"));

            m_savedRecentlyFromThisEditor = true;
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(4);

#endregion
    
    }

    /// <summary>
    /// Adds a new game object on page
    /// </summary>
    /// <param name="i_pageIndex">Page index to add a new game object to</param>
    /// <returns>Returns the newly added game object</returns>
    private GameObjectClass AddNewGameObjectOnPage(int i_pageIndex)
    {
        PageClass storyPage = m_rcStoryBook.pages[i_pageIndex];
        GameObjectClass[] rcNewArray = new GameObjectClass[storyPage.gameObjects.Length + 1];
        Array.Copy(storyPage.gameObjects, rcNewArray, storyPage.gameObjects.Length);
        GameObjectClass newObject = new GameObjectClass();
        newObject.anim = new Anim[0];
        newObject.Animations = new string[0];
        newObject.AnimationsID = new int[0];
        newObject.draggable = false;
        newObject.id = storyPage.gameObjects.Length;
        rcNewArray[storyPage.gameObjects.Length] = newObject;
        storyPage.gameObjects = rcNewArray;
        return newObject;
    }

    /// <summary>
    /// Creates a path lookup dictionary for a given directory
    /// </summary>
    /// <param name="pathInfo">DirectoryInfo object for the target path</param>
    /// <returns>Dictionary containing sub directory names as keys and full paths as values</returns>
    private Dictionary<string, string> CreateSubDirectoryPathsLookup(DirectoryInfo pathInfo) 
    {
        Dictionary<string, string> directoryPathsLookup = new Dictionary<string, string>();

        foreach (DirectoryInfo storySubDirectory in pathInfo.GetDirectories())
        {
            directoryPathsLookup.Add(storySubDirectory.Name, storySubDirectory.FullName);
        }

        return directoryPathsLookup;
    }

    public string[] GetSelectedObjectAnimationsForTrigger(int i_nTriggerIndex) 
    {
        PageClass storyPage = m_rcStoryBook.pages[m_activePageID];
        if (i_nTriggerIndex < 0 || i_nTriggerIndex > storyPage.triggers.Length - 1) 
        {
            return null;
        } else
        {
            return storyPage.gameObjects[storyPage.triggers[i_nTriggerIndex].sceneObjectId].Animations;
        }
    }

    /// <summary>
    /// Uses regex replace to remove consecutive whitespaces from the input string and trim it
    /// </summary>
    /// <param name="i_input">Input string</param>
    /// <param name="i_regex">Regex to search for</param>
    /// <param name="i_replaceWith">String to replace the matches with</param>
    /// <returns>Replaced and trimmed string</returns>
    private string ReplaceRegexAndTrim(string i_input, string i_regex, string i_replaceWith) 
    {
        return Regex.Replace(i_input, i_regex, i_replaceWith, RegexOptions.IgnoreCase).Trim();
    }

    private void addFileWatcherForReloadingAtPath(string path) 
    {
        if (m_bookFileLoadWatcher == null)
            m_bookFileLoadWatcher = new FileSystemWatcher();
        
        m_bookFileLoadWatcher.Path = System.IO.Path.GetDirectoryName(path);
        m_bookFileLoadWatcher.Filter = System.IO.Path.GetFileName(path);

        m_bookFileLoadWatcher.Changed += onLoadedBookFileChangedExternally;

        m_bookFileLoadWatcher.EnableRaisingEvents = true;

        Debug.LogFormat("Watching file at path: " + path);
    }

    private void onLoadedBookFileChangedExternally(object source, FileSystemEventArgs e)
    {
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                if (!m_savedRecentlyFromThisEditor)
                {
                    Debug.LogWarning($"File: {System.IO.Path.GetFileName(e.FullPath)} content changed. Reloading!!!");
                    this.m_needToLoadBookContent = true;
                } else 
                {
                    m_savedRecentlyFromThisEditor = false;
                }
                break;
        }
    }
    
    private string[] GetAnimationNames()
    {
        List<string> racAnimNames = new List<string>();

        string [] strAnimations = AssetDatabase.FindAssets("t:SpriteAnimation");

        if ( strAnimations != null )
        {
            foreach( string strAnim in strAnimations)
            {
                string strPath = AssetDatabase.GUIDToAssetPath(strAnim);

                if ( !string.IsNullOrEmpty(strPath))
                {
                    SpriteAnimation rcAnim = AssetDatabase.LoadAssetAtPath<SpriteAnimation>(strPath);

                    if ( rcAnim != null )
                    {
                        racAnimNames.Add(rcAnim.name);
                    }
                }
            }
        }

        return racAnimNames.ToArray();
    }

    private void SetBoldFoldoutStyleToBold() {
        m_boldFoldoutStyle.fontStyle = FontStyle.Bold;
    }

    private void SetBoldFoldoutStyleToNormal() {
        m_boldFoldoutStyle.fontStyle = FontStyle.Normal;
    }

    private void EditPage(PageClass i_rcPage, int i_nOrdinal)
    {

#region Page Data

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutPageData = EditorGUILayout.Foldout(m_foldoutPageData, "Page Parameters", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        if (m_foldoutPageData)
        {
            EditorGUI.indentLevel++;

            i_rcPage.pageNumber = EditorGUILayout.IntField("Page Number", i_rcPage.pageNumber, EditorStyles.numberField);
            i_rcPage.script = EditorGUILayout.TextField("Script", i_rcPage.script, EditorStyles.textField);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

#region Page Text
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutPageText = EditorGUILayout.Foldout(m_foldoutPageText, "Page Text", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        int nRemove = -1;

        if (m_foldoutPageText) 
        {
            
            EditorGUI.indentLevel++;

            for (int j = 0; j < i_rcPage.texts.Length; j++)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                EditText(i_rcPage.texts[j], j);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = j;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcPage.texts = i_rcPage.texts.RemoveAt(nRemove);
                this.ShowNotification(new GUIContent("Page(" + m_activePageID + ") Text(" + nRemove + ") Has Been Removed!"));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

#region Page Audio
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutPageAudio = EditorGUILayout.Foldout(m_foldoutPageAudio, "Page Audio", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        if (m_foldoutPageAudio)
        {
            EditorGUI.indentLevel++;

            string strSearchPath = m_strBookPath.Replace("Resources/" + System.IO.Path.GetFileName(m_strBookPath), "");
            strSearchPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(strSearchPath).FullName).FullName).FullName;

            i_rcPage.audioFile = ObjectFieldToString<AudioClip>(ref i_rcPage.audioFile,"Audio File",ref i_rcPage.AudioClip, "", strSearchPath);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(EditorGUI.indentLevel * 10);

            m_bMicrophoneHot = GUILayout.Button("Record");

            if (GUILayout.Button("Play"))
            {
                AudioUtility.PlayClip(m_rcPageAudio[i_nOrdinal], 0);
            }

            EditorGUILayout.EndHorizontal();

            if (m_bMicrophoneHot)
            {
                if (Microphone.IsRecording(Microphone.devices[0]))
                {
                    float fDuration = Time.realtimeSinceStartup - m_fRecordingStart;
                    Debug.Log("Recording Ended: Duration " + fDuration);
                    Microphone.End(Microphone.devices[0]);

                    m_rcPageAudio[i_nOrdinal] = RARE.TrimEndOfAudioClip(m_rcRecordingClip, (int)(fDuration * 44100));

                    string strFilename = "audio_stanza_page_" + i_nOrdinal;
                    string strFilePath = m_strBookPath.Replace("Resources/" + System.IO.Path.GetFileName(m_strBookPath), "Audio/Stanza/" + strFilename);
                    RARE.ExportClip(strFilePath, m_rcPageAudio[i_nOrdinal]);
                    i_rcPage.audioFile = strFilename + ".wav";
                }
                else
                {
                    m_fRecordingStart = Time.realtimeSinceStartup;
                    Debug.Log("Recording Started at " + m_fRecordingStart);
                    m_rcRecordingClip = Microphone.Start(Microphone.devices[0], false, 300, 44100);
                }
            }

            m_rcPageAudio[i_nOrdinal] = i_rcPage.AudioClip;

            if (m_rcPageAudio[i_nOrdinal] != null)
            {
                m_rcAudioRects[i_nOrdinal] = EditorGUILayout.GetControlRect(false, 100.0f);
                m_rcAudioRects[i_nOrdinal].xMin = 20.0f;

                ClearRect(m_rcAudioRects[i_nOrdinal], Color.black);
                RenderAudioClip(m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], 1.0f);

                if (AudioUtility.IsClipPlaying(m_rcPageAudio[i_nOrdinal]))
                {
                    EditorUtility.SetDirty(this);
                    this.Repaint();

                    int nSamplePosition = AudioUtility.GetClipSamplePosition(m_rcPageAudio[i_nOrdinal]);
                    DrawCurrentAudioPosition(m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], nSamplePosition);
                }

                Rect rcRect2 = EditorGUILayout.GetControlRect();
                rcRect2.xMin = 20.0f;

                GUILayout.BeginHorizontal();
                GUILayout.Space((EditorGUI.indentLevel + 1) * 10);

                GUILayout.EndHorizontal();
            }


            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

#region Audio Timestamps
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutTimestamps = EditorGUILayout.Foldout(m_foldoutTimestamps, "Page Timestamps", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        if (m_foldoutTimestamps)
        {
            EditorGUI.indentLevel++;

            int nDelete = -1;

            for (int j = 0; j < i_rcPage.timestamps.Length; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                TimeStampClass rcTimestamp = i_rcPage.timestamps[j];
                AudioClip rcClip = m_rcPageAudio[i_nOrdinal];

                EditTimestamps(rcTimestamp, rcClip, m_rcAudioRects[i_nOrdinal], i_rcPage.texts, j);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (rcTimestamp.IsPlaying)
                {
                    if (AudioUtility.IsClipPlaying(rcClip))
                    {
                        int nSamplePosition = AudioUtility.GetClipSamplePosition(rcClip);

                        if (((float)nSamplePosition / (float)rcClip.samples) > ((rcTimestamp.end/1000.0f) / rcClip.length))
                        {
                            AudioUtility.StopAllClips();
                            rcTimestamp.IsPlaying = false;
                        }
                    }
                    else
                    {
                        rcTimestamp.IsPlaying = false;
                    }
                }

                GUILayout.BeginHorizontal();

                if ( GUILayout.Button("Play"))
                {
                    int nSampleStart = (int)((rcClip.samples / rcClip.length)*(rcTimestamp.start/1000f));

                    AudioUtility.PlayClip(rcClip);
                    AudioUtility.SetClipSamplePosition(rcClip, nSampleStart);
                    rcTimestamp.IsPlaying = true;
                }

                if (GUILayout.Button("Remove"))
                {
                    nDelete = j;
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nDelete != -1)
            {
                i_rcPage.timestamps = i_rcPage.timestamps.RemoveAt(nDelete);
                this.ShowNotification(new GUIContent("Page(" + m_activePageID + ") Timestamp(" + nDelete + ") Has Been Removed!"));
            }

            if (m_rcPageAudio[i_nOrdinal] != null)
            {
                DrawTextOnAudioClip(m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], i_rcPage.timestamps, i_rcPage.texts);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

#region Page Objects
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutGameObjects = EditorGUILayout.Foldout(m_foldoutGameObjects, "Page Objects", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        if (m_foldoutGameObjects)
        {
            EditorGUI.indentLevel++;

            nRemove = -1;

            for (int j = 0; j < i_rcPage.gameObjects.Length; j++)
            {
                // for statement here

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                EditGameObject(i_rcPage.gameObjects[j], j);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = j;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcPage.gameObjects = i_rcPage.gameObjects.RemoveAt(nRemove);
                this.ShowNotification(new GUIContent("Page(" + m_activePageID + ") Object(" + nRemove + ") Has Been Removed!"));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

#region Page Triggers
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SetBoldFoldoutStyleToBold();

        m_foldoutTriggers = EditorGUILayout.Foldout(m_foldoutTriggers, "Page Triggers", m_boldFoldoutStyle);

        SetBoldFoldoutStyleToNormal();

        if (m_foldoutTriggers)
        {
            EditorGUI.indentLevel++;

            nRemove = -1;

            for (int j = 0; j < i_rcPage.triggers.Length; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                EditTriggers(i_rcPage.triggers[j], j, i_nOrdinal);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = j;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcPage.triggers = i_rcPage.triggers.RemoveAt(nRemove);
                this.ShowNotification(new GUIContent("Page(" + m_activePageID + ") Trigger(" + nRemove + ") Has Been Removed!"));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        
#endregion

    }

    private List<string> GetTextWords(TextClass[] i_racTexts)
    {
        List<string> rcWords = new List<string>();

        if (i_racTexts != null)
        {
            for (int i = 0; i < i_racTexts.Length; i++)
            {
                if (!string.IsNullOrEmpty(i_racTexts[i].text))
                {
                    string[] words = i_racTexts[i].text.Split(new char[] { ' ' });

                    for (int j = 0; j < words.Length; j++)
                    {
                        rcWords.Add(words[j]);
                    }
                }
            }
        }

        return rcWords;
    }

    private void DrawTextOnAudioClip(AudioClip rcClip, Rect rect, TimeStampClass[] timestamps, TextClass[] texts)
    {
        List<string> rcWords = GetTextWords(texts);

        if (rcWords != null)
        {
            if (rcWords.Count != timestamps.Length)
            {
                Debug.Log("Error! Words don't match number of timestamps!");
            }
            else
            {
                for (int i = 0; i < rcWords.Count; i++)
                {
                    DrawTimestamp(rcClip, rect, timestamps[i].start, timestamps[i].end);
                    DrawText(rcClip, rect, timestamps[i].start, timestamps[i].end, rcWords[i]);
                }
            }
        }
    }

    private void EditTimestamps(TimeStampClass i_rcTimestamp, AudioClip i_rcClip, Rect i_rcRect, TextClass[] i_rcTexts, int i_nOrdinal)
    {
        if ( (i_rcTexts != null) && !string.IsNullOrEmpty(i_rcTexts[0].text ))
        {
            List<string> rcWords = GetTextWords(i_rcTexts);

            if (rcWords != null)
            {
                string[] rastrWords = rcWords.ToArray();

                if (rastrWords.Length > i_nOrdinal)
                {
                    i_rcTimestamp.Show = EditorGUILayout.Foldout(i_rcTimestamp.Show, "Timestamp " + i_nOrdinal + " - " + rastrWords[i_nOrdinal] + (i_rcTimestamp.starWord.ToLower() == "yes" ? " [ ★ Word ]" : ""));
                }
                else
                {
                    i_rcTimestamp.Show = EditorGUILayout.Foldout(i_rcTimestamp.Show, "Timestamp " + i_nOrdinal);
                }
            }
            else
            {
                i_rcTimestamp.Show = EditorGUILayout.Foldout(i_rcTimestamp.Show, "Timestamp " + i_nOrdinal);
            }
        }
        else
        {
            i_rcTimestamp.Show = EditorGUILayout.Foldout(i_rcTimestamp.Show, "Timestamp " + i_nOrdinal);
        }

        EditorGUI.indentLevel++;

        if (i_rcTimestamp.Show)
        {
            EditorGUI.BeginDisabledGroup(true);
            i_rcTimestamp.wordIdx = EditorGUILayout.IntField("Word Index", i_rcTimestamp.wordIdx, EditorStyles.numberField);
            EditorGUI.EndDisabledGroup();

            GUILayout.BeginHorizontal();
            i_rcTimestamp.start = EditorGUILayout.IntField("Start", i_rcTimestamp.start, EditorStyles.numberField);
            i_rcTimestamp.end = EditorGUILayout.IntField("End", i_rcTimestamp.end, EditorStyles.numberField);
            bool starWord = i_rcTimestamp.starWord.ToLower() == "yes" ? true : false;
            i_rcTimestamp.starWord = EditorGUILayout.Toggle("Star Word", starWord) ? "Yes" : "No";
            GUILayout.EndHorizontal();
//          i_rcTimestamp.audio = EditorGUILayout.TextField("Audio", i_rcTimestamp.audio, EditorStyles.textField);

            string strSearchPath = m_strBookPath.Replace("Resources/" + System.IO.Path.GetFileName(m_strBookPath), "");
            strSearchPath = Directory.GetParent(Directory.GetParent(strSearchPath).FullName).FullName;

            i_rcTimestamp.audio = ObjectFieldToString<AudioClip>(ref i_rcTimestamp.audio, "Audio File", ref i_rcTimestamp.AudioClip, "", strSearchPath);

            if (i_rcClip != null)
            {

                i_rcRect = EditorGUILayout.GetControlRect(false, 60.0f);
                i_rcRect.xMin = 40.0f;

                ClearRect(i_rcRect, Color.black);
                RenderAudioClip(i_rcClip, i_rcRect, 1.0f);

                Rect rcRect2 = EditorGUILayout.GetControlRect();
                rcRect2.yMax = 60f;
                rcRect2.xMin = 40.0f;

                if (AudioUtility.IsClipPlaying(i_rcClip))
                {
                    EditorUtility.SetDirty(this);
                    this.Repaint();

                    int nSamplePosition = AudioUtility.GetClipSamplePosition(i_rcClip);
                    DrawCurrentAudioPosition(i_rcClip, i_rcRect, nSamplePosition); 
                }

                if (i_rcClip != null)
                {
                    DrawTimestamp(i_rcClip, i_rcRect, i_rcTimestamp.start, i_rcTimestamp.end);
                    DrawTimestamp(i_rcClip, rcRect2, i_rcTimestamp.start, i_rcTimestamp.end);
                }

            }

        }

        EditorGUI.indentLevel--;
    }

    private void EditTriggers(TriggerClass i_rcTrigger, int i_nOrdinal, int i_pageOrdinal)
    {
        GameObjectClass triggerObject = m_rcStoryBook.pages[i_pageOrdinal].gameObjects[i_rcTrigger.sceneObjectId];
        
        string sceneObjectID = string.IsNullOrEmpty(triggerObject.label) ? triggerObject.imageName : triggerObject.label;
        
        i_rcTrigger.Show = EditorGUILayout.Foldout(i_rcTrigger.Show, 
            $"{i_nOrdinal} - {i_rcTrigger.type} on [{sceneObjectID}] with {i_rcTrigger.prompts.Length} prompts and {i_rcTrigger.invokers.Length} invokers.");

        EditorGUI.indentLevel++;

        TriggerType eLastTriggerType = i_rcTrigger.type;

        if (i_rcTrigger.Show)
        {
            i_rcTrigger.type = (TriggerType)EditorGUILayout.EnumPopup(i_rcTrigger.type, EditorStyles.popup);

            if (eLastTriggerType != i_rcTrigger.type)
            {
                i_rcTrigger.Params = "";
                i_rcTrigger.EditorFields = null;
                i_rcTrigger.PerformanceParams = null;
            }
            switch (i_rcTrigger.type)
            {
                case TriggerType.Navigation:
                    i_rcTrigger.Params = OnBookGUI<NavigationParams>(i_rcTrigger, i_nOrdinal);
                    break;
                case TriggerType.Animation:
                    i_rcTrigger.Params = OnBookGUI<SpriteAnimationParams>(i_rcTrigger, i_nOrdinal);
                    break;
                case TriggerType.Move:
                    i_rcTrigger.Params = OnBookGUI<MoveParams>(i_rcTrigger, i_nOrdinal);
                    break;
                case TriggerType.Highlight:
                    i_rcTrigger.Params = OnBookGUI<HighlightParams>(i_rcTrigger, i_nOrdinal);
                    break;
                case TriggerType.Rotate:
                    i_rcTrigger.Params = OnBookGUI<RotateParams>(i_rcTrigger, i_nOrdinal);
                    break;
                case TriggerType.Scale:
                    i_rcTrigger.Params = OnBookGUI<ScaleParams>(i_rcTrigger, i_nOrdinal);
                    break;
                default:
                    break;
            }
            EditPrompts(i_rcTrigger);
            EditorGUI.BeginDisabledGroup(true);
            i_rcTrigger.Params = EditorGUILayout.TextField(i_rcTrigger.Params);
            EditorGUI.EndDisabledGroup();
            AddObjectWordLinking(i_rcTrigger, i_nOrdinal, i_pageOrdinal);
        }

        EditorGUI.indentLevel--;
    }

    private void EditPrompts (TriggerClass i_rcTrigger)
    {
        int arraySize;
        if (i_rcTrigger.prompts != null)
        {
            arraySize = i_rcTrigger.prompts.Length;
        }
        else
        {
            i_rcTrigger.prompts = new PromptType[1]; //always want to have at least one prompt!
            arraySize = 1;
        }

        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        i_rcTrigger.showPrompts = EditorGUILayout.Foldout(i_rcTrigger.showPrompts, "Prompts(" + i_rcTrigger.prompts.Length + ")");
        if (GUILayout.Button("Add")) 
        {   
            i_rcTrigger.showPrompts = true;
            arraySize += 1;
        }
        EditorGUILayout.EndHorizontal();
        if (i_rcTrigger.showPrompts)
        {
            EditorGUI.indentLevel++;
            arraySize = EditorGUILayout.IntField(new GUIContent().text = "size", arraySize);
            if (i_rcTrigger.prompts.Length > 0)
            {
                PromptType[] temp = new PromptType[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    if(i >= i_rcTrigger.prompts.Length)
                    {
                        break;
                    }
                    temp[i] = i_rcTrigger.prompts[i];
                }
                i_rcTrigger.prompts = new PromptType[arraySize];
                for( int i = 0; i < arraySize; i++)
                {
                    i_rcTrigger.prompts[i] = temp[i];
                }
            }
            else
            {
                i_rcTrigger.prompts = new PromptType[arraySize];
            }
            int removePromptID = -1;
            for (int i = 0; i < arraySize; i++)
            {
                PromptType prompt = i_rcTrigger.prompts[i];
                EditorGUILayout.BeginHorizontal();
                i_rcTrigger.prompts[i] = (PromptType)EditorGUILayout.EnumPopup(prompt, EditorStyles.popup);
                if (GUILayout.Button("Remove"))
                {
                    removePromptID = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removePromptID != -1)
            {
                i_rcTrigger.prompts = i_rcTrigger.prompts.RemoveAt(removePromptID);
            }
            EditorGUI.indentLevel--;
        }
    }
     

    private void AddObjectWordLinking (TriggerClass i_rcTrigger, int i_nOrdinal, int i_pageOrdinal)
    {
        string[] gameObjectsDropdownNames;
        FormatGameObjectIDsAndLabels(m_rcStoryBook.pages[i_pageOrdinal].gameObjects, out gameObjectsDropdownNames);
        
        // i_rcTrigger.timestamp = EditorGUILayout.IntField("Timestamp", i_rcTrigger.timestamp, EditorStyles.numberField);

        GUILayout.Space(8);
        EditorGUI.BeginDisabledGroup(gameObjectsDropdownNames.Length == 0);
        if (gameObjectsDropdownNames.Length == 0)
        {
            i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId,
                new string[] { "No GameObjects have been added to this page" });
        }
        else
        {
            i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId, gameObjectsDropdownNames);
        }
        EditorGUI.EndDisabledGroup();

        ////////////////////////////////////
        if (i_rcTrigger.invokers == null)
        {
            i_rcTrigger.invokers = new PerformanceInvoker[0];
        }
        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        i_rcTrigger.showInvokers = EditorGUILayout.Foldout(i_rcTrigger.showInvokers, "Invokers(" + i_rcTrigger.invokers.Length + ")");
        if (GUILayout.Button("Add"))
        {
            i_rcTrigger.showInvokers = true;
            PerformanceInvoker[] rcNewInvokers = new PerformanceInvoker[i_rcTrigger.invokers.Length + 1];
            Array.Copy(i_rcTrigger.invokers, rcNewInvokers, i_rcTrigger.invokers.Length);
            rcNewInvokers[i_rcTrigger.invokers.Length] = new PerformanceInvoker();
            i_rcTrigger.invokers = rcNewInvokers;
            i_rcTrigger.invokers[i_rcTrigger.invokers.Length - 1].showVars = true;
        }
        EditorGUILayout.EndHorizontal();

        if (i_rcTrigger.showInvokers)
        {
            EditorGUI.indentLevel++;

            int invokerIndexToRemove = -1;

            for (int i = 0; i < i_rcTrigger.invokers.Length; i++)
            {
                if (i_rcTrigger.invokers[i] == null)
                {
                    i_rcTrigger.invokers[i] = new PerformanceInvoker();
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                string[] formattedTextIDDropdownValues;
                FormatTextDropdownListForTextID(m_rcStoryBook.pages[i_pageOrdinal].texts, out formattedTextIDDropdownValues);

                if (i_rcTrigger.invokers[i].invokerType == TriggerInvokerType.Text)
                {
                    i_rcTrigger.invokers[i].showVars = EditorGUILayout.Foldout(i_rcTrigger.invokers[i].showVars,
                        $"{i} - Text invoker on [" + formattedTextIDDropdownValues[i_rcTrigger.invokers[i].invokerID] + (i_rcTrigger.invokers[i].symmetricallyPaired ? "]. Symmetric" : "]."));
                } 
                else if (i_rcTrigger.invokers[i].invokerType == TriggerInvokerType.Actor)
                {
                    i_rcTrigger.invokers[i].showVars = EditorGUILayout.Foldout(i_rcTrigger.invokers[i].showVars,
                        $"{i} - Actor invoker on [" + gameObjectsDropdownNames[i_rcTrigger.invokers[i].invokerID] + (i_rcTrigger.invokers[i].symmetricallyPaired ? "]. Symmetric" : "]."));
                }
                
                if (i_rcTrigger.invokers[i].showVars)
                {
                    EditorGUI.indentLevel++;
                    i_rcTrigger.invokers[i].invokerType = (TriggerInvokerType)EditorGUILayout.EnumPopup("Invoker Type", i_rcTrigger.invokers[i].invokerType);
                    if (i_rcTrigger.invokers[i].invokerType == TriggerInvokerType.Text)
                    {
                        i_rcTrigger.invokers[i].symmetricallyPaired = EditorGUILayout.Toggle(new GUIContent("Symmetrically Paired "), i_rcTrigger.invokers[i].symmetricallyPaired);
                        i_rcTrigger = UpdatePairing(i_rcTrigger, i_rcTrigger.invokers[i]);
                        i_rcTrigger.stanzaID = EditorGUILayout.IntField("Stanza ID", i_rcTrigger.stanzaID, EditorStyles.numberField);
                        bool textIDValuesEmpty = formattedTextIDDropdownValues.Length == 0 || formattedTextIDDropdownValues.Length == 1 && formattedTextIDDropdownValues[0] == null;
                        EditorGUI.BeginDisabledGroup(textIDValuesEmpty);
                        if (textIDValuesEmpty)
                        {
                            i_rcTrigger.invokers[i].invokerID = EditorGUILayout.Popup("Word", i_rcTrigger.invokers[i].invokerID,
                                new string[] { "No Text have been entered for this page" });
                        }
                        else
                        {
                            i_rcTrigger.invokers[i].invokerID = EditorGUILayout.Popup("Word", i_rcTrigger.invokers[i].invokerID, formattedTextIDDropdownValues);
                        }
                        EditorGUI.EndDisabledGroup();

                    }
                    else if (i_rcTrigger.invokers[i].invokerType == TriggerInvokerType.Actor) //do not expose PerformanceInvoker.symmetricallyPaired on SceneObjects
                    {
                        EditorGUI.BeginDisabledGroup(gameObjectsDropdownNames.Length == 0);
                        if (gameObjectsDropdownNames.Length == 0)
                        {
                            i_rcTrigger.invokers[i].invokerID = EditorGUILayout.Popup("Invoker Actor", i_rcTrigger.invokers[i].invokerID,
                                new string[] { "No GameObjects have been added to this page yet." });
                        }
                        else
                        {
                            i_rcTrigger.invokers[i].invokerID = EditorGUILayout.Popup("Invoker Actor", i_rcTrigger.invokers[i].invokerID, gameObjectsDropdownNames);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.indentLevel--;

                }
                GUILayout.Space(4);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Remove"))
                {
                    invokerIndexToRemove = i;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (invokerIndexToRemove != -1)
            {
                i_rcTrigger.invokers = i_rcTrigger.invokers.RemoveAt(invokerIndexToRemove);
            }

            EditorGUI.indentLevel--;
        }

    }

    /// <summary>
    /// Updates which text Invoker is symetrically paired across all a SceneObject's triggers
    /// </summary>
    /// <returns>The updated InvokerList.</returns>
    /// <param name="i_rcTrigger">the trigger's invoker list.</param>
    /// <param name="i_rcLastChanged"> the last changed invoker.</param>
    TriggerClass UpdatePairing(TriggerClass i_rcTrigger, PerformanceInvoker i_rcLastChanged)
    {
        if(i_rcLastChanged.symmetricallyPaired) //don't run unless we need to change the pairing
        {
            PageClass rcCurrentPage = m_rcStoryBook.pages[m_activePageID];
            foreach (TriggerClass rcTrigger in rcCurrentPage.triggers)
            {
                if (rcTrigger.sceneObjectId.Equals(i_rcTrigger.sceneObjectId))
                {
                    for (int i = 0; i < rcTrigger.invokers.Length; i++)
                    {
                        if (rcTrigger.invokers[i] != null)
                        {
                            if (rcTrigger.invokers[i] == i_rcLastChanged)
                            {
                                // set the new pairing to the index of i_rcLastChanged
                                rcTrigger.symmetricPairingId = i;
                            }
                            else
                            {
                                rcTrigger.invokers[i].symmetricallyPaired = false;
                            }
                        }
                    }
                }
            }
        }

        return i_rcTrigger;
    }

    /// <summary>
    /// Joins the array of strings together and then split on ' ', each string in array should be trimmed and
    /// multiple whitespace characters should be replaced with one before calling this method. 
    /// </summary>
    /// <param name="array">Input array of strings</param>
    /// <returns>Simple word count</returns>
    private int GetSimpleWordCountInStringArray(string[] array) {
        return String.Join(" ", array).Split(' ').Length;
    }

    /// <summary>
    /// Uses regex replace to remove consecutive whitespaces from the input string and trim it
    /// </summary>
    /// <param name="i_input">Input string</param>
    /// <returns>String that doesn't have consecutive whitespaces and trimmed</returns>
    private string TrimAndRemoveWhitespace(string i_input) {
        return Regex.Replace(i_input, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Uses TrimAndRemoveWhitespace method to filter and make a string array from an array of TextClass objects
    /// </summary>
    /// <param name="i_textObjects">TextClass objects array</param>
    /// <returns>Cleaned up string array of page texts</returns>
    private string[] MakeCleanedUpStringArray(TextClass[] i_textObjects) {
        string[] cleanedUpSentences = new string[i_textObjects.Length];
        for (int i = 0; i < i_textObjects.Length; i++)
            cleanedUpSentences[i] = TrimAndRemoveWhitespace(i_textObjects[i].text); // removed whitespace
        return cleanedUpSentences;
    }

    /// <summary>
    /// Formats TextClass page sentences that creates string array of each individual words with a decoration that
    /// highlights the chosen word for that particular dropdown option, which underneath is just a Text ID
    /// Adding "Sentence" / for the page that has multiple TextClass objects to make it clear which sentence are we
    /// selecting the word from
    /// </summary>
    /// <param name="i_textObjects">TextClass objects array</param>
    /// <param name="i_formattedDropDownValues">Out parameter that is being filled up with dropdown values</param>
    private void FormatTextDropdownListForTextID(TextClass[] i_textObjects, out string[] i_formattedDropDownValues) {
        string[] cleanedUpSentences = MakeCleanedUpStringArray(i_textObjects);
        int totalWordCount = GetSimpleWordCountInStringArray(cleanedUpSentences);
        i_formattedDropDownValues = new string[totalWordCount];
        int formattedIndex = 0;
        for (int i = 0; i < cleanedUpSentences.Length; i++) {
            string[] words = cleanedUpSentences[i].Split();
            for (int j = 0; j < words.Length; j++) {
                string[] formattedWords = new string[words.Length];
                words.CopyTo(formattedWords, 0);
                i_formattedDropDownValues[formattedIndex++] = $"{formattedIndex} - {words[j]}";
            }
        }
    }

    private void FormatGameObjectIDsAndLabels(GameObjectClass[] i_sceneObjects, out string[] i_gameObjectNames) {
        i_gameObjectNames = new string[i_sceneObjects.Length];
        for (int i = 0; i < i_sceneObjects.Length; i++) {
            i_gameObjectNames[i] = $"{i + 1} - " + (string.IsNullOrEmpty(i_sceneObjects[i].label) ? i_sceneObjects[i].imageName : i_sceneObjects[i].label);
        }
    }

    private void EditText(TextClass i_rcText, int i_nOrdinal)
    {
        if (!string.IsNullOrEmpty(i_rcText.text))
        {
            i_rcText.Show = EditorGUILayout.Foldout(i_rcText.Show, "Text " + i_nOrdinal + " - " + i_rcText.text);
        }
        else
        {
            i_rcText.Show = EditorGUILayout.Foldout(i_rcText.Show, "Text " + i_nOrdinal);
        }
        EditorGUI.indentLevel++;

        if (i_rcText.Show)
        {
            i_rcText.id = EditorGUILayout.IntField("ID", i_rcText.id, EditorStyles.numberField);
            i_rcText.text = EditorGUILayout.TextField("Text", i_rcText.text, EditorStyles.textField);
            i_rcText.customPosition = EditorGUILayout.Toggle("Custom Position", i_rcText.customPosition);
            
            if ( i_rcText.customPosition )
            {
                i_rcText.x = EditorGUILayout.FloatField("x", i_rcText.x, EditorStyles.numberField);
                i_rcText.y = EditorGUILayout.FloatField("y", i_rcText.y, EditorStyles.numberField);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void EditGameObject(GameObjectClass i_rcGameObject, int i_nOrdinal)
    {
        if (!string.IsNullOrEmpty(i_rcGameObject.imageName))
        {
            i_rcGameObject.Show = EditorGUILayout.Foldout(i_rcGameObject.Show, i_nOrdinal + " - " + 
                (string.IsNullOrEmpty(i_rcGameObject.label) ? i_rcGameObject.imageName : i_rcGameObject.label));
        }
        else
        {
            i_rcGameObject.Show = EditorGUILayout.Foldout(i_rcGameObject.Show, "GameObject " + i_nOrdinal);
        }

        EditorGUI.indentLevel++;

        if (i_rcGameObject.Show)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);
            i_rcGameObject.id = EditorGUILayout.IntField("ID", i_rcGameObject.id, EditorStyles.numberField);
            EditorGUI.EndDisabledGroup();

            i_rcGameObject.imageName = ObjectFieldToString<GameObject>(ref i_rcGameObject.imageName, "Image", ref i_rcGameObject.editorImageObject, "svg", m_strCommonPath);

            EditorGUILayout.Space();
            // GUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            i_rcGameObject.label = EditorGUILayout.TextField("Label", i_rcGameObject.label, EditorStyles.textField);
            i_rcGameObject.tag = EditorGUILayout.TextField("Tag", i_rcGameObject.tag, EditorStyles.textField);
            i_rcGameObject.destroyOnCollision = EditorGUILayout.TextField("Destroy On Collision", i_rcGameObject.destroyOnCollision, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            i_rcGameObject.posX = EditorGUILayout.FloatField("Pos X", i_rcGameObject.posX, EditorStyles.numberField);
            i_rcGameObject.posY = EditorGUILayout.FloatField("Pos Y", i_rcGameObject.posY, EditorStyles.numberField);
            i_rcGameObject.posZ = EditorGUILayout.FloatField("Pos Z", i_rcGameObject.posZ, EditorStyles.numberField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            i_rcGameObject.rotX = EditorGUILayout.FloatField("Rot X", i_rcGameObject.rotX, EditorStyles.numberField);
            i_rcGameObject.rotY = EditorGUILayout.FloatField("Rot Y", i_rcGameObject.rotY, EditorStyles.numberField);
            i_rcGameObject.rotZ = EditorGUILayout.FloatField("Rot Z", i_rcGameObject.rotZ, EditorStyles.numberField);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            i_rcGameObject.scaleX = EditorGUILayout.FloatField("Scale X", i_rcGameObject.scaleX, EditorStyles.numberField);
            i_rcGameObject.scaleY = EditorGUILayout.FloatField("Scale Y", i_rcGameObject.scaleY, EditorStyles.numberField);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Scale Z", 0, EditorStyles.numberField);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            i_rcGameObject.orderInLayer = EditorGUILayout.IntField("Order In Layer", i_rcGameObject.orderInLayer, EditorStyles.numberField);
            i_rcGameObject.inText = EditorGUILayout.ToggleLeft("In Text", i_rcGameObject.inText, EditorStyles.textField);
            i_rcGameObject.draggable = EditorGUILayout.ToggleLeft("Draggable?", i_rcGameObject.draggable, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

//            i_rcGameObject.SpriteAnimation = (Elendow.SpritedowAnimator.SpriteAnimation)EditorGUILayout.ObjectField(i_rcGameObject.SpriteAnimation, typeof(Elendow.SpritedowAnimator.SpriteAnimation), false);

            int nRemove = -1;

            for (int k = 0; k < i_rcGameObject.anim.Length; k++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();

                EditAnim(i_rcGameObject.anim[k], k);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = k;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcGameObject.anim = i_rcGameObject.anim.RemoveAt(nRemove);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add Anim"))
            {
                Anim[] rcNewArray = new Anim[i_rcGameObject.anim.Length + 1];
                Array.Copy(i_rcGameObject.anim, rcNewArray, i_rcGameObject.anim.Length);
                rcNewArray[i_rcGameObject.anim.Length] = new Anim();
                rcNewArray[i_rcGameObject.anim.Length].id = i_rcGameObject.anim.Length;
                rcNewArray[i_rcGameObject.anim.Length].sequences = new Sequence[0];
                rcNewArray[i_rcGameObject.anim.Length].secPerFrame = new float[0];

                i_rcGameObject.anim = rcNewArray;
            }
            GUILayout.EndHorizontal();

            nRemove = -1;

            if (i_rcGameObject.Animations != null)
            {
                if (i_rcGameObject.AnimationsID == null) i_rcGameObject.AnimationsID = new int[i_rcGameObject.Animations.Length];

                if (i_rcGameObject.Animations.Length != i_rcGameObject.AnimationsID.Length)
                {
                    i_rcGameObject.AnimationsID = new int[i_rcGameObject.Animations.Length];
                }

                for (int l = 0; l < i_rcGameObject.Animations.Length; l++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();



                    // Find it in the list if it is there.  Make the the text field disabled by default.
                    for (int n = 0; n < m_rastrAnimationNames.Length; n++)
                    {
                        if (m_rastrAnimationNames[n].Equals(i_rcGameObject.Animations[l]))
                        {
                            i_rcGameObject.AnimationsID[l] = n;
                            break;
                        }
                    }

                    i_rcGameObject.AnimationsID[l] = EditorGUILayout.Popup(i_rcGameObject.AnimationsID[l], m_rastrAnimationNames);

                    EditorGUI.BeginDisabledGroup(true);
                    i_rcGameObject.Animations[l] = EditorGUILayout.TextField(m_rastrAnimationNames[i_rcGameObject.AnimationsID[l]]);
                    EditorGUI.EndDisabledGroup();


                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();

                    if (GUILayout.Button("Remove"))
                    {
                        nRemove = l;
                    }

                    if (GUILayout.Button("Edit"))
                    {
                        if ( !string.IsNullOrEmpty( i_rcGameObject.Animations[l]))
                        {
                            string[] strAnimation = AssetDatabase.FindAssets(i_rcGameObject.Animations[l]);

                            if ( strAnimation != null )
                            {
                                if (!string.IsNullOrEmpty(strAnimation[0]))
                                {
                                    string strAnimPath = AssetDatabase.GUIDToAssetPath(strAnimation[0]);

                                    if ( !string.IsNullOrEmpty(strAnimPath))
                                    {
                                        SpriteAnimation rcAnimation = AssetDatabase.LoadAssetAtPath<SpriteAnimation>(strAnimPath);

                                        if ( rcAnimation != null )
                                        {
                                            EditorSpriteAnimation rcWindow = EditorSpriteAnimation.GetWindow<EditorSpriteAnimation>();
                                            rcWindow.selectedAnimation = rcAnimation;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                if (nRemove != -1)
                {
                    i_rcGameObject.Animations = i_rcGameObject.Animations.RemoveAt(nRemove);
                    i_rcGameObject.AnimationsID = i_rcGameObject.AnimationsID.RemoveAt(nRemove);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);

            if (GUILayout.Button("Add Animation"))
            {
                string[] rcNewArray = new string[i_rcGameObject.Animations.Length + 1];
                Array.Copy(i_rcGameObject.Animations, rcNewArray, i_rcGameObject.Animations.Length);
                i_rcGameObject.Animations = rcNewArray;
                i_rcGameObject.Animations[i_rcGameObject.Animations.Length-1] = "";

                int[] rcNewArray2 = new int[i_rcGameObject.AnimationsID.Length + 1];
                Array.Copy(i_rcGameObject.AnimationsID, rcNewArray2, i_rcGameObject.AnimationsID.Length);
                i_rcGameObject.AnimationsID = rcNewArray2;
                i_rcGameObject.AnimationsID[i_rcGameObject.AnimationsID.Length-1] = 0;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical();
/*
            if (!string.IsNullOrEmpty(i_rcGameObject.imageName))
            {
                string[] strImgDir = i_rcGameObject.imageName.Split('-');

                if (strImgDir != null)
                {
                    string[] strObjects = AssetDatabase.FindAssets(i_rcGameObject.imageName + " t:Sprite");

                    foreach (string strFile in strObjects)
                    {
                        string strImagePath = AssetDatabase.GUIDToAssetPath(strFile);

                        if ( !string.IsNullOrEmpty(strImagePath))
                        {
                            Rect rcControl = EditorGUILayout.GetControlRect(GUILayout.MinWidth(300.0f), GUILayout.MinHeight(300.0f));

                            Sprite rcImage = AssetDatabase.LoadAssetAtPath<Sprite>(strImagePath);

                            if (rcImage != null)
                            {
                                if ( rcImage.texture != null )
                                {
                                    EditorGUI.DrawTextureTransparent(rcControl, rcImage.texture, ScaleMode.ScaleToFit,0, -1);
                                }
                            }

                        }
                    }
                }
            }
*/
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
    }

    private void EditAnim(Anim i_rcAnim, int i_nOrdinal)
    {
        i_rcAnim.Show = EditorGUILayout.Foldout(i_rcAnim.Show, "Anim " + i_nOrdinal);

        EditorGUI.indentLevel++;

        if (i_rcAnim.Show)
        {
            i_rcAnim.id = EditorGUILayout.IntField("ID", i_rcAnim.id, EditorStyles.numberField);
            i_rcAnim.animName = EditorGUILayout.TextField("Anim Name", i_rcAnim.animName, EditorStyles.textField);
            i_rcAnim.startX = EditorGUILayout.IntField("Start X", i_rcAnim.startX, EditorStyles.numberField);
            i_rcAnim.startY = EditorGUILayout.IntField("Start Y", i_rcAnim.startY, EditorStyles.numberField);
            i_rcAnim.startZ = EditorGUILayout.IntField("Start Z", i_rcAnim.startZ, EditorStyles.numberField);
            i_rcAnim.startIndex = EditorGUILayout.IntField("Start Index", i_rcAnim.startIndex, EditorStyles.numberField);
            i_rcAnim.endIndex = EditorGUILayout.IntField("End Index", i_rcAnim.endIndex, EditorStyles.numberField);
            i_rcAnim.onStart = EditorGUILayout.ToggleLeft("On Start", i_rcAnim.onStart, EditorStyles.textField);
            i_rcAnim.onTouch = EditorGUILayout.ToggleLeft("On Touch", i_rcAnim.onTouch, EditorStyles.textField);

            EditorGUI.indentLevel++;

            int nRemove = -1;

            for (int i = 0; i < i_rcAnim.secPerFrame.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                i_rcAnim.secPerFrame[i] = EditorGUILayout.FloatField("Frame " + i, i_rcAnim.secPerFrame[i], EditorStyles.numberField);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = i;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcAnim.secPerFrame = i_rcAnim.secPerFrame.RemoveAt(nRemove);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add secPerFrame"))
            {
                float[] rcNewArray = new float[i_rcAnim.secPerFrame.Length + 1];
                Array.Copy(i_rcAnim.secPerFrame, rcNewArray, i_rcAnim.secPerFrame.Length);
                rcNewArray[i_rcAnim.secPerFrame.Length] = 1.0f;

                i_rcAnim.secPerFrame = rcNewArray;
            }
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            nRemove = -1;

            for (int i = 0; i < i_rcAnim.sequences.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditSequences(i_rcAnim.sequences[i], i);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = i;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcAnim.sequences = i_rcAnim.sequences.RemoveAt(nRemove);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add Sequence"))
            {
                Sequence[] rcNewArray = new Sequence[i_rcAnim.sequences.Length + 1];
                Array.Copy(i_rcAnim.sequences, rcNewArray, i_rcAnim.sequences.Length);
                rcNewArray[i_rcAnim.sequences.Length] = new Sequence();
                rcNewArray[i_rcAnim.sequences.Length].movable = new Movable();

                i_rcAnim.sequences = rcNewArray;
            }
            GUILayout.EndHorizontal();

        }

        EditorGUI.indentLevel--;
    }

    private void EditSequences(Sequence i_rcSequence, int i_nOrdinal)
    {
        i_rcSequence.Show = EditorGUILayout.Foldout(i_rcSequence.Show, "Sequence " + i_nOrdinal);

        EditorGUI.indentLevel++;

        if (i_rcSequence.Show)
        {
            i_rcSequence.startFrame = EditorGUILayout.IntField("Start Frame", i_rcSequence.startFrame, EditorStyles.numberField);
            i_rcSequence.endFrame = EditorGUILayout.IntField("End Frame", i_rcSequence.endFrame, EditorStyles.numberField);
            i_rcSequence.noOfLoops = EditorGUILayout.IntField("Num of Loops", i_rcSequence.noOfLoops, EditorStyles.numberField);

            if (i_rcSequence.movable != null)
            {
                i_rcSequence.movable.speed = EditorGUILayout.FloatField("Speed", i_rcSequence.movable.speed, EditorStyles.numberField);
                i_rcSequence.movable.finalx = EditorGUILayout.FloatField("FinalX", i_rcSequence.movable.finalx, EditorStyles.numberField);
                i_rcSequence.movable.finaly = EditorGUILayout.FloatField("FinalY", i_rcSequence.movable.finaly, EditorStyles.numberField);
            }
        }


        EditorGUI.indentLevel--;
    }

    private void DrawText(AudioClip rcAudioClip, Rect i_rcRect, int i_nStart, int i_nEnd, string i_strText)
    {
        if (Event.current.type == EventType.Repaint)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(i_rcRect);

            float fStart = i_rcRect.width * ((i_nStart / 1000.0f) / rcAudioClip.length);
            float fEnd = i_rcRect.width * ((i_nEnd / 1000.0f) / rcAudioClip.length);

            Rect rcLabel = new Rect(new Vector2(fStart, 5.0f), new Vector2(fEnd - fStart, 20.0f));

            GUI.Label(rcLabel, i_strText);

            GUI.EndClip();
        }
    }

    private void DrawTimestamp(AudioClip rcAudioClip, Rect i_rcRect, int i_nStart, int i_nEnd)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Material material = new Material(Shader.Find("Hidden/Internal-Colored"));
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(i_rcRect);
            GL.PushMatrix();

            // set material for rendering.
            material.SetPass(0);

            float fStart = i_rcRect.width * ((i_nStart / 1000.0f) / rcAudioClip.length);
            float fEnd = i_rcRect.width * ((i_nEnd / 1000.0f) / rcAudioClip.length);

            GL.Begin(GL.QUADS);

            GL.Color(new Color(0.5f, 0.5f, 0.5f, 0.4f));

            GL.Vertex3(fStart, 0, 0);
            GL.Vertex3(fEnd, 0, 0);
            GL.Vertex3(fEnd, i_rcRect.height, 0);
            GL.Vertex3(fStart, i_rcRect.height, 0);

            GL.End();

            GL.PopMatrix();
            GUI.EndClip();
        }
    }

    private void DrawCurrentAudioPosition(AudioClip rcAudioClip, Rect i_rcRect, int i_nSamplePosition)
    {
        Material material = new Material(Shader.Find("Hidden/Internal-Colored"));
        // If we are currently in the Repaint event, begin to draw a clip of the size of 
        // previously reserved rectangle, and push the current matrix for drawing.
        GUI.BeginClip(i_rcRect);
        GL.PushMatrix();

        // set material for rendering.
        material.SetPass(0);

        float fStart = i_rcRect.width * ((float)i_nSamplePosition / (float)rcAudioClip.samples);
        float fEnd = fStart + 5f;

        GL.Begin(GL.QUADS);

        GL.Color(new Color(1.0f, 0.0f, 0.0f, 1.0f));

        GL.Vertex3(fStart, 0, 0);
        GL.Vertex3(fEnd, 0, 0);
        GL.Vertex3(fEnd, i_rcRect.height, 0);
        GL.Vertex3(fStart, i_rcRect.height, 0);

        GL.End();

        GL.PopMatrix();
        GUI.EndClip();
    }

    void initializeNewSetOfMissingAssets()
    {
        m_assetsMissingForPage = new HashSet<string>();
    }

    public string ObjectFieldToString<T>(ref string i_strCurrentValue, string i_strLabel, ref T i_rcContainer, string i_strExtension = "", string i_strSearchPath = "") where T : UnityEngine.Object
    {
        // If the container is null (because it doesn't serialize) then we need to populate it if we can
        if (i_rcContainer == null)
        {
            // If the string file name is not empty, then... then we need to propagate the audioclip.
            if (!string.IsNullOrEmpty(i_strCurrentValue))
            {
                string strPath = null;

                if (!string.IsNullOrEmpty(i_strSearchPath))
                {
                    if (!m_assetsMissingForPage.Contains(i_strCurrentValue))
                    {
                        strPath = FindAssetPathRecursive(i_strCurrentValue, i_strSearchPath);
                        if (string.IsNullOrEmpty(strPath))
                        {
                            Debug.LogWarning("Asset not found: " + i_strCurrentValue);
                            this.ShowNotification(new GUIContent(i_strCurrentValue + " not found!"));
                            m_assetsMissingForPage.Add(i_strCurrentValue);
                        }
                    }
                }
                else
                {
                    if (!m_assetsMissingForPage.Contains(i_strCurrentValue))
                    {
                        strPath = FindAssetPathRecursive(i_strCurrentValue, Application.dataPath);
                        if (string.IsNullOrEmpty(strPath))
                        {
                            Debug.LogWarning("Asset not found: " + i_strCurrentValue);
                            this.ShowNotification(new GUIContent(i_strCurrentValue + " not found!"));
                            m_assetsMissingForPage.Add(i_strCurrentValue);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(strPath))
                {
                    i_rcContainer = AssetDatabase.LoadAssetAtPath<T>(strPath);
                }
            }
        }
        else
        {
            string strObjectName = i_rcContainer.name;

            // The clip has been already loaded.
            // If the clip name doesn't match, set it.

            string strStrippedName = System.IO.Path.GetFileNameWithoutExtension(i_strCurrentValue);

            if (!strStrippedName.Equals(strObjectName))
            {
                i_strCurrentValue = strObjectName;
            }
        }

        i_rcContainer = (T)EditorGUILayout.ObjectField(i_strLabel, i_rcContainer, typeof(T), false);

        return i_strCurrentValue;
    }

    public string FindAssetPathRecursive(string i_strAssetName, string i_strStartingPath = "")
    {
        foreach (string strFile in Directory.GetFiles(i_strStartingPath))
        {
            if (strFile.ToLower().Contains(".meta")) continue;

            string strStrippedAssetName = System.IO.Path.GetFileNameWithoutExtension(i_strAssetName);
            string strStrippedFile = System.IO.Path.GetFileNameWithoutExtension(strFile);

            Debug.Log("Searching " + strFile + "...");
            if (strStrippedAssetName.Equals(strStrippedFile) || strStrippedFile.Contains('_' + strStrippedAssetName))
            {
                string strAssetPath = Application.dataPath.Replace("/Assets", "");
                string strFileName = strFile.Replace("\\", "/");
                strFileName = strFileName.Replace(strAssetPath, "").TrimStart('/');
                Debug.Log("Bingo! " + strFileName);
                return strFileName;
            }
        }

        foreach (string strDirectory in Directory.GetDirectories(i_strStartingPath))
        {
            Debug.Log("Searching Directory" + strDirectory + "...");

            string strSearch = FindAssetPathRecursive(i_strAssetName, strDirectory);

            if (!string.IsNullOrEmpty(strSearch))
            {
                return strSearch;
            }
        }

        return "";
    }

    private void ClearRect(Rect i_rcRect, Color i_rcColor)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Material material = new Material(Shader.Find("Hidden/Internal-Colored"));
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(i_rcRect);
            GL.PushMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            GL.Clear(true, false, i_rcColor);
            material.SetPass(0);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.
            GL.Begin(GL.QUADS);
            GL.Color(i_rcColor);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(i_rcRect.width, 0, 0);
            GL.Vertex3(i_rcRect.width, i_rcRect.height, 0);
            GL.Vertex3(0, i_rcRect.height, 0);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(i_rcRect.width, 0, 0);

            GL.Vertex3(0, i_rcRect.height, 0);
            GL.Vertex3(i_rcRect.width, i_rcRect.height, 0);

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, i_rcRect.height, 0);

            GL.Vertex3(i_rcRect.width, 0, 0);
            GL.Vertex3(i_rcRect.width, i_rcRect.height, 0);

            GL.End();

            GL.PopMatrix();
            GUI.EndClip();
        }
    }

    private void RenderAudioClip(AudioClip clip, Rect wantedRect, float scaleFactor)
    {
        if (Event.current.type == EventType.Repaint)
        {
            scaleFactor *= 0.95f;
            float[] minMaxData = new float[clip.samples * clip.channels];

            clip.GetData(minMaxData, 0);
            int numChannels = clip.channels;
            int numSamples = (minMaxData != null) ? (minMaxData.Length / (2 * numChannels)) : 0;
            float num = wantedRect.height / (float)numChannels;
            int channel;
            for (channel = 0; channel < numChannels; channel++)
            {
                Rect r = new Rect(wantedRect.x, wantedRect.y + num * (float)channel, wantedRect.width, num);
                Color curveColor = new Color(1f, 0.549019635f, 0f, 1f);
                AudioCurveRendering.DrawMinMaxFilledCurve(r, delegate (float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    if (numSamples <= 0)
                    {
                        minValue = 0f;
                        maxValue = 0f;
                    }
                    else
                    {
                        float f = Mathf.Clamp(x * (float)(numSamples - 2), 0f, (float)(numSamples - 2));
                        int num2 = (int)Mathf.Floor(f);
                        int num3 = (num2 * numChannels + channel) * 2;
                        int num4 = num3 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[num3 + 1], minMaxData[num4 + 1]) * scaleFactor;
                        maxValue = Mathf.Max(minMaxData[num3], minMaxData[num4]) * scaleFactor;
                        if (minValue > maxValue)
                        {
                            float num5 = minValue;
                            minValue = maxValue;
                            maxValue = num5;
                        }
                    }
                });
            }

        }
    }

    public void AddImagesInPath(string i_strPath)
    {
        string[] allowedExtensions = new string[] { "*.svg", "*.png" };
        string[] Directories = Directory.GetDirectories(i_strPath);

        foreach (string strDir in Directories)
        {
            AddImagesInPath(strDir);
        }

        foreach (string strType in allowedExtensions)
        {
            foreach (string strFile in Directory.GetFiles(i_strPath, strType))
            {
                AssetImporter a = AssetImporter.GetAtPath(strFile);
                a.SetAssetBundleNameAndVariant("differentplaces", "");
            }
        }
    }

    public void ConstructAnimationsInPath(string i_strPath)
    {
        string[] allowedExtensions = new string[] { "*.svg", "*.png" };
        string[] Directories = Directory.GetDirectories(i_strPath);

        // Presuming path like /Animations/1/<name>/<name>-1
        // Presuming path like /Animations/1/<name>/<name>-2
        // Presuming path like /Animations/1/<name>/<name>-.
        // Presuming path like /Animations/1/<name>/<name>-N

        // Walk through the page directories
        foreach (string strPage in Directories)
        {
            Debug.Log(strPage + " is the page.");
            string strPageNumber = strPage.Replace(i_strPath + "\\", "");
            Debug.Log(strPageNumber + " is the the stripped page.");

            string[] GameObjects = Directory.GetDirectories(strPage);

            foreach (string strGameObjectDirectories in GameObjects)
            {
                Debug.Log("GameObject Directory: " + strGameObjectDirectories);

                foreach (string strType in allowedExtensions)
                {
                    string [] AnimationFiles = Directory.GetFiles( strGameObjectDirectories , strType);

                    if ( AnimationFiles != null )
                    {
                        if ( AnimationFiles.Length > 0)
                        {
                            string strAnimationName = AnimationFiles[0].Replace(strGameObjectDirectories + "\\","");
                            strAnimationName = strAnimationName.Replace(strType.Replace("*",""), "");
                            strAnimationName = strAnimationName.Replace("-1", "");
                            Debug.Log("Animation Name = " + strAnimationName);

                            CreateNewAnimation(strAnimationName, strGameObjectDirectories, AnimationFiles);
                        }
                    }

                }
            }

        }
    }

    public void CreateNewAnimation(string i_strFile, string i_strPath, string[] i_strFrames)
    {
        // First Create the Asset
        SpriteAnimation asset = CreateInstance<SpriteAnimation>();

        asset.Frames = new List<Sprite>();
        asset.FramesDuration = new List<int>();

        // Add the sprites to the frames
        foreach (string strFrame in i_strFrames)
        {
            Sprite rcSprite = AssetDatabase.LoadAssetAtPath<Sprite>(strFrame);
            if (rcSprite != null)
            {
                asset.Frames.Add(rcSprite);
                asset.FramesDuration.Add(30 / i_strFrames.Length);
            }
        }

#if UNITY_EDITOR_OSX
        string strFile = i_strPath + "/" + i_strFile + ".asset";

        if ( !File.Exists(strFile) )
        {
            AssetDatabase.CreateAsset(asset, i_strFile + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetImporter importer = AssetImporter.GetAtPath(i_strFile + ".asset");
            importer.SetAssetBundleNameAndVariant(m_strAssetBundleName, "");
        }
        else
        {
            Debug.Log(strFile + " exists.  Skipping animation.");
        }
#endif

#if UNITY_EDITOR_WIN
        string strFile = i_strPath + "\\" + i_strFile + ".asset";

        if (!File.Exists(strFile))
        {
            AssetDatabase.CreateAsset(asset, strFile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetImporter.GetAtPath(strFile).SetAssetBundleNameAndVariant(m_strAssetBundleName, "");
        }
        else
        {
            Debug.Log(strFile + " exists.  Skipping animation.");
        }
#endif
    }

    public string OnBookGUI<T>(TriggerClass i_rcTrigger, int i_nOrdinal) where T : PerformanceParams, new()
    {
        string strParams = "";

        if (i_rcTrigger.EditorFields == null)
        {
            T rcParams = new T();
            if (!string.IsNullOrEmpty(i_rcTrigger.Params))
            {
                rcParams = JsonUtility.FromJson<T>(i_rcTrigger.Params);
            }

            i_rcTrigger.EditorFields = ExposeFields.GetFields(rcParams);
            i_rcTrigger.PerformanceParams = (PerformanceParams)rcParams;
        }

        if (i_rcTrigger.EditorFields != null)
        {
            ExposeFields.Expose(i_rcTrigger.EditorFields, i_nOrdinal, this);
        }

        if (i_rcTrigger.PerformanceParams != null)
        {
            T rcParams = (T)i_rcTrigger.PerformanceParams;
            strParams = JsonUtility.ToJson(rcParams);
        }
        return strParams;
    }

}

public static class OpenBookEditor
{

    [MenuItem("Curious Reader/Book Editor")]
    public static void ShowWindow()
    {

        string path = EditorUtility.OpenFilePanel("Find Book File", "", "json");

        if (path.Length != 0)
        {
            Debug.Log(path);

            LoadBookAtPathInEditor(path, false);
        }
    }

    /// <summary>
    /// Opens the selected JSON file in the current editor. Shortcut: Ctrl + Shift + E
    /// </summary>
    [MenuItem("Assets/Open In Book Editor %#e")]
    public static void OpenJSONFileInFirstBookEditor()
    {
        if (Selection.activeObject == null) return;
        string filePath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        if (string.IsNullOrEmpty(filePath) || System.IO.Path.GetExtension(filePath).ToLower() != ".json")
            return;

        LoadBookAtPathInEditor(filePath, false);
    }

    /// <summary>
    /// Opens the selected JSON file in a new editor. Shortcut: Alt + Shift + E
    /// </summary>
    [MenuItem("Assets/Open In New Book Editor &#e")]
    public static void OpenJSONFileInNewBookEditor() 
    {
        if (Selection.activeObject == null) 
        {
            Debug.Log("No selected object found in the assets.");
            return;
        }
        string filePath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        if (string.IsNullOrEmpty(filePath) || System.IO.Path.GetExtension(filePath).ToLower() != ".json")
            return;

        LoadBookAtPathInEditor(filePath, true);
    }

    /// <summary>
    /// Loads the JSON book data file at path in new or existing editor depending on the second parameter
    /// </summary>
    /// <param name="i_path">Path of the book file</param>
    /// <param name="i_openInNewEditor">If true, loading the file at path in a new BookEditor instance</param>
    private static void LoadBookAtPathInEditor(string i_path, bool i_openInNewEditor) 
    {
        BookEditor rcEditor;
        
        if (i_openInNewEditor)
        {
            rcEditor = EditorWindow.CreateInstance<BookEditor>();
        }
        else
        {
            rcEditor = (BookEditor)EditorWindow.GetWindow(typeof(BookEditor));
        }
        
        rcEditor.m_strBookPath = i_path;
        rcEditor.m_needToLoadBookContent = true;
        rcEditor.Show();
    }
}

[CanEditMultipleObjects, CustomEditor(typeof(Transform))]
public class TransformEditor2D : Editor
{
    public override void OnInspectorGUI()
    {
        Transform transform = (Transform)target;
        
        Vector3 position = EditorGUILayout.Vector3Field("Local Pos", transform.localPosition);
        Vector3 rotation = EditorGUILayout.Vector3Field("Local Rot", transform.localRotation.eulerAngles);
        Vector3 scale = EditorGUILayout.Vector2Field("Scale", transform.localScale);

        if (GUI.changed)
        {
            Undo.RecordObject(transform, "Transform Changed");
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }
    }

}
