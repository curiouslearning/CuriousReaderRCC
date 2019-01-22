using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using Elendow.SpritedowAnimator;
using CuriousReader.Performance;

public class GTinkerGraphic : MonoBehaviour
{
	public GameObjectClass dataTinkerGraphic;

	public GTinkerText pairedText1;
	public GTinkerText pairedText2;
	public GSManager sceneManager;
	public Canvas myCanvas;
	public Sprite[] sprite;

	// Reset and highlight colors defaults (change from scene manager subclasses)
	public Color resetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color highlightColor = GGameManager.yellow;
    public int m_nNavigationPage = -1;

    /// <summary>
    /// set the draggable property for that tinkergraphic
    /// </summary>
    /// <param name="value">bool value to be set</param>
    public void SetDraggable(bool value)
    {
		dataTinkerGraphic.draggable = value;
	}
    
    /// <summary>
    /// return whether the graphic is draggable or not 
    /// </summary>
    /// <returns></returns>
	public bool GetDraggable()
    {
		return dataTinkerGraphic.draggable;
	}

    /// <summary>
    /// this handles the mouse down events on the tinkergraphic 
    /// playes the animation for that graphic so it is attached
    /// also handles the animating of the text paired to that graphic
    /// </summary>
	public void MyOnMouseDown()
	{
		System.DateTime time=  System.DateTime.Now;
        //sending data directly to firebase using "72 hours rule"! (removed local data storage)
        //DataCollection.AddInTouchData (("Graphic_"+dataTinkerGraphic.label),  time.ToString());
        
		FirebaseHelper.LogInAppTouch(("Graphic_"+dataTinkerGraphic.label) ,  time.ToString());
        PerformanceSystem.SendPrompt(this.gameObject, this.gameObject, PromptType.Click);
        sceneManager.OnMouseDown (this);
    }

        /// <summary>
    /// this function is called when the tinkergraphic is paired to some tinkertext 
    /// and we want have some paired graphical animation for that text
    /// </summary>
    /// <param name="tinkerText"></param>
    // Paired TinkerText Mouse Down Event
    public void OnPairedMouseDown(GTinkerText tinkerText)
	{
		sceneManager.OnPairedMouseDown(tinkerText);
        PerformanceSystem.SendPrompt(tinkerText.gameObject, this.gameObject, PromptType.PairedClick);
    }

    /// <summary>
    /// Raises the paired mouse down event for tinkergraphic.
    /// </summary>
    /// <param name="tinkerGraphic">Tinker graphic.</param>
    public void OnPairedMouseDown(GTinkerGraphic tinkerGraphic)
	{
        sceneManager.OnMouseDown (this);
	} 

	// Mouse Currently Down Event
	public void OnMouseCurrentlyDown()
	{
		sceneManager.OnMouseCurrentlyDown(this);
	}

	// Paired TinkerText Mouse Down Event
	public void OnPairedMouseCurrentlyDown(GTinkerText tinkerText)
	{
		sceneManager.OnPairedMouseCurrentlyDown(tinkerText);
	}

	// Mouse Up Event
	public void MyOnMouseUp()
	{
		sceneManager.OnMouseUp(this);
	}

	// Paired TinkerText Mouse Up Event
	public void OnPairedMouseUp(GTinkerText tinkerText)
	{
		sceneManager.OnPairedMouseUp(tinkerText);
	}

	public void MoveObject()
    {
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
		transform.position = myCanvas.transform.TransformPoint(pos);
	}

	public Vector2 GetCoordinates()
    {
		return transform.position;
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		if(col.gameObject.name == dataTinkerGraphic.destroyOnCollision)
		{    
//         Use the new extension Invoke method here.
//			destroyObject = StartCoroutine(DestroyCollisionObject (col.gameObject));
		}
	}

}
