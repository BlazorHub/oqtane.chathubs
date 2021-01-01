using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFileUpload
{
    public class BlazorFileUploadModel
    {

        public string Base64ImageUrl { get; set; }

        public IBrowserFile BrowserFile { get; set; }

    }
}
