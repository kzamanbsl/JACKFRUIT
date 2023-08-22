using KGERP.Data.Models;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace KGERP.Service.ServiceModel.FTP_Models
{
    public class FileUrl
    {
        public long DocId { get; set; }
        public string FilePath { get; set; }
        public bool IsFileExists { get; set; } = true;
        public string Error { get; set; }
    }
    public class FileItem:FileArchive
    {
        public HttpPostedFileBase file { get; set; }
        public bool IsUploaded { get; set; } = false;
        public string ErrorMessage { get; set; }
    }
    public class FileViewModel
    {
        public List<HttpPostedFileBase> file { get; set; }
        public string fileTitle { get; set; }

    }

    public class FilePreviewModel
    {
        public MemoryStream Data { get; set; }
        public string FileName { get; set; }
        public string mimeType { get; set; }
        
        public string ErrorMessage { get; set; }

    }
}
