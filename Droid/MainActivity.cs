using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Support.V4.App;
using Android;
using Android.Util;
using Android.Gms.Vision.Barcodes;
using Android.Gms.Vision.Texts;
using Android.Gms.Common;
using System.Collections.Generic;
using System;

namespace DataCollector.Droid
{
    [Activity(Label = "DataCollector", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.FullSensor)]
    public class MainActivity : Activity, MultiProcessor.IFactory
    {
        private static readonly string __CLASS__ = "MainActivity";

        private CameraSource mCameraSource = null;

        private CameraSourcePreview mPreview;
        private TextView mText;
        private TextView mBarcode;
        private TextView mService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mPreview = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            mText = FindViewById<TextView>(Resource.Id.text);
            mBarcode = FindViewById<TextView>(Resource.Id.barcode);
            mService = FindViewById<TextView>(Resource.Id.service);

            CreateCameraSource();
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mPreview.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mCameraSource != null)
            {
                mCameraSource.Release();
            }
        }

        private void CreateCameraSource()
        {
            var context = Application.Context;
            BarcodeDetector barcodes = new BarcodeDetector.Builder(context).Build();
            TextRecognizer text = new TextRecognizer.Builder(context).Build();
            MultiDetector detector = new MultiDetector.Builder().Add(barcodes).Add(text).Build();

            barcodes.SetProcessor(new MultiProcessor.Builder(this).Build());
            text.SetProcessor(new MultiProcessor.Builder(this).Build());

            if (!detector.IsOperational)
            {
                Log.Warn(__CLASS__, "Detection is not ready");
            }

            mCameraSource = new CameraSource.Builder(context, detector)
                .SetRequestedPreviewSize(512, 300)
                .SetFacing(CameraFacing.Front)
                .SetRequestedFps(15.0f)
                .SetAutoFocusEnabled(true)
                .Build();
        }

        public void StartCameraSource()
        {
            int code = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.ApplicationContext);
            if (code != ConnectionResult.Success)
            {
                Dialog dlg = GoogleApiAvailability.Instance.GetErrorDialog(this, code, 9001);
                dlg.Show();
            }

            if (mCameraSource != null)
            {
                try
                {
                    mPreview.Start(mCameraSource);
                }
                catch (System.Exception e)
                {
                    Log.Error(__CLASS__, "Unable to start camera", e);
                    mCameraSource.Release();
                    mCameraSource = null;
                }
            }
        }

        public void AddBarcode(string msg)
        {
            RunOnUiThread(() => {
                List<string> msgs = new List<string>(mBarcode.Text.Split('\n'));
                msgs.Add(msg);
                while (msgs.Count > 10)
                {
                    msgs.RemoveAt(0);
                }
                mBarcode.Text = String.Join("\n", msgs.ToArray());
            });
        }

        public void AddText(string msg)
        {
            RunOnUiThread(() => {
                List<string> msgs = new List<string>(mText.Text.Split('\n'));
                msgs.Add(msg);
                while (msgs.Count > 10)
                {
                    msgs.RemoveAt(0);
                }
                mText.Text = String.Join("\n", msgs.ToArray());
            });
        }

        public void AddService(string msg)
        {
            RunOnUiThread(() => {
                List<string> msgs = new List<string>(mService.Text.Split('\n'));
                msgs.Add(msg);
                while (msgs.Count > 5)
                {
                    msgs.RemoveAt(0);
                }
                mService.Text = String.Join("\n", msgs.ToArray());
            });
        }

        public Tracker Create(Java.Lang.Object item)
        {
            return new DetectionTracker(this, mCameraSource);
        }
    }
}

