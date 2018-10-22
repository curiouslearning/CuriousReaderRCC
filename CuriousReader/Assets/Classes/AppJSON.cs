using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppJSON {
	public int sectionID;
	public InAppSection[] IN_APP_SECTION;
	public InAppTouch[] IN_APP_TOUCH;
	public InAppResponse[] IN_APP_RESPONSE;
}
