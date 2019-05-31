using System;
using UnityEngine;

[Serializable]
public class BookTitleTranslation 
{
    [SerializeField]
    public int              BookLevel;
    [SerializeField]
    public ReaderLanguage   BookLanguage;
    [SerializeField]
    public string           BookFileName;
}