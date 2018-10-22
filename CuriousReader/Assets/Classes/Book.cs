using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Serializable objects

[System.Serializable]
public class Book{
	
	//only put properties that will be retrieved from manifest file!
	public string fileName, pathToThumbnail, title, author, language;
	public int bookId, version, readingLevel;
	public List<string> tags;
}
