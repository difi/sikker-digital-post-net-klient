﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Difi.SikkerDigitalPost.Klient.Testklient.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("04036125433")]
        public string MottakerPersonnummer {
            get {
                return ((string)(this["MottakerPersonnummer"]));
            }
            set {
                this["MottakerPersonnummer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ove.jonsen#6K5A")]
        public string MottakerDigipostadresse {
            get {
                return ((string)(this["MottakerDigipostadresse"]));
            }
            set {
                this["MottakerDigipostadresse"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B43CAAA0FBEE6C8DA85B47D1E5B7BCAB42AB9ADD")]
        public string MottakerSertifikatThumbprint {
            get {
                return ((string)(this["MottakerSertifikatThumbprint"]));
            }
            set {
                this["MottakerSertifikatThumbprint"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("984661185")]
        public string OrgnummerPosten {
            get {
                return ((string)(this["OrgnummerPosten"]));
            }
            set {
                this["OrgnummerPosten"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8702F5E55217EC88CF2CCBADAC290BB4312594AC")]
        public string DatabehandlerSertifikatThumbprint {
            get {
                return ((string)(this["DatabehandlerSertifikatThumbprint"]));
            }
            set {
                this["DatabehandlerSertifikatThumbprint"] = value;
            }
        }
    }
}