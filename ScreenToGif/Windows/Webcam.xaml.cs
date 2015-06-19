﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ScreenToGif.FileWriters;
using ScreenToGif.Properties;
using ScreenToGif.Util;
using ScreenToGif.Util.ActivityHook;
using ScreenToGif.Util.Enum;
using ScreenToGif.Util.Writers;
using ScreenToGif.Webcam.DirectX;

namespace ScreenToGif.Windows
{
    /// <summary>
    /// Interaction logic for Webcam.xaml
    /// </summary>
    public partial class Webcam
    {
        #region Variables

        private CaptureWebcam _capture = null;
        private Filters _filters;

        /// <summary>
        /// The object of the keyboard and mouse hooks.
        /// </summary>
        private readonly UserActivityHook _actHook;

        #region Flags

        /// <summary>
        /// The actual stage of the program.
        /// </summary>
        public Stage Stage { get; set; }

        /// <summary>
        /// The action to be executed after closing this Window.
        /// </summary>
        public ExitAction ExitArg = ExitAction.Return;

        #endregion

        #region Counters

        /// <summary>
        /// The numbers of frames, this is updated while recording.
        /// </summary>
        private int _frameCount = 0;

        #endregion

        /// <summary>
        /// Lists of cursors.
        /// </summary>
        public List<FrameInfo> ListFrames = new List<FrameInfo>();

        /// <summary>
        /// The Path of the Temp folder.
        /// </summary>
        private readonly string _pathTemp = Path.GetTempPath() +
            String.Format(@"ScreenToGif\Recording\{0}\", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")); //TODO: Change to a more dynamic folder naming.

        private Timer _timer = new Timer();

        #endregion

        #region Async Load

        public delegate bool Load();

        private Load _loadDel;

        /// <summary>
        /// Loads the list of video devices.
        /// </summary>
        private bool LoadVideoDevices()
        {
            _filters = new Filters();

            //If no Video Input Devices Detected
            if (_filters.VideoInputDevices.Count == 0)
                return false;

            for (int i = 0; i < _filters.VideoInputDevices.Count; i++)
            {
                Dispatcher.Invoke(() =>
                {
                    VideoDevicesComboBox.Items.Add(_filters.VideoInputDevices[i].Name);
                });
            }

            return true;
        }

        private void LoadCallBack(IAsyncResult r)
        {
            var result = _loadDel.EndInvoke(r);

            if (result)
            {
                Dispatcher.Invoke(() =>
                {
                    //Selects the first video device.
                    VideoDevicesComboBox.SelectedIndex = 0;

                    StopButton.IsEnabled = true;
                    RecordPauseButton.IsEnabled = true;
                    NumericUpDown.IsEnabled = true;
                    VideoDevicesComboBox.IsEnabled = true;

                    _actHook.Start(false, true); //false for the mouse, true for the keyboard.
                });

                return;
            }

            Dispatcher.Invoke(() =>
            {
                StopButton.IsEnabled = false;
                RecordPauseButton.IsEnabled = false;
                NumericUpDown.IsEnabled = false;
                VideoDevicesComboBox.IsEnabled = false;

                NoVideoLabel.Visibility = Visibility.Visible;
            });
        }

        #endregion

        #region Inicialization

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Webcam()
        {
            InitializeComponent();

            //Load
            _timer.Tick += Normal_Elapsed;

            #region Global Hook

            try
            {
                _actHook = new UserActivityHook();
                _actHook.KeyDown += KeyHookTarget;
            }
            catch (Exception) { }

            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _loadDel = LoadVideoDevices;
            _loadDel.BeginInvoke(LoadCallBack, null);
        }

        #endregion

        #region Hooks

        /// <summary>
        /// KeyHook event method. This fires when the user press a key.
        /// </summary>
        private void KeyHookTarget(object sender, CustomKeyEventArgs e)
        {
            //TODO: I need a better way of comparing the keys.
            if (e.Key.ToString().Equals(Settings.Default.StartPauseKey.ToString()))
            {
                RecordPauseButton_Click(null, null);
            }
            else if (e.Key.ToString().Equals(Settings.Default.StopKey.ToString()))
            {
                StopButton_Click(null, null);
            }
        }

        #endregion

        #region Other Events

        private void VideoDevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get current devices and dispose of capture object
                // because the video device can only be changed
                // by creating a new Capture object.
                Filter videoDevice = null;

                // To change the video device, a dispose is needed.
                if (_capture != null)
                {
                    _capture.Dispose();
                    _capture = null;
                }

                // Get new video device
                videoDevice = (VideoDevicesComboBox.SelectedIndex > -1 ? _filters.VideoInputDevices[VideoDevicesComboBox.SelectedIndex] : null);

                // Create capture object
                if (videoDevice != null)
                {
                    _capture = new CaptureWebcam(videoDevice) { PreviewWindow = this };
                    _capture.StartPreview();
          
                    Height = _capture.Height + 70;
                    Width = _capture.Width;
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Video device not supported");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                _actHook.Stop(); //Stop the user activity watcher.
            }
            catch (Exception) { }

            if (Stage != (int)Stage.Stopped)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (_capture != null)
            {
                _capture.StopPreview();
                _capture.Dispose();
            }
        }

        #endregion

        #region Record Async

        /// <summary>
        /// Saves the Bitmap to the disk and adds the filename in the list of frames.
        /// </summary>
        /// <param name="filename">The final filename of the Bitmap.</param>
        /// <param name="bitmap">The Bitmap to save in the disk.</param>
        public delegate void AddFrame(string filename, Bitmap bitmap);

        private AddFrame _addDel;

        private void AddFrames(string filename, Bitmap bitmap)
        {
            bitmap.Save(filename);
            bitmap.Dispose();
        }

        private void CallBack(IAsyncResult r)
        {
            //if (!this.IsLoaded) return;

            _addDel.EndInvoke(r);
        }

        #endregion

        #region Timer

        private void Normal_Elapsed(object sender, EventArgs e)
        {
            string fileName = String.Format("{0}{1}.bmp", _pathTemp, _frameCount);
            ListFrames.Add(new FrameInfo(fileName, _timer.Interval));

            _addDel.BeginInvoke(fileName, new Bitmap(_capture.GetFrame()), CallBack, null);

            //ThreadPool.QueueUserWorkItem(delegate { AddFrames(fileName, new Bitmap(_capture.GetFrame())); });
            
            Dispatcher.Invoke(() => Title = String.Format("Screen To Gif • {0}", _frameCount));

            _frameCount++;
            GC.Collect(1);
        }

        #endregion

        #region Click Events

        private void RecordPauseButton_Click(object sender, RoutedEventArgs e)
        {
            Extras.CreateTemp(_pathTemp);

            _capture.PrepareCapture();

            if (Stage == Stage.Stopped)
            {
                #region To Record

                _timer = new Timer { Interval = 1000 / NumericUpDown.Value };

                ListFrames = new List<FrameInfo>();

                RefreshButton.IsEnabled = false;
                VideoDevicesComboBox.IsEnabled = false;
                NumericUpDown.IsEnabled = false;
                Topmost = true;

                _addDel = AddFrames;
                _capture.GetFrame();

                #region Start - Normal or Snap

                if (!Settings.Default.Snapshot)
                {
                    #region Normal Recording

                    _timer.Tick += Normal_Elapsed;
                    Normal_Elapsed(null, null);
                    _timer.Start();

                    Stage = Stage.Recording;
                    RecordPauseButton.Text = Properties.Resources.Pause;
                    RecordPauseButton.Content = (Canvas)FindResource("Pause");

                    #endregion
                }
                else
                {
                    #region SnapShot Recording

                    Stage = Stage.Snapping;
                    RecordPauseButton.Content = (Canvas)FindResource("CameraOld");
                    RecordPauseButton.Text = Properties.Resources.btnSnap;
                    Title = "Screen To Gif - " + Properties.Resources.Con_SnapshotMode;

                    Normal_Elapsed(null, null);

                    #endregion
                }

                #endregion

                #endregion
            }
            else if (Stage == Stage.Recording)
            {
                #region To Pause

                Stage = Stage.Paused;
                RecordPauseButton.Text = Properties.Resources.btnRecordPause_Continue;
                RecordPauseButton.Content = (Canvas)FindResource("RecordDark");
                Title = Properties.Resources.TitlePaused;

                _timer.Stop();

                #endregion
            }
            else if (Stage == Stage.Paused)
            {
                #region To Record Again

                Stage = Stage.Recording;
                RecordPauseButton.Text = Properties.Resources.Pause;
                RecordPauseButton.Content = (Canvas)FindResource("Pause");
                Title = Properties.Resources.TitleRecording;

                _timer.Start();

                #endregion
            }
            else if (Stage == Stage.Snapping)
            {
                #region Take Screenshot

                Normal_Elapsed(null, null);

                #endregion
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _frameCount = 0; 

                _timer.Stop();

                if (Stage != Stage.Stopped && Stage != Stage.PreStarting && ListFrames.Any())
                {
                    #region If not Already Stoped nor Pre Starting and FrameCount > 0, Stops

                    //TODO: Stop the keyboard and mouse watcher.
                    //TODO: Do async the merge of the cursor with the image and the resize of full screen recordings.
                    //Or maybe just open the editor and do that there.
                    //Close this window and return the list of frames.

                    ExitArg = ExitAction.Recorded;
                    DialogResult = false;

                    #endregion
                }
                else if ((Stage == Stage.PreStarting || Stage == Stage.Snapping) && !ListFrames.Any())
                {
                    #region if Pre-Starting or in Snapmode and no Frames, Stops

                    Stage = Stage.Stopped;

                    //Enables the controls that are disabled while recording;
                    NumericUpDown.IsEnabled = true;
                    RecordPauseButton.IsEnabled = true;
                    RefreshButton.IsEnabled = true;
                    VideoDevicesComboBox.IsEnabled = true;
                    Topmost = true;

                    RecordPauseButton.Text = Properties.Resources.btnRecordPause_Record;
                    RecordPauseButton.Content = (Canvas)FindResource("RecordDark");
                    Title = Properties.Resources.TitleStoped;

                    #endregion
                }
            }
            catch (NullReferenceException nll)
            {
                var errorViewer = new ExceptionViewer(nll);
                errorViewer.ShowDialog();
                LogWriter.Log(nll, "NullPointer in the Stop function");
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();
                LogWriter.Log(ex, "Error in the Stop function");
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;

            //TODO: If recording started, maybe disable some properties.
            var options = new Options();
            options.ShowDialog();

            Topmost = true;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RecordPauseButton.IsEnabled = false;
            RecordPauseButton.IsEnabled = false;

            //Clear the combo box.
            VideoDevicesComboBox.Items.Clear();

            //Check again for video devices.
            LoadVideoDevices();
        }

        #endregion
    }
}
