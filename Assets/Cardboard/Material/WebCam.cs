using UnityEngine;
using System.Collections;
using System.Threading;
using ZXing;
using ZXing.QrCode;

public class WebCam : MonoBehaviour {
	public Texture2D encoded;
	public string deviceName;
	private int W,H;
	private Thread qrThread;
	WebCamTexture tex;
	private Color32[] c;
	public string LastResult;
	private bool shouldEncodeNow;
	private bool isQuit;

   void OnApplicationQuit()
   {
      isQuit = true;
   }
	// Use this for initialization

	 void OnDisable()
   {
      if (tex != null)
      {
         tex.Pause();
      }
   }

	IEnumerator Start () 
	{
		encoded = new Texture2D (256, 256);
		LastResult = "http://www.google.com";
		shouldEncodeNow = true;

		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
		if(Application.HasUserAuthorization(UserAuthorization.WebCam))
		{
			WebCamDevice[] devices = WebCamTexture.devices;
			deviceName = devices[0].name;
			tex = new WebCamTexture(deviceName, 640, 480, 30);
			GetComponent<Renderer>().material.mainTexture = tex;
			tex.Play();
			W = tex.width;
			H = tex.height;
		}

		qrThread = new Thread (DecodeQR);
		qrThread.Start();
	}

	void OnDestroy()
	{
		qrThread.Abort();
		tex.Stop();
	}

	void DecodeQR()
	{

		var barcodeReader = new BarcodeReader {AutoRotate = false, TryHarder = false};
		while (true) {
			if (isQuit)
				break;
			try{
				var result = barcodeReader.Decode(c,W,H);
				if (result != null)
				{
					LastResult = result.Text;
					shouldEncodeNow = true;
					print(result.Text);
				}
				Thread.Sleep(200);
				c = null;
			}
			catch{
			}
		}

	}

	void Update()
	{

		if (c == null) {
			c = tex.GetPixels32();
		}

		var textForEncoding = LastResult;

		if (shouldEncodeNow && textForEncoding != null) {
			var color32 = Encode(textForEncoding,encoded.width,encoded.height);
			encoded.SetPixels32(color32);
			encoded.Apply();
			shouldEncodeNow = false;

		}

	}

   private static Color32[] Encode(string textForEncoding, int width, int height)
   {
      var writer = new BarcodeWriter
      {
         Format = BarcodeFormat.QR_CODE,
         Options = new QrCodeEncodingOptions
         {
            Height = height,
            Width = width
         }
      };
      return writer.Write(textForEncoding);
   }

}