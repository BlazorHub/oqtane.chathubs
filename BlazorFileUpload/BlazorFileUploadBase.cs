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

        public event EventHandler<Dictionary<Guid, BlazorFileUploadModel>> OnUploadImagesEvent;
        public Dictionary<Guid, BlazorFileUploadModel> FileUploadModels = new Dictionary<Guid, BlazorFileUploadModel>();

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

                var model = new BlazorFileUploadModel()
                {
                    Base64ImageUrl = imageDataUrl,
                    BrowserFile = iBrowserFile,
                };

                this.FileUploadModels.Add(Guid.NewGuid(), model);
            }
        }

        private void OnUploadImagesExecute(object sender, Dictionary<Guid, BlazorFileUploadModel> e)
        {
            Console.WriteLine("on upload images execute..");
        }

        public async void UploadImages_Clicked()
        {
            this.OnUploadImagesEvent.Invoke(this, this.FileUploadModels);
            await this.UploadFiles(this.FileUploadModels);

            this.FileUploadModels.Clear();
            this.StateHasChanged();
        }

        public void RemoveThumbnail_Clicked(Guid key)
        {
            this.FileUploadModels.Remove(key);
            this.StateHasChanged();
        }

        private async Task UploadFiles(Dictionary<Guid, BlazorFileUploadModel> files)
        {
            try
            {
                string Output = string.Empty;
                MultipartFormDataContent content = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    var readstream = file.Value.BrowserFile.OpenReadStream();                    
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

                    content.Add(new StreamContent(stream), "file", file.Value.BrowserFile.Name);
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
