using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using UnityEngine.Audio;

public class RARE : MonoBehaviour {

	private static int headerSize = 44; //default for uncompressed wav
	private bool recOutput;
	//public reference to volume slider
	private Queue<float> dataQ = new Queue<float>();
	private float outputVol = 0.65f;
	private bool micActive = false;
	private bool audioListenerActive = false;
	private GameObject microphoneGO;
	private float[] audioLevels = new float[2048];
	private AudioSource recSource;
	private bool micPlayBack = false;

	public static RARE _Instance;

	public static RARE Instance {
		get {
			return _Instance;
		}
	}

	private void Awake(){
		if (_Instance == null) {
			_Instance = this;
		}

		//uncomment this line if you want RARE to be active accross multiple scenes, or you can just add RARE as a component to each scene you use
		//DontDestroyOnLoad (this.gameObject); 

		//prepares microphone game object
		microphoneGO = new GameObject();
		microphoneGO.name = "Microphone";
		recSource = microphoneGO.AddComponent<AudioSource>();
		recSource.loop = true;
	}

	//*********************************************************************************************
	//  MICROPHONE RECORDING FUNCTIONS START >>>>>>>> MICROPHONE RECORDING FUNCTIONS START>>>>>>>
	//*********************************************************************************************

	public void StartMicRecording(int maxLengthInSeconds = 300){
		if (!micActive) {
			micActive = true;
			AudioSource aud = microphoneGO.GetComponent<AudioSource> ();
			int minFreq, maxFreq;
			Microphone.GetDeviceCaps ("", out minFreq, out maxFreq);
			if (maxFreq > 0) {
				aud.clip = Microphone.Start ("", false, maxLengthInSeconds, maxFreq);
			} else {
				aud.clip = Microphone.Start ("", false, maxLengthInSeconds, AudioSettings.outputSampleRate);
			}
			if (micPlayBack) {
				while (!(Microphone.GetPosition (null) > 0)) {}
				recSource.Play ();
			}
		} else {
			Debug.Log ("Cannot record multiple things at once, stop current recording to try again");
		}
	}

	public void StopMicRecording(string fileName, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null){
		if (micActive) {
			if (fileName.Length > 0) {
				//STOP RECORDING
				micActive = false;
				if (popUp != null) {
					popUp.SetActive (true);
				}
				int timeSinceStart = Microphone.GetPosition(""); 
				if (timeSinceStart == 0) {
					Debug.Log ("Recording length = 0? -> not a long enough recording to process");
					return;
				}
				AudioSource aud = microphoneGO.GetComponent<AudioSource>();
				Microphone.End("");
				float[] recordedClip = new float[aud.clip.samples*aud.clip.channels];
				aud.clip.GetData(recordedClip, 0);
				//shortenedClip is the recorded audio with the extra silence trimmed off
				float[] shortenedClip = new float[timeSinceStart]; 
				Array.Copy(recordedClip, shortenedClip, shortenedClip.Length-1);
				//put audio in dataQ so ListenerRecordStop() can use it
				FileStream fileStream;
				fileStream = new FileStream(Application.persistentDataPath +"/"+ fileName + ".wav", FileMode.Create);
				byte emptyByte = new byte();
				for(int i = 0; i<headerSize; i++) //preparing the header 
				{
					fileStream.WriteByte(emptyByte);
				}
				int outputRate = aud.clip.frequency;
				for ( int i = 0; i < shortenedClip.Length ; i++){
					float temp = shortenedClip [i] * outputRate * outputVol;
					if (temp >= Int16.MinValue && temp <= Int16.MaxValue) {
						byte[] temp2 = BitConverter.GetBytes (Convert.ToInt16 (temp));
						fileStream.Write (temp2, 0, temp2.Length);
					}
				}
				FinishWritingFile (fileStream, outputRate,1);
				if (popUp != null) {
					popUp.SetActive (false);
				}
				if (callBackFunction != null) {
					StartCoroutine (LoadClip (fileName, callBackFunction));
				}
			} else {
				Debug.Log ("StopMicRecording requires fileName length greater than 0");
			}
		} else {
			Debug.Log ("Microphone recording cannot be stopped if it hasn't started yet.");
		}
	}

	//****************************************************
	//  MICROPHONE RECORDING FUNCTIONS END <<<<<<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	//  AUDIOLISTENER RECORDING FUNCTIONS START >>>>> AUDIOLISTENER RECORDING FUNCTIONS START >>>>
	//*********************************************************************************************

	public void StartAudioListenerRecording(){
		if (!audioListenerActive) {
			audioListenerActive = true;
			recOutput = true;
		} else {
			Debug.Log ("Cannot record multiple things at once, stop current recording to try again");
		}
	}

	public void StopAudioListenerRecording(string fileName, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null){
		if (audioListenerActive) {
			if (fileName.Length > 0) {
				audioListenerActive = false;
				if (popUp != null) {
					popUp.SetActive (true);
				}
				recOutput = false;
				FileStream fileStream;
				fileStream = new FileStream(Application.persistentDataPath +"/"+ fileName + ".wav", FileMode.Create);
				byte emptyByte = new byte();
				for(int i = 0; i<headerSize; i++) //preparing the header 
				{
					fileStream.WriteByte(emptyByte);
				}
				while(dataQ.Count > 0) {
					float temp = dataQ.Dequeue ();
					if (temp >= Int16.MinValue && temp <= Int16.MaxValue) {
						byte[] temp2 = BitConverter.GetBytes (Convert.ToInt16 (temp));
						fileStream.Write (temp2, 0, temp2.Length);
					}
				}
				FinishWritingFile (fileStream, AudioSettings.outputSampleRate,2);
				dataQ.Clear ();
				if (popUp != null) {
					popUp.SetActive (false);
				}
				if (callBackFunction != null) {
					StartCoroutine (LoadClip (fileName, callBackFunction));
				}
			} else {
				Debug.Log ("StopAudioRecording requires fileName length greater than 0");
			}
		} else {
			Debug.Log ("AudioListener recording cannot be stopped if it hasn't started yet.");
		}
	}

	/*void FixedUpdafte(){
		if (micActive) {
		    recSource.GetSpectrumData (audioLevels, 2, FFTWindow.BlackmanHarris);
		}
	}*/

	public void OnAudioFilterRead(float[] data, int channels){ //this function runs repeatedly during audiolistener recording
		//all the audio data is stuffed into a queue to avoid latency 
		//and then pulled out and processed in writeheader() after the recording has been stopped
		//if (!micActive) {
		audioLevels = data;
		//}
		if(recOutput) { 
			for (int i = 0; i < data.Length; i++) { 			
				dataQ.Enqueue(data [i] * 32767 * outputVol);
			}   
		}
	}

	//****************************************************
	//  AUDIOLISTENER RECORDING FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	//  EXPORT FUNCTIONS START >>>>> EXPORT FUNCTIONS START >>>>
	//*********************************************************************************************

	public static void ExportClip(string fileName, AudioClip myClip, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null)
    {
		if (popUp != null) {
			popUp.SetActive (true);
		}
		//get audio data stream
		float[] data = new float[myClip.samples*myClip.channels];
		myClip.GetData (data, 0);
		//get correct sample rate-----
		FileStream fileStream;
		fileStream = new FileStream(/*Application.persistentDataPath +"/"+*/ fileName + ".wav", FileMode.Create);
		byte emptyByte = new byte();
		for(int i = 0; i<headerSize; i++) { //preparing the header
			fileStream.WriteByte(emptyByte);
		}
		for ( int i = 0; i < data.Length ; i++) {
			float temp = data [i] * 32767;
			if (temp >= Int16.MinValue && temp <= Int16.MaxValue) {
				byte[] temp2 = BitConverter.GetBytes (Convert.ToInt16 (temp));
				fileStream.Write (temp2, 0, temp2.Length);
			}
		}
		FinishWritingFile (fileStream, myClip.frequency, myClip.channels);
		if (popUp != null) {
			popUp.SetActive (false);
		}
	}

	//****************************************************
	//  EXPORT FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// GET AUDIOCLIP FUNCTIONS START >>>>> 
	//*********************************************************************************************

	public void GetAudioClipFromFile(string fileName, Action<AudioClip, string> callBackFunction, string fileType = "wav") {
		if(fileType.Equals("wav")||fileType.Equals("mp3")||fileType.Equals("ogg")){
			StartCoroutine (LoadClip (fileName, callBackFunction, fileType));
		}
	}

	IEnumerator LoadClip(string fileName, Action<AudioClip, string> callBackFunction, string fileType = "wav") {
		/* old code
        WWW www;
		www = new WWW (Application.persistentDataPath + fileName + "." + fileType);
        yield return www;
		AudioClip myClip = www.GetAudioClip (false, false);*/
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + Application.persistentDataPath +"/"+ fileName + "." + fileType, AudioType.WAV)) {
			yield return www.Send();
			if (www.isNetworkError) {
				Debug.Log(www.error);
			} else {
				callBackFunction(DownloadHandlerAudioClip.GetContent(www),fileName);
			}
		}     
	}

	public void GetAudioClipFromURL(string urlPath, Action<AudioClip, string> callBackFunction){
		StartCoroutine (LoadClipUrl (urlPath, callBackFunction));
	}

	IEnumerator LoadClipUrl(string urlPath, Action<AudioClip, string> callBackFunction){
		WWW www;
		www = new WWW (urlPath);
		yield return www;
		AudioClip myClip = www.GetAudioClip (false, false);
		callBackFunction (myClip, urlPath);
	}

	public AudioClip MakeAudioClipMono(AudioClip myClip) {
		Debug.Log(myClip.channels);
		float[] recordedClip = new float[myClip.samples ];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the samples not apart of channels

		List<float> shortenedClip = new List<float>(0);
		// Debug.Log(shortenedClip.Length + "  "  + recordedClip.Length);
		//if clip returns no remainder when divided by the number of channels, which is then asigned to the shorted array to make the clip mono

		for (int i = 0 ; i < recordedClip.Length; i++){
			if (i % myClip.channels == 0) {
				// Debug.Log("auder");\
				shortenedClip.Add ( recordedClip[i]);
			}
		}

		// Array.Copy(recordedClip, shortenedClip, 0, shortenedClip.Length - 1);

		AudioClip newClip = AudioClip.Create("trimmedMonoClip", shortenedClip.ToArray().Length, 1, myClip.frequency, false);
		newClip.SetData(shortenedClip.ToArray(), 0);
		return newClip;
	}

	//****************************************************
	//  GET AUDIOCLIP FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// TRIM AUDIOCLIP FUNCTIONS START >>>>> 
	//*********************************************************************************************

	public void CropAudioClip(string fileName, int frontIndex, int endIndex, AudioClip myClip, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null) {
		myClip = TrimEndOfAudioClip(myClip, endIndex);
		myClip = TrimFrontOfAudioClip(myClip, frontIndex);
		ExportClip (fileName, myClip, callBackFunction, popUp);
	}

	public AudioClip TrimFrontOfAudioClip(AudioClip myClip, int frontIndex) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the front trimmed off
		float[] shortenedClip = new float[(myClip.samples * myClip.channels) - frontIndex];
		Array.Copy(recordedClip, frontIndex, shortenedClip, 0, shortenedClip.Length - 1);
		AudioClip newClip = AudioClip.Create (myClip.name, shortenedClip.Length / myClip.channels, myClip.channels, myClip.frequency, false);
		newClip.SetData(shortenedClip, 0);
		return newClip;
	}

	public static AudioClip TrimEndOfAudioClip(AudioClip myClip, int endIndex) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the end trimmed off
		float[] shortenedClip = new float[endIndex];
		Array.Copy(recordedClip, shortenedClip, shortenedClip.Length - 1);
		AudioClip newClip = AudioClip.Create (myClip.name, shortenedClip.Length / myClip.channels, myClip.channels, myClip.frequency, false);
		newClip.SetData(shortenedClip, 0);
		return newClip;
	}

	public KeyValuePair<AudioClip, AudioClip> SplitAudioClip(AudioClip myClip, int splitIndex) {
		AudioClip startClip = TrimEndOfAudioClip(myClip, splitIndex);
		AudioClip endClip = TrimFrontOfAudioClip(myClip, splitIndex); 
		return new KeyValuePair<AudioClip, AudioClip>(myClip, myClip);
	}

	//****************************************************
	//  TRIM AUDIOCLIP FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// OTHER FUNCTIONS AND HELPER FUNCTIONS START >>>>> 
	//*********************************************************************************************

	private static void FinishWritingFile(FileStream fileStream, int outputRate, int channels){
		fileStream.Seek(0,SeekOrigin.Begin);
		byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff,0,4);
		byte[]  chunkSize = BitConverter.GetBytes(fileStream.Length-8);
		fileStream.Write(chunkSize,0,4);
		byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave,0,4);
		byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt,0,4);
		byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1,0,4);
		ushort two = 2;
		ushort one = 1;
		byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat,0,2);
		byte[] numChannels = BitConverter.GetBytes(two);
		if (channels == 2)
		{
			numChannels = BitConverter.GetBytes(two);
		} else if (channels == 1)
		{
			numChannels = BitConverter.GetBytes(one);
		} else 
		{//should we try to support 8 channels and change this to a case switch satement? or just support two?
			numChannels = BitConverter.GetBytes(two);
		}

		fileStream.Write(numChannels,0,2);
		byte[] sampleRate = BitConverter.GetBytes(outputRate);
		fileStream.Write(sampleRate,0,4);
		byte[] byteRate = BitConverter.GetBytes(outputRate*4);
		// sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate,0,4);
		ushort four = 4;
		byte[] blockAlign = BitConverter.GetBytes(four);
		fileStream.Write(blockAlign,0,2);
		ushort sixteen = 16;
		byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
		fileStream.Write(bitsPerSample,0,2);
		byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(dataString,0,4);
		byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length-headerSize);
		fileStream.Write(subChunk2,0,4);
		fileStream.Close();
	}

	public void OutputVolume(float input){
		if (input >= 0.0f && input <= 1.0f) {
			outputVol = input;
		}
	}

	public AudioClip RemoveSilenceFromFrontOfAudioClip(AudioClip myClip) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		int frontIndex = 0;
		bool flagged = false;
		for (int i = 0; i < recordedClip.Length; i++) {
			if (recordedClip [i] > 0f) {
				frontIndex = i;
				flagged = true;
				i = recordedClip.Length;
			}
		}
		if (flagged) {
			return TrimFrontOfAudioClip (myClip, frontIndex);
		} else {
			return myClip;
		}
	}

	public static AudioClip RemoveSilenceFromEndOfAudioClip(AudioClip myClip) {
		float[] recordedClip = new float[myClip.samples *myClip.channels];
		myClip.GetData(recordedClip, 0);
		int endIndex = 0;
		bool flagged = false;
		for (int i = recordedClip.Length-1; i >= 0; --i) {
			if (recordedClip[i] > 0f) {
				endIndex = i;
				flagged = true;
				i = -1;
			}
		}
		if (flagged) {
			return TrimEndOfAudioClip (myClip, endIndex);
		} else {
			return myClip;
		}
	}

	public float[] GetAudioLevels(){
		return audioLevels;
	}

	public void SetMicPlayBack(bool trueOrFalse){
		micPlayBack = trueOrFalse;
	}

	public void SetMicPlayBackVolume(float volume){
		if (volume <= 1.0f && volume >= 0.0f) {
			recSource.volume = volume;
		}
	}
	//****************************************************
	//  OTHER FUNCTIONS AND HELPER FUNCTIONS END <<<<<<
	//****************************************************
}

