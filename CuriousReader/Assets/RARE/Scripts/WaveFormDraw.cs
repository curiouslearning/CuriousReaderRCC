using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaveFormDraw : MonoBehaviour {

	int resolution = 60;
	private Color[] blankColorArr;
	float[] waveForm;
	float[] samples;
	public Texture2D texture;
	public Slider playbackSli;
	public Slider rightcropSli;
	public Slider leftcropSli;
	public AudioSource referenceAsrc; //audiosource attached to this gameobject

	public  void StartWaveFormGeneration(AudioClip myClip = null) {
		if (myClip == null) {
			return;
		} 
		resolution = 60;
		AudioSource audio = GetComponent<AudioSource>();
	    audio.clip = myClip;
		resolution = audio.clip.frequency  / resolution;
		samples = new float[audio.clip.samples * audio.clip.channels];
		audio.clip.GetData(samples, 0);

        if (((audio.clip.samples * audio.clip.channels) / resolution) < 10000) {
            waveForm = new float[((audio.clip.samples * audio.clip.channels) / resolution)];
        } else {
            //if too big of a waveform then make 10000 width and calculate the proper resolution to be at the proper ratio so the clip is in time
            waveForm = new float[10000];
            resolution = (audio.clip.samples * audio.clip.channels) / 10000;
        }

        //  Debug.Log(waveForm.Length + "Res "  + resolution);
		for (int i = 0; i < waveForm.Length; i++) {
			waveForm[i] = 0;
			for (int ii = 0; ii < resolution; ii++) {
				waveForm[i] += Mathf.Abs(samples[(i * resolution) + ii]);
			}
			waveForm[i] /= resolution;
		}
		Denerate();
	}

	public void Denerate() {
		blankColorArr = new Color[waveForm.Length * 100];
		for (int i = 0; i < blankColorArr.Length; i++) {
			blankColorArr[i] = Color.gray;
		}
		leftcropSli.value = 0;
		playbackSli.value = 0;
		texture = new Texture2D(waveForm.Length, 100);
		texture.SetPixels(blankColorArr, 0);
		GetComponent<RawImage>().texture = texture;
		AudioSource audio = GetComponent<AudioSource>();
		playbackSli.maxValue = audio.clip.samples *audio.clip.channels;
		rightcropSli.maxValue = audio.clip.samples * audio.clip.channels;
		leftcropSli.maxValue = audio.clip.samples * audio.clip.channels;
		rightcropSli.value = audio.clip.samples * audio.clip.channels;

		//  rightcrop.value = .99f;
		if (audio.clip.channels == 1) {
			for (int i = 0; i < waveForm.Length - 1; i++) {
				Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
				Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);
				// Debug.Log(Mathf.Abs(waveForm[i]));
				DrawLine(texture, i, ((int)(waveForm[i] * 250) + 50), i, 50, Color.green);
				DrawLine(texture, i, (-(int)(waveForm[i] * 250) + 50), i, 50, Color.green);
				//  Debug.DrawLine(sv, ev, Color.yellow);
			}
		}
		else if (audio.clip.channels == 2) {
			for (int i = 0; i < waveForm.Length - 1; i++) {
				Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
				Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);
				// Debug.Log(Mathf.Abs(waveForm[i]));
				//we factor in the even and odds to get the difference in channels and then asign it to the textures
				if (i % 2 == 0)	{
					DrawLine(texture, i, ((int)(waveForm[i] * 100) + 75), i, 75, Color.green);
					DrawLine(texture, i, (-(int)(waveForm[i] * 100) + 75), i, 75, Color.green);
					//because we are only drawing half of the time duplicate the same thing to the line behind it hacky solution could maybe make a condensed texture if two channels
					DrawLine(texture, i - 1, ((int)(waveForm[i] * 100) + 75), i - 1, 75, Color.green);
					DrawLine(texture, i - 1, (-(int)(waveForm[i] * 100) + 75), i - 1, 75, Color.green);
			    } else {
					//minus 1s and two keep the stereo lines in sync
					DrawLine(texture, i, (-(int)(waveForm[i] * 100) + 25), i, 25, Color.green);
					DrawLine(texture, i, ((int)(waveForm[i] * 100) + 25), i, 25, Color.green);
					DrawLine(texture, i + 1, (-(int)(waveForm[i] * 100) + 25), i + 1, 25, Color.green);
					DrawLine(texture, i + 1, ((int)(waveForm[i] * 100) + 25), i + 1, 25, Color.green);
				}
			}
		}
		texture.Apply();
	}

	public Texture2D GetWaveformTex(AudioClip myClip) {
		//AudioSource audio = GetComponent<AudioSource>();
		resolution = myClip.frequency / resolution;
		samples = new float[myClip.samples * myClip.channels];
		myClip.GetData(samples, 0);
		waveForm = new float[(myClip.samples / resolution)];
		for (int i = 0; i < waveForm.Length; i++) {
			waveForm[i] = 0;
			for (int ii = 0; ii < resolution; ii++)	{
				waveForm[i] += Mathf.Abs(samples[(i * resolution) + ii]);
			}
			waveForm[i] /= resolution;
		}

		blankColorArr = new Color[waveForm.Length * 100];
		for (int i = 0; i < blankColorArr.Length; i++) {
			blankColorArr[i] = Color.gray;
		}
		texture = new Texture2D(waveForm.Length, 100);
		texture.SetPixels (blankColorArr, 0);

	    if (myClip.channels == 1) {
			for (int i = 0; i < waveForm.Length - 1; i++) {
				Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
				Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);
				// Debug.Log(Mathf.Abs(waveForm[i]));
				DrawLine(texture, i, ((int)(waveForm[i] * 250) + 50), i, 50, Color.green);
				DrawLine(texture, i, (-(int)(waveForm[i] * 250) + 50), i, 50, Color.green);
				//  Debug.DrawLine(sv, ev, Color.yellow);
			}
		} else if (myClip.channels == 2) {
			for (int i = 0; i < waveForm.Length - 1; i++) {
				Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
				Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);
				// Debug.Log(Mathf.Abs(waveForm[i]));
				//we factor in the even and odds to get the difference in channels and then asign it to the textures
				if (i % 2 == 0) {
					DrawLine(texture, i, ((int)(waveForm[i] * 100) + 75), i, 75, Color.green);
					DrawLine(texture, i, (-(int)(waveForm[i] * 100) + 75), i, 75, Color.green);
					//because we are only drawing half of the time duplicate the same thing to the line behind it hacky solution could maybe make a condensed texture if two channels
					DrawLine(texture, i -1, ((int)(waveForm[i] * 100) + 75), i - 1, 75, Color.green);
				    DrawLine(texture, i -1, (-(int)(waveForm[i] * 100) + 75), i -1, 75, Color.green);
				} else {
					//minus 1s and two keep the stereo lines in sync
					DrawLine(texture, i, (-(int)(waveForm[i] * 100) + 25), i, 25, Color.green);
					DrawLine(texture, i, ((int)(waveForm[i] * 100) + 25), i, 25, Color.green);
					DrawLine(texture, i+1, (-(int)(waveForm[i] * 100) + 25), i+1, 25, Color.green);
					DrawLine(texture, i+1, ((int)(waveForm[i] * 100) + 25), i+1, 25, Color.green);
				}
			}
		}
		texture.Apply();
		return texture;
    }

	void Update() {
		if (waveForm == null) {
			return;
		}
		for (int i = 0; i < waveForm.Length - 1; i++) {
			Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
			Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);
			Debug.DrawLine(sv, ev, Color.yellow);
		}
		int current = referenceAsrc.timeSamples / resolution;
		current *= 2;
		Vector3 c = new Vector3(current * .01f, 0, 0);
		Debug.DrawLine(c, c + Vector3.up * 10, Color.white);
	}

	void DrawLine(Texture2D tex, int x1, int y1, int x0, int y0, Color col) {
		int dy = (int)(y1 - y0);
		int dx = (int)(x1 - x0);
		int stepx, stepy;

		if (dy < 0) { dy = -dy; stepy = -1; }
		else { stepy = 1; }
		if (dx < 0) { dx = -dx; stepx = -1; }
		else { stepx = 1; }
		dy <<= 1;
		dx <<= 1;

		float fraction = 0;

		tex.SetPixel(x0, y0, col);
		if (dx > dy) {
			fraction = dy - (dx >> 1);
			while (Mathf.Abs(x0 - x1) > 1) {
				if (fraction >= 0) {
					y0 += stepy;
					fraction -= dx;
				}
				x0 += stepx;
				fraction += dy;
				tex.SetPixel(x0, y0, col);
			}
		} else {
			fraction = dx - (dy >> 1);
			while (Mathf.Abs(y0 - y1) > 1) {
				if (fraction >= 0) {
					x0 += stepx;
					fraction -= dy;
				}
				y0 += stepy;
				fraction += dx;
				tex.SetPixel(x0, y0, col);
			}
		}
	}
}
