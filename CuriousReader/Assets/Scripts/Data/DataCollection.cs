using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

/// <summary>
/// Data collection Script .
/// </summary>
//sending data directly to firebase using "72 hours rule"! (removed local data storage)
public class DataCollection : MonoBehaviour {

	private static string path;
	private string[] sectionsData;
	string[] wholeData;
	static JSONNode dataNode;
	public static string appID;
	public static string secID;

	public void Awake(){
		LoadLocalJSON();
		List<string> opt = new List<string> ();
		//SaveLocalJSON (dataNode);
		long val= CheckSize ();
	}

	/// <summary>
	/// Initialize the file for local JSON usage data storage.
	/// </summary>
	public void AddFile(){
		File.CreateText (Application.persistentDataPath + "/JSONData.json").Dispose();
		File.WriteAllText (Application.persistentDataPath + "/JSONData.json", "{\"tabletID\": {} }");	
	}

	public void AddNewBook(string name){
		appID = name;
		if (dataNode ["tabletID"] [name] == null) {
			JSONNode node = new JSONObject ();
			dataNode ["tabletID"].Add (name, node);
		}

	}

	public void AddNewSection(string appID,string sectionID){
		secID = sectionID;
		if (dataNode ["tabletID"] [appID] [sectionID] == null) {
		JSONNode node = new JSONObject ();
			dataNode ["tabletID"] [appID].Add (sectionID.ToString(), node);
	    }
	}

	/// <summary>
	/// Checks the size of the file.
	/// </summary>
	/// <returns>The size of file.</returns>
	public static long CheckSize(){
		var fileInfo = new System.IO.FileInfo (Application.persistentDataPath + "/JSONData.json");
		return fileInfo.Length;
	}

	/// <summary>
	/// Loads the dataNode with the file's current value.
	/// </summary>
	public void LoadLocalJSON()
	{
		string dataAsJSON;
		path = Application.persistentDataPath + "/JSONData.json";
		if (! File.Exists (Application.persistentDataPath + "/JSONData.json")) {
			AddFile ();
		} 
	    	dataAsJSON = File.ReadAllText (path);
			dataNode = JSON.Parse (dataAsJSON);
	}

	/// <summary>
	/// Saves the local JSON file.
	/// </summary>
	/// <param name="node">JSON Node with all the words.</param>
	public static void SaveLocalJSON(JSONNode node)
	{
		File.WriteAllText (Application.persistentDataPath + "/JSONData.json", node.ToString() );
	}

	/// <summary>
	/// Adds the IN_APP_SECTION event data to JSON node.
	/// </summary>
	/// <param name="inTime">Time of enter to the section.</param>
	/// <param name="timeSpent">Time spent.</param>
	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	public static void AddInSectionData( string inTime, string timeSpent){
		JSONNode node = new JSONObject();
		node ["inTime"] = inTime;
		node["timeSpent"] = timeSpent;
		Debug.Log ("tablet: "+appID+secID);
		dataNode["tabletID"][appID][secID]["IN_APP_SECTION"].Add(node);

		SaveLocalJSON (dataNode);
	}

	/// <summary>
	/// Adds the IN_APP_TOUCH event data to JSON node.
	/// </summary>
	/// <param name="label">Label of the object touched.</param>
	/// <param name="time">Time of touch.</param>
	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	public static void AddInTouchData( string label, string time){
		//type will be button, text or image
		JSONNode node = new JSONObject();
		node ["time"] = time;
		node["label"] = label;
		dataNode["tabletID"][appID][secID]["IN_APP_TOUCH"].Add(node);
		SaveLocalJSON (dataNode);

	}

	/// <summary>
	/// Adds IN_APP_RESPONSE event data to JSON Node.
	/// </summary>
	/// <param name="selection">Selection made by the user.</param>
	/// <param name="answer">Correct answer.</param>
	/// <param name="foilList">foil list with wrong options.</param>
	/// <param name="timeElapsed">Time elapsed before answering the question.</param>
	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	public static void AddInResponseData( string selection, string answer, List<string> foilList, string timeElapsed){
		//type will be button, text or image
		JSONNode node = new JSONObject();
		node ["selection"] = selection;
		node ["answer"] = answer;
		node ["timeTaken"] = timeElapsed;

		node ["foil"] = new JSONArray ();
		for (int i = 0; i < foilList.Count; i++) {
			node ["foil"].Add( foilList[i]);
		}

		dataNode["tabletID"][appID][secID]["IN_APP_RESPONSE"].Add(node);

	}

	/// <summary>
	/// Gets the path of LocalJSON file.
	/// </summary>
	/// <returns>The path.</returns>
	public static string GetPath(){
		return path;
	}
}