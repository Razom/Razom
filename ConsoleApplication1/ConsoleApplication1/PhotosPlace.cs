//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    
    public partial class PhotosPlace
    {
        public int FotoID { get; set; }
        public Nullable<int> PlaceID { get; set; }
        public byte[] FileFoto { get; set; }
        public string href { get; set; }
    
        public virtual Places Places { get; set; }
    }
}
