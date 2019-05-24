using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
[System.Serializable]
public class BookInfo : ScriptableObject
{
    [SerializeField]
    public string                       BookAssetBundleName;
    [SerializeField]
    public List<BookCoverTranslation>   BookCover;
    [SerializeField]
    public List<BookTitleTranslation>   BookTitleTranslations;
}