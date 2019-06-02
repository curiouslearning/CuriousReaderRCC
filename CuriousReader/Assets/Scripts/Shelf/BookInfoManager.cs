using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class BookInfoManager : MonoBehaviour 
{
    #region Private Members
    
    [SerializeField]
    private List<BookInfo>  m_bookInfos;
    
    #endregion

    #region Public Methods

    public int GetTotalBookCount()
    {
        return m_bookInfos.Count;
    }

    public Sprite GetBookCoverWithLanguage(int i_bookIndex, ReaderLanguage i_language)
    {
        try
        {
            BookCoverTranslation bookCoverTranslation = m_bookInfos[i_bookIndex].BookCover
                .Find((cover) => { return cover.BookLanguage == i_language;});
            return bookCoverTranslation.BookCoverSprite;
        } 
        catch
        {
            Debug.LogError($"Unable to get cover image for book with index {i_bookIndex} and language {i_language}.");
            return null;
        }
    }

    public (string, string) GetBookFileNameAndAssetBundle(int i_bookIndex, ReaderLanguage i_language, int i_bookLevel)
    {
        try 
        {
            string bookAssetBundle = m_bookInfos[i_bookIndex].BookAssetBundleName;
            string bookFileName = m_bookInfos[i_bookIndex].BookTitleTranslations.Find((bookTitleTranslation) =>
            {
                return bookTitleTranslation.BookLanguage == i_language && bookTitleTranslation.BookLevel == i_bookLevel;
            }).AssetBundleBookFileName;
            return (bookFileName, bookAssetBundle);
        } 
        catch (Exception e)
        {
            Debug.LogError($"Unable to find book details with book index: {i_bookIndex}, language: {i_language} and level: {i_bookLevel}. Message: {e.Message}");
            return (null, null);
        }
    }
    
    #endregion
}