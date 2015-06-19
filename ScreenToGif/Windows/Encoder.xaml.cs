﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ScreenToGif.Controls;
using ScreenToGif.FileWriters;
using ScreenToGif.FileWriters.GifWriter;
using ScreenToGif.ImageUtil;
using ScreenToGif.Properties;
using ScreenToGif.Util;
using ScreenToGif.Util.Enum;
using ScreenToGif.Util.Writers;
using ScreenToGif.Windows.Other;

namespace ScreenToGif.Windows
{
    /// <summary>
    /// Interaction logic for Encoder.xaml
    /// </summary>
    public partial class Encoder : Window
    {
        #region Variables

        /// <summary>
        /// The static Encoder window.
        /// </summary>
        private static Encoder _encoder = null;

        /// <summary>
        /// List of Tasks, each task executes the encoding process for one recording.
        /// </summary>
        private static readonly List<Task> TaskList = new List<Task>();

        /// <summary>
        /// List of CancellationTokenSource, used to cancel each task.
        /// </summary>
        private static readonly List<CancellationTokenSource> CancellationTokenList = new List<CancellationTokenSource>();

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Encoder()
        {
            InitializeComponent();
        }

        #region Events

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TODO: Check for memmory leaks.

            foreach (CancellationTokenSource tokenSource in CancellationTokenList)
            {
                tokenSource.Cancel();
            }

            _encoder = null;
            GC.Collect();
        }

        private void EncoderItem_CloseButtonClickedEvent(object sender)
        {
            var item = sender as EncoderListViewItem;
            if (item == null) return;

            if (item.Status != Status.Encoding)
            {
                item.CloseButtonClickedEvent -= EncoderItem_CloseButtonClickedEvent;

                int index = EncodingListBox.Items.IndexOf(item);

                TaskList.RemoveAt(index);
                CancellationTokenList.RemoveAt(index);
                EncodingListBox.Items.RemoveAt(index);
            }
            else if (!item.TokenSource.IsCancellationRequested)
            {
                item.TokenSource.Cancel();
            }
        }

        private void EncoderItem_LabelLinkClickedEvent(object name)
        {
            var fileName = name as String;

            if (!String.IsNullOrEmpty(fileName))
                if (File.Exists(fileName))
                    Process.Start(fileName);
        }

        #endregion

        #region Private

        private void InternalAddItem(List<FrameInfo> listFrames, string fileName, Export type)
        {
            //Creates the Cancellation Token
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationTokenList.Add(cancellationTokenSource);

            var context = TaskScheduler.FromCurrentSynchronizationContext();

            //Creates Task and send the Task Id.
            int a = -1;
            var task = new Task(() =>
                {
                    Encode(listFrames, a, fileName, type, cancellationTokenSource);
                },
                CancellationTokenList.Last().Token, TaskCreationOptions.LongRunning);
            a = task.Id;

            #region Error Handling

            task.ContinueWith(t =>
            {
                AggregateException aggregateException = t.Exception;
                if (aggregateException != null)
                    aggregateException.Handle(exception => true);

                SetStatus(Status.Error, a);

                LogWriter.Log(t.Exception, "Encoding Error");
            },
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, context);

            #endregion

            #region Creates List Item

            var encoderItem = new EncoderListViewItem
            {
                Image = type == Export.Gif ? (UIElement)Resources["Vector.Image"] : (UIElement)Resources["Vector.Info"], //TODO: Video vector.
                Text = "Starting",
                FrameCount = listFrames.Count,
                Id = a,
                TokenSource = cancellationTokenSource,
            };

            encoderItem.CloseButtonClickedEvent += EncoderItem_CloseButtonClickedEvent;
            encoderItem.LabelLinkClickedEvent += EncoderItem_LabelLinkClickedEvent;

            EncodingListBox.Items.Add(encoderItem);

            #endregion

            try
            {
                TaskList.Add(task);
                TaskList.Last().Start();
            }
            catch (Exception ex)
            {
                Dialog.Ok("Task Error", "Unable to start the encoding task", "A generic error occured while trying to start the encoding task. " + ex.Message);
                LogWriter.Log(ex, "Errow while starting the task.");
            }
        }

        private void InternalUpdate(int id, int currentFrame, string status)
        {
            Dispatcher.Invoke(() =>
            {
                var item = EncodingListBox.Items.Cast<EncoderListViewItem>().FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.CurrentFrame = currentFrame;
                    item.Text = status;
                }
            });
        }

        private void InternalUpdate(int id, int currentFrame)
        {
            Dispatcher.Invoke(() =>
            {
                var item = EncodingListBox.Items.Cast<EncoderListViewItem>().FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.CurrentFrame = currentFrame;
                }
            });
        }

        private void InternalSetStatus(Status status, int id, string fileName)
        {
            Dispatcher.Invoke(() =>
            {
                var item = EncodingListBox.Items.Cast<EncoderListViewItem>().FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.Status = status;

                    if (status == Status.Completed)
                    {
                        item.Image = (UIElement)Resources["Vector.Success"];

                        if (File.Exists(fileName))
                        {
                            item.SizeInBytes = new FileInfo(fileName).Length;
                            item.OutputPath = fileName;
                            item.Text = "Completed";
                        }
                    }
                    else if (status == Status.Cancelled)
                    {
                        item.Text = "Cancelled";
                    }
                    else if (status == Status.Error)
                    {
                        item.Image = (UIElement)Resources["Vector.Error"];
                        item.Text = "Error";
                    }
                }
            });
        }

        private void Encode(List<FrameInfo> listFrames, int id, string fileName, Export type, CancellationTokenSource tokenSource)
        {
            if (type == Export.Gif)
            {
                #region Gif

                if (Settings.Default.CustomEncoding)
                {
                    #region Custom Gif Encoding

                    using (var encoder = new AnimatedGifEncoder())
                    {
                        string cutFolder = null;

                        #region Cut/Paint Unchanged Pixels

                        if (Settings.Default.DetectUnchanged)
                        {
                            Update(id, 0, "Analizing Unchanged Pixels");

                            if (Settings.Default.PaintTransparent)
                            {
                                var color = Color.FromArgb(Settings.Default.TransparentColor.R,
                                    Settings.Default.TransparentColor.G, Settings.Default.TransparentColor.B);

                                listFrames = ImageMethods.PaintTransparentAndCut(listFrames, color, id, tokenSource);

                                //TODO: Use System.Windows.Media.Color inside the AnimatedGifEncoder.
                                encoder.SetTransparent(color);
                                encoder.SetDispose(1); //Undraw Method, "Leave".
                            }
                            else
                            {
                                listFrames = ImageMethods.CutUnchanged(listFrames, id, tokenSource);
                            }
                        }

                        #endregion

                        encoder.Start(fileName);
                        encoder.SetQuality(Settings.Default.Quality);
                        encoder.SetRepeat(Settings.Default.Looped ? (Settings.Default.RepeatForever ? 0 : Settings.Default.RepeatCount) : -1); // 0 = Always, -1 once

                        int numImage = 0;
                        foreach (FrameInfo image in listFrames)
                        {
                            #region Cancellation

                            if (tokenSource.Token.IsCancellationRequested)
                            {
                                SetStatus(Status.Cancelled, id);

                                break;
                            }

                            #endregion

                            var bitmapAux = new Bitmap(image.ImageLocation);

                            encoder.SetDelay(image.Delay);
                            encoder.AddFrame(bitmapAux, image.PositionTopLeft.X, image.PositionTopLeft.Y);

                            bitmapAux.Dispose();

                            Update(id, numImage, "Processing " + numImage);
                            numImage++;
                        }

                        #region Specific Clear

                        try
                        {
                            if (!String.IsNullOrEmpty(cutFolder))
                                if (Directory.Exists(cutFolder))
                                    Directory.Delete(cutFolder, true);
                        }
                        catch (Exception ex)
                        {
                            LogWriter.Log(ex, "Errow while Deleting and Cleaning Specific Variables");
                        }

                        #endregion
                    }

                    #endregion
                }
                else
                {
                    #region paint.NET encoding

                    //0 = Always, -1 = no repeat, n = repeat number (first shown + repeat number = total number of iterations)
                    var repeat = (Settings.Default.Looped ? (Settings.Default.RepeatForever ? 0 : Settings.Default.RepeatCount) : -1);

                    using (var stream = new MemoryStream())
                    {
                        using (var encoderNet = new GifEncoder(stream, null, null, repeat))
                        {
                            for (int i = 0; i < listFrames.Count; i++)
                            {
                                var bitmapAux = new Bitmap(listFrames[i].ImageLocation);
                                encoderNet.AddFrame(bitmapAux, 0, 0, TimeSpan.FromMilliseconds(listFrames[i].Delay));
                                bitmapAux.Dispose();

                                Update(id, i, "Processing " + i);

                                #region Cancellation

                                if (tokenSource.Token.IsCancellationRequested)
                                {
                                    SetStatus(Status.Cancelled, id);

                                    break;
                                }

                                #endregion
                            }
                        }

                        stream.Position = 0;

                        try
                        {
                            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None,
                            Constants.BufferSize, false))
                            {
                                stream.WriteTo(fileStream);
                            }
                        }
                        catch (Exception ex)
                        {
                            SetStatus(Status.Error, id);
                            LogWriter.Log(ex, "Error while writing to disk.");
                        }
                    }

                    #endregion
                }

                #endregion
            }
            else
            {
                #region Avi

                var image = listFrames[0].ImageLocation.SourceFrom();

                using (var aviWriter = new AviWriter(fileName, 1000 / listFrames[0].Delay, (int)Math.Round(image.Width, MidpointRounding.AwayFromZero), (int)Math.Round(image.Height, MidpointRounding.AwayFromZero), 5000))
                {
                    int numImage = 0;
                    foreach (FrameInfo frame in listFrames)
                    {
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            var bitImage = frame.ImageLocation.SourceFrom();

                            var enc = new BmpBitmapEncoder();
                            enc.Frames.Add(BitmapFrame.Create(bitImage));
                            enc.Save(outStream);

                            outStream.Flush();

                            using (var bitmap = new Bitmap(outStream))
                            {
                                aviWriter.AddFrame(bitmap);
                            }
                        }

                        //aviWriter.AddFrame(new BitmapImage(new Uri(frame.ImageLocation)));

                        Update(id, numImage, "Processing " + numImage);
                        numImage++;

                        #region Cancellation

                        if (tokenSource.Token.IsCancellationRequested)
                        {
                            SetStatus(Status.Cancelled, id);
                            break;
                        }

                        #endregion
                    }
                }

                #endregion
            }

            #region Delete Encoder Folder

            try
            {
                var encoderFolder = Path.GetDirectoryName(listFrames[0].ImageLocation);

                if (!String.IsNullOrEmpty(encoderFolder))
                    if (Directory.Exists(encoderFolder))
                        Directory.Delete(encoderFolder, true);
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Errow while deleting and cleaning the Encode folder");
            }

            #endregion

            GC.Collect();

            if (!tokenSource.Token.IsCancellationRequested)
                SetStatus(Status.Completed, id, fileName);
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            var finishedTasks = TaskList.Where(x => x.IsCompleted || x.IsCanceled || x.IsFaulted).ToList();

            foreach (Task task in finishedTasks)
            {
                int index = TaskList.IndexOf(task);
                TaskList.Remove(task);
                task.Dispose();

                CancellationTokenList[index].Dispose();
                CancellationTokenList.RemoveAt(index);

                EncodingListBox.Items.RemoveAt(index);
            }

            GC.Collect();
        }

        #endregion

        #region Private Static

        private static WindowState _lastState = WindowState.Normal;

        private static Screen GetScreen(Window window)
        {
            return Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

        #endregion

        #region Public Static

        /// <summary>
        /// Shows the Encoder window.
        /// </summary>
        public static void Start()
        {
            if (_encoder != null)
            {
                if (_encoder.WindowState == WindowState.Minimized)
                {
                    Restore();
                }
            }

            _encoder = new Encoder();

            var screen = GetScreen(_encoder);

            //Lower Right corner.
            _encoder.Left = screen.WorkingArea.Width - _encoder.Width;
            _encoder.Top = screen.WorkingArea.Height - _encoder.Height;

            _encoder.Show();
        }

        /// <summary>
        /// Add one list of frames to the encoding batch.
        /// </summary>
        /// <param name="listFrames"></param>
        /// <param name="fileName"></param>
        public static void AddItem(List<FrameInfo> listFrames, string fileName)
        {
            if (_encoder == null)
                Start();

            if (_encoder == null)
                throw new ApplicationException("Error while starting the Encoding window.");

            var type = fileName.EndsWith(".gif") ? Export.Gif : Export.Avi;

            _encoder.InternalAddItem(listFrames, fileName, type);
        }

        /// <summary>
        /// Minimizes the Encoder window.
        /// </summary>
        public static void Minimize()
        {
            if (_encoder == null)
                return;

            _lastState = _encoder.WindowState;

            _encoder.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Minimizes the Encoder window.
        /// </summary>
        public static void Restore()
        {
            if (_encoder == null)
                return;

            _encoder.WindowState = _lastState;
        }

        /// <summary>
        /// Updates a specific item of the list.
        /// </summary>
        /// <param name="id">The unique ID of the item.</param>
        /// <param name="currentFrame">The current frame being processed.</param>
        /// <param name="status">Status description.</param>
        public static void Update(int id, int currentFrame, string status)
        {
            if (_encoder == null)
                return;

            _encoder.InternalUpdate(id, currentFrame, status);
        }

        /// <summary>
        /// Updates a specific item of the list.
        /// </summary>
        /// <param name="id">The unique ID of the item.</param>
        /// <param name="currentFrame">The current frame being processed.</param>
        public static void Update(int id, int currentFrame)
        {
            if (_encoder == null)
                return;

            _encoder.InternalUpdate(id, currentFrame);
        }

        /// <summary>
        /// Sets the Status of the encoding of a current item.
        /// </summary>
        /// <param name="status">The current status.</param>
        /// <param name="id">The unique ID of the item.</param>
        /// <param name="fileName">The name of the output file.</param>
        public static void SetStatus(Status status, int id, string fileName = null)
        {
            if (_encoder == null)
                return;

            _encoder.InternalSetStatus(status, id, fileName);
        }

        #endregion
    }
}
