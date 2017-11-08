
using System;
using System.Threading.Tasks;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.Gms.Vision.Texts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataCollector.Droid
{
    public class DetectionTracker : Tracker, CameraSource.IPictureCallback
    {
        private MainActivity mActivity;
        private CameraSource mCameraSource = null;
        private bool isProcessing = false;

        public DetectionTracker(MainActivity activity, CameraSource cameraSource = null)
        {
            mActivity = activity;
            mCameraSource = cameraSource;
        }

        public override void OnNewItem(int id, Java.Lang.Object item)
        {
            Barcode barcode = item as Barcode;
            TextBlock textBlock = item as TextBlock;
            if (barcode != null)
            {
                mActivity.AddBarcode(String.Format("Barcode: {0}", barcode.DisplayValue));
            }
            else if (textBlock != null)
            {
                mActivity.AddText(String.Format("Text: {0}", textBlock.Value));
                if (mCameraSource != null && !isProcessing)
                {
                    Task.Run(() =>
                    {
                        lock (mActivity)
                        {
                            if (!isProcessing)
                            {
                                isProcessing = true;
                                mCameraSource.TakePicture(null, this);
                                mActivity.StartCameraSource();
                            }
                        }
                    });
                }
            }
        }

        //OnUpdate
        //OnMissing
        //OnDone

        public void OnPictureTaken(byte[] data)
        {
            Task.Run(async () =>
            {
                try
                {
                    mActivity.AddService("Sending Photo");

                    ServiceHelper sh = new DataCollector.ServiceHelper();

                    JObject response = await sh.SendRequestAsync("", data);

                    mActivity.AddService(response.ToString(Formatting.None));
                }

                finally
                {
                    lock (mActivity)
                    {
                        isProcessing = false;
                    }
                }

            });
        }
    }
}
