using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFileUpload
{

    public class BlazorFileUploadBase : ComponentBase, IDisposable
    {

        [Parameter] public Dictionary<string, string> FileUploadHeaders { get; set; }
        [Parameter] public string ApiUrl { get; set; }

        public event EventHandler<Dictionary<string, IBrowserFile>> OnUploadImagesEvent;
        public Dictionary<string, IBrowserFile> Base64ImageUrls = new Dictionary<string, IBrowserFile>();

        protected override Task OnInitializedAsync()
        {
            this.OnUploadImagesEvent += OnUploadImagesExecute;
            return base.OnInitializedAsync();
        }

        public async Task OnBlazorFileUploadChange(InputFileChangeEventArgs e)
        {
            var maxFiles = 3;
            var imageFormat = "image/png";

            foreach (var iBrowserFile in e.GetMultipleFiles(maxFiles))
            {
                var previewImage = await iBrowserFile.RequestImageFileAsync(imageFormat, 100, 100);
                var bytes = new byte[previewImage.Size];
                await previewImage.OpenReadStream().ReadAsync(bytes);
                var imageDataUrl = $"data:{imageFormat};base64,{Convert.ToBase64String(bytes)}";
                this.Base64ImageUrls.Add(imageDataUrl, iBrowserFile);
            }
        }

        private void OnUploadImagesExecute(object sender, Dictionary<string, IBrowserFile> e)
        {
            Console.WriteLine("on upload images execute..");
        }

        public async void UploadImages_Clicked()
        {
            this.OnUploadImagesEvent.Invoke(this, this.Base64ImageUrls);
            await this.UploadFiles(this.Base64ImageUrls);

            this.Base64ImageUrls.Clear();
            this.StateHasChanged();
        }

        public void RemoveThumbnail_Clicked(string key)
        {
            this.Base64ImageUrls.Remove(key);
            this.StateHasChanged();
        }

        private async Task UploadFiles(Dictionary<string, IBrowserFile> files)
        {
            try
            {
                string Output = string.Empty;
                MultipartFormDataContent content = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    var readstream = file.Value.OpenReadStream();                    
                    var newline = Environment.NewLine;
                    var bufferSize = 4096;
                    var buffer = new byte[bufferSize];
                    int read;

                    MemoryStream stream = new MemoryStream(100);
                    while ((read = await readstream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await stream.WriteAsync(buffer, 0, read);
                        Output += $"Read {read} bytes. {readstream.Position} / {readstream.Length}{newline}";
                        this.StateHasChanged();
                    }

                    if (stream.Length == stream.Position)
                    {
                        stream.Position = 0;
                    }

                    content.Add(new StreamContent(stream), "file", file.Value.Name);
                }
                using (var httpClient = new HttpClient())
                {
                    foreach(var item in this.FileUploadHeaders)
                    {
                        content.Headers.Add(item.Key, item.Value);
                    }
                    
                    var result = httpClient.PostAsync(this.ApiUrl, content).Result;
                    var remotePath = await result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            this.OnUploadImagesEvent -= OnUploadImagesExecute;
        }

    }
}
