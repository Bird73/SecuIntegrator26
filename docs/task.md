# 台灣證券交易所爬蟲系統開發任務清單

- [ ] **Phase 1: 需求分析與架構設計 (Requirements & Architecture)**
    - [x] 產出需求規格書 (SRS) <!-- id: 0 -->
    - [x] 產出架構設計文件 (SDD) <!-- id: 22 -->
    - [x] 確認技術堆疊與第三方套件版本 (.NET 10) <!-- id: 1 -->
    - [ ] 定義資料庫 Schema (SQLite) <!-- id: 2 -->
    - [ ] 設計 API 介面 (OpenAPI/Swagger) <!-- id: 3 -->

- [ ] **Phase 2: 專案基礎建設 (Project Infrastructure)**
    - [x] 建立 Git Repository 與方案結構 (.sln, React) <!-- id: 4 -->
    - [x] 設定 Docker 環境與 GitOps 流程 (CI/CD 腳本) <!-- id: 5 -->
    - [x] 設定 Serilog 與 Log 基礎建設 <!-- id: 6 -->
    - [ ] 設定 Line Notify 告警服務 <!-- id: 23 -->

- [ ] **Phase 3: 後端核心開發 (Backend Core)**
    - [x] 實作 Entity Framework Core Context & Repository Pattern <!-- id: 7 -->
    - [x] 實作 Business Logic Layer & DTOs <!-- id: 8 -->
    - [x] 實作 Service Layer (Utility, Helpers) <!-- id: 9 -->
    - [x] 單元測試 (xUnit) Setup <!-- id: 10 -->

- [ ] **Phase 4: 排程與爬蟲模組 (Scheduler & SecuIntegrator26)**
    - [x] 整合 Quartz.NET 排程系統 <!-- id: 11 -->
    - [x] 實作股票清單同步排程 (上市/上櫃/興櫃) <!-- id: 24 -->
    - [x] 實作基本爬蟲 HTTP Client (Rate Limiting, Retry) <!-- id: 12 -->
    - [x] 建立獨立 Playwright Worker (.NET 8) <!-- id: 25 -->
    - [x] 實作檔案下載與緩存機制 (File Buffering) <!-- id: 13 -->
    - [x] 實作解析器 (Regex/HtmlAgilityPack) <!-- id: 14 -->
    - [x] 實作休假日與排程設定邏輯 <!-- id: 15 -->

- [ ] **Phase 5: 前端開發 (Frontend - React)**
    - [x] 初始化 React 專案 (Vite + TypeScript) <!-- id: 16 -->
    - [x] 實作排程設定介面 <!-- id: 17 -->
    - [x] 實作休市日管理 (JSON I/O, 農曆顯示, 設定 UI) <!-- id: 26 -->
    - [ ] 實作 Log 檢視與狀態監控 Dashboard <!-- id: 18 -->

- [ ] **Phase 6: 測試與驗證 (Testing & Verification)**
    - [ ] 整合測試 (Integration Tests) <!-- id: 19 -->
    - [ ] 系統壓力測試與頻率限制驗證 <!-- id: 20 -->
    - [ ] 產出操作手冊與維運文件 <!-- id: 21 -->
