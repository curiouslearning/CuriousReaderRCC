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

    /// <summary>
    /// Get the count of number of books that we have
    /// </summary>
    /// <returns>Number of current books</returns>
    public int GetTotalBookCount()
    {
        return m_bookInfos.Count;
    }

    /// <summary>
    /// Try to return cover with book index and language
    /// </summary>
    /// <param name="i_bookIndex">Index of the book that we need to get</param>
    /// <param name="i_language">Language of the book that we need to get the cover for</param>
    /// <returns></returns>
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

    /// <summary>
    /// Get book file name and asset bundle using the passed parameters
    /// </summary>
    /// <param name="i_bookIndex">Index of the book</param>
    /// <param name="i_language">Language of the book</param>
    /// <param name="i_bookLevel">Level of the book</param>
    /// <returns>Tuple that contains: book file name and book asset bundle</returns>
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