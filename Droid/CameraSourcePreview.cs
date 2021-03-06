﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Vision;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

//
// Taken from the guide and code below
// guide: https://blog.xamarin.com/adding-face-tracking-live-recognition-android-app/
// code: https://github.com/nishanil/Cogs/tree/master/LiveCam
//

namespace DataCollector.Droid
{
    public class CameraSourcePreview : ViewGroup, ISurfaceHolderCallback
    {
        private static readonly String __CLASS__ = "CameraSourcePreview";

        private Context mContext;
        private SurfaceView mSurfaceView;
        private bool mStartRequested;
        private bool mSurfaceAvailable;
        private CameraSource mCameraSource;

        public CameraSourcePreview(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            mContext = context;
            mStartRequested = false;
            mSurfaceAvailable = false;

            mSurfaceView = new SurfaceView(context);
            mSurfaceView.Holder.AddCallback(this);

            AddView(mSurfaceView);
        }

        public void Start(CameraSource cameraSource)
        {
            if (cameraSource == null)
            {
                Stop();
            }

            mCameraSource = cameraSource;

            if (mCameraSource != null)
            {
                mStartRequested = true;
                StartIfReady();
            }
        }

        public void Stop()
        {
            if (mCameraSource != null)
            {
                mCameraSource.Stop();
            }
        }

        public void Release()
        {
            if (mCameraSource != null)
            {
                mCameraSource.Release();
                mCameraSource = null;
            }
        }

        private void StartIfReady()
        {
            if (mStartRequested && mSurfaceAvailable)
            {
                mCameraSource.Start(mSurfaceView.Holder);
                mStartRequested = false;
            }
        }

        private bool IsPortraitMode()
        {
            var orientation = mContext.Resources.Configuration.Orientation;
            if (orientation == Android.Content.Res.Orientation.Landscape)
            {
                return false;
            }
            if (orientation == Android.Content.Res.Orientation.Portrait)
            {
                return true;
            }

            Log.Debug(__CLASS__, "IsPortraitMode returning false by default");
            return false;
        }



        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {

        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mSurfaceAvailable = true;


            try
            {
                StartIfReady();
            }
            catch (Exception e)
            {
                Log.Error(__CLASS__, "Could not start camera source.", e);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            mSurfaceAvailable = false;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int width = 320;
            int height = 240;
            if (mCameraSource != null)
            {
                var size = mCameraSource.PreviewSize;
                if (size != null)
                {
                    width = size.Width;
                    height = size.Height;
                }
            }

            // Swap width and height sizes when in portrait, since it will be rotated 90 degrees
            if (IsPortraitMode())
            {
                int tmp = width;
                width = height;
                height = tmp;
            }

            int layoutWidth = r - l;
            int layoutHeight = b - t;

            // Computes height and width for potentially doing fit width.
            int childWidth = layoutWidth;
            int childHeight = (int)(((float)layoutWidth / (float)width) * height);

            // If height is too tall using fit width, does fit height instead.
            if (childHeight > layoutHeight)
            {
                childHeight = layoutHeight;
                childWidth = (int)(((float)layoutHeight / (float)height) * width);
            }

            for (int i = 0; i < ChildCount; ++i)
            {

                GetChildAt(i).Layout(0, 0, childWidth, childHeight);
            }

            try
            {
                StartIfReady();
            }
            catch (Exception e)
            {
                Log.Error(__CLASS__, "Could not start camera source.", e);
            }
        }
    }
}
