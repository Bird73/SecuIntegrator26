# 需求規格書 (Software Requirements Specification) - 台灣證券交易所爬蟲系統

## 1. 簡介
本文件定義「台灣證券交易所爬蟲系統」之軟體需求。系統旨在自動化抓取台股相關交易資訊，支援彈性排程、自動重試、檔案緩存與告警通知。

## 2. 系統架構概念
系統採前後端分離架構：
- **Backend**: .NET 8 Web API + Quartz.NET (排程) + SQLite (DB)
- **Frontend**: React (排程管理與監控 UI)
- **Data Source**: 台灣證券交易所 (TWSE)、公開資訊觀測站 (MOPS)

## 3. 功能需求 (Functional Requirements)

### 3.1 爬蟲核心 (CSecuIntegrator26)
- **FR-01 資料來源支援**: 系統需支援抓取 TWSE 與 MOPS 網站資料。
- **FR-02 靜態解析**: 針對一般頁面，使用 HTTP Client 下載並以 Regex 或 HtmlAgilityPack 解析。
- **FR-03 動態解析 (Playwright)**: 針對 JavaScript 動態渲染頁面，系統需自動切換或指定使用 Playwright 進行瀏覽器模擬抓取。
- **FR-04 檔案緩存 (File Buffering)**: 抓回之原始資料 (HTML/JSON/CSV) 需先儲存於本機檔案系統 (Raw Data)，再進行解析入庫。
- **FR-04-1 保留策略**: 提供設定「原始檔案保留天數」，逾期自動清理。
- **FR-05 頻率限制 (Rate Limiting)**: 支援設定連續抓取間隔 (最小單位 0.1秒)。
- **FR-06 阻擋處理 (Anti-Blocking)**: 偵測被阻擋 (403/Captchas) 時，依設定進行 Backoff (指數退避) 或暫停特定時間後重試。

### 3.2 排程管理 (Scheduler)
- **FR-07 排程頻率**: 支援 年、季、月、周、日 之抓取頻率設定。
- **FR-08 啟動時間**: 可設定每日啟動時間 (HH:mm)。
- **FR-09 啟始日期**: 可設定補抓資料的起始日期。
- **FR-10 休假日排除**: 系統需讀取休假日設定檔 (如行政院行事曆)，於休市日自動跳過排程。
- **FR-11 股票清單同步**: 系統需內建排程，定期更新上市、上櫃、興櫃之股票代碼清單。

### 3.3 異常處理與告警 (Error Handling & Alerting)
- **FR-12 自動重試**: 針對網路錯誤或 5xx 錯誤，依設定次數與間隔自動重試。
- **FR-13 Line Notify 告警**: 當任務連續失敗達閾值、或系統發生未處理例外時，發送 Line Notify 通知。
- **FR-13-1 Log 保留策略**: Log 記錄需支援設定「保留天數」，逾期自動清理。

- **FR-13-1 Log 保留策略**: Log 記錄需支援設定「保留天數」，逾期自動清理。

### 3.4 休市日管理 (Holiday Manager)
- **FR-17 設定檔管理**: 休市日資料需儲存於 JSON 檔案 (`holidays.json`)，並支援透過 UI 進行 **匯出** (下載) 與 **匯入** (上傳覆蓋)。
- **FR-18 行事曆 UI**:
    - **年視圖**: 顯示特定年份 (預設當年)，提供「上一年」、「下一年」切換或下拉選擇特定年份。
    - **月份區塊**: 畫面呈現 12 個月份區塊 (Grid Layout)。
    - **日期資訊**: 每個日期需顯示國曆日期、**農曆日期**、以及**特殊節慶**標示。
- **FR-19 休市設定**:
    - 點選特定日期可切換「休市/正常」狀態。
    - 設定為休市日時，需輸入「休市原因」(如：颱風假、農曆春節)。
    - 休市日需以特定顏色 (如紅色背景) 區別顯示。

### 3.5 管理介面 (System UI)
- **FR-14 排程設定**: 提供圖形化介面設定上述排程參數。
- **FR-15 手動啟動**: 提供按鈕手動觸發特定爬蟲任務。
- **FR-16 系統狀態**: 顯示目前排程狀態 (執行中/閒置/錯誤) 與最近執行的 Log。

## 4. 非功能需求 (Non-Functional Requirements)

### 4.1 效能 (Performance)
- **NFR-01**: 單一節點需能支撐每日收盤後之全市場歷史資料補齊 (需評估流量控制)。

### 4.2 可靠性 (Reliability)
- **NFR-02**: 資料庫寫入需具備 Transaction 保護，確保資料一致性。
- **NFR-03**: 原始檔案 (Raw Data) 與 Log 需依設定之天數自動清理，確保磁碟空間不被耗盡。

### 4.3 可維護性 (Maintainability)
- **NFR-04**: 後端採分層架構 (Controller, Service, Repository)。
- **NFR-05**: 完整 Log 記錄 (Serilog + JSON)，包含 Request/Response 資訊以便除錯。
- **NFR-06**: 採用 GitOps 流程，配置 Dockerfile。

## 5. 資料需求 (Data Requirements)
- **資料庫**: SQLite (Phase 1), 保留未來遷移至 PostgreSQL 之介面彈性 (EF Core)。
- **Schema**:
    - `StockSymbols`: 股票代碼表 (上市/上櫃/興櫃)
    - `DailyClosingQuotes`: 每日收盤行情 (上市/上櫃合併儲存)
    - `MonthlyRevenue`: 月營業收入資訊
    - `CrawlLogs`: 系統執行紀錄
    - `SystemConfigs`: 系統設定 (排程、頻率、保留天數)

## 6. 介面設計 (Interface Design)
- **API**: RESTful API，提供 Swagger/OpenAPI 文件。
