using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Audio;
using System.IO;
using System;
using Unity.VectorGraphics;

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

    VectorUtils.TessellationOptions tessOptions = new VectorUtils.TessellationOptions()
    {
        StepDistance = 100.0f,
        MaxCordDeviation = 0.5f,
        MaxTanAngleDeviation = 0.1f,
        SamplingStepSize = 0.01f
    };

    private void OnGUI()
    {
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

        GUILayout.BeginHorizontal();
        GUILayout.Label("Book Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Images To AssetBundle"))
        {
            Debug.Log(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strAnimPath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
            AddImagesInPath(m_strImagePath.Replace(m_strAssetPath.Replace("/", "\\"), "").Replace("\\Assets\\", "Assets\\"));
        }
        GUILayout.EndHorizontal();


        if (!string.IsNullOrEmpty(m_strBookPath)) GUILayout.Label(m_strBookPath);
        if (!string.IsNullOrEmpty(m_strCommonPath)) GUILayout.Label(m_strCommonPath);
        if (!string.IsNullOrEmpty(m_strAnimPath)) GUILayout.Label(m_strAnimPath);
        if (!string.IsNullOrEmpty(m_strAssetPath)) GUILayout.Label(m_strAssetPath);

        m_vScrollPosition = GUILayout.BeginScrollView(m_vScrollPosition);

        m_rcStoryBook.id = EditorGUILayout.IntField("id", m_rcStoryBook.id, EditorStyles.numberField);
        m_rcStoryBook.language = EditorGUILayout.TextField("Language", m_rcStoryBook.language, EditorStyles.textField);
        m_rcStoryBook.fontFamily = EditorGUILayout.TextField("Font Family", m_rcStoryBook.fontFamily, EditorStyles.textField);
        m_rcStoryBook.fontColor = EditorGUILayout.TextField("Font Family", m_rcStoryBook.fontColor, EditorStyles.textField);
        m_rcStoryBook.textFontSize = EditorGUILayout.IntField("Font Size", m_rcStoryBook.textFontSize, EditorStyles.numberField);
        m_rcStoryBook.textStartPositionX = EditorGUILayout.FloatField("Text Start X", m_rcStoryBook.textStartPositionX, EditorStyles.numberField);
        m_rcStoryBook.textStartPositionY = EditorGUILayout.FloatField("Text Start Y", m_rcStoryBook.textStartPositionY, EditorStyles.numberField);

        int nRemove = -1;

        for (int i = 0; i < m_rcStoryBook.pages.Length; i++)
        {
            // for statement here

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditPage(m_rcStoryBook.pages[i], i);

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
            m_rcStoryBook.pages = m_rcStoryBook.pages.RemoveAt(nRemove);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
        if (GUILayout.Button("Add Page"))
        {
            PageClass[] rcNewArray = new PageClass[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcStoryBook.pages, rcNewArray, m_rcStoryBook.pages.Length);

            rcNewArray[m_rcStoryBook.pages.Length] = new PageClass();
            rcNewArray[m_rcStoryBook.pages.Length].gameObjects = new GameObjectClass[0];
            rcNewArray[m_rcStoryBook.pages.Length].triggers = new TriggerClass[0];
            rcNewArray[m_rcStoryBook.pages.Length].timestamps = new TimeStampClass[0];
            rcNewArray[m_rcStoryBook.pages.Length].texts = new TextClass[0];
            rcNewArray[m_rcStoryBook.pages.Length].audio = new AudioClass();
            rcNewArray[m_rcStoryBook.pages.Length].pageNumber = m_rcStoryBook.pages.Length;
            rcNewArray[m_rcStoryBook.pages.Length].script = "GSManager";

            Rect[] NewAudioRects = new Rect[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcAudioRects, NewAudioRects, m_rcStoryBook.pages.Length);

            AudioClip[] NewAudioClips = new AudioClip[m_rcStoryBook.pages.Length + 1];
            Array.Copy(m_rcPageAudio, NewAudioClips, m_rcStoryBook.pages.Length);

            m_rcPageAudio = NewAudioClips;
            m_rcAudioRects = NewAudioRects;
            m_rcStoryBook.pages = rcNewArray;

        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Save JSON"))
        {
            Debug.Log(m_strBookPath + " file to be saved.");

            StreamWriter rcWriter = new StreamWriter(m_strBookPath, false);
            rcWriter.Write(JsonUtility.ToJson(m_rcStoryBook, true));
            rcWriter.Close();
        }

        GUILayout.EndScrollView();
    }

    private void EditPage(PageClass i_rcPage, int i_nOrdinal)
    {
        if (i_rcPage.texts != null)
        {
            if ( i_rcPage.texts[0] != null )
            {
                i_rcPage.Show = EditorGUILayout.Foldout(i_rcPage.Show, new GUIContent("Page " + i_rcPage.pageNumber + " - " + i_rcPage.texts[0].text ));
            }
        }
        else
        {
            i_rcPage.Show = EditorGUILayout.Foldout(i_rcPage.Show, new GUIContent("Page " + i_rcPage.pageNumber));
        }
        EditorGUI.indentLevel++;

        if (i_rcPage.Show)
        {
            i_rcPage.pageNumber = EditorGUILayout.IntField("Page Number", i_rcPage.pageNumber, EditorStyles.numberField);
            i_rcPage.script = EditorGUILayout.TextField("Script", i_rcPage.script, EditorStyles.textField);
            i_rcPage.audioFile = EditorGUILayout.TextField("Audio File", i_rcPage.audioFile, EditorStyles.textField);

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            m_bMicrophoneHot = GUILayout.Button("Record");

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
            GUILayout.EndHorizontal();

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
                    if (GUILayout.Button("Play"))
                    {
                        AudioUtility.PlayClip(m_rcPageAudio[i_nOrdinal], 0);
                    }

                    GUILayout.EndHorizontal();
                }

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
                }

                if (m_rcPageAudio[i_nOrdinal] != null)
                {
                    DrawTextOnAudioClip(m_rcPageAudio[i_nOrdinal], m_rcAudioRects[i_nOrdinal], i_rcPage.timestamps, i_rcPage.texts);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
                if (GUILayout.Button("Generate Timestamps"))
                {
                    List<string> rcWords = GetTextWords(i_rcPage.texts);

                    TimeStampClass[] rcNewArray = new TimeStampClass[rcWords.Count];

                    for (int i = 0; i < rcWords.Count; i++)
                    {
                        rcNewArray[i] = new TimeStampClass();
                        rcNewArray[i].start = i * Convert.ToInt32(m_rcPageAudio[i_nOrdinal].length * 1000) / rcWords.Count;
                        rcNewArray[i].end = rcNewArray[i].start + Convert.ToInt32(m_rcPageAudio[i_nOrdinal].length * 1000) / rcWords.Count;
                        rcNewArray[i].starWord = "No";
                        rcNewArray[i].wordIdx = i;
                    }

                    i_rcPage.timestamps = rcNewArray;
                }
                GUILayout.EndHorizontal();
            }

            int nRemove = -1;

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
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add GameObject"))
            {
                GameObjectClass[] rcNewArray = new GameObjectClass[i_rcPage.gameObjects.Length + 1];
                Array.Copy(i_rcPage.gameObjects, rcNewArray, i_rcPage.gameObjects.Length);
                rcNewArray[i_rcPage.gameObjects.Length] = new GameObjectClass();
                rcNewArray[i_rcPage.gameObjects.Length].anim = new Anim[0];
                rcNewArray[i_rcPage.gameObjects.Length].draggable = false;
                rcNewArray[i_rcPage.gameObjects.Length].id = i_rcPage.gameObjects.Length;
                i_rcPage.gameObjects = rcNewArray;
            }
            GUILayout.EndHorizontal();

            nRemove = -1;

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
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add Text"))
            {
                TextClass[] rcNewArray = new TextClass[i_rcPage.texts.Length + 1];
                Array.Copy(i_rcPage.texts, rcNewArray, i_rcPage.texts.Length);
                rcNewArray[i_rcPage.texts.Length] = new TextClass();
                rcNewArray[i_rcPage.texts.Length].id = i_rcPage.texts.Length;
                rcNewArray[i_rcPage.texts.Length].text = "";

                i_rcPage.texts = rcNewArray;
            }
            GUILayout.EndHorizontal();

            nRemove = -1;

            for (int j = 0; j < i_rcPage.triggers.Length; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                EditTriggers(i_rcPage.triggers[j], j);

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
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
            if (GUILayout.Button("Add Trigger"))
            {
                TriggerClass[] rcNewArray = new TriggerClass[i_rcPage.triggers.Length + 1];
                Array.Copy(i_rcPage.triggers, rcNewArray, i_rcPage.triggers.Length);
                rcNewArray[i_rcPage.triggers.Length] = new TriggerClass();
                i_rcPage.triggers = rcNewArray;
            }
            GUILayout.EndHorizontal();


        }

        EditorGUI.indentLevel--;
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

    private void EditTriggers(TriggerClass i_rcTrigger, int i_nOrdinal)
    {
        i_rcTrigger.Show = EditorGUILayout.Foldout(i_rcTrigger.Show, "Trigger " + i_nOrdinal);

        EditorGUI.indentLevel++;

        if (i_rcTrigger.Show)
        {
            i_rcTrigger.animId = EditorGUILayout.IntField("Anim ID", i_rcTrigger.animId, EditorStyles.numberField);
            i_rcTrigger.stanzaID = EditorGUILayout.IntField("Stanza ID", i_rcTrigger.stanzaID, EditorStyles.numberField);
            i_rcTrigger.textId = EditorGUILayout.IntField("Text ID", i_rcTrigger.textId, EditorStyles.numberField);
            i_rcTrigger.timestamp = EditorGUILayout.IntField("Timestamp", i_rcTrigger.timestamp, EditorStyles.numberField);
            i_rcTrigger.sceneObjectId = EditorGUILayout.IntField("SceneObject ID", i_rcTrigger.sceneObjectId, EditorStyles.numberField);
            i_rcTrigger.typeOfLinking = EditorGUILayout.IntField("Type of Linking", i_rcTrigger.typeOfLinking, EditorStyles.numberField);
        }

        EditorGUI.indentLevel--;
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
            i_rcGameObject.scaleX = EditorGUILayout.FloatField("Scale X", i_rcGameObject.scaleX, EditorStyles.numberField);
            i_rcGameObject.scaleY = EditorGUILayout.FloatField("Scale Y", i_rcGameObject.scaleY, EditorStyles.numberField);
            i_rcGameObject.orderInLayer = EditorGUILayout.IntField("Order In Layer", i_rcGameObject.orderInLayer, EditorStyles.numberField);
            i_rcGameObject.inText = EditorGUILayout.ToggleLeft("In Text", i_rcGameObject.inText, EditorStyles.textField);
            i_rcGameObject.draggable = EditorGUILayout.ToggleLeft("Draggable?", i_rcGameObject.draggable, EditorStyles.textField);

            ///

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
    [MenuItem("Window/Book Editor")]
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
