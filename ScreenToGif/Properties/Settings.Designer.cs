﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace ScreenToGif.Properties {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool FullScreen {
            get {
                return ((bool)(this["FullScreen"]));
            }
            set {
                this["FullScreen"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool PreStart {
            get {
                return ((bool)(this["PreStart"]));
            }
            set {
                this["PreStart"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool ShowCursor {
            get {
                return ((bool)(this["ShowCursor"]));
            }
            set {
                this["ShowCursor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool Snapshot {
            get {
                return ((bool)(this["Snapshot"]));
            }
            set {
                this["Snapshot"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0")]
        public int StartUp {
            get {
                return ((int)(this["StartUp"]));
            }
            set {
                this["StartUp"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool MouseClicks {
            get {
                return ((bool)(this["MouseClicks"]));
            }
            set {
                this["MouseClicks"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool UseDefaultOutput {
            get {
                return ((bool)(this["UseDefaultOutput"]));
            }
            set {
                this["UseDefaultOutput"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("")]
        public string DefaultOutput {
            get {
                return ((string)(this["DefaultOutput"]));
            }
            set {
                this["DefaultOutput"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("auto")]
        public string Language {
            get {
                return ((string)(this["Language"]));
            }
            set {
                this["Language"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("15")]
        public int LastFps {
            get {
                return ((int)(this["LastFps"]));
            }
            set {
                this["LastFps"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("518")]
        public int Width {
            get {
                return ((int)(this["Width"]));
            }
            set {
                this["Width"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("269")]
        public int Height {
            get {
                return ((int)(this["Height"]));
            }
            set {
                this["Height"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool CustomEncoding {
            get {
                return ((bool)(this["CustomEncoding"]));
            }
            set {
                this["CustomEncoding"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("#FF32CD32")]
        public Color TransparentColor {
            get {
                return ((Color)(this["TransparentColor"]));
            }
            set {
                this["TransparentColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool PaintTransparent {
            get {
                return ((bool)(this["PaintTransparent"]));
            }
            set {
                this["PaintTransparent"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool Looped {
            get {
                return ((bool)(this["Looped"]));
            }
            set {
                this["Looped"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("2")]
        public int RepeatCount {
            get {
                return ((int)(this["RepeatCount"]));
            }
            set {
                this["RepeatCount"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool RepeatForever {
            get {
                return ((bool)(this["RepeatForever"]));
            }
            set {
                this["RepeatForever"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("10")]
        public int Quality {
            get {
                return ((int)(this["Quality"]));
            }
            set {
                this["Quality"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("F7")]
        public Keys StartPauseKey {
            get {
                return ((Keys)(this["StartPauseKey"]));
            }
            set {
                this["StartPauseKey"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("F8")]
        public Keys StopKey {
            get {
                return ((Keys)(this["StopKey"]));
            }
            set {
                this["StopKey"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool DetectUnchanged {
            get {
                return ((bool)(this["DetectUnchanged"]));
            }
            set {
                this["DetectUnchanged"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("#FFF5F5F5")]
        public Color GridColor1 {
            get {
                return ((Color)(this["GridColor1"]));
            }
            set {
                this["GridColor1"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("#FFF0F0F0")]
        public Color GridColor2 {
            get {
                return ((Color)(this["GridColor2"]));
            }
            set {
                this["GridColor2"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0,0,20,20")]
        public Rect GridSize {
            get {
                return ((Rect)(this["GridSize"]));
            }
            set {
                this["GridSize"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool FixedFrameRate {
            get {
                return ((bool)(this["FixedFrameRate"]));
            }
            set {
                this["FixedFrameRate"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("1000")]
        public int SnapshotDefaultDelay {
            get {
                return ((int)(this["SnapshotDefaultDelay"]));
            }
            set {
                this["SnapshotDefaultDelay"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("15")]
        public int LastFpsImport {
            get {
                return ((int)(this["LastFpsImport"]));
            }
            set {
                this["LastFpsImport"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("#FFFFFFFF")]
        public Color InsertFillColor {
            get {
                return ((Color)(this["InsertFillColor"]));
            }
            set {
                this["InsertFillColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("#FFFFFFFF")]
        public Color CreateLastSelectedColor {
            get {
                return ((Color)(this["CreateLastSelectedColor"]));
            }
            set {
                this["CreateLastSelectedColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("500")]
        public int CreateWidth {
            get {
                return ((int)(this["CreateWidth"]));
            }
            set {
                this["CreateWidth"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("200")]
        public int CreateHeight {
            get {
                return ((int)(this["CreateHeight"]));
            }
            set {
                this["CreateHeight"] = value;
            }
        }
    }
}
