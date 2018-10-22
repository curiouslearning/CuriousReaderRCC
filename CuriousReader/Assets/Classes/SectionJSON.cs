using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SectionJSON {
	public int sectionID;
	public List<InAppSection> IN_APP_SECTION;
	public List<InAppTouch> IN_APP_TOUCH;
	public List<InAppResponse> IN_APP_RESPONSE;

}
