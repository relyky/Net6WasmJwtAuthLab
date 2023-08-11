using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SmallEco.Client.Services;

/// <summary>
/// ※用於與前端 JavaScript 互動。(應用延伸)
/// 為『~/js/interop-module.js』(JavaScript interaction operator)的操作介面
/// </summary>
internal class JSInterOpService : IAsyncDisposable
{
  //# Injection Member
  readonly IJSRuntime jsr;

  //# resource
  readonly Lazy<Task<IJSObjectReference>> moduleTask;

  public async ValueTask DisposeAsync()
  {
    if (moduleTask.IsValueCreated)
    {
      var module = await moduleTask.Value;
      await module.DisposeAsync();
    }
  }

  public JSInterOpService(IJSRuntime jsRuntime)
  {
    jsr = jsRuntime;
    moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
      "import", "./js/interop-module.js").AsTask());
  }

  public async Task ShowAlertAsync(string message)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("showAlert", message);
  }

  public async Task<string> ShowPromptAsync(string question)
  {
    var jsModule = await moduleTask.Value;
    return await jsModule.InvokeAsync<String>("showPrompt", question);
  }

  public async Task SetElementTextByIdAsync(string id, string text)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("setElementTextById", id, text);
    /// 未測試
  }

  public async Task SetElementTextAsync(ElementReference element, string text)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("setElementText", text);
    /// 未測試
  }

  public async Task FocusElementAsync(ElementReference element)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("focusElement");
    /// 未測試
  }

  public async Task InvokeClickAsync(ElementReference element)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("invokeClick");
    /// 未測試
  }

  public async Task RedirectAsync(Uri uri, string target = "")
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("redirect", uri.ToString(), target);
  }

  public async Task ScrollTo(ElementReference element, int offset = 0)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("scrollToOffset", element, offset);
  }

  public async Task ScrollIntoView(ElementReference baseElement, string elementSelector)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("scrollIntoView", baseElement, elementSelector);
  }

  /// <summary>
  /// 預計用於 MudList 捲動到 MudListItem
  /// </summary>
  public async Task ScrollToChild(string containerSelector, string elementSelector)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("scrollToChild", containerSelector, elementSelector);
  }

  /// <summary>
  /// 伺服器下載檔案 by URL.
  /// </summary>
  public async Task DownloadFileFromUrlAsync(string fileName, Uri fileURL)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, fileURL.AbsoluteUri);
    // 未測試
  }

  /// <summary>
  /// 伺服器下載檔案 by binary blob.
  /// </summary>
  public async Task DownloadFileAsync(string fileName, byte[] fileBlob, string contentType = System.Net.Mime.MediaTypeNames.Application.Octet)
  {
    var jsModule = await moduleTask.Value;
    string base64Url = $"data:{contentType};base64," + Convert.ToBase64String(fileBlob);
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, base64Url);
  }

  /// <summary>
  /// 伺服器下載檔案 by MemoryStream.
  /// </summary>
  public async Task DownloadFileAsync(string fileName, System.IO.MemoryStream ms, string contentType = System.Net.Mime.MediaTypeNames.Application.Octet)
  {
    var jsModule = await moduleTask.Value;
    string base64Url = $"data:{contentType};base64," + Convert.ToBase64String(ms.ToArray());
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, base64Url);
  }

  /// <summary>
  /// 伺服器下載檔案 by stream.
  /// </summary>
  public async Task DownloadStreamAsync(string fileName, System.IO.Stream fileStream)
  {
    var jsModule = await moduleTask.Value;

    //※ 從頭開始讀檔，因為前面步驟可能已讀到檔尾。
    if (fileStream.CanSeek) fileStream.Seek(0, System.IO.SeekOrigin.Begin);

    using var streamRef = new DotNetStreamReference(fileStream);
    await jsModule.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
  }

  /// <summary>
  /// /// 複製文字到剪貼簿
  /// </summary>
  public async Task<bool> WriteClipboardAsync(string newClip)
  {
    var jsModule = await moduleTask.Value;
    bool ret = await jsModule.InvokeAsync<bool>("writeClipboard", newClip);
    return ret;
  }

  /// <summary>
  /// 機制：註冊或反註冊鎖定過期螢幕
  /// </summary>
  public async Task SetDisableBlockScreenWhenExpiredAsync(bool flag)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("setDisableBlockScreenWhenExpired", flag);
  }

  #region 直接叫用(已安裝好的JS函式庫) JavaScript 指令。  

  /// <summary>
  /// 直接送印 PDF 檔
  /// ※ 請先安裝好 print-js 函式庫。
  /// </summary>
  public async Task PrintPdfAsync(byte[] pdfBlob)
  {
    //# binary 轉換成 base64
    string base64 = Convert.ToBase64String(pdfBlob);
    await jsr.InvokeVoidAsync("printJS", new
    {
      printable = base64,
      type = "pdf",
      base64 = true
    });
  }

  #endregion


}
