﻿// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.17020
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Resources.Strings {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("Resources.Strings.Strings", typeof(Strings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string unknown_location_dialog_title {
            get {
                return ResourceManager.GetString("unknown_location_dialog_title", resourceCulture);
            }
        }
        
        public static string about_view_title {
            get {
                return ResourceManager.GetString("about_view_title", resourceCulture);
            }
        }
        
        public static string unknown_location_dialog_text {
            get {
                return ResourceManager.GetString("unknown_location_dialog_text", resourceCulture);
            }
        }
        
        public static string no {
            get {
                return ResourceManager.GetString("no", resourceCulture);
            }
        }
        
        public static string yes {
            get {
                return ResourceManager.GetString("yes", resourceCulture);
            }
        }
        
        public static string no_thanks {
            get {
                return ResourceManager.GetString("no_thanks", resourceCulture);
            }
        }
    }
}
