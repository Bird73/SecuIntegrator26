# 實作計畫：台灣證券交易所爬蟲系統

## 目標描述
開發一套基於 .NET 10 (Backend) 與 React (Frontend) 的爬蟲系統，目標針對台灣證券交易所 (TWSE) 與公開資訊觀測站 (MOPS) 進行資料抓取。系統需具備高彈性的排程設定、完善的 Log 記錄、檔案中繼儲存機制、以及防止被阻擋的策略。

## 使用者審查項目 (User Review Required)
> [!IMPORTANT]
> 請確認以下針對「是否有遺漏」的補充建議：

1.  **監控與告警機制 (Monitoring & Alerting)**:
    - [x] 加入 **Line Notify** 主動告警，當爬蟲連續失敗或系統崩潰時即時通知。
2.  **瀏覽器模擬 (Browser Automation)**:
    - [x] 引入 **Playwright** 作為備援，處理 Web API 無法抓取的動態頁面。
3.  **股票代碼來源管理**:
    - [x] 加入「股票清單同步」排程，定期從證交所更新 上市/上櫃/興櫃 清單。
4.  **代理 IP 池 (Proxy Rotation)**:
    - [ ] ~~不需要 Proxy，證交所會封鎖特定 IP。~~ (已移除由原生 IP 直接連線)
5.  **容器化與部署 (Docker & Deployment)**:
    - [x] 建議保留 Dockerfile。**理由**：GitOps 核心精神在於環境一致性。即使目前在 Windows 執行，容器化能確保 Build/Test/Run 環境隔離，且未來若需遷移至 Linux (節省 OS 授權費) 或雲端環境會非常容易。
6.  **身份驗證 (Authentication)**:
    - [ ] ~~Phase 1 不實作驗證機制。~~ (留待 Phase 2)

## 擬定變更 (Proposed Changes)

### Documentation
#### [NEW] [Requirements_Specification.md](file:///C:/Users/birdc/.gemini/antigravity/brain/88d5fcd4-2d34-4e92-8915-5f9a43a01331/Requirements_Specification.md)
- 撰寫詳細需求規格書，定義資料流、系統邊界。

### Backend (.NET 10)
#### [NEW] Solution Structure
- `SecuIntegrator26.sln`
- `SecuIntegrator26.API` (Web API, Quartz Hosting)
- `SecuIntegrator26.Core` (Business Objects, Interfaces)
- `SecuIntegrator26.Infrastructure` (EF Core, Repositories, Parsers)
- `SecuIntegrator26.Shared` (DTOs, Utils)
- `SecuIntegrator26.Tests` (xUnit)

### Frontend (React)
#### [NEW] Project Structure
- 使用 Vite + TypeScript 初始化。
- 設定路由、API Client、狀態管理 (React Query/Zustand)。
#### [NEW] Feature: Holiday Manager
- 實作行事曆 UI Component (Grid Layout, 12 Months).
- 整合農曆轉換套件 (如 `lunar-javascript`).
- 實作休市日設定與 JSON 匯入/匯出功能。

## 驗證計畫 (Verification Plan)
### Automated Tests
- 後端：`dotnet test` 執行 xUnit 單元測試與整合測試。
- 前端：基本 Component 渲染測試。
- 爬蟲邏輯：撰寫針對靜態 HTML 樣本的解析測試，確保 Regex/HtmlAgilityPack 邏輯正確。
- Playwright 測試：針對範例動態頁面進行渲染測試。

### Manual Verification
- 啟動排程，觀察 Log 是否依設定時間產出。
- 驗證 Line Notify 是否收到告警 (模擬失敗情境)。
- 檢視 SQLite 資料庫是否正確寫入資料。
- 測試斷網或 HTTP Error 時的 Retry 機制。

