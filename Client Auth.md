# Client Side Auth. 紀錄
在 WASM 的授權管理，除了後端 Web API 必需管制。前端的 @page 用戶是否有權開啟使用也是管制的項目之一。  
在實作上 Blazor WASM App 的 Client 授權管制的指令與 Blazor Server App 是一樣的，都是以 AuthenticationStateProvider 為中心。

參考文章：[Blazor WASM App 驗證與授權](https://rely-ky.gitbook.io/gitbook/blazor-wasm-app-yan-zheng-yu-shou-quan)

# 登入認證概述
1. 為客製化方案，全程使用 Bearer token 為認證基礎。
2. 後端 Api 授權管制與 Web API 是一致的。
3. 前端 @page 的管制與 Blazor Server App 一樣也是以 AuthenticationStateProvider 為中心。所以也是用 Task<AuthenticationState> 來取認證狀態。

> ※注意：為練習用，只為確認有登入認證與授權效果，正式版應用應再強化或重構使更成熟可靠。

# 關鍵原碼紀錄
