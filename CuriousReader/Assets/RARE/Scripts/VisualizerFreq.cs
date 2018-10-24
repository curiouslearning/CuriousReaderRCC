using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizerFreq : MonoBehaviour {

	private float[] freqBand = new float[16];
	private float[] audioLevels;
	private float[] bandBuffer = new float[16];
	private float[] bufferDecrease = new float[16];
	private bool[] justClipped = new bool[16];
	public GameObject[] warnings = new GameObject[16];
	public RectTransform[] rects = new RectTransform[16];
	public float maxHeight;
	public float sensitivity;


	// Update is called once per frame
	void Update () {
		audioLevels = RARE._Instance.GetAudioLevels ();
		MakeFrequencyBands ();
		BandBuffer ();
		PumpRects ();
	}

	void MakeFrequencyBands(){
		int count = 0;
		for (int i = 0; i < 8; i++) {
			float average1 = 0;
			float average2 = 0;
			int sampleCount = (int)((Mathf.Pow (2.0f, i) * (audioLevels.Length/512)));
			if (i == 7) {
				sampleCount += (audioLevels.Length/512);
			}
			for (int j = 0; j < sampleCount; j++) {
				average1 += (audioLevels [count] * 32767 *(count + 1))/10000.0f;
				count++;
			}
			average1 /= count;
			for (int j = 0; j < sampleCount; j++) {
				average2 += (audioLevels [count] * 32767 *(count + 1))/10000.0f;
				count++;
			}
			average2 /= count;
			freqBand [i * 2] = average1 * sensitivity;
			freqBand [i * 2 + 1] = average2 * sensitivity;
		}
	}

	void PumpRects(){
		for (int i = 0; i < rects.Length; i++) {
			if (bandBuffer[i] < 1.0f) {
				rects [i].localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			} else if (bandBuffer [i] > maxHeight) {
				rects [i].localScale = new Vector3 (1.0f, maxHeight, 1.0f);
				if (!justClipped [i]) {
					StartCoroutine (Clipping (i));
				}
			} else {
				//this is where the rectangle images are being scaled... the bandbuffer[] is the edited version of the audio to make it look nice
				//to get the raw output you should use freqBand[] instead, but bandBuffer[] is recommended 
				rects [i].localScale = new Vector3 (1.0f, bandBuffer[i], 1.0f);
			}
		}
	}

	IEnumerator Clipping(int i){
		warnings [i].SetActive (true);
		justClipped [i] = true;
		yield return new WaitForSeconds (2.0f);
		justClipped [i] = false;
		warnings [i].SetActive (false);
	}

	void BandBuffer(){
		for (int i = 0; i < freqBand.Length; ++i) {
			if (freqBand [i] > bandBuffer [i]) {
				bandBuffer [i] = freqBand [i];
				bufferDecrease [i] = 0.1f;
			} 
			if (freqBand [i] < bandBuffer [i]) {
				bandBuffer [i] -= bufferDecrease [i];
				bufferDecrease [i] *= 1.2f;
			} 
		}
	}

}