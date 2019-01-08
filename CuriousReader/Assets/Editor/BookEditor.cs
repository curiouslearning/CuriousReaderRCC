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

public class BookEditor : EditorWindow
{
    StoryBookJson m_rcStoryBook;

    public bool m_bMicrophoneHot = false;
    public bool m_bWasMicrophoneHotLastFrame = false;
    public AudioClip m_rcRecordingClip;
    float m_fRecordingStart;

    public string m_strBookPath;
    string m_strAssetPath;
    string m_strCommonPath;
    string m_strBookRoot;
    string m_strAnimPath;
    string m_strImagePath;

    List<string> m_rcImageNames;

    Rect[] m_rcAudioRects;

    bool[] m_bShowPage;

    AudioClip m_rcTempAudioClip;

    AudioClip[] m_rcPageAudio;

    Vector2 m_vScrollPosition;

    GUIStyle m_boldFoldoutStyle;
    bool    m_foldoutPathsText      = false;
    bool    m_foldoutPageData       = false;
    bool    m_foldoutPageText       = false;
    bool    m_foldoutPageAudio      = false;
    bool    m_foldoutTimestamps     = false;
    bool    m_foldoutGameObjects    = false;
    bool    m_foldoutTriggers       = false;

    int     m_activePageID = 0;

    VectorUtils.TessellationOptions tessOptions = new VectorUtils.TessellationOptions()
    {
        StepDistance = 100.0f,
        MaxCordDeviation = 0.5f,
        MaxTanAngleDeviation = 0.1f,
        SamplingStepSize = 0.01f
    };

    private void OnGUI()
    {
        m_boldFoldoutStyle = EditorStyles.foldout;

        if (m_rcStoryBook == null)
        {
            if (!string.IsNullOrEmpty(m_strBookPath))
            {
                string strBook = File.ReadAllText(m_strBookPath);
                m_rcStoryBook = JsonUtility.FromJson<StoryBookJson>(strBook);

                if (m_rcStoryBook != null)
                {
                    m_strBookRoot = m_strBookPath;
                    m_strCommonPath = m_strBookPath.Replace(System.IO.Path.GetFileName(m_strBookRoot), "");
#if UNITY_EDITOR_OSX
                    m_strCommonPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(m_strCommonPath).FullName).FullName).FullName + "/Common";
#endif

#if UNITY_EDITOR_WIN
                    m_strCommonPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(m_strCommonPath).FullName).FullName).FullName + "\\Common";
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

                    // Allocate each of the audio tracks for each of the pages
                    // NOTE: We might need to do this for sub-audio?  Maybe?
                    m_rcAudioRects = new Rect[m_rcStoryBook.pages.Length];
                    m_rcPageAudio = new AudioClip[m_rcStoryBook.pages.Length];
                }

                m_rcImageNames = GetImagesInPath(m_strCommonPath,"-1");

            }
            return;
        }

        #region Title & Current Page 

        // Editor Title
        GUILayout.Space(8);

        GUIStyle bookEditorLabelStyle = new GUIStyle();
        bookEditorLabelStyle.alignment = TextAnchor.UpperCenter;
        bookEditorLabelStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("Book Editor", bookEditorLabelStyle);

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
        
        m_activePageID = EditorGUILayout.Popup("Current Page", m_activePageID, pageTexts, currentPageDropdownStyle, GUILayout.ExpandWidth(false), GUILayout.Width(580));

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
            rcNewArray[currentPage.gameObjects.Length].SpriteAnimation = new Elendow.SpritedowAnimator.SpriteAnimation[0];
            rcNewArray[currentPage.gameObjects.Length].draggable = false;
            rcNewArray[currentPage.gameObjects.Length].id = currentPage.gameObjects.Length;
            currentPage.gameObjects = rcNewArray;

            this.ShowNotification(new GUIContent("New Scene Game Object Added to Page " + m_activePageID));
        }

        if (GUILayout.Button("Add Page Trigger", GUILayout.Height(24)))
        {
            PageClass currentPage = m_rcStoryBook.pages[m_activePageID];
            TriggerClass[] rcNewArray = new TriggerClass[currentPage.triggers.Length + 1];
            Array.Copy(currentPage.triggers, rcNewArray, currentPage.triggers.Length);
            rcNewArray[currentPage.triggers.Length] = new TriggerClass();
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
                m_activePageID--;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(m_activePageID == m_rcStoryBook.pages.Length - 1);
        string nextButtonLabel = m_activePageID == m_rcStoryBook.pages.Length - 1 ? 
            "Next Page" : 
            string.Format("Next Page({0})", Mathf.Clamp(m_activePageID + 1, 1, m_rcStoryBook.pages.Length - 1));
        if (GUILayout.Button(nextButtonLabel, GUILayout.Height(24))) {
            if (m_activePageID < m_rcStoryBook.pages.Length -1)
                m_activePageID++;
        }
        EditorGUI.EndDisabledGroup();

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

        if (GUILayout.Button("Add Images To AssetBundle", GUILayout.Height(24)))
        {
            Debug.Log(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strImagePath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
        }

        if (GUILayout.Button("Save JSON", GUILayout.Height(24)))
        {
            Debug.Log(m_strBookPath + " file to be saved.");

            StreamWriter rcWriter = new StreamWriter(m_strBookPath, false);
            rcWriter.Write(JsonUtility.ToJson(m_rcStoryBook, true));
            rcWriter.Close();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            this.ShowNotification(new GUIContent("Changes Saved!"));
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        #endregion
    
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

        if (m_foldoutPageAudio) {
            EditorGUI.indentLevel++;

            i_rcPage.audioFile = EditorGUILayout.TextField("Audio File", i_rcPage.audioFile, EditorStyles.textField);

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

            if (!string.IsNullOrEmpty(i_rcPage.audioFile)) 
            {
                string strPath = m_strBookPath.Replace("Resources/" + System.IO.Path.GetFileName(m_strBookPath), "Audio/Stanza/" + i_rcPage.audioFile);
                string strAssetPath = Application.dataPath.Replace("/Assets", "");
                strPath = strPath.Replace(strAssetPath, "").TrimStart('/');

                m_rcPageAudio[i_nOrdinal] = AssetDatabase.LoadAssetAtPath<AudioClip>(strPath);

                if (m_rcPageAudio[i_nOrdinal] != null)
                {
                    m_rcAudioRects[i_nOrdinal] = EditorGUILayout.GetControlRect(false, 100.0f);
                    m_rcAudioRects[i_nOrdinal].xMin = 20.0f;

                    ClearRect(m_rcAudioRects[i_nOrdinal], Color.black);
                    RenderAudioClip(m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], 1.0f);

                    Rect rcRect2 = EditorGUILayout.GetControlRect();
                    rcRect2.xMin = 20.0f;

                    AudioClip rcAudioClip = (AudioClip)EditorGUI.ObjectField(rcRect2, m_rcPageAudio[i_nOrdinal], m_rcPageAudio[i_nOrdinal].GetType());

                    GUILayout.BeginHorizontal();
                    GUILayout.Space((EditorGUI.indentLevel + 1) * 10);

                    GUILayout.EndHorizontal();
                }
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

                EditTimestamps(i_rcPage.timestamps[j], m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], i_rcPage.texts, j);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nDelete = j;
                }

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
                    i_rcTimestamp.Show = EditorGUILayout.Foldout(i_rcTimestamp.Show, "Timestamp " + i_nOrdinal + " - " + rastrWords[i_nOrdinal]);
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
            if (i_rcTimestamp.SubClip == null)
            {
                i_rcTimestamp.SubClip = MakeSubclip(i_rcClip, i_rcTimestamp.AudioData, i_rcTimestamp.start / 1000.0f, i_rcTimestamp.end / 1000.0f);

                string strPath = m_strBookPath.Replace("Resources/" + System.IO.Path.GetFileName(m_strBookPath), "Audio/Stanza/TimeStamp" + i_nOrdinal + ".ogg");
                string strAssetPath = Application.dataPath.Replace("/Assets", "");
                strPath = strPath.Replace(strAssetPath, "").TrimStart('/');

                Debug.Log(strPath);

                //                AssetDatabase.CreateAsset(i_rcTimestamp.SubClip, strPath);
            }

            if (i_rcTimestamp.SubClip != null)
            {
                Rect rcRect = EditorGUILayout.GetControlRect(false, 100.0f);
                rcRect.xMin = 40.0f;

                ClearRect(rcRect, Color.black);
                RenderAudioClip(i_rcTimestamp.SubClip, rcRect, 1.0f);

                if (GUILayout.Button("Play"))
                {
                    AudioUtility.PlayClip(i_rcTimestamp.SubClip);
                }
            }

            i_rcTimestamp.start = EditorGUILayout.IntField("Start", i_rcTimestamp.start, EditorStyles.numberField);
            i_rcTimestamp.end = EditorGUILayout.IntField("End", i_rcTimestamp.end, EditorStyles.numberField);
            i_rcTimestamp.starWord = EditorGUILayout.TextField("Star Word", i_rcTimestamp.starWord, EditorStyles.numberField);
            i_rcTimestamp.audio = EditorGUILayout.TextField("Audio", i_rcTimestamp.audio, EditorStyles.textField);
            i_rcTimestamp.wordIdx = EditorGUILayout.IntField("Word Index", i_rcTimestamp.wordIdx, EditorStyles.numberField);

            if (i_rcClip != null)
            {
                DrawTimestamp(i_rcClip, i_rcRect, i_rcTimestamp.start, i_rcTimestamp.end);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void EditTriggers(TriggerClass i_rcTrigger, int i_nOrdinal, int i_pageOrdinal)
    {
        i_rcTrigger.Show = EditorGUILayout.Foldout(i_rcTrigger.Show, "Trigger " + i_nOrdinal);

        // Add Highlight Trigger 
        // Choose a gameobject, choose a word
        // If the button is pressed, create it and add it to the list

        // Add Animation Trigger
        // Choose a gameobject, choose an animation, choose a word
        // If the button is pressed, create it and add it to the list

        EditorGUI.indentLevel++;

        if (i_rcTrigger.Show)
        {
            i_rcTrigger.type = (TriggerType)EditorGUILayout.EnumPopup(i_rcTrigger.type, EditorStyles.popup);

            if ( i_rcTrigger.type == TriggerType.Navigation )
            {
                i_rcTrigger.DeactivateNextButton = EditorGUILayout.ToggleLeft("Deactivate Next", i_rcTrigger.DeactivateNextButton);
                i_rcTrigger.NavigationPage = EditorGUILayout.IntField("Page Number", i_rcTrigger.NavigationPage, EditorStyles.numberField);
                string[] gameObjectsDropdownNames;

                FormatGameObjectIDsAndLabels(m_rcStoryBook.pages[i_pageOrdinal].gameObjects, out gameObjectsDropdownNames);
                EditorGUI.BeginDisabledGroup(gameObjectsDropdownNames.Length == 0);
                if (gameObjectsDropdownNames.Length == 0) {
                    i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId, 
                        new string[]{ "No GameObjects have been entered for this page" });
                } else {
                    i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId, gameObjectsDropdownNames);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                i_rcTrigger.animId = EditorGUILayout.IntField("Anim ID", i_rcTrigger.animId, EditorStyles.numberField);
                i_rcTrigger.stanzaID = EditorGUILayout.IntField("Stanza ID", i_rcTrigger.stanzaID, EditorStyles.numberField);

                // Text ID dropdown which is now an actual Word
                string[] formattedTextIDDropdownValues;
                FormatTextDropdownListForTextID(m_rcStoryBook.pages[i_pageOrdinal].texts, out formattedTextIDDropdownValues);
                bool textIDValuesEmpty = formattedTextIDDropdownValues.Length == 0 || formattedTextIDDropdownValues.Length == 1 && formattedTextIDDropdownValues[0] == null;
                EditorGUI.BeginDisabledGroup(textIDValuesEmpty);
                if (textIDValuesEmpty) {
                    i_rcTrigger.textId = EditorGUILayout.Popup("Word", i_rcTrigger.textId, 
                        new string[]{ "No Text have been entered for this page" });
                } else {
                    i_rcTrigger.textId = EditorGUILayout.Popup("Word", i_rcTrigger.textId, formattedTextIDDropdownValues);
                }
                EditorGUI.EndDisabledGroup();

                i_rcTrigger.timestamp = EditorGUILayout.IntField("Timestamp", i_rcTrigger.timestamp, EditorStyles.numberField);

                // Scene Object Dropdown
                string[] gameObjectsDropdownNames;
                FormatGameObjectIDsAndLabels(m_rcStoryBook.pages[i_pageOrdinal].gameObjects, out gameObjectsDropdownNames);
                EditorGUI.BeginDisabledGroup(gameObjectsDropdownNames.Length == 0);
                if (gameObjectsDropdownNames.Length == 0) {
                    i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId, 
                        new string[]{ "No GameObjects have been entered for this page" });
                } else {
                    i_rcTrigger.sceneObjectId = EditorGUILayout.Popup("Scene Object", i_rcTrigger.sceneObjectId, gameObjectsDropdownNames);
                }
                EditorGUI.EndDisabledGroup();
                i_rcTrigger.typeOfLinking = EditorGUILayout.IntField("Type of Linking", i_rcTrigger.typeOfLinking, EditorStyles.numberField);
            }
        }

        EditorGUI.indentLevel--;
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
                formattedWords[j] = String.Format("▶ {0} ◀", words[j]);
                if (cleanedUpSentences.Length > 1)
                    i_formattedDropDownValues[formattedIndex++] = String.Format("{0} / [{1}] {2}", cleanedUpSentences[i], formattedIndex, String.Join(" ", formattedWords));
                else
                    i_formattedDropDownValues[formattedIndex++] = String.Format("[{0}] {1}", formattedIndex, String.Join(" ", formattedWords));
            }
        }
    }

    private void FormatGameObjectIDsAndLabels(GameObjectClass[] i_sceneObjects, out string[] i_gameObjectNames) {
        i_gameObjectNames = new string[i_sceneObjects.Length];
        for (int i = 0; i < i_sceneObjects.Length; i++) {
            i_gameObjectNames[i] = string.Format("[{0}] {1}", i + 1, 
                string.IsNullOrEmpty(i_sceneObjects[i].label) ? i_sceneObjects[i].imageName : i_sceneObjects[i].label);
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
        }

        EditorGUI.indentLevel--;
    }

    private void EditGameObject(GameObjectClass i_rcGameObject, int i_nOrdinal)
    {
        if (!string.IsNullOrEmpty(i_rcGameObject.imageName))
        {
            i_rcGameObject.Show = EditorGUILayout.Foldout(i_rcGameObject.Show, "GameObject " + i_nOrdinal + " - " + i_rcGameObject.imageName);
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
            i_rcGameObject.id = EditorGUILayout.IntField("ID", i_rcGameObject.id, EditorStyles.numberField);

            if ( m_rcImageNames != null )
            {
                if (m_rcImageNames.Contains(i_rcGameObject.imageName))
                {
                    i_rcGameObject.ImageIndex = m_rcImageNames.IndexOf(i_rcGameObject.imageName);

                    i_rcGameObject.ImageIndex = EditorGUILayout.Popup(i_rcGameObject.ImageIndex, m_rcImageNames.ToArray());
                    i_rcGameObject.imageName = m_rcImageNames[i_rcGameObject.ImageIndex];
                }
                else
                {
                    i_rcGameObject.ImageIndex = EditorGUILayout.Popup(i_rcGameObject.ImageIndex, m_rcImageNames.ToArray());
                    i_rcGameObject.imageName = m_rcImageNames[i_rcGameObject.ImageIndex];
                }
            }

            i_rcGameObject.imageName = EditorGUILayout.TextField("Image Name", i_rcGameObject.imageName, EditorStyles.textField);

            i_rcGameObject.label = EditorGUILayout.TextField("Label", i_rcGameObject.label, EditorStyles.textField);
            i_rcGameObject.tag = EditorGUILayout.TextField("Tag", i_rcGameObject.tag, EditorStyles.textField);
            i_rcGameObject.destroyOnCollision = EditorGUILayout.TextField("Destroy On Collision", i_rcGameObject.destroyOnCollision, EditorStyles.textField);
            i_rcGameObject.posX = EditorGUILayout.FloatField("Pos X", i_rcGameObject.posX, EditorStyles.numberField);
            i_rcGameObject.posY = EditorGUILayout.FloatField("Pos Y", i_rcGameObject.posY, EditorStyles.numberField);
            i_rcGameObject.posZ = EditorGUILayout.FloatField("Pos Z", i_rcGameObject.posZ, EditorStyles.numberField);
            i_rcGameObject.rotX = EditorGUILayout.FloatField("Rot X", i_rcGameObject.rotX, EditorStyles.numberField);
            i_rcGameObject.rotY = EditorGUILayout.FloatField("Rot Y", i_rcGameObject.rotY, EditorStyles.numberField);
            i_rcGameObject.rotZ = EditorGUILayout.FloatField("Rot Z", i_rcGameObject.rotZ, EditorStyles.numberField);
            i_rcGameObject.scaleX = EditorGUILayout.FloatField("Scale X", i_rcGameObject.scaleX, EditorStyles.numberField);
            i_rcGameObject.scaleY = EditorGUILayout.FloatField("Scale Y", i_rcGameObject.scaleY, EditorStyles.numberField);
            i_rcGameObject.orderInLayer = EditorGUILayout.IntField("Order In Layer", i_rcGameObject.orderInLayer, EditorStyles.numberField);
            i_rcGameObject.inText = EditorGUILayout.ToggleLeft("In Text", i_rcGameObject.inText, EditorStyles.textField);
            i_rcGameObject.draggable = EditorGUILayout.ToggleLeft("Draggable?", i_rcGameObject.draggable, EditorStyles.textField);

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

            for (int l = 0; l < i_rcGameObject.SpriteAnimation.Length; l++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();

                i_rcGameObject.SpriteAnimation[l] = (Elendow.SpritedowAnimator.SpriteAnimation)EditorGUILayout.ObjectField(i_rcGameObject.SpriteAnimation[l], typeof(Elendow.SpritedowAnimator.SpriteAnimation), false);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    nRemove = l;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (nRemove != -1)
            {
                i_rcGameObject.SpriteAnimation = i_rcGameObject.SpriteAnimation.RemoveAt(nRemove);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);

            if (GUILayout.Button("Add Sprite Animation"))
            {
                SpriteAnimation[] rcNewArray = new SpriteAnimation[i_rcGameObject.SpriteAnimation.Length + 1];
                Array.Copy(i_rcGameObject.SpriteAnimation, rcNewArray, i_rcGameObject.SpriteAnimation.Length);

                i_rcGameObject.SpriteAnimation = rcNewArray;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(i_rcGameObject.imageName))
            {
                string[] strImgDir = i_rcGameObject.imageName.Split('-');

                if (strImgDir != null)
                {
                    string strAssetPath = Application.dataPath.Replace("/Assets", "");
                    string strPath = m_strAnimPath + "\\" + strImgDir[0] + "\\" + i_rcGameObject.imageName;
                    strPath = strPath.Replace("\\", "/");
                    strPath = strPath.Replace(strAssetPath, "").TrimStart('/');

                    GameObject rcGo = AssetDatabase.LoadAssetAtPath<GameObject>(strPath);

                    Rect rcControl = EditorGUILayout.GetControlRect(GUILayout.MinWidth(300.0f), GUILayout.MinHeight(300.0f));

                    if (rcGo != null)
                    {
                        Texture2D rcTexture = AssetPreview.GetAssetPreview(rcGo);

                        if (rcTexture != null)
                        {
                            EditorGUI.DrawTextureTransparent(rcControl, rcTexture, ScaleMode.ScaleToFit, 0, -1);
                        }
                    }


                    /*                    if (strPath.Contains(".svg") || strPath.Contains(".SVG"))
                                        {

                                            string strContents = File.ReadAllText(strPath);

                                            // Dynamically import the SVG data, and tessellate the resulting vector scene.
                                            var sceneInfo = SVGParser.ImportSVG(new StringReader(strContents));
                                            var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

                                            // Build a sprite with the tessellated geometry.
                                            var sprite = VectorUtils.BuildSprite(geoms, 10.0f, VectorUtils.Alignment.Center, Vector2.zero, 128);

                                            //                        Texture rcTexture = (Texture)AssetDatabase.LoadAssetAtPath(strPath, typeof(Texture));


                                            var mat = new Material(Shader.Find("Unlit/Vector"));

                                            Texture2D rcText = VectorUtils.RenderSpriteToTexture2D(sprite, (int)rcControl.width, (int)rcControl.height, mat);
                                        }*/
                }
            }

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

    private AudioClip MakeSubclip(AudioClip i_rcClip, float[] i_rcData, float i_fStart, float i_fStop)
    {
        m_rcTempAudioClip = i_rcClip;

        int nFrequency = i_rcClip.frequency;
        float fTimeLength = i_fStop - i_fStart;
        int nSamplesLength = (int)(nFrequency * fTimeLength);

        AudioClip rcNewClip = AudioClip.Create(i_rcClip.name + "-sub", nSamplesLength, 1, nFrequency, false);
        i_rcData = new float[nSamplesLength];
        bool Result = i_rcClip.GetData(i_rcData, (int)(nFrequency * i_fStart));
        bool Result2 = rcNewClip.SetData(i_rcData, 0);

        return rcNewClip;
    }

    void OnAudioRead(float[] data)
    {
        m_rcTempAudioClip.GetData(data, (int)(m_rcTempAudioClip.frequency * 0.3f));

        /*        int position = 0;
                int count = 0;
                while (count < data.Length)
                {
                    data[count] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * 440.0f * position / 48000));
                    position++;
                    count++;
                } */
    }

    void OnAudioSetPosition(int newPosition)
    {
        int position = newPosition;
    }

    public List<string> GetImagesInPath(string i_strPath, string i_strPattern="")
    {
        List<string> rcFiles = new List<string>();
        string[] allowedExtensions = new string[] { "*.svg", "*.png", "prefab_*" };
        string[] Directories = Directory.GetDirectories(i_strPath);

        foreach (string strDir in Directories)
        {
            List<string> rcReturnFiles = GetImagesInPath(strDir,i_strPattern);

            if (rcReturnFiles != null)
            {
                foreach (string strFile in rcReturnFiles)
                {
                    rcFiles.Add(System.IO.Path.GetFileNameWithoutExtension(strFile));
                }
            }
        }

        foreach (string strType in allowedExtensions)
        {
            foreach (string strFile in Directory.GetFiles(i_strPath, strType))
            {
                if (string.IsNullOrEmpty(i_strPattern))
                {
                    rcFiles.Add(System.IO.Path.GetFileNameWithoutExtension(strFile));
                }
                else
                {
                    if ( strFile.Contains(i_strPattern))
                    {
                        rcFiles.Add(System.IO.Path.GetFileNameWithoutExtension(strFile));
                    }
                }
            }
        }

        return rcFiles;
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
                AssetImporter.GetAtPath(strFile).SetAssetBundleNameAndVariant("differentplaces", "");
            }
        }
    }
}

public class OpenBookEditor
{
    [MenuItem("Curious Reader/Book Editor")]
    static void ShowWindow()
    {

     string path = EditorUtility.OpenFilePanel("Find Book File", "", "json");

        if (path.Length != 0)
        {
            Debug.Log(path);

            BookEditor rcEditor = (BookEditor)EditorWindow.GetWindow(typeof(BookEditor));
            rcEditor.m_strBookPath = path;
            rcEditor.Show();
        }
    }
}
