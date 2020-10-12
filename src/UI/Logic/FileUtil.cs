using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace UI.Logic
{
    internal static class DownloadUtil
    {
        public async static Task StartClientDownload(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }
    }
}
