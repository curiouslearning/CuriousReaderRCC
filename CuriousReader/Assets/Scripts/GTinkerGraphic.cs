using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GTinkerGraphic : MonoBehaviour{
	public GameObjectClass dataTinkerGraphic;

	// private Animator anim;
	public int size;
	public GTinkerText pairedText1;
	public GTinkerText pairedText2;
	public GSManager sceneManager;
	public Canvas myCanvas;
	private int framecount = 30;
	private float initial_scale_x;
	private float initial_scale_y;
	private float totalchange = 1.33f;
	private float deltasize;
	private int totaltime = 1;
	private float deltatime;
	private float final_scale_x;
	private float final_scale_y;
	public Sprite[] sprite;
	private int currentframe=0;
	public SpriteRenderer spr;
    public bool forcestop = false;
    public bool isanimplaying = false;
	public Sequence[] sequences;
	private int seqIterator;
	public float[] secPerFrame;
    private Coroutine par_routine;
    private Coroutine current_routine;
	// Reset and highlight colors defaults (change from scene manager subclasses)
	public Color resetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color highlightColor = GGameManager.yellow;
	private Coroutine destroyObject;
    /// <summary>
    /// fetch the sprite renderer attached to given graphical object 
    /// </summary>
	private void Awake()
	{
		spr = GetComponent<SpriteRenderer>();
	}
	private void Start()
	{
		initial_scale_x = this.transform.localScale.x;
		initial_scale_y = this.transform.localScale.y;
		final_scale_x = initial_scale_x * totalchange;
		final_scale_y = initial_scale_y * totalchange;
		deltasize = (final_scale_x - initial_scale_x) / framecount;
		deltatime = totaltime / framecount;
	}
    private void Update()
    {
        if(forcestop)
        {
            if (isanimplaying)

            {
                checkandStoproutines();
                isanimplaying = false;
            }
            forcestop = false;
        }
    }
    /// <summary>
    /// set the draggable property for that tinkergraphic
    /// </summary>
    /// <param name="value">bool value to be set</param>
	public void SetDraggable(bool value){
		dataTinkerGraphic.draggable = value;
	}
    /// <summary>
    /// return whether the graphic is draggable or not 
    /// </summary>
    /// <returns></returns>
	public bool GetDraggable(){
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
		int length = dataTinkerGraphic.anim.Length;
        //for (int i = 0; i < length; i++) {
        //Debug.Log ("anim_no "+ i);
        //LoadAndPlayAnimation (i);
        //StartCoroutine (animdelay());
        //}
        PlayCompleteAnim();
		sceneManager.OnMouseDown (this);
}
	public void PlayCompleteAnim()
	{
        checkandStoproutines();
        isanimplaying = true;
		par_routine=StartCoroutine(Animdelay());
	}
	public IEnumerator Animdelay()
	{
        float sum;
		for (int i = 0; i < dataTinkerGraphic.anim.Length; i++) { 
            sum = 0f;
			LoadAndPlayAnimation(i);
			float[] sec = dataTinkerGraphic.anim [i].secPerFrame;
			for (int t = 0; t < sec.Length; t++) 
			{
				sum = sum + sec[t];
			}
			yield return new WaitForSeconds(sum);
		}
	}
    public void checkandStoproutines()
    {
        if (par_routine != null)
        {
            StopCoroutine(par_routine);
            par_routine = null;
        }
        if (current_routine != null)
        {
            StopCoroutine(current_routine);
            current_routine = null;
        }
        if (destroyObject != null)
        {
            StopCoroutine(destroyObject);
            destroyObject = null;
        }

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
        isanimplaying = true;
        checkandStoproutines();
        LoadAndPlayAnimation(tinkerText.pairedAnim);
		 

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


	public void MoveObject(){
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
		transform.position = myCanvas.transform.TransformPoint(pos);
	}

	public Vector2 GetCoordinates(){
		return transform.position;
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		if(col.gameObject.name == dataTinkerGraphic.destroyOnCollision)
		{    
			
			destroyObject = StartCoroutine(DestroyCollisionObject (col.gameObject));
		}
	}

	public IEnumerator DestroyCollisionObject (GameObject go)
	{       //for (currentframe = sequences [seqIterator].startFrame; currentframe <= sequences [seqIterator].endFrame; currentframe++) {
			//spr.sprite = sprite [currentframe];
			//yield return new WaitForSeconds (secPerFrame [currentframe]);
		//}
			//yield return new WaitForSeconds (secPerFrame[currentframe]+secPerFrame[currentframe+1]+secPerFrame[currentframe+2]+secPerFrame[currentframe + 3]);
		yield return new WaitForSecondsRealtime (1.0f);
		
		go.SetActive (false);
	}

	/// <summary>
	/// Loads the animation assets/frames and triggers PlayAnimation().
	/// </summary>
	public void LoadAndPlayAnimation(int pairedAnim){
		
		if (dataTinkerGraphic.anim.Length > 0) {

			//if (dataTinkerGraphic.anim [pairedAnim].onTouch) 
			{
				
                LoadAssetFromJSON.LoadAssetImages(this, dataTinkerGraphic.anim[pairedAnim].animName, dataTinkerGraphic.anim[pairedAnim].startIndex,dataTinkerGraphic.anim[pairedAnim].endIndex,dataTinkerGraphic.anim[pairedAnim].startX,dataTinkerGraphic.anim[pairedAnim].startY, dataTinkerGraphic.anim[pairedAnim].startZ);
				secPerFrame = dataTinkerGraphic.anim [pairedAnim].secPerFrame;
				sequences = dataTinkerGraphic.anim [pairedAnim].sequences;
				PlayAnimation();

			} 

		}

	}

	public void ResetandZoom()
	{
        forcestop = true;
        this.gameObject.transform.position = new Vector3 (dataTinkerGraphic.posX,dataTinkerGraphic.posY,dataTinkerGraphic.posZ);
		LoadAssetFromJSON.LoadAssetImage (this,dataTinkerGraphic.imageName);
		StartCoroutine (Zoom());
        
	}
	/// <summary>
	/// Resets the graphic object and triggers the animation play.
	/// </summary>
      public void PlayAnimation(){
        //StopAllCoroutines()
        if(current_routine!=null)
        {
            StopCoroutine(current_routine);
        }
		if (destroyObject != null) {
			StopCoroutine (destroyObject);
		}
		//transform.position = new Vector3 (dataTinkerGraphic.posX, dataTinkerGraphic.posY);
		current_routine=StartCoroutine("Animate");
	}
    public void reset()
    {
        forcestop = true;
        this.gameObject.transform.position = new Vector3(dataTinkerGraphic.posX, dataTinkerGraphic.posY, dataTinkerGraphic.posZ);
        LoadAssetFromJSON.LoadAssetImage(this, dataTinkerGraphic.imageName);
    }

    /// <summary>
    /// Animate this instance with loaded animation frames.
    /// </summary>
    IEnumerator Animate()
	{  
		currentframe = 0;
		int i = 1;

		for (seqIterator = 0; seqIterator < sequences.Length; seqIterator++) {
			
			//animate for non moving sequences of PNGs
			if (sequences [seqIterator].movable.speed == 0 ) {
				i = 1;       //count the number of loops from start for every sequence!
				while (i <= sequences [seqIterator].noOfLoops) {
					for (currentframe = sequences [seqIterator].startFrame; currentframe <= sequences [seqIterator].endFrame; currentframe++) {
						spr.sprite = sprite [currentframe];
						yield return new WaitForSeconds (secPerFrame [currentframe]);
					}
					i++;

				}
			}
			//animate for moving sequences of PNGs towards right.
			else if(transform.position.x < sequences [seqIterator].movable.finalx) {
				
				currentframe = sequences [seqIterator].startFrame;
				while (transform.position.x < sequences [seqIterator].movable.finalx) {
					spr.sprite = sprite[currentframe];
					yield return new WaitForSeconds(secPerFrame[currentframe]);
					currentframe++;
					var posx = transform.position.x;
					posx += sequences [seqIterator].movable.speed;
					transform.position = new Vector3(posx, this.transform.position.y, 0);

					//loop if we reached the end frame but not the final position!
					if (currentframe > sequences [seqIterator].endFrame)
					{
						currentframe = sequences [seqIterator].startFrame;
					} 

				}

				spr.sprite = sprite[sequences [seqIterator].endFrame];
			}
			//animate for moving sequences of PNGs towards left.
			else if(transform.position.x > sequences [seqIterator].movable.finalx) 
			{   
				currentframe = sequences [seqIterator].startFrame;

				while (transform.position.x > sequences [seqIterator].movable.finalx) {
					spr.sprite = sprite[currentframe];

					yield return new WaitForSeconds(secPerFrame[currentframe]);
					currentframe++;
					var posx = transform.position.x;
					posx += sequences [seqIterator].movable.speed;
					transform.position = new Vector3(posx, this.transform.position.y, 0);

					//loop if we reached the end frame but not the final position!
					if (currentframe > sequences [seqIterator].endFrame)
					{
						currentframe = sequences [seqIterator].startFrame;
					} 

				}

				spr.sprite = sprite[sequences [seqIterator].endFrame];
			}


		}
		Destroy (spr.gameObject.GetComponent<PolygonCollider2D> ());
		PolygonCollider2D col = spr.gameObject.AddComponent<PolygonCollider2D>();
		col.isTrigger = true;
	
		yield break;

	}
	public IEnumerator Zoom()
	{
		while(this.transform.localScale.x<=final_scale_x && this.transform.localScale.y<=final_scale_y)
		{
			Vector3 temp = this.transform.localScale;
			temp.x += deltasize;
			temp.y += deltasize;
			this.transform.localScale = temp;
			yield return new WaitForSeconds(deltatime/2);
		}
		while (this.transform.localScale.x >= initial_scale_x && this.transform.localScale.y >= initial_scale_y)
		{
			Vector3 temp = this.transform.localScale;
			temp.x -= deltasize;
			temp.y -= deltasize;
			this.transform.localScale = temp;
			yield return new WaitForSeconds(deltatime/2);
		}
	}
}
