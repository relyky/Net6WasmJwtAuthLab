export function showAlert(msg) {
  alert(msg);
}

export function showPrompt(question) {
  return prompt(question);
}

export function setElementTextById(id, text) {
  document.getElementById(id).innerText = text;
}

export function setElementText(element, text) {
  element.innerText = text;
}

export function focusElement(element) {
  element.focus();
}

export function invokeClick(element) {
  element.click();
}

export function redirect(url, target) {
  if (target === '_blank')
    window.open(url, "");
  else
    window.location.assign(url);
}

export function scrollToOffset(element, offset /* int */) {
  //ref→https://newbedev.com/javascript-scrollintoview-smooth-scroll-and-offset
  const bodyRect = document.body.getBoundingClientRect().top;
  const elementRect = element.getBoundingClientRect().top;
  const elementPosition = elementRect - bodyRect;
  const offsetPosition = elementPosition - offset;
  window.scrollTo({
    top: offsetPosition,
    behavior: "smooth"
  });
}

/// 預計用於 MudList 捲動到 MudListItem
export function scrollToChild(containerSelector, elementSelector) {
  //console.log('containerScrollToChild', { containerSelector, zeroElementSelector, elementSelector })

  const container = document.querySelector(containerSelector)
  const zero = container.firstElementChild;
  const element = container.querySelector(elementSelector);

  //console.log('containerScrollToChild el', { container, zero, element})
  const cntrHeight = container.getBoundingClientRect().height;

  const zeroRect = zero.getBoundingClientRect();
  const elementRect = element.getBoundingClientRect();

  const elementPosition = elementRect.top - zeroRect.top;
  const offsetPosition = elementPosition - cntrHeight / 2 + elementRect.height / 2; // center

  //console.log('containerScrollToChild pos', { offsetPosition, zeroRect, elementRect, elementPosition });

  container.scrollTo({
    top: offsetPosition,
    behavior: 'smooth'
  });
}

export function scrollIntoView(baseElement, elementSelector /* string */) {
  //console.log('scrollIntoView', { baseElement, elementSelector });
  if (!(baseElement instanceof HTMLElement))
    console.error('baseElement must be instanceof HTMLElement.');

  baseElement
    .querySelector(elementSelector)
    .scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });
}

/// 伺服器下載檔案
//function downloadFileAsync(filename, bytesBase64) {
//  var link = document.createElement('a');
//  link.download = filename;
//  link.href = "data:application/octet-stream;base64," + bytesBase64;
//  document.body.appendChild(link);
//  link.click();
//  document.body.removeChild(link);
//}

/// 伺服器下載檔案 by stream
// ref→[ASP.NET Core Blazor file downloads](https://docs.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-6.0)
export function triggerFileDownload(fileName, url) {
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
}

/// 伺服器下載檔案 by URL
// ref→[ASP.NET Core Blazor file downloads](https://docs.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-6.0)
export async function downloadFileFromStream(fileName, contentStreamReference) {
  const arrayBuffer = await contentStreamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
  URL.revokeObjectURL(url);
}

/// 複製文字到剪貼簿
export async function writeClipboard(newClip) {
  try {
    await navigator.clipboard.writeText(newClip);
    return true;
  }
  catch (e) {
    return false;
  }
}

///§ 機制：註冊或反註冊鎖定過期螢幕
///  需搭配：main.js 
export async function setDisableBlockScreenWhenExpired(flag /* bool */) {
  if (flag) {
    globalDisableBlockScreenWhenExpired = true;
  }
  else {
    globalDisableBlockScreenWhenExpired = undefined;
  }
}
